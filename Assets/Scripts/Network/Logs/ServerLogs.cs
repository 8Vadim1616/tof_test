using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Platform.Mobile.Analytics.Partners;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.Utils;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
#if !UNITY_IOS
using GoogleMobileAds.Api;
#endif
using SimpleDiskUtils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Network.Logs
{
	public class ServerLogs
	{
		public static AbstractWindow LAST_WINDOW;
		public static IServerLogsClip LAST_LOG_CLIP;

		public const string GOOGLE_PLAY = "googlePlay";
		public const string APP_STORE = "appStore";

		public const string PREROLL_SHOW = "prs";
		public const string NEW_VERSION_GET = "nwc";
		public const string PRELOADER_SHOW = "ms2";
		public const string CODE_WINDOW_SHOW = "ms3";
		public const string PRELOADER_ClICK = "mc2";
		public const string CODE_WINDOW_ClICK = "mc3";
		public const string GOOGLE_PLAY_PREFIX = "g";
		public const string WINDOWS_STORE_PREFIX = "w";

		public const string WIN_OPEN = "winOpen";
		public const string WIN_CLOSE = "winClose";

		public const string START_BUY = "startBuy";

		public const string SAVES_LOGS_OCCUPIED_SPACE = "files_mem";
		
		private const string APP_OPEN_AD_LOADING_STARTED = "app_open_ad_loading_started";
		private const string APP_OPEN_AD_LOADING_FINISHED = "app_open_ad_loading_finished";
		private const string APP_OPEN_AD_SHOWING_STARTED = "app_open_ad_showing_started";
		private const string APP_OPEN_AD_SHOWING_FINISHED = "app_open_ad_showing_finished";

		public ServerLogs()
		{
			Game.Windows.WindowOpenEvent += OnWindowOpen;
			Game.Windows.WindowClosedEvent += OnWindowClose;
		}

		public static void SendAppsFlyerConversionData(string data)
		{
#if !UNITY_WEBGL
			var sendData = new Dictionary<string, object>();
			sendData["data"] = JsonConvert.DeserializeObject(data);
			sendData["uid"] = Game.Mobile.Analytics.AppsFlyer.AppsFlyerId;
			SendLog("af", sendData);
			
			AppsFlyerPartner.ConversionData = sendData;
#endif
		}

		public static void LoadGame()
		{
			if (Game.User != null && Game.User.WasRegisterFromServer)
			{
				SendLog("gl_reg");
			}
			else
			{
				SendLog("gl");
			}
		}

		public static void OnShowInterstitial(Dictionary<string, object> obj)
		{
			SendLog("show_int:" + obj["type_ads"]);
		}

		public static void SendFPS(int fps)
		{
			SendLog("fps", new Dictionary<string, object> {["val"] = fps});
		}

		public static void SendActivate()
		{
			SendLog("OnActivate");
		}

		public static void SendDeactivate()
		{
			SendLog("OnDeactivate");
		}

		public static void PaymentErrorFromAne(string val)
		{
			SendLog("pay_error", new Dictionary<string, object> {["val"] = val});
		}

		public static void PaymentBefore(string val, int bankId)
		{
			SendLog("pay_before", new Dictionary<string, object> { ["val"] = val, ["bank"] = bankId });
		}

		public static void PaymentGotAlreadyProvidedFromServer(string val, int bankId)
		{
			SendLog("pay_doublet", new Dictionary<string, object> { ["val"] = val, ["bank"] = bankId });
		}

		public static void PaymentGotGoodAnswerFromServer(string val, int bankId)
		{
			SendLog("pay_success", new Dictionary<string, object> { ["val"] = val, ["bank"] = bankId });
		}

		public static void PaymentGotBadAnswerFromServer(string val, int bankId)
		{
			SendLog("pay_fail", new Dictionary<string, object> { ["val"] = val, ["bank"] = bankId });
		}

		public static void PaymentGotGoodAnswerFromLocal(string val)
		{
			SendLog("pay_success_local", new Dictionary<string, object> {["val"] = val});
		}

		public static void OnFriendsLoadError(Exception ex)
		{
			SendLog("fr_error", new Dictionary<string, object> { ["msg"] = ex.Message });
		}

		public static void PaymentGotBadAnswerFromLocal(string val)
		{
			SendLog("pay_fail_local", new Dictionary<string, object> {["val"] = val});
		}

		public void OnWindowOpen(AbstractWindow win)
		{
			ServerLogsParams logParams = win.LogParams;

			if (logParams == null)
				return;

			Dictionary<string, object> data = logParams.Params;

			// if (!(win is BankNewWindow))
			// {
			// 	LAST_LOG_CLIP = win;
			// }

			LAST_WINDOW = win;

			// var copyParams: Object = JSON.parse(JSON.stringify(logParams.getParams()));
			//
			// if (Localization.COUNT_USED_KEYS && Localization.USED_KEYS_TO_SEND.length > 0)
			// {
			// 	copyParams["lang_stats"] = JSON.stringify(Localization.USED_KEYS_TO_SEND);
			// 	Localization.onSendUsedKeysCount();
			// }
			//
			// if (Localization.COUNT_NOT_TRANSLATED_KEYS && Localization.NOT_TRANSLATED_KEYS_TO_SEND.length > 0)
			// {
			// 	var noLang: Object = {};
			// 	noLang[Settings.LOCALE] = Localization.NOT_TRANSLATED_KEYS_TO_SEND;
			//
			// 	copyParams["no_lang"] = JSON.stringify(noLang);
			//
			// 	Localization.onSendNotTranslatedKeysCount();
			// }

			// Queries.flashLogs(WIN_OPEN, null, copyParams);

			var jObject = JObject.FromObject(data);
			GameLogger.info($"[{WIN_OPEN}] {jObject.ToString(Formatting.None)}");

			SendLog(WIN_OPEN, data);

			/**Очень нужная штука, для определения оригинальной позиции при покупке**/
			if (logParams.Params.ContainsKey("pos"))
				SendBankLogs(logParams);
			else if (logParams.Params.ContainsKey("val"))
				SendLog("w_show", logParams.Params);
		}

		public void OnWindowClose(AbstractWindow win)
		{
			if (win == null)
				return;

			ServerLogsParams logParams = win.LogParams;

			if (logParams == null)
				return;

			Dictionary<string, object> data = win.LogParams.Params;

			var jObject = JObject.FromObject(data);
			GameLogger.info($"[{WIN_CLOSE}] {jObject.ToString(Formatting.None)}");

			SendLog(WIN_CLOSE, data);
		}

		public static void SendBankLogs(ServerLogsParams logParams)
		{
			if (logParams != null)
			{
				/*
				 * showbankpos пока не работает на сервер
				 */
				// Game.ServiceProvider.MultiRequest(new ShowBankPosOperation(logParams.Params));
			}
		}

		public static Dictionary<string, object> GetLastLogParams()
		{
			if (LAST_LOG_CLIP != null)
				return LAST_LOG_CLIP.LogParams.Params;
			return null;
		}

		public static void ResetLastLogClip()
		{
			LAST_LOG_CLIP = null;
		}

		public static string GetLastWindow()
		{
			if (LAST_WINDOW != null)
				return LAST_WINDOW.ClassName;

			return null;
		}

		public static void StartBuy(UserBankItem bankItem)
		{
			ServerLogsParams severLogParams = new ServerLogsParams();
			severLogParams.AddBankItems(new UserBankItem[]{bankItem});

			SendLog(START_BUY, severLogParams.Params);
		}

		public static void SocialConnect(string sn)
		{
			var obj = new Dictionary<string, object>();
			obj["val"] = sn;

			SendLog("sc", obj);
		}

		public static void OnChangeArea(int areaId, string userId)
		{
			var obj = new Dictionary<string, object>();
			obj["val"] = areaId;
			obj["uid"] = userId;

			SendLog("area", obj);
		}
		
		public static void BankShowPos(bool showAll)
		{
			var obj = new Dictionary<string, object>();
			obj["val"] = showAll;

			SendLog("bank_show_pos", obj);
		}

		public static void SendLog(string action, Dictionary<string, object> postData = null)
		{
			if(!string.IsNullOrEmpty(Game.User?.RegisterData?.Uid))
				Game.Network.QueryManager.MultiRequest(new LogOperation(action, postData));
		}
		
		public static void Feedback(int stars, string email, string message)
		{
			var data = new Dictionary<string, object>();
			data["star"] = stars;
			data["email"] = email;
			data["message"] = message;
			SendLog("feedback", data);
		}

		public static void InviteSuccessFromLocation(string val)
		{
			SendLog("invite_success", new Dictionary<string, object> { ["val"] = val });
		}

		public static void SocketConnect()
		{
			if (Game.User != null && Game.User.WasRegisterFromServer)
				SendLog("sr1_reg");
			else
				SendLog("sr1");
		}

		public static void SocketDisconnect(int disconnectType = -1)
		{
			if (Game.User != null && Game.User.WasRegisterFromServer)
				SendLog("sr0_reg");
			else if (disconnectType < 0)
				SendLog("sr0");
			else
				SendLog($"sr0_{disconnectType}");
		}

		public static void GDPRClick(string val)
		{
			SendLog("gdpr", new Dictionary<string, object> { ["val"] = val });
		}

		public static void LevelTutor(int levelId, int stepId)
		{
			SendLog("tut", new Dictionary<string, object> { ["lvl"] = levelId, ["stp"] = stepId });
		}

		public static void WinLevelClick(int levelId, string button)
		{
			SendLog("bravo", new Dictionary<string, object> { ["lvl"] = levelId, ["btn"] = button });
		}

		public static void StarsClick(int levelId, string button)
		{
			SendLog("stars", new Dictionary<string, object> { ["lvl"] = levelId, ["btn"] = button });
		}

		public static void StartLevelClick(int levelId, string button)
		{
			SendLog("st_lvl", new Dictionary<string, object> { ["lvl"] = levelId, ["btn"] = button });
		}

		public static void ZonesClick(int levelId)
		{
			SendLog("zn", new Dictionary<string, object> { ["lvl"] = levelId });
		}

		public static void TasksClick(int levelId, string button)
		{
			SendLog("tasks", new Dictionary<string, object> { ["lvl"] = levelId, ["btn"] = button });
		}

		public static void QuestClick(int questId)
		{
			SendLog("qst", new Dictionary<string, object> { ["id"] = questId });
		}

		public static void QuestAdditionalStart(int questId, int addStars)
		{
			SendLog("qst_stars", new Dictionary<string, object> { ["id"] = questId, ["strs"] = addStars });
		}

		public static void SkipSuperBoomClick(int levelId)
		{
			SendLog("sbm", new Dictionary<string, object> { ["lvl"] = levelId });
		}

		public static void MenuSliderClick(int button)
		{
			SendLog("menu", new Dictionary<string, object> { ["btn"] = button });
		}

		public static void FieldMove(int moveNum, int chip, string combination)
		{
			SendLog("move", new Dictionary<string, object> { ["mv_step"] = moveNum, ["chip"] = chip, ["cmb"] = combination });
		}

		public const string ANALYTICS_START_INIT_ACTION = "start init";
		public const string ANALYTICS_END_INIT_ACTION = "end init";
		
		public static void AnalyticsInitEvent(string val)
		{
			SendLog("analytics", new Dictionary<string, object> { ["action"] = val });
		}

		public static void LoadGameProgress(string val)
		{
			SendLog("ldpr", new Dictionary<string, object> { ["action"] = val });
		}

		public static void StartLoadingProfile(string path)
		{
			SendLog("load_udata", new Dictionary<string, object> { ["path"] = path });
		}

		public static void CantLoadProfile(string error)
		{
			SendLog("no_udata", new Dictionary<string, object> { ["error"] = error });
		}
		
		public static void AppOpenAdLoadingStarted()
		{
			SendLog(APP_OPEN_AD_LOADING_STARTED);
		}

		public static void AppOpenAdLoadingFinished()
		{
			SendLog(APP_OPEN_AD_LOADING_FINISHED);
		}
		
		public static void AppOpenAdShowingStarted()
		{
			SendLog(APP_OPEN_AD_SHOWING_STARTED);
		}
		
#if !UNITY_IOS

		public static void AppOpenAdShowingFinished(AdValue adValue, int adPoint)
		{
			var data = new Dictionary<string, object>
			{
				{ "currencyCode", adValue.CurrencyCode },
				{ "precision", adValue.Precision },
				{ "value", adValue.Value },
				{ "adPoint", adPoint }
			};
			
			SendLog(APP_OPEN_AD_SHOWING_FINISHED, data);
		}

#endif

		public static void ShareSuccess() { SendLog("share_success", new Dictionary<string, object>()); }

		internal static void SendRatingsTab(string tab, params string[] values)
		{
			var data = new Dictionary<string, object>
			{
				{ "tab", tab },
			};

			if (!values.IsNullOrEmpty())
				data.Add("params", values);

			SendLog("ratings_screen", data);
		}
		
		public static void EventItemReceived(int id, int eventId, float itemsReceived, float totalAmount)
		{
			SendLog("event_item_received", new Dictionary<string,object>
			{
				{ "id", id },
				{ "eventId", eventId }, 
				{ "itemsReceived", itemsReceived }, 
				{ "totalAmount",  totalAmount}
			});
		}
	}
}