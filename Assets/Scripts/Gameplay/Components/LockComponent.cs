using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Components
{
	public class LockComponent : MonoBehaviour
	{
		private readonly List<string> _lockedByKey = new List<string>();
		public void ClearAllLocks() => _lockedByKey.Clear();
		public bool IsLocked => !_lockedByKey.IsNullOrEmpty();
		public bool IsLockedByKey(string key) => _lockedByKey.Contains(key);
		
		public void Lock(string key)
		{ 
			_lockedByKey.Add(key);
		}

		public void Unlock(string key = null)
		{
			_lockedByKey.Remove(key);
		}
	}
}