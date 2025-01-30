using System;
using System.Linq;
using Assets.Scripts.Animations;
using Assets.Scripts.Libraries.RSG;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(AbstractAnimationController))]
	public class AnimationHelper : MonoBehaviour
	{
		[SerializeField, Tooltip("Задержка запуска")] float _startDelay = 0;
		[SerializeField] bool _loopEndAnimation = true;
		[SerializeField] string[] _animations;

		private AbstractAnimationController _animator;

		private void Awake()
		{
			_animator = GetComponent<AbstractAnimationController>();
		}

		private void OnEnable()
		{
			if (!_animator)
				return;

			IPromise delayPromise;
			if (_startDelay > 0)
			{
				_animator.Alpha = 0;
				delayPromise = Scripts.Utils.Utils.Wait(_startDelay)
					.Then(() =>
					{
						if (!_animator || !gameObject.activeSelf)
							return Promise.Rejected(null);
						_animator.Alpha = 1;
						return Promise.Resolved();
					});
			}
			else
				delayPromise = Promise.Resolved();

			delayPromise
				.ThenSequence(() => _animations.Select(x => (Func<IPromise>) (() => Play(x, _loopEndAnimation && x == _animations.Last()))));

			IPromise Play(string anim, bool loop)
			{
				if (!_animator || !gameObject.activeSelf)
					return Promise.Rejected(null);
				return _animator.Play(new AnimationData(anim), loop);
			}
		}
	}
}