using UnityEngine;
using System;
using CarePackage.Main;
using UnityEngine.Scripting.APIUpdating;

namespace CarePackage.Interaction.UI
{
    [MovedFrom("CarePackage.Interaction.Miscellaneous.UI")]
    [Serializable]
    public class PopPopupWindowAction : InteractAction
    {
        [SerializeField] private GameObject popupWindow;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            UIManager.Instance.OpenPopupWindow(popupWindow);
        }
    }
    
    [MovedFrom("CarePackage.Interaction.Miscellaneous.UI")]
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
