using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.User.MetaPayments;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;
using Platform.Mobile.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Static.Bank
{
	public class UserBank
	{
		private const string TAG = "[UserBank]";

		public int UserBuyWeight { get; set; }
		public int UserBuyWeightSum { get; set; }
		public int FixedUserBuyWeightSum => Mathf.Max(UserBuyWeightSum, UserBuyWeight);
		public List<int> BoughtBankPos { get; private set; }

		public int PayCount { get; set; }
		public long LevelLastPay { get; set; }
		public ServerUserBank Data { get; private set; }

		public int FixedPayCount
		{
			get
			{
				if (PayCount > 0)
					return PayCount;

				if (UserBuyWeight > 0)
					return 1;

				return PayCount;
			}
		}

		public bool IsPayer => UserBuyWeight > 0;

		public Dictionary<int, UserBankItem> All { get; private set; } = new Dictionary<int, UserBankItem>();

		public event Action<UserBankItem> OnBuySuccess;
		public event Action<string> OnEndedProccessingProduct;

		public void EndProccessingProduct(string productId)
		{
			OnEndedProccessingProduct?.Invoke(productId);
		}

		//Содержит все банковские позиции которые когда либо присылал сервер за время сессии.
		//При добавлении в список - запрашиваем у платформы данные о продуктам
		public static List<BankItem> BankItems = new List<BankItem>();

		private string curRef;
		private Dictionary<string, object> refParams;

		public UserBank()
		{
			foreach (var kv in Game.Static.Bank.All)
			{
				if (!kv.Value.SnAlowed)
					continue;

				var userBankItem = new UserBankItem(kv.Key, kv.Value);

				All.Add(kv.Key, userBankItem);
				BankItems.Add(BankItem.Of(userBankItem));

				var bankOldItem = BankItem.OfOldPos(userBankItem);
				if (bankOldItem != null)
					BankItems.Add(bankOldItem);
			}
		}

		public bool IsPositionBought(int pos)
		{
			return !BoughtBankPos.IsNullOrEmpty() && BoughtBankPos.Contains(pos);
		}

		public void Update(ServerUserBank data, bool byServer)
		{
			if (data is null)
				return;

			Data = data;

			if (data.BuyWeight.HasValue)
				UserBuyWeight = data.BuyWeight.Value;

			if (data.BuyWeightSum.HasValue)
				UserBuyWeightSum = data.BuyWeightSum.Value;

			if (data.PayCount.HasValue)
				PayCount = data.PayCount.Value;

			if (data.LevelLastPay.HasValue)
				LevelLastPay = data.LevelLastPay.Value;

			if (data.BoughtBankPos != null)
				BoughtBankPos = data.BoughtBankPos;

			if (data.ShopTimers != null)
				ShopTimers = data.ShopTimers;
		}

		public void Update(JToken token)
		{
			if (token == null) return;

			try
			{
				All = token != null ? token.ToObject<Dictionary<int, UserBankItem>>() : new Dictionary<int, UserBankItem>();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
		}
		
		public Dictionary<int, long> ShopTimers { get; private set; }

		public long GetTimeLeftForFreeShopItem(ShopItem shopItem)
		{
			if (ShopTimers == null)
				return 0;

			if (!ShopTimers.ContainsKey(shopItem.Id))
				return 0;

			return Math.Max(0, ShopTimers[shopItem.Id] - GameTime.Now);
		}

		public UserBankItem GetById(int id)
		{
			return All.Values.FirstOrDefault(item => item.Id == id);
		}

		public UserBankItem GetByProductId(string productId)
		{
			return All.Values.FirstOrDefault(item => item.ProductId == productId);
		}

		public List<UserBankItem> GetAllByItemId(int itemId)
		{
			return All.Values.Where(b => b.BuyItemCount != null &&
										 string.IsNullOrEmpty(b.OfferId) &&
										 b.BuyItemCount.ItemId == itemId)
					  .OrderBy(it => it.SnMoney)
					  .ToList();
		}

		public void SetRef(string refName)
		{
			curRef = refName;

			if (ServerLogs.LAST_LOG_CLIP != null)
				refParams = ServerLogs.GetLastLogParams();
			else
				refParams = null;
		}

		public void SetRefNull()
		{
			refParams = null;
			curRef = null;
		}

		public void Buy(UserBankItem bankItem, MetaPayment metaPayment)
		{
			/*
			if (bankItem is null)
			{
				Debug.LogWarning("BankItem was null");
				metaPayment.OnCancel();
				return;
			}

			Debug.Log("Buy BankItem " + bankItem.Id);

			ServerLogs.StartBuy(bankItem);

			AddMetaPayment(metaPayment)
				.Then(() =>
				{
#if UNITY_WEBGL
					Game.Mobile.SocialPurchase.MakePurchase(bankItem, curRef, refParams);
#else
					Game.Mobile.Purchases.MakePurchase(bankItem, curRef, refParams);
#endif
					SetRefNull();
				});
			
			*/
		}
/*
		public void BuyAd(UserBankPackItem pack, Action<IList<ItemCount>, AdOptions> onComplete = null, Action onError = null)
		{
			Debug.Log("Buy UserBankPackItem with Ad" + pack.Id);

			var point = pack.AdvertId;
			var t = (UserAdType) point;
			if (t == UserAdType.NONE)
			{
				Debug.LogWarning("Buy UserBankPackItem fail, adType: " + t);
				onError?.Invoke();
				return;
			}

			if (!(Game.AdvertisingController.IsRewardAvailable(point) && Game.User.Ads.IsRewardAdAvailable(point, true)))
			{
				Debug.LogWarning("Buy UserBankPackItem fail, ad unavailable");
				onError?.Invoke();
				return;
			}

			Game.AdvertisingController.ShowAdPoint(t, x => OnComplete(x), onError);

			void OnComplete(AdOptions adOptions)
			{
				Game.ServiceProvider.RequestPromise(new BuyBankItemPackWithAdOperation(pack.Id))
					.Then(response =>
					{
						Game.ServerDataUpdater.Update(response);


						onComplete?.Invoke(response.GetDrop(), adOptions);
					})
					.Catch(x => onError?.Invoke());
			}
		}
		*/

		/// <summary>
		/// id - реальный ("ga_XXXX")
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public BankItem GetBankItemById(string id)
		{
			return BankItems.FirstOrDefault(bankItem => bankItem.Id == id);
		}

#if UNITY_WEBGL
		public static void MakeBuyButton(ButtonText btn, Image image, UserBankItem bankItem, Action onClick, bool needDoubleClick = false)
		{
			MakeBuyButton(btn, btn.TextField, null, bankItem, onClick, needDoubleClick);
		}

		public static void MakeBuyButton(ButtonTextIcon btn, UserBankItem bankItem, Action onClick, bool needDoubleClick = false)
		{
			MakeBuyButton(btn, btn.TextField, btn.Icon, bankItem, onClick, needDoubleClick);
		}

		public static void MakeBuyButton(BasicButton btn, TextMeshProUGUI textField, Image image, UserBankItem bankItem, Action onClick, bool needDoubleClick = false)
		{
			if (btn == null || bankItem == null)
				return;

			btn.SetOnClick(null);
			btn.onClick.RemoveAllListeners();

			if (UserBankItem.CurrencyIsText)
			{
				if (image != null)
					image.SetActive(false);

				textField.text = bankItem.Label;
			}
			else
			{
				if (image != null)
				{
					image.SetActive(true);
					UserBankItem.GetIcon(image);
				}

				textField.text = bankItem.SnMoney.ToString();
			}

			if (needDoubleClick)
			{
				btn.SetOnDoubleDownCallback(() =>
				{
					if (FullscreenWebGL.isFullscreen())
						FullscreenWebGL.ExitFullscreen();
				});
				btn.SetOnDoubleClick(onClick);
			}
			else
			{
				btn.SetOnClick(() =>
				{
					if (FullscreenWebGL.isFullscreen())
						FullscreenWebGL.ExitFullscreen();
				});
				btn.onClick.AddListener(() => onClick?.Invoke());
			}
		}
#else

		public void SaveBankItemData(UserBankItem bankItem)
		{
			/*
			Game.ServiceProvider.RequestPromise(new BankItemBuyOperation(bankItem))
				.Then(Game.ServerDataUpdater.Update);
				*/
		}

		public static void MakeBuyButton(ButtonText btn, Image image, UserBankItem bankItem, Action onClick)
		{
			MakeBuyButton(btn, btn.TextField, image, bankItem, onClick);
		}

		public static void MakeBuyButton(ButtonTextIcon btn, UserBankItem bankItem, Action onClick)
		{
			MakeBuyButton(btn, btn.TextField, btn.Icon, bankItem, onClick);
		}

		public static void MakeBuyButton(BasicButton btn, TextMeshProUGUI textField, Image image, UserBankItem bankItem, Action onClick)
		{
			if (btn == null || bankItem == null)
				return;

			btn.SetOnDownCallback(null);
			btn.onClick.RemoveAllListeners();

			if (UserBankItem.CurrencyIsText)
			{
				if (image != null)
					image.SetActive(false);

				textField.text = bankItem.Label;
			}
			else
			{
				if (image != null)
				{
					image.SetActive(true);
					UserBankItem.GetIcon(image);
				}

				textField.text = bankItem.SnMoney.ToString();
			}

			btn.onClick.AddListener(() => onClick?.Invoke());
		}
#endif
		public IPromise ExecSuccessCallback(string productId, List<ItemCount> drop)
		{
			return Promise.Resolved();
			/*
			MetaPayment metaPayment = null;
			return GetMetaPayment(productId)
				.Then(pp =>
				{
					metaPayment = pp;
					return ConfirmMetaPayment(productId, metaPayment, drop);
				})
				.Then(() => RemoveMetaPayment(productId))
				.Then(() =>
				{
					var bankItem = metaPayment?.BankItem;
					if (bankItem != null)
					{
						OnBuySuccess?.Invoke(bankItem);
						SaveBankItemData(bankItem);
					}
				})
				.Catch(Debug.LogError);
			*/
		}

		public IPromise ExecErrorCallback(string productId, Exception e)
		{
			return Promise.Resolved();
			// var isNoInternetException = e is NoInternetException;
			//
			// return GetMetaPayment(productId)
			// 	.Then(CancelMetaPayment)
			// 	.Finally(() =>
			// 	   {
			// 		   if (isNoInternetException) // Если ошибка интернета, то удалять MetaPayment не нужно
			// 			   return Promise.Resolved();
			//
			// 		   return RemoveMetaPayment(productId);
			// 	   });
		}
		//
		// private IPromise AddMetaPayment(MetaPayment metaPayment)
		// {
		// 	return Game.ServiceProvider
		// 		.RequestPromise(new AddMetaPaymentOperation(metaPayment))
		// 		.Then(null);
		// }
		//
		// private IPromise<MetaPayment> GetMetaPayment(string productId)
		// {
		// 	return Game.ServiceProvider
		// 		.RequestPromise(new GetMetaPaymentOperation(productId))
		// 		.Then(response => Promise<MetaPayment>.Resolved(response.MetaPayment));
		// }
		//
		// private IPromise RemoveMetaPayment(string productId)
		// {
		// 	return Promise.Resolved();
		// 	return Game.ServiceProvider
		// 			   .RequestPromise(new DeleteMetaPaymentOperation(productId))
		// 			   .Then(null);
		// }

		private IPromise ConfirmMetaPayment(string productId, MetaPayment metaPayment, List<ItemCount> drop)
		{
			if (metaPayment == null)
				return Promise.Rejected(new PurchaseTransactionException(productId, PurchaseTransactionException.Reason.metaPaymentIsEmpty));
			return metaPayment.OnConfirm(drop);
		}

		private IPromise CancelMetaPayment(MetaPayment metaPayment)
		{
			return metaPayment?.OnCancel() ?? Promise.Resolved();
		}
	}
}