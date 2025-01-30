using System.Collections.Generic;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using static Assets.Scripts.Network.Queries.QueryManager;

namespace Assets.Scripts.Network.Queries
{
    public class BaseResponse : IError
    {
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("warn", NullValueHandling = NullValueHandling.Ignore)]
        public string Warning { get; set; }

        [JsonProperty("fv", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long TimeStamp { get; set; }

        [JsonProperty("ssk", NullValueHandling = NullValueHandling.Ignore)]
        public int? Ssk { get; set; }

        [JsonProperty("fstssk", NullValueHandling = NullValueHandling.Ignore)]
        public int? Fstssk { get; set; }

		[JsonProperty("is_local", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsLocal { get; set; }
		
		[JsonProperty("c_items", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<int, ServerItemInfo> ChangeItems { get; set; }

		[JsonProperty("profile_changed", NullValueHandling = NullValueHandling.Ignore)]
		public bool? ProfileChanged { get; set; }

		[JsonProperty("remote_profile", NullValueHandling = NullValueHandling.Ignore)]
		public ServerRemoteUser RemoteProfile { get; set; }
		
		public virtual void OnParsed(string originalJson)
        {
            GameTime.UpdateFromServer(TimeStamp);

            if (Fstssk.HasValue)
                Game.SessionKey = Fstssk;
        }

        public BaseResponse()
        {
            
        }
    }
}
