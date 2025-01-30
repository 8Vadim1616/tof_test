using Febucci.UI;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
	[ExecuteAlways]
    public class DoubleTitleText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textFront;
        [SerializeField] private TextMeshProUGUI textBack;

        [SerializeField] private TextAnimatorPlayer txtAnimFront;
        [SerializeField] private TextAnimatorPlayer txtAnimBack;

        public void ShowText(string txt, bool instant = false)
        {
            if (!instant)
            {
                txtAnimFront.ShowText(txt);
                txtAnimBack.ShowText(txt);
            }
            else
            {
                textBack.text = txt;
                textFront.text = txt;
            }
        }

		public void RepeatShow()
		{
			txtAnimFront.ShowText(textFront.text);
			txtAnimBack.ShowText(textBack.text);
		}

        public void Update()
        {
            if (textBack.text != textFront.text)
            {
                textBack.text = textFront.text;
            }
        }
    }
}