using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_WSA && !UNITY_EDITOR
using Assets.Scripts.BuildSettings;
using UWPPlugin;
#endif


namespace Assets.Scripts.Platform.Adapter.Implements
{
	/**
	 * Для дебага нужно:
	 * 1) Дебажный билд
	 * 2) Запустить и не выключать на время дебага CheckNetIsolation.exe LoopbackExempt -is -n=4ACEF246.GoldenFarm_05g3z837ka020
	 * 3) После уже можно коннектится rider и unity
	 */
	public class WNDSAdapter : AbstractSocialAdapter
	{
		private static BoolReactiveProperty IS_LOGGED = new BoolReactiveProperty(false);

		private const string GRAPH_API_URL = "https://graph.facebook.com/v11.0/";

		private const string OAUTH_EXCEPTION = "OAuthException";
		private const string WNDS_FB_TOKEN = "wnds_fb_token";

		private const string FIRST_NAME = "first_name";
		private const string LAST_NAME = "last_name";
		private const string EMAIL = "email";
		private const string UID = "id";
		private const string PICTURE = "picture";
		private const string DATA = "data";
		private const string URL = "url";

		private string _fields = "id,first_name,last_name,picture,email";
		private List<string> _scope = new List<string>() {"public_profile", "email"};

		protected override string TAG => "[WNDSAdapter] ";

		public override string AccessToken => _accessToken;
		public override BoolReactiveProperty IsLoggedIn => IS_LOGGED; // При смене адаптера состояние забывается, поэтому пишем в статичное свойство

		private string _accessToken;

#if UNITY_WSA && !UNITY_EDITOR
		private UWP _uwp;
#endif

		public WNDSAdapter(SocialAdapterParams parameters = null) : base(SocialNetwork.WINDOWS_STORE, parameters)
		{
			CreateAdapter();
		}

		private void CreateAdapter()
		{
			_accessToken = PlayerPrefs.GetString(WNDS_FB_TOKEN, null);
			// _accessToken = "EAAYI0en5XWoBAEAiHoahY5zcPi55eyB09Pv2yclhpiFdeT9VFovKECqA2eDFzwKGpAP7owZAhPjaq5ArUnw24ZA1vJBXUPLqsjbuKUoO2KZBzZCYoCyKBGSvUbp8XZCLqsh28fsBCzWwhltF8r4hLn4ulEcoXmqnEtdyV07IyJixszTelesfWfBHllklMnAxHZA5PxN1y8JwZDZD";
			IS_LOGGED.Value = !_accessToken.IsNullOrEmpty();

#if UNITY_WSA && !UNITY_EDITOR
			//it's not an "Agile" API and all of it's functions must be invoked on the same thread it was created on.
			//https://forum.unity.com/threads/the-application-called-an-interface-that-was-marshalled-for-a-different-thread.1115419/
			UnityEngine.WSA.Application.InvokeOnUIThread(() =>
			{
				_uwp = new UWP(GameConsts.WINDOWS_STORE_PACKAGE_IDENTITY_NAME,
						   GameConsts.WINDOWS_STORE_PACKAGE_PUBLISHER_ID,
						   GameConsts.WINDOWS_STORE_NOTIFICATION_TITLE,
						   GameConsts.WINDOWS_STORE_NOTIFICATION_ICON,
						   GameConsts.FacebookAppId,
						   string.Join(",", _scope));

				_uwp.TraceCallback += str => GameLogger.debug(str);
			}, true);

			// long unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 10;

            // _uwp.SendLocalNotification("test", unixTimestamp, 1);
#endif
			AfterInit();
		}

		public override void Free()
		{

		}

		public override Promise Login()
		{
			var promise = new Promise();

			Utils.Utils.StartCoroutine(DoLogin());

			return promise;

			IEnumerator DoLogin()
			{
				string accessToken = null;
				bool asyncOperationDone = false;
#if UNITY_WSA && !UNITY_EDITOR
				UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
				{
					accessToken = await _uwp.FacebookLogin();
					asyncOperationDone = true;
				}, false);
#else
				asyncOperationDone = true;
#endif
				while (!asyncOperationDone)
					yield return null;

				if (!accessToken.IsNullOrEmpty())
				{
					_accessToken = accessToken;
					IS_LOGGED.Value = !_accessToken.IsNullOrEmpty();
					PlayerPrefs.SetString(WNDS_FB_TOKEN, _accessToken);

					promise.Resolve();
				}
				else
				{
					promise.Reject(null);
				}
			}
		}

