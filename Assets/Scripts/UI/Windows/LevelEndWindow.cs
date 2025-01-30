using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.General;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.Utils;
using DG.Tweening;
using Gameplay.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows
{
	public class LevelEndWindow : AbstractWindow
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private TMP_Text _waveTitle;
		[SerializeField] private TMP_Text _waveNum;
		[SerializeField] private TMP_Text _contributionTitle;
		[SerializeField] private PlayerPanelInHudView _currentPlayerPanel;
		[SerializeField] private PlayerPanelInHudView _otherPlayerPanel;
		[SerializeField] private Slider _contributionSlider;
		[SerializeField] private TMP_Text _contributionCurrent;
		[SerializeField] private TMP_Text _contributionOther;
		[SerializeField] private ButtonTextIcon _btnReplay;
		[SerializeField] private ButtonText _btnLobby;
		[SerializeField] private ItemCountView _dropItemPrefab;

		private List<ItemCount> _drop;

		public static LevelEndWindow Of(GameStats stats, List<ItemCount> drop) =>
						Game.Windows.ScreenChange<LevelEndWindow>(false, w => w.Init(stats, drop));

		private void Init(GameStats stats, List<ItemCount> drop)
		{
			_drop = drop;
			_title.text = (stats.IsWin ? "win_title" : "lose_title").Localize();
			_waveTitle.text = "wave_title".Localize();
			_waveNum.text = stats.Wave.ToString();
			_contributionTitle.text = "contribution_title".Localize();
			
			_btnReplay.Text2 = "play".Localize();
//java			_btnReplay.Text = Game.Settings.EnergyForLevel.Count.ToString();
			_btnReplay.SetOnClick(() =>
			{
				Close();
			});
			
			_btnLobby.Text = "lobby".Localize();
			_btnLobby.SetOnClick(Close);
			
			_currentPlayerPanel.SetUser(Game.User);
			_otherPlayerPanel.SetUser(stats.Coop);
			
			_contributionSlider.maxValue = stats.PlayerDamage + stats.CoopDamage;
			_contributionSlider.value = 0;
			_contributionSlider.DOValue(stats.PlayerDamage, 1f)
							   .SetEase(Ease.InOutQuad)
							   .SetLink(_contributionSlider.gameObject);

			int curPrc = 0;
			DOTween.To(() => curPrc, val => curPrc = val, stats.PlayerPrc, 1f)
				   .SetLink(_contributionCurrent.gameObject)
				   .OnUpdate(() =>
					{
						var otherPrc = 100 - curPrc;
						_contributionCurrent.text = $"{curPrc}%";
						_contributionOther.text = $"{otherPrc}%";
					});


			DrawDrop();
		}

		private void DrawDrop()
		{
			if (!_drop.IsNullOrEmpty())
			{
				_dropItemPrefab.SetActive(true);
				foreach (var d in _drop)
				{
					var itemView = Instantiate(_dropItemPrefab, _dropItemPrefab.transform.parent);
					itemView.SetItemCount(d);
				}
			}
			
			_dropItemPrefab.SetActive(false);
		}
	}
}