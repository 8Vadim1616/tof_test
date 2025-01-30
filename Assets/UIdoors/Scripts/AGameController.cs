
using System;
using UnityEngine;

public abstract class AGameController : MonoBehaviour
{
    public abstract void MoveNextRoom(Action onCompleted = null); // Активирует перемещение в следующую комнату
    public abstract void MoveToRoom(ARoomController toRoomController, int throughTheDoorNum,Action onCompleted = null); // Активирует перемещение в выбранную комнату
}