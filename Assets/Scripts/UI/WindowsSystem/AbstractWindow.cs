using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.WindowsSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class AbstractWindow : MonoBehaviour
    {
        protected Animator animatorController;
        protected CanvasGroup canvasGroup;

        public ReactiveProperty<bool> CanClose { get; set; } = new ReactiveProperty<bool>(true);
        public bool CanCloseByBackButton { get; set; } = true;

        public virtual bool IsOnlyOnePrefab => true;

        // // По идее нужны для включения звуков
        // public event EventHandler<AbstractWindow> WindowStartedEnteringEvent;
        // public event EventHandler<AbstractWindow> WindowStartedLeavingEvent;

        public Promise ClosePromise { get; } = new Promise();
        public Promise OpenAnimEndPromise { get; } = new Promise();

        public WindowsController Controller { get; set; }

        public Dictionary<string, object> AdditionalLogParams { get; private set; }
        
        public virtual ServerLogsParams LogParams
        {
            get
            {
                ServerLogsParams serverLogParams = ServerLogsParams.OfWindow(this);

                if (AdditionalLogParams != null)
                    serverLogParams.AddCustomParams(AdditionalLogParams);

				// if (this is IOfferWindow offerWindow)
				// {
				// 	if (offerWindow.Offer != null)
				// 		serverLogParams.AddOffer(new[] {offerWindow.Offer});
				// }

                return serverLogParams;
            }
        }

		public IPromise OnFocus()
		{
			if (Game.Windows.CurrentScreen.Value == this)
				return Promise.Resolved();

			var result = new Promise();
			IDisposable sub = null;
			sub = Game.Windows.CurrentScreen.Subscribe(win =>
			{
				if (win == this)
				{
					sub?.Dispose();
					result.Resolve();
				}
			}).AddTo(this);
			
			return result;
		}

		public event EventHandler onCloseEvent;

        public virtual void SetAdditionalLogParams(Dictionary<string, object> obj)
        {
            if (AdditionalLogParams == null)
                AdditionalLogParams = obj;
            else
            {
                foreach (var kv in obj)
                {
                    AdditionalLogParams[kv.Key] = kv.Value;
                }
            }
        }

        public virtual string ClassName
        {
            get
            {
                return GetType().Name;
            }
		}

		protected IDisposable closeObserver;

		protected bool _isOpening = false;
        public bool IsOpening => _isOpening;

        protected bool _isClosing = false;
        public bool IsClosing => _isClosing;

		public virtual bool HideHudAll { get; } = false;

        // Переопределить чтобы не было анимации при появлении окна
        public virtual bool AnimationOnAppear { get; } = true;

        public const float OPEN_ANIM_DURATION = .23f;

        // Переопределить чтобы не было анимации при закрытии
        public virtual bool AnimationOnDisappear { get; } = true;
        public const float CLOSE_ANIM_DURATION = 0.23f;

        public virtual float LastCloseTime { get; set; }

        protected RectTransform content;
        protected RectTransform Content
        {
            get => content ??= transform.Find("Content").transform as RectTransform;
            set => content = value;
        }

        /// <summary>Проигрывает действие "Назад" в окне, либо закрывает его.</summary>
        public virtual void Back()
        {
			if (Game.Locker?.IsLocked.Value != true)
                Close();
        }
        
		/// <summary>Переопределять только в крайних случаях, т.к. тут используется проверка на CanClose</summary>
        public virtual void Close()
        {
            if (!CanClose.Value) return;

			if (_isClosing)
				return;
			_isClosing = true;

			if (!Controller)
				Controller = Game.Windows;

			if (Controller != null)
				Controller.RemoveFromController(this);

			OnBeforeClose();
			StartHide();
			OnClose();

			onCloseEvent?.Invoke(this, EventArgs.Empty);
			ClosePromise.ResolveOnce();
		}

        // Закрывает окно либо сразу либо когда CanClose становится доступным
        public void CloseWhenCan()
        {
            if (CanClose.Value)
            {
                Close();
                return;
            }

            closeObserver?.Dispose();
            closeObserver = CanClose.Subscribe(x =>
            {
                if (x)
                {
                    Close();
                }
            }).AddTo(this);
        }

        #region Unity Actions

        protected void Awake()
        {
            animatorController = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();

            //ResetContent();
            OnAwake();
        }

        protected virtual void OnShow()
        {
        }

		protected Tween AppearAnimTween;
		protected virtual void Start()
        {
            OnStart();
            _isOpening = true;

            if (AnimationOnAppear && Content)
            {
				if (animatorController != null)
				{
					SetInteractiveState(false);
					animatorController.PlayAnimation("Open")
						.Then(() => { SetInteractiveState(true); })
						.Finally(OnAnimationEnd);
                }
                else
				{
					SetInteractiveState(false);
					AppearAnimTween = GetAppearAnimation()
						.AppendCallback(() => SetInteractiveState(true))
						.OnComplete(OnAnimationEnd)
						.OnKill(OnAnimationEnd);
					//seq.Play();
				}
            }
            else
            {
                OnAnimationEnd();
            }
        }

        #endregion
        protected virtual Sequence GetAppearAnimation()
        {
			return AbstractWindow.GetScaleAppearAnimation(Content, gameObject, canvasGroup);
        }

        protected virtual Sequence GetCloseAnimation()
        {
            var animSequence = DOTween.Sequence();

            if (canvasGroup) 
				animSequence.Insert(0, canvasGroup.DOFade(0, CLOSE_ANIM_DURATION));
            else
				animSequence.Insert(0, transform.DOScale(0, CLOSE_ANIM_DURATION));

            animSequence.SetLink(gameObject);

            return animSequence;
        }

        #region Virtual

        protected virtual void OnAnimationEnd()
        {
			if (!_isOpening)
				return;
			
			OpenAnimEndPromise.ResolveOnce();
            OnShow();

            _isOpening = false;
        }

        protected virtual void OnStart()
        {
            //WindowStartedEnteringEvent?.Invoke(this, this);

           // Game.HUD?.builder.HideHint();

            OnShowPlaySound();
        }

        protected virtual void OnAwake() { }

		protected virtual void OnHideStart() { }

        protected virtual void OnHideEnd()
        {
            if (gameObject)
                Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            AppearAnimTween?.Kill();
        }

        protected virtual void OnShowPlaySound()
		{
			Game.Sound.WinOpen();
		}

        protected virtual void OnHidePlaySound()
        {
			Game.Sound.WinClose(); 
        }

        protected virtual void SetInteractiveState(bool interactive)
        {
            //GameLogger.debug(name + " interactive = " + interactive);
            if (canvasGroup) canvasGroup.interactable = interactive;
        }

		/// <summary>
		/// Вызывается если окно прошло проверку CanClose и закрыли в Close
		/// </summary>
		protected virtual void OnClose() { }

		protected virtual void OnBeforeClose() { }

		/// <summary>
        /// Проигрываем анимацию закрытия и уничтожаем окно
        /// </summary>
        protected void StartHide()
        {
            /*if (_isClosing) 
				return;
            _isClosing = true;*/

            OnHideStart();

            SetInteractiveState(false);
            if (canvasGroup) 
				canvasGroup.blocksRaycasts = false;
            OnHidePlaySound();

			if (AnimationOnDisappear)
			{
				if (animatorController && animatorController.runtimeAnimatorController)
				{
					animatorController
						.PlayAnimation("Close")
						.Finally(Destroy);
				}
				else
				{
					GetCloseAnimation()
						.Play()
						.OnComplete(Destroy);
				}
			}
			else
				Destroy();
        }

		#endregion
        /// <summary>
        /// Уничтожает обьект через событие в WindowEndAnimationBehaviour по достижении определенного стейта в аниматоре
        /// Чтобы 
        /// </summary>
        protected void Destroy()
        {
			OnHideEnd();
        }

        #region Public

        protected Tween minimizeTween;
        protected const float MINIMIZE_TIME = .25f;
        public virtual IPromise Minimize()
		{
			if (_isClosing)
				return Promise.Resolved();

			canvasGroup.blocksRaycasts = false;
			IsMinimized.Value = true;
			minimizeTween?.Kill();

			if (canvasGroup.alpha.CloseTo(0)) 
				return Promise.Resolved();

            var promise = new Promise();

            //if (animatorController)
            //{
            //    animatorController.SetBool("Minimized", true);
            //    DOVirtual.DelayedCall(MINIMIZE_TIME, () => promise.Resolve());
            //}
            //else
            {
                minimizeTween = canvasGroup.DOFade(0, MINIMIZE_TIME)
										   .SetLink(gameObject)
										   .OnComplete(() => promise.Resolve());
            }

            return promise;
            //canvasGroup.DOFade(0, FADE_DURATION);
        }

        //public bool IsMinimized => canvasGroup.alpha.CloseTo(0);
        public ReactiveProperty<bool> IsMinimized { get; } = new ReactiveProperty<bool>();

        public virtual IPromise Maximize()
		{
			canvasGroup.blocksRaycasts = true;

            IsMinimized.Value = false;
			minimizeTween?.Kill();

			if (canvasGroup.alpha.CloseTo(1))
                return Promise.Resolved();

            var promise = new Promise();
            //if (animatorController)
            //{
            //    animatorController.SetBool("Minimized", false);
            //    DOVirtual.DelayedCall(MINIMIZE_TIME, () => promise.Resolve());
            //}
            //else 
            {
                minimizeTween = canvasGroup.DOFade(1, MINIMIZE_TIME)
										   .SetLink(gameObject)
										   .OnComplete(() => promise.Resolve());
            }

            return promise;
        }

        

        #endregion

		public static Sequence GetScaleAppearAnimation(RectTransform content, GameObject gameObject, CanvasGroup canvasGroup)
		{
			//content.localScale = Vector3.zero;
			var fadeInDuration = .25f;

			return DOTween.Sequence()
						  .SetLink(gameObject)
						  //.Append(content.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack).SetLink(gameObject))
						  .Join(content.DOFade(1, fadeInDuration, 0))
						  .Join(canvasGroup.DOFade(1, fadeInDuration).ChangeStartValue(0));
		}
    }

    public enum WindowState
    {
        Open,
        Process,
        Close
    }
}
