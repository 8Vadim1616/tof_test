using Assets.Scripts.Events;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.User.Ad.Points;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.User.Ad
{
	public class UserAds
	{
		private Dictionary<int, UserAdPoint> All = new Dictionary<int, UserAdPoint>();

		public UserAdPoint GetUserAdPoint(UserAdType point) => GetUserAdPoint((int) point);
		public UserAdPoint GetUserAdPoint(int point) { return All.ContainsKey(point) ? All[point] : null; }

		public int AdvertisingLevel { get; set; }

		public Dictionary<string, Dictionary<string, string>> PartnersSettings { get; private set; }

		public void UpdateSettings(Dictionary<string, Dictionary<string, string>> data)
		{
			if (data == null) return;

			PartnersSettings = data;

			if (Game.AdvertisingController != null)
				foreach (var partner in PartnersSettings.Keys)
					Game.AdvertisingController.UpdatePartner(partner, PartnersSettings[partner]);
		}

		public void Update(Dictionary<int, ServerAd> data, bool byServer)
		{
			ServerAd serverAd;
			UserAdPoint userAdPoint;

			if (data == null) return;

			foreach (var keyValuePair in data)
			{
				serverAd = keyValuePair.Value;

				userAdPoint = GetUserAdPoint((UserAdType) keyValuePair.Key);

				if (userAdPoint == null)
				{
					userAdPoint = new UserAdPoint(keyValuePair.Key, serverAd);
				}

				userAdPoint.Update(serverAd);

				All[keyValuePair.Key] = userAdPoint;
			}

			EventController.TriggerEvent(new GameEvents.UserAdsUpdated());
			Debug.Log("UserAds updated");
		}

		private List<UserServerAdvert> serverAdQueue = new List<UserServerAdvert>();

		public void OnServerAd(UserServerAdvert userServerAdvert)
		{
			if (userServerAdvert == null) return;

			serverAdQueue.Add(userServerAdvert);

			if (Game.Instance.IsLoaded.Value)
				CheckInterstitial();
		}

		public Promise CheckInterstitial()
		{
			var promise = new Promise();
			process();
			return promise;

			void process()
			{
				if (serverAdQueue.Count > 0)
				{
					var userServerInterstitial = serverAdQueue[0];
					serverAdQueue.RemoveAt(0);
					userServerInterstitial.Show()
						.Then(process);
				}
				else
				{
					promise.Resolve();
				}
			}
		}
		
		public bool IsAppOpenAdAvailable(int point)
		{
			if (Game.User == null)
				return false;

			if (!IsAdAvailable())
				return false;

			if (IsServerUniversalPoint(point)) // Если пришло с сервера то полюбому показываем
				return true;

			var userAdPoint = GetUserAdPoint(point);

			if (userAdPoint != null)
				return userAdPoint.IsAvailableAppOpen();

			return false;
		}

		public bool IsRewardAdAvailable(int point, bool checkLoaded)
		{
			if (Game.User == null)
				return false;

			if (!IsAdAvailable())
				return false;

			if (IsServerUniversalPoint(point)) // Если пришло с сервера то полюбому показываем
				return true;

			var userAdPoint = GetUserAdPoint(point);

			if (userAdPoint != null)
				return checkLoaded ? userAdPoint.IsLoadedReward() : userAdPoint.IsAvailableReward();

			return false;
		}

		public bool IsRewardedInterstitialAdAvailable(int point, bool checkLoaded)
		{
			if (Game.User == null)
				return false;

			if (!IsAdAvailable())
				return false;

			if (IsServerUniversalPoint(point)) // Если пришло с сервера то полюбому показываем
				return true;

			var userAdPoint = GetUserAdPoint(point);

			if (userAdPoint != null)
				return checkLoaded ? userAdPoint.IsLoadedRewardedInterstitial() : userAdPoint.IsAvailableRewardedInterstitial();

			return false;
		}


		public bool IsInterstitialAdAvailable(int point)
		{
			if (Game.User == null)
				return false;

			if (IsServerUniversalPoint(point))  // Интерстишл реклама с сервера всегда доступна
				return true;

			var userAdPoint = GetUserAdPoint(point);

			if (userAdPoint is null)
				return false;

			return userAdPoint.IsAvailableInterstitial();
		}

		public static bool IsAdAvailable()
		{
			// return Game.User.Level.Id >= CinemaLevel;
			return true;
		}

		public static int CinemaLevel => Game.User?.Ads?.AdvertisingLevel > 0 ? Game.User.Ads.AdvertisingLevel : 0;/*Game.Settings.LEVEL_NEED_CINEMA;*/

		public static bool IsServerUniversalPoint(int pointType)
		{
			return pointType == (int) UserAdType.AD_UNIVERSAL_SERVER_POINT;
		}
	}
}