using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class GDPRWindow
	{
		public static IPromise GDPRCheck()
		{
			var isMobile = Game.Social.IsMobile;
			var regData = Game.User.RegisterData;
			var needGDPR = isMobile && !regData.IsGDPRConfirmed;
			var isGDPRForbidden = (regData.NeedShowGDPR ?? Game.Settings.GDPRDefaultValue) == false;

			if (!needGDPR)
				return Promise.Resolved();
			
			if (isGDPRForbidden)
			{
				SetAccepted();
				return Promise.Resolved();
			}

			return Of();
		}

		private static IPromise Of()
		{
			var promise = new Promise();

			ServerLogs.SendLog("winOpen", new Dictionary<string, object> {{"win", "GDPRWindow"}});

			var dialog = new TermsOfServiceDialog()
						.SetTermsOfServiceLink(Game.Settings.GDPR_TERM1_HREF, () => ServerLogs.GDPRClick("term"))
						.SetPrivacyPolicyLink(Game.Settings.GDPR_TERM2_HREF, () => ServerLogs.GDPRClick("policy"));

			SimpleGDPR.ShowDialog(dialog, OnDialogClosed);

			return promise;

			void OnDialogClosed()
			{
				if (SimpleGDPR.IsTermsOfServiceAccepted)
				{
					ServerLogs.GDPRClick("accept");
					SetAccepted();
					promise.ResolveOnce();
				}
				else
					Game.Quit();
			}
		}

		private static void SetAccepted()
		{
			Game.User.RegisterData.IsGDPRConfirmed = true;
		}

		public static void OpenTermsOfService()
		{
			ServerLogs.GDPRClick("term");
			Application.OpenURL(Game.Settings.GDPR_TERM1_HREF);
		}

		public static void OpenPrivacyPolicy()
		{
			ServerLogs.GDPRClick("policy");
			Application.OpenURL(Game.Settings.GDPR_TERM2_HREF);
		}
	}
}