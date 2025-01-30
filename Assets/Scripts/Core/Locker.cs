using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts.Core
{
    public class Locker : MonoBehaviour
    {
        public static bool ENABLE_LOG = true;
        private readonly List<string> _lockedByKey = new List<string>();

		public List<string> GetAllLocks() => _lockedByKey.Clone();

        public BoolReactiveProperty IsLocked = new BoolReactiveProperty(false);
        
        public void ClearAllLocks()
        {
            _lockedByKey.Clear();
            UpdateActive();
        }

		public IPromise WhenUnlock()
		{
			if (!IsLocked.Value)
				return Promise.Resolved();

			IDisposable sub = null;
			var result = new Promise();
			sub = IsLocked.Subscribe(_ =>
			{
				if (!IsLocked.Value)
				{
					result.Resolve();
					sub?.Dispose();
				}
			}).AddTo(this);

			return result;
		}

        public bool IsLockedByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return _lockedByKey.Contains(key);
        }

        public void Lock(string key)
        {
            _lockedByKey.Add(key);
            UpdateActive();
            if (ENABLE_LOG)
                Debug.Log($"Lock {key} {_lockedByKey.Count}");
        }
		
		public void Lock(IEnumerable<string> lockKeys)
		{
			if (lockKeys.IsNullOrEmpty())
				return;
			
			foreach (var key in lockKeys)
				_lockedByKey.Add(key);
			
			UpdateActive();
			Debug.Log($"MultyLock {string.Join(",", _lockedByKey)} {_lockedByKey.Count}");
		}
		
		public void LockOnce(IEnumerable<string> lockKeys)
		{
			if (lockKeys.IsNullOrEmpty())
				return;
			
			foreach (var key in lockKeys)
				if (!IsLockedByKey(key))
					_lockedByKey.Add(key);
			
			UpdateActive();
			Debug.Log($"MultyLock {string.Join(",", _lockedByKey)} {_lockedByKey.Count}");
		}

        public void LockOnce(string key)
        {
            if (IsLockedByKey(key))
				return;
            Lock(key);
        }

        public void Unlock(string key)
        {
            _lockedByKey.Remove(key);
            UpdateActive();
            if (ENABLE_LOG)
                Debug.Log($"Unlock {key} {_lockedByKey.Count}");
        }

        public void UnlockAll(string key)
        {
            _lockedByKey.RemoveAll(x => x == key);
            UpdateActive();
            if (ENABLE_LOG)
                Debug.Log($"UnlockAll {key} {_lockedByKey.Count}");
        }

        private void UpdateActive()
        {
            var newValue = _lockedByKey.Count > 0;
            if (IsLocked.Value != newValue)
            {
                gameObject.SetActive(newValue);
                IsLocked.Value = newValue;
            }
        }

        public string GetAllLocksString => _lockedByKey.Aggregate((x, y) => x + " " + y);
    }
}