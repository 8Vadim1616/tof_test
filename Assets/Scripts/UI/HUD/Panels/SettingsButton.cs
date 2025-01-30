using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Windows;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class SettingsButton : BasicButton
	{
		public override void OnAwake()
		{
			SetOnClick(() => SettingsWindow.Of());
		}
	}
}