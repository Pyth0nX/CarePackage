using CarePackage.Main;
using UnityEngine.Rendering;
using UnityEngine;

namespace CarePackage.UI
{
    public class SettingsMenuController : MonoBehaviour, IUserInterfaceElement
    {
        [SerializeField] private GameObject areYouSurePopup;
        
        public bool IsOpen => _toggled;
        
        private PlayerController _playerController;
        private SerializedDictionary<MenuSettingElement, object>  _originalValues = new();//private readonly System.Collections.Generic.Dictionary<MenuSettingElement, object> _originalValues = new();
        private System.Collections.Generic.List<ISettingStrategy> _settingStrategies = new();
        private System.Collections.Generic.List<MenuSettingElement> _elements = new();
        private bool _hasUnsavedChanges;
        private bool _forceClose;
        private bool _toggled;

        private void Start()
        {
            var menuSettingObjects = transform.GetComponentsInChildren<MenuSettingElement>(true);
            if (menuSettingObjects.Length == 0) return;
            
            foreach (var element in menuSettingObjects)
            {
                if (element?.Strategy == null)
                {
                    Debug.LogWarning($"No valid strategy found for {element?.name}");
                    continue;
                }
                _elements.Add(element);
                _settingStrategies.Add(element.Strategy);
            }
            
            _playerController = GameManager.Instance.Player.ActivePlayer.GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            PrimeTween.Tween.Delay(0.1f, InitializeAfterFrame);
        }
        
        private void InitializeAfterFrame()
        {
            foreach (var setting in _settingStrategies)
            {
                (setting as IActivatable)?.OnEnable();
            }
        }
        
        private void OnDisable()
        {
            foreach (var setting in _settingStrategies)
            {
                if (setting is IActivatable activatable)
                {
                    activatable.OnDisable();
                }
            }
        }
        
        public void Open()
        {
            gameObject.SetActive(true);
            _toggled = true;
            _originalValues.Clear();
        }

        public void Close()
        {
            if (_hasUnsavedChanges && !_forceClose)
            {
                ShowUnsavedChangesPopup();
                return;
            }
            
            InternalClose();
        }
        
        private void InternalClose()
        {
            if (!_toggled) return;

            _toggled = false;
            gameObject.SetActive(false);
            UIManager.Instance.ClosePopupWindow(areYouSurePopup);

            Main.Sound.AudioManager.Instance.LoadVolumes();
            foreach (var element in _elements)
            {
                element.Strategy.Load();
            }
        }
        
        public void RequestClose()
        {
            UIManager.Instance.ClosePopupWindow(gameObject);
        }
        
        public void ConfirmClose()
        {
            UIManager.Instance.ClosePopupWindow(areYouSurePopup);
            _forceClose = true;
            UIManager.Instance.ClosePopupWindow(gameObject);
            _forceClose = false;
        }

        public void SaveAndClose()
        {
            SaveSettings();
            Close();
        }

        public void Cancel()
        {
            UIManager.Instance.ClosePopupWindow(areYouSurePopup);
        }
        
        public void SaveSettings()
        {
            foreach (var element in _elements)
            {
                element.Strategy.Save();
            }
            _hasUnsavedChanges = false;
            PlayerPrefs.Save();
        }
        
        public void SaveChange(MenuSettingElement owningElement, object newValue)
        {
            if (!_originalValues.ContainsKey(owningElement)) 
            {
                _originalValues[owningElement] = newValue;
                return;
            }

            var original = _originalValues[owningElement];
            bool changed = !Equals(original, newValue);

            if (changed) 
            {
                _hasUnsavedChanges = true;
            } 
            else 
            {
                _hasUnsavedChanges = AnyElementChanged();
            }
        }
        
        private void RemoveElementFromChanges(MenuSettingElement elementToRemove)
        {
            if (_elements.Contains(elementToRemove)) _originalValues.Remove(elementToRemove);
        }
        
        private void ShowUnsavedChangesPopup()
        {
            UIManager.Instance.OpenPopupWindow(areYouSurePopup);
            var popupInstance = areYouSurePopup.GetComponentInChildren<Interaction.Interactable>();
            if (popupInstance == null) return;
            
            popupInstance.InteractAction = new Interaction.UI.ButtonAction(() => UIManager.Instance.ClosePopupWindow(areYouSurePopup));
        }
        
        private bool AnyElementChanged() 
        {
            foreach (var kvp in _originalValues) 
            {
                if (!Equals(kvp.Value, GetCurrentValue(kvp.Key))) return true;
                /*
                var element = kvp.Key;
                var original = kvp.Value;

                var current = GetCurrentValue(element);
                if (!Equals(original, current))
                    return true;*/
            }
            return false;
        }
        
        private object GetCurrentValue(MenuSettingElement element) 
        {/*
            if (element.TryGetComponent(out Slider slider)) return slider.value;
            if (element.TryGetComponent(out Toggle toggle)) return toggle.isOn;
            if (element.TryGetComponent(out InputField input)) return input.text;*/
            if (element?.Strategy != null) return element.Strategy.GetValue();
            return null;
        }

        #region Preview Save Methods
        public void PreviewAudioSlider(MenuSettingElement owningElement, Main.Sound.EAudioCategory musicGroup, float newValue)
        {
            Main.Sound.AudioManager.Instance.PreviewVolume(musicGroup, newValue);
            SaveChange(owningElement, newValue);
        }
        
        public void SaveAudioSlider(MenuSettingElement owningElement, Main.Sound.EAudioCategory musicGroup)
        {
            Main.Sound.AudioManager.Instance.SaveVolume(musicGroup);
            RemoveElementFromChanges(owningElement);
        }
        
        public void PreviewSensitivity(MenuSettingElement owningElement, SensitivitySetting.ESensitivity sensitivityCategory, float newValue)
        {
            _playerController.PreviewSensitivity(sensitivityCategory, newValue);
            SaveChange(owningElement, newValue);
        }
        
        public void SaveSensitivity(MenuSettingElement owningElement, SensitivitySetting.ESensitivity sensitivityCategory, float newValue)
        {
            _playerController.SaveSensitivity(sensitivityCategory, newValue);
            RemoveElementFromChanges(owningElement);
        }

        public float GetSensitivityByCategory(SensitivitySetting.ESensitivity sensitivityCategory)
        {
            return _playerController.GetSensitivity(sensitivityCategory);
        }

        public void SetPackageAmount(MenuSettingElement owningElement, int newValue)
        {
            _playerController.OwningPlayer.DeliveryManager.PackageMax = newValue;
            SaveChange(owningElement, newValue);
        }

        public void SavePackageAmount(MenuSettingElement owningElement, int newValue)
        {
            _playerController.OwningPlayer.DeliveryManager.SavePackageMax(newValue);
            RemoveElementFromChanges(owningElement);
        }

        public int GetPackageAmount()
        {
            return _playerController.OwningPlayer.DeliveryManager.PackageMax;
        }
        #endregion
    }
}