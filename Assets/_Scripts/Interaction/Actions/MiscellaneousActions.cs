using System;
using UnityEngine;
using CarePackage.Main;

namespace CarePackage.Interaction
{
    [Serializable]
    public class PickupAction : InteractAction
    {
        [SerializeField] private bool hideInstedOfDestroy;
        [SerializeField] private bool disapearAfterUse;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            // Pickup logic
            interactingPlayer.Pickup(interactingObject);
            
            if (!disapearAfterUse) return;
            if (hideInstedOfDestroy) interactingObject.SetActive(false);
            else GameObject.Destroy(interactingObject);
        }
    }
    
    [Serializable]
    public class ToggleObjectAction : InteractAction
    {
        [SerializeField] private bool enable = true;
        [SerializeField] private bool toggleItself;
        [SerializeField] private bool targetSelf = true;
        [SerializeField] private GameObject objectToToggle;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (targetSelf)
            {
                if (!toggleItself) interactingObject.SetActive(enable);
                else interactingObject.SetActive(!interactingObject.activeInHierarchy);
                return;
            }
            if (!toggleItself) objectToToggle.SetActive(enable);
            else objectToToggle.SetActive(!objectToToggle.activeInHierarchy);
        }
    }
    
    [Serializable]
    public class SwitchSceneAction : InteractAction
    {
        [SerializeField] private string  sceneName;
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            SceneController.Instance.LoadScene(sceneName);
        }
    }

    namespace UI
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
}