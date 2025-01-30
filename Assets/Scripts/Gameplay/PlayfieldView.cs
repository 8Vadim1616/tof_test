using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Tower;
using Assets.Scripts.UI;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.HUD;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Gameplay;
using Gameplay.Components;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Windows.Artifacts;
using NSubstitute.ClearExtensions;
using System.Xml.Linq;
using System.Reflection;
using NSubstitute.ReceivedExtensions;
using System.Text;

namespace Assets.Scripts.Gameplay
{
	public class PlayfieldView : MonoBehaviour
	{
		
		//[SerializeField] private ButtonTextIcon _itemPrefab;
		
		[SerializeField] private PlayerComponent _player;
		public PlayerComponent Player => _player;
		
		private PlayerComponent _bot;
		/**
		[SerializeField] private PlayerComponent _bot;
		public PlayerComponent Bot => _bot;
		*/
        //UI doors
		private Assets.UIDoors.GameController _gameController;
		private List<Assets.UIDoors.ClickableObject> openedDoors = new();
		private List<Assets.UIDoors.ClickableObject> openedChests = new();
        private int exitDoorIndex;
        private InGameContent InGameContent => Game.HUD.Content.InGameContent;


        [SerializeField] private Transform _monstersContainer;
		public Transform MonstersContainer => _monstersContainer;

		public WaveController WaveController { get; private set; }
		public PlayfieldPoolManager PoolManager { get; private set; }
		public GameStats Stats { get; private set; }
		public IntReactiveProperty UnitsMaxCount { get; } = new();
		public IntReactiveProperty MonstersMaxCount { get; } = new ();

		

        private float buttonWidth = 400f;   // Ширина кнопки
		private float buttonHeight = 200f;  // Высота кнопки
		private float buttonGap = 50f;	   // Расстояние между кнопками
		
		public ButtonTextIcon buttonPrefab; // Префаб кнопки
		public Sprite imageSpriteBoss;
		public Sprite imageSpriteFight;
		public Sprite imageSpriteRest;
		public Sprite imageSpriteHell;
		
		
		private Transform _container;
		private UserTower _tower;
		private UserFloor _floor;
		private UserAbility _ability;
 
		private int _attempts;

		private int _exp;
		private int _money1;

		private GameObject _itemsPanel;
		private GameObject _statsPanel;

		private Dictionary<int, int> statsMap;		

		public static void StartGame()
		{
			if (Game.Instance.Playfiled.Value)
				Destroy(Game.Instance.Playfiled.Value.gameObject);

				Game.QueryManager.RequestPromise(new LevelStartOperation())
					.Then(r =>
					{
						//Game.ServerDataUpdater.Update(r);
						var playfield = Instantiate(Game.BasePrefabs.PlayfieldPrefab);
						Game.Instance.Playfiled.Value = playfield;
						playfield.Init(r.Tower, r.Ability);
					 });
		}

		public static IPromise EndGame()
		{
			if (!Game.Instance.Playfiled.Value)
				return Promise.Rejected(null);
			
			return Game.QueryManager.RequestPromise(new LevelEndOperation(Game.Instance.Playfiled.Value.Stats))
				.Then(r =>
				 {
					 Game.ServerDataUpdater.Update(r);
					 LevelEndWindow.Of(Game.Instance.Playfiled.Value.Stats, r.GetDrop()).ClosePromise.Then(() =>
					 {
						 Destroy(Game.Instance.Playfiled.Value.gameObject);
						 Game.Instance.Playfiled.Value = null;
					 });
				 }).Then(null);
		}
		
		public void Init(UserTower tower, UserAbility ability)
		{
			_container = Game.HUD.HudTopLayer.transform;
			_tower = tower;
			_ability = ability;
			_exp = 0;
			_money1 = 0;
			Stats = new GameStats(this);

			foreach (FloorItem item in _tower.Items)
			{
				Stats.Items.Add(item.Id, item.Value);
			}
			_gameController = Game.GameController;
            _gameController.gameObject.SetActive(true);
            NextFloor(true);
            /** JAVA
			_player.Init(Game.User);
			_bot.Init(coop);
			*/
        }


		public void UpdateAbility(UserAbility ability)
		{
			_ability = ability;
		}
		
