using Spine.Unity;
using UnityEngine;

namespace Assets.Scripts.Animations
{
    public static class AnimationControllerFactory
    {
        public static AbstractAnimationController CreateAnimationController(this GameObject gameObject, bool force = false, GameObject targetObject = null)
        {
            if (!force && gameObject.TryGetComponent(out AbstractAnimationController result))
                return result;

            if (targetObject)
                return InitController(targetObject, gameObject);

            return InitController(gameObject, gameObject);
        }

        private static AbstractAnimationController InitController(GameObject objForSearch, GameObject objForAdd)
        {
            var animator = objForSearch.GetComponent<Animator>();
            if (animator)
            {
                var unityAnimationController = objForAdd.AddComponent<UnityAnimationController>();
                unityAnimationController.Init(animator);
                return unityAnimationController;
            }

            var animatorSk = objForSearch.GetComponent<SkeletonAnimation>();
            if (animatorSk)
            {
                var spineSkeletonAnimationController = objForAdd.AddComponent<SpineSkeletonAnimationController>();
                spineSkeletonAnimationController.Init(animatorSk);
                return spineSkeletonAnimationController;
            }

            var animatorGr = objForSearch.GetComponent<SkeletonGraphic>();
            if (animatorGr)
            {
                var spineGrController = objForAdd.AddComponent<SpineSkeletonGraphicAnimationController>();
                spineGrController.Init(animatorGr);
                return spineGrController;
            }

            return objForAdd.AddComponent<AnimationController>();
        }
    }
}