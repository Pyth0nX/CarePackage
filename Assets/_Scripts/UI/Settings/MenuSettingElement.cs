using UnityEngine;

namespace CarePackage.UI
{
    public class MenuSettingElement : MonoBehaviour, IUserInterfaceElement
    {
        [SerializeReference, SerializeReferenceEditor.SR] private ISettingStrategy strategy;
        public ISettingStrategy Strategy => strategy;
        
        private void Awake() 
        {
            if (strategy == null) return; 
            strategy.Owner = this;
        }

        public void Open()
        {
            
        }

        public void Close()
        {
            
        }
    }
}