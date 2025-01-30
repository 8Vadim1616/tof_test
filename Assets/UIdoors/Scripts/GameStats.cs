using UnityEngine;

namespace Assets.UIDoors
{
    [CreateAssetMenu()]

    public class GameStats : ScriptableObject
{
    [SerializeField] private float _cameraMoveTreshold = 0.5f; // Какую часть должна преодолеть камера, чтобы начать двигаться к двери
    public float CameraMoveTreshold => _cameraMoveTreshold;

    [SerializeField] private Vector3 _cameraDoorOffset;
    public Vector3 CameraDoorOffset => _cameraDoorOffset;
}
}