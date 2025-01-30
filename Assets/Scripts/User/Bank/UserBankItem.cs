using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Platform.Adapter;
using Assets.Scripts.Platform.Mobile.Purchases;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Static.Bank
{
    public class UserBankItem
    {
		private const string PATH = "img/Bank/";

        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("tier")]
        public int Tier { get; private set; }

        [JsonProperty("val")]
        public int Count { get; private set; }

        [JsonProperty("bonus_val")]
        public int Bonus { get; private set; }

		[JsonProperty("old_value")]
		public int WasCount => SnData.OldValue;

		[JsonProperty("sale")]
		public int SalePrc => SnData.Sale;

		public List<ItemCountFormula> BuyItems;

		public ItemCount BuyItemCount => BuyItems?.Count > 0 ? BuyItems?[0] : null;

		// public List<ItemCount> BuyItemsWithOffers()
		// {
		// 	var result = BuyItems.Clone();
		// 	
		// 	if (Game.User.Offers.TryGetOffer<UserOfferBlackFriday>(out var blackFriday))
		// 		if (blackFriday.IsAffectsToUserBankItem(this))
		// 			foreach (var itemCount in result)
		// 				itemCount.Count *= Mathf.RoundToInt(blackFriday.Data.Bonus / 100f);
		//
		// 	return result;
		// }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("best")]
        private int Best;

        public bool IsBest => Best == 2;
        public bool IsHit => Best == 1;

        [JsonProperty("action")]
        public string OfferId { get; private set; }

        [JsonProperty("period")]
        public int Period => SnData.Period;

        [JsonProperty("bonus_items")]
        public List<ItemCount> BonusItems;

        [JsonProperty("transl")]
        public string TranslKey { get; private set; }

        [JsonProperty("prefix")] private string prefix;
        public string Prefix => prefix ?? Game.Settings.IAPPrefix;
		
		[JsonProperty("prefix_old")] private string prefixOld;
		public string PrefixOld => prefixOld ?? Game.Settings.IAPPrefixOld;

        [JsonProperty("uni_pos")]
        public string UniversalPosition { get; private set; }

        [JsonProperty("time")]
        public int Time { get; private set; }

		public int WeighUsd { get; private set; }

		[JsonProperty("sort")] private int _sort;

		public int Sort => _sort;

		[JsonProperty("soc_val")]
		private float _snMoney { get; set; }

		public float SnMoney
		{
			get
			{
				if (Game.Social.Network == SocialNetwork.GAMES_MAIL_RU)
				{
					var currencyName = Game.Social.Adapter.GetProfileCached().Currency;
					//
					// if (currencyName == "USD")
					// {
					// 	return Game.Static.BankGMRCurrency.GetUSDForRUB(_snMoney);
					// }
					// else if (currencyName == "EUR")
					// {
					// 	return Game.Static.BankGMRCurrency.GetEURForRUB(_snMoney);
					// }
				}

				return _snMoney;
			}
		}

		public string Position => !string.IsNullOrEmpty(UniversalPosition)
            ? UniversalPosition
            : Id.ToString();

        public string ProductId => Prefix + Position;
		public IStoreProduct Product
		{
			get
			{
#if !UNITY_WEBGL
				return Game.Mobile?.Purchases?.GetProduct(ProductId);
#else
				return null;
#endif
			}
		}
		
		public string OldProductId => !PrefixOld.IsNullOrEmpty() ? PrefixOld + Position : null;
		public IStoreProduct ProductOld
		{
			get
			{
				if (OldProductId.IsNullOrEmpty())
					return null;
#if !UNITY_WEBGL
				return Game.Mobile?.Purchases?.GetProduct(OldProductId);
#else
				return null;
#endif
			}
		}

		public bool IsSubscription => Time > 0;

		private StaticBankItem.StaticBankItemSnData SnData = null;

		public UserBankItem() { }

		public UserBankItem(int buyItemId, int count)
		{
			BuyItems.Add(new ItemCountFormula(buyItemId, count));
		}

		/** Обёртка для получения цены в мобилке **/
		public UserBankItem(int id, string prefix)
		{
			Id = id;
			this.prefix = prefix;
		}

		public UserBankItem(int id, StaticBankItem staticBankItem)
		{
			Id = id;
			UniversalPosition = staticBankItem.UniversalPosition;
			Tier = staticBankItem.Tier;
			Icon = staticBankItem.Icon;
			BuyItems = staticBankItem.Items;
			WeighUsd = staticBankItem.WeightUsd;
			_sort = staticBankItem.Sort;

			SnData = staticBankItem.SnData;

			if (!staticBankItem.CloseActions.IsNullOrEmpty())
				CloseActionIds ??= staticBankItem.CloseActions?
					.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => int.TryParse(x, out int value) ? value : 0)
					.Where(x => x > 0)
					.ToArray();
		}

		private int[] CloseActionIds;
		public bool CanCloseAction(int actionId) =>
			!CloseActionIds.IsNullOrEmpty() && CloseActionIds.Contains(actionId);

		public string Description
        {
            get
            {
                var pref = "bank_desc_";
                if (!string.IsNullOrEmpty(TranslKey) && Game.Localization.ContainsKey(pref + TranslKey))
                    return (pref + TranslKey).Localize();

                return (pref + Id).Localize();
            }
        }

        public string Name
        {
            get
            {
                var pref = "bank_";
                if (!string.IsNullOrEmpty(TranslKey) && Game.Localization.ContainsKey(pref + TranslKey))
                    return (pref + TranslKey).Localize();

                return (pref + Id).Localize();
            }
        }
		
		public string OldLabel
		{
			get
			{
				if (CurrencyIsText)
				{
					if (ProductOld != null)
						return ProductOld.Price;
				}

				return null;
			}
		}

        public string Label
		{
			get
			{
				if (CurrencyIsText)
				{
					if (Product != null)
						return Product.Price;

					var currencyName = Game.Social.Adapter.GetProfileCached()?.Currency ?? "USD";

					return TextFormatting.GetCurrencyText(SnMoney, currencyName, 1, Game.Social.Network != SocialNetwork.RAMBLER);
				}

				return SnMoney.ToString();
			}
		}

		public static bool CurrencyIsText
        {
            get
            {
				if (SocialNetwork.IsFacebook ||
					Game.Social.Network == SocialNetwork.DRAUGIEM ||
					Game.Social.Network == SocialNetwork.YAHOO ||
					Game.Social.Network == SocialNetwork.RAMBLER ||
					Game.Social.Network == SocialNetwork.GAMES_MAIL_RU ||
					Game.Social.Network == SocialNetwork.PLINGA)
					return true;

#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
				return true;
#endif
				return false;
			}
        }

		public static void GetIcon(Image image)
		{
			var fileName = "bank_money_" + Game.Social.Network;
			image.LoadFromAssets(PATH + fileName);
		}

		public bool TryGetOldBankItemPriceString(out string result)
		{
			result = OldLabel;
			
			Debug.Log($"BankId = {Id}; Label = {Label}; OldLabel = {OldLabel}; Prefix = {Prefix}; OldPrefix = {PrefixOld}");
			
			return !result.IsNullOrEmpty() && OldLabel != Label;
		}
	}
}
