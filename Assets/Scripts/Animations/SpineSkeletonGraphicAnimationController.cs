using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Animations
{
    public class SpineSkeletonGraphicAnimationController : SpineAnimationController
    {
        [SerializeField] SkeletonGraphic _animator;

        private new SkeletonGraphic animator => base.animator as SkeletonGraphic;

#if UNITY_EDITOR
        private void Reset()
        {
            if (!_animator)
                _animator = GetComponentInChildren<SkeletonGraphic>();
        }
#endif

        private void Awake()
        {
            base.animator = _animator ? _animator : GetComponentInChildren<SkeletonGraphic>();
        }

        public void Init(SkeletonGraphic skeletonGraphic)
        {
            if (skeletonGraphic == null)
            {
                Debug.LogWarning("[SpineSkeletonGraphicAnimationController] Init() skeletonGraphic is null");
                return;
            }

            base.animator = skeletonGraphic;
            skeletonGraphic.Initialize(false);
        }

        public override IPromise Play(AnimationData animation, bool loop = false)
        {
			if (!this || !animator)
				return Promise.Rejected(null);

            var result = base.Play(animation, loop);

            animator.Update(0);
            animator.LateUpdate();

            return result;
        }

        public override void GoToTime(float val)
        {
            if (animator == null || animator.AnimationState == null)
                return;

            var track = animator.AnimationState.GetCurrent(0);

            if (track != null)
                track.TrackTime = track.AnimationEnd * val;
        }

        public override void StopOnTime(float val = 0f)
        {
            GoToTime(val);
            Pause();
        }

        public override bool PlayOnInvisible
        {
            get => animator.updateWhenInvisible == UpdateMode.FullUpdate;
            set => animator.updateWhenInvisible = value ? UpdateMode.FullUpdate : UpdateMode.Nothing;
        }

        public override void SetChildVisible(string boneName, bool visible)
        {
            var bone = animator.Skeleton.FindBone(boneName);
            if (bone == null) return;

            foreach (var slot in animator.Skeleton.Slots.Where(s => s != null && s.Bone == bone))
                slot.A = visible ? 1 : 0;

            animator.Update(0);
            animator.LateUpdate();
        }

        public override void SetChildVisibleByPattern(string boneNamePattern, bool visible)
        {
            var bones = animator.Skeleton.Bones.Where(b => b.Data.Name.StartsWith(boneNamePattern)).ToList();
            foreach (var bone in bones)
                SetChildVisible(bone.Data.Name, visible);
        }

        public override Vector2 GetBoneLocalPosition(string boneName)
        {
            return default;
        }

        public override bool HasBone(string boneName) => false;

        public override void Pause()
        {
            if (!animator) return;

            if (!Enabled)
                return;

            animator.Update(0);
            animator.LateUpdate();

            Enabled = false;
        }

        public override void Resume()
        {
            Enabled = true;
        }

        public override bool HasAnimation(AnimationData animation)
        {
            if (!animator)
                return false;

            return animator.SkeletonData?.FindAnimation(animation.Name) != null;
        }

        private float alpha = 1f;
        public override float Alpha
        {
            get => alpha;
            set
            {
                SetAlpha2(value);

                // Старый код который вроде как неправильно засвет делает - хз что в нем не так 
                //alpha = value;

                //if (!animator) return;

                //var wasAnimatorEnabled = animator.enabled;
                //animator.enabled = true;

                //foreach (var slot in animator.canvasRenderers)
                //{
                //    var color = slot.GetColor();
                //    color.a = value;
                //    slot.SetColor(color);
                //}

                //if (animator.canvasRenderer)
                //{
                //    var color = animator.canvasRenderer.GetColor();
                //    color.a = value;
                //    animator.canvasRenderer.SetColor(color);
                //}

                //animator.Update(0);
                //animator.LateUpdate();

                //animator.enabled = wasAnimatorEnabled;
            }
        }

        private void SetAlpha2(float value)
        {
            alpha = value;

            if (!animator) return;

            var wasAnimatorEnabled = animator.enabled;
            
            animator.enabled = true;
            animator.color = animator.color.Set(a: value);

            animator.Update(0);
            animator.LateUpdate();

            animator.enabled = wasAnimatorEnabled;
        }

        public override Color Color
        {
            set
            {
                if (!animator) return;

                var wasAnimatorEnabled = animator.enabled;
                animator.enabled = true;

                foreach (var slot in animator.canvasRenderers)
                {
                    slot.SetColor(value);
                    alpha = value.a;
                }

                animator.Update(0);
                animator.LateUpdate();

                animator.enabled = wasAnimatorEnabled;
            }
        }

        public override bool Enabled
        {
            get
            {
                if (animator != null)
                    return !animator.freeze;

                return false;
            }

            set
            {
                if (animator != null)
                    animator.freeze = !value;
            }
        }

        #region Skins
        private Dictionary<string, Skin> currentSkins = new Dictionary<string, Skin>();
        
        public override void AddSkin(string value, string customName = "")
        {
            var seekingSkin = animator.Skeleton.Data.FindSkin(value);
            if (seekingSkin == null) return;
            var skinName = customName != "" ? customName : value;
            if (!currentSkins.ContainsKey(skinName))
                currentSkins.Add(skinName, seekingSkin);
        }

        public override bool HasSkin(string skinName)
        {
            return animator.Skeleton.Data.Skins.Any(x => x.Name == skinName);
        }

        public override void RemoveSkin(string value)
        {
            if (currentSkins.ContainsKey(value))
            {
                currentSkins.Remove(value);
            }
        }

        public override void UpdateSkin()
        {
            var currentSkin = new Skin("customSkin");
            Skeleton skeleton = animator.Skeleton;
            foreach (var skin in currentSkins)
            {
                currentSkin.AddSkin(skin.Value);
            }
            skeleton.SetSkin(currentSkin);
            skeleton.SetSlotsToSetupPose();
        }

        public override string[] GetAnimations()
        {
            return animator?.SkeletonData.Animations.Select(x => x.Name).ToArray();
        }

        public override void ClearSkin()
        {
            currentSkins.Clear();
        }
        #endregion
    }
}