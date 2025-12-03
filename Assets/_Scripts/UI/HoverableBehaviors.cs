using UnityEngine.EventSystems;
using UnityEngine;
using System;
using PrimeTween;

namespace CarePackage.UI
{
    [Serializable]
    public class HoverScale : IHoverBehavior
    {
        [SerializeField] private ScaleAxis axis = ScaleAxis.XY;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private float scaleMultiplier = 1.2f;
        [SerializeField] private float duration = 0.2f;

        [SerializeField] private RectTransform target;
        private Vector3 _originalScale;
        private Tween _currentTween;
        private bool _isClicked;
        
        public HoverScale() : this(null) {}

        public HoverScale(RectTransform inTarget)
        {
            target = inTarget;
            if (target != null) _originalScale = target.localScale;
            else _originalScale = Vector3.one;
        }

        public void OnHovered(PointerEventData eventData)
        {
            if (_isClicked) return;
            Vector3 targetScale = GetTargetScale();
            TweenTo(targetScale);
        }
        
        public void OnClicked(PointerEventData eventData)
        {
            _isClicked = true;
            _currentTween.Stop();
            target.localScale = GetTargetScale();
            Reset();
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            if (_isClicked) return;
            Reset();
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
            if (target.localScale == targetScale) return;
            
            _currentTween.Stop();
            _currentTween = Tween.Scale(target, targetScale, duration, ease);
        }
    }

    [Serializable]
    public class HoverColor : IHoverBehavior, IActivatable
    {
        [SerializeField] private Color hoveredColor = Color.white;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private bool useEasing = true;
        [SerializeField] private Ease ease;

