using UnityEngine;

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
        public enum EValueFormat
        {
            Decimal = 0,
            WholeNumber = 1,
            Percentage = 2
        }
        
        [SerializeField] protected UnityEngine.UI.Slider slider;
        [SerializeField] protected TMPro.TextMeshProUGUI title;
        [SerializeField] private TMPro.TextMeshProUGUI valueLabel;
        
        [Header("Internal Range")]
        [SerializeField] protected float minValue = 0f;
        [SerializeField] protected float maxValue = 1f;
        [SerializeField, Range(0f, 1f)] protected float defaultValue;
        
        [Header("Visual Range")]
        [SerializeField] protected float visualMin = 0f;
        [SerializeField] protected float visualMax = 1f;
        
        [SerializeField] private EValueFormat valueFormat;
        
        protected float CurrentValue = 1f;
        
        public MenuSettingElement Owner { get; set; }

        public void SetValue(float value)
        {
            CurrentValue = MapSliderToDomain(value);
            if (slider != null)
                slider.value = CurrentValue;
            UpdateLabel();
        }

        public void SetValue(object value)
        {
            if (value is float floatValue) SetValue(floatValue);
        }

        object ISettingStrategy.GetValue() => GetValue();
        
        public virtual float GetValue() => CurrentValue;
        
        public void Save() => SaveSetting();

        public void Load() => LoadSetting();
        
        public float GetDomainValue() => CurrentValue;
        
        public void SetDomainValue(float domain)
        {
            CurrentValue = Mathf.Clamp(domain, minValue, maxValue);
            if (slider != null)
                slider.value = MapDomainToSlider(CurrentValue);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (valueLabel == null) return;
            
            float visual = MapInternalToVisual(CurrentValue);
            valueLabel.text = FormatValue(visual);
        }
/*
        protected virtual float MapSliderToDomain(float normalized) =>
            Mathf.Lerp(minValue, maxValue, normalized);

        protected virtual float MapDomainToSlider(float domain) =>
            Mathf.InverseLerp(minValue, maxValue, domain);

        protected virtual float MapInternalToVisual(float internalValue) =>
            Mathf.InverseLerp(minValue, maxValue, internalValue) * (visualMax - visualMin) + visualMin;*/

        protected virtual float MapSliderToDomain(float normalized) =>
            Mathf.Lerp(minValue, maxValue, normalized);

        protected virtual float MapDomainToSlider(float domain) =>
            Mathf.InverseLerp(minValue, maxValue, domain);

        protected virtual float MapInternalToVisual(float internalValue)
        {
            float normalized = MapDomainToSlider(internalValue);
            return Mathf.Lerp(visualMin, visualMax, normalized);
        }
        

        public void OnEnable()
        {
            if (title != null) title.text = GetTitle();
            CurrentValue = Mathf.Lerp(minValue, maxValue, defaultValue);//CurrentValue = Mathf.Clamp(defaultValue, minValue, maxValue);
            if (slider != null) 
            {
                slider.onValueChanged.AddListener(OnSliderChanged);
                slider.value = CurrentValue;
            }
            UpdateLabel();
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
        
        protected virtual string FormatValue(float normalized)
        {
            float domain = MapInternalToVisual(normalized);

            return valueFormat switch
            {
                EValueFormat.Decimal => domain.ToString("0.0"),
                EValueFormat.WholeNumber => Mathf.RoundToInt(domain).ToString(),
                EValueFormat.Percentage => (domain * 100f).ToString("0") + "%",
                _ => domain.ToString()
            };
        }
    }
    
    [System.Serializable]
    public class SensitivitySetting : SliderSetting
    {
        [SerializeField] private ESensitivity category;
        [SerializeReference] private SettingsMenuController controller;
        
        public ESensitivity Category => category;
        /*
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
        }*/

        protected override string GetTitle() => category.ToString() + " Sensitivity";
        
        protected override void HandleValueChanged(float newValue) => controller.PreviewSensitivity(Owner, category, MapSliderToDomain(newValue));
        
        protected override void SaveSetting() => controller.SaveSensitivity(Owner, category, MapSliderToDomain(CurrentValue));

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
        /*
        private int MapSliderToDomain(float sliderValue)
        {
            return Mathf.RoundToInt(Mathf.Lerp(1, 15, sliderValue));
        }

        private float MapDomainToSlider(int domainValue)
        {
            return Mathf.InverseLerp(1, 15, domainValue);
        }*/

        protected override string GetTitle() => "Package Amount";
        
        protected override void HandleValueChanged(float newValue) => controller.SetPackageAmount(Owner, Mathf.RoundToInt(MapSliderToDomain(newValue)));
        
        protected override string FormatValue(float normalized)
        {
            int domain = Mathf.RoundToInt(MapSliderToDomain(normalized));
            return domain.ToString();
        }
        
        protected override void SaveSetting() => controller.SavePackageAmount(Owner, Mathf.RoundToInt(MapSliderToDomain(CurrentValue)));

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
            interactable.OnInteracted += (interacted) => SetValue(Value = !Value);
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