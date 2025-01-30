using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Network.Logs;
using NativeWebSocket;
using UnityEngine;

namespace Assets.Scripts.Network.Sockets.WebSockets
{
    public sealed class SocketPool : IDisposable
    {
		private enum State
		{
			Disconnected, Connected
		}

        private string TAG = "[WebSocketPool]";

        private string _poolName;

        private readonly string _webSocketUrl;
        private readonly List<string> _channels = new List<string>();
        private WebSocket _nextSocket;
        private WebSocket _currentSocket;
		private readonly List<int> _errorSockets = new List<int>();

        public event Action<string> OnMessage;
        public event Action OnConnect;
        public event Action<string> OnDisconnect;

		private State _curState = State.Disconnected;

		public SocketPool(string webSocketUrl, string poolName)
        {
            _webSocketUrl = webSocketUrl;
            _poolName = poolName;
        }

        public void Subscribe(string channelName)
        {
            if (_channels.Contains(channelName))
                return;

            if (_channels.Count == 0)
				GameLogger.debug($"{TAG} WebSocket pool {_poolName} opened");

			_channels.Add(channelName);
            UpdateSocket();
        }

        public void Subscribe(List<string> channelNames)
        {
            if (_channels.Count == 0 && channelNames.Count > 0)
				GameLogger.debug($"{TAG} WebSocket pool {_poolName} opened");

			bool allSocketAlreadySubscribed = true;
            foreach (string channelName in channelNames)
            {
                if (_channels.Contains(channelName))
                    continue;

                allSocketAlreadySubscribed = false;
                _channels.Add(channelName);
            }

            if (allSocketAlreadySubscribed)
                return;

            UpdateSocket();
        }

        public void Unsubscribe(List<string> channelNames)
        {
            bool allSocketAlreadyUnsubscribed = true;
            foreach (string channelName in channelNames)
            {
                if (!_channels.Contains(channelName))
                    continue;

                allSocketAlreadyUnsubscribed = false;
                _channels.Remove(channelName);
            }

            if (allSocketAlreadyUnsubscribed)
                return;

            UpdateSocket();
        }

        public void Unsubscribe(string channelName)
        {
            if (!_channels.Contains(channelName))
                return;

            _channels.Remove(channelName);
            UpdateSocket();
        }

        public bool ContainsChannel(string channelName) =>
            _channels.Contains(channelName);

        public void Dispose()
        {
            _channels.Clear();
            TryClose(_currentSocket);
            TryClose(_nextSocket);
        }

        private void UpdateSocket()
        {
            _nextSocket?.ClearEvents();
            TryClose(_nextSocket);

            bool needCreateSocket = _channels.Count > 0;
            if (needCreateSocket)
                _nextSocket = GetNextSocket();
            else
                StopSockets();
        }

        private void StopSockets()
        {
			_nextSocket?.ClearEvents();
			TryClose(_nextSocket);
            _nextSocket = null;

            _currentSocket?.ClearEvents();
            TryClose(_currentSocket);
            _currentSocket = null;

            GameLogger.debug($"{TAG} WebSocket pool {_poolName} closed");
		}

		public void OnApplicationFocus()
		{
#if !UNITY_EDITOR
			UpdateSocket();
#endif
		}

		public void OnApplicationUnFocus()
		{
#if !UNITY_EDITOR
			StopSockets();
#endif
		}

        private WebSocket GetNextSocket()
        {
            WebSocket socket = CreateSocket();
            if (socket == null)
                return null;

            AddSocketListeners(socket);

            socket.Connect();

            return socket;
        }

        private WebSocket CreateSocket()
        {
            string url = ConcatSocketUrl();

            WebSocket socket;
            try
            {
                socket = WebSocketFactory.CreateInstance(url);
                // Debug.Log($"{Time.frameCount}: Create socket to {url}");
            }
            catch (Exception e)
            {
                GameLogger.error(e.Message);
                return null;
            }

            return socket;
        }

        private static void TryClose(IWebSocket socket)
        {
            if (socket == null)
                return;

			WebSocketState state = socket.State;
            if (state == WebSocketState.Connecting || state == WebSocketState.Open)
                socket?.Close();
        }

        private void AddSocketListeners(IWebSocket socket)
        {
			socket.OnMessage += bytes =>
			{
				var message = Encoding.UTF8.GetString(bytes);

				OnMessageSocketInOtherThread(socket.InstanceId, message);
			};
            socket.OnOpen += OnOpenNextSocketInOtherThread;
            socket.OnClose += closeCode => OnCloseSocketInOtherThread(socket.InstanceId, closeCode);
            socket.OnError += errorMessage => OnErrorSocketInOtherThread(socket.InstanceId, errorMessage);
        }

        private string ConcatSocketUrl()
        {
            StringBuilder builder = new StringBuilder(_webSocketUrl);
            int maxIndex = _channels.Count - 1;

            for (int index = 0; index <= maxIndex; index++)
            {
                builder.Append(_channels[index]);
                if (index != maxIndex)
                    builder.Append(",");
            }

            return builder.ToString();
        }

