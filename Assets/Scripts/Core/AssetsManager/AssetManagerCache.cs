using System.Collections.Generic;

namespace Assets.Scripts.Core.AssetsManager
{
    public class AssetManagerCache : Dictionary<string, object>
    {
        public void Add<T>(Dictionary<string, T> prefabs)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.Value != null)
                    this[prefab.Key] = prefab.Value;
            }
        }

        public void Add(Dictionary<string, object> prefabs)
        {
            foreach (var prefab in prefabs)
                if (prefab.Value != null)
                    this[prefab.Key] = prefab.Value;
        }
    }
}