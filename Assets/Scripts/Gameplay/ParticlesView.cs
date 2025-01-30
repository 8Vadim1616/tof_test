using System.Linq;
using Assets.Scripts.Libraries.RSG;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.GamePlay.Effects
{
    public class ParticlesView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _mainParticleSystem;
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private float _timeToDestrooy = 2f;


        public static ParticlesView GetOrAddParticlesView(GameObject instance)
        {
            var pv = instance.GetComponent<ParticlesView>();
            if (pv) return pv;

            pv = instance.AddComponent<ParticlesView>();
            pv._particles = instance.GetComponentsInChildren<ParticleSystem>();
            pv._mainParticleSystem = pv._particles.FirstOrDefault();

            return pv;
        }

        public static ParticlesView Create(GameObject target, string resource) => Create(target.transform, resource);
        public static ParticlesView Create(Transform target, string resource)
        {
			var go = Resources.Load<GameObject>(resource);
			if (!go)
			{
				Debug.LogError($"View not found: {resource}");
				return null;
			}
			var inst = Instantiate(go, target);
            return GetOrAddParticlesView(inst);
        }
		
		public static ParticlesView Create(Transform target, ParticlesView prefab)
		{
			var inst = Instantiate(prefab, target);
			return GetOrAddParticlesView(inst.gameObject);
		}

		public ParticlesView SetLocalPosition(Vector3 localPosition)
		{
			transform.localPosition = localPosition;
			return this;
		}
		
		public ParticlesView SetPosition(Vector3 position)
		{
			transform.position = position;
			return this;
		}

		public ParticlesView SetSortingLayer(int sortingLayerId)
		{
			SortingGroup sg = GetComponent<SortingGroup>();
			if (sg)
				sg.sortingLayerID = sortingLayerId;
			
			return this;
		}

		public ParticlesView SetSortingOrder(int sortingOrder)
		{
			SortingGroup sg = GetComponent<SortingGroup>();
			if (sg)
				sg.sortingOrder = sortingOrder;

			return this;
		}

		public void Pause()
        {
            foreach (var particleSystem in _particles)
                particleSystem?.Stop();
        }
        
        public void Play()
        {
            foreach (var particleSystem in _particles)
                particleSystem?.Play();
        }

        public IPromise FreeWhenCompleted()
        {
            if (_mainParticleSystem.main.loop)
                return Free();

            var longestDur = _particles.Length > 0 ? _particles.Max(x => x.main.duration) : 1f;
            var result = new Promise();
			DOVirtual.DelayedCall(longestDur, () => Free().Then(result.Resolve), false)
					 .SetLink(gameObject);
            return result;
        }

		public bool IsDestroyed { get; private set; }

		public IPromise Free()
		{
			if (IsDestroyed)
				return Promise.Resolved();
			IsDestroyed = true;
            Pause();
            var result = new Promise();
            DOVirtual.DelayedCall(_timeToDestrooy,
                () =>
                {
                    result.Resolve();
                    Destroy(gameObject);
                },false)
                .SetLink(gameObject);

            return result;
        }
    }
}