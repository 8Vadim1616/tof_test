using Assets.Scripts.Utils;
using ntw.CurvedTextMeshPro;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Loading
{
	public class LoadingScreenSpawnPrefab : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _titleText;
		[SerializeField] private string _titleKey;

		[SerializeField] private SpawnPrefabLoadFromResourses[] _fromResourses;

		private void Awake()
		{
			_titleText.text = _titleKey.Localize();

			if (_fromResourses != null)
				foreach (var data in _fromResourses)
					data.Image.LoadFromAssets(data.Path);
		}

		[Serializable]
		private struct SpawnPrefabLoadFromResourses
		{
			public Image Image;
			public string Path;
		}
	}
}
