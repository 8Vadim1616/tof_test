using Assets.Scripts.Static;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Core
{
	public class Options
	{
		[JsonProperty("max_available_level")]
		public int MAX_AVAILABLE_LEVEL;
		
		[JsonProperty("max_available_loc")]
		public int MAX_AVAILABLE_LOC;
		
		[JsonProperty("level_delta_items")]
		public int LEVEL_DELTA_WINRATE_BY_ITEMS { get; private set; } = 50;
		[JsonProperty("delta_items")]
		public ItemCountFloat[] DeltaMovesByItemsPrc;
		
		[JsonProperty("level_delta_mouse_bonus")]
		public int LEVEL_DELTA_WIN_RATE_BY_MOUSE_BONUS { get; private set; } = 50;
		[JsonProperty("delta_mouse_bonus")]
		public ItemCountFloat[] DeltaMovesByMouseBonusPrc;

		[JsonProperty("delta_need_wr")]
		private ObjectCnt<float>[] _addWinRateByTrysCount;
		
		[JsonProperty("delta_need_wr_pay")]
		private ObjectCnt<float>[] _addWinRateByTrysCountForPayer;

		[JsonIgnore]
		public ObjectCnt<float>[] AddWinRateByTrysCount
		{
			get
			{
				if (Game.User?.Bank?.IsPayer == true)
					return _addWinRateByTrysCountForPayer;
				
				return _addWinRateByTrysCount;
			}
		}
		
		
		[JsonProperty("col_no_goals_prc")]
		public float CollectionsNoGoalsPrc = 10f;
		
		[JsonProperty("col_no_bombs_prc")]
		public float CollectionsNoBombsPrc = 30f;
		
		
		// [JsonProperty("level_live_add_max_try")]
		// public int LIVE_ADD_MAX_TRY_LEVEL { get; private set; } = 50;
		// [JsonProperty("live_add_max_try")]
		// public ItemCountFloat[] LiveAddMaxTry;

		[JsonProperty("min_wr")]
		public float MinWr = 25;
		
		[JsonProperty("max_wr")]
		public float MaxWr = 80;
	}
}