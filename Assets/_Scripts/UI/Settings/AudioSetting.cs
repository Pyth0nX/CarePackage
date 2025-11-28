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

        protected override string GetCategoryKey() => AudioManager.Instance.GetVolumeParamByGroup(category);

        protected override void HandleValueChanged(float newValue) => controller.PreviewAudioSlider(Owner, category, CurrentValue);
        
        protected override void SaveSetting() => controller.SaveAudioSlider(Owner, category, CurrentValue);

        protected override void LoadSetting()
        {
            float stored = AudioManager.Instance.GetStoredVolume(category);
            SetValueWithDomain(stored);
        }
    }
}