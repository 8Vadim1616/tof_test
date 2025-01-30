using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Animations
{
    public class AnimationController : AbstractAnimationController
    {
        protected float alpha = 1f;
        protected bool alphaIsValid = true;
        private SpriteRenderer[] renderers;
        private Tilemap[] tilemaps;
        public override float Alpha
        {
            get => alpha;
            set
            {
                if (!alpha.Equals(value)) alphaIsValid = false;
                if (alphaIsValid) return;
                alpha = value;

                if (!gameObject) return;

                if (renderers == null)
                    renderers = GetComponentsInChildren<SpriteRenderer>();
                if (tilemaps == null)
                    tilemaps = GetComponentsInChildren<Tilemap>();

                foreach (var r in renderers)
                    if (r) r.color = new Color(r.color.r, r.color.g, r.color.b, value);
                foreach (var r in tilemaps)
                    if (r) r.color = new Color(r.color.r, r.color.g, r.color.b, value);

                var particles = gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    if (!Alpha.Equals(1f))
                        particle.Stop();
                    else particle.Play();
                }
            }
        }

        public override Color Color
        {
            set
            {
                if (renderers == null)
                    renderers = GetComponentsInChildren<SpriteRenderer>();
                if (tilemaps == null)
                    tilemaps = GetComponentsInChildren<Tilemap>();

                foreach (var r in renderers)
                    if (r) r.color = value;
                foreach (var r in tilemaps)
                    if (r) r.color = value;
            }
        }

        public override void SetTrigger(AnimationData trigger)
        {
        }

        public override void SetInteger(AnimationData name, int value)
        {
        }

        public override void SetFloat(AnimationData name, float value)
        {
            
        }

        public override void SetBoolean(AnimationData name, bool value)
        {
        }

        public override bool PlayOnInvisible { get; set; }

        public override void SetChildVisible(string boneName, bool visible)
        {
        }

        public override void SetChildVisibleByPattern(string boneNamePattern, bool visible)
        {
        }

        public override Vector2 GetBoneLocalPosition(string boneName)
        {
            var pos = gameObject?.transform.FindInChildrenRecursively(boneName)?.transform.localPosition;
            if (pos.HasValue)
            {
                return pos.Value;
            }
            return default;
        }

        public override bool HasBone(string boneName) => gameObject && gameObject.transform.FindInChildrenRecursively(boneName);

        public override void Pause() { }

        public override void Resume() { }

        public override IPromise OnCompleteCurrentAnimation()
        {
            return Promise.Resolved();
        }

        public override bool HasAnimation(AnimationData animation)
        {
            return false;
        }

        public override IPromise Play(AnimationData animation, bool loop = false)
        {
            return Promise.Resolved();
        }

        public override void GoToTime(float val) { }
        public override void StopOnTime(float val = 0f) { }

        public override bool Enabled { get; set; }

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
            return new string[] { };
        }
    }
}