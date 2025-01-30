using Assets.Scripts.UI.Windows.Components;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Panels
{
	/**
	 * Панель с аватаром, уровнем и именем игрока в худе
	 */
	public class PlayerHudInfoPanel : PlayerPanelView
	{
		[SerializeField] private Slider _levelProgress;

		private void Awake()
		{
			Game.User.Nick.Subscribe(newName =>
			{
				SetUser(Game.User);
			}).AddTo(this);

			Game.User.Level.Subscribe(val =>
			{
				SetUser(Game.User);
			}).AddTo(this);
		}
	}
}