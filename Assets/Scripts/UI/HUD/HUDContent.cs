using System;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD
{
	public class HUDContent : MonoBehaviour
	{
		public MainScreen.MainScreen MainScreen;
		public InGameContent InGameContent;
		
		[SerializeField] public CanvasGroup CanvasGroup;

		private void Awake()
		{
			Game.Instance.Playfiled.Subscribe(_ =>
			{
				MainScreen.gameObject.SetActive(!Game.Instance.Playfiled.Value);
				InGameContent.gameObject.SetActive(Game.Instance.Playfiled.Value);
			}).AddTo(this);
		}
	}
}