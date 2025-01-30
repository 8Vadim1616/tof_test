using Assets.Scripts.UI.Utils;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Widgets
{
	public class TextHint : SimpleHintView
	{
		[SerializeField] protected RectTransform _leftPipka;
		[SerializeField] protected RectTransform _centerPipka;
		[SerializeField] protected RectTransform _rightPipka;
		[Space]
		[SerializeField] protected TextMeshProUGUI _title;
		[SerializeField] protected TextMeshProUGUI _message;
		[Space]
		[SerializeField] protected float _sideOffset = 90;

        protected override bool CloseOnAnyMouseDown => true;

		public void Show(Transform target, string message) =>
			Show(target, null, message);

		public void Show(Transform target, string title, string message)
		{
			Show();

			_title.text = title;
			_title.SetActive(!title.IsNullOrEmpty());

			_message.text = message;
			_message.SetActive(!message.IsNullOrEmpty());

			SetTarget(target);

			CheckPipka();

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_message.transform);
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }

		public void SetTarget(Transform target)
		{
			transform.position = target.position;
		}

		private void CheckPipka()
		{
			var canvas = Game.MainCanvas.transform as RectTransform;
			var hintSize = (transform as RectTransform).sizeDelta;
			var halfHintWidth = hintSize.x / 2 - _sideOffset;
			var screenWidth = canvas.rect.width;
			var targetX = canvas.InverseTransformPoint(transform.position).x;

			bool needLeft = false;
			bool needRight = false;

			if (targetX + hintSize.x / 2 > screenWidth / 2)
			{
				needLeft = true;
				transform.localPosition += new Vector3(-halfHintWidth, 0, 0);
			}
			else if (targetX - hintSize.x / 2 < -screenWidth / 2)
			{
				needRight = true;
				transform.localPosition += new Vector3(halfHintWidth, 0, 0);
			}

			_leftPipka.gameObject.SetActive(needRight);
			_rightPipka.gameObject.SetActive(needLeft);
			_centerPipka.gameObject.SetActive(!needLeft && !needRight);
		}
	}
}
