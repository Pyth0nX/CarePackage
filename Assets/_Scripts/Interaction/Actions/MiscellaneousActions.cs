using System;
using UnityEngine;
using CarePackage.Main;
using SerializeReferenceEditor;
using UnityEngine.Scripting.APIUpdating;

namespace CarePackage.Interaction.Miscellaneous
{
    [Serializable]
    public class PickupAction : IInteractAction, IPickup
    {
        [SerializeField] private bool hideInstedOfDestroy;
        [SerializeField] private bool disapearAfterUse;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(this, interactingObject);
        }

        public Vector3 Offset { get; }
        public GameObject OwningObject { get; set; }
        
        public void OnPickedUp(PlayerState interactingPlayer)
        {
            if (!disapearAfterUse) return;
            if (hideInstedOfDestroy) OwningObject.SetActive(false);
            else GameObject.Destroy(OwningObject);
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            
        }
    }
    
    [Serializable]
    public class ToggleObjectAction : IInteractAction
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
    public class SwitchSceneAction : IInteractAction
    {
        [SerializeField] private string sceneName;

        public SwitchSceneAction(string inSceneName)
        {
            sceneName = inSceneName;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            SceneController.Instance.LoadScene(sceneName);
        }
    }
}