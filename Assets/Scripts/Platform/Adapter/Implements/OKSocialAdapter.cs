using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Platform.Adapter.Network;
using ExternalScripts;
using Newtonsoft.Json.Linq;
using Assets.Scripts.Platform.Adapter.Data;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class OKSocialAdapter : AbstractSocialAdapter
	{
		protected override string TAG => "[OKSocialAdapter] ";

		private string _fields = "uid,first_name,last_name,gender,pic_1,pic_2,pic_3,pic_4,url_profile,birthday";

		private OKAdapterNetworkManager _network = null;

		// private Promise _actionPromise = null;

		private JObject _streamPublishArgs = null;

		private bool _wallPostMediaTopic = false;
		private Promise<bool> _wallPostPromise = null;

		public OKSocialAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.VKONTAKTE, parameters)
		{
			_isLoggedIn.Value = true;

			_network = new OKAdapterNetworkManager(IframeVars["api_server"] + "api/", "", IframeVars);

			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.AddCallback("confirmStreamPublish", ConfirmStreamPublish);
				ExternalInterface.AddCallback("confirmPayment", OnPayment);
				ExternalInterface.AddCallback("confirmMidroll", ConfirmMidroll);
				ExternalInterface.AddCallback("confirmRequest", OnRequest);
				ExternalInterface.AddCallback("onFapiCallback", ApiCallback);
				ExternalInterface.AddCallback("confirmInvite", ConfirmInvite);
			}

			SocialActionHandler[] handlers =
			{
							new SocialActionHandler(typeof(SocialRequest), SocialRequest),
							new SocialActionHandler(typeof(Payment), Payment),
							new SocialActionHandler(typeof(Advertising), ShowAdvertising)
			};

			AddActionsHandlers(handlers);

			AfterInit();
		}

		private Promise<JToken> Api(string method, JObject obj = null)
		{
			return _network.SendToSN(method, obj);
		}

		private object ApiCallback(object obj)
		{
			Log("apiCallback " + obj);

			// if (_actionPromise != null)
			// {
			// 	_actionPromise.Resolve(obj);
			// 	_actionPromise = null;
			// }

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

			if (_friendList != null)
			{
				get();
			}
			else
			{
				Api("friends/get", null)
					.Then(data =>
					 {
						 string[] newData = data.ToObject<string[]>();

						 _friendList = newData != null ? newData : new string[] { };
						 get();
					 }).Done();
			}

			return result;

			void get()
			{
				string[] uids = MaxCountFriendsFromNetwork > 0 ? _friendList.Skip(MaxCountFriendsFromNetwork * curPage).Take(MaxCountFriendsFromNetwork).ToArray() : _friendList;
				if (uids.Length > 0)
				{
					JObject args = new JObject();
					args["uids"] = string.Join(",", uids);
					args["fields"] = _fields;
					args["emptyPictures"] = true;

					Api("users/getInfo", args)
								   .Then(data =>
									{
										if (data == null)
										{
											Log("getProfiles response is null");
											return;
										}

										var newJsonArray = data.ToObject<JArray>();

										var list = new List<SocialProfile>();

										foreach (var token in newJsonArray)
											list.Add(CreateProfile(token));

										result.Resolve(list);
									}).Done();
				}
				else
				{
					result.Resolve(new List<SocialProfile>());
				}
			}
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
				Api("friends/getAppUsers", null)
					.Then(data =>
					 {
						 if (data["uids"] != null)
						 {
							 string[] newData = data["uids"].ToObject<string[]>();

							 _appFriendList = newData != null ? newData : new string[] { };
							 get();
						 }
						 else
						 {
							 Log("GetAppFriends No uids");

							 result.Resolve(new List<SocialProfile>());
						 }
					 }).Done();
			}

			return result;

			void get()
			{
				string[] uids = MaxCountFriendsFromNetwork > 0 ? _appFriendList.Skip(MaxCountFriendsFromNetwork * curPage).Take(MaxCountFriendsFromNetwork).ToArray() : _appFriendList;
				if (uids.Length > 0)
				{
					var args = new JObject();
					args["uids"] = string.Join(",", uids);
					args["fields"] = _fields;
					args["emptyPictures"] = true;

					Api("users/getInfo", args)
								   .Then(data =>
									{
										if (data == null)
										{
											Log("getProfiles response is null");
											return;
										}

										var newJsonArray = data.ToObject<JArray>();

										var list = new List<SocialProfile>();

										foreach (var token in newJsonArray)
											list.Add(CreateProfile(token));

										result.Resolve(list);
									}).Done();
				}
				else
				{
					result.Resolve(new List<SocialProfile>());
				}
			}
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			var jsonObject = new JObject();
			jsonObject["uids"] = Uid;
			jsonObject["fields"] = this._fields;
			jsonObject["emptyPictures"] = true;

			Api("users/getInfo", jsonObject)
						   .Then(data =>
							{
								if (data.Type == JTokenType.Array)
								{
									var newJsonArray = data.ToObject<JArray>();
									var userProfile = CreateProfile(newJsonArray.First);
									result.Resolve(userProfile);
								}
								else
								{
									Log("No User Profile");
									result.Resolve(new SocialProfile());
								}
							});

			return result;
		}
		public override TransactionQueueData ParseTransactionQueue(JToken data) => new TransactionQueueData(data.Value<int>("product_code"), data.Value<string>("sig"));

		private Promise<object> Payment(ISocialAction action)
		{
			var payment = (Payment) action;
			var promise = new Promise<object>();

			_paymentPromise = promise;

			ExternalInterface.CallFromIframe("FAPI.UI.showPayment", payment.Pname, payment.Pdesc, payment.Pid, payment.Amt, null, null, "ok", "true");

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

		private object ConfirmInvite(object success)
		{
			Log("onInvite");
			if (_requestPromise != null)
			{
				_requestPromise.Resolve(success);
				_requestPromise = null;
			}

			return null;
		}

		public override Promise<object> Invite(string message = "", JObject parameters = null)
		{
			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.CallFromIframe("FAPI.UI.showInvite", message, "", "");
			}

			_requestPromise = new Promise<object>();

			return _requestPromise;
		}

		private object ConfirmStreamPublish(object result)
		{
			JObject newJsonNode = JObject.Parse((string) result);

			Log("resig = " + result);

			if (_wallPostMediaTopic)
			{
				if (result != null)
				{
					_wallPostPromise.Resolve(true);
				}
				else
				{
					_wallPostPromise.Resolve(false);
				}
			}
			else
			{
				if (result != null)
				{
					_streamPublishArgs["resig"] = newJsonNode;
					Api("stream/publish", _streamPublishArgs)
						.Then(data =>
						 {
							 Log("on stream/publish");
							 if (data != null)
								 _wallPostPromise.Resolve(true);
							 else
								 _wallPostPromise.Resolve(false);
						 }).Done();
				}
				else
				{
					_wallPostPromise.Resolve(false);
				}
			}

			return null;
		}

		// override protected IPromise<object> WallPost(ISocialAction action)
		// {
		// 				var wallPost = (WallPost) action;
			// var deferred = new Deferred();
			//
			// this.wallPostPromise = deferred;
			// this.streamPublishArgs = new JSONObject();
			// this.streamPublishArgs["message"] = wallPost.title;
			//
			// log.info("wallPost image " + wallPost.image);
			//
			// string link = "";
			// if (wallPost.parameters)
			// {
			// 	foreach (string i in wallPost.parameters.Keys)
			// 	{
			// 		link += "&" + i + "=" + wallPost.parameters[i];
			// 	}
			//
			// 	link = link.Substring(1);
			// }
			//
			// string caption = wallPost.title;
			// caption = caption.Substring(0, 63);
			//
			// JSONObject src = new JSONObject();
			// src["src"] = wallPost.image;
			// src["type"] = "image";
			//
			// JSONObject obj = new JSONObject();
			// obj["media"] = new JSONArray();
			// obj["media"].Add(src);
			// obj["caption"] = caption;
			//
			// this.streamPublishArgs["attachment"] = obj.ToString();
			// if (wallPost.action != null)
			// {
			// 	if (link.Length == 0)
			// 		link = "none";
			//
			// 	JSONArray newArray = new JSONArray();
			// 	JSONObject newObject = new JSONObject();
			// 	newObject["text"] = wallPost.action;
			// 	newObject["href"] = link;
			// 	newArray.Add(newObject);
			//
			// 	this.streamPublishArgs["action_links"] = newArray.ToString();
			// }
			//
			// this.streamPublishArgs["application_key"] = this._flashVars["application_key"];
			// this.streamPublishArgs["format"] = "JSON";
			// this.streamPublishArgs["session_key"] = this._flashVars["session_key"];
			// this.streamPublishArgs["sig"] = generateSignature(this.streamPublishArgs, this._flashVars["session_secret_key"]).ToLower();
			//
			// if (wallPost.image.IndexOf("http") == -1)
			// {
			// 	this.wallPostMediaTopic = false;
			// 	ExternalInterface.call("FAPI.UI.showConfirmation", "stream.publish", wallPost.title, this.streamPublishArgs["sig"]);
			// 	return this.wallPostPromise.promise;
			// }
			//
			// //Заменяем путь к картинке. Чтобы всегда был http
			// wallPost.image = wallPost.image.Replace("https://", "http://");
			//
			// this.wallPostMediaTopic = true;
			//
			// this.streamPublishArgs["attachment"] = new JSONObject();
			// JSONArray mediaArray = new JSONArray();
			// JSONObject first = new JSONObject();
			// first["type"] = "app";
			// first["text"] = wallPost.body;
			//
			// JSONArray imagesArray = new JSONArray();
			// JSONObject imagesObject = new JSONObject();
			// imagesObject["url"] = wallPost.image;
			// imagesObject["mark"] = "wallpost";
			// imagesObject["url"] = caption;
			//
			// imagesArray.Add(imagesObject);
			// first["images"] = imagesArray;
			//
			// JSONArray actionsArray = new JSONArray();
			// JSONObject actionsObject = new JSONObject();
			// actionsObject["text"] = wallPost.action;
			// actionsObject["mark"] = "wallpost";
			//
			// actionsArray.Add(actionsObject);
			// first["actions"] = actionsArray;
			//
			// JSONObject second = new JSONObject();
			// second["type"] = "text";
			// second["text"] = caption;
			//
			// mediaArray.Add(first);
			// mediaArray.Add(second);
			//
			// this.streamPublishArgs["attachment"]["media"] = mediaArray;
			//
			// ExternalInterface.call("FAPI.UI.postMediatopic", this.streamPublishArgs["attachment"], false);
			//
			// return this.wallPostPromise.promise;
		// }

		private Promise<object> SocialRequest(ISocialAction action)
		{
			var socialRequest = (SocialRequest) action;
			var promise = new Promise<object>();

			_requestPromise = promise;

			if (socialRequest.Uid != null)
			{
				Log("try to send request " + socialRequest.Uid);
				string[] splited = socialRequest.Uid.Split(',');

				socialRequest.Uid = string.Join(";", splited);

				ExternalInterface.CallFromIframe("FAPI.UI.showNotification", socialRequest.Message, "", socialRequest.Uid);
			}
			else
			{
				ExternalInterface.CallFromIframe("FAPI.UI.showNotification", socialRequest.Message);
			}

			return promise;
		}

		private Promise<object> ShowAdvertising(ISocialAction action)
		{
			Log("Adv Call");

			var advertising = (Advertising) action;
			_completeAdPromise = new Promise<object>();

			ExternalInterface.CallFromIframe("FAPI.invokeUIMethod", advertising.State);

			return _completeAdPromise;
		}

		private object ConfirmMidroll(object response /*rewarded : Boolean, data : Object*/)
		{
			JObject obj = response as JObject;
			bool isExist = obj["isExist"].ToObject<bool>();
			string data = (string)obj["data"];

			if (_completeAdPromise != null)
			{
				if (isExist)
					Log("ads is " + data);
				else
					Log("ads is absent with err: " + data);

				_completeAdPromise.Resolve(isExist);

				_completeAdPromise = null;
			}

			return null;
		}

		public override JObject WallParams
		{
			get
			{
				Log("flashVars.wp = " + IframeVars["wp"]);

				if (IframeVars["custom_args"] != null)
				{
					try
					{
						JObject result = new JObject();
						string post = Regex.Unescape((string)IframeVars["custom_args"]);
						string[] temp = post.Split('&');

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

			string photoRec = (string)data["pic_1"];
			if (photoRec == null && IframeVars["default_photo_50"] != null)
			{
				data["pic_1"] = IframeVars["default_photo_50"];
			}

			if (data["uid"] != null) result.Uid = (string) data["uid"];
			if (data["first_name"] != null) result.FirstName = (string) data["first_name"];
			if (data["last_name"] != null) result.LastName = (string) data["last_name"];
			if (data["pic_1"] != null) result.Avatar = (string) data["pic_1"];

			return result;
		}

		public override string Uid => IframeVars["logged_user_id"] != null ? (string)IframeVars["logged_user_id"] : "";
		public override string CommunityURL => IframeVars["commun"] != null ? "http://www.odnoklassniki.ru/group/" + (string)IframeVars["commun"] : "";
		public override bool AppUser => (string)IframeVars["authorized"] == "1";
		public override string FlashVarsReferrer => IframeVars["referer"] != null ? (string)IframeVars["referer"] : "";
	}

	public class OKAdapterNetworkManager : AbstractAdapterNetworkManager
	{
		private int _callId = 1;
		private JObject _vars = null;

		public OKAdapterNetworkManager(string server, string entryPoint, JObject vars) : base(server, entryPoint, vars["session_secret_key"].ToObject<string>())
		{
			_vars = vars;
		}

		protected override void AddSNArguments(JObject getArgs)
		{
			getArgs["application_key"] = _vars["application_key"];
			getArgs["call_id"] = _callId++;
			getArgs["format"] = "JSON";
			getArgs["session_key"] = _vars["session_key"];

			var dict = getArgs.ToObject<Dictionary<string, string>>();

			if (getArgs["sig"] == null)
				getArgs["sig"] = Signature(dict).ToLower();
		}
	}
}