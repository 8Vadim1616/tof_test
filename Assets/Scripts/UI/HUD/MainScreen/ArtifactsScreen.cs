using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class ArtifactsScreen : MainScreenBase
	{
		[SerializeField] private ScrollRect _scroll;
		[SerializeField] private RectTransform _groupOwned;
		[SerializeField] private RectTransform _groupNotOwned;
		[SerializeField] private TextHolder _notOwndedDevider;
		[SerializeField] private ArtifactView _artifactPrefab;
		
		protected override void Init()
		{
			_notOwndedDevider.Text = "not_owned".Localize();

			_artifactPrefab.SetActive(true);
			foreach (var unit in Game.Static.Artifacts.All.Values)
			{
				var userArtifact = Game.User.Artifacts.Get(unit);
				var artifactCardView = Instantiate(_artifactPrefab, userArtifact.IsOwned ? _groupOwned : _groupNotOwned);
				artifactCardView.Init(userArtifact);
			}
			_artifactPrefab.SetActive(false);

			base.Init();
			
			LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content.transform as RectTransform);
		}
	}
}