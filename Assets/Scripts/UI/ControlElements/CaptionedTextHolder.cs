using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class CaptionedTextHolder : TextHolder
	{
		[SerializeField] TextMeshProUGUI _caption;

		public string Caption
		{
			get => _caption.text;
			set => _caption.text = value;
		}
	}
}