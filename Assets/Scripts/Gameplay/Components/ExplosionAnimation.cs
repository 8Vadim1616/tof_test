using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Animations;
using Assets.Scripts.Gameplay;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Gameplay.Components
{
	public class ExplosionAnimation : MonoBehaviour
	{
		private readonly AnimationData IN = new AnimationData("in");
		private readonly AnimationData IDLE = new AnimationData("idle");
		private readonly AnimationData OUT = new AnimationData("out");
		private readonly AnimationData EXPLOSION = new AnimationData("explosion");
		
		[SerializeField] private AbstractAnimationController _animator;
		
		public IPromise PlayInThenIdle() => _animator.PlaySequence(new List<AnimationData> {IN, IDLE});

		public static void PlayExplosionAndDestroy(PoolObjectList<ExplosionAnimation, ExplosionAnimation> pool,
												   ExplosionAnimation effectPrefab,
												   Vector3 globalPosition)
		{
			var expl = pool.Rent(effectPrefab)
						   .SetPosition(globalPosition);

			expl.PlayExplosion()
				.Then(() => pool.Return(effectPrefab, expl));
		}

		public IPromise PlayOut() => _animator.Play(OUT);
		public IPromise PlayExplosion() => _animator.Play(EXPLOSION);

		public static void PlayAndDestroy(PoolObjectList<ExplosionAnimation, ExplosionAnimation> pool,
										  ExplosionAnimation effectPrefab,
										  Vector3 globalPosition,
										  float waitTime = 0f)
		{
			var expl = pool.Rent(effectPrefab)
						   .SetPosition(globalPosition);

			expl.PlayInThenIdle()
				.Then(() => Utils.Wait(waitTime))
				.Then(expl.PlayOut)
				.Then(() =>
				 {
					 if (expl)
						pool.Return(effectPrefab, expl);
				 });
		}
		
		public ExplosionAnimation SetPosition(Vector3 position)
		{
			transform.position = position;
			return this;
		}
	}
}