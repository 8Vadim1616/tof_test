//using Assets.Scripts.UI.ControlElements;
//using Assets.Scripts.Utils;
//using UnityEngine;


//public class ButtonViewCreator
//{
//    private float buttonWidth = 400f;   // Ширина кнопки
//    private float buttonHeight = 200f;  // Высота кнопки
//    private float buttonGap = 50f;     // Расстояние между кнопками

//    public ButtonTextIcon buttonPrefab; // Префаб кнопки
//    private Transform _container;
//    public void Init(ButtonTextIcon buttonPrefab, Transform _container)
//	{
//		this.buttonPrefab = buttonPrefab;
//        this._container = _container;
//	}
//    public RectTransform CreateFightButton()
//    {
//        RectTransform rectTransform;
//        buttonPrefab.SetActive(true);
//        var fightButton = GameObject.Instantiate(buttonPrefab, _container);
//        fightButton.Text = "Win Fight";
//        rectTransform = fightButton.GetComponent<RectTransform>();
//        rectTransform.anchoredPosition = new Vector2(0, 0); // Позиция кнопки в Canvas
//        rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight); // Размер кнопки
//        fightButton.onClick.AddListener(() => DoWin(fightButton.gameObject));
//        buttonPrefab.SetActive(false);
//        return rectTransform;
//    }
//}