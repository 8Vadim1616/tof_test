using Assets.Scripts.Gameplay;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Panels
{
	public abstract class ResourcePanelInGame : MonoBehaviour
	{
		[SerializeField] protected TextMeshProUGUI amount;
		[SerializeField] protected Image icon;

		public FloatReactiveProperty Property { get; protected set; }
		
		private float? lastX;

		private Tween iconBlinkTween;
		private float iconScaleMax = 1.2f;
		private float iconScaleMin = .8f;
		private float iconScaleTime = .5f;

		private Sequence appearSeq;
		private int _transformIndex;

		protected abstract void OnPlayfieldChanged(PlayfieldView playfieldView);

		private void Awake()
		{
			Game.Instance.Playfiled.Subscribe(_ =>
			{
				var playfield = Game.Instance.Playfiled.Value;

				if (!playfield)
					return;

				OnPlayfieldChanged(playfield);
			}).AddTo(this);
			
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
	}
}
