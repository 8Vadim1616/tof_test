using Assets.Scripts.Libraries.RSG;
using Spine;
using Spine.Unity;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Animations
{
	public abstract class SpineAnimationController : AbstractAnimationController
	{
		[SerializeField] float _mixDuration = 0;
		protected IAnimationStateComponent animator;

		public override IPromise Play(AnimationData animation, bool loop)
		{
			if (!this || !gameObject)
				return Promise.Resolved();

			if (animation == currentPlayingAnimation && currentPlayAnimationPromise?.IsPending == true)
				return currentPlayAnimationPromise;

			if (currentPlayAnimationPromise?.IsPending == true)
				currentPlayAnimationPromise.ResolveOnce();

			if (!loop && !gameObject.activeInHierarchy)
			{
				currentPlayingAnimation = null;
				currentPlayAnimationPromise = Promise.Resolved() as Promise;
				return currentPlayAnimationPromise;
			}

			currentPlayingAnimation = animation;

			if (animator != null && HasAnimation(animation))
			{
				var wasEnabled = Enabled;
				var wasPlayOnInvisible = PlayOnInvisible;
				if (!Enabled)
					Enabled = true;

				LastPlayingAnimation = animation;

				if (!loop)
					PlayOnInvisible = true;

				animator.AnimationState.Data.DefaultMix = _mixDuration;
				var track = animator.AnimationState.SetAnimation(0, animation.Name, loop);

                if (!loop)
				{
					currentPlayAnimationPromise = new Promise();
					track.Complete += OnComplete;

					void OnComplete(TrackEntry entry)
					{
						track.Complete -= OnComplete;
						currentPlayingAnimation = null;
						Enabled = wasEnabled;
						PlayOnInvisible = wasPlayOnInvisible;
						if (currentPlayAnimationPromise?.IsPending == true)
							currentPlayAnimationPromise.ResolveOnce();
					};

				}
				else
				{
					currentPlayAnimationPromise = Promise.Resolved() as Promise;
				}
			}
			else
			{
				currentPlayingAnimation = null;
				currentPlayAnimationPromise = Promise.Resolved() as Promise;
			}

			return currentPlayAnimationPromise;
		}

		public override IPromise OnCompleteCurrentAnimation()
		{
			if (currentPlayAnimationPromise != null)
				return currentPlayAnimationPromise;

			return Promise.Resolved();
		}
		
		public override void SetFloat(AnimationData name, float value)
		{
			throw new System.NotImplementedException();
		}

		public override void SetTrigger(AnimationData trigger)
		{
			throw new System.NotImplementedException();
		}

		public override void SetInteger(AnimationData name, int value)
		{
			throw new System.NotImplementedException();
		}

		public override void SetBoolean(AnimationData name, bool value)
		{
			throw new System.NotImplementedException();
		}

		public IAnimationStateComponent GetAnimator() { return animator; }

	}
}