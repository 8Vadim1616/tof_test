using System;
using System.Collections;
//using Unity.VisualScripting;
using UnityEngine;

public class CameraController : ACameraController
{
    [SerializeField] private CameraStats _stats;
    public override CameraStats Stats => _stats;

    public override void MoveToPosition(Vector3 position, Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(position, onComplete));
    }

    // В реализации движения камеры требовалось
    // 1) Добавить настройку скорости камеры
    // 2) Добавить настройку ускорения камеры
    // 3) К моменту, когда персонаж достигает двери, камера полностью догоняет его.
    // Реализация 3 пункта на уровне скриптов противоречит 1 и 2 пункту. Можно только подогнать скорость и ускорение в редакторе

    IEnumerator MoveRoutine(Vector3 position, Action onComplete = null)
    {
        float fullDistance = Vector3.Distance(transform.position, position);
        float speed = 0;
        float distance = fullDistance;
        Vector3 startPosition = transform.position;

        while(speed >= 0 && distance > speed * Time.deltaTime) // При ускорении или замедлении мы учитываем варианты, когда камера может замедлиться до нуля или достичь цели
        {
            transform.Translate(Vector3.Normalize(position - transform.position) * speed * Time.deltaTime, Space.World);

            distance = Vector3.Distance(transform.position, position);
            if(speed < Stats.MaxSpeed)
            {
                // В случае невозможности достижения максимальной скорости следует обработать вариант замедления на полпути
                if(distance > fullDistance / 2)
                {
                    speed += Stats.Acceleration * Time.deltaTime;
                }
                else
                {
                    speed -= Stats.Acceleration * Time.deltaTime;
                }
            }
            else
            {
                if(distance > Stats.MaxSpeed * Stats.MaxSpeed / 2 / Stats.Acceleration) // Формула в правой части расчитывает расстояние, проходимое от 0 до максимальной скорости (теоретически можно вынести в отдельный метод)
                {
                    speed -= Stats.Acceleration * Time.deltaTime;
                }
                else
                {
                    speed = Stats.MaxSpeed;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        transform.position = position;
        onComplete?.Invoke();
    }
}
