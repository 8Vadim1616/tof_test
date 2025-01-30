using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ServerUserInfo
    {
        [JsonProperty("uid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Uid { get; set; }
        
        [JsonProperty("register", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Register { get; set; }

		[JsonProperty("avatar_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? AvatarId { get; set; }

		[JsonProperty("frame_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? AvatarFrameId { get; set; }

		[JsonProperty("fn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty("ln", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LastName { get; set; }
		
		[JsonProperty("nick", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Nick { get; set; }
		
		[JsonProperty("level", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Level;
        
        [JsonProperty("in_app_pref", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string InAppPrefix { get; set; }
        
		[JsonProperty("grp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Group { get; set; }

		[JsonProperty("rt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long? LastWasOnline { get; private set; }

		[JsonProperty("golden_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long? GoldenNameTime { get; private set; }

		[JsonProperty("regtm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long? UserRegistrationDate;
		
#region Energy
		[JsonProperty("energy")]
		public long? EnergyTimeWhenFull { get; private set; }
        
		[JsonProperty("energy_max")]
		public int? MaxEnergy { get; private set; }

		[JsonProperty("energy_time")]
		public long? EnergyTime { get; private set; }
        
		[JsonProperty("energy_extra")]
		public int? BonusEnergy { get; private set; }
#endregion
	}
}