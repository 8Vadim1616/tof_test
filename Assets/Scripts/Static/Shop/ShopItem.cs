using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Static.Shop
{
    public class ShopItem : StaticCollectionItemCode
	{
		[JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private long _count;
		public long Count => ItemCountFormula?.GetCount() ?? _count;
		
		[JsonProperty("time", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long Time { get; private set; }

        [JsonProperty("it_in", DefaultValueHandling = DefaultValueHandling.Ignore)]
        protected ItemCount[] prices;

        [JsonProperty("item", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int _itemId;
        public Item Item => Game.Static.Items.Get(_itemId);
		
		[JsonProperty("formula", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private string _formula;
		private ItemCountFormula _itemCountFormula;
		public ItemCountFormula ItemCountFormula => !_formula.IsNullOrEmpty() ? _itemCountFormula ??= new ItemCountFormula(Item, _formula) : null;
		
		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Type { get; private set; }

        public ItemCount Price => !prices.IsNullOrEmpty() ? prices[0] : null;

		//Айтем за рекламу
		[JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int AdvertId { get; private set; }
		public bool IsAd => AdvertId > 0;

		//Айтем за банковскую позицию (на будущее)
		[JsonProperty("pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private int _bankItemId;
		public UserBankItem BankItem => IsReal ? Game.User.Bank.GetById(_bankItemId) : null;
		public bool IsReal => _bankItemId != default;
		public ItemCount BuyItemCount => Item.CreateItemCount(Count);
	}
}
