using System.Reflection;
using UnityEngine;

namespace CarePackage.UI
{
    [CreateAssetMenu(fileName = "SO_HoverStyle", menuName = "CarePackage/UI/Hover Style Asset")]
    public class SO_HoverStyle : ScriptableObject
    {
        [SerializeField] private HoverBehaviorBinding[] defaultBehaviors;
        
        public IHoverBehavior[] ApplyStyle(GameObject owner)
        {
            var clones = new System.Collections.Generic.List<IHoverBehavior>();
            
            foreach (var binding in defaultBehaviors)
            {
                var clone = binding.behavior;
                var target = ResolveTarget(owner, binding);
                InjectTarget(clone, target);
                clones.Add(clone);
            }
            return clones.ToArray();
        }
        
        private Transform ResolveTarget(GameObject owner, HoverBehaviorBinding binding)
        {
            switch (binding.targetMode)
            {
                case HoverBehaviorBinding.TargetMode.Owner:
                    return owner.transform;
                case HoverBehaviorBinding.TargetMode.ChildByIndex:
                    if (binding.childIndex >= 0 && binding.childIndex < owner.transform.childCount)
                        return owner.transform.GetChild(binding.childIndex);
                    break;
                case HoverBehaviorBinding.TargetMode.ChildByName:
                    var child = owner.transform.Find(binding.childName);
                    if (child != null) return child;
                    break;
                case HoverBehaviorBinding.TargetMode.MultiChild:
                    if (string.IsNullOrEmpty(binding.childPath)) return null;
                    var indices = binding.childPath.Split('/');
                    Transform current = owner.transform;
                    foreach (var idxStr in indices)
                    {
                        if (int.TryParse(idxStr, out int idx) && idx >= 0 && idx < current.childCount)
                            current = current.GetChild(idx);
                        else return null;
                    }
                    return current;
            }
            return null;
        }
        
        private void InjectTarget(IHoverBehavior behavior, Transform target)
        {
            if (target == null) return;

            var type = behavior.GetType();

            // Look for "target" or "_target"
            var targetField = type.GetField("target", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                              ?? type.GetField("_target", BindingFlags.NonPublic | BindingFlags.Instance);

            if (targetField != null && targetField.FieldType == typeof(Transform))
            {
                targetField.SetValue(behavior, target);
                return;
            }

            var graphicField = type.GetField("_targetGraphic", BindingFlags.NonPublic | BindingFlags.Instance);
            if (graphicField != null && graphicField.FieldType == typeof(UnityEngine.UI.Graphic))
            {
                var graphic = target.GetComponent<UnityEngine.UI.Graphic>();
                if (graphic != null)
                    graphicField.SetValue(behavior, graphic);
                return;
            }
        }
    }
    
    [System.Serializable]
    public class HoverBehaviorBinding
    {
        [SerializeReference, SerializeReferenceEditor.SR] public IHoverBehavior behavior;

        public TargetMode targetMode = TargetMode.None;
        public int childIndex = 0;
        public string childName = "";
        public string childPath = "";

        public enum TargetMode { None, Owner, ChildByIndex, ChildByName, MultiChild }
    }
}