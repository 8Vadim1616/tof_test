using Assets.Scripts.UI.ControlElements;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class SummonPanelView : MonoBehaviour
	{
		[SerializeField] private BasicButton _btnClose;

		private void Awake()
		{
			_btnClose.SetOnClick(() => gameObject.SetActive(false));
		}
	}
}