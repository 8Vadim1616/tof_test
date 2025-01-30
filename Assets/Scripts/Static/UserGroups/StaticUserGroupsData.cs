using Newtonsoft.Json;

namespace Assets.Scripts.Static.UserGroups
{
	public class StaticUserGroupsData : StaticCollectionItemCode
	{
		[JsonProperty("curr_grp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? CurrentGroup { get; set; }

		[JsonProperty("payer", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool? IsPayer { get; set; }

		[JsonProperty("min_reg_ver", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string MinRegisterVersion { get; set; }

		[JsonProperty("max_reg_ver", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string MaxRegisterVersion { get; set; }

		[JsonProperty("slevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? StartLevel { get; set; }

		[JsonProperty("elevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? EndLevel { get; set; }

		[JsonProperty("dev", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Divider { get; set; }

		[JsonProperty("val", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Modulo { get; set; }

		[JsonProperty("set_grp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int SetGroup { get; set; }
	}
}