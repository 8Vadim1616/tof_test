using Assets.UIDoors;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
{
    [SerializeField] private Transform _startPoint; // Начальная точка, куда переходит игрок и где комната стыкуется с предыдущей
    public Transform StartPoint => _startPoint;

    [SerializeField] private Transform _centerPoint;
    public Transform CenterPoint => _centerPoint;

    [SerializeField] private Transform[] _doorPoints; // Точки дверей
    public Transform[] DoorPoints => _doorPoints;
    [SerializeField]
    private bool _isChestsRoom;
    public bool IsChestsRoom => _isChestsRoom;
    [SerializeField]
    private List<ClickableObject> _clickableDoors;
    public List<ClickableObject> ClickableDoors => _clickableDoors;//кнопки дверей
    [SerializeField]
    private List<ClickableObject> _clickableChests;
    public List<ClickableObject> ClickableChests => _clickableChests;//кнопки сундуков

    [SerializeField] float _unfadingDuration; // Время растуманивания дверей
    public float UnfadingDuration => _unfadingDuration;

    [SerializeField] float _hidingDuration; // Время исчезновения предыдущей комнаты
    public float HidingDuration => _hidingDuration;
    [SerializeField] Vector3 _hidingVelocity; // Скорость и направление ухода предыдущей комнаты
    public Vector3 HidingVelocity => _hidingVelocity;

    [SerializeField] Transform _startPlayerPoint;
    public Transform StartPlayerPoint => _startPlayerPoint;

}
