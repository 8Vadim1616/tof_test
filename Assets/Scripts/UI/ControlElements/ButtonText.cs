using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class ButtonText : BasicButton
    {
        [SerializeField] protected TextMeshProUGUI text;

		public TextMeshProUGUI TextField => text;
		
		[SerializeField] protected TextMeshProUGUI text2;

		public TextMeshProUGUI TextField2 => text2;

        public string Text
        {
            get => text.text;
            set
            {
                text.text = value;
                Scripts.Utils.Utils.ForceRebuildLayoutOnNextFrame(text.transform.parent as RectTransform);
            }
        }
		
		public string Text2
		{
			get => text2.text;
			set
			{
				text2.text = value;
				Scripts.Utils.Utils.ForceRebuildLayoutOnNextFrame(text2.transform.parent as RectTransform);
			}
		}
	}
}