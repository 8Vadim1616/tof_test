using Assets.UIDoors;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class RoomController : ARoomController
{
    [SerializeField] Room _room;
    public override Room Room => _room;

    [SerializeField] RoomView _view;
    public override RoomView RoomView => _view;

    Vector3 startRoomScale = Vector3.one;
    Transform _previoudDoorPoint = null;
    
    void Start()
    {
        for(int i = 0; i < _view.FogMaterials.Length; i++)
        {
            ChangeFogAlpha(i, 1);
        }
        
    }

    public override void UnfadeDoor(int doorNumber, Action onCompleted = null)
    {
        StartCoroutine(UnfadingDoor(doorNumber, onCompleted));
    }

    IEnumerator UnfadingDoor(int doorNumber, Action onCompleted = null)
    {
        for(float t = 0; t < _room.UnfadingDuration; t += Time.deltaTime)
        {
            float fogAlpha = (_room.UnfadingDuration - t) / _room.UnfadingDuration;
            ChangeFogAlpha(doorNumber, fogAlpha);
            Debug.Log("new fog alpha " + fogAlpha);
            yield return new WaitForEndOfFrame();
        }
        ChangeFogAlpha(doorNumber, 0);
        onCompleted?.Invoke();
    }

    

    public override void SetOnPosition(Vector3 position)
    {
        transform.position = position - (_room.StartPoint.position - transform.position);
        
        for(int i = 0; i < _view.FogMaterials.Length; i++)
        {
            ChangeFogAlpha(i, 1);
        }

        if(_previoudDoorPoint != null)
        {
            _previoudDoorPoint.localScale = startRoomScale;
        }
        gameObject.SetActive(true);
    }

    public override void HideRoom(int doorNumber, Action onCompleted = null)
    {
        // gameObject.SetActive(false);
        // onCompleted?.Invoke();
        StartCoroutine(Hiding(doorNumber, onCompleted));
    }

    IEnumerator Hiding(int doorNumber, Action onCompleted = null)
    {
        _view.roomMeshTransform.parent = _room.DoorPoints[doorNumber];
        startRoomScale = _room.DoorPoints[doorNumber].localScale;
        _previoudDoorPoint = _room.DoorPoints[doorNumber];

        for(float t = 0; t < _room.HidingDuration; t += Time.deltaTime)
        {
            _room.DoorPoints[doorNumber].localScale += _room.HidingVelocity * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        for(int i = 0; i < _view.FogMaterials.Length; i++)
        {
            ChangeFogAlpha(i, 0);
        }
        gameObject.SetActive(false);
        onCompleted?.Invoke();
    }

    public override void SetRendererSortingOrder(int order)
    {
        // _view.Renderer.sortingOrder = order;
        // foreach(var renderer in _view.FogRenderers)
        // {
        //     renderer.sortingOrder = order - 1;
        // }
    }
    
    private void ChangeFogAlpha(int doorNumber, float a)
    {
        Color color = _view.FogMaterials[doorNumber].color;
        color.a = a;
        _view.FogMaterials[doorNumber].color = color;
    }

    public override int GetDoorIndex(ClickableObject clickableDoor)
    {
        for (int i = 0; i < Room.ClickableDoors.Count; i++)
        {
            if (Room.ClickableDoors[i] == clickableDoor)
                return i;
        }
        return -1;
    }
}
