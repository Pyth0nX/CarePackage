using CarePackage.Main;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace CarePackage.Interaction.UI
{
    [Serializable]
    public class PopPopupWindowAction : IInteractAction
    {
        [SerializeField] private GameObject popupWindow;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            CarePackage.UI.UIManager.Instance.OpenPopupWindow(popupWindow);
        }
    }
    
    [Serializable]
    public class PopPopupWindowsAction : IInteractAction
    {
        [SerializeField] private GameObject[] popupWindows;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            foreach (var popup in popupWindows)
            {
                CarePackage.UI.UIManager.Instance.OpenPopupWindow(popup);
            }
        }
    }
    
    [Serializable]
    public class ButtonAction : IInteractAction
    {
        [SerializeField] private UnityEvent function = new();
        
        public ButtonAction() : this(null) {}

        public ButtonAction(Action inFunction)
        {
            if (inFunction == null) return;
            function.RemoveAllListeners();
            function.AddListener(() => inFunction());
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (function == null) return;
            function.Invoke();
        }
    }
}
