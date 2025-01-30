using System.Collections.Generic;
using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    public class RandomPrefabTimedInstantiator : MonoBehaviour
    {
        [SerializeField] private List<GameObject> prefabs;
        [SerializeField] private Transform parent;
        [SerializeField] private float timerToDestroy;
        [SerializeField] private float setScale = 1;

        public void Start()
        {
            var item = prefabs.RandomFromCollection();
            if (!item) return;
            var inst = Instantiate(item, parent);

            var ps = inst.GetComponentInChildren<ParticleSystem>();

            if (setScale != 1)
            {
                inst.transform.localScale *= setScale;
            }

            void DestroyInstance()
            {
                if (!ps) 
                    Destroy(inst);
                else
                {
                    ps.Stop();
                    var lifeTime = ps.main.startLifetime;
                    Debug.Log($"ANIM CURVE {lifeTime.Evaluate(0)} {lifeTime.Evaluate(1)}");
                    DOVirtual.DelayedCall(lifeTime.Evaluate(1), () => Destroy(inst)).SetLink(inst);
                }
            }

            if (timerToDestroy > 0)
                DOVirtual.DelayedCall(timerToDestroy, DestroyInstance).SetLink(inst);
        }
    }
}