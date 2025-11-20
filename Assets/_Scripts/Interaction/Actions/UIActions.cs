using CarePackage.Main;
using UnityEngine;
using System;

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
}
