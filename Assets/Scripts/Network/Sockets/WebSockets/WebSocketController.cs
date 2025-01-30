using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.User.Sockets;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Network.Sockets.WebSockets
{
	public sealed class WebSocketController : AbstractSocketController
	{
		private string TAG = "[WebSocketController]";

		private readonly string _webSocketUrl;
		private readonly Dictionary<string, SocketPool> _pools = new Dictionary<string, SocketPool>();

		private IDisposable _onFrame = null;

		public WebSocketController(UserSocketSettings settings, GetServerOperation.WebSocketSettings webSocketSetup) :
						base(settings)
		{
			_webSocketUrl = $"wss://{webSocketSetup.host}:{webSocketSetup.port}/{webSocketSetup.vhost}/msub?id=";
			UniRx.MainThreadDispatcher.Initialize();

			_onFrame = Utils.Utils.ForEachFrame(OnFrame);
		}

		public override bool IsConnectedToChannel(string channelName, string channelPool)
		{
			if (!_pools.ContainsKey(channelPool))
				return false;

			SocketPool pool = _pools[channelPool];
			return pool.ContainsChannel(channelName);
		}

		public override void Subscribe(string channelName, string poolName)
		{
			Debug.Log($"{TAG} Add subscription pool {poolName} channel {channelName}");
			SocketPool pool = GetExistOrNewPool(poolName);
			pool.Subscribe(channelName);
		}

		public override void Subscribe(List<string> channelNames, string poolName)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string name in channelNames)
				builder.Append(name).Append(", ");

			Debug.Log($"{TAG} Add subscription pool {poolName} channels {builder}");
			SocketPool pool = GetExistOrNewPool(poolName);
			pool.Subscribe(channelNames);
		}

		public override void Unsubscribe(string channelName, string poolName)
		{
			if (!_pools.ContainsKey(poolName))
				return;

			Debug.Log($"{TAG} Remove subscription pool {poolName} channel {channelName}");
			SocketPool pool = _pools[poolName];
			pool.Unsubscribe(channelName);
		}

		public override void Unsubscribe(List<string> channelNames, string poolName)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string name in channelNames)
				builder.Append(name).Append(", ");

			Debug.Log($"{TAG} Remove subscription pool {poolName} channels {builder}");
			SocketPool pool = GetExistOrNewPool(poolName);
			pool.Unsubscribe(channelNames);
		}

		public override void OnApplicationFocus()
		{
			Debug.Log(TAG + " OnApplicationFocus");
			
			if (_pools == null)
				return;

			foreach (KeyValuePair<string, SocketPool> pair in _pools)
				pair.Value.OnApplicationFocus();
		}

		public override void OnApplicationUnFocus()
		{
			Debug.Log(TAG + " OnApplicationUnFocus");
			
			if (_pools == null)
				return;

			foreach (KeyValuePair<string, SocketPool> pair in _pools)
				pair.Value.OnApplicationUnFocus();
		}

		private SocketPool GetExistOrNewPool(string poolName) =>
						_pools.ContainsKey(poolName) ? _pools[poolName] : CreateSocketPool(poolName);

		private SocketPool CreateSocketPool(string poolName)
		{
			SocketPool pool = new SocketPool(_webSocketUrl, poolName);
			pool.OnMessage += OnMessage;
			pool.OnConnect += OnPoolConnect;
			pool.OnDisconnect += OnPoolDisconnect;
			_pools.Add(poolName, pool);
			return pool;
		}

		private void OnMessage(string message) => OnDataStringReceived(message);
		private void OnPoolConnect() => OnConnected();
		private void OnPoolDisconnect(string message) => OnDisconnected(message);

		protected override void Disconnect()
		{
			foreach (KeyValuePair<string, SocketPool> pair in _pools)
				pair.Value.Dispose();

			OnDisconnected($"{TAG} End Connection");
			_onFrame?.Dispose();
		}

		private void OnFrame()
		{
			foreach (KeyValuePair<string, SocketPool> pair in _pools)
				pair.Value.OnFrame();
		}
	}
}