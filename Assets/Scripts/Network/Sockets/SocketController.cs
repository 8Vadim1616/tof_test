using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.User.Sockets;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using Thread = System.Threading.Thread;

namespace Assets.Scripts.Network.Sockets
{
	public class SocketController : AbstractSocketController
	{
		private string TAG = "[SocketController]";

		private const int CR = 13; // Carriage Return (CR)
		private const int WILL = 0xFB; // 251 - WILL (option code)
		private const int WONT = 0xFC; // 252 - WON'T (option code)
		private const int DO = 0xFD; // 253 - DO (option code)
		private const int DONT = 0xFE; // 254 - DON'T (option code)
		private const int IAC = 0xFF; // 255 - Interpret as Command (IAC)

		private const string PING_COMMAND = "ping\n";
		private const string PONG_ANSWER = "pong";

		public const string DISCONNECT_NO_CONNECTED = "!IsConnected";
		public const string DISCONNECT_PING = "On waitingForPong";
		public const string DISCONNECT_CAN_WRITE = "On stream.CanWrite";
		public const string DISCONNECT_SEND_SOCKET = "On send socket";
		public const string DISCONNECT_CHECK_CONNECTION = "CheckConnection";

		private TcpClient socketConnection;
		private Thread clientReceiveThread;
		private IDisposable onUpdate;

		private readonly List<(string name, string pool)> _channels = new List<(string, string)>();

		public SocketController(UserSocketSettings settings) : base(settings)
		{
			if (Game.User?.Sockets != null)
				pingInterval = Game.User.Sockets.PingInterval;
			if (pingInterval < 0)
				pingOn = false;
		}

		public override bool IsConnectedToChannel(string channelName, string channelPool)
		{
			foreach ((string name, string pool) in _channels)
				if (channelName == name && channelPool == pool)
					return true;

			return false;
		}

		private Promise connectingPromise = null;

		private bool isConnected;
		//private long lastCheckedConnectionStatusTime = 0;
		//private uint checkConnectionInterval = 10;

		private long lastPingTime = 0;
		private int pingInterval = 60;
		private bool pingOn = true;

		private bool waitingForPong;

		/*
		public override bool IsConnected
		{
			get
			{
				if (GameTime.Now <= lastCheckedConnectionStatusTime + checkConnectionInterval) return isConnected;
				
				lastCheckedConnectionStatusTime = GameTime.Now;
				if (socketConnection != null && socketConnection.Connected)
					isConnected = socketConnection.Client.Poll(-1, SelectMode.SelectWrite);

				return isConnected;
			}
		}
		*/

		internal override void CheckConnection()
		{
			if (!IsConnected)
				SafeReconnect(DISCONNECT_CHECK_CONNECTION, 0f);
		}

		private bool IsConnected
		{
			get
			{
				//https://stackoverflow.com/questions/6993295/how-to-determine-if-the-tcp-is-connected-or-not
				try
				{
					//if (GameTime.Now <= lastCheckedConnectionStatusTime + checkConnectionInterval) return isConnected;
					//lastCheckedConnectionStatusTime = GameTime.Now;

					if (socketConnection != null && socketConnection.Client != null && socketConnection.Client.Connected)
					{
						/* pear to the documentation on Poll:
						 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
						 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
						 * -or- true if data is available for reading; 
						 * -or- true if the connection has been closed, reset, or terminated; 
						 * otherwise, returns false
						 */

						// Detect if client disconnected
						if (socketConnection.Client.Poll(0, SelectMode.SelectRead))
						{
							var buff = new byte[1];
							isConnected = socketConnection.Client.Receive(buff, SocketFlags.Peek) != 0;
						}
						else
							isConnected = true;
					}
					else
						isConnected = false;
				}
				catch
				{
					isConnected = false;
				}

				return isConnected;
			}
		}

