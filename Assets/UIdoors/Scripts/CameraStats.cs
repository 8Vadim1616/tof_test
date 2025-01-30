using UnityEngine;

[CreateAssetMenu()]
public class CameraStats : ScriptableObject
{
    [SerializeField] private float _accceleration;
    public float Acceleration => _accceleration;

    [SerializeField] private float _maxSpeed;
    public float MaxSpeed => _maxSpeed;
}
