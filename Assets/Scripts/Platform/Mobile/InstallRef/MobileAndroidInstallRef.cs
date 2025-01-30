using System;
using AndroidInstallReferrer;

namespace Assets.Scripts.Platform.Mobile.Ref
{
	public class MobileAndroidInstallRef : MobileInstallRef
	{
		private const string TAG = "[MobileAndroidInstallRef] ";

		public override void Init()
		{
			GameLogger.info(TAG + "init");
			
			//InstallReferrer.GetReferrer(OnGetRef);

#if UNITY_EDITOR
			// if (Game.User.Uid != "23")
			// 	OnGetRef(new InstallReferrerData("friend_23", "1.0", false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now));
#endif
		}

		private void OnGetRef(InstallReferrerData data)
		{
			if (data.IsSuccess)
			{
				UpdateRef(data.InstallReferrer);
			}
			else
			{
				GameLogger.warning(TAG + "got error: " + data.Error);
			}
		}

	}
}