		private IPromise Connect()
		{
			if (connectingPromise != null && connectingPromise.CurState == PromiseState.Pending)
				return connectingPromise;

			if (IsConnected) return Promise.Resolved();

			GameLogger.debug($"{TAG} Start connect {Settings.Host}:{Settings.Port}");
			connectingPromise = new Promise();
			socketConnection = new TcpClient();
			IDisposable sub = null;
			try
			{
				var task = socketConnection.ConnectAsync(Settings.Host, Settings.Port);

				sub = Observable.EveryUpdate().Subscribe(_ =>
				{
					if (task.IsFaulted || task.IsCanceled)
					{
						onFail();
					}
					else if (task.IsCompleted)
					{
						//lastCheckedConnectionStatusTime = GameTime.Now;
						lastPingTime = GameTime.Now;
						isConnected = true;
						GameLogger.debug($"{TAG} Connected to {Settings.Host}:{Settings.Port}");
						connectingPromise.Resolve();
						connectingPromise = null;
						sub?.Dispose();
						sub = null;
						OnConnected();
					}
				});
			}
			catch (Exception e)
			{
				onFail();
			}

			void onFail()
			{
				GameLogger.debug($"{TAG} Fail connect to {Settings.Host}:{Settings.Port}");
				connectingPromise.Reject(null);
				connectingPromise = null;
				sub?.Dispose();
				sub = null;
				OnDisconnected(message: "OnFail");
			}


			return connectingPromise;
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			onUpdate?.Dispose();
			//onUpdate = Observable.EveryUpdate().Subscribe(_=> OnUpdate());
			onUpdate = Observable.Interval(TimeSpan.FromSeconds(.5d)).Subscribe(_ => OnUpdate());
		}

		private int state = 0;
		private int st = 0;
		private List<byte> receivedByteArray = new List<byte>();
		private int packageLength;

		private void OnUpdate()
		{
			if (!IsConnected)
			{
				SafeReconnect(DISCONNECT_NO_CONNECTED);
				return;
			}

			if (socketConnection.Available != 0)
			{
				lastPingTime = GameTime.Now;
				ReadData();
			}
			else if (pingOn)
			{
				bool pingIntervalPassed = GameTime.Now > lastPingTime + pingInterval;

				if (waitingForPong && pingIntervalPassed)
				{
					waitingForPong = false;
					SafeReconnect(DISCONNECT_PING, 0f);
				}
				else if (pingIntervalPassed)
					Ping();
			}
		}

		private void ReadData()
		{
			var stream = socketConnection.GetStream();

			while (stream.DataAvailable)
			{
				var b = stream.ReadByte();
				switch (state)
				{
					case 0:
						// If the current byte is the "Interpret as Command" code, set the state to 1.

						//if (b == IAC) {
						//	state = 1;
						// Else, if the byte is not a carriage return, display the character using the msg() method.
						//} else {
						if (b == '$' && st == 0)
						{
							//*
							st = 1;
						}
						else
						{
							if (st == 1 && b == 13) st = 2;
							else if (st == 2 && b == 10)
							{
								packageLength = int.Parse(Encoding.UTF8.GetString(receivedByteArray.ToArray()));
								if (packageLength == 0)
								{
									packageLength = -1;
									st = 0;
								}
								else st = 4;
								receivedByteArray.Clear();
							}
							else if (st == 1)
							{
								receivedByteArray.Add((byte) b);
							}
							else if (st == 4)
							{
								receivedByteArray.Add((byte) b);

								if (receivedByteArray.Count >= packageLength)
								{
									var dataArray = receivedByteArray.ToArray();
									var answer = Encoding.UTF8.GetString(dataArray);
									if (string.Equals(answer, PONG_ANSWER))
										waitingForPong = false;

									OnDataBytesReceived(dataArray);

									st = 0;
									packageLength = -1;
									receivedByteArray.Clear();
								}
							}
						}
						//}
						break;
					case 1:
						// If the current byte is the "DO" code, set the state to 2.
						if (b == DO)
						{
							state = 2;
						}
						else
						{
							state = 0;
						}
						break;
					// Blindly reject the option.
					case 2:
						/*
						 Write the "Interpret as Command" code, "WONT" code,
						 and current byte to the socket and send the contents
						 to the server by calling the flush() method.
						 */
						Send(IAC.ToString());
						Send(WONT.ToString());
						Send(b.ToString());
						state = 0;
						break;
				}
			}
		}

		private void SafeDisconectInner(string message)
		{
			onUpdate?.Dispose();
			onUpdate = null;
			OnDisconnected(message: message);
		}

