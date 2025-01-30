using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Network.Queries.ServerObjects.Sockets;
using Assets.Scripts.Network.Sockets;
using Assets.Scripts.Network.Sockets.WebSockets;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.User.Sockets
{
    public class UserSockets : IDisposable
    {
        private const string WEB_SOCKET_KEY = "wss";
		private const string TAG = "[UserSockets]";
        public int PingInterval = 60;

        private static bool _inited;
        private static readonly List<UserSocketSettings> Settings = new List<UserSocketSettings>();
        private static SocketTypes _socketType;

        private List<AbstractSocketController> _sockets = new List<AbstractSocketController>();
        private static GetServerOperation.WebSocketSettings _webSocketSetup;
        private bool _delaySubscribing;
        private readonly List<ServerSocketChannel> _delayChannels = new List<ServerSocketChannel>();

        public static void Init(GetServerOperation.WebSocketSettings webSocketSetup)
        {
            if (_inited)
                return;

            InitSocketSettings(webSocketSetup);

            _inited = true;
        }

        private static void InitSocketSettings(GetServerOperation.WebSocketSettings webSocketSetup)
        {
            _webSocketSetup = webSocketSetup;

			_socketType = SocketTypes.Nchan;

            switch (_socketType)
            {
                case SocketTypes.Redis:
                    /*foreach (var server in data.servers)
                        Settings.Add(new UserSocketSettings(server.Key, server.Value.host, server.Value.port));*/
                    break;
                case SocketTypes.Nchan:
                    Settings.Add(new UserSocketSettings(WEB_SOCKET_KEY, webSocketSetup.host, webSocketSetup.port));
                    break;
            }
		}

		public UserSockets()
		{
			AddListeners();
		}

        public void Dispose()
        {
			RemoveListeners();

            if (_sockets != null)
                foreach (var socket in _sockets)
                    socket.Dispose();

            _inited = false;
            _sockets = null;
        }

        public void CheckConnection()
        {
            if (_sockets != null)
                foreach (var socket in _sockets)
                    socket.CheckConnection();
        }

        public void StartDelaySubscribing()
        {
            _delaySubscribing = true;
        }

        public void StopDelaySubscribing()
        {
            if (!_delaySubscribing)
                return;
            
            _delaySubscribing = false;

            Subscribe(_delayChannels);
            _delayChannels.Clear();
        }

        public UserSocketSettings GetSettingsById(string serverId)
        {
            return Settings.FirstOrDefault(s => s.Id.Equals(serverId));
        }

        public AbstractSocketController GetSocketById(string serverId)
        {
            return _sockets.FirstOrDefault(s => s.Settings.Id == serverId);
        }

        public bool IsConnectedToChannel(ServerSocketChannel socketChannel)
        {
            AbstractSocketController socket = GetSocketById(GetServerId(socketChannel));
            return socket != null && socket.IsConnectedToChannel(socketChannel.Name, socketChannel.Pool);
        }

        private void OnDataStringReceive(string str)
        {
            Debug.Log($"{TAG} [OnDataReceive] {str}");

            if (str[0] == '{')
            {
                BaseApiResponse response = null;
                try
                {
                    response = JsonConvert.DeserializeObject<BaseApiResponse>(str);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

                if (response != null)
                    Game.ServerDataUpdater.Update(response);
            }
        }

        private void OnDataBytesReceive(byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            OnDataStringReceive(str);
        }

        private void OnConnect(AbstractSocketController controller)
        {
			//Game.Network.OnHaveInternet();

            Debug.Log($"{TAG} [OnConnect]");

            ServerLogs.SocketConnect();
		}

        private void OnDisconnect(AbstractSocketController controller, string message)
        {
			Debug.Log($"{TAG} [OnDisconnect] {message}");

            int type = -1;
            if (!string.IsNullOrEmpty(message))
            {
                switch (message)
                {
                    case SocketController.DISCONNECT_NO_CONNECTED:
                        type = 1;
                        break;
                    case SocketController.DISCONNECT_PING:
                        type = 2;
                        break;
                    case SocketController.DISCONNECT_CAN_WRITE:
                        type = 3;
                        break;
                    case SocketController.DISCONNECT_SEND_SOCKET:
                        type = 4;
                        break;
                    case SocketController.DISCONNECT_CHECK_CONNECTION:
                        type = 5;
                        break;
                }
            }

			ServerLogs.SocketDisconnect(type);
        }

        public void Unsubscribe(ServerSocketChannel socketChannel)
        {
            if (!_inited)
                return;

            string serverId = GetServerId(socketChannel);
            AbstractSocketController socket = GetSocketById(serverId);
            if (socket == null)
                return;
            
            if (socket.IsConnectedToChannel(socketChannel.Name, socketChannel.Pool))
                socket.Unsubscribe(socketChannel.Name, socketChannel.Pool);
        }

#pragma warning disable 618
        public static string GetServerId(ServerSocketChannel socketChannel) =>
            _socketType == SocketTypes.Nchan ? WEB_SOCKET_KEY : socketChannel.ServerId;
#pragma warning restore 618

        public void Subscribe(ServerSocketChannel socketChannel)
        {
            if (!_inited)
                return;

            Assert.IsNotNull(socketChannel);

            if (_delaySubscribing)
            {
                AddChannelToDelayList(socketChannel);
                return;
            }

            string serverId = GetServerId(socketChannel);
            string channelName = socketChannel.Name;
            string pool = socketChannel.Pool;
            
            if (serverId == null || channelName == null)
                return;

            if (IsConnectedToChannel(socketChannel))
                return;

            AbstractSocketController socket = GetSocketById(serverId);

            if (socket == null)
            {
                socket = CreateSocket(serverId);
                _sockets.Add(socket);
            }

            socket.Subscribe(channelName, pool);
        }

        public void Subscribe(IList<ServerSocketChannel> channels)
        {
            if (!_inited)
                return;
            
            if (_delaySubscribing)
            {
                AddChannelToDelayList(channels);
                return;
            }
            
            Dictionary<string, Dictionary<string, List<string>>> serverPoolChannelTree = GetServerPoolChannelTree(channels);

            foreach (KeyValuePair<string, Dictionary<string, List<string>>> servers in serverPoolChannelTree)
            {
                string serverId = servers.Key;
                
                AbstractSocketController socket = GetSocketById(serverId);

                if (socket == null)
                {
                    socket = CreateSocket(serverId);
                    _sockets.Add(socket);
                }

                foreach (KeyValuePair<string,List<string>> pools in servers.Value)
                {
                    string pool = pools.Key;
                    List<string> channelNames = pools.Value;
                    socket.Subscribe(channelNames, pool);
                }
            }
        }

        private void AddChannelToDelayList(ServerSocketChannel socketChannel)
        {
            if (!_delayChannels.Contains(socketChannel))
                _delayChannels.Add(socketChannel);
        }

        private void AddChannelToDelayList(IList<ServerSocketChannel> channels)
        {
            foreach (ServerSocketChannel serverChannel in channels)
                AddChannelToDelayList(serverChannel);
        }

        private Dictionary<string, Dictionary<string, List<string>>> GetServerPoolChannelTree(IList<ServerSocketChannel> channels)
        {
            Dictionary<string, Dictionary<string, List<string>>> serverPoolChannelTree =
                new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (ServerSocketChannel channel in channels)
            {
                string serverId = GetServerId(channel);
                string channelName = channel.Name;
                string pool = channel.Pool;

                if (serverId == null || channelName == null)
                    continue;

                if (IsConnectedToChannel(channel))
                    continue;

                if (!serverPoolChannelTree.ContainsKey(serverId))
                    serverPoolChannelTree[serverId] = new Dictionary<string, List<string>>(1);

                if (!serverPoolChannelTree[serverId].ContainsKey(pool))
                    serverPoolChannelTree[serverId][pool] = new List<string>(1);

                serverPoolChannelTree[serverId][pool].Add(channelName);
            }

            return serverPoolChannelTree;
        }

        private AbstractSocketController CreateSocket(string serverId)
        {
            UserSocketSettings settings = GetSettingsById(serverId);
            if (settings == null)
                throw new Exception($"{TAG} Unknown socket server {serverId}");

            AbstractSocketController socketController = _socketType switch
            {
                SocketTypes.Redis => CreateTcpSocket(settings),
                SocketTypes.Nchan => CreateWebSocket(settings, _webSocketSetup),
                _ => throw new ArgumentOutOfRangeException()
            };

            socketController.OnConnect += OnConnect;
            socketController.OnDisconnect += OnDisconnect;
            socketController.OnDataBytesReceive += OnDataBytesReceive;
            socketController.OnDataStringReceive += OnDataStringReceive;

            return socketController;
        }

        private AbstractSocketController CreateWebSocket(UserSocketSettings settings, GetServerOperation.WebSocketSettings webSocketSettings) => new WebSocketController(settings, webSocketSettings);
		private AbstractSocketController CreateTcpSocket(UserSocketSettings settings) => new SocketController(settings);

		private void OnApplicationFocus()
		{
			if (_sockets != null)
			{
				foreach (var socket in _sockets)
					socket.OnApplicationFocus();
			}
		}

		private void OnApplicationUnFocus()
		{
			if (_sockets != null)
			{
				foreach (var socket in _sockets)
					socket.OnApplicationUnFocus();
			}
		}

		private void AddListeners()
		{
#if !UNITY_WEBGL
			Game.GameReloader.ApplicationFocus += OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus += OnApplicationUnFocus;
#endif
		}

		private void RemoveListeners()
		{
#if !UNITY_WEBGL
			Game.GameReloader.ApplicationFocus -= OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus -= OnApplicationUnFocus;
#endif
		}
    }
}