#if UNITY_ANDROID || UNITY_IOS
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Google;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class GSIMobileAdapter : AbstractSocialAdapter
	{
		private IMultiAdapter _mobileAdapter;

		private const string EDITOR_LOCAL_ID = "100153983048016634516";

		protected override string TAG => "[GSIMobileAdapter] ";

		public override string AccessToken => _googleSignInUser?.UserId;

		private GoogleSignInUser _googleSignInUser;

		public static GSIMobileAdapter OfSaved(SocialAdapterParams parameters, IMultiAdapter mobileAdapter)
		{
			var result = new GSIMobileAdapter(parameters, mobileAdapter);
			result.ForceConnect();
			return result;
		}

		public GSIMobileAdapter(SocialAdapterParams parameters, IMultiAdapter mobileAdapter) : base(SocialNetwork.GOOGLE_SIGN_IN, parameters)
		{
			_mobileAdapter = mobileAdapter;

			Init().Then(AfterInit);
		}

		private Promise Init()
		{
			var initPromise = new Promise();

			if (GoogleSignIn.Configuration == null)
			{
				GoogleSignInConfiguration configuration = new GoogleSignInConfiguration {
																		  WebClientId = GameConsts.GoogleSignInClientId,
																		  RequestIdToken = true
														  };

				GoogleSignIn.Configuration = configuration;
				// GoogleSignIn.DefaultInstance.EnableDebugLogging(true);
			}

			initPromise.Resolve();

			return initPromise;
		}

		public override Promise Login()
		{
			var result = new Promise();

#if UNITY_EDITOR
			_googleSignInUser = new GoogleSignInUser();
			_googleSignInUser.UserId = EDITOR_LOCAL_ID;

			_isLoggedIn.Value = true;

			if (_isLoggedIn.Value)
				_mobileAdapter.OnLogin(this);

			result.Resolve();
#else
			GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
#endif
			return result;

			void OnAuthenticationFinished(Task<GoogleSignInUser> task)
			{
				Log("Status: " + task.Status);

				if (task.IsFaulted)
				{
					using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
							Log("Got Error: " + error.Status + " " + error.Message);

							_isLoggedIn.Value = false;

							result.Reject();
						}
						else
						{
							Log("Got Unexpected Exception?!?" + task.Exception);
							_isLoggedIn.Value = false;

							result.Reject();
						}
					}
				}
				else if(task.IsCanceled)
				{
					Log("Canceled");

					_isLoggedIn.Value = false;

					result.Reject();
				}
				else
				{
					_googleSignInUser = task.Result;

					Log("UserId: " + _googleSignInUser.UserId);

					_isLoggedIn.Value = true;

					if (_isLoggedIn.Value)
						_mobileAdapter.OnLogin(this);

					result.Resolve();
				}
			}
		}

		public override void Logout()
		{
#if UNITY_EDITOR
			_googleSignInUser = null;
			_isLoggedIn.Value = false;
			_mobileAdapter.OnLogout(this);
#else
			GoogleSignIn.DefaultInstance.SignOut();
			_isLoggedIn.Value = false;
			_mobileAdapter.OnLogout(this);
#endif
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
			profile.Uid = _googleSignInUser?.UserId;
			profile.FirstName = _googleSignInUser?.DisplayName;
			profile.Avatar = _googleSignInUser?.ImageUrl?.ToString();
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