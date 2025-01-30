using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class MobileAdapter : AbstractSocialAdapter, IMultiAdapter
	{
		private static BoolReactiveProperty IS_LOGGED = new BoolReactiveProperty(false);

		public const string AdapterKey = "adapter";

		// private readonly string[] LANGS_FOR_VK = new string[]{"ru"};
		private readonly string[] LANGS_FOR_VK = new string[] { };

		protected override string TAG => "[MobileAdapter] ";
		public override string SN => _childAdapter.SN;

		private AbstractSocialAdapter _childAdapter = null;
		public override bool IsMobile => true;

		public override string AccessToken => _childAdapter.AccessToken;
		public override BoolReactiveProperty IsLoggedIn => IS_LOGGED; // При смене адаптера состояние забывается, поэтому пишем в статичное свойство

		public AbstractSocialAdapter ChildAdapter => _childAdapter;

		public MobileAdapter(SocialAdapterParams parameters = null) : base(null, parameters)
		{
			CreateAdapter();
		}

		private void CreateAdapter()
		{
#if UNITY_EDITOR || !UNITY_WEBGL
			if (PlayerPrefs.HasKey(AdapterKey)) // Проверяем к какому адаптеру были последний раз подключены
			{
				var adapterKey = PlayerPrefs.GetString(AdapterKey);

				switch (adapterKey)
				{
					case SocialNetwork.VKONTAKTE:
						_childAdapter = new VKMobileAdapter(_params, this);
						break;
					// case SocialNetwork.FACEBOOK:
					// 	_childAdapter = new FBMobileAdapter(_params, this);
					// 	break;
#if UNITY_IOS
					case SocialNetwork.APPLE:
						_childAdapter = AppleMobileAdapter.OfSaved(_params, this);
						break;
#endif
					case SocialNetwork.OKMM:
						_childAdapter = CodeMobileAdapter.OfSaved(_params, this);
						break;
#if UNITY_ANDROID || UNITY_IOS
					case SocialNetwork.GOOGLE_SIGN_IN:
						{
							_childAdapter = GSIMobileAdapter.OfSaved(_params, this);

							if (PlayerPrefs.GetString(AdapterKey) != null && PlayerPrefs.GetString(AdapterKey) == _childAdapter.SN)
								IS_LOGGED.Value = true;
						}
						break;
#endif
#if BUILD_HUAWEI
					case SocialNetwork.HUAWEI_SIGN_IN:
						_childAdapter = HSIMobileAdapter.OfSaved(_params, this);
						break;
#endif
					default:
						_childAdapter = new VKMobileAdapter(_params, this);
						break;
				}
			}

			if (_childAdapter != null)
				return;

			_childAdapter = new VKMobileAdapter(_params, this);
#endif
				}



#if UNITY_EDITOR || !UNITY_WEBGL
		public override Promise<AbstractSocialAdapter> ChangeAdapter(string sn)
		{
			var promise = new Promise<AbstractSocialAdapter>();

			_params.AddInitCallBack(onInit);

			Log("change adapter to " + sn);

			/*if (sn == SocialNetwork.FACEBOOK || sn == SocialNetwork.FACEBOOK_2)
			{
				if (_childAdapter != null && (_childAdapter.SN == SocialNetwork.FACEBOOK || _childAdapter.SN == SocialNetwork.FACEBOOK_2))
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new FBMobileAdapter(_params, this);
				}
			}
			else */if (sn == SocialNetwork.VKONTAKTE)
			{
				if (_childAdapter != null && _childAdapter.SN == SocialNetwork.VKONTAKTE)
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new VKMobileAdapter(_params, this);
				}

			}
			else if (sn == SocialNetwork.APPLE)
			{
				if (_childAdapter != null && _childAdapter.SN == SocialNetwork.APPLE)
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new AppleMobileAdapter(_params, this);
				}
			}
#if UNITY_ANDROID || UNITY_IOS
			else if (sn == SocialNetwork.GOOGLE_SIGN_IN)
			{
				if (_childAdapter != null && _childAdapter.SN == SocialNetwork.GOOGLE_SIGN_IN)
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new GSIMobileAdapter(_params, this);
				}
			}
#endif
#if BUILD_HUAWEI
			else if (sn == SocialNetwork.HUAWEI_SIGN_IN)
			{
				if (_childAdapter != null && _childAdapter.SN == SocialNetwork.HUAWEI_SIGN_IN)
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new HSIMobileAdapter(_params, this);
				}
			}
#endif
			else if (sn == SocialNetwork.ODNOKLASSNIKI || sn == SocialNetwork.MOJMIR)
			{
				if (_childAdapter != null && _childAdapter.SN == SocialNetwork.OKMM)
					onInit(_childAdapter);
				else
				{
					freeOldAdapter();
					_childAdapter = new CodeMobileAdapter(_params, this, sn);
				}
			}

			return promise;

			void onInit(AbstractSocialAdapter adapter)
			{
				promise.Resolve(adapter);
			}

			void freeOldAdapter()
			{
				if (_childAdapter != null)
					_childAdapter.Free();
			}
		}
#endif

		void IMultiAdapter.OnLogin(AbstractSocialAdapter adapter)
		{
			IS_LOGGED.Value = true;

			PlayerPrefs.SetString(AdapterKey, adapter.SN);
		}

		void IMultiAdapter.OnLogout(AbstractSocialAdapter adapter)
		{
			IS_LOGGED.Value = false;

			PlayerPrefs.DeleteKey(AdapterKey);
		}

		void CheckLogged(AbstractSocialAdapter adapter)
		{
			if (PlayerPrefs.GetString(AdapterKey) != null && PlayerPrefs.GetString(AdapterKey) == adapter.SN)
				IS_LOGGED.Value = true;
			else
				IS_LOGGED.Value = false;
		}

		public override void Free()
		{
			_childAdapter?.Free();
		}

		public override IPromise Share(string link) => _childAdapter.Share(link);

		public override Promise Login() { return _childAdapter.Login(); }
		public override void Logout() { _childAdapter.Logout(); }

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0) { return _childAdapter.GetFriends(curPage); }
		public override Promise<List<SocialProfile>> GetAppFriends() { return _childAdapter.GetAppFriends(); }
		public override Promise<SocialProfile> GetProfile() { return _childAdapter.GetProfile(); }
	}
}