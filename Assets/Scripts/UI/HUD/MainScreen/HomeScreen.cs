using Assets.Scripts.Gameplay;
using Assets.Scripts.UI.ControlElements;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class HomeScreen : MainScreenBase
	{
		public ButtonText PlayButton;
		
		private void Awake()
		{
			PlayButton.SetOnClick(() =>
			{
				//Game.Instance.Play();
				PlayfieldView.StartGame();
			});
		}
	}
}