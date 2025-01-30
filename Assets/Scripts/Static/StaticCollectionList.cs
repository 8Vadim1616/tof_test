using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Static
{
    public class StaticCollectionList<T>
        where T : StaticCollectionItem
    {
        [JsonProperty("all")]
        public List<T> All = new List<T>();

        public StaticCollectionList(JToken token)
        {
            try
            {
                var dict = token != null ? token.ToObject<Dictionary<string, T>>() : new Dictionary<string, T>();
                foreach (var item in dict.Values)
                    All.Add(item);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public T this[int id] => Get(id);

        public T Get(int id)
        {
            return All.FirstOrDefault(x => x.Id == id);
        }
    }
}
