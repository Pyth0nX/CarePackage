using CarePackage.Delivery;
using UnityEngine;
using UnityEngine.Serialization;

namespace CarePackage.UI
{
    public interface ISettingStrategy
    {
        MenuSettingElement Owner { get; set; }
        void SetValue(object value);
        object GetValue();
        void Save();
        void Load();
    }
    
    public interface ISettingStrategy<T> : ISettingStrategy
    {
        MenuSettingElement Owner { get; set; }
        void SetValue(T value);
        T GetValue();
        void Save();
        void Load();
    }

    [System.Serializable]
    public abstract class SliderSetting : ISettingStrategy<float>, IActivatable
    {
        [SerializeField] protected TMPro.TextMeshProUGUI title;
        [SerializeField] protected UnityEngine.UI.Slider slider;
        [SerializeField] private TMPro.TextMeshProUGUI valueLabel;
        
        protected float CurrentValue = 1f;
        
        public MenuSettingElement Owner { get; set; }

        public void SetValue(float value)
        {
            if (value is float floatValue) 
            {
                CurrentValue = Mathf.Clamp(floatValue, 0, 1);
                if (slider != null) 
                    slider.value = floatValue;
                if (valueLabel != null)
                    valueLabel.text = CurrentValue.ToString();
            }
        }

        public void SetValue(object value)
        {
            if (value is float floatValue) SetValue(floatValue);
        }

        object ISettingStrategy.GetValue() => GetValue();
        
        public virtual float GetValue() => CurrentValue;
        
        public void Save() => SaveSetting();

        public void Load() => LoadSetting();
        

        public void OnEnable()
        {
            if (title != null) title.text = GetTitle();
            if (slider != null) 
            {
                slider.onValueChanged.AddListener(OnSliderChanged);
                slider.value = CurrentValue;
            }
            Debug.Log("Setting Title to: " + GetTitle() + " for: " + title.gameObject.name);
        }

        public void OnDisable()
        {
            if (slider != null) slider.onValueChanged.RemoveListener(OnSliderChanged);
        }
        
        protected virtual void OnSliderChanged(float newValue) 
        {
            SetValue(newValue);
            HandleValueChanged(newValue);
        }
        
        protected abstract string GetTitle();
        
        protected abstract void HandleValueChanged(float newValue);
        
        protected abstract void SaveSetting();
        
        protected abstract void LoadSetting();
    }
    
    [System.Serializable]
    public class SensitivitySetting : SliderSetting
    {
        [SerializeField] private ESensitivity category;
        [SerializeReference] private SettingsMenuController controller;
        
        public ESensitivity Category => category;
        
        private float MapSliderToDomain(float sliderValue)
        {
            return category switch
            {
                ESensitivity.LookX => Mathf.Lerp(0.1f, 2f, sliderValue),
                ESensitivity.LookY => Mathf.Lerp(0.1f, 2f, sliderValue),
                ESensitivity.Scroll => Mathf.Lerp(0.02f, 0.5f, sliderValue),
                _ => sliderValue
            };
        }

        private float MapDomainToSlider(float domainValue)
        {
            return category switch
            {
                ESensitivity.LookX => Mathf.InverseLerp(0.1f, 2f, domainValue),
                ESensitivity.LookY => Mathf.InverseLerp(0.1f, 2f, domainValue),
                ESensitivity.Scroll => Mathf.InverseLerp(0.02f, 0.5f, domainValue),
                _ => domainValue
            };
        }

        protected override string GetTitle() => category.ToString() + " Sensitivity";
        
        protected override void HandleValueChanged(float newValue) => controller.PreviewSensitivity(Owner, category, MapSliderToDomain(newValue));
        
        protected override void SaveSetting() => controller.SaveSensitivity(Owner, category, CurrentValue);

        protected override void LoadSetting()
        {
            float stored = MapDomainToSlider(controller.GetSensitivityByCategory(category));
            SetValue(stored);
        }
        
        public enum ESensitivity { LookX, LookY, Scroll }
    }
    
    [System.Serializable]
    public class PackageAmountSetting : SliderSetting
    {
        [SerializeReference] private SettingsMenuController controller;
        
        private int MapSliderToDomain(float sliderValue)
        {
            return Mathf.RoundToInt(Mathf.Lerp(1, 15, sliderValue));
        }

        private float MapDomainToSlider(int domainValue)
        {
            return Mathf.InverseLerp(1, 15, domainValue);
        }

        protected override string GetTitle() => "Package Amount";
        
        protected override void HandleValueChanged(float newValue) => controller.SetPackageAmount(Owner, MapSliderToDomain(newValue));
        
        protected override void SaveSetting() => controller.SavePackageAmount(Owner, (int)MapDomainToSlider((int)CurrentValue));

        protected override void LoadSetting()
        {
            float stored = MapDomainToSlider(controller.GetPackageAmount());
            SetValue(stored);
        }
    }

    [System.Serializable]
    public abstract class CheckBoxSetting : ISettingStrategy<bool>, IActivatable
    {
        [SerializeField] protected TMPro.TextMeshProUGUI title;
        [SerializeField] private GameObject checkMarkImage;
        [SerializeField] private Interaction.Interactable interactable;
        
        protected bool Value;
        
        public MenuSettingElement Owner { get; set; }
        
        public CheckBoxSetting() : this(null, null) {}
        
        public CheckBoxSetting(GameObject inCheckMarkImage, Interaction.Interactable inInteractable)
        {
            checkMarkImage = inCheckMarkImage;
            interactable = inInteractable;
            if (interactable == null) return;
            interactable.OnInteracted += () => SetValue(Value = !Value);
        }
        
        public void SetValue(bool value)
        {
            Value = value;
            if (checkMarkImage != null)
                checkMarkImage.SetActive(value);
            OnValueChanged(Value);
        }

        public void SetValue(object value)
        {
            if (value is bool boolValue) SetValue(boolValue);
        }

        object ISettingStrategy.GetValue() => GetValue();

        public bool GetValue() => Value;

        public void Save() => SaveSetting();

        public void Load() => LoadSetting();
        
        
        public void OnEnable()
        {
            if (title != null) title.text = GetTitle();
            Debug.Log("Setting Title to: " + GetTitle() + " for: " + title.gameObject.name);
        }

        public void OnDisable() {}
        
        protected abstract void OnValueChanged(bool value);
        
        protected abstract string GetTitle();
        
        protected abstract void SaveSetting();
        
        protected abstract void LoadSetting();
    }

    [System.Serializable]
    public class CheckBoxGamesSetting : CheckBoxSetting
    {
        [SerializeField] private EGameSetting gameSetting;
        
        public EGameSetting GameSetting => gameSetting;
        
        protected override void OnValueChanged(bool value) =>  Main.GameManager.Instance.SetGameSetting(gameSetting, value);
        
        protected override string GetTitle() => gameSetting.ToString();

        protected override void SaveSetting()
        {
            Main.GameManager.Instance.SaveDeliverySetting(gameSetting, Value);
        }
        
        protected override void LoadSetting()
        {
            var loaded = Main.GameManager.Instance.GetDeliverySetting(gameSetting);
            SetValue(loaded);
        }
        
        public enum EGameSetting { EndWhenEmpty, LoseAtDay }
    }
}