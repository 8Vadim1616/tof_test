using TMPro;
using UnityEngine;
using Assets.Scripts.Utils;
using DG.Tweening;
using System;

namespace Assets.Scripts.UI.Utils
{
	public class FloatingText : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI text;
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] Vector3 moveTargetVector;
		[SerializeField] Ease easing;

		private Sequence _sequence;

		public float TWEEN_DURATION = 2f;
		public float FADE_IN_DURATION = .5f;

		public bool FadeIn { get; set; }

		private bool _needFixPosition = true;

		private void Start()
		{
			_sequence?.Kill();

			var rectTransform = gameObject.GetComponent<RectTransform>();
			_sequence = DOTween.Sequence()
				.SetLink(gameObject)
				.SetEase(easing)
				.OnComplete(Destroy)
				.OnKill(Destroy);

			if (FadeIn)
				_sequence.Join(canvasGroup.DOFade(1, FADE_IN_DURATION).ChangeStartValue(0));

			_sequence
				.Join(rectTransform.DOLocalMove(rectTransform.localPosition + moveTargetVector, TWEEN_DURATION))
				.Insert(FadeIn ? FADE_IN_DURATION : 0, canvasGroup.DOFade(0, TWEEN_DURATION));

			if (_needFixPosition)
				FixPosition();
		}

		private void FixPosition()
		{
			var position = Game.MainCamera.WorldToScreenPoint(text.transform.position);
			var rect = (RectTransform) text.transform;
			var maxX = position.x + rect.sizeDelta.x / 2;
			var minX = position.x - rect.sizeDelta.x / 2;
			var cRect = (RectTransform) Game.MainCanvas.transform;
			var screenMaxX = cRect.sizeDelta.x;

			if (maxX > screenMaxX)
			{
				var delta = screenMaxX - maxX;
				position.x += delta;
				text.transform.position = Game.MainCamera.ScreenToWorldPoint(position);
			}
			else if (minX < 0)
			{
				position.x -= minX;
				text.transform.position = Game.MainCamera.ScreenToWorldPoint(position);
			}
		}

		private void Destroy()
		{
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			_sequence?.Kill();
		}

		public virtual string Text
		{
			get => text.text;
			set => text.text = value != null ? value : "second_tap_to_continue".Localize();
		}

		public void Kill()
		{
			Destroy(gameObject);
		}

		public static FloatingText SpawnFloatingText(Vector3 posGlobal, string txt, FloatingText prefabExtra = null)
		{
			var p = prefabExtra == null ? Game.BasePrefabs.PrefabFloatingText : prefabExtra;
			var inst = Instantiate(p, Game.HUD.HudTopLayer);
			inst.Text = txt;
			inst.transform.position = posGlobal;
			return inst;
		}

		public static FloatingText SpawnFloatingStillText(Vector3 posGlobal, string txt, FloatingText prefabExtra = null)
		{
			var inst = SpawnFloatingText(posGlobal, txt, prefabExtra);
			inst.moveTargetVector = Vector3.zero;
			inst.FadeIn = false;
			inst.TWEEN_DURATION = float.MaxValue;

			return inst;
		}

		public static FloatingText SpawnFloatingStillText(float viewPortX, float viewPortY, string txt, FloatingText prefabExtra = null)
		{
			viewPortX = viewPortX.Clamp(0, 1);
			viewPortY = viewPortY.Clamp(0, 1);

			var screenPointX = viewPortX * Screen.width;
			var screenPointY = viewPortY * Screen.height;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(Game.HUD.HudTopLayer,
			new Vector2(screenPointX, screenPointY), Game.MainCamera, out var worldP);

			//var posGlobal = Game.HUD.HudTopLayer.position + localP.toVector3();
			Debug.Log($"GLOBAL {Game.HUD.HudTopLayer.position } + {worldP} ");
			return SpawnFloatingStillText(worldP, txt, prefabExtra);
		}

		public FloatingText SetTextSize(float width, float height)
		{
			var rt = text.transform as RectTransform;
			rt.sizeDelta = new Vector2(width, height);
			_needFixPosition = false;
			return this;
		}
	}
}