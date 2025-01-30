using System.Collections.Generic;
using Assets.Scripts.Network.Queries.ServerObjects.Sockets;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Static.Tower;
using Assets.Scripts.User;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ServerUser
	{
		[JsonProperty("crash_log", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool? CrashLog { get; private set; }

		[JsonProperty("info", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerUserInfo Info { get; set; }

		[JsonProperty("tower", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public UserTower Tower;
		
		[JsonProperty("ability", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public UserAbility Ability;
		
		[JsonProperty("bank", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerUserBank Bank;

		[JsonProperty("bank_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, BankItem> BankPos;

		[JsonProperty("real_bank_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, JToken> RealBankPos;

		[JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerItems Items;

		[JsonProperty("shop", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<int, ShopItem> Shop;

		[JsonConverter(typeof(NoLogsConverter))]
		[JsonProperty("adverts", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<int, ServerAd> Ads;

		[JsonProperty("advert_data", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerUserAdvertSavedData AdvertsData;

		[JsonProperty("sett", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, object> Settings;

		[JsonProperty("tester", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonConverter(typeof(BoolConverter))]
		public bool? Tester { get; private set; }

		[JsonProperty("adv_partners", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, Dictionary<string, string>> AdvertisingPartners { get; private set; }

		[JsonProperty("bank_packs", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<int, UserBankPackItem> BankPacks { get; private set; }

		[JsonProperty("reg_tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long RegisterTime;

		[JsonProperty("reg_ver", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string RegisterVersion;

		[JsonProperty("channels", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, ServerSocketChannel> SocketChannels;

		[JsonProperty("win", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, (int, long)> OppenedWindows;
		
		[JsonProperty("win_m", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, long> OppenedWindowsTimes;

		[JsonProperty("last_action_tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long LastActionTime;

		[JsonProperty("iron_source")]
		public IronSourceServerSegment IronSourceSegment { get; private set; }
		
		[JsonProperty("units")]
		public Dictionary<int, int> Units { get; private set; }
		
		[JsonProperty("artifacts")]
		public Dictionary<int, int> Artifacts { get; private set; }
		
		[JsonProperty("items_delta")]
		public Dictionary<int, long> ItemsDelta { get; private set; }

		public static ServerUser OfUserData(UserData user)
		{
			var result = new ServerUser();

			result.Info = new ServerUserInfo() 
			{
				Uid = user.Uid, 
				FirstName = user.FirstName, 
				LastName = user.LastName, 
			};

			return result;
		}
	}
}