using System.Linq;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.General;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.UI.HUD.Widgets;
using Assets.Scripts.Utils;
using Gameplay;
using HuaweiMobileServices.Game;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD
{
	public class InGameContent : MonoBehaviour
	{
		[SerializeField] private ButtonTextIcon _itemPrefab;
		
		[SerializeField] private TMP_Text _currentWave;
		[SerializeField] private TMP_Text _waveTimeLeft;
		[SerializeField] private TMP_Text _monstersCount;
		[SerializeField] private GameObject _waveBossTimerBack;
		//[SerializeField] private ButtonText _generateButton;
		[SerializeField] private ButtonText _speedUpButton;
		[SerializeField] private ButtonText _upgradesButton;
		[SerializeField] private ButtonText _summonsButton;
		[SerializeField] private ItemCountView _needSummonWaveCoins;
		[SerializeField] private TMP_Text _unitsCount;
		[SerializeField] private BasicButton _btnExit;

		[Header("Monsters slider")]
		[SerializeField] private Slider _monstersSlider;
		[SerializeField] private Image _monstersCountSliderFill;
		[SerializeField] private Sprite _monstersSliderRed;
		[SerializeField] private Sprite _monstersSliderGreen;
		[SerializeField] private Sprite _monstersSliderYellow;

		[Header("Panels")]
		[SerializeField] private UnitsUpgradesView _upgradesView;
		[SerializeField] private SummonPanelView _summonsView;

		[Header("UI doors")]
		[SerializeField] private Image ExpBar;
		[SerializeField] private TextMeshProUGUI ExpText;

		[SerializeField] private Image LifeBar;
		[SerializeField] private TextMeshProUGUI LifeText;
        private int life;
		private int maxLife;

        [SerializeField] private Image AttackBar;
		[SerializeField] private TextMeshProUGUI AttackText;

        //[SerializeField] private Image ArmorBar;
		[SerializeField] private TextMeshProUGUI ArmorText;

        //[SerializeField] private Image MoneyBar;
		[SerializeField] private TextMeshProUGUI MoneyText;

		[SerializeField] private TextMeshProUGUI topLabel;
		[SerializeField] public GoButton GoButton;
		[SerializeField] private TextMeshProUGUI AchievementLabel;
		[SerializeField] private TextMeshProUGUI AchievementDescription;
        public void SetAchievment(string description, string label = null)
		{
            AchievementDescription.text = description;
			if (label != null)
			{
				AchievementLabel.text = label;
			}
        }
		public void SetExp(int now, int max)
		{
            SetUI(now, max, ExpBar, ExpText);
        }
		public void SetLife(int now, int max)
		{
			if(now == -1)
			{
                maxLife = max;
			}
			if (max == -1)
			{
                life = now;
			}
			if(life != -1 && maxLife != -1)
				SetUI(life, maxLife, LifeBar,LifeText);
        }
		public void SetAttack(int now, int max)
		{
			SetUI(now, max, AttackBar, AttackText);
        }
		public void SetArmor(int now)
		{
			ArmorText.text = now.ToString();
        }
		public void SetMoney(int now)
		{
            MoneyText.text = now.ToString();
        }
        public void SetTopLabelText(string text)
		{
            topLabel.text = text;

        }
		private void SetUI(int now , int max, Image image, TextMeshProUGUI textMeshProUGUI)
		{
			Mathf.Clamp(now, 0, max);
			Mathf.Clamp(max, 0, max);
			if (image != null)
			{
				if (max == 0)
					image.fillAmount = 1;
				else
					image.fillAmount = (float)now / max;
			}
			Debug.Log("fillAmount " + image.fillAmount + now + "/" + max);
            textMeshProUGUI.text = now.ToString();
		}

		private void Awake()
		{
            //_generateButton.Text = "summon_button".Localize();
            //_speedUpButton.Text = "speedup_text".Localize();
            //_upgradesButton.Text = "upgrades_text".Localize();
            //_summonsButton.Text = "lucky_button".Localize();
            //_needSummonWaveCoins.SetItem(Game.Static.Items.WaveCoin);
            GoButton.SetInteractable(false);

            _btnExit.SetOnClick(() =>
			{
				Game.Instance.EndGame();
			});
			
			

			/**
			
			
			Transform parentTransform = this.transform;
						
			
			_itemPrefab.SetActive(true);
			var itemView = Instantiate(_itemPrefab, _itemPrefab.transform.parent);
			
			itemView.transform.SetParent(parentTransform, false); // false сохраняет локальные координаты
			RectTransform rectTransform = itemView.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(300, 300); // Устанавливаем размер
			rectTransform.anchoredPosition = new Vector2(0, -200); // Позиция кнопки относительно родителя
			
			
			
			_itemPrefab.SetActive(false);
			*/
			
			/**
			// Создаем объект кнопки
			GameObject buttonObject = new GameObject("DynamicButton");

			// Добавляем его как дочерний к указанному компоненту
			buttonObject.transform.SetParent(parentTransform, false); // false сохраняет локальные координаты

			// Добавляем компонент Button и Image (для отображения кнопки)
			Button button = buttonObject.AddComponent<Button>();
			Image buttonImage = buttonObject.AddComponent<Image>();
			buttonImage.color = Color.green; // Устанавливаем цвет фона кнопки

			// Настраиваем RectTransform
			RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(300, 300); // Устанавливаем размер
			rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки относительно родителя

			// Добавляем текст на кнопку
			GameObject textObject = new GameObject("ButtonText");
			textObject.transform.SetParent(buttonObject.transform, false);

			Text buttonText = textObject.AddComponent<Text>();
			buttonText.text = "Click Me!";
			buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Встроенный шрифт
			buttonText.alignment = TextAnchor.MiddleCenter;
			buttonText.color = Color.black;

			RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
			textRectTransform.sizeDelta = rectTransform.sizeDelta; // Размер текста совпадает с кнопкой
			textRectTransform.anchoredPosition = Vector2.zero; // Центр текста внутри кнопки

			// Добавляем обработчик нажатия
			button.onClick.AddListener(() => Debug.Log("Button clicked!"));			
			*/
			
			
			/**
			
			_itemPrefab.SetActive(true);
			foreach (var item in Game.Static.Items.All.Values)
			{
				var itemView = Instantiate(_itemPrefab, _itemPrefab.transform.parent);
				itemView.Icon.LoadItemImage(item);
				var i = item;
			
				itemView.SetOnClick(() =>
				{
					var cnt = i.Type == ItemType.UNIT ? 1000 : 100000;
					Game.QueryManager.RequestPromise(new TestAddItemOperation(i.Id, cnt))
						.Then(r =>
						{
							Game.ServerDataUpdater.Update(r);
						});
				});
			
				item.UserReactive().Subscribe(_ =>
				{
					itemView.Text = $"{item.ModelId} ({item.UserAmount()})";
				}).AddTo(itemView.gameObject);
			}
			_itemPrefab.SetActive(false);
			*/
			
			
			
			/** JAVA
			Game.Instance.Playfiled.Subscribe(_ =>
			{
				var playfield = Game.Instance.Playfiled.Value;
				
				if (!playfield)
					return;

				
				playfield.MonstersMaxCount.Subscribe(val =>
				{
					_monstersSlider.maxValue = val;
				}).AddTo(playfield.gameObject);
				
				_unitInfoPanel.SetActive(false);
				
				playfield.WaveController.CurrentWave.Subscribe(_ =>
				{
					_currentWave.text = "wave_number".Localize(playfield.WaveController.CurrentWave.Value.Id.ToString());
					_waveBossTimerBack.SetActive(playfield.WaveController.CurrentWave.Value.Boss != null);
				}).AddTo(playfield.gameObject);
				
				playfield.WaveController.TimeLeftToWaveEnd.Subscribe(_ =>
				{
					_waveTimeLeft.text = ((long) playfield.WaveController.TimeLeftToWaveEnd.Value).GetNumericTime(false);
				}).AddTo(playfield.gameObject);
				
				var selector = playfield.gameObject.GetComponentInChildren<UnitPositionSelector>();
				selector.SelectedUnit.Subscribe(unitGroup =>
				{
					if (unitGroup)
					{
						_unitInfoPanel.SetActive(true);
						_unitInfoPanel.Init(unitGroup.Units.First().Data);
					}
					else
						_unitInfoPanel.SetActive(false);
				}).AddTo(playfield.gameObject);
				
				playfield.WaveController.Monsters.ObserveCountChanged(true).Subscribe(_ =>
				{
					_monstersCount.text = $"{playfield.WaveController.MonstersCount}/{playfield.MonstersMaxCount}";
					_monstersSlider.value = playfield.WaveController.MonstersCount;

					if (playfield.WaveController.MonstersCount >= Game.Settings.MonstersRedCount)
						_monstersCountSliderFill.sprite = _monstersSliderRed;
					else if (playfield.WaveController.MonstersCount >= Game.Settings.MonstersYellowCount)
						_monstersCountSliderFill.sprite = _monstersSliderYellow;
					else
						_monstersCountSliderFill.sprite = _monstersSliderGreen;
				}).AddTo(playfield.gameObject);
				
				playfield.Player.SummonController.SummonPrice.Subscribe(_ =>
				{
					_needSummonWaveCoins.Count = playfield.Player.SummonController.SummonPrice.Value;
				}).AddTo(playfield.gameObject);
				
				playfield.Player.UnitsCount.Subscribe(_ => updateUnitsCount()).AddTo(playfield.gameObject);
				playfield.UnitsMaxCount.Subscribe(_ => updateUnitsCount()).AddTo(playfield.gameObject);

				void updateUnitsCount()
				{
					_unitsCount.text = $"{playfield.Player.UnitsCount.Value}/{playfield.UnitsMaxCount}";
				}

				
			}).AddTo(this);
			
			_generateButton.SetOnClick(() =>
			{
				var playfield = Game.Instance.Playfiled.Value;

				if (!playfield)
					return;

				if (playfield.Player.SummonController.CanSummon)
					playfield.Player.SummonController.Summon();
			});
			
			_speedUpButton.SetOnClick(() =>
			{
				Time.timeScale = Time.timeScale.Equals(1f) ? 2f : 1f;
			});
			
			_upgradesButton.SetOnClick(() => { _upgradesView.SetActive(true); });
			_upgradesView.SetActive(false);
			
			_summonsButton.SetOnClick(() => { _summonsView.SetActive(true); });
			_summonsView.SetActive(false);
			*/			
		}
	}
}