using System.Collections.Generic;
using UnityEngine;

namespace CarePackage.Persistance
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TValue> values = new();
        [SerializeField] private List<TKey> keys = new();
        
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
            {
                Debug.LogError("Tried to deserialize a dictionary with different number of keys [" + keys.Count + "] and values [" + values.Count + "].");
            }

            for (int i = 0; i < Keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }
    }
}
