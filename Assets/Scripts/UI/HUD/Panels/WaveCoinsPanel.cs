using Assets.Scripts.Gameplay;
using UniRx;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class WaveCoinsPanel : ResourcePanelInGame 
	{
		protected override void OnPlayfieldChanged(PlayfieldView playfieldView) 
		{
			Property = playfieldView.Player.WaveCoin;
			Property.Subscribe(_ =>
			{
				SetAmount(Property.Value);
			}).AddTo(playfieldView.gameObject);
		}
	}
}