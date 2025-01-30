using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class NoConnectionPanel : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _text;

		private void Start()
		{
			_text.text = "no_connection".Localize();
		}
	}
}
