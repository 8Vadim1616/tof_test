using System;
using System.Collections.Generic;
using Assets.Scripts.Platform.Adapter.Implements;

namespace Assets.Scripts.Platform.Adapter
{
	public class SocialAdapterFactory
	{
		private static Dictionary<string, Type> CREATORS
		{
			get
			{
				Dictionary<string, Type> obj = new Dictionary<string, Type>();

				obj[SocialNetwork.VKONTAKTE] = typeof(VKSocialAdapter);
				obj[SocialNetwork.ODNOKLASSNIKI] = typeof(OKSocialAdapter);
				obj[SocialNetwork.FACEBOOK] = typeof(FBSocialAdapter);
				obj[SocialNetwork.FACEBOOK_2] = typeof(FBSocialAdapter);
				obj[SocialNetwork.MOJMIR] = typeof(MMSocialAdapter);

				obj[SocialNetwork.AND_AMZ] = typeof(MobileAdapter);
				obj[SocialNetwork.AND_HW] = typeof(MobileAdapter);
				obj[SocialNetwork.AND_RU] = typeof(MobileAdapter);
				obj[SocialNetwork.AND_AMZ] = typeof(MobileAdapter);
				obj[SocialNetwork.AND_HW] = typeof(MobileAdapter);
				obj[SocialNetwork.IOS] = typeof(MobileAdapter);
				obj[SocialNetwork.WINDOWS_STORE] = typeof(WNDSAdapter);

				//obj[SocialNetwork.NONE] = typeof(AbstractSocialAdapter);

				obj[SocialNetwork.LOCAL] = typeof(LocalAdapter);

				return obj;
			}
		}

		public static AbstractSocialAdapter Create(string sn, SocialAdapterParams parameters)
		{
			if (parameters == null)
				throw new Exception("No arguments");

			Type adapterClass = null;

			var needAdapter = sn;

			if (parameters.IframeVars["isLocal"] != null)
			{
				var val = parameters.IframeVars["isLocal"].ToObject<bool>();
				needAdapter = val ? SocialNetwork.LOCAL : sn;
			}

			if (CREATORS.ContainsKey(needAdapter))
			{
				adapterClass = CREATORS[needAdapter];

				var adapter = Activator.CreateInstance(adapterClass, parameters) as AbstractSocialAdapter;

				return adapter;
			}
			else
			{
				GameLogger.warning("No adapter for sn " + sn);
			}

			return null;
		}
	}
}