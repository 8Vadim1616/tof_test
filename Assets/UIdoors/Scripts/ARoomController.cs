using Assets.UIDoors;
using System;
using UnityEngine;

public abstract class ARoomController : MonoBehaviour
{
    public abstract Room Room { get; }
    public abstract RoomView RoomView { get; }
    public abstract void UnfadeDoor(int doorNumber, Action onCompleted = null);
    public abstract void SetOnPosition(Vector3 position);
    public abstract void HideRoom(int doorNumber, Action onCompleted = null);
    public abstract void SetRendererSortingOrder(int order);
    public abstract int GetDoorIndex(ClickableObject clickableDoor);
}