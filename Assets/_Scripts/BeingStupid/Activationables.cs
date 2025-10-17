using System.Collections.Generic;
using System.Reflection;
using SerializeReferenceEditor;
using UnityEngine;

public class Activationables : MonoBehaviour
{
    [SerializeReference] private object root;
    
    [SerializeReference, SR] private IActivatable[] _activatables;
    
    public static Activationables Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        if (_activatables == null) return;
        foreach (var activatable in _activatables)
        {
            activatable.OnEnable();
        }
    }

    private void OnDisable()
    {
        if (_activatables == null) return;
        foreach (var activatable in _activatables)
        {
            activatable.OnDisable();
        }
    }
    
    public void SetRoot(object root)
    {
        this.root = root;
        _activatables = FindAndRegisterActivatables(root);
    }

    public static void Register(IActivatable activatable)
    {
    //    _activatables.Add(activatable);
    }

    public static void Unregister(IActivatable activatable)
    {
    //    _activatables.Remove(activatable);
    }

    //public static List<IActivatable> GetActivatables() => _activatables;

    private IActivatable[] FindAndRegisterActivatables(object root)
    {
        if (root == null) return new IActivatable[0];

        var found = new List<IActivatable>();
        
        void Recurse(object obj)
        {
            if (obj == null) return;

            if (obj is IActivatable a)
                found.Add(a);
            
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(GameObject) || typeof(Component).IsAssignableFrom(field.FieldType))
                    continue; // skip Unity object fields

                var value = field.GetValue(obj);
                if (value == null) continue;

                // Recurse into SerializeReference or ScriptableObjects
                if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum)
                    Recurse(value);
            }
        }

        Recurse(root);
        return found.ToArray();
    }
}
