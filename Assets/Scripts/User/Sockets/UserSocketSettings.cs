namespace Assets.Scripts.User.Sockets
{
	public class UserSocketSettings
	{
		public string Id { get; private set; }
		public string Host { get; private set; }
		public int Port { get; private set; }

		public UserSocketSettings(string id, string host, int port)
		{
			Id = id;
			Host = host;
			Port = port;
		}

		public override string ToString()
		{
			return $"{Id} | {Host}:{Port}";
		}
	}
}