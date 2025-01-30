using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Animations
{
    public abstract class AbstractAnimationController : MonoBehaviour
    {
        public AnimationData LastPlayingAnimation { get; protected set; }
        private AnimationData _currentPlayingAnimation;

        protected AnimationData currentPlayingAnimation
        {
            get => _currentPlayingAnimation;
            set
            {
                _currentPlayingAnimation = value;
                if (value != null) LastPlayedAnimation = value;
            }
        }

        public AnimationData CurrentPlayingAnimation => currentPlayingAnimation;

        public AnimationData LastPlayedAnimation { get; private set; }
        
        public Promise currentPlayAnimationPromise;

        public abstract bool PlayOnInvisible { get; set; }

        public virtual IPromise PlaySequence(IList<AnimationData> animations, bool loopLast = false)
        {
            IPromise result = Promise.Resolved();

            for (var i = 0; i < animations.Count; i++)
            {
                var j = i;
                result = result.Then(() => Play(animations[j], loopLast && j == animations.Count - 1));
            }

            return result;
        }

        public abstract void SetChildVisible(string boneName, bool visible);

        public abstract void SetChildVisibleByPattern(string boneNamePattern, bool visible);

        public abstract Vector2 GetBoneLocalPosition(string boneName);
        public abstract bool HasBone(string boneName);

        public abstract IPromise Play(AnimationData animation, bool loop = false);

        public void GoToRandomTime()
        {
            GoToTime(Random.Range(0, 1f));
        }

        /// <summary>
        /// Перейти к определённому времени
        /// </summary>
        /// <param name="val">0 - начало, 1 - конец</param>
        public abstract void GoToTime(float val);
        /// <summary>
        /// Перейти к определённому времени и остановиться
        /// </summary>
        /// <param name="val">0 - начало, 1 - конец</param>
        public abstract void StopOnTime(float val = 0f);

        public abstract void Pause();

        public abstract void Resume();

        public abstract bool HasAnimation(AnimationData animation);

        public abstract bool Enabled { get; set; }

        public abstract float Alpha { get; set; }

        public abstract Color Color { set; }

        public abstract void SetTrigger(AnimationData trigger);

        public abstract void SetInteger(AnimationData name, int value);
        
        public abstract void SetFloat(AnimationData name, float value);

        public abstract void SetBoolean(AnimationData name, bool value);

        public abstract IPromise OnCompleteCurrentAnimation();

        public abstract void AddSkin(string value, string customName = "");
        public abstract void RemoveSkin(string value);
        public abstract void ClearSkin();
        public abstract void UpdateSkin();
        public abstract string[] GetAnimations();

        public void OnDestroy()
        {
            if (currentPlayAnimationPromise?.IsPending == true)
                currentPlayAnimationPromise.ResolveOnce();
        }

        public virtual bool HasSkin(string skinName) => false;

        public Tween AlphaTween(float endAlpha, float duration) =>
            DOTween.To(() => Alpha, x => Alpha = x, endAlpha, duration).SetLink(gameObject);
    }
}