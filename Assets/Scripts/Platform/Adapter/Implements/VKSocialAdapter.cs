using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Platform.Adapter.Data;
using Assets.Scripts.Utils;
using ExternalScripts;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class VKSocialAdapter : AbstractSocialAdapter
	{
		protected override string TAG => "[VKSocialAdapter] ";

		private string _fields = "uid,first_name,last_name,sex,photo_medium_rec,photo_rec,photo_big,bdate";

		public VKSocialAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.VKONTAKTE, parameters)
		{
			_isLoggedIn.Value = true;

			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.AddCallback("apiCallback", ApiCallback);
				ExternalInterface.AddCallback("onPayment", OnPayment);
				ExternalInterface.AddCallback("onRequest", OnRequest);
				ExternalInterface.AddCallback("onSubscription", OnSubscription);
				ExternalInterface.AddCallback("onMidrollReadyState", OnMidrollReadyState);
				ExternalInterface.AddCallback("onMidrollCompleteState", OnMidrollCompleteState);
				ExternalInterface.AddCallback("onPrerollCompleteState", OnPrerollCompleteState);
			}

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

		private Promise<object> Api(string method, object obj)
		{
			var promise = new Promise<object>();

			_actions.Add(promise);

			if (ExternalInterface.IsAvailable)
				ExternalInterface.CallFromIframe("api", method, obj.ToString(), _actions.Count - 1);

			return promise;
		}

		private object ApiCallback(object obj)
		{
			Log("apiCallback " + obj);

			var jsonObject = obj as JObject;
			int id = jsonObject["id"].ToObject<int>();
			var data = jsonObject["data"].ToObject<JObject>();

			if (data == null)
				Log("apiCallback " + id + " is null");

			if (_actions.Count > id && _actions[id] != null)
			{
				var promise = _actions[id];

				if (data == null)
				{
					promise.Reject(new Exception("data is null"));
				}
				else if (data["response"] == null)
				{
					promise.Reject(new Exception("response is null"));
				}
				else
				{
					promise.Resolve(data["response"]);
				}

				_actions[id] = null;
			}

			return null;
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			if (_friendList != null)
			{
				get();
			}
			else
			{
				var jsonObject = new JObject();
				jsonObject["user_id"] = Uid;

				Api("friends.get", jsonObject)
					.Then(data =>
					 {
						 var newJsonObject = (JObject) data;

						 string[] newData = null;

						 if (newJsonObject["items"] != null)
							 newData = newJsonObject["items"].ToObject<string[]>();

						 _friendList = newData != null ? newData : new string[] { };
						 get();
					 })
					.Done();
			}

			void get()
			{
				string[] uids = MaxCountFriendsFromNetwork > 0 ? _friendList.Skip(MaxCountFriendsFromNetwork * curPage).Take(MaxCountFriendsFromNetwork).ToArray() : _friendList;
				if (_friendList.Length == 0 && curPage == 0)
				{
					result.Resolve(new List<SocialProfile>());
				}
				else if (uids.Length > 0)
				{
					var jsonObject = new JObject();
					jsonObject["user_ids"] = string.Join(",", uids);
					jsonObject["fields"] = _fields;

					Api("getProfiles", jsonObject)
								   .Then(data =>
									{
										if (data == null)
										{
											Log("getProfiles response is null");
											return;
										}

										var newJsonArray = (JArray) data;

										var list = new List<SocialProfile>();

										foreach (var token in newJsonArray)
											list.Add(CreateProfile(token));

										result.Resolve(list);
									})
								   .Done();
				}
				else
				{
					result.Reject(new Exception("Wrong page"));
				}
			}

			return result;
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			var curPage = 0;

			if (_appFriendList != null)
			{
				get();
			}
			else
			{
				var jsonObject = new JObject();
				jsonObject["user_id"] = Uid;

				Api("friends.getAppUsers", jsonObject)
					.Then(data =>
					 {
						 if (data == null)
						 {
							 Log("friends.getAppUsers response is null");
							 return;
						 }

						 var newJsonObject = (JArray) data;
						 string[] newData = newJsonObject.ToObject<string[]>();

						 _appFriendList = newData != null && newData is string[] ? newData : new string[] { };
						 get();
					 })
					.Done();
			}

			void get()
			{
				string[] uids = MaxCountFriendsFromNetwork > 0 ? _appFriendList.Skip(MaxCountFriendsFromNetwork * curPage).Take(MaxCountFriendsFromNetwork).ToArray() : _appFriendList;
				if (_appFriendList.Length == 0 && curPage == 0)
				{
					result.Resolve(new List<SocialProfile>());
				}
				else if (uids.Length > 0)
				{
					var jsonObject = new JObject();
					jsonObject["user_ids"] = string.Join(",", uids);
					jsonObject["fields"] = _fields;

					Api("getProfiles", jsonObject)
								   .Then(data =>
									{
										if (data == null)
										{
											Log("getProfiles response is null");
											return;
										}

										var newJsonArray = (JArray) data;

										var list = new List<SocialProfile>();

										foreach (var token in newJsonArray)
											list.Add(CreateProfile(token));

										result.Resolve(list);
									})
								   .Done();
				}
				else
				{
					result.Reject(new Exception("Wrong page"));
				}
			}

			return result;
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			var jsonObject = new JObject();
			jsonObject["user_ids"] = Uid;
			jsonObject["fields"] = _fields;

			Api("getProfiles", jsonObject)
				.Then(data =>
				 {
					 var newJsonArray = (JArray) data;
					 var userProfile = CreateProfile(newJsonArray.First);
					 result.Resolve(userProfile);
				 })
				.Done();

			return result;
		}

		public override TransactionQueueData ParseTransactionQueue(JToken data) => new TransactionQueueData(data.Value<int>("item_id"), data.Value<string>("sig"));

		private Promise<object> Payment(ISocialAction action)
		{
			var payment = (Payment) action;
			var promise = new Promise<object>();

			_paymentPromise = promise;

			// ExternalInterface.CallFromIframe("hideGame");
			ExternalInterface.CallFromIframe("payment", payment.Pid, payment.Amt, payment.Pname);

			return promise;
		}

		private Promise<object> Subscription(ISocialAction action)
		{
			var subscription = (Subscription) action;
			var promise = new Promise<object>();

			_subscriptionPromise = promise;

			// ExternalInterface.CallFromIframe("hideGame");
			ExternalInterface.CallFromIframe("subscription", subscription.Pid, subscription.Amt, subscription.Pname);

			return promise;
		}

		private object OnPayment(object success)
		{
			if (_paymentPromise != null)
			{
				_paymentPromise.Resolve(success);
				_paymentPromise = null;
			}

			return null;
		}

		private object OnRequest(object success)
		{
			if (_requestPromise != null)
			{
				_requestPromise.Resolve(success);
				_requestPromise = null;
			}

			return null;
		}

		private object OnSubscription(object success)
		{
			if (_subscriptionPromise != null)
			{
				_subscriptionPromise.Resolve(success);
				_subscriptionPromise = null;
			}

			return null;
		}

		public override Promise<object> Invite(string message = "", JObject parameters = null)
		{

			if (ExternalInterface.IsAvailable)
			{
				// ExternalInterface.CallFromIframe("hideGame");
				ExternalInterface.CallFromIframe("VK.callMethod", "showInviteBox");
			}

			var promise = new Promise<object>();
			promise.Resolve(new[] {""});

			return promise;
		}

		// override protected IPromise<object> WallPost(ISocialAction action)
		// {
		// 	var wallPost = (WallPost) action;
		// 	var deferred = new Deferred();
		//
		// 	var args = new JObject();
		// 	args["owner_id"] = wallPost.uid;
		// 	args["message"] = wallPost.body;
		// 	args["attachments"] = "photo" + IframeVars["viral_id"] + '_' + wallPost.image;
		// 	string url = "http://vk.com/app" + IframeVars["api_id"];
		//
		// 	if (wallPost.parameters != null)
		// 	{
		// 		string post_id = "";
		//
		// 		foreach (string key in wallPost.parameters.Keys)
		// 		{
		// 			post_id = post_id + key + "=" + wallPost.parameters[key] + "_";
		// 		}
		//
		// 		url += "#"             + post_id.Substring(0, post_id.Length - 1);
		// 		args["message"] += " " + url;
		// 	}
		//
		// 	args["attachments"] += "," + url;
		//
		// 	ExternalInterface.CallFromIframe("hideFlash");
		// 	Api("wall.post", args)
		// 	    .Then(data =>
		// 	    {
		// 		    var newJsonObject = (JObject) data;
		//
		// 		    deferred.Resolve(newJsonObject != null && newJsonObject["post_id"] != null);
		// 	    })
		// 	    .Done();
		//
		// 	return deferred.promise;
		// }

		private Promise<object> SocialRequest(ISocialAction action)
		{
			var socialRequest = (SocialRequest) action;
			var promise = new Promise<object>();

			_requestPromise = promise;

			// ExternalInterface.CallFromIframe("hideGame");
			ExternalInterface.CallFromIframe("VK.callMethod", "showRequestBox", socialRequest.Uid, socialRequest.Message, socialRequest.RequestKey);

			return promise;
		}

		private Promise<object> ShowAdvertising(ISocialAction action)
		{
			Log("Adv Call");

			var advertising = (Advertising) action;
			var promise = new Promise<object>();

			if (advertising.State == Advertising.PREPARE)
				_prepareAdList.Add(promise);
			else if (advertising.State == Advertising.SHOW)
				_completeAdPromise = promise;

			ExternalInterface.CallFromIframe("invokeAd", advertising.State);

			return promise;
		}

		private Promise<object> AdvertisingPreroll(ISocialAction action)
		{
			Log("Preroll Call");

			var promise = new Promise<object>();

			_prerollPromise = promise;

			ExternalInterface.CallFromIframe("invokePreroll");

			return _prerollPromise;
		}

		private object OnMidrollReadyState(object response /*available : Boolean, data : Object*/)
		{
			JObject obj = response as JObject;
			bool available = obj["available"].ToObject<bool>();
			string data = obj["data"].ToString();

			Log("Adv is " + available + " with data: " + data);

			while (_prepareAdList.Count > 0)
			{
				_prepareAdList.Shift().Resolve(available);
			}

			return null;
		}

		private object OnMidrollCompleteState(object response /*rewarded : Boolean, data : Object*/)
		{
			JObject obj = response as JObject;
			bool rewarded = obj["rewarded"].ToObject<bool>();
			string data = (string)obj["data"];

			Log("Adv complete " + rewarded + " with data: " + data);

			if (_completeAdPromise != null)
			{
				if (rewarded)
					_completeAdPromise.Resolve(rewarded);
				else
					_completeAdPromise.Reject(new Exception("Adv error: " + data));
				_completeAdPromise = null;
			}

			return null;
		}

		private object OnPrerollCompleteState(object response /*completed : Boolean, data : Object*/)
		{
			JObject obj = response as JObject;
			bool completed = obj["completed"].ToObject<bool>();
			string data = (string)obj["data"];

			Log("Preroll complete " + completed + " with data: " + data);

			if (_prerollPromise != null)
			{
				if (completed)
					_prerollPromise.Resolve(completed);
				else
					_prerollPromise.Reject(new Exception("Preroll Error: " + data));

				_prerollPromise = null;
			}

			return null;
		}

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

		private SocialProfile CreateProfile(JToken data)
		{
			if (data == null) return null;

			var result = new SocialProfile();

			string photoRec = (string)data["photo_rec"];
			if (photoRec != null && (photoRec.IndexOf("camera_") != -1 || (photoRec.IndexOf("deactivated_") != -1) && IframeVars["default_photo_50"] != null))
			{
				data["photo_rec"] = IframeVars["default_photo_50"];
				data["photo_medium_rec"] = IframeVars["default_photo_100"];
				data["photo_big"] = IframeVars["default_photo_200"];
			}

			if (data["id"] != null) result.Uid = (string) data["id"];
			if (data["first_name"] != null) result.FirstName = (string) data["first_name"];
			if (data["last_name"] != null) result.LastName = (string) data["last_name"];
			if (data["photo_medium_rec"] != null) result.Avatar = (string) data["photo_medium_rec"];

			return result;
		}

		public override string Uid => IframeVars["viewer_id"] != null ? (string)IframeVars["viewer_id"] : "";
		public override string CommunityURL => IframeVars["commun"] != null ? "http://vk.com/" + (string)IframeVars["commun"] : "";
		public override bool AppUser => (string)IframeVars["is_app_user"] == "1";
		public override string FlashVarsReferrer => IframeVars["request_key"] != null ? (string)IframeVars["request_key"] : "";
	}
}