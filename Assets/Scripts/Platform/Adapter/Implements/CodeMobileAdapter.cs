using Assets.Scripts.Libraries.RSG;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class CodeMobileAdapter : AbstractSocialAdapter
	{
		private const string SnKey = "сode_mobile_adapter_sn";

		private IMultiAdapter _mobileAdapter = null;

		public string CurSn { get; private set; }

		protected override string TAG => "[CodeMobileAdapter] ";
		public override bool IsMobile => true;

		public static CodeMobileAdapter OfSaved(SocialAdapterParams parameters, IMultiAdapter mobileAdapter)
		{
			var result = new CodeMobileAdapter(parameters, mobileAdapter);

			result.CurSn = result.GetSavedSn();

			return result;
		}

		public CodeMobileAdapter(SocialAdapterParams parameters, IMultiAdapter mobileAdapter, string curSn = null) : base(SocialNetwork.OKMM, parameters)
		{
			_mobileAdapter = mobileAdapter;

			if (curSn != null)
			{
				CurSn = curSn;
				SaveSn(CurSn);
			}

			Init().Then(AfterInit);
		}

		private Promise Init()
		{
			_isLoggedIn.Value = true;

			if (_isLoggedIn.Value)
				_mobileAdapter.OnLogin(this);

			return Promise.Resolved() as Promise;
		}

		public override Promise Login()
		{
			_isLoggedIn.Value = true;

			if (_isLoggedIn.Value)
				_mobileAdapter.OnLogin(this);

			return Promise.Resolved() as Promise;
		}

		public override void Logout()
		{
			RemoveSavedSn();

			_isLoggedIn.Value = false;

			_mobileAdapter.OnLogout(this);
		}

		private void SaveSn(string sn) { PlayerPrefs.SetString(SnKey, sn); }
		private string GetSavedSn() { return PlayerPrefs.GetString(SnKey, SocialNetwork.ODNOKLASSNIKI); }
		private void RemoveSavedSn() { PlayerPrefs.DeleteKey(SnKey); }
	}
}