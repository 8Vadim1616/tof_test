using System;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Spine.Unity;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Animations
{
    public class UnityAnimationController : AbstractAnimationController
    {
        public Animator animator;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            animator.keepAnimatorStateOnDisable = true;
        }

        public void Init(Animator anim)
        {
            animator = anim;
            animator.keepAnimatorStateOnDisable = true;
        }

        public override void SetChildVisible(string boneName, bool visible)
        {
        }

        public override void SetChildVisibleByPattern(string boneNamePattern, bool visible)
        {

        }

        public override Vector2 GetBoneLocalPosition(string boneName)
        {
            var pos = animator?.transform.Find(boneName)?.transform.localPosition;
            if (pos.HasValue)
            {
                return pos.Value;
            }
            return default;
        }

        public override bool HasBone(string boneName) => animator && animator.transform.Find(boneName);

        public override IPromise Play(AnimationData animation, bool loop)
        {
            if (currentPlayAnimationPromise?.CurState == PromiseState.Pending)
                currentPlayAnimationPromise.Resolve();

            if (animation == currentPlayingAnimation)
                return currentPlayAnimationPromise;
			
			subscription?.Dispose();
			subscription = null;

            currentPlayingAnimation = animation;

            if (animator)
            {
                currentPlayAnimationPromise = new Promise();
                LastPlayingAnimation = animation;

                playAnimationInternal(animation, loop)
                    .Then(() =>
                    {
                        if (!loop)
                            currentPlayingAnimation = null;
                        if (currentPlayAnimationPromise?.CurState == PromiseState.Pending)
                            currentPlayAnimationPromise.Resolve();
                    });
            }
            else
            {
                currentPlayingAnimation = null;
                currentPlayAnimationPromise = Promise.Resolved() as Promise;
            }

            return currentPlayAnimationPromise;
        }

        public override void GoToTime(float val) { }
        public override void StopOnTime(float val = 0f) { }

        public override bool HasAnimation(AnimationData animation)
        {
            if (animator)
                for (var i = 0; i < animator.layerCount; i++)
                    if (animator.HasState(i, animation.Hash))
                        return true;

            return false;
        }

        public override bool Enabled
        {
            get => animator && animator.enabled;

            set
            {
                if (animator)
                    animator.enabled = value;
            }
        }

        public override void Pause()
        {
            Enabled = false;
        }

        public override void Resume()
        {
            Enabled = true;
        }

        protected float alpha = 1f;
        protected bool alphaIsValid = true;
        private SpriteRenderer[] renderers;
        private SkeletonAnimation[] skeletonAnimations;
        public override float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (!alpha.Equals(value)) alphaIsValid = false;
                if (alphaIsValid) return;
                alpha = value;

                if (!gameObject) return;

                if (renderers == null)
                    renderers = GetComponentsInChildren<SpriteRenderer>();

                foreach (var r in renderers)
                {
                    if (r)
                        r.color = new Color(r.color.r, r.color.g, r.color.b, value);
                }

                if (skeletonAnimations == null)
                    skeletonAnimations = GetComponentsInChildren<SkeletonAnimation>();
                foreach (var spineSkeleton in skeletonAnimations)
                {
                    if (spineSkeleton)
                        spineSkeleton.skeleton.A = value;
                }

                if (animator)
                {
                    var wasAnimatorEnabled = animator.enabled;
                    animator.enabled = IsAnimatorNeedToBeActive();
                    if (wasAnimatorEnabled != animator.enabled)
                    {
                        alphaIsValid = false;
                        renderers = null;
                        Alpha = Alpha;
                    }
                }

                var particles = gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    if (!Alpha.Equals(1f))
                        particle.Stop();
                    else particle.Play();
                }
            }
        }

        protected bool IsAnimatorNeedToBeActive()
        {
            return Alpha.Equals(1f)/*//todo ??? && IsAnimatorWasEnabledOnStart*/;
        }

        public override Color Color
        {
            set { }
        }

        private IDisposable subscription;
        private IPromise playAnimationInternal(AnimationData animation, bool loop)
        {
            if (!animator.gameObject.activeInHierarchy /*|| !animator.gameObject.activeSelf*/)
                return Promise.Resolved();

            var wasPlayOnInvisible = PlayOnInvisible;

            PlayOnInvisible = true;
            animator.enabled = true;
            animator.Play(animation.Hash);
            //animator.Update(0);

            if (loop)
            {
                PlayOnInvisible = wasPlayOnInvisible;
                return Promise.Resolved();
            }

            var result = new Promise();

            subscription?.Dispose();

            subscription = Observable.EveryUpdate().Subscribe(
                val =>
                {
                    if (animator == null) { subscription?.Dispose(); return; }
                    if (!animator.isActiveAndEnabled)
                        animator.enabled = true;
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    if (state.normalizedTime >= 1f || state.shortNameHash != animation.Hash)
                    {
                        PlayOnInvisible = wasPlayOnInvisible;
                        subscription?.Dispose();
                        subscription = null;
                        result.Resolve();
                    }
                }).AddTo(animator);

            return result;
        }

        private IPromise OnChangeStage()
        {
            var result = new Promise();

            var curAnim = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            IDisposable sub = null;

            sub = Observable.EveryUpdate().Subscribe(
                val =>
                {
                    if (!animator.isActiveAndEnabled)
                        animator.enabled = true;

                    if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != curAnim)
                    {
                        result.Resolve();
                        sub?.Dispose();
                    }
                }).AddTo(animator);

            return result;
        }

        public override void SetTrigger(AnimationData trigger)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

            //var wasPlayOnInvisible = PlayOnInvisible;
            //PlayOnInvisible = true;
            if (animator)
                animator.SetTrigger(trigger.Hash);
            //animator.Update(0);
            //PlayOnInvisible = wasPlayOnInvisible;
        }

        public override void SetInteger(AnimationData name, int value)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;
			
            //var wasPlayOnInvisible = PlayOnInvisible;
            //PlayOnInvisible = true;
            if (animator)
                animator.SetInteger(name.Hash, value);
            //animator.Update(0);
            //PlayOnInvisible = wasPlayOnInvisible;
        }

        public override void SetFloat(AnimationData name, float value)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

            //var wasPlayOnInvisible = PlayOnInvisible;
            //PlayOnInvisible = true;
            if (animator)
                animator.SetFloat(name.Hash, value);
            //animator.Update(0);
            //PlayOnInvisible = wasPlayOnInvisible;
        }

        public override void SetBoolean(AnimationData name, bool value)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

            //var wasPlayOnInvisible = PlayOnInvisible;
            //PlayOnInvisible = true;
            if (animator)
                animator.SetBool(name.Hash, value);
            //animator.Update(0);
            //PlayOnInvisible = wasPlayOnInvisible;
        }

        public override bool PlayOnInvisible
        {
            get => animator.cullingMode == AnimatorCullingMode.AlwaysAnimate;
            set => animator.cullingMode =
                value ? AnimatorCullingMode.AlwaysAnimate : AnimatorCullingMode.CullCompletely;
        }

        public override IPromise OnCompleteCurrentAnimation()
        {
            if (currentPlayAnimationPromise != null)
                return currentPlayAnimationPromise;

            return OnChangeStage();
        }
        public override void AddSkin(string value, string customName = "")
        {

        }
        public override void RemoveSkin(string value)
        {

        }
        public override void ClearSkin()
        {

        }
        public override void UpdateSkin()
        {

        }

        public override string[] GetAnimations()
        {
            return animator.runtimeAnimatorController.animationClips.Select(x => x.name).ToArray();
        }
    }
}