using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Static
{
    public class StaticCollection<T>
    {
        public Dictionary<int, T> All = new Dictionary<int, T>();

		public T UnknownItem { get; set; }

		public void Update(Dictionary<int, T> all)
		{
			All = all;
		}
		
		public void Update(JToken token)
		{
			All = token.ToObject<Dictionary<int, T>>();
		}

		public StaticCollection(JToken token)
        {
            try
            {
                All = token != null ? token.ToObject<Dictionary<int, T>>() : new Dictionary<int, T>();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        public StaticCollection(Dictionary<int, T> all)
        {
            All = all ?? new Dictionary<int, T>();
        }

        public T this[int id]
        {
            get { return Get(id); }
        }

		private readonly List<int> _wasErrors = new List<int>();

		public T Get(int id, bool errorEnabled = true)
		{
			if (All.TryGetValue(id, out T result))
				return result;

			if (id > 0 && errorEnabled)
			{
				if (!_wasErrors.Contains(id))
				{
					Debug.LogError($"Element '{id}' not found in collection {GetType().Name}");
					_wasErrors.Add(id);
				}

				return UnknownItem;
			}

			return default;
		}
    }
}
