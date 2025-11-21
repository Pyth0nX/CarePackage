using UnityEngine;

namespace CarePackage.UI
{
    public interface ISettingStrategy
    {
        void SetValue(object value);
        object GetValue();
    }
}