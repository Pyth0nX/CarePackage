using SerializeReferenceEditor;
using UnityEngine.EventSystems;
using UnityEngine;

namespace CarePackage.UI
{
    public class HoverableElement : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private bool toggleAble;
        [SerializeReference, SR] private IHoverBehavior[] hoverStrategy;

        private bool _toggled;

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var strategy in hoverStrategy)
            {
                strategy.OnHovered(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (toggleAble)
            {
                UIManager.Instance.RegisterToggle(this);
            }
            
            foreach (var strategy in hoverStrategy)
            {
                strategy.OnClicked(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var strategy in hoverStrategy)
            {
                strategy.OnUnhovered(eventData);
            }
        }
        
        public void Toggle(bool self = true, bool toggle = false)
        {
            if (!self) _toggled = toggle;
            else
            {
                _toggled = !_toggled;
            }

            if (!toggle)
            {
                foreach (var strategy in hoverStrategy)
                {
                    strategy.Reset();
                }
            }
        }
    }
}