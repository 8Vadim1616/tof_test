using Assets.Scripts.UI.Windows.Components;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class PlayerPanelInHudView : PlayerPanelView
	{
		[SerializeField] private bool _isCurrent;
		
		private void Awake()
		{
			Game.Instance.Playfiled.Subscribe(_ =>
			{
				var playfield = Game.Instance.Playfiled.Value;

				gameObject.SetActive(Game.Instance.Playfiled.Value);

				if (!playfield)
					return;

				/**
				SetUser(_isCurrent ? Game.Instance.Playfiled.Value.Player.User
								   : Game.Instance.Playfiled.Value.Bot.User);
					*/			
								
			}).AddTo(this);
		}
	}
}