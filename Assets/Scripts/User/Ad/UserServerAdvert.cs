using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.User.Ad
{
    public class UserServerAdvert
    {
        [JsonProperty("id")]
        public int Id { get; private set; }
        
        [JsonProperty("aid")]
        public int OfferId { get; private set; }
        
        public IPromise Show()
        {
            var point = Game.User.Ads.GetUserAdPoint(Id);
            if (point != null)
            {
                if (point.IsInterstitial)
                    return ShowInterstitial();
                if (point.IsOfferwall)
                    return ShowOfferwall();
                if (point.IsReward)
                    return ShowReward();
            }
            
            return Promise.Resolved();
        }

		public IPromise ShowInterstitial()
		{
			var promise = new Promise();

			var partner = Game.User.Ads.GetUserAdPoint(Id)?.GetAvailableInterstitialPartner()?.Partner;
			if (partner != null && partner.IsInterstitialLoadedReactive.Value)
				partner.ShowInterstitial(Id, _ => afterShow(), afterShow);
			else
				promise.ResolveOnce();

			return promise;

			void afterShow()
			{
				promise.ResolveOnce();
				Game.AdvertisingController.GetStandardReward(Id, null, false);
			}
		}

		public IPromise ShowOfferwall()
        {
            var promise = new Promise();
            var partner = Game.User.Ads.GetUserAdPoint(Id)?.GetAvailableOfferwallPartner()?.Partner;
            
            if(partner != null && partner.IsOfferwallLoadedReactive.Value)
                partner.ShowOfferwallAd(OnShowComplete, OnAdvertClose);
            else
                OnAdvertClose();

            return promise;

            void OnShowComplete(Dictionary<string, object> extraData)
            {
                Game.AdvertisingController.GetStandardReward(Id, null, false, extraData);
                promise.ResolveOnce();
            }
            
            void OnAdvertClose() => promise.ResolveOnce();
        }
        
        public IPromise ShowReward()
        {
            var promise = new Promise();
            var partner = Game.User.Ads.GetUserAdPoint(Id)?.GetAvailableRewardPartner()?.Partner;
            
            if(partner != null && partner.IsRewardLoadedReactive.Value)
                partner.ShowRewardAd(Id, OnShowComplete, OnAdvertClose);
            else
                OnAdvertClose();

            return promise;

            void OnShowComplete(Dictionary<string, object> adParams)
            {
                Game.AdvertisingController.GetStandardReward(Id, null, false, extraData: adParams);
                promise.ResolveOnce();
            }

            void OnAdvertClose() => promise.ResolveOnce();
        }
    }
}