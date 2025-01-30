using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.User.BankPacks
{
    public class UserBankPackItem
	{
		private const int BOOKMARK_COIN = 1;
		private const int BOOKMARK_BANK = 2;
		private const int BOOKMARK_OTHER = 3;

		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; private set; }
        [JsonProperty("general", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool General { get; private set; }
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string type;
        public int Type { get; private set; } = UserBankPackItemType.TYPE_NONE;
        [JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Icon;

		[JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private List<ItemCountFormula> _items;

		public List<ItemCountFormula> Items => _items ??= new List<ItemCountFormula>{new ItemCountFormula(ShopItem.Item, ShopItem.Count)};
		
        [JsonProperty("price", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ItemCount Price;
        [JsonProperty("pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int BankPos { get; private set; }
        [JsonProperty("offer_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int OfferId { get; private set; }
        [JsonProperty("sale", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Sale { get; private set; }
        [JsonProperty("sale_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SaleKey { get; private set; }
        [JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AdvertId { get; private set; }
		
		/// <summary>
		/// Сортировка в банке для свернутого состояния
		/// </summary>
		[JsonProperty("sort", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Sort { get; private set; }
		/// <summary>
		/// Сортировка в банке для развернутого состояния
		/// </summary>		
		[JsonProperty("sort_all", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int SortAll { get; private set; }

		[JsonProperty("more", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int more;
        public bool NeedMoreDetailedBtn => more > 0;
        
        [JsonProperty("old_real_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OldProductId { get; private set; }

		public int Group { get; private set; }
        public int Pos { get; internal set; }
        public int ShopPos { get; internal set; }

		public ShopItem ShopItem { get; private set; }

		public int Bookmark { get; internal set; }
		public int StartLevel { get; internal set; }
		public int EndLevel { get; internal set; }
		public int[] PosMustBeBought { get; private set; }
		public int[] PosMustNotBeBought { get; private set; }
		public int EventId { get; private set; }

		public bool Updated { get; internal set; }

        public string Name => ("bank_pack_title_" + Id).Localize();
        public string Desc => ("bank_pack_desc_" + Id).Localize();

        public bool IsAd => Type == UserBankPackItemType.TYPE_ADVERT;
		public bool NeedCustomIcon => string.IsNullOrEmpty(Icon) == false;

		public string IconPath => $"img/icons/{Icon}";

		private int StartWeight = -1;
		private int EndWeight = -1;
		
		public bool IsValidLevel(int level) =>
			(StartLevel == 0 || level >= StartLevel) && (EndLevel == 0 || level <= EndLevel);

		public bool IsValidWeight(int userWeight) =>
			(userWeight >= StartWeight || StartWeight < 0) && (userWeight <= EndWeight || EndWeight < 0);

		public bool IsValidByBoughtPositions()
		{
			if (Game.User == null || Game.User.Bank == null)
				return false;

			if (!PosMustBeBought.IsNullOrEmpty() && PosMustBeBought.Any(p => !Game.User.Bank.IsPositionBought(p)))
				return false;
			
			if (!PosMustNotBeBought.IsNullOrEmpty() && PosMustNotBeBought.Any(p => Game.User.Bank.IsPositionBought(p)))
				return false;

			return true;
		}

        private UserBankItem bankItem;
        public UserBankItem BankItem
        {
            get
            {
                if (Type == UserBankPackItemType.TYPE_PACK)
                    return null;

                if (bankItem == null)
                {
//                    if (Offer != null)
//                        bankItem = offer.BankItem;
//                    else
                    
                        bankItem = Game.User.Bank.GetById(BankPos);
                }

                return bankItem;
            }
        }

		public ItemCount MainItemCount()
        {
            if (BankItem != null && BankItem.BuyItemCount != null)
				return BankItem.BuyItemCount;

			if (Items != null && Items.Count > 0)
                return Items[0];

            return null;
        }
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Type = UserBankPackItemType.GetTypeByString(type);
            type = null;
        }

		public static bool IsEqual(UserBankPackItem item1, UserBankPackItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var cond1 = item1.Pos == item2.Pos;
            var cond2 = item1.Id == item2.Id;

            return cond1 && cond2;
        }
    }
}