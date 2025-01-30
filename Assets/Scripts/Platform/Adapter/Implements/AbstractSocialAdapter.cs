using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Platform.Adapter.Data;
using ExternalScripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class AbstractSocialAdapter
	{
		protected virtual string TAG => "[AbstractSocialAdapter] ";
		public virtual string AccessToken => null;
		public virtual string SN => _sn;

		public virtual BoolReactiveProperty IsLoggedIn => _isLoggedIn;

		/** Количество друзей которое можно запросить за 1 раз */
		public virtual int MaxCountFriendsFromNetwork => 50;

		protected SocialAdapterParams _params = null;
		protected BoolReactiveProperty _isLoggedIn = new BoolReactiveProperty(false);
		protected List<Promise<object>> _actions = new List<Promise<object>>();

		protected SocialProfile _userProfile = null;
		protected string[] _friendList = null;
		protected string[] _appFriendList = null;
		protected Promise<object> _paymentPromise = null;
		protected Promise<object> _subscriptionPromise = null;
		protected Promise<object> _requestPromise = null;
		protected Promise<object> _prerollPromise = null;
		protected Promise<object> _completeAdPromise = null;
		protected List<Promise<object>> _prepareAdList = new List<Promise<object>>();

		protected JObject IframeVars => _params.IframeVars;

		private SocialActionHandler[] _snActionsHandlers = { };

		private string _sn = "Abstract";

		public AbstractSocialAdapter(string sn = null, SocialAdapterParams parameters = null)
		{
			Log("Create");

			if (sn != null)
				_sn = sn;

			_params = parameters;

			if (ExternalInterface.IsAvailable)
			{
				ExternalInterface.AddCallback("getUserName", GetUserName);
				ExternalInterface.AddCallback("getLogString", GetLogString);
				ExternalInterface.AddCallback("enableDisableKeyboardCapture", EnableDisableKeyboardCapture);
				ExternalInterface.AddCallback("getScreenshot", GetScreenshot);
			}
		}

		protected void AddActionsHandlers(SocialActionHandler[] actions)
		{
			_snActionsHandlers = Enumerable.Concat(_snActionsHandlers, actions).ToArray();
		}

		public Promise<object> Request(ISocialAction action)
		{
			foreach (SocialActionHandler actionHandler in _snActionsHandlers)
			{
				if (actionHandler.Action.IsInstanceOfType(action))
				{
					return actionHandler.Handler(action);
				}
			}

			return Promise<object>.Rejected(new Exception()) as Promise<object>;
		}

		public virtual Promise Login() { return Promise.Resolved() as Promise;}
		public virtual void Logout() {}

		public virtual Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			result.Resolve(new List<SocialProfile>());

			return result;
		}

		public virtual Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			result.Resolve(new List<SocialProfile>());

			return result;
		}

		public virtual Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			result.Resolve(new SocialProfile());

			return result;
		}

		public virtual SocialProfile GetProfileCached() => _userProfile;

		public virtual Promise<AbstractSocialAdapter> ChangeAdapter(string sn)
		{
			var promise = new Promise<AbstractSocialAdapter>();
			promise.Resolve(null);
			return promise;
		}

		public virtual TransactionQueueData ParseTransactionQueue(JToken data)
		{
			throw new System.NotImplementedException();
		}

		public virtual Promise<object> Invite(string message = "", JObject parameters = null)
		{
			Log(this + " has no implementation for Invite");

			return Promise<object>.Resolved(null) as Promise<object>;
		}

		public Promise<object> ReloadPage()
		{
			var result = new Promise<object>();

			if (ExternalInterface.IsAvailable)
			{
				var obj = new JObject();
				obj["result"] = true;

				ExternalInterface.CallFromIframe("reloadPage");
				result.Resolve(obj);
			}
			else
			{
				result.Reject(new Exception("reload error"));
			}

			return result;
		}

		private string GetUserName(object data = null)
		{
			if (_userProfile != null)
			{
				return _userProfile.FirstName + " " + _userProfile.LastName;
			}
			else
			{
				return "";
			}
		}

		private string GetLogString(object data = null)
		{
			return Game.GameLogger.GetCurrentLog();
		}

		private object EnableDisableKeyboardCapture(object data = null)
		{
			var val = (data as JValue).ToObject<bool>();

#if UNITY_WEBGL
			WebGLInput.captureAllKeyboardInput = val;

			Log("EnableDisableKeyboardCapture " + WebGLInput.captureAllKeyboardInput);
#endif

			return null;
		}

		private static Promise<object> GetScreenshot(object data = null)
		{
			var promise = new Promise<object>();

			Utils.Utils.StartCoroutine(UploadPNG());

			return promise;

			IEnumerator UploadPNG()
			{
				// We should only read the screen after all rendering is complete
				yield return new WaitForEndOfFrame();

				// Create a texture the size of the screen, RGB24 format
				int width = Screen.width;
				int height = Screen.height;
				var tex = new Texture2D( width, height, TextureFormat.RGB24, false );

				// Read screen contents into the texture
				tex.ReadPixels( new Rect(0, 0, width, height), 0, 0 );
				tex.Apply();

				// Encode texture into PNG
				byte[] bytes = tex.EncodeToPNG();
				GameObject.Destroy( tex );

				//string ToBase64String byte[]
				string encodedText = System.Convert.ToBase64String (bytes);

				// var image_url = "data:image/png;base64," + encodedText;
				var image_url = encodedText;


				// Debug.Log (image_url);

				promise.Resolve(image_url);
			}
		}

		public virtual bool IsShareAvailable => false;

		public virtual IPromise Share(string link)
		{
			Debug.LogError("Share is not available on " + SN + " adapter");
			return Promise.Rejected(null);
		}

		public virtual string Locale => IframeVars["locale"] != null ? (string)IframeVars["locale"] : _params.Locale;
		public virtual string Uid => IframeVars["uid"] != null ? (string)IframeVars["uid"] : "";
		public virtual string AuthKey => IframeVars["auth_key"] != null ? (string)IframeVars["auth_key"] : "";
		public virtual string Pm8Uid => IframeVars["multi_uid"] != null ? (string)IframeVars["multi_uid"] : Uid;
		public virtual string Pm8AuthKey => IframeVars["multi_auth_key"] != null ? (string)IframeVars["multi_auth_key"] : AuthKey;
		public virtual bool AppUser => IframeVars["app_user"] != null ? IframeVars["app_user"].ToObject<bool>() : true;
		public virtual string CommunityURL => (string)IframeVars["commun"];
		public virtual bool IsDirectGames => IframeVars["is_direct_games"] != null ? IframeVars["is_direct_games"].ToObject<bool>() : false;
		public virtual bool IsMobile => IsDirectGames;
		public virtual string FlashVarsReferrer => "";
		public virtual JObject WallParams => new JObject();

		public virtual void Free() { }

		/**Необходимо вызвать после полной инициализации*/
		protected void AfterInit()
		{
			Utils.Utils.NextFrame()
				 .Then(() => { _params.InitCallback?.Invoke(this); })
				 .Then(() =>
				  {
					if (ExternalInterface.IsAvailable)
						ExternalInterface.CallFromIframe("onAdapterInit");
				  });
		}

		protected void Log(string str)
		{
			Debug.Log(TAG + str);
		}
	}
}