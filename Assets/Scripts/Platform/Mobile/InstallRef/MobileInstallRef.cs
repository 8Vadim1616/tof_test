using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Ref
{
	public class MobileInstallRef
	{
		private const string TAG = "[MobileInstallRef] ";

		public virtual void Init()
		{
			GameLogger.info(TAG + "init");
		}

		public void UpdateRef(string newRef)
		{
			Debug.Log(TAG + "Received Install Ref: " + newRef);

			if (!string.IsNullOrEmpty(newRef))
			{
				if (Game.User != null)
					Game.User.RegisterData.InstallRef = newRef;
			}
		}
	}
}