using UnityEngine;
using AYellowpaper.SerializedCollections;
namespace OpenDialogue
{
    public class ValueTracker : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        [SerializedDictionary]
        public SerializedDictionary<valueKey, int> values = new SerializedDictionary<valueKey, int>();
        // Update is called once per frame
        void Update()
        {

        }
        public void SetValue(valueKey key, int value)
        {
            values[key] = value;
        }
        public void ChangeValue(valueKey key, int change)
        {
            values[key] += change;
        }
        public int GetValue(valueKey key)
        {
            return values[key];
        }
    }

    public enum valueKey
    {

    }

}
