using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class ForcedUpdateWindow : AbstractWindow
	{
		[SerializeField] private TextMeshProUGUI _attention;
		[SerializeField] private TextMeshProUGUI _message;
		[SerializeField] private ButtonText _btn;

		public static void Of() =>
			Game.Windows.ScreenChange<ForcedUpdateWindow>(true, x => x.Init());

		private void Init()
		{
			CanClose.Value = false;
			CanCloseByBackButton = false;

			_attention.text = "update_game_title".Localize();
			_message.text = "update_game_desc".Localize();

			_btn.Text = "update".Localize();
			_btn.onClick.RemoveAllListeners();
			_btn.onClick.AddListener(Game.UpdateBuild);
		}
	}
}
