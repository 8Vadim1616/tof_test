using System;
using System.Collections;
using UnityEngine;

public class PlayerController : APlayerController
{
    [SerializeField] private PlayerStats _stats;
    public override PlayerStats Stats => _stats;

    public override void MoveToPosition(Vector3 position, Action onCompleted = null)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(position, onCompleted));
    }

    IEnumerator MoveRoutine(Vector3 position, Action onCompleted = null)
    {
        while(Vector3.Distance(transform.position, position) > Stats.Speed * Time.deltaTime) // Линейное перемещение игрока
        {
            transform.Translate(Vector3.Normalize(position - transform.position) * Stats.Speed * Time.deltaTime, Space.World);
            yield return new WaitForEndOfFrame();
        }
        transform.position = position;
        
        onCompleted?.Invoke();
    }

}

