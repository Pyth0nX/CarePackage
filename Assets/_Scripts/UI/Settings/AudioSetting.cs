using CarePackage.Main.Sound;
using UnityEngine;

namespace CarePackage.UI
{
    [System.Serializable]
    public class AudioSetting : SliderSetting
    {
        [SerializeField] private EAudioCategory category;
        [SerializeReference] private SettingsMenuController controller;
        
        public EAudioCategory Category => category;

        protected override string GetTitle() => category.ToString() + " Volume";
        
        protected override void HandleValueChanged(float newValue) => controller.PreviewAudioSlider(Owner, category, newValue);
        
        protected override void SaveSetting() => controller.SaveAudioSlider(Owner, category);

        protected override void LoadSetting()
        {
            float stored = AudioManager.Instance.GetVolumeByAudioGroup(category);
            SetValue(stored);
        }
    }
}