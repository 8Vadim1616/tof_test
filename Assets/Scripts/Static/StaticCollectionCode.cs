using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Static
{
	public class StaticCollectionCode<T> : StaticCollection<T> where T : StaticCollectionItemCode
    {
        public StaticCollectionCode(JToken token) : base(token)
        {

        }
        
        public StaticCollectionCode(Dictionary<int, T> token) : base(token)
        {

        }
		
		private readonly List<string> _wasErrors = new List<string>();
		
        public T Get(string codeName, bool errorEnabled = true)
        {
			var result = All.Values.FirstOrDefault(item => item.ModelId == codeName);
			if (result != null)
				return result;

			if (errorEnabled && !_wasErrors.Contains(codeName))
			{
				Debug.LogWarning($"Element '{codeName}' not found in collection {GetType().Name}");
				_wasErrors.Add(codeName);
			}

			return UnknownItem;

		}

        public T this[string codeName] => Get(codeName);
    }
}
