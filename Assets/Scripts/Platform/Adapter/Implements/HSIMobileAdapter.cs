#if BUILD_HUAWEI
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Google;
using HmsPlugin;
using HuaweiMobileServices.Id;
using HuaweiMobileServices.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class HSIMobileAdapter : AbstractSocialAdapter
	{
		private IMultiAdapter _mobileAdapter;

		private const string EDITOR_LOCAL_ID = "MDFvaMIxZMicTnXpYibSWOfnz3Kiaj9yCE9yXF1z2oyMCMIoA";

		protected override string TAG => "[HSIMobileAdapter] ";

		public override string AccessToken
		{
			get
			{
#if UNITY_EDITOR
				return EDITOR_LOCAL_ID;
#endif
				return _authAccount?.UnionId;
			}
		}

		private AuthAccount _authAccount;

		public static HSIMobileAdapter OfSaved(SocialAdapterParams parameters, IMultiAdapter mobileAdapter)
		{
			var result = new HSIMobileAdapter(parameters, mobileAdapter);
			result.ForceConnect();
			return result;
		}

		public HSIMobileAdapter(SocialAdapterParams parameters, IMultiAdapter mobileAdapter) : base(SocialNetwork.HUAWEI_SIGN_IN, parameters)
		{
			if (HMSAccountManager.Instance == null)
				new GameObject("HMSAccountManager").AddComponent<HMSAccountManager>();

			_mobileAdapter = mobileAdapter;

			Init().Then(AfterInit);
		}

		private Promise Init()
		{
			var initPromise = new Promise();

			initPromise.Resolve();

			return initPromise;
		}

		public override Promise Login()
		{
			var result = new Promise();

#if UNITY_EDITOR
			_isLoggedIn.Value = true;

			if (_isLoggedIn.Value)
				_mobileAdapter.OnLogin(this);

			result.Resolve();
#else
			HMSAccountManager.Instance.OnSignInSuccess = OnLoginSuccess;
			HMSAccountManager.Instance.OnSignInFailed = OnLoginFailure;

			HMSAccountManager.Instance.SignIn();
#endif
			return result;

			void OnLoginSuccess(AuthAccount account)
			{
				_authAccount = account;

				Log("UserId: " + _authAccount.UnionId);

				_isLoggedIn.Value = true;

				if (_isLoggedIn.Value)
					_mobileAdapter.OnLogin(this);

				result.Resolve();
			}

			void OnLoginFailure(HMSException hmsException)
			{
				Log("Got Error: " + hmsException.ErrorCode + " " + hmsException.Message);

				_isLoggedIn.Value = false;

				result.Reject();
			}
		}

		public override void Logout()
		{
			HMSAccountManager.Instance.SignOut();

			_isLoggedIn.Value = false;

			_mobileAdapter.OnLogout(this);
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

			var profile = new SocialProfile();

#if UNITY_EDITOR
			profile.Uid = EDITOR_LOCAL_ID;
#else
			profile.Uid = _authAccount?.UnionId;
			profile.FirstName = _authAccount?.DisplayName;
			profile.Avatar = _authAccount?.AvatarUriString;
			profile.Email = _authAccount?.Email;
#endif

			result.Resolve(new SocialProfile());

			return result;
		}

		public override bool IsShareAvailable => false;

		public override IPromise Share(string link)
		{
			Debug.Log("Start share");
			
			var promise = new Promise();

			return promise;
		}

		private void ForceConnect()
		{
			_isLoggedIn.Value = true;

			_mobileAdapter.OnLogin(this);
		}
	}
}
#endif