        [SerializeField] private UnityEngine.UI.Graphic _targetGraphic;
        private Tween _currentTween;

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
                _currentTween = Tween.Color(_targetGraphic, targetColor, duration, ease);
            }
            else
            {
                _targetGraphic.color = targetColor;
            }
        }

        public void OnEnable()
        {
            if (_targetGraphic == null) return;
            if (_targetGraphic.color == defaultColor) return;
            _targetGraphic.color = defaultColor;
        }

        public void OnDisable() {}
    }
    
    [Serializable]
    public class HoverFade : IHoverBehavior
    {
        [SerializeField] private CanvasGroup targetGroup;
        [SerializeField] private float hoveredAlpha = 1f;
        [SerializeField] private float defaultAlpha = 0.5f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private Tween _currentTween;

        public HoverFade() : this(null) {}

        public HoverFade(CanvasGroup group)
        {
            targetGroup = group;
            if (targetGroup != null) targetGroup.alpha = defaultAlpha;
        }

        public void OnHovered(PointerEventData eventData)
        {
            TweenTo(hoveredAlpha);
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            TweenTo(defaultAlpha);
        }

        public void OnClicked(PointerEventData eventData)
        {
            _currentTween.Stop();
            targetGroup.alpha = hoveredAlpha;
        }

        public void Reset()
        {
            TweenTo(defaultAlpha);
        }

        private void TweenTo(float alpha)
        {
            if (Mathf.Approximately(targetGroup.alpha, alpha)) return;
            _currentTween.Stop();
            _currentTween = Tween.Alpha(targetGroup, alpha, duration, ease);
        }
    }
    
    [Serializable]
    public class HoverColorTransition : IHoverBehavior
    {
        [SerializeField] private Color hoveredColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        [SerializeField] private UnityEngine.UI.Graphic targetGraphic;
        private Tween _currentTween;

        public HoverColorTransition() : this(null) {}

        public HoverColorTransition(UnityEngine.UI.Graphic graphic)
        {
            targetGraphic = graphic;
            if (targetGraphic != null) targetGraphic.color = defaultColor;
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
            targetGraphic.color = hoveredColor;
        }

        public void Reset()
        {
            TweenTo(defaultColor);
        }

        private void TweenTo(Color targetColor)
        {
            if (targetGraphic.color == targetColor) return;
            _currentTween.Stop();
            _currentTween = Tween.Color(targetGraphic, targetColor, duration, ease);
        }
    }
    
    [Serializable]
    public class HoverRotation : IHoverBehavior
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector3 hoveredRotation = new Vector3(0, 0, 10);
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private Quaternion _originalRotation;
        private Tween _currentTween;

        public HoverRotation() : this(null) {}

        public HoverRotation(RectTransform inTarget)
        {
            target = inTarget;
            if (target != null) _originalRotation = target.localRotation;
        }

        public void OnHovered(PointerEventData eventData)
        {
            TweenTo(Quaternion.Euler(hoveredRotation));
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            TweenTo(_originalRotation);
        }

        public void OnClicked(PointerEventData eventData)
        {
            _currentTween.Stop();
            target.localRotation = Quaternion.Euler(hoveredRotation);
        }

        public void Reset()
        {
            TweenTo(_originalRotation);
        }

        private void TweenTo(Quaternion rotation)
        {
            _currentTween.Stop();
            _currentTween = Tween.LocalRotation(target, rotation, duration, ease);
        }
    }
    
    [Serializable]
    public class HoverShake : IHoverBehavior
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 strength = new Vector2(10f, 10f);
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float frequency = 10f;
        [SerializeField] private bool enableFalloff = true;
        [SerializeField] private Ease easeBetweenShakes = Ease.Default;
        
        [SerializeField] private bool shakeOnHover = true;
        [SerializeField] private bool shakeOnClick = true;

        private Vector3 _defaultPosition;
        private Tween _currentTween;
        
        public HoverShake() : this(null) {}

        public HoverShake(RectTransform inTarget)
        {
            target = inTarget;
            _defaultPosition = inTarget != null ? inTarget.anchoredPosition : Vector3.zero;
        }

        public void OnHovered(PointerEventData eventData)
        {
            if (!shakeOnHover) return;
            _currentTween.Stop();
            _currentTween = Tween.ShakeLocalPosition
            (
                target,
                new Vector3(strength.x, strength.y, 0f),
                duration,
                frequency,
                enableFalloff,
                easeBetweenShakes
            );
        }
        
        public void OnClicked(PointerEventData eventData)
        {
            if (!shakeOnClick) return;
            _currentTween.Stop();
            _currentTween = Tween.PunchLocalPosition
            (
                target,
                strength * 0.5f,
                duration * 0.5f,
                frequency
            );
            Reset();
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            if (!shakeOnHover) return;
            Reset();
        }

        public void Reset()
        {
            Debug.Log("[OnShake localPos: ]" + target.localPosition);
            _currentTween.Stop();
            target.anchoredPosition = _defaultPosition;
            Debug.Log("[OnShake localPos after reset: ]" + target.localPosition);
        }
    }

    [Serializable]
    public class HoverNewText : IHoverBehavior
    {
        [SerializeField] private TMPro.TextMeshProUGUI target;
        [TextArea]
        [SerializeField] private string text;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private bool animateOnHover;
        [SerializeField] private bool animateOnClick;

        private string _originalText;

        public void OnHovered(PointerEventData eventData)
        {
            if (!animateOnHover) return;
            _originalText = target.text;
            target.text = text;
        }

        public void OnClicked(PointerEventData eventData)
        {
            if (!animateOnClick) return;
            target.text = text;
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            if (!animateOnHover) return;
            Reset();
        }

        public void Reset()
        {
            target.text = _originalText;
        }
    }
    
    [Serializable]
    public class PunchHover : IHoverBehavior
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 hoverStrength = new(5f, 5f, 0f);
        [SerializeField] private Vector3 unhoverStrength = new(3f, 3f, 0f);
        [SerializeField] private Vector3 clickStrength = new(10f, 10f, 0f);
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private float frequency = 10f;

        private Tween _currentTween;

        public void OnHovered(PointerEventData eventData)
        {
            _currentTween.Stop();
            if (hoverStrength == Vector3.zero) return;
            _currentTween = Tween.PunchLocalPosition(target, hoverStrength, duration, frequency);
        }
        
        public void OnClicked(PointerEventData eventData)
        {
            _currentTween.Stop();
            if (clickStrength == Vector3.zero) return;
            _currentTween = Tween.PunchLocalPosition(target, clickStrength, duration, frequency);
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            Reset();
            if (unhoverStrength == Vector3.zero) return;
            _currentTween = Tween.PunchLocalPosition(target, unhoverStrength, duration, frequency);
        }

        public void Reset()
        {
            _currentTween.Stop();
            target.localPosition = Vector3.zero;
        }
    }

    [Serializable]
    public class HoverAudio : IHoverBehavior
    {
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip unhoverClip;
        
        public HoverAudio() : this(null, null, null) { }

        public HoverAudio(AudioClip inHoverClip, AudioClip inClickClip, AudioClip inUnhoverClip)
        {
            hoverClip = inHoverClip;
            clickClip = inClickClip;
            unhoverClip = inUnhoverClip;
        }
        
        public void OnHovered(PointerEventData eventData)
        {
            if (hoverClip != null)
            {
                Main.Sound.AudioManager.PlayOneShotAtLocation(hoverClip, Main.Sound.EAudioCategory.UI, Vector3.zero);
            }
        }
        
        public void OnClicked(PointerEventData eventData)
        {
            if (clickClip != null)
            {
                Main.Sound.AudioManager.PlayOneShotAtLocation(clickClip, Main.Sound.EAudioCategory.UI, Vector3.zero);
            }
        }

        public void OnUnhovered(PointerEventData eventData)
        {
            Reset();
        }

        public void Reset()
        {
            if (unhoverClip != null)
            {
                Main.Sound.AudioManager.PlayOneShotAtLocation(unhoverClip, Main.Sound.EAudioCategory.UI, Vector3.zero);
            }
        }
    }
}