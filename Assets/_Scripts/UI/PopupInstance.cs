using CarePackage.Interaction.UI;
using UnityEngine;

namespace CarePackage.UI
{
    public class PopupInstance : MonoBehaviour
    {
        [SerializeField] private Interaction.Interactable closeButton;
        
        public System.Action<PopupInstance> OnClosed;

        private void Start()
        {
            closeButton.InteractAction = new ButtonAction(Close);
        }

        public void Close()
        {
            OnClosed?.Invoke(this);
            Destroy(gameObject);
        }
    }
}