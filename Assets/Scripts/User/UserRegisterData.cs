using System;
using Assets.Scripts.Localization;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.User
{
	public class UserRegisterData
	{
		public class RegisterDataFile
		{
			[JsonProperty("uid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string Uid;
			
			[JsonProperty("auth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AuthKey;
			
			[JsonProperty("muid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileUid;
			
			[JsonProperty("mauth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileAuthKey;
		}

		private static string REGISTER_DATA_FILE = "register_data";
		private static string LocaleKey = "locale";

		public const string UID_KEY = "uid";
		public const string AUTH_KEY_KEY = "auth_key";

		public const string MOBILE_UID_KEY = "muid";
		public const string MOBILE_AUTH_KEY_KEY = "mauth_key";

		public string MobileUid => DataFile?.MobileUid;
		public string MobileAuthKey => DataFile?.MobileAuthKey;

		private string _uid = null;
		public string Uid => _uid ?? DataFile?.Uid;
		public string AuthKey => DataFile?.AuthKey;

		public event Action<string> OnChangeUid;

		public RegisterDataFile DataFile { get; private set; }

		public bool? NeedShowGDPR { get; private set; } = null;

		public bool HasValidUid =>
			Uid?.Length > 0 && Uid != UserData.DEFAULT_UID;

		public UserData User { get; private set; }

		public UserRegisterData(UserData user, string uid = null)
		{
			User = user;
			_uid = uid;
		}

		public void InitCurrent()
		{
#if UNITY_WEBGL
			Uid = Game.Social.Adapter.Pm8Uid;
			AuthKey = Game.Social.Adapter.Pm8AuthKey;
#else
			LoadUidAndAuthKey();
#endif
			if (HasValidUid)
				OnChangeUid?.Invoke(Uid);
		}

		private void LoadUidAndAuthKey()
		{
			LoadRegisterFile();

			var needToSave = false;

			if (DataFile.Uid.IsNullOrEmpty())
			{
				DataFile.Uid = PlayerPrefs.GetString(UID_KEY, UserData.DEFAULT_UID);
				if (!DataFile.Uid.IsNullOrEmpty())
					needToSave = true;
			}

			if (DataFile.MobileUid.IsNullOrEmpty())
			{
				DataFile.MobileUid = PlayerPrefs.GetString(MOBILE_UID_KEY, null);
				if (!DataFile.MobileUid.IsNullOrEmpty())
					needToSave = true;
			}

			if (DataFile.MobileAuthKey.IsNullOrEmpty())
			{
				DataFile.MobileAuthKey = PlayerPrefs.GetString(MOBILE_AUTH_KEY_KEY, null);
				if (!DataFile.MobileAuthKey.IsNullOrEmpty())
					needToSave = true;
			}

			if (DataFile.AuthKey.IsNullOrEmpty())
			{
				DataFile.AuthKey = PlayerPrefs.GetString(AUTH_KEY_KEY, null); //"66877433462cd9dcba57068a43257f78"; 
				if (!DataFile.AuthKey.IsNullOrEmpty())
					needToSave = true;
			}

			if (needToSave)
				SaveRegisterFile();
		}
		
		public void ChangeUid(string uid, string auth_key)
		{
			DataFile.Uid = uid;
			DataFile.AuthKey = auth_key;

			PlayerPrefs.SetString(UID_KEY, Uid);
			PlayerPrefs.SetString(AUTH_KEY_KEY, AuthKey);
			SaveRegisterFile();

			OnChangeUid?.Invoke(uid);
		}

		public bool IsGDPRConfirmed
		{
			get => PlayerPrefs.HasKey("gdpr") && PlayerPrefs.GetInt("gdpr") > 0;
			set => PlayerPrefs.SetInt("gdpr", value ? 1 : 0);
		}

		public void Register(string muid, string mauth_key, bool needShowGDPR)
		{
			DataFile.MobileUid = muid;
			DataFile.MobileAuthKey = mauth_key;
			NeedShowGDPR = needShowGDPR;

			IsGDPRConfirmed = false;

			PlayerPrefs.SetString(MOBILE_UID_KEY, MobileUid);
			PlayerPrefs.SetString(MOBILE_AUTH_KEY_KEY, MobileAuthKey);
			SaveRegisterFile();
		}
		
		private void SaveRegisterFile()
		{
			FileResourcesLoader.NoGroup().SaveJson(REGISTER_DATA_FILE, DataFile);
		}

		private void LoadRegisterFile()
		{
			DataFile = FileResourcesLoader.NoGroup().LoadJsonFromFile<RegisterDataFile>(REGISTER_DATA_FILE);
			if (DataFile == null)
				DataFile = new RegisterDataFile();
		}

		private string appsFlyerId = null;
		public string AppsFlyerId
		{
			get => appsFlyerId;
			set
			{
				if (appsFlyerId == value)
					return;

				appsFlyerId = value;

				Debug.Log("save AppsFlyerId " + appsFlyerId);

				Game.QueryManager.MultiRequest(new UserNameOperation(new UserNameOperation.Request
				{
					AppsFlyerId = appsFlyerId,
				}));
			}
		}

		private string advertisingId = null;
		public string AdvertisingId
		{
			get => advertisingId;
			set
			{
				if (advertisingId == value)
					return;

				advertisingId = value;

				Debug.Log("save AdvertisingId " + advertisingId);

				Game.QueryManager.MultiRequest(new UserNameOperation(new UserNameOperation.Request
				{
					AdvertId = advertisingId,
					Lang = GameLocalization.Locale,
					TimeZoneOffset = -(int) TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes
				}));
			}
		}

		private string firebaseCloudMessagingToken = null;
		public string FirebaseCloudMessagingToken
		{
			get => firebaseCloudMessagingToken;
			set
			{
				if (firebaseCloudMessagingToken == value)
					return;

				firebaseCloudMessagingToken = value;

				Debug.Log("save FirebaseCloudMessagingToken " + firebaseCloudMessagingToken);

				Game.QueryManager.MultiRequest(new UserNameOperation(new UserNameOperation.Request
				{
					FirebaseToken = firebaseCloudMessagingToken,
					Lang = GameLocalization.Locale,
					TimeZoneOffset = -(int) TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes
				}));
			}
		}

		private string installRef = null;
		public string InstallRef
		{
			get => installRef;
			set
			{
				if (installRef == value)
					return;

				installRef = value;

				// Game.Network.QueryManager.MultiRequest(new UserNameOperation(new UserNameOperation.Request
				// 															 {
				// 																			 InstallRef = installRef,
				// 																			 Lang = GameLocalization.Locale,
				// 																			 TimeZoneOffset = -(int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes
				// 															 }));
			}
		}

		public static void SaveLocale(string locale) { PlayerPrefs.SetString(LocaleKey, locale); }
		public static string GetLocale() { return PlayerPrefs.GetString(LocaleKey, null); }
	}
}
