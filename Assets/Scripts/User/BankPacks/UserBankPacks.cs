using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Utils;
using UniRx;

namespace Assets.Scripts.User.BankPacks
{
    public class UserBankPacks
    {
		public event Action OnUserBankPacksUpdatedEvent;

        public List<UserBankPackItem> All { get; private set; } = new List<UserBankPackItem>();

		public bool Inited { get; private set; }

		public UserBankPacks()
		{
		}

        public Promise<List<ItemCount>> BuyBankPack(UserBankPackItem pack, AdOptions adOptions = null)
        {
            var result = new Promise<List<ItemCount>>();

			// var operation = new BankPackBuyOperation(pack.Id, adOptions);
   //
   //          Game.Network.LongReactivePropertyger.RequestPromise(operation)
   //              .Then(resp =>
   //              {
   //                  Game.ServerDataUpdater.Update(resp);
   //                  result.Resolve(resp.GetDrop());
   //              });

            return result;
        }
        
        public UserBankPackItem GetById(int id)
        {
            return All.FirstOrDefault(userBankPackItem =>
                userBankPackItem.Id == id);
        }
/*
        
        public bool NeedAlarm(bool isForReal)
        {
            var dict = isForReal ? NormalBankPacks : MoneyPacks;

            return dict.Any(userBankPackItem => userBankPackItem.Updated);
        }

		public bool NeedAdAlarm(bool isForReal)
		{
			var dict = isForReal ? NormalBankPacks : MoneyPacks;

			foreach (var userBankPackItem in dict)
			{
				if (userBankPackItem.IsAd)
				{
					if (Game.AdvertisingController.IsBankPackAdvAvailable(userBankPackItem.AdvertId))
						return true;
				}
			}

			return false;
		}
*/
        public void Free()
        {
            All.Clear();
		}
    }
}