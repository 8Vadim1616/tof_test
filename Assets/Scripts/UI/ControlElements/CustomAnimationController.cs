using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;
using Assets.Scripts.Animations;

namespace Assets.Scripts.UI.ControlElements
{
	///<summary>Эффект появления контрола после заданного интервала</summary>
	public class CustomAnimationController : MonoBehaviour
	{
		[SerializeField] private string[] _animations;
		private AnimationData[] _animationDatas;

		private AbstractAnimationController _animationController;

		private void Awake()
		{
			if (!_animations.IsNullOrEmpty())
				_animationDatas = _animations.Select(a => new AnimationData(a)).ToArray();
			_animationController = gameObject.CreateAnimationController();
		}

		private void OnEnable()
		{
			if (_animationController == null)
			{
				Debug.LogWarning("No animation controller.");
				return;
			}

			if (_animations == null || _animations.Length == 0)
			{
				Debug.LogWarning("No animations for play.");
				return;
			}

			IPromise animPromise = Promise.Resolved();
			for (int i = 0; i < (_animations.Length - 1); i++)
				animPromise = animPromise.Then(() => _animationController.Play(_animationDatas[i]));

			animPromise = animPromise.Then(() => _animationController.Play(_animationDatas[_animationDatas.Length - 1], true));
		}
	}
}