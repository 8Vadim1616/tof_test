using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Animations
{
    public class SpineSkeletonAnimationController : SpineAnimationController
    {
        private new SkeletonAnimation animator => base.animator as SkeletonAnimation;
        public SkeletonAnimation Animator => animator;

        private void Awake()
        {
            base.animator = GetComponentInChildren<SkeletonAnimation>();

#if UNITY_EDITOR
            //Фикс сбоя шейдеров. На мобилке всё было ок
            if (animator)
                ChangeShader(animator);
#endif
        }

        public void Init(SkeletonAnimation skeletonAnimation)
        {
            base.animator = skeletonAnimation;

#if UNITY_EDITOR
            //Фикс сбоя шейдеров. На мобилке всё было ок
            if (animator)
                ChangeShader(animator);
#endif
        }

        public static void ChangeShader(SkeletonAnimation skeletonAnimation)
        {
            var originalMaterials = new List<Material>();

            if (skeletonAnimation.CustomMaterialOverride.Count == 0)
            {
                var renderer = skeletonAnimation.GetComponent<SkeletonRenderer>();
                if (renderer)
                {
                    var skeletonDataAsset = renderer.skeletonDataAsset;

                    if (skeletonDataAsset is null)
                        Debug.LogError($"{skeletonAnimation.name} has no SkeletonData Asset (SkeletonAnimation)");
                    else if (skeletonDataAsset.atlasAssets.Any(x => x is null))
                    {
                        Debug.LogError($"{skeletonAnimation.name} has no atlas asset");
                        //FEditor.Error($"Не найден атлас для {skeletonAnimation.name}");
                    }
                    else
                    {
                        foreach (var mat in skeletonDataAsset.atlasAssets.SelectMany(x => x.Materials))
                        {
                            skeletonAnimation.CustomMaterialOverride.Add(mat, mat);
                            originalMaterials.Add(mat);
                        }
                    }
                }
            }
            else
            {
                foreach (var kvp in skeletonAnimation.CustomMaterialOverride)
                {
                    originalMaterials.Add(kvp.Key);
                }
            }

            for (var i = 0; i < originalMaterials.Count; i++)
            {
                skeletonAnimation.CustomMaterialOverride[originalMaterials[i]].shader =
                    Shader.Find(skeletonAnimation.CustomMaterialOverride[originalMaterials[i]].shader.name);
            }
        }

        public override bool PlayOnInvisible
        {
            get => animator.updateWhenInvisible == UpdateMode.FullUpdate;
            set => animator.updateWhenInvisible = value ? UpdateMode.FullUpdate : UpdateMode.Nothing;
        }

        public override void SetChildVisible(string boneName, bool visible)
        {
            if (animator == null || animator.skeleton == null) return;
            var bone = animator.skeleton.FindBone(boneName);
            if (bone == null) return;

            foreach (var slot in animator.skeleton.Slots.Where(s => s != null && s.Bone == bone))
                slot.A = visible ? 1 : 0;

            animator.Update(0);
            animator.LateUpdate();
        }

        public override void SetChildVisibleByPattern(string boneNamePattern, bool visible)
        {
            if (animator == null || animator.skeleton == null) return;

            var bones = animator.skeleton.Bones.Where(b => b.Data.Name.StartsWith(boneNamePattern)).ToList();
            foreach (var bone in bones)
                SetChildVisible(bone.Data.Name, visible);
        }

        public override Vector2 GetBoneLocalPosition(string boneName)
        {
            if (animator == null || animator.skeleton == null) return new Vector2();

            var bone = animator.skeleton.FindBone(boneName);
            if (bone == null)
                return default;

            //return new Vector2(bone.AX, bone.AY);
            return bone.GetLocalPosition();
        }

        public override bool HasBone(string boneName) =>
            animator && animator.skeleton?.FindBone(boneName) != null;

        public override void Pause()
        {
            if (animator == null || animator.skeleton == null) return;

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

        public override void GoToTime(float val)
        {
            if (animator == null || animator.state == null) return;

            var track = animator.state.GetCurrent(0);

            if (track != null)
                track.TrackTime = track.TrackEnd * val;
        }

        public override void StopOnTime(float val = 0f)
        {
            GoToTime(val);
            Pause();
        }

        private float alpha = 1f;
        public override float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;

				if (animator)
				{
					var wasAnimatorEnabled = animator.enabled;
					animator.enabled = true;

					if (animator.skeleton != null)
						animator.skeleton.A = alpha;
					if (animator)
						animator.Update(0);
					if (animator)
						animator.LateUpdate();
					if (animator)		// dont ask
						animator.enabled = wasAnimatorEnabled;
				}
            }
        }

        public override Color Color
        {
            set
            {
                if (!animator) return;

                var wasAnimatorEnabled = animator.enabled;
                animator.enabled = true;

                animator.skeleton.SetColor(value);

                // этот способ закрашивает не все кости
                //foreach (var slot in animator.skeleton.Slots)
                //{
                //    //slot.A = value.a; убрано т.к при строительстве объект отображался как люксовый
                //    alpha = value.a;
                //    slot.R = value.r;
                //    slot.G = value.g;
                //    slot.B = value.b;
                //}

                animator.Update(0);
                animator.LateUpdate();

                animator.enabled = wasAnimatorEnabled;
            }
        }

        public override void SetFloat(AnimationData name, float value)
        {
            throw new System.NotImplementedException();
        }

        public override void SetBoolean(AnimationData name, bool value)
        {
            throw new System.NotImplementedException();
        }

        public override IPromise Play(AnimationData animation, bool loop)
        {
            var result = base.Play(animation, loop);

            animator.Update(0);
            animator.LateUpdate();

            return result;
        }

        public override bool HasAnimation(AnimationData animation)
        {
            if (animation == null)
                return false;

            return animator?.Skeleton?.Data.FindAnimation(animation.Name).NotNull() ?? false;
        }

        public override bool Enabled
        {
            get => animator && animator.enabled;
            set
            {
                if (animator != null)
                    animator.enabled = value;
            }
        }
        #region Skins
        private Dictionary<string, Skin> currentSkins = new Dictionary<string, Skin>();
        public override void AddSkin(string value, string customName = "")
        {
            if (animator is null || animator.Skeleton is null)
                return;

            var seekingSkin = animator.Skeleton.Data.FindSkin(value);

            if (seekingSkin is null)
            {
                Debug.LogError($"skin '{value}' not found in {animator}");
                return;
            }

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
            if (!animator)
                return;

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
            return animator?.skeleton?.Data.Animations.Select(x => x.Name).ToArray();
        }

        public override void ClearSkin()
        {
            currentSkins.Clear();
        }
        #endregion
    }
}