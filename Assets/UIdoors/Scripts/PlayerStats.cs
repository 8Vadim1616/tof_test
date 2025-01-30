using UnityEngine;

[CreateAssetMenu()]
public class PlayerStats : ScriptableObject
{
    [SerializeField] private float _speed; // Скорость игрока
    public float Speed => _speed;
}
