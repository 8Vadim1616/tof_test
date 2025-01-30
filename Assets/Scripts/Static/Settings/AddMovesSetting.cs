using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Shop;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Assets.Scripts.Static.Settings
{
	public class AddMovesSetting
	{
		[JsonProperty("shop_item", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private int _shopItemId;
		[JsonProperty("drop", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private int _dropId;
		[JsonProperty("ad", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool NeedAdvertOption;

		public List<Item> AdditionalBoosts
		{
			get
			{
				if (_additionalBoosts == null)
					_additionalBoosts = Game.Static.Drops.GetDrop(_dropId)?.GetAllItems() ?? new List<Item>();

				return _additionalBoosts;
			}
		}
		private List<Item> _additionalBoosts = null;

		public ShopItem ShopItem => Game.Static.Shop.Get(_shopItemId);
	}

	public class AddMovesAdvertSetting
	{
		[JsonProperty("shop_item", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int ShopItemId;
	}
}