using System;
using UnityEngine;

public abstract class ACameraController : MonoBehaviour
{
    public abstract CameraStats Stats { get; }
    public abstract void MoveToPosition(Vector3 position, Action onComplete = null);
}