		private void SafeReconnect(string message, float reconnectDelay = 5f)
		{
			if (socketConnection.Connected)
			{
				isConnected = false;
				socketConnection.Close();
			}
			SafeDisconectInner(message);
			Reconnect(reconnectDelay);
		}

		private void Reconnect(float delay = 5f)
		{
			if (delay > 0f)
			{
				Debug.Log($"{TAG} Trying reconnect to socket after {delay} sec");
				Utils.Utils.Wait(delay).Then(() => ConnectInner());
			}
			else
			{
				Debug.Log($"{TAG} Trying reconnect to socket immediate");
				ConnectInner();
			}

			void ConnectInner()
			{
				Connect().Then(() =>
				{
					foreach (var channel in _channels)
						Send($"{COMMAND_SUBSCRIBE} {channel.name}\r\n");
				});
			}
		}

		private void Ping()
		{
			Connect().Then
			(
				() =>
				{
					Send(PING_COMMAND).Then(() =>
					{
						lastPingTime = GameTime.Now;
						waitingForPong = true;
					});
				}
			);
		}

		protected override void Disconnect()
		{
			onUpdate?.Dispose();
			onUpdate = null;

			if (socketConnection.Connected)
			{
				foreach (var channel in _channels.ToList())
					Unsubscribe(channel.name, channel.pool);

				isConnected = false;
				_channels.Clear();
				socketConnection.Close();
				OnDisconnected("End Connection");
			}
		}

		public override void Subscribe(string channelName, string poolName)
		{
			//if (IsConnectedToChannel(channelName)) return;

			Connect()
				.Then(() => Utils.Utils.NextFrame())
				.Then(() =>
				{
					Send($"{COMMAND_SUBSCRIBE} {channelName}\r\n");
					_channels.AddOnce((channelName, pool: poolName));
				});
		}

		public override void Subscribe(List<string> channelNames, string pool)
		{
			foreach (string channelName in channelNames)
				Subscribe(channelName, pool);
		}

		public override void Unsubscribe(string channelName, string pool)
		{
			if (!IsConnectedToChannel(channelName, pool))
				return;

			if (IsConnected)
				Send($"{COMMAND_UNSUBSCRIBE} {channelName}\r\n")
					.Then(() =>
					{
						if (IsConnectedToChannel(channelName, pool))
							_channels.Remove((channelName, pool));
					});
		}

		public override void Unsubscribe(List<string> channelNames, string pool)
		{
			foreach (string channelName in channelNames)
				Unsubscribe(channelName, pool);
		}

		public override void OnApplicationFocus()
		{

		}

		public override void OnApplicationUnFocus()
		{

		}

		private IPromise Send(string msg)
		{
			if (!IsConnected)
				return Connect().Then(() => SendInner(msg));
			else return SendInner(msg);
		}

		private IPromise SendInner(string msg)
		{
			try
			{
				var stream = socketConnection.GetStream();
				Debug.Log($"{TAG} Trying send: {msg}");
				if (stream.CanWrite)
				{
					Debug.Log($"{TAG} >> {msg}");
					byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(msg);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
					return Promise.Resolved();
				}
				else
				{
					SafeReconnect(DISCONNECT_CAN_WRITE);
					return Send(msg);
				}
			}
			catch (SocketException socketException)
			{
				GameLogger.warning($"{TAG} Socket exception: " + socketException);
				SafeDisconectInner(DISCONNECT_SEND_SOCKET);
				return Send(msg);
			}
			catch (Exception ex)
			{
				GameLogger.error(ex);
				return Promise.Resolved();
			}
			/*catch (System.IO.IOException ioException)
            {
				GameLogger.warning("Rethrowed socket exception: " + ioException);
			}*/
		}

		/*
		public void ConnectToTcpServer ()
		{
			try
			{
				clientReceiveThread = new Thread(ListenForData);
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.Log("On client connect exception " + e);
			}
		}
		
		private void ListenForData()
		{
			try
			{ 			
				socketConnection = new TcpClient("develop2.playme8123.ru", 7051);
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading 				
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary.
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 						
							string msg = Encoding.UTF8.GetString(incommingData);
							Debug.Log(TAG + "<< " + msg);
						} 				
					} 			
				}         
			}         
			catch (SocketException socketException)
			{
				Debug.LogError(TAG + "Socket exception: " + socketException);
			}     
		}
*/


	}
}