		private void Clear()
		{
			foreach (Transform child in _container)
			{
				Destroy(child.gameObject);
			}
		}

        private void DoChoice()
		{
            AddItems(_floor.Actions[_attempts].Items);
            ShowLootPopup(_floor.Actions[_attempts].Items);
            _attempts++;
            if (_attempts == _floor.Exit)
            {
                CreateDoorbtnToNextFloor();
            }
         }
        private void DoChoice(GameObject btn, bool destroy = false)
        {
            // Создаем объект текста
            CreateTextForButton(btn); // Позиция текста относительно центра Canvas
            if (destroy)
                Destroy(btn);
            DoChoice();
        }

        private void DoWin(GameObject gameObject)
	    {
		    Destroy(gameObject);
		    AddItems(_floor.Actions[_attempts].Items);
		    ShowLootPopup(_floor.Actions[_attempts].Items);
			CreateDoorbtnToNextFloor();
	    }
	    
	    private void DoTreasure(GameObject gameObject)
	    {
		    Destroy(gameObject);
		    AddItems(_floor.Actions[_attempts].Items);
		    ShowLootPopup(_floor.Actions[_attempts].Items);
		    CreateDoorbtnToNextFloor();
	    }
        private void NextFloor(bool delayed = true)
	    {
            Item item = Game.Static.Items.Money1;
		    TowerItem ti = Game.Static.TowerItems.Money1;

		    Boolean res1 = item.IsMoney1;
		    Boolean res2 = ti.IsMoney1;
		    
		    int level=0;
		    if (_floor == null)
		    {
			    level = 1;
		    }
		    else
		    {
			    Stats.AddFloorStats(_floor.Level, _attempts);
			    level = _floor.Level + 1;
		    }
		    _attempts = 0;
		    

		    if (level > _tower.Floors.Count)
		    {
			    Clear();
			    EndGame();
		    }
		    else
            {
                _floor = _tower.Floors[level - 1];
                Clear();

                //UIdoors
                foreach (var door in openedDoors)
                {
                    door.Close();
                    door.SetActive(false);
                }
                foreach (var chest in openedChests)
                {
                    chest.Close();
                    chest.SetActive(false);
                }

                UIDoors.GameController.RoomRules roomRules = new UIDoors.GameController.RoomRules();
                switch (_floor.Type.SharpId)
                {
                    case "doors":
                        roomRules.ChestsRoomsAvailable = false;
                        roomRules.NoChestsRoomsAvailable = true;
                        break;
                    case "chests":
                        roomRules.ChestsRoomsAvailable = true;
                        roomRules.NoChestsRoomsAvailable = false;
                        break;
                    default:
                        roomRules = null;
                        break;
                }
                
                ARoomController roomController = _gameController.GetRandomAvailableRoomController(roomRules);
                DisableClickablesInRoom(_gameController.CurrentRoomController);
                DisableClickablesInRoom(roomController);

                int exitDoorIndex = GetExitDoorIndex();
                doorToExit = _gameController.CurrentRoomController.Room.ClickableDoors[exitDoorIndex];
                if (delayed)
                    _gameController.MoveToRoom(roomController, exitDoorIndex, ShowNextFloor);
                else
                    ShowNextFloor();
                //_gameController.MoveNextRoom(NextFloor);
                /**
                AbilityWindow.Of().ClosePromise.Then(() =>
                {
                    ShowNextFloor();
                });
                */


                /**
                Game.QueryManager.RequestPromise(new AbilitySelectOperation())
                    .Then(r =>
                    {
                        Game.ServerDataUpdater.Update(r);
                        
                        AbilityWindow.Of().ClosePromise.Then(() =>
                        {
                            ShowNextFloor();
                            //Destroy(Game.Instance.Playfiled.Value.btn);
                            //Game.Instance.Playfiled.Value = null;
                        });
                    }).Then(null);
                    */

            }
        }

        private void DisableClickablesInRoom(ARoomController aRoomController)
        {
            foreach (Assets.UIDoors.ClickableObject roomDoor in aRoomController.Room.ClickableDoors)
            {
                roomDoor.Close();
                roomDoor.SetActive(false);
            }
            foreach (Assets.UIDoors.ClickableObject chest in aRoomController.Room.ClickableChests)
            {
                chest.Close();
                chest.SetActive(false);
            }
        }

