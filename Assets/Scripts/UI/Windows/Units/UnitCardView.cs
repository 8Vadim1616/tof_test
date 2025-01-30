using Assets.Scripts.Static.Units;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.User.Units;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Units
{
	public class UnitCardView : MonoBehaviour
	{
		[SerializeField] private Color32 _commonBarColor;
		[SerializeField] private Color32 _mythicBarColor;
		
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _level;
		[SerializeField] private Image _icon;
		[SerializeField] private Slider _progress;
		[SerializeField] private Image _progressBar;
		[SerializeField] private TMP_Text _progressText;
		[SerializeField] private BasicButton _selectBtn;
		[SerializeField] private Image _cardBack;
		[SerializeField] private GameObject _mythicItemIcon;
		[SerializeField] private SpriteDictionary _cardBacks;

		public UserUnit Unit { get; private set; }

		public void Init(UserUnit unit)
		{
			Unit = unit;
			_name.text = Unit.Data.Name;
			
			if (_mythicItemIcon)
				_mythicItemIcon.SetActive(Unit.Data.UnitType.ModelId == UnitType.MYTHICAL);

			_progressBar.color = Unit.Data.UnitType.ModelId == UnitType.MYTHICAL ? _mythicBarColor : _commonBarColor;
			
			Unit.Data.Card.UserReactive().Subscribe(_ => UpdateUpgradePrice()).AddTo(gameObject);
			Game.User.Items.ReactiveMoney1.Subscribe(_ => UpdateUpgradePrice()).AddTo(gameObject);

			Unit.Level.Subscribe(_ =>
			{
				_level.text = "lvl".Localize(Unit.Level.Value.ToString());
				
				UpdateUpgradePrice();
			}).AddTo(this);
			
			_icon.LoadFromAssets(Unit.Data.IconPath);
			_cardBack.sprite = _cardBacks[Unit.Data.UnitType.ModelId];
			
			if (_selectBtn)
				_selectBtn.SetOnClick(() =>
				{
					UnitInfoWindow.Of(Unit);
				});
		}

		private void UpdateUpgradePrice()
		{
			_progress.maxValue = Unit.GetUpgradeCards()?.Count ?? 0;
			_progress.value = Unit.Data.Card.UserAmount();
			_progressText.text = $"{Unit.Data.Card.UserAmount()}/{_progress.maxValue}";
		}
	}
}