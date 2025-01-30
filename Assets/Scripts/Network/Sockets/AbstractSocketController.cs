using System;
using System.Collections.Generic;
using Assets.Scripts.User.Sockets;

namespace Assets.Scripts.Network.Sockets
{
	public abstract class AbstractSocketController : IDisposable
	{
		public static string COMMAND_SUBSCRIBE = "subscribe";
		public static string COMMAND_UNSUBSCRIBE = "unsubscribe";

		public event Action<AbstractSocketController, string> OnDisconnect;
		public event Action<AbstractSocketController> OnConnect;
		public event Action<byte[]> OnDataBytesReceive;
		public event Action<string> OnDataStringReceive;

		public abstract bool IsConnectedToChannel(string channelName, string channelPool);

		public readonly UserSocketSettings Settings;

		protected AbstractSocketController(UserSocketSettings settings)
		{
			Settings = settings;
		}

		internal virtual void CheckConnection() { }
		protected abstract void Disconnect();
		public abstract void Subscribe(string channelName, string pool);
		public abstract void Subscribe(List<string> channelNames, string pool);
		public abstract void Unsubscribe(string channelName, string pool);
		public abstract void Unsubscribe(List<string> channelNames, string pool);
		public abstract void OnApplicationFocus();
		public abstract void OnApplicationUnFocus();

		protected virtual void OnConnected() =>
			OnConnect?.Invoke(this);

		protected void OnDisconnected(string message) =>
			OnDisconnect?.Invoke(this, message);

		protected void OnDataBytesReceived(byte[] bytes) =>
			OnDataBytesReceive?.Invoke(bytes);

		protected void OnDataStringReceived(string data) =>
			OnDataStringReceive?.Invoke(data);

		public override string ToString()
		{
			return $"{Settings}";
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}
