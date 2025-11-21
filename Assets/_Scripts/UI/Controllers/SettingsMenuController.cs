using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private Slider[] volumeSliders;
        [SerializeField] private GameObject[] bojec;
        
        private System.Collections.Generic.List<ISettingStrategy> _settingStrategies = new();
        
        public bool IsOpen => _toggled;
        
        private bool _toggled;

        private async void Start()
        {
            await System.Threading.Tasks.Task.Yield();
            var menuSettingObjects = transform.GetComponentsInChildren<MenuSettingElement>(true);
            if (menuSettingObjects == null && menuSettingObjects.Length <= 0) return;
            
            foreach (var obj in menuSettingObjects)
            {
                if (obj == null) continue;

                var p = obj.Strategy;
                if (p == null)
                {
                    Debug.Log("No strategy found for " + obj.name + " bkebjhbhjb: " + obj.Strategy);
                    return;
                }
                Debug.Log(obj.Strategy);
                var s = obj.Strategy as ISettingStrategy;
                _settingStrategies.Add(s);
            }
        }

        private async void OnEnable()
        {
            await System.Threading.Tasks.Task.Yield();
            foreach (var setting in _settingStrategies)
            {
                if (setting is IActivatable activatable)
                {
                    activatable.OnEnable();
                }
            }
            
            foreach (var volumeSlider in volumeSliders)
            {
                var f = MenuSettingLibrary.TryGetStrategyFromObject<AudioSetting>(volumeSlider.transform.parent.gameObject);
                if (f == null)
                {
                    Debug.LogWarning($"No AudioSetting strategy found on {volumeSlider.gameObject.name}");
                    continue; // skip this slider
                }
                var category = f.Category;
                
                volumeSlider.onValueChanged.AddListener((newValue) => PreviewSliderValue(category, newValue));
            }
        }

        private void PreviewSliderValue(Main.Sound.EAudioCategory musicGroup, float newValue)
        {
            Main.Sound.AudioManager.Instance.PreviewVolume(musicGroup, newValue);
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
            
            foreach (var volumeSlider in volumeSliders)
            {
                volumeSlider.onValueChanged.RemoveAllListeners();
            }
        }
        
        public void Open()
        {
            _toggled = true;
        }

        public void Close()
        {
            UIManager.Instance.SetInputSchema("Player");
            _toggled = false;
        }

        public void SaveSettings()
        {
            //Main.Sound.AudioManager.Instance.SaveVolumeCurrent();
        }
    }
}

public static class MenuSettingLibrary
{
    public static T TryGetStrategyFromObject<T>(GameObject obj) where T : class, CarePackage.UI.ISettingStrategy
    {
        var element = obj.GetComponent<CarePackage.UI.MenuSettingElement>();
        if (element == null)
        {
            Debug.Log($"[TryGetStrategyFromObject] No MenuSettingElement on {obj.name}");
            return null;
        }

        var strat = element.Strategy;
        if (strat == null)
        {
            Debug.Log("[TryGetStrategyFromObject] No strategy assigned");
            return null;
        }

        if (strat is T typed)
        {
            Debug.Log($"[TryGetStrategyFromObject] Found strategy of type {typed.GetType().Name}");
            return typed;
        }
        
        return null;
    }
}