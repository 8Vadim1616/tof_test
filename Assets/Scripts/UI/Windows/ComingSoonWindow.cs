using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class ComingSoonWindow : AbstractWindow
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _desc;
		[SerializeField] private ButtonText _btnOk;
		
		public static ComingSoonWindow Of() =>
						Game.Windows.ScreenChange<ComingSoonWindow>(false, x => x.Init());

		private void Init()
		{
			_title.text = "coming_soon_title".Localize();
			_desc.text = "coming_soon_desc".Localize();
			_btnOk.Text = "Ok";
			_btnOk.SetOnClick(Close);
		}
	}
}