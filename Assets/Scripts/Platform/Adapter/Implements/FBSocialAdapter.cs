using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using ExternalScripts;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class FBSocialAdapter : AbstractSocialAdapter
	{
		protected override string TAG => "[FBSocialAdapter] ";

		private string _fields = "id,first_name,last_name,gender,picture.width(128).height(128),email,link,birthday,third_party_id,currency";

		public FBSocialAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.VKONTAKTE, parameters)
		{
			_isLoggedIn.Value = true;

			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.AddCallback("apiCallback", ApiCallback);
				ExternalInterface.AddCallback("onPayment", OnPayment);
				ExternalInterface.AddCallback("onSubscription", OnSubscription);
			}

			SocialActionHandler[] handlers =
			{
							new SocialActionHandler(typeof(SocialRequest), SocialRequest),
							new SocialActionHandler(typeof(Payment), Payment),
							new SocialActionHandler(typeof(Subscription), Subscription),
							new SocialActionHandler(typeof(FBViralGraph), Opengraph)
			};

			AddActionsHandlers(handlers);

			AfterInit();
		}

		private Promise<object> Api(string method)
		{
			var promise = new Promise<object>();

			_actions.Add(promise);

			if (ExternalInterface.IsAvailable)
				ExternalInterface.CallFromIframe("api", method, _actions.Count - 1);

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
				else
				{
					promise.Resolve(data);
				}

				_actions[id] = null;
			}

			return null;
		}

		private Promise<object> Ui(object ob)
		{
			var promise = new Promise<object>();

			Log("Ui call: " + ob);

			_actions.Add(promise);

			if (ExternalInterface.IsAvailable)
				ExternalInterface.CallFromIframe("ui", ob, _actions.Count - 1);

			return promise;
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			Api("me/invitable_friends?limit=10000")
						   .Then(data =>
							{
								var newJsonObject = (JObject) data;

								var list = new List<SocialProfile>();

								if (newJsonObject["data"] != null)
								{
									var newJsonArray = (JArray) newJsonObject["data"];
									foreach (var token in newJsonArray)
										list.Add(CreateProfile(token));
								}

								result.Resolve(list);
							})
						   .Done();

			return result;
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			Api("me/friends?limit=10000&fields=" + _fields)
							   .Then(data =>
								{
									if (data == null)
									{
										Log("me/friends response is null");
										return;
									}

									var newJsonObject = (JObject) data;

									var list = new List<SocialProfile>();

									if (newJsonObject["data"] != null)
									{
										var newJsonArray = (JArray) newJsonObject["data"];
										foreach (var token in newJsonArray)
											list.Add(CreateProfile(token));
									}

									result.Resolve(list);
								})
							   .Done();

			return result;
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			Api("me?fields=" + _fields)
						   .Then(data =>
							{
								var newJsonObject = (JObject) data;
								var userProfile = CreateProfile(newJsonObject);
								result.Resolve(userProfile);
							})
						   .Done();

			return result;
		}

		private Promise<object> Opengraph(ISocialAction action)
		{
			FBViralGraph fbViralGraph = (FBViralGraph) action;

			string url = IframeVars["ogurl"] + "?";
			url += "obj="     + fbViralGraph.Obj;
			url += "&id="     + fbViralGraph.Id;
			url += "&action=" + fbViralGraph.Action;

			if (fbViralGraph.ExtraParams != null)
			{
				foreach (string paramName in fbViralGraph.ExtraParams.Properties())
				{
					url += "&" + paramName + "=" + fbViralGraph.ExtraParams[paramName];
				}
			}

			ExternalInterface.CallFromIframe("opengraph", fbViralGraph.Action, fbViralGraph.Obj, url, fbViralGraph.Mess);

			return Promise<object>.Resolved(null) as Promise<object>;
		}

		private Promise<object> Payment(ISocialAction action)
		{
			var payment = (Payment) action;
			var promise = new Promise<object>();

			_paymentPromise = promise;

			ExternalInterface.CallFromIframe("payment", payment.Pid, payment.Amt, payment.Pname, payment.Xbank, payment.Pdesc);

			return promise;
		}

		private Promise<object> Subscription(ISocialAction action)
		{
			var subscription = (Subscription) action;
			var promise = new Promise<object>();

			_subscriptionPromise = promise;

			ExternalInterface.CallFromIframe("subscription", subscription.Pid, subscription.Amt, subscription.Pname, subscription.Xbank, subscription.Pdesc);

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
			var promise = new Promise<object>();

			JObject jsonObject = new JObject();
			jsonObject["method"] = "apprequests";
			jsonObject["message"] = message;
			jsonObject["data"] = parameters != null ? parameters["request_key"] : "";
			jsonObject["filters"] = new JArray();
			(jsonObject["filters"] as JArray).Add("app_non_users");

			if (parameters != null && parameters["to"] != null)
			{
				jsonObject["to"] = parameters["to"];
			}

			Ui(jsonObject)
						   .Then(data =>
							{
								if (data != null)
								{
									JObject newJsonObject = (JObject) data;
									Log("Invited :" + newJsonObject);

									promise.Resolve(newJsonObject);
								}
								else
									promise.Resolve(null);
							});

			return promise;
		}

		// override protected IPromise<object> WallPost(ISocialAction action)
		// {
		// var wallPost = (WallPost) action;
		// var deferred = new Deferred();
		//
		// JSONObject obj = new JSONObject();
		// obj["caption"] = wallPost.title;
		// obj["description"] = wallPost.body;
		// obj["link"] = wallPost.link;
		// obj["picture"] = wallPost.image;
		// obj["name"] = wallPost.postTitle;
		// obj["method"] = "feed";
		//
		// if (wallPost.link != null && wallPost.action != null)
		// {
		// 	string link = wallPost.link;
		// 	if (wallPost.parameters)
		// 	{
		// 		string sub = "";
		// 		foreach (string a in wallPost.parameters.Keys)
		// 		{
		// 			sub = sub + "wp" + a + "=" + wallPost.parameters[a] + "&";
		// 		}
		//
		// 		link += "?"        + sub.Substring(0, sub.Length - 1);
		// 		obj["link"] += "?" + sub.Substring(0, sub.Length - 1);
		// 	}
		//
		// 	var newObject = new JSONObject();
		// 	newObject["name"] = wallPost.action;
		// 	newObject["link"] = link;
		//
		// 	obj["actions"] = new JSONArray();
		// 	obj["actions"].Add(newObject);
		// }
		//
		// if (wallPost.uid != null)
		// {
		// 	obj["to"] = wallPost.uid;
		// }
		//
		// this.ui(obj)
		// 				.Then(data =>
		// {
		// 	log.info("Post was" + (data == null ? "" : "n't") + " published");
		// 	deferred.resolve(data);
		// }
		// ).Done();
		//
		// 				return deferred.promise;
		// }

		private Promise<object> SocialRequest(ISocialAction action)
		{
			var socialRequest = (SocialRequest) action;
			var promise = new Promise<object>();

			JObject jsonObject = new JObject();
			jsonObject["method"] = "apprequests";
			jsonObject["message"] = socialRequest.Message;
			jsonObject["data"] = socialRequest.RequestKey != null ? socialRequest.RequestKey : "";
			jsonObject["to"] = socialRequest.Uid;

			Ui(jsonObject)
						   .Then(data =>
								 {
									 Log("Request was" + (data != null ? "" : "n't") + " sent");
									 promise.Resolve(data);
								 }
								).Done();

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

			string image = "";
			if (data["picture"] != null)
			{
				image = (string)data["picture"]["data"]["url"];
			}

			if (data["first_name"] == null && data["name"] != null)
			{
				var str = (string) data["name"];
				string[] tmp = str.Split(' ');
				data["first_name"] = tmp[0];
				data["last_name"] = tmp[1];
			}

			if (data["id"] != null) result.Uid = (string) data["id"];
			if (data["first_name"] != null) result.FirstName = (string) data["first_name"];
			if (data["last_name"] != null) result.LastName = (string) data["last_name"];
			if (image != null) result.Avatar = image;

			return result;
		}

		public override string Uid => IframeVars["viewer_id"] != null ? (string)IframeVars["viewer_id"] : "";
		public override string CommunityURL => IframeVars["commun"] != null ? "https://www.facebook.com/pages/" + IframeVars["communpage"] + "/" + IframeVars["commun"] : "";
		public override bool AppUser => true;
		public override string FlashVarsReferrer => IframeVars["owner_id"] != null ? (string)IframeVars["owner_id"] : "";
		public string FlashVarsAccessToken => IframeVars["access_token"] != null ? (string)IframeVars["access_token"] : "";
	}
}