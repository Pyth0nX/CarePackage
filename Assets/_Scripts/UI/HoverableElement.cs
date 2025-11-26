using SerializeReferenceEditor;
using UnityEngine.EventSystems;
using UnityEngine;

namespace CarePackage.UI
{
    public class HoverableElement : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [UnityEngine.Serialization.FormerlySerializedAs("hoverStrategy")] [SerializeReference, SR] private IHoverBehavior[] hoverStrategies;
        [SerializeField] private SO_HoverStyle presetStyle;
        [SerializeField, HideInInspector] private bool[] isDirtyFlags;
        [SerializeField] private bool useDirty;
        [SerializeField] private bool toggleAble;
        [SerializeField] private bool useOnlyPreset;

        [SerializeReference, HideInInspector] private IHoverBehavior[] _presetOnlyBackup;
        private bool _toggled;

        private void Start()
        {
            if (presetStyle == null) return;
            var styleBehaviors = presetStyle.ApplyStyle(gameObject);
            if (hoverStrategies == null || hoverStrategies.Length == 0)
            {
                hoverStrategies = styleBehaviors;
            }
            else
            {
                var merged = new IHoverBehavior[hoverStrategies.Length + styleBehaviors.Length];
                hoverStrategies.CopyTo(merged, 0);
                styleBehaviors.CopyTo(merged, hoverStrategies.Length);
                hoverStrategies = merged;
            }
            
            foreach (var strategy in hoverStrategies) 
                if (strategy is IActivatable activatable) 
                    activatable.OnEnable();
        }

        private void OnEnable()
        {
            foreach (var strategy in hoverStrategies)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (var strategy in hoverStrategies)
            {
                if (strategy is IActivatable activatable)
                    activatable.OnDisable();
            }
        }

        private void OnValidate()
        {/*
            if (presetStyle == null)
            {
                // No preset, just respect whatever is in hoverStrategy
                return;
            }
            
            IHoverBehavior[] styleBehaviors = presetStyle != null 
                ? presetStyle.ApplyStyle(gameObject) 
                : System.Array.Empty<IHoverBehavior>();
            
            if (useOnlyPreset)
            {
                // Backup current custom strategies if not already backed up
                if (_presetOnlyBackup == null || _presetOnlyBackup.Length == 0)
                {
                    _presetOnlyBackup = hoverStrategies;
                }

                // Replace with only preset behaviors
                hoverStrategies = styleBehaviors;
            }
            else
            {
                if (_presetOnlyBackup != null && _presetOnlyBackup.Length > 0)
                {
                    // Merge restored custom strategies with preset
                    var merged = new IHoverBehavior[_presetOnlyBackup.Length + styleBehaviors.Length];
                    _presetOnlyBackup.CopyTo(merged, 0);
                    styleBehaviors.CopyTo(merged, _presetOnlyBackup.Length);
                    hoverStrategies = merged;

                    _presetOnlyBackup = null;
                }
                else
                {
                    if (hoverStrategies == null || hoverStrategies.Length == 0)
                    {
                        hoverStrategies = styleBehaviors;
                    }
                    else
                    {
                        var merged = new IHoverBehavior[hoverStrategies.Length + styleBehaviors.Length];
                        hoverStrategies.CopyTo(merged, 0);
                        styleBehaviors.CopyTo(merged, hoverStrategies.Length);
                        hoverStrategies = merged;
                    }
                }
            }*/
            
            foreach (var strategy in hoverStrategies)
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
            
            if (presetStyle == null) return;

            var styleBehaviors = presetStyle.ApplyStyle(gameObject);
            
            if (hoverStrategies == null || hoverStrategies.Length == 0)
            {
                hoverStrategies = styleBehaviors;
                return;
            }
            if (AreBehaviorsEqual(hoverStrategies, styleBehaviors)) return;
            
            var merged = new IHoverBehavior[hoverStrategies.Length + styleBehaviors.Length];
            hoverStrategies.CopyTo(merged, 0);
            styleBehaviors.CopyTo(merged, hoverStrategies.Length);
            hoverStrategies = merged;
            
            foreach (var strategy in hoverStrategies)
                if (strategy is IActivatable activatable)
                    activatable.OnEnable();
        }
        
        private IHoverBehavior[] MergeBehaviors(IHoverBehavior[] current, IHoverBehavior[] preset, bool useDirty, bool[] dirtyFlags)
        {
            int max = System.Math.Max(current?.Length ?? 0, preset?.Length ?? 0);
            var merged = new IHoverBehavior[max];

            for (int i = 0; i < max; i++)
            {
                bool hasCurrent = current != null && i < current.Length;
                bool hasPreset = preset != null && i < preset.Length;

                if (useDirty && hasCurrent && dirtyFlags != null && i < dirtyFlags.Length && dirtyFlags[i])
                {
                    merged[i] = current[i]; // keep dirty override
                }
                else if (hasPreset)
                {
                    merged[i] = preset[i]; // use preset
                }
                else if (hasCurrent)
                {
                    merged[i] = current[i]; // keep extra current
                }
            }

            return merged;
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
            foreach (var strategy in hoverStrategies)
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
            
            foreach (var strategy in hoverStrategies)
            {
                strategy.OnClicked(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var strategy in hoverStrategies)
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
                foreach (var strategy in hoverStrategies)
                {
                    strategy.Reset();
                }
            }
        }
    }
}