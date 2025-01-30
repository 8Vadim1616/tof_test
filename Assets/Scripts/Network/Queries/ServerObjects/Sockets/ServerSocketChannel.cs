using System;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects.Sockets
{
	public class ServerSocketChannel
	{
		[JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Name { get; private set; }

		[JsonProperty("db", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[Obsolete("Use UserSocket.GetServerId(ServerChannel)")]
		public string ServerId { get; private set; }

		[JsonProperty("pool", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Pool { get; private set; }
	}
}
