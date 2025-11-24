using UnityEngine;

namespace CarePackage.UI
{
    public static class MenuFunctionLibrary
    {
        public static T TryGetStrategyFromObject<T>(GameObject obj) where T : class, UI.ISettingStrategy<T>
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
}