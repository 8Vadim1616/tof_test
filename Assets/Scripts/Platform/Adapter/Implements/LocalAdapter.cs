using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class LocalAdapter : AbstractSocialAdapter
	{
		protected override string TAG => "[LocalAdapter] ";

		private SocialProfile _localProfile = null;

		public LocalAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.LOCAL, parameters)
		{
			_isLoggedIn.Value = true;

			SocialActionHandler[] handlers =
			{
							new SocialActionHandler(typeof(SocialRequest), SocialRequest),
							new SocialActionHandler(typeof(Payment), Payment),
							new SocialActionHandler(typeof(Subscription), Subscription),
							new SocialActionHandler(typeof(Advertising), ShowAdvertising),
							new SocialActionHandler(typeof(AdvertisingPreroll), AdvertisingPreroll)
			};

			AddActionsHandlers(handlers);

			AfterInit();
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			result.Resolve(new List<SocialProfile>());

			return result;
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			result.Resolve(new List<SocialProfile>());

			return result;
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			if (_localProfile == null)
			{
				_localProfile = new SocialProfile();
				_localProfile.FirstName = Uid;
				_localProfile.LastName = Uid;
			}

			result.Resolve(_localProfile);

			return result;
		}

		private Promise<object> Payment(ISocialAction action)
		{
			var promise = new Promise<object>();

			promise.Resolve(null);

			return promise;
		}

		private Promise<object> Subscription(ISocialAction action)
		{
			var promise = new Promise<object>();

			promise.Resolve(null);

			return promise;
		}

		public override Promise<object> Invite(string message = "", JObject parameters = null)
		{
			var promise = new Promise<object>();

			promise.Resolve(new[] {""});

			return promise;
		}

		private Promise<object> SocialRequest(ISocialAction action)
		{
			var promise = new Promise<object>();

			promise.Resolve(true);

			return promise;
		}

		private Promise<object> ShowAdvertising(ISocialAction action)
		{
			var promise = new Promise<object>();

			// promise.Resolve(true);
			promise.Resolve(false);

			return promise;
		}

		private Promise<object> AdvertisingPreroll(ISocialAction action)
		{
			var promise = new Promise<object>();

			promise.Resolve(true);

			return promise;
		}

		public override string Uid => IframeVars["viewer_id"] != null ? (string)IframeVars["viewer_id"] : "";
		public override string CommunityURL => IframeVars["commun"] != null ? "http://vk.com/" + (string)IframeVars["commun"] : "";
		public override bool AppUser => (string)IframeVars["is_app_user"] == "1";
		public override string FlashVarsReferrer => IframeVars["request_key"] != null ? (string)IframeVars["request_key"] : "";

		public override JObject WallParams
		{
			get
			{
				Log("flashVars.wp = " + IframeVars["wp"]);

				if (IframeVars["wp"] != null)
				{
					try
					{
						var result = new JObject();
						string post = IframeVars["wp"].ToObject<string>();
						string[] temp = post.Split('_');

						foreach (string a in temp)
						{
							string[] temp2 = a.Split('=');
							result[temp2[0]] = temp2[1];
						}

						return result;
					}
					catch (Exception e)
					{
						return new JObject();
					}
				}

				return new JObject();
			}
		}
	}
}