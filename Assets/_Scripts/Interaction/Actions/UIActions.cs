using CarePackage.Main;
using UnityEngine;
using System;

namespace CarePackage.Interaction.UI
{
    [Serializable]
    public class PopPopupWindowAction : InteractAction
    {
        [SerializeField] private GameObject popupWindow;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            UIManager.Instance.OpenPopupWindow(popupWindow);
        }
    }
    
    [Serializable]
    public class PopPopupWindowsAction : InteractAction
    {
        [SerializeField] private GameObject[] popupWindows;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            foreach (var popup in popupWindows)
            {
                UIManager.Instance.OpenPopupWindow(popup);
            }
        }
    }
}
