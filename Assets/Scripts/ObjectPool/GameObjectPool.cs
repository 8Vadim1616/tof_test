using UniRx.Toolkit;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    public class GameObjectPool<T> : ObjectPool<T> where T : Component
    {
        protected Transform parent;
        protected T prefab;

        public GameObjectPool() { }

        public GameObjectPool(T prefab, Transform parent) : this() => 
            Init(prefab, parent);

        public void Init(T prefab, Transform parent)
        {
            this.parent = parent;
            this.prefab = prefab;
        }

        protected override T CreateInstance()
        {
            return prefab != null ? Object.Instantiate(prefab, parent) : default;
        }
    }
}