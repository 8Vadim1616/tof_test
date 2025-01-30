using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class MainScreenTab : BasicButton
	{
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _text;
		[SerializeField] private Image _backActive;
		[SerializeField] private Image _backInactive;

		private const float ANIM_TIME = 0.1f;

		public void SetTitle(string title) => _text.text = title;
		
		public void Select(bool anim)
		{
			(_icon.transform as RectTransform).DOAnchorPosY(50, anim ? ANIM_TIME : 0f);
			_text.transform.DOScale(Vector3.one, anim ? ANIM_TIME : 0f);
			_backActive.SetAlphaTween(1f, anim ? ANIM_TIME : 0f);
			_backInactive.SetAlphaTween(0f, anim ? ANIM_TIME : 0f);
		}

		public void Unselect(bool anim)
		{
			(_icon.transform as RectTransform).DOAnchorPosY(0, anim ? ANIM_TIME : 0f);
			_text.transform.DOScale(Vector3.zero, anim ? ANIM_TIME : 0f);
			_backActive.SetAlphaTween(0f, anim ? ANIM_TIME : 0f);
			_backInactive.SetAlphaTween(1f, anim ? ANIM_TIME : 0f);
		}
	}
}