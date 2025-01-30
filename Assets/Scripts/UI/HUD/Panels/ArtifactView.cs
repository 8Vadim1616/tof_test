using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Windows.Artifacts;
using Assets.Scripts.User.Artifacts;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class ArtifactView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _level;
		[SerializeField] private TMP_Text _progressText;
		[SerializeField] private Image _icon;
		[SerializeField] private Slider _progress;
		[SerializeField] private BasicButton _selectBtn;
		
		public UserArtifact Artifact { get; private set; }
		
		public void Init(UserArtifact artifact)
		{
			Artifact = artifact;
			_name.text = Artifact.Data.Name;
			Artifact.Level.Subscribe(_ =>
			{
				_level.text = "lvl".Localize(artifact.Level.Value.ToString());
				
				UpdateUpgradePrice();
			}).AddTo(this);
			
			Artifact.Data.Card.UserReactive().Subscribe(_ => UpdateUpgradePrice()).AddTo(gameObject);
			Game.User.Items.ReactiveMoney1.Subscribe(_ => UpdateUpgradePrice()).AddTo(gameObject);
			
			_icon.LoadFromAssets(Artifact.Data.IconPath);
			
			if (_selectBtn)
				_selectBtn.SetOnClick(() =>
				{
					ArtifactInfoWindow.Of(Artifact);
				});
		}
		
		private void UpdateUpgradePrice()
		{
			_progress.maxValue = Artifact.GetUpgradeCards()?.Count ?? 0;
			_progress.value = Artifact.Data.Card.UserAmount();
			_progressText.text = $"{Artifact.Data.Card.UserAmount()}/{_progress.maxValue}";
		}
	}
}