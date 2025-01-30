using System.Linq;
using Assets.Scripts.Static.Drops;
using Assets.Scripts.Static.HR;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using org.mariuszgromada.math.mxparser;
using UniRx;

namespace Assets.Scripts.Static.Items
{
	public class Item : StaticCollectionItemCode
	{
		public const string MONEY1 = "Money1";
		public const string MONEY2 = "Money2";
		public const string MYTHIC = "mythic";

		public const string WAVE_COIN = "wave_coin";
		public const string WAVE_SPECIAL_COIN = "wave_special_coin";
						
		public const string EXP = "Exp";
		public const string NOADS = "noads";
		
		public const string NULL = "Null";
		
		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Type
		{
			get;
			private set;
		} = ItemType.COMMON;

		[JsonProperty("val", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public float Value
		{
			get;
			private set;
		}

		[JsonProperty("level", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int NeedLevel
		{
			get;
			private set;
		}

		[JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string IconReplace
		{
			get;
			protected set;
		}

		[JsonProperty("time_item", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string TimeItemId
		{
			get;
			private set;
		}

		[JsonProperty("drop", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private int _dropId;

		[JsonProperty("other_panel_level", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int NeedLocatedInOtherResourcePanelLevel { get; private set; }

		public Drop Drop => _dropId > 0 ? Game.Static.Drops.GetDrop(_dropId) : null;

		public bool IsTimerActive => GameTime.Now <= Game.User.Items.GetCount(this);

		public bool HasTimeItem => !string.IsNullOrEmpty(TimeItemId);
		public Item TimeItem => HasTimeItem ? Game.Static.Items[TimeItemId] : null;

		public static string[] TimeItems = new string[]
										   {
														   
										   };

		/// <summary>
		/// Предмет на время
		/// </summary>
		public bool IsTimeItem
		{
			get
			{
				return TimeItems.Contains(ModelId);
			}
		}

		/// <summary>
		/// Предмет, котороый при добавлении увеличиват времся использования предмета TimeItemId 
		/// </summary>
		public bool IsEternalBoostAdd
		{
			get
			{
				return TimeItems.Contains(TimeItemId);
			}
		}

		public string Name => Game.Localize("item_" + Id);
		
		public override string ToString()
		{
			return $"Item {ModelId} [{Id}]";
		}

		public ItemCount CreateItemCount(long count)
		{
			return new ItemCount(this, count);
		}

		public Item()
		{
		}

		public Item(float value, string timeItemId, string iconReplace)
		{
			Value = value;
			TimeItemId = timeItemId;
			IconReplace = iconReplace;
		}

		public long UserAmount() => Game.User?.Items?[this] ?? 0;

		public ItemCount UserItemCount() => new ItemCount(this, Game.User?.Items?[this] ?? 0);

		public LongReactiveProperty UserReactive() => Game.User.Items.GetReactiveProperty(this);

		public virtual string IconPath
		{
			get
			{
				var iconName = string.IsNullOrEmpty(IconReplace) ? ModelId : IconReplace;

				return "img/icons/" + iconName;
			}
		}

		public bool IsMoney1 => this == Game.Static.Items.Money1;
		public bool IsBase => IsMoney1;
		public float ValueToTime => Value;
		
		[JsonProperty("formula", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private string _formula;
		private ItemCountFormula _itemCountFormula;
		public ItemCountFormula ItemCountFormula => !_formula.IsNullOrEmpty() ? _itemCountFormula ??= new ItemCountFormula(this, _formula) : null;

		public string GetDescription(params string[] parameters)
		{
			return ("item_desc_" + Id).Localize(parameters);
		}
	}
}
