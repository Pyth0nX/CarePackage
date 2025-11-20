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

    public interface IHoverBehavior
    {
        public void OnHovered(PointerEventData eventData);
        public void OnUnhovered(PointerEventData eventData);
        public void OnClicked(PointerEventData eventData);
        public void Reset();
    }

    public enum ScaleAxis
    {
        XY,
        X,
        Y
    }

    [System.Serializable]
    public class HoverScale : IHoverBehavior
    {
        [SerializeField] private ScaleAxis axis = ScaleAxis.XY;
        [SerializeField] private PrimeTween.Ease ease = PrimeTween.Ease.Linear;
        [SerializeField] private float scaleMultiplier = 1.2f;
        [SerializeField] private float duration = 0.2f;

        [SerializeField] private Transform _target;
        private Vector3 _originalScale;
        private PrimeTween.Tween _currentTween;
        private bool _isClicked;
        
        public HoverScale() : this(null) {}

        public HoverScale(Transform targetTransform)
        {
            _target = targetTransform;
            if (_target != null) _originalScale = _target.localScale;
            else _originalScale = Vector3.one;
        }

        public void OnHovered(PointerEventData eventData)
        {
            if (_isClicked) return;
            Vector3 targetScale = GetTargetScale();
            TweenTo(targetScale);
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            if (_isClicked) return;
            TweenTo(_originalScale);
        }

        public void OnClicked(PointerEventData eventData)
        {
            _isClicked = true;
            _currentTween.Stop();
            _target.localScale = GetTargetScale();
        }

        public void Reset()
        {
            _isClicked = false;
            TweenTo(_originalScale);
        }

        private Vector3 GetTargetScale()
        {
            return axis switch
            {
                ScaleAxis.XY => _originalScale * scaleMultiplier,
                ScaleAxis.X => new Vector3(_originalScale.x * scaleMultiplier, _originalScale.y, _originalScale.z),
                ScaleAxis.Y => new Vector3(_originalScale.x, _originalScale.y * scaleMultiplier, _originalScale.z),
                _ => _originalScale
            };
        }

        private void TweenTo(Vector3 targetScale)
        {
            if (_target.localScale == targetScale) return;
            
            _currentTween.Stop();
            _currentTween = PrimeTween.Tween.Scale(_target, targetScale, duration, ease);
        }
    }

    [System.Serializable]
    public class HoverColor : IHoverBehavior
    {
        [SerializeField] private Color hoveredColor = Color.white;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private bool useEasing = true;
        [SerializeField] private PrimeTween.Ease ease;

        [SerializeField] private UnityEngine.UI.Graphic _targetGraphic;
        private PrimeTween.Tween _currentTween;
        
        public HoverColor() : this(null) {}

        public HoverColor(UnityEngine.UI.Graphic graphic)
        {
            _targetGraphic = graphic;
            if (_targetGraphic != null) 
                _targetGraphic.color = defaultColor;
        }

        public void OnHovered(PointerEventData eventData)
        {
            TweenTo(hoveredColor);
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            TweenTo(defaultColor);
        }

        public void OnClicked(PointerEventData eventData)
        {
            _currentTween.Stop();
            _targetGraphic.color = hoveredColor;
        }

        public void Reset()
        {
            TweenTo(defaultColor);
        }

        private void TweenTo(Color targetColor)
        {
            if (_targetGraphic.color == targetColor) return;
            
            _currentTween.Stop();
            if (useEasing)
            {
                _currentTween = PrimeTween.Tween.Color(_targetGraphic, targetColor, duration, ease);
            }
            else
            {
                _targetGraphic.color = targetColor;
            }
        }
    }
}