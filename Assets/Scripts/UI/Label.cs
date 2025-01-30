using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
	[RequireComponent(typeof(TMP_Text))]
	public class Label : MonoBehaviour
	{
		public string Text => _tmpText.text;
		
		[Header("Settings")]
		[SerializeField] private string _translationKey;
		[SerializeField] private string _suffix;
		
		private TMP_Text _tmpText;
		private bool _isLocalized;
		
		private void Start()
		{
			if (_isLocalized)
				return;
			
			Localize();
		}

		private void TryInitialize()
		{
			if (_tmpText != null)
				return;
			
			_tmpText = GetComponent<TMP_Text>();
		}

		public void Localize(params string[] parameters)
		{
			TryInitialize();
			
			_tmpText.text = $"{_translationKey.Localize(parameters)}{_suffix}";
			_isLocalized = true;
		}

		public void SetText(string text)
		{
			TryInitialize();
			
			_tmpText.text = $"{text}{_suffix}";
			_isLocalized = true;
		}
	}
}