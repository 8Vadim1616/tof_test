using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
	public class PlaceholderHider : MonoBehaviour
	{
		[SerializeField] private TMP_InputField input;

		private void Awake()
		{
			input.onSelect.AddListener((str) => OnInputSelect());
			input.onDeselect.AddListener((str) => OnInputDeselect());
		}

		private void OnInputSelect()
		{
			input.placeholder.gameObject.SetActive(false);
		}

		private void OnInputDeselect()
		{
			if (string.IsNullOrEmpty(input.text))
				input.placeholder.gameObject.SetActive(true);
		}
	}
}