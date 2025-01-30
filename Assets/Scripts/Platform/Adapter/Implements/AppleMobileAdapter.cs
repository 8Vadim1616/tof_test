using Assets.Scripts.Libraries.RSG;
using UnityEngine.SignInWithApple;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class AppleMobileAdapter : AbstractSocialAdapter
	{
		private IMultiAdapter _mobileAdapter;
		private SignInWithApple _siwa = null;
		private UserInfo _userInfo;

		protected override string TAG => "[AppleMobileAdapter] ";
		public override bool IsMobile => true;

		public override string AccessToken => _userInfo.idToken;

		public static AppleMobileAdapter OfSaved(SocialAdapterParams parameters, IMultiAdapter mobileAdapter)
		{
			var result = new AppleMobileAdapter(parameters, mobileAdapter);
			result.ForceConnect();
			return result;
		}

		public AppleMobileAdapter(SocialAdapterParams parameters, IMultiAdapter mobileAdapter) : base(SocialNetwork.APPLE, parameters)
		{
			_mobileAdapter = mobileAdapter;

			Init().Then(AfterInit);
		}

		private Promise Init()
		{
			_siwa = new SignInWithApple();

			return Promise.Resolved() as Promise;
		}

		public override Promise Login()
		{
			var result = new Promise();

			_siwa.Login(LoginCallback);

			return result;

			void LoginCallback(SignInWithApple.CallbackArgs args)
			{
				if (args.error != null)
				{
					Log("Errors occurred: " + args.error);
					result.Reject();
					return;
				}

				_userInfo = args.userInfo;

				Log("UserId: " + _userInfo.userId);
				Log("AccessToken: " + _userInfo.idToken);

				_isLoggedIn.Value = true;

				_mobileAdapter.OnLogin(this);

				result.Resolve();
			}
		}

		public override void Logout()
		{
			_isLoggedIn.Value = false;

			_mobileAdapter.OnLogout(this);
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			var profile = new SocialProfile();

			profile.FirstName = _userInfo.displayName;
			profile.Email = _userInfo.email;

			result.Resolve(profile);

			return result;
		}

		private void ForceConnect()
		{
			_isLoggedIn.Value = true;

			_mobileAdapter.OnLogin(this);
		}
	}
}