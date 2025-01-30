using System;
using UnityEngine;

[Serializable]
public class RoomView
{
    public Material[] FogMaterials; // Для работы с прозрачностью частиц необходимы материалы частиц
    public Transform roomMeshTransform;
}
