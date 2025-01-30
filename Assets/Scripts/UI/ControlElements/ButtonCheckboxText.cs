using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	public class ButtonCheckboxText : ButtonText
	{
		[SerializeField] private Image _buttonImage;
		[SerializeField] private Sprite _uncheckedImage;

		private Sprite _checkedImage;
		private bool _inited;

		public bool IsChecked { get; private set; }

		public override void OnAwake()
		{
			if (_buttonImage)
				_checkedImage = _buttonImage.sprite;
			SetChecked(false);
			_inited = true;
		}

		public void SetChecked(bool isChecked)
		{
			if (_inited && isChecked == IsChecked)
				return;
			IsChecked = isChecked;
			OnChecked();
		}

		public virtual void OnChecked()
		{
			_buttonImage.sprite = IsChecked ? _checkedImage : _uncheckedImage;
		}
	}
}
