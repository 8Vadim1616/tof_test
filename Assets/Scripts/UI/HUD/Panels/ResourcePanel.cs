using System;
using System.Linq;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Panels
{
	public abstract class ResourcePanel : MonoBehaviour, IItemDropAnimated
	{
		[SerializeField] protected TextMeshProUGUI amount;
		[SerializeField] protected BasicButton btnAdd;
		[SerializeField] protected Image icon;

		public Vector3 FloatingPosition => icon.transform.position;

		private Transform _parent;

		public abstract LongReactiveProperty Property { get; }
		private float? lastX;

		private Tween iconBlinkTween;
		private float iconScaleMax = 1.2f;
		private float iconScaleMin = .8f;
		private float iconScaleTime = .5f;

		private Sequence appearSeq;
		private int _transformIndex;

		public void SetupButtonClicks(Action onClick)
		{
			if (btnAdd)
			{
				btnAdd.onClick.RemoveAllListeners();
				btnAdd.onClick.AddListener(() => onClick?.Invoke());
			}
		}

		private void Awake()
		{
			_parent = transform.parent;
			_transformIndex = transform.GetSiblingIndex();

			Property?
				.Subscribe(value => SetAmount(value))
				.AddTo(this);

			OnAwake();
		}

		protected virtual void SetAmount(float count, bool anim = true)
		{
			amount.text = count.ToKiloFormat();

			if (anim && lastX.HasValue && lastX < count)
			{
				if (iconBlinkTween?.active != true)
				{
					iconBlinkTween = DOTween.Sequence()
						.SetLink(gameObject)
						.Append(icon.transform.DOScale(iconScaleMax, iconScaleTime / 4))
						.Append(icon.transform.DOScale(iconScaleMin, iconScaleTime / 2))
						.Append(icon.transform.DOScale(1f, iconScaleTime / 4));
				}
			}

			lastX = count;
		}

		protected virtual void OnAwake() { }

		public virtual void SetButtonsNotClickable()
		{
			btnAdd.onClick.RemoveAllListeners();
			btnAdd.enabled = false;
			var scaleComponent = btnAdd.gameObject.GetComponent<ButtonInnerScaleAnim>();
			if (scaleComponent)
				Destroy(scaleComponent);
		}

		public void AppearTween(float iconStart, float iconDuration, float btnStart, float btnDuration)
		{
			appearSeq?.Kill();
			appearSeq = DOTween.Sequence().SetLink(gameObject);
			appearSeq.InsertCallback(0, () =>
			{
				icon.transform.localScale = 0f.toVector3();
				if (btnAdd)
					btnAdd.transform.localScale = 0f.toVector3();
			});

			appearSeq.Insert(iconStart, icon.transform.DOScale(1, iconDuration).SetEase(Ease.OutCubic));
			if (btnAdd)
				appearSeq.Insert(btnStart, btnAdd.transform.DOScale(1, btnDuration).SetEase(Ease.OutCubic));
			appearSeq.Play();
		}

		public void AddToWindow()
		{
			transform.SetParent(Game.Windows.HOLDER);

			Canvas canvas = gameObject.GetComponent<Canvas>();
			if (!canvas)
				canvas = gameObject.AddComponent<Canvas>();

			canvas.overrideSorting = true;
			//canvas.sortingLayerID = SortingLayer.NameToID("Foreground");
			canvas.sortingOrder = 200;
		}

		public void AddBackToHud()
		{
			transform.SetParent(_parent);
			transform.SetSiblingIndex(_transformIndex);

			Canvas canvas = gameObject.GetComponent<Canvas>();
			if (!canvas)
				return;

			Destroy(canvas);
		}


		public void OnItemDropArrival()
		{
		}

		public Vector3 GetPositionGlobal()
		{
			return icon.transform.position;
		}
	}
}
