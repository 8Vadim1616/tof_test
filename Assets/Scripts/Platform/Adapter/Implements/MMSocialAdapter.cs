using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Utils;
using ExternalScripts;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class MMSocialAdapter : AbstractSocialAdapter
	{
		protected override string TAG => "[MMSocialAdapter] ";

		protected Promise<object> _invitePromise = null;
		protected Promise<object> _publishPromise = null;

		public MMSocialAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.VKONTAKTE, parameters)
		{
			_isLoggedIn.Value = true;

			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.AddCallback("apiCallback", ApiCallback);
				ExternalInterface.AddCallback("onPayment", OnPayment);
				ExternalInterface.AddCallback("onRequest", OnRequest);
				ExternalInterface.AddCallback("onPublish", OnPublish);
				ExternalInterface.AddCallback("onInvite", OnInvite);
			}

			SocialActionHandler[] handlers =
			{
							new SocialActionHandler(typeof(SocialRequest), SocialRequest),
							new SocialActionHandler(typeof(Payment), Payment),
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
			var data = jsonObject["data"].ToObject<JToken>();

			if (data == null)
				Log("apiCallback " + id + " is null");

			if (_actions.Count > id && _actions[id] != null)
			{
				var promise = _actions[id];

				if (data == null)
				{
					promise.Reject(new Exception("data is null"));
				}
				else
				{
					promise.Resolve(data);
				}

				_actions[id] = null;
			}

			return null;
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			JObject jsonObject = new JObject();

			Api("common.friends.getExtended", jsonObject)
						   .Then(data =>
							{
								JArray newJsonArray = (JArray) data;

								var list = new List<SocialProfile>();

								foreach (var token in newJsonArray)
									list.Add(CreateProfile(token));

								result.Resolve(list);
							})
						   .Done();

			return result;
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			Api("common.friends.getAppUsers", true)
						   .Then(data =>
							{
								if (data == null)
								{
									Log("common.friends.getAppUsers response is null");
									return;
								}

								JArray newJsonArray = (JArray) data;

								var list = new List<SocialProfile>();

								foreach (var token in newJsonArray)
									list.Add(CreateProfile(token));

								result.Resolve(list);
							})
						   .Done();

			return result;
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			Api("common.users.getInfo", Uid)
				.Then(data =>
				 {
					 var newJsonArray = (JArray) data;
					 var userProfile = CreateProfile(newJsonArray.First);
					 result.Resolve(userProfile);
				 })
				.Done();

			return result;
		}

		private Promise<object> Payment(ISocialAction action)
		{
			var payment = (Payment) action;
			var promise = new Promise<object>();

			_paymentPromise = promise;

			var toSend = new JObject();
			toSend["service_id"] = payment.Pid;
			toSend["service_name"] = payment.Pname;
			toSend["mailiki_price"] = payment.Amt;

			Api("app.payments.showDialog", toSend).Done();

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

		private object OnInvite(object data)
		{
			if (_invitePromise != null)
			{
				_invitePromise.Resolve(data);
				_invitePromise = null;
			}

			return null;
		}

		private object OnPublish(object data)
		{
			if (_publishPromise != null)
			{
				_publishPromise.Resolve(data);
				_publishPromise = null;
			}

			return null;
		}

		public override Promise<object> Invite(string message = "", JObject parameters = null)
		{
			Api("app.friends.invite", new JObject()).Done();

			var promise = new Promise<object>();
			promise.Resolve(new[] {""});

			return promise;
		}

		// override protected IPromise<object> WallPost(ISocialAction action)
		// {
		// var wallPost = (WallPost) action;
		// var deferred = new Deferred();
		//
		// publishPromise = new Deferred();
		//
		// string link = "";
		//
		// 				if (wallPost.parameters != null)
		// {
		// 	foreach (string key in wallPost.parameters.Keys)
		// 	{
		// 		link += "&" + key + "=" + wallPost.parameters[key];
		// 	}
		//
		// 	link = link.Substring(0, link.Length - 1);
		// }
		//
		// log.info("post image = " + wallPost.image);
		//
		// JSONObject post = new JSONObject();
		// post["title"] = wallPost.title;
		// post["text"] = wallPost.body;
		// post["img_url"] = wallPost.image;
		//
		// if (wallPost.uid != null && wallPost.uid != uid)
		// {
		// 	post["uid"] = wallPost.uid;
		// }
		//
		// if (wallPost.action != null)
		// {
		// 	var actionLinks = new JSONArray();
		//
		// 	var obj = new JSONObject();
		// 	obj["text"] = wallPost.action;
		// 	obj["href"] = link;
		//
		// 	actionLinks.Add(obj);
		//
		// 	post["action_links"] = actionLinks;
		// }
		//
		// api(wallPost.uid != null ? "common.guestbook.post" : "common.stream.post", post).Done();
		//
		// 				return deferred.promise;
		// }

		private Promise<object> SocialRequest(ISocialAction action)
		{
			var socialRequest = (SocialRequest) action;
			var promise = new Promise<object>();

			_requestPromise = promise;

			var uids = new JArray();
			uids.Add(socialRequest.Uid);

			JObject req = new JObject();
			req["friends"] = uids;
			req["text"] = socialRequest.Message;
			req["img_url"] = socialRequest.Url;

			Api("app.friends.request", req).Done();

			return promise;
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

			if (data["uid"] != null) result.Uid = (string) data["uid"];
			if (data["first_name"] != null) result.FirstName = (string) data["first_name"];
			if (data["last_name"] != null) result.LastName = (string) data["last_name"];
			if (data["pic"] != null) result.Avatar = (string) data["pic"];

			return result;
		}

		public override string Uid => IframeVars["vid"] != null ? (string)IframeVars["vid"] : "";
		public override string CommunityURL => IframeVars["commun"] != null ? "http://my.mail.ru/community/" + (string)IframeVars["commun"] : "";
		public override bool AppUser => (string)IframeVars["is_app_user"] == "1";
		public override string FlashVarsReferrer => IframeVars["referer_id"] != null ? (string)IframeVars["referer_id"] : "";
	}
}