		public override void Logout()
		{
			_accessToken = null;
			IS_LOGGED.Value = !_accessToken.IsNullOrEmpty();
			PlayerPrefs.SetString(WNDS_FB_TOKEN, _accessToken);
		}

		private Promise<JObject> ApiRequest(string url)
		{
			var promise = new Promise<JObject>();

			url += "&access_token=" + _accessToken;

			SendRequest(url)
						   .Then(after);

			return promise;

			void after(JObject data)
			{
				if (data != null)
				{
					if (data.ContainsKey("error") && data["error"]["type"].ToString() == OAUTH_EXCEPTION) // Протух accesstoken
					{
						Logout();
						Login();
						promise.Reject(null);
						return;
					}
				}

				promise.Resolve(data);
			}
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			ApiRequest($"me/invitable_friends?limit=10000")
						   .Then(response =>
							{
								if (response != null && response.TryGetValue("data", out JToken token))
								{
									if (token is JArray arr)
									{
										var data = arr.ToObject<List<object>>();

										if (data != null)
										{
											result.Resolve(CreateProfiles(data));
											return;
										}
									}
								}

								result.Reject(null);
							});

			return result;
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			ApiRequest($"me/friends?limit=10000&fields={_fields}")
						   .Then(response =>
							{
								if (response != null && response.TryGetValue("data", out JToken token))
								{
									if (token is JArray arr)
									{
										var data = arr.ToObject<List<object>>();

										if (data != null)
										{
											result.Resolve(CreateProfiles(data));
											return;
										}
									}
								}

								result.Reject(null);
							});

			return result;
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			ApiRequest($"/me?fields={FIRST_NAME},{LAST_NAME},{EMAIL}")
						   .Then(response =>
							{
								if (response != null)
								{
									Log("GetProfile => ");

									// var dict = response.ToObject<Dictionary<string, object>>();
									//
									// foreach (var item in dict)
									// 	Log($"{item.Key} : {item.Value}");

									result.Resolve(CreateProfile(response));
									return;
								}

								result.Reject(null);
							});

			return result;
		}

		private List<SocialProfile> CreateProfiles(List<object> data)
		{
			return data.Select(item => CreateProfile(item as JObject)).ToList();
		}

		private SocialProfile CreateProfile(JObject data)
		{
			if (data == null) return null;

			var result = new SocialProfile();

			if (data.ContainsKey(UID)) result.Uid = (string) data[UID];
			if (data.ContainsKey(FIRST_NAME)) result.FirstName = (string) data[FIRST_NAME];
			if (data.ContainsKey(LAST_NAME)) result.LastName = (string) data[LAST_NAME];
			if (data.ContainsKey(EMAIL)) result.Email = (string) data[EMAIL];
			if (data.ContainsKey(PICTURE)) result.Avatar = (string) data[PICTURE][DATA][URL];

			return result;
		}


		private Promise<JObject> SendRequest(string url)
		{
			var promise = new Promise<JObject>();

			var req = UnityWebRequest.Get(GRAPH_API_URL + url);

			req.SendWebRequest().completed += operation =>
			{
				if (req.result == UnityWebRequest.Result.Success)
				{
					var text = req.downloadHandler.text;

					Log($"data: {text}");

					try
					{
						var result = JObject.Parse(text);

						promise.Resolve(result);
					}
					catch (Exception e)
					{
						Log($"[ERROR]: {e.Message}");

						promise.Resolve(null);
					}
				}
				else
				{
					Log($"onError url = {url}");

					promise.Resolve(null);
				}
			};

			return promise;
		}

		public void CancelAllLocalNotifications()
		{
#if UNITY_WSA && !UNITY_EDITOR
			UnityEngine.WSA.Application.InvokeOnUIThread(() =>
			{
				_uwp.CancelAllLocalNotifications();
			}, true);
#endif
		}

		public void SendLocalNotification(string scheduleMessage, long scheduleTimestamp, int scheduleNotificationId)
		{
#if UNITY_WSA && !UNITY_EDITOR
			UnityEngine.WSA.Application.InvokeOnUIThread(() =>
			{
				_uwp.SendLocalNotification(scheduleMessage, scheduleTimestamp, scheduleNotificationId);
			}, true);
#endif
		}
	}
}