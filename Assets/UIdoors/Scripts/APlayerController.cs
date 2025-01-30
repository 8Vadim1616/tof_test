using System;
using UnityEngine;

public abstract class APlayerController : MonoBehaviour
{
    public abstract PlayerStats Stats { get; }
    public abstract void MoveToPosition(Vector3 position, Action onCompleted = null); // Передвигает игрока в указанную позицию
}