        private int GetExitDoorIndex()
        {
            int exitDoorIndex;
            if (doorToExit == null)
            {
                int index = UnityEngine.Random.Range(0, _gameController.CurrentRoomController.Room.ClickableDoors.Count);
                exitDoorIndex = index;
            }
            else
                exitDoorIndex = _gameController.CurrentRoomController.GetDoorIndex(doorToExit);
            if (exitDoorIndex == -1)
            {
                Debug.LogAssertion(doorToExit.gameObject.name + " not found in " + _gameController.CurrentRoomController);  
                exitDoorIndex = 0;
            }

            return exitDoorIndex;
        }

        private void ShowStats()
	    {
		    foreach (Transform child in _statsPanel.transform)
		    {
			    Destroy(child.gameObject);
		    }
		    
		    //CreateLevelText();
		    
		    int valY = 500;

		    foreach (KeyValuePair<int, int> item in Stats.Items)
		    {
                TowerItem towerItem = Game.Static.TowerItems.Get(item.Key);
                //SET EXP HUD 
                HUDController hUDController = Game.HUD;
                HUDContent content = hUDController.Content;
                InGameContent inGameContent = content.InGameContent;
				string totalText;
                if (towerItem.IsExp)
			    {
					string ExpText = Game.Static.TowerItems.Get(item.Key).ModelId + ": " + (item.Value - _tower.GetLevel(Stats.Level).Exp) + " (" + ( _tower.GetLevel(Stats.Level+1).Exp -  _tower.GetLevel(Stats.Level).Exp) + ")"; // Устанавливаем текст
                    totalText = ExpText;

                    int now = item.Value - _tower.GetLevel(Stats.Level).Exp;
					int nextLevel = _tower.GetLevel(Stats.Level+1).Exp;
					inGameContent.SetExp(now, nextLevel);
                }
			    else 
                {
					string statString = Game.Static.TowerItems.Get(item.Key).ModelId;
					switch(statString)
					{

						case "HP":
							inGameContent.SetLife(item.Value, -1);
							break;
						case "MaxHP":
                            inGameContent.SetLife(-1, item.Value);
                            break;
						case "Attack":
                            inGameContent.SetAttack(item.Value, item.Value);
							break;
						case "Money1":
                            inGameContent.SetMoney(item.Value);
							break;
						case "Armor":
                            inGameContent.SetArmor(item.Value);
							break;
                    }

					Debug.Log("statString + " + statString);
                    totalText = statString + ": " + item.Value; // Устанавливаем текст
			    }

				//GameObject textObject = CreateStatDebugText(totalText, valY);

                // Настраиваем RectTransform текста
                valY -= 80;
		    }
	    }
        private void AddItems(List<FloorItem> items)
	    {
		    foreach (FloorItem item in items)
		    {
			    
			    
			    if (Stats.Items.ContainsKey(item.Id))
			    {
				    Stats.Items[item.Id] = Stats.Items[item.Id] + item.Value; // Обновляем значение
			    }
			    else
			    {
				    Stats.Items.Add(item.Id, item.Value); // Добавляем новый ключ
			    }	
			    
			    TowerItem towerItem = Game.Static.TowerItems.Get(item.Id);
			    if (towerItem.IsExp)
			    {
				    int level = Stats.Level;
				    int nextLevelExp = _tower.GetLevel(Stats.Level + 1).Exp;
				    if (Stats.Items[item.Id] >= nextLevelExp)
				    {
					    Stats.Level = Stats.Level + 1;
					    _container.SetActive(false);
					    AbilityWindow.Of(this, _ability).onResponse.Then((ability) =>
					    {
						    _ability = ability;
						    _container.SetActive(true);

					    });
					    /**
					    AbilityWindow.Of(this, _ability).ClosePromise.Then(() =>
					    {
						    
						    _container.SetActive(true);
					    });
					    */
				    }
			    }
		    }
		    ShowStats();
	    }
	    