		private void OnOpenNextSocket(object obj)
		{
			GameLogger.debug($"{TAG} Pool {_poolName} channels {string.Join(",", _channels)}");

			OnChangeState(State.Connected);

			TryClose(_currentSocket);

            _currentSocket = _nextSocket;
            _nextSocket = null;
        }

        private void OnMessageSocket(object obj)
        {
            MessageData data = (MessageData) obj;
            if (data.ID == _currentSocket?.InstanceId)
            {
                GameLogger.debug($"{TAG} Message from socket with ID = {data.ID} in pool {_poolName}");
                OnMessage?.Invoke(data.Message);
            }
            else
                GameLogger.warning($"{TAG} Ignore message {data.Message} from ID = {data.ID} in pool {_poolName}"); // todo VasilevLE debug
        }

        private void OnCloseSocket(object obj)
        {
			CloseMessageData data = (CloseMessageData) obj;
			WebSocket curSocket = null;

			if (data.ID == _currentSocket?.InstanceId)
				curSocket = _currentSocket;
			else if (data.ID == _nextSocket?.InstanceId)
				curSocket = _nextSocket;

			if (data.CloseCode == WebSocketCloseCode.Normal) // При штатном закрытии сокета
			{
				//OnChangeState(State.Disconnected); // Не меняем состояние чтобы не слать серверу sr0, sr1
				//GameLogger.debug($"{TAG} WebSocket with ID {data.ID} in pool {_poolName} normal closed.");
			}
			else
            {
				OnChangeState(State.Disconnected, SocketController.DISCONNECT_CHECK_CONNECTION);

				if (data.CloseCode == WebSocketCloseCode.Abnormal)
				{
					if (!_errorSockets.Contains(data.ID))
						_errorSockets.Add(data.ID);
				}

                if (_errorSockets.Contains(data.ID))
                {
                    // Debug.LogWarning($"{TAG} {Time.frameCount}: WebSocket with ID {data.ID} in pool {_poolName} closed after error. URL = {curSocket?.Url}, CloseCode = {data.CloseCode}");
                    GameLogger.warning($"{TAG} WebSocket closed after error. URL = {curSocket?.Url}, CloseCode = {data.CloseCode}");
                    _errorSockets.Remove(data.ID);
                    // ServerLogs.WebSocketDisconnectOnError();

                    /*Установить соединение заново через 5 секунд*/
					GameLogger.debug($"{TAG} WebSocket set reconnect after 5 seconds.");
                    Utils.Utils.Wait(5f)
                        .Then(() =>
                        {
							GameLogger.debug($"{TAG} WebSocket start reconnecting socket with ID {data.ID} in pool {_poolName}.");
                            if (curSocket != null)
                            {
								GameLogger.debug($"{TAG} WebSocket reconnecting after failed socket with ID {data.ID} in pool {_poolName}.");
                                UpdateSocket();
                            }
                        });
                }
                else
					GameLogger.warning($"{TAG} {Time.frameCount}: WebSocket with ID {data.ID} in pool {_poolName} closed. CloseCode = {data.CloseCode}");
            }
        }

        private void OnErrorSocket(object obj)
        {
            /*No info
             *If the user agent was required to fail the WebSocket connection or the WebSocket connection is closed with prejudice,
             *fire a simple event named error at the WebSocket object.*/
            MessageData data = (MessageData) obj;
			GameLogger.warning($"{TAG} WebSocket error. Socket ID = {data.ID}. Pool name = {_poolName}. Error message = {data.Message}");
            _errorSockets.Add(data.ID);
        }

		private void OnChangeState(State state, string message = null)
		{
			if (_curState == state)
				return;

			_curState = state;

			if (_curState == State.Connected)
				OnConnect?.Invoke();
			else
				OnDisconnect?.Invoke(message);
		}

        private void OnOpenNextSocketInOtherThread() => UniRx.MainThreadDispatcher.Post(OnOpenNextSocket, null);
		private void OnMessageSocketInOtherThread(int id, string message) => UniRx.MainThreadDispatcher.Post(OnMessageSocket, new MessageData(id, message));
		private void OnCloseSocketInOtherThread(int id, WebSocketCloseCode closeCode) => UniRx.MainThreadDispatcher.Post(OnCloseSocket, new CloseMessageData(id, closeCode));
		private void OnErrorSocketInOtherThread(int id, string errorMessage) => UniRx.MainThreadDispatcher.Post(OnErrorSocket, new MessageData(id, errorMessage));

        private readonly struct MessageData
        {
            public int ID { get; }
            public string Message { get; }

            public MessageData(int id, string message)
            {
                ID = id;
                Message = message;
            }
        }

        private readonly struct CloseMessageData
        {
            public int ID { get; }
            public WebSocketCloseCode CloseCode { get; }

            public CloseMessageData(int id, WebSocketCloseCode closeCode)
            {
                ID = id;
                CloseCode = closeCode;
            }
        }

		public void OnFrame()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
				_nextSocket?.DispatchMessageQueue();
				_currentSocket?.DispatchMessageQueue();
#endif
		}
	}
}