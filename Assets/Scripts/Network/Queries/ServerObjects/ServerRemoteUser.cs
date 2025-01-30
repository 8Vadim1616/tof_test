using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ServerRemoteUser
	{
		public string uid;
		public string auth_key;
		public int level;
		public string fn;
		public int rt;
		public int exp;
		public string avatar;
		public int money;
	}
}
