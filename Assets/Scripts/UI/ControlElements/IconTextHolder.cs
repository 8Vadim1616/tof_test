using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	public class IconTextHolder : TextHolder
	{
		[SerializeField] Image _icon;

		public Image Icon => _icon;
	}
}