	    private void ShowLootPopup(List<FloorItem> items)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (FloorItem item in items) {
                string text =   Game.Static.TowerItems.Get(item.Id).ModelId + ": " + item.Value; // Устанавливаем текст
                stringBuilder.AppendLine(text);
            }
            if ( !string.IsNullOrEmpty(stringBuilder.ToString()))
                InGameContent.SetAchievment(stringBuilder.ToString(), "Floor " + _floor.Level.ToString() + ":");
            //CreateTextForGrabbedItems(items);
        }
       
        private void ShowNextFloor()
        {
            foreach (var door in openedDoors)
            {
                door.SetActive(true);
            }
            openedDoors.Clear();

            CreateItemPanel();
            CreateStatsPanel();

            //CreateFloorTypeText();
            SetTopLabel("Floor " + _floor.Level.ToString());
            ShowStats();

            float valY = -(_floor.Actions.Count - 1) / (float)2 * (buttonHeight + buttonGap);
            //ButtonTextIcon itemView;
            switch (_floor.Type.SharpId)
            {
                case "doors":
                    
                    int i = 0;
                    foreach (UserFloorAction action in _floor.Actions)
                    {
                        Assets.UIDoors.ClickableObject door_i = GetDoor(i);
                        door_i.clicked = () => OnDoorClicked(door_i);
                        door_i.SetActive(true);
                        //CreateDoorbtnByWoldPosition(valY, label, i);
                        i++;
                        valY += buttonHeight + buttonGap;
                    }
                    break;
                case "chests":
                    Assets.UIDoors.ClickableObject door = GetDoor(0);
                    door.Close();
                    door.SetActive(false);
                    int j = 0;
                    foreach (UserFloorAction action in _floor.Actions)
                    {
                        Assets.UIDoors.ClickableObject chest = _gameController.CurrentRoomController.Room.ClickableChests[j];
                        chest.clicked = () => OnChestClicked(chest);
                        chest.SetActive(true);
                        j++;
                        //valY += buttonHeight + buttonGap;
                    }
                    break;
                case "rest":
                    ShowImage(imageSpriteRest);
                    CreateDoorbtnToNextFloor();
                    break;
                case "treasure":
                    //ShowImage(imageSpriteFight);
                    buttonPrefab.SetActive(true);
                    var treasureButton = Instantiate(buttonPrefab, _container);
                    treasureButton.Text = "Collect";
                    RectTransform treasureButtonRectTransform = treasureButton.GetComponent<RectTransform>();
                    treasureButtonRectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
                    treasureButtonRectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
                    treasureButton.onClick.AddListener(() => DoTreasure(treasureButton.gameObject));
                    buttonPrefab.SetActive(false);
                    break;
                case "fight":
                    ShowImage(imageSpriteFight);
                    RectTransform FightButtonrectTransform = CreateFightButton();
                    /**
					Game.QueryManager.RequestPromise(new LevelFightOperation("1"))
						.Then(r =>
						{
								buttonPrefab.SetActive(true);
								var itemView = Instantiate(buttonPrefab, _container);
								itemView.Text = "Win Fight";
								rectTransform = itemView.GetComponent<RectTransform>();
								rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
								rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
								itemView.onClick.AddListener(() => DoWin(itemView.btn));
								buttonPrefab.SetActive(false);
						}).Then(null);
					*/

                    break;
                case "boss":
                    ShowImage(imageSpriteBoss);
                    buttonPrefab.SetActive(true);
                    var bossButton = Instantiate(buttonPrefab, _container);
                    bossButton.Text = "Win Boss";
                    RectTransform bossButtonrectTransform = bossButton.GetComponent<RectTransform>();
                    bossButtonrectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
                    bossButtonrectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
                    bossButton.onClick.AddListener(() => DoWin(bossButton.gameObject));
                    buttonPrefab.SetActive(false);

                    /**
					Game.QueryManager.RequestPromise(new LevelFightOperation("1"))
						.Then(r =>
						{
							buttonPrefab.SetActive(true);
							var itemView = Instantiate(buttonPrefab, _container);
							itemView.Text = "Win Boss";
							rectTransform = itemView.GetComponent<RectTransform>();
							rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
							rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
							itemView.onClick.AddListener(() => DoWin(itemView.btn));
							buttonPrefab.SetActive(false);
						}).Then(null);
					*/
                    break;
                case "hell":
                    ShowImage(imageSpriteHell);
                    CreateDoorbtnToNextFloor();
                    break;
                default:
                    Debug.Log("Score is undefined.");
                    break;
            }
        }
        UIDoors.ClickableObject doorToExit;
        private void OnDoorClicked(Assets.UIDoors.ClickableObject clickableDoor)
        {
            DoChoice();
            if (_attempts == _floor.Exit)
            {
                doorToExit = clickableDoor;
            }
            //clickableDoor.clicked -= DoChoice;
            openedDoors.Add(clickableDoor);
            clickableDoor.Open();
            clickableDoor.SetActive(false);


        }
        private void OnChestClicked(Assets.UIDoors.ClickableObject chest)
        {
            DoChoice();
            if (_attempts == _floor.Exit)
            {
                doorToExit = GetDoor(0);
            }
            openedChests.Add(chest);
            chest.Open();
            //chest.SetActive(false);


        }

