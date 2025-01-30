using System;
using UnityEngine;
using UnityEngine.UI;

public class GoButton : MonoBehaviour
{
    [SerializeField] AGameController gameController;
    [SerializeField] Button button;
    public Action clicked;
    // Кнопка при нажатии переместит игрока, а на время перемещения станет неактивной
    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }
    public void OnClick()
    {
        clicked?.Invoke();
        button.interactable = false;
    }
    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}
