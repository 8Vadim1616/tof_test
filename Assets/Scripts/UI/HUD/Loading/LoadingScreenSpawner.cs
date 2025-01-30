using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Loading
{
	public class LoadingScreenSpawner : MonoBehaviour
	{
		[SerializeField] private LoadingScreenSpawnPrefab[] _prefabs;

		private (int index, LoadingScreenSpawnPrefab prefab)? _lastPrefab;

		private System.Random _random;

		public LoadingScreenSpawnPrefab CurrentPrefab => _lastPrefab?.prefab;

		private void Awake()
		{
			_random = new System.Random();
		}

		public void SpawnNext()
		{
			if (_prefabs == null)
				return;

			int nextIndex = _random.Next(0, _prefabs.Length);

			if (_lastPrefab.HasValue)
			{
				if (nextIndex == _lastPrefab.Value.index)
				{
					_lastPrefab.Value.prefab.SetActive(true);
					return;
				}

				Destroy(_lastPrefab.Value.prefab.gameObject);
				_lastPrefab = null;
			}

			LoadingScreenSpawnPrefab nextPrefab = Instantiate(_prefabs[nextIndex], transform);
			_lastPrefab = (nextIndex, nextPrefab);
		}

		public IPromise ShowAll()
		{
			if (_prefabs == null)
				return Promise.Resolved();

			IPromise showPromise = Promise.Resolved();

			for (int i = 0; i < _prefabs.Length; i++)
			{
				int index = i;
				showPromise = showPromise
					.Then(() =>
					{
						if (_lastPrefab.HasValue)
						{
							Destroy(_lastPrefab.Value.prefab.gameObject);
							_lastPrefab = null;
						}

						LoadingScreenSpawnPrefab nextPrefab = Instantiate(_prefabs[index], transform);
						_lastPrefab = (index, nextPrefab);
					})
					.Then(() => Scripts.Utils.Utils.Wait(1f));
				
			}

			return showPromise;			
		}
	}
}
