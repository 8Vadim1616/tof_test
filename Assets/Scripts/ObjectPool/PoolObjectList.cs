using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>Мультиобъектный пул</summary>
    /// <typeparam name="TKey">Ключ словаря, определяющий тип префаба</typeparam>
    public class PoolObjectList<TKey, TPoolable> where TPoolable : Component
    {
        private readonly Dictionary<TKey, GameObjectPool<TPoolable>> pools;
        private readonly Func<TKey, TPoolable> prefabGetter;
        private readonly Transform parent;

        public PoolObjectList(Transform parent, Func<TKey, TPoolable> prefabGetter)
        {
            pools = new Dictionary<TKey, GameObjectPool<TPoolable>>();

            this.parent = parent;
            this.prefabGetter = prefabGetter;
        }

        /// <summary>Создать объект из пула если возможно, или создать новый</summary>
        public TPoolable Rent(TKey key)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = pools[key] = new GameObjectPool<TPoolable>();
                pool.Init(prefabGetter(key), parent);
            }

            var result = pool?.Rent();
            if (result)
                result.transform.SetParent(parent);
            return result;
        }

        /// <summary>Вернуть объект в пул</summary>
        public void Return(TKey key, TPoolable item)
        {
            if (pools.TryGetValue(key, out var pool))
                pool.Return(item);
        }
    }
}