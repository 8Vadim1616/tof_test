using System;
using Assets.Scripts;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.User;
using Assets.Scripts.Utils;

namespace DefaultNamespace
{
	public class UserShop
	{
		private UserData _user;

		public UserShop(UserData user)
		{
			_user = user;
		}

		/*
		public IPromise BuyItem(ShopItem shopItem, bool checkEnoughPrice = true)
		{
			if (checkEnoughPrice)
			{
				if (shopItem.Price != null && !Game.Checks.EnoughItems(shopItem.Price) ||
					shopItem.Time > 0 && Game.User.Bank.GetTimeLeftForFreeShopItem(shopItem) > 0)
					return Promise.Rejected(null);
			}

			return Game.ServiceProvider.RequestPromise(new ItemBuyOperation(shopItem))
					   .Then(response =>
						{
							Game.Sound.PlayPurchase();
							Game.ServerDataUpdater.Update(response);
							return Promise.Resolved();
						});
		}

		public IPromise<BaseApiResponse> BuyItemWithParams(ShopItem shopItem, int count = 1, bool checkEnoughPrice = true,
			bool drop = false, bool needSound = true)
		{
			if (checkEnoughPrice && !Game.Checks.EnoughItems(shopItem.Price))
				return Promise<BaseApiResponse>.Rejected(null);

			return Game.ServiceProvider.RequestPromise(new ItemBuyOperation(shopItem, buyCount: count, needDrop: drop))
				.Then(response =>
				{
					if (needSound)
						Game.Sound.PlayPurchase();
					Game.ServerDataUpdater.Update(response);

					return Promise<BaseApiResponse>.Resolved(response);
				});
		}

		public IPromise BuyItem(ShopItem shopItem, AdOptions adOptions)
		{
			return Game.ServiceProvider.RequestPromise(new ItemBuyOperation(shopItem, adOptions))
				.Then(response =>
				{
					Game.Sound.PlayPurchase();
					Game.ServerDataUpdater.Update(response);
					return Promise.Resolved();
				});
		}
		*/
	}
}