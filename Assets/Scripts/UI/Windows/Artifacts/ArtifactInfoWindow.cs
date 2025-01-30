using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User.Artifacts;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.Artifacts
{
	public class ArtifactInfoWindow : AbstractWindow
	{
		[SerializeField] private ArtifactView _artifactView;
		[SerializeField] private TMP_Text _desc;
		[SerializeField] private TMP_Text _desc2;
		[SerializeField] private ButtonTextIcon _btnUpgrade;

		private UserArtifact _userArtifact;
		
		public static ArtifactInfoWindow Of(UserArtifact userArtifact) =>
						Game.Windows.ScreenChange<ArtifactInfoWindow>(false, w => w.Init(userArtifact), false);

		private void Init(UserArtifact userArtifact)
		{
			_userArtifact = userArtifact;

			_artifactView.Init(userArtifact);
			_desc2.text = _userArtifact.Data.Desc2;
			_btnUpgrade.Text2 = "upgrade_card_text".Localize();
			
			_userArtifact.Level.Subscribe(_ =>
			{
				_desc.text = _userArtifact.Data.Desc(_userArtifact.Level.Value);
				_btnUpgrade.SetItemCount(_userArtifact.GetUpgradeMoney1(), new List<ItemCount> {_userArtifact.GetUpgradeCards()});
			}).AddTo(this);
			
			_btnUpgrade.SetOnClick(() =>
			{
				if (Game.Checks.EnoughItems(_userArtifact.GetUpgradeCost()))
					_userArtifact.Upgrade();
			});
			
			_btnUpgrade.SetActive(_userArtifact.IsOwned);
		}
	}
}