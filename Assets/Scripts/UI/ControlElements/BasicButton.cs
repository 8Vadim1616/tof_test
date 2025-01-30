using Assets.Scripts.UI.Utils;
using Assets.Scripts.User.Ad;
using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Castle.Core.Internal;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	public class BasicButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IItemDropAnimated
	{
		private const float DOUBLE_CLICK_DELAY = 2f;

		[SerializeField] private Button button;
		[SerializeField] private ParticleSystem effect;
		[SerializeField] private Transform ItemDropLocalPos;
		[SerializeField] private ParticleSystem dropParticleSystem;
		[SerializeField] private List<GameObject> enabled;
		[SerializeField] private List<GameObject> disabled;

		public bool NeedAlphaOnDisabled = false;
		public bool NeedAnimateOnClick = true;
		public bool NeedAnimateOnUnPress = true;
		public bool NeedGrayOnDisabled = true;

		[SerializeField] private float scaleX = .95f;
		[SerializeField] private float scaleY = .95f;

		public Button Button => button;
		private RectTransform rectTransform;
		public RectTransform RectTransform
		{
			get
			{
				if (!rectTransform)
					rectTransform = GetComponent<RectTransform>();
				return rectTransform;
			}
		}

		private Button.ButtonClickedEvent _onClick = new Button.ButtonClickedEvent();
        public Button.ButtonClickedEvent onClick => button ? button.onClick : _onClick;

        private CanvasGroup group;
        public CanvasGroup CanvasGroup => group;
        public ParticleSystem Effect => effect;

		private event Action OnDownCallback;
		private event Action OnDoubleClickButton;
		private event Action OnDoubleDownCallback;

		private Tween doubleClickTween = null;
		private IDisposable DoubleClickSubscription;

        /** Наложен материал **/
        public bool Locked { get; private set; } = false;

        private bool touchable = true;
        /** Кликабельность по кнопке **/
        public bool Touchable
        {
            get => touchable;
            set
            {
                touchable = value;

                if (button)
                    button.interactable = touchable;
            }
        }



		public void ResetDoubleClick()
        {
            doubleClickTween?.Kill();
            doubleClickTween = null;
        }

        public bool Enabled
        {
            get => !Locked;
            set
            {
                SetLock(!value);
            }
        }

		private bool _firstLockPass;

        public void SetLock(bool val, bool touchable = false)
        {
            if (Locked == val && _firstLockPass) 
				return;
			_firstLockPass = true;
            Locked = val;

            Touchable = val ? touchable : true; // При снятии лока всегда возвращаем кликабельность кнопки

            if (gameObject && NeedGrayOnDisabled)
                gameObject.SetEnabled(!Locked, NeedAlphaOnDisabled);
			
			if (!enabled.IsNullOrEmpty())
				enabled.Each(g => g.SetActive(!Locked));
			if (!disabled.IsNullOrEmpty())
				disabled.Each(g => g.SetActive(Locked));
        }

        private Vector3 baseScale;

        private void SetEffect(bool appear)
        {
            if (!Effect) return;
            if (!appear)
                Effect.gameObject.SetActive(false);
            else
            {
                Effect.gameObject.SetActive(true);
                Effect.Play();
            }
        }

        private void OnDisable()
        {
            SetEffect(false);
        }


        private Action resetAction;
        private Tween appearTween;
        private void AppearDisappear(bool appear, bool needEffect = false, float duration = .25f)
        {
			if (!gameObject)
				return;

            resetAction?.Invoke();
            appearTween?.Kill();

            Touchable = appear;

            gameObject.SetActive(appear);
            SetEffect(needEffect && appear);

            if (CanvasGroup)
            {
                var alpha = appear ? 1f : 0f;
                //Debug.Log($"{gameObject.name} {CanvasGroup.alpha} => {alpha} for {duration}s");
                appearTween = 
                    /*DOTween.To(() => CanvasGroup.alpha, x =>
                {
                    CanvasGroup.alpha = x;
                    Debug.Log($"{gameObject.name} {CanvasGroup.alpha} {x}");
                }, alpha, duration).SetEase(Ease.InQuart).SetLink(gameObject);*/
                CanvasGroup.DOFade(alpha, duration).SetEase(Ease.InSine).SetLink(gameObject);

                if (!appear)
                    appearTween.OnComplete(() => gameObject.SetActive(false));

                resetAction = () =>
                {
                    appearTween?.Kill();
                    if (!appear) gameObject.SetActive(false);
                    if (CanvasGroup) CanvasGroup.alpha = alpha;
                };
            }
        }

        public void ShowEffect()
        {
            IsVisible = true;
            SetEffect(true);
        }

        public void Awake()
        {
            if (!button) button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
            baseScale = rectTransform.localScale;
            group = GetComponent<CanvasGroup>();
            if (effect) effect.gameObject.SetActive(false);

			if (dropParticleSystem)
			{
				dropParticleSystem.gameObject.SetActive(false);
				dropParticleSystem.Stop();
			}

            OnAwake();
			Debug.Log("a");
		}

        public virtual void OnAwake() { }
        
        protected Sequence selectTween;

		public virtual void OnSelectAnimation()
		{
			if (!this || !gameObject || !rectTransform)
				return;

			selectTween?.Kill();
			selectTween = DOTween.Sequence()
				.SetLink(gameObject)
				.Append(rectTransform.DOScale(new Vector3(baseScale.x * 0.95f, baseScale.y * 1.01f, baseScale.z), 0.05f))
				.Append(rectTransform.DOScale(new Vector3(baseScale.x * 1.05f, baseScale.y * 0.9f, baseScale.z), 0.05f))
				.Append(rectTransform.DOScale(new Vector3(baseScale.x * 0.95f, baseScale.y * 1.01f, baseScale.z), 0.05f))
				.Append(rectTransform.DOScale(baseScale, 0.05f))
				.OnComplete(() => selectTween = null);
			selectTween.Play();
		}

        private Sequence pressTween;
        public virtual void OnPressAnimationStart()
        {
            if (!gameObject) return;

            if (pressTween != null)
            {
                pressTween.Rewind();
                pressTween.Play();
                return;
            }

            pressTween = DOTween.Sequence()
                .Append(rectTransform.DOScale(new Vector3(baseScale.x * scaleX, baseScale.y * scaleY, baseScale.z), 0.05f));

            pressTween.SetLink(gameObject);
            pressTween.SetAutoKill(false);

            Vector2 startPivot = rectTransform.pivot;
            //pressTween.OnStart(OnStart);
            //pressTween.OnRewind(OnComplete);
            //pressTween.OnComplete(OnComplete);
            //pressTween.OnKill(OnComplete);
            // чет не работает эта фигня
            void OnStart()
            {
                startPivot = rectTransform.pivot;
                rectTransform.pivot = new Vector2(.5f, 0);
            }
            void OnComplete()
            {
                rectTransform.pivot = startPivot;
            }

            pressTween.Play();
        }

		public void SetOnDownCallback(Action callback)
		{
			OnDownCallback = callback;
		}

		public void SetOnDoubleClick(Action callback, string text = null)
		{
			//var delay = 1f;

			onClick.RemoveAllListeners();

			DoubleClickSubscription?.Dispose();
			DoubleClickSubscription = null;
			DoubleClickSubscription = this.OnClickAsObservable().Subscribe(x =>
			{
				if (doubleClickTween == null)
				{
					PlaySoundClick();
					var floatingText = Instantiate(Game.BasePrefabs.PrefabFloatingText, transform);
					floatingText.Text = text;
					doubleClickTween = DOVirtual.DelayedCall(DOUBLE_CLICK_DELAY, ResetDoubleClick);
				}
				else
				{
					ResetDoubleClick();
					callback?.Invoke();
					OnDoubleClickButton?.Invoke();
				}
			}).AddTo(this);
		}

		public void SetOnDoubleDownCallback(Action callback)
		{
			OnDoubleDownCallback = callback;

			DoubleClickSubscription?.Dispose();
			DoubleClickSubscription = null;
			DoubleClickSubscription = this.OnClickAsObservable().Subscribe(ddx =>
			{
				if (doubleClickTween == null)
				{
					PlaySoundClick();
					var floatingText = Instantiate(Game.BasePrefabs.PrefabFloatingText, transform);
					doubleClickTween = DOVirtual.DelayedCall(DOUBLE_CLICK_DELAY, ResetDoubleClick);
				}
				else ResetDoubleClick();
			}).AddTo(this);
		}

		public virtual void OnPressAnimationFinished()
        {
            if (!gameObject) return;

            if (pressTween != null)
            {
                pressTween.Rewind();
                //pressTween.Play();
                return;
            }
        }

        public virtual void StartBlinkAnimation(float duration = .4f)
        {
            if (!gameObject || !rectTransform) return;
            selectTween?.Kill();

            //Нужна не вырвиглазная анимация
            selectTween = DOTween.Sequence();
            selectTween.Append(rectTransform.DOScale(new Vector3(baseScale.x * 0.95f, baseScale.y * 1.01f, baseScale.z), duration / 4));
            selectTween.Append(rectTransform.DOScale(baseScale, duration / 4));
            selectTween.Append(rectTransform.DOScale(new Vector3(baseScale.x * 1.05f, baseScale.y * 0.9f, baseScale.z), duration / 4));
            selectTween.Append(rectTransform.DOScale(baseScale, duration / 4));
            selectTween.SetLoops(-1);
            selectTween.OnComplete(() => selectTween = null);
            selectTween.SetLink(gameObject);
			selectTween.SetAutoKill(false);
            selectTween.Play();
        }

        public virtual void StartGrowAnimation()
        {
            if (!gameObject || !rectTransform) return;
            selectTween?.Kill();

            //Нужна не вырвиглазная анимация
            selectTween = DOTween.Sequence();
            selectTween.Append(rectTransform.DOScale(new Vector3(baseScale.x * 1.05f, baseScale.y * 1.05f, baseScale.z), 0.5f));
            selectTween.Append(rectTransform.DOScale(baseScale, 0.5f));
            selectTween.SetLoops(-1);
            selectTween.OnComplete(() => selectTween = null);
            selectTween.SetLink(gameObject);
            selectTween.Play();
        }


        protected virtual void OnDestroy()
        {
            selectTween?.Kill();
            selectTween = null;
        }


        private bool isVisible = true;
		private Action _soundClickCallback;

		public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                SetVisible(value);
            }
        }
        public void SetVisible(bool visible)
        {
            isVisible = visible;
            AppearDisappear(visible);
        }

        public void StopAnim()
        {
            selectTween?.Kill();
            selectTween = null;
        }

		public void SetBaseScale()
		{
			if (!gameObject || !rectTransform)
				return;

			transform.localScale = baseScale;
		}


		public void SetSound(Action soundClickCallback)
		{
			_soundClickCallback = soundClickCallback;
		}

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        internal void PlaySoundClick()
		{
			if (_soundClickCallback != null)
				_soundClickCallback?.Invoke();
			else
				Game.Sound.PlayBasicButtonClick();
        }

		private bool _wasPointerDown = false;
		private bool _wasPointerExit = false;

        public void OnPointerDown(PointerEventData eventData)
		{
			if (!Touchable)
				return;

			_wasPointerDown = true;
			_wasPointerExit = false;

			OnDownCallback?.Invoke();

			if (doubleClickTween != null)
				OnDoubleDownCallback?.Invoke();

            if (NeedAnimateOnClick)
                OnPressAnimationStart();
        }

		public void SetOnClickAndDoubleClick(Action<FloatingText> onFirstClick, Action onDoubleClick, string text = null)
		{
			//var delay = 1f;

			onClick.RemoveAllListeners();
			DoubleClickSubscription = this.OnClickAsObservable().Subscribe(x =>
			{
				if (doubleClickTween == null)
				{
					PlaySoundClick();
					var floatingText = Instantiate(Game.BasePrefabs.PrefabFloatingText, transform);
					floatingText.Text = text; // После первого
					doubleClickTween = DOVirtual.DelayedCall(DOUBLE_CLICK_DELAY, ResetDoubleClick);
					onFirstClick.Invoke(floatingText);
				}
				else
				{
					ResetDoubleClick();
					onDoubleClick?.Invoke();
					OnDoubleClickButton?.Invoke();
				}
			}).AddTo(this);
		}

		public void SetOnClick(Action onClick)
		{
			this.onClick.RemoveAllListeners();
			this.onClick.AddListener(() => onClick?.Invoke());
		}

		public void OnPointerUp(PointerEventData eventData)
        {
			_wasPointerDown = false;

			if (_wasPointerExit)
			{
				_wasPointerExit = false;
				if (NeedAnimateOnClick)
					OnPressAnimationFinished();
				return;
			}

			//float dragDistance = Vector2.Distance(eventData.pressPosition, eventData.position);
			//var dragThreshold = Screen.dpi / 12;
			var isDrag = eventData.dragging;//dragDistance > dragThreshold;
 
            //Брать приходится каждый раз, т.к. парент у кнопок может поменятся
            var parentCanvas = GetComponentInParent<CanvasGroup>();
            if (parentCanvas != null && !parentCanvas.interactable)
				return;

			if (!Touchable)
			{
				if (NeedAnimateOnClick)
					OnPressAnimationFinished();
				return;
			}

            if (isDrag)
            {
                if (NeedAnimateOnClick) 
                    OnPressAnimationFinished();
                return;
            }

			if (NeedAnimateOnClick)
			{
				if (NeedAnimateOnUnPress)
					OnSelectAnimation();
				else
					OnPressAnimationFinished();
			}

            if (!button)
                _onClick?.Invoke();

            PlaySoundClick();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!_wasPointerDown)
				return;

			_wasPointerExit = false;
			if (NeedAnimateOnClick)
				OnPressAnimationStart();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!_wasPointerDown)
				return;

			_wasPointerExit = true;
			if (NeedAnimateOnClick)
				OnPressAnimationFinished();
		}

		public const float animDropParticleTime = 1f;
		private IDisposable stopTimer;
		public void OnItemDropArrival()
		{
			if (dropParticleSystem)
			{
				if (!dropParticleSystem.gameObject.activeSelf)
				{
					dropParticleSystem.gameObject.SetActive(true);
					dropParticleSystem.Clear();
					dropParticleSystem.Play();
				}

				RestartStopTimer();
			}

			void RestartStopTimer()
			{
				stopTimer?.Dispose();
				stopTimer = Observable.Timer(TimeSpan.FromSeconds(animDropParticleTime))
									  .Subscribe(_ => ParticlesStop()).AddTo(this);
			}

			void ParticlesStop()
			{
				stopTimer?.Dispose();
				stopTimer = null;

				dropParticleSystem.gameObject.SetActive(false);
				dropParticleSystem.Stop();
			}

			OnSelectAnimation();
		}

		public Vector3 GetPositionGlobal()
		{
			return ItemDropLocalPos ? ItemDropLocalPos.transform.position : transform.position;
		}

		private enum AdButtonState
		{
			None, Hidden, Loading, Active
		}

		private IDisposable adUpdate;
		public void SetupAdRewardButton(UserAdType t, Action<AdOptions> onRewardDone, Action onFail, Func<bool> canShowReward = null)
		{
			var b = this;
			var canv = b.gameObject.GetOrAddComponent<CanvasGroup>();

			AdButtonState GetButtonState()
			{
				var canShow = canShowReward?.Invoke();
				if (canShow == false)
					return AdButtonState.Hidden;

				var adPoint = Game.User.Ads.GetUserAdPoint(t);
				var hasAd = adPoint != null && adPoint.IsAvailableReward();

				if (!hasAd) return AdButtonState.Hidden;

				var loaded = adPoint.IsLoadedReward();
				return loaded ? AdButtonState.Active : AdButtonState.Loading;
			}

			AdButtonState lastState = AdButtonState.None;

			void CheckButtonState()
			{
				var state = GetButtonState();
				if (state == lastState) return;
				lastState = state;

				var visible = state == AdButtonState.Loading || state == AdButtonState.Active;
				canv.alpha = visible ? 1 : 0;

				if (state == AdButtonState.Active)
				{
					b.onClick.RemoveAllListeners();
					b.onClick.AddListener(OnClick);
				}
				else
				{
					b.onClick.RemoveAllListeners();
					if (state == AdButtonState.Loading)
						b.SetOnClickAndDoubleClick(null, null, "ad_loading".Localize());
				}

				b.Locked = state != AdButtonState.Active;
			}

			CheckButtonState();
			adUpdate?.Dispose();
			adUpdate = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => CheckButtonState()).AddTo(b.gameObject);

			void OnClick()
			{
				Game.AdvertisingController.ShowAdPoint(t, options => onRewardDone?.Invoke(options), onFail);
			}

		}

		public void StopRewardAdButton()
		{
			adUpdate?.Dispose();
			onClick.RemoveAllListeners();
		}
	}

    public static class RXBasicButtonExtension
    {
        public static IObservable<Unit> OnClickAsObservable(this BasicButton button)
        {
            return button.onClick.AsObservable();
        }
    }
}


