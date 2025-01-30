using System;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.HUD
{
    public class JoystickView : FixedJoystick
    {
        [SerializeField] float floatingHandleRange = 3f;
        [SerializeField] bool hideHandleWhenInactive = true;

        private Action _onClick;
        private bool isDrag;
        private Vector2 baseAnchoredPosition;
        private Vector2 baseHanglePosition;

        protected override void Start()
        {
            base.Start();
            DeadZone = 0.1f;
            baseAnchoredPosition = background.anchoredPosition;
        }

        public void SetOnButtonClick(Action onClick)
        {
            _onClick = onClick;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            //background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
			background.position = eventData.position;
            base.OnPointerDown(eventData);
        }

        protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            base.HandleInput(magnitude, normalised, radius, cam);

            if (magnitude > DeadZone)
                isDrag = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (!isDrag)
                _onClick?.Invoke();

            isDrag = false;
            background.anchoredPosition = baseAnchoredPosition;
        }
    }
}