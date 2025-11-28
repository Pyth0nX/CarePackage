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
        void SetValue(T value);
        T GetValue();
    }

    [System.Serializable]
    public abstract class SliderSetting : ISettingStrategy<float>, IActivatable
    {
        [SerializeField] protected UnityEngine.UI.Slider slider;
        [SerializeField] protected TMPro.TextMeshProUGUI title;
        [SerializeField] private TMPro.TextMeshProUGUI valueLabel;

        [Header("Internal Range")] 
        [SerializeField] protected float minValue = 0f;
        [SerializeField] protected float maxValue = 1f;
        [SerializeField] protected float defaultValue;

        [Header("Visual Range")] [SerializeField]
        protected float visualMin = 0f;

        [SerializeField] protected float visualMax = 1f;

        [SerializeField] private bool useNormalizedNormal;
        [SerializeField] private EValueFormat valueFormat;
        [SerializeField, Range(0, 4)] private int decimalPrecision = 1;

        protected float CurrentValue = 1f;
        protected float VisualValue;

        public MenuSettingElement Owner { get; set; }
        
        public SliderSetting() : this(0, 1, 0, 1, 1, EValueFormat.Number) {}

        public SliderSetting(float inMinValue, float inMaxValue, float inVisualMin, float inVisualMax, float inStartingValue, EValueFormat inValueFormat)
        {
            minValue = inMinValue;
            maxValue = inMaxValue;
            visualMin = inVisualMin;
            visualMax = inVisualMax;
            defaultValue = inStartingValue;
            valueFormat = inValueFormat;
            Init();
        }

        public void Init()
        {
            if (useNormalizedNormal) SetValue(defaultValue);
            else SetValueWithDomain(defaultValue);
        }
        
        public void SetValue(object value)
        {
            if (value is float floatValue) SetValue(floatValue);
        }
        
        public void SetValue(float value)
        {
            CurrentValue = MapSliderToDomain(value);
            VisualValue = MapInternalToVisual(CurrentValue);
            
            if (slider != null)
                slider.value = value;
            
            UpdateLabel();
        }

        public void SetValueWithDomain(float domainValue)
        {
            var normalizedDomainValue = MapDomainToSlider(domainValue);
            SetValue(normalizedDomainValue);
        }

        object ISettingStrategy.GetValue() => GetValue();
        
        public virtual float GetValue() => CurrentValue;
        
        public void Save() => SaveSetting();

        public void Load()
        {
            if (!SettingsMenuController.IsSettingSaved(GetCategoryKey())) Init();
            else LoadSetting();
        }

        private void UpdateLabel()
        {
            if (valueLabel == null) return;
            valueLabel.text = FormatValue();
        }

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
            Load();
            if (slider != null)
                slider.onValueChanged.AddListener(OnSliderChanged);
            
            Debug.Log("Setting Title to: " + GetTitle() + " for: " + title.gameObject.name);
        }

        public void OnDisable()
        {
            Load();
            if (slider != null) 
                slider.onValueChanged.RemoveListener(OnSliderChanged);
        }
        
        protected virtual void OnSliderChanged(float newValue) 
        {
            SetValue(newValue);
            HandleValueChanged(newValue);
        }
        
        protected abstract string GetTitle();

        protected abstract string GetCategoryKey();
        
        protected abstract void HandleValueChanged(float newValue);
        
        protected abstract void SaveSetting();
        
        protected abstract void LoadSetting();
        
        protected virtual string FormatValue()
        {
            return valueFormat switch
            {
                EValueFormat.Number => FormatSmartDecimal(VisualValue),
                EValueFormat.Percentage => (VisualValue * 100f).ToString("0") + "%",
                _ => VisualValue.ToString()
            };

            string FormatSmartDecimal(float value)
            {
                var multiplier = 1;
                for (int i = 0; i < decimalPrecision; i++)
                    multiplier *= 10;
                
                float rounded = Mathf.Round(value * multiplier) / multiplier;
                
                if (decimalPrecision == 0 || Mathf.Abs(rounded % 1f) < 0.001f)
                    return ((int)rounded).ToString();
                
                return rounded.ToString("F" + decimalPrecision);
            }
        }
        
        public enum EValueFormat
        {
            Number = 0,
            Percentage = 1
        }
    }
    
    [System.Serializable]
    public class SensitivitySetting : SliderSetting
    {
        [SerializeField] private ESensitivity category;
        [SerializeReference] private SettingsMenuController controller;
        
        public ESensitivity Category => category;

        protected override string GetTitle() => category + " Sensitivity";

        protected override string GetCategoryKey() => category.ToString();

        protected override void HandleValueChanged(float newValue) => controller.PreviewSensitivity(Owner, category, CurrentValue);
        
        protected override void SaveSetting() => controller.SaveSensitivity(Owner, category, CurrentValue);

        protected override void LoadSetting()
        {
            float loadedSens = controller.GetSensitivityByCategory(category);
            SetValueWithDomain(loadedSens);
        }

        public enum ESensitivity { LookX, LookY, Scroll }
    }
    
    [System.Serializable]
    public class PackageAmountSetting : SliderSetting
    {
        [SerializeReference] private SettingsMenuController controller;
        [SerializeField] private bool changeMaxAmount;
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

        protected override string GetCategoryKey() => changeMaxAmount ? "PackageMax" : "PackageMin";

        protected override void HandleValueChanged(float newValue) => controller.SetPackageAmount(Owner, (int)newValue, changeMaxAmount);
        
        protected override void SaveSetting() => controller.SavePackageAmount(Owner, (int)CurrentValue, changeMaxAmount);

        protected override void LoadSetting()
        {
            float loadedMaxPackageNum = controller.GetPackageAmount(changeMaxAmount);
            SetValueWithDomain(loadedMaxPackageNum);
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