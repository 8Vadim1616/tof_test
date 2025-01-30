using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Assets.UIDoors
{
    public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action clicked;
        [SerializeField]
        private bool IsOpened = false;
        [SerializeField]
        private bool IsActive = true;
        [SerializeField]
        private Sprite OpenedSprite;
        [SerializeField]
        private Sprite ClosedSprite;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
            set { _spriteRenderer = value; }
        }
        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        public void Open()
        {
            IsOpened = true;
            if(OpenedSprite != null)
                SpriteRenderer.sprite = OpenedSprite;
        }
        public void Close()
        {
            IsOpened = false;
            if(ClosedSprite != null)
                SpriteRenderer.sprite = ClosedSprite;
        }
        public void SetActive(bool state)
        {
            IsActive = state;
            gameObject.SetActive(state);

        }
        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(gameObject.name + " Clicked!");
            if (!IsOpened && IsActive)
                clicked?.Invoke();
        }

      
    }
}
