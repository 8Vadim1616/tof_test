using System;
using Assets.Scripts.Localization;
using Assets.Scripts.Platform.Adapter.Implements;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter
{
	public class SocialAdapterParams
	{
		public bool IsLocal { get; private set; } = false;
		public string Locale { get; private set; } = LOCALE.EN;
		public JObject IframeVars { get; private set; } = new JObject();

		public SocialNetwork SocialNetwork { get; private set; }

		public Action<AbstractSocialAdapter> InitCallback { get; private set; } = null;

		public SocialAdapterParams(SocialNetwork socialNetwork)
		{
			SocialNetwork = socialNetwork;
		}

		public SocialAdapterParams AddInitCallBack(Action<AbstractSocialAdapter> onInitCallback)
		{
			InitCallback = onInitCallback;
			return this;
		}

		public SocialAdapterParams AddLocale(string locale)
		{
			Locale = locale;
			return this;
		}

		public SocialAdapterParams AddIframeVars(JObject vars)
		{
			if (vars != null)
				IframeVars = vars;

			return this;
		}
	}
}