using UnityEngine;

namespace CarePackage.UI
{
    public class MenuSettingElement : MonoBehaviour
    {
        [SerializeReference, SerializeReferenceEditor.SR] private ISettingStrategy strategy;
        public ISettingStrategy Strategy => strategy;
    }
}