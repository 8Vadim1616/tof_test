using Assets.Scripts.Gameplay;
using UniRx;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class WaveSpecialConsPanel : ResourcePanelInGame
	{
		protected override void OnPlayfieldChanged(PlayfieldView playfieldView)
		{
			Property = playfieldView.Player.WaveSpecialCoin;
			Property.Subscribe(_ =>
			{
				SetAmount(Property.Value);
			}).AddTo(playfieldView.gameObject);
		}
	}
}