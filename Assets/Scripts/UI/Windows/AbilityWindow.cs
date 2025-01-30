using System.Collections.Generic;
using Assets.Scripts.Gameplay;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Ability;
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
	public class AbilityWindow : AbstractWindow
	{
		public ButtonTextIcon buttonPrefab; // Префаб кнопки

		private PlayfieldView _playfield;
		private UserAbility _ability;
		
		/**
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
		*/

		private float buttonWidth = 700f;   // Ширина кнопки
		private float buttonHeight = 150f;  // Высота кнопки
		private float buttonGap = 50f;	   // Расстояние между кнопками

		public Promise<UserAbility> onResponse { get; private set; } = new();
		
		/**
		public static AbilityWindow Of(UserAbility ability) =>
						Game.Windows.ScreenChange<AbilityWindow>(false, w => w.ShowChoice());
		*/
		public static AbilityWindow Of(PlayfieldView playfield,  UserAbility ability) =>
			Game.Windows.ScreenChange<AbilityWindow>(false, w => w.Init(playfield, ability));
		
		private void Init(PlayfieldView playfield, UserAbility ability)
		{
			_playfield = playfield;
			_ability = ability;
			ShowChoice();
		}

		/**
		private void Init()
		{
			float valY = buttonHeight + buttonGap;
			
			for (int i = 0; i < Game.User.Ability.Select.Count; i++)
			{
				UserAbilityItem abilityItem = Game.User.Ability.Select[i]; 
				buttonPrefab.SetActive(true);

				var itemView = Instantiate(buttonPrefab, this.Content.transform);
				itemView.Text = abilityItem.Name;

				RectTransform rectTransform = itemView.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
				rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
				
				itemView.onClick.AddListener(() => DoChoice(abilityItem.Id));
				//itemView.SetOnClick(Close);

				buttonPrefab.SetActive(false);

				valY -= buttonHeight + buttonGap;
			}

			CanClose.Value = false;
		}
		*/

		private void Clear()
		{
			foreach (Transform child in this.Content.transform)
			{
				Destroy(child.gameObject);
			}
		}
		
		private void ShowChoice()
		{
			Clear();
			
			float valY = buttonHeight + buttonGap;

			RectTransform rectTransform;
			
			for (int i = 0; i < _ability.Select.Count; i++)
			{
				UserAbilityItem abilityItem = _ability.Select[i]; 
				buttonPrefab.SetActive(true);

				var choiceButton = Instantiate(buttonPrefab, this.Content.transform);
				choiceButton.Text = abilityItem.Name;

				rectTransform = choiceButton.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
				rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
				
				choiceButton.onClick.AddListener(() => DoChoice(abilityItem.Id));
				//itemView.SetOnClick(Close);

				buttonPrefab.SetActive(false);

				valY -= buttonHeight + buttonGap;
			}
			
			buttonPrefab.SetActive(true);

			var summaryButton = Instantiate(buttonPrefab, this.Content.transform);
			summaryButton.Text = "Abilities";

			rectTransform = summaryButton.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(0, -550); // Позиция кнопки в Canvas
			rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
				
			summaryButton.onClick.AddListener(() => ShowAll());
			buttonPrefab.SetActive(false);
			
		}

		
		private void ShowAll()
		{
			Clear();
			float valY = 500;

			RectTransform rectTransform;


			for (int i = 0; i < _ability.List.Count; i++)
			{
				UserAbilityItem abilityItem = _ability.List[i];


				// Создаем объект текста
				GameObject textObject = new GameObject("DynamicText");
				textObject.transform.SetParent(Content.transform, false); // Добавляем текст в Canvas

				// Добавляем компонент Text
				Text textComponent = textObject.AddComponent<Text>();

				// Настраиваем текст
				textComponent.text = abilityItem.Name; // Устанавливаем текст
				textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
				textComponent.fontSize = 37; // Размер шрифта
				textComponent.alignment = TextAnchor.MiddleLeft; // Выравнивание текста
				textComponent.color = Color.black; // Цвет текста

				// Настраиваем RectTransform текста
				rectTransform = textObject.GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(600, 42); // Размер области текста
				rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
				valY -= 51;
			}

			buttonPrefab.SetActive(true);

			var backButton = Instantiate(buttonPrefab, this.Content.transform);
			backButton.Text = "Back";

			rectTransform = backButton.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(0, -550); // Позиция кнопки в Canvas
			rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
				
			backButton.onClick.AddListener(() => ShowChoice());
			buttonPrefab.SetActive(false);
			
		}

		/**
		public UserAbility getResult()
		{
			return 
		}
		*/

		private void DoChoice(string id)
		{
			Game.QueryManager.RequestPromise(new AbilityChooseOperation(id))
				.Then(r =>
				{
					onResponse.Resolve(r.Ability);
					//_playfield.UpdateAbility(r.Ability);
					Close();
				}).Then(null);
		}
		
	}
}