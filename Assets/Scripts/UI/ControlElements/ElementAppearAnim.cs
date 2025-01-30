using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	///<summary>Эффект появления контрола после заданного интервала</summary>
	[RequireComponent(typeof(Animator))]
	public class ElementAppearAnim : MonoBehaviour
	{
		public bool AnimateAppearence = true;
		public float Delay = 0;

		public bool AnimateOnEnable = false;

		private Animator _animator;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			if (_animator && !AnimateAppearence)
				_animator.enabled = false;
		}

		private void Start()
		{
			if (!AnimateOnEnable)
				PlayAnim();
		}

		private void OnEnable()
		{
			if (AnimateOnEnable)
				PlayAnim();
		}

		private void PlayAnim()
		{
			if (_animator && AnimateAppearence)
			{
				if (Delay > 0)
				{
					_animator.enabled = false;
					transform.localScale = Vector3.zero;

					Scripts.Utils.Utils.Wait(Delay)
						   .Then(() =>
							{
								if (!this)
									return;

								transform.localScale = Vector3.one;
								_animator.enabled = true;
								Play();
							});
				}
				else
					Play();

				IPromise Play() => _animator.PlayAnimation("Open")
											.Then(() =>
											 {
												 if (_animator)
													 _animator.enabled = false;
											 });
			}
		}
	}
}