        private UIDoors.ClickableObject GetDoor(int i)
        {
			UIDoors.ClickableObject door;
            if (_gameController.CurrentRoomController.Room.DoorPoints.Count() > i)
                door = _gameController.CurrentRoomController.Room.ClickableDoors[i];
            else
            {
                Debug.LogAssertion(" doorsPoinstCount less then _floor.Actions");
				door = _gameController.CurrentRoomController.Room.ClickableDoors[0];
            }
            return door;
        }


        private Vector3 GetDoorPosition(int i)
        {
            Vector3 doorPosition;
            if (_gameController.CurrentRoomController.Room.DoorPoints.Count() > i)
                doorPosition = _gameController.CurrentRoomController.Room.DoorPoints[i].position;
            else
            {
                Debug.LogAssertion(" doorsPoinstCount less then _floor.Actions");
                doorPosition = _gameController.CurrentRoomController.Room.DoorPoints[0].position;
            }

            return doorPosition;
        }

        private void SetTopLabel(string text)
        {
            InGameContent.SetTopLabelText(text);
        }


        private string GetLabel()
        {
            string label = "door";
            if (_floor.Type.SharpId.Equals("chests"))
            {
                label = "Chest";
            }

            return label;
        }
        #region DynamicUI
        private void CreateDoorbtnToNextFloor()
        {
            InGameContent.GoButton.gameObject.SetActive(true);
            InGameContent.GoButton.SetInteractable(true);
            InGameContent.GoButton.clicked = () => { NextFloor(true); InGameContent.GoButton.SetInteractable(false); };
            if (false)
            {
                buttonPrefab.SetActive(true);

                var itemView = Instantiate(buttonPrefab, _container);
                itemView.Text = "Next";

                RectTransform rectTransform = itemView.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(buttonWidth + buttonGap, 0); // Позиция кнопки в Canvas
                rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки

                itemView.onClick.AddListener(() => NextFloor(true));

                buttonPrefab.SetActive(false);
            }
        }
        private void CreateDoorbtnByWoldPosition(float valY, string label, int i)
        {
            Vector3 doorPosition = GetDoorPosition(i);
            CreateChoiceButtonUIDoors(valY, label, doorPosition);
        }
        private void CreateTextForButton(GameObject btn)
        {
            GameObject textObject = new GameObject("DynamicText");
            textObject.transform.SetParent(_container, false); // Добавляем текст в Canvas

            // Добавляем компонент Text
            Text textComponent = textObject.AddComponent<Text>();

            // Настраиваем текст
            textComponent.text = _floor.Actions[_attempts].SharpId; // Устанавливаем текст
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
            textComponent.fontSize = 46; // Размер шрифта
            textComponent.alignment = TextAnchor.MiddleCenter; // Выравнивание текста
            textComponent.color = Color.black; // Цвет текста

            // Настраиваем RectTransform текста
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100); // Размер области текста
            rectTransform.anchoredPosition = btn.GetComponent<RectTransform>().anchoredPosition;
        }

        private void ShowImage(Sprite imageSprite)
        {
            // Создаём объект Image
            GameObject imageObject = new GameObject("DynamicImage");
            imageObject.transform.SetParent(_container, false); // Добавляем в Canvas

            // Добавляем компонент Image
            Image imageComponent = imageObject.AddComponent<Image>();

            // Устанавливаем спрайт для Image
            imageComponent.sprite = imageSprite;

            // Настраиваем размеры и позицию
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 800); // Размер изображения
            rectTransform.anchoredPosition = new Vector2(0, 0); // Центр Canvas	    


        }
        private GameObject CreateStatDebugText(string totalText, int valY)
        {
            GameObject textObject = new GameObject("DynamicText");
            textObject.transform.SetParent(_statsPanel.transform, false); // Добавляем текст в Canvas

            // Добавляем компонент Text
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
            textComponent.fontSize = 40; // Размер шрифта
            textComponent.alignment = TextAnchor.MiddleLeft; // Выравнивание текста
            textComponent.color = Color.black; // Цвет текста
            textComponent.text = totalText;

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(350, 100); // Размер области текста
            rectTransform.anchoredPosition = new Vector2(-300, valY); // Позиция кнопки в Canvas
            return textObject;
        }
        private void CreateLevelText()
        {
            int valY = 800;

            // Создаем объект текста
            GameObject textObject = new GameObject("DynamicText");
            textObject.transform.SetParent(_statsPanel.transform, false); // Добавляем текст в Canvas

            // Добавляем компонент Text
            Text textComponent = textObject.AddComponent<Text>();

            // Настраиваем текст
            textComponent.text = "Level " + Stats.Level; // Устанавливаем текст
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
            textComponent.fontSize = 60; // Размер шрифта
            textComponent.alignment = TextAnchor.MiddleCenter; // Выравнивание текста
            textComponent.color = Color.black; // Цвет текста

            // Настраиваем RectTransform текста
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100); // Размер области текста
            rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
        }
        private void CreateFloorTypeText()
        {
            // Создаем объект текста
            GameObject textObject = new GameObject("DynamicText");
            textObject.transform.SetParent(_container, false); // Добавляем текст в Canvas

            // Добавляем компонент Text
            Text textComponent = textObject.AddComponent<Text>();

            // Настраиваем текст
            textComponent.text = _floor.Type.SharpId; // Устанавливаем текст
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
            textComponent.fontSize = 80; // Размер шрифта
            textComponent.alignment = TextAnchor.MiddleCenter; // Выравнивание текста
            textComponent.color = Color.black; // Цвет текста

            // Настраиваем RectTransform текста
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100); // Размер области текста
            rectTransform.anchoredPosition = new Vector2(0, 700); // Позиция кнопки в Canvas
        }

        /// <summary>
        /// создание плашки текста для поднятых предметов
        /// </summary>
        /// <param name="items"></param>
        private void CreateTextForGrabbedItems(List<FloorItem> items)
        {
            foreach (Transform child in _itemsPanel.transform)
            {
                Destroy(child.gameObject);
            }

            int valX = -380;
            foreach (FloorItem item in items)
            {
                // Создаем объект текста
                GameObject textObject = new GameObject("DynamicText");
                textObject.transform.SetParent(_itemsPanel.transform, false); // Добавляем текст в Canvas

                // Добавляем компонент Text
                Text textComponent = textObject.AddComponent<Text>();

                // Настраиваем текст
                textComponent.text = Game.Static.TowerItems.Get(item.Id).ModelId + ": " + item.Value; // Устанавливаем текст

                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Используем Arial
                textComponent.fontSize = 40; // Размер шрифта
                textComponent.alignment = TextAnchor.MiddleLeft; // Выравнивание текста
                textComponent.color = Color.black; // Цвет текста

                // Настраиваем RectTransform текста
                RectTransform rectTransform = textObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 100); // Размер области текста
                rectTransform.anchoredPosition = new Vector2(valX, 0); // Позиция кнопки в Canvas
                valX += 200;
            }
        }
        private void CreateStatsPanel()
        {
            _statsPanel = new GameObject("StatsPanel");

            // Добавляем компонент RectTransform (он будет добавлен автоматически)
            RectTransform rectTransform = _statsPanel.AddComponent<RectTransform>();

            // Добавляем компонент Image (фон для Panel)
            //panelImage = _statsPanel.AddComponent<Image>();
            //panelImage.color = new Color(0, 0, 0, 0.5f); // Полупрозрачный чёрный цвет

            // Устанавливаем Panel как дочерний объект Canvas
            _statsPanel.transform.SetParent(_container, false);

            // Настраиваем размеры Panel
            rectTransform.sizeDelta = new Vector2(200, 1200); // Размер 300x200
            rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
        }

        private void CreateChoiceButton(float valY, string label)
        {
            
            buttonPrefab.SetActive(true);

            var choiceButton = Instantiate(buttonPrefab, _container);
            choiceButton.Text = label;

            RectTransform rectTransform = choiceButton.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
            rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки

            choiceButton.onClick.AddListener(() => DoChoice(choiceButton.gameObject));

            /**
            item.UserReactive().Subscribe(_ =>
            {
                itemView.Text = $"{item.ModelId} ({item.UserAmount()})";
            }).AddTo(itemView.btn);
            */
            buttonPrefab.SetActive(false);
        }
        private void CreateChoiceButtonUIDoors(float valY, string label,Vector3 doorPosition)
        {
            buttonPrefab.SetActive(true);

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(doorPosition);
			screenPosition.y += 60;
            var choiceButton = Instantiate(buttonPrefab, _container);
            //choiceButton.Text = label;

            RectTransform rectTransform = choiceButton.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Game.MainCanvas.GetComponent<RectTransform>(), // RectTransform канваса
            screenPosition, // Экранные координаты
            Game.MainCanvas.worldCamera, // Камера канваса
            out Vector2 localPosition // Локальная позиция на канвасе
        );

			// Устанавливаем позицию RectTransform
            rectTransform.anchoredPosition = localPosition;
            //rectTransform.pre
            //rectTransform.anchoredPosition = new Vector2(0, valY); // Позиция кнопки в Canvas
            //rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки

            choiceButton.onClick.AddListener(() => DoChoice(choiceButton.gameObject));

            buttonPrefab.SetActive(false);
        }

        private RectTransform CreateFightButton()
        {
            RectTransform rectTransform;
            buttonPrefab.SetActive(true);
            var fightButton = Instantiate(buttonPrefab, _container);
            fightButton.Text = "Win Fight";
            rectTransform = fightButton.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
            rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
            fightButton.onClick.AddListener(() => DoWin(fightButton.gameObject));
            buttonPrefab.SetActive(false);
            return rectTransform;
        }

        private void CreateItemPanel()
        {
            RectTransform rectTransform;
            Image panelImage;

            _itemsPanel = new GameObject("ItemsPanel");

            // Добавляем компонент RectTransform (он будет добавлен автоматически)
            rectTransform = _itemsPanel.AddComponent<RectTransform>();

            // Добавляем компонент Image (фон для Panel)
            panelImage = _itemsPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f); // Полупрозрачный чёрный цвет

            // Устанавливаем Panel как дочерний объект Canvas
            _itemsPanel.transform.SetParent(_container, false);

            // Настраиваем размеры Panel
            rectTransform.sizeDelta = new Vector2(1000, 100); // Размер 300x200
            rectTransform.anchoredPosition = new Vector2(0, -800); // Позиция кнопки в Canvas
        }
        #endregion
        private void Awake()
		{
			/** JAVA
		
			UnitsMaxCount.Value = Game.Settings.UnitsMaxCount;
			MonstersMaxCount.Value = Game.Settings.MonstersMaxCount;
			
			WaveController = btn.AddComponent<WaveController>();
			WaveController.Init(this);

			AbilitiesController = btn.AddComponent<AbilitiesController>();
			AbilitiesController.Init(this);

			PoolManager = new PlayfieldPoolManager(this);
			*/
		}
		
		private void Update()
		{
			/** JAVA
			if (Stats.IsFinished)
				return;
			
			if (WaveController.MonstersCount >= MonstersMaxCount.Value ||
				(WaveController.CurrentWave.Value.Boss != null && WaveController.MonstersCount > 0 && WaveController.TimeLeftToWaveEnd.Value <= 0))
			{
				Stats.FinishGame(GameResult.LOSE);
				EndGame();
			}
			*/
		}


	}
}