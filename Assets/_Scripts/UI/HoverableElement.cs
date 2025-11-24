using SerializeReferenceEditor;
using UnityEngine.EventSystems;
using UnityEngine;

namespace CarePackage.UI
{
    public class HoverableElement : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private bool toggleAble;
        [SerializeReference, SR] private IHoverBehavior[] hoverStrategy;
        [SerializeField] private SO_HoverStyle presetStyle;

        private bool _toggled;

        private void Start()
        {
            if (presetStyle == null) return;
            var styleBehaviors = presetStyle.ApplyStyle(gameObject);
            if (hoverStrategy == null || hoverStrategy.Length == 0)
            {
                hoverStrategy = styleBehaviors;
            }
            else
            {
                var merged = new IHoverBehavior[hoverStrategy.Length + styleBehaviors.Length];
                hoverStrategy.CopyTo(merged, 0);
                styleBehaviors.CopyTo(merged, hoverStrategy.Length);
                hoverStrategy = merged;
            }
            
            foreach (var strategy in hoverStrategy)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
            }
        }

        private void OnEnable()
        {
            foreach (var strategy in hoverStrategy)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (var strategy in hoverStrategy)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnDisable();
            }
        }

        private void OnValidate()
        {
            if (presetStyle == null) return;

            var styleBehaviors = presetStyle.ApplyStyle(gameObject);
            
            if (hoverStrategy == null || hoverStrategy.Length == 0)
            {
                hoverStrategy = styleBehaviors;
                return;
            }
            if (AreBehaviorsEqual(hoverStrategy, styleBehaviors)) return;
            
            var merged = new IHoverBehavior[hoverStrategy.Length + styleBehaviors.Length];
            hoverStrategy.CopyTo(merged, 0);
            styleBehaviors.CopyTo(merged, hoverStrategy.Length);
            hoverStrategy = merged;
            
            foreach (var strategy in hoverStrategy)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
            }
        }
        
        private bool AreBehaviorsEqual(IHoverBehavior[] a, IHoverBehavior[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].GetType() != b[i].GetType()) return false;
            }
            return true;
        }

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