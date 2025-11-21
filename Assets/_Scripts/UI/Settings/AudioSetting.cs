using CarePackage.Main.Sound;
using UnityEngine;
using System;
using TMPro;

namespace CarePackage.UI
{
    [Serializable]
    public class AudioSetting : ISettingStrategy, IActivatable
    {
        [SerializeField] private EAudioCategory category;
        [SerializeField] private TextMeshProUGUI title;
        
        private float currentValue;
        
        public EAudioCategory Category => category;

        public void SetValue(object value)
        {
            if (value is float f)
            {
                currentValue = f;
            }
        }

        public object GetValue() => currentValue;
        
        public void OnEnable()
        {
            title.text = category.ToString();
            Debug.Log("Setting Title to: " + category.ToString() + " for: " + title.gameObject.name);
        }

        public void OnDisable()
        {
            
        }
    }
}