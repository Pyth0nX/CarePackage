using CarePackage.Main;
using UnityEngine;
using System;

namespace CarePackage.Interaction.Miscellaneous
{
    [Serializable]
    public class PickupAction : Pickup, IInteractAction
    {
        [SerializeField] private bool hideInstedOfDestroy;
        [SerializeField] private bool removeAfterUse;

        public PickupAction(bool inHideInstedOfDestroy = false, bool inDisapearAfterUse = false) : base()
        {
            hideInstedOfDestroy = inHideInstedOfDestroy;
            removeAfterUse = inDisapearAfterUse;
        }
        
        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset) : this(inHideInstedOfDestroy, inDisapearAfterUse)
        {
            Offset = inOffset;
        }

        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset, IPickupExtension inPickupExtension) : this(inHideInstedOfDestroy, inDisapearAfterUse, inOffset)
        {
            ExtendedLogic = new[]{ inPickupExtension };
        }
        
        public PickupAction(bool inHideInstedOfDestroy, bool inDisapearAfterUse, Vector3 inOffset, IPickupExtension[] inPickupExtensions) : this(inHideInstedOfDestroy, inDisapearAfterUse, inOffset)
        {
            ExtendedLogic = inPickupExtensions;
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            interactingPlayer.Pickup(this, interactingObject);
        }

        public void OnPickedUp(PlayerState interactingPlayer)
        {
            if (!removeAfterUse) return;
            if (hideInstedOfDestroy) OwningObject.SetActive(false);
            else GameObject.Destroy(OwningObject);
        }

        public void OnDropped(PlayerState interactingPlayer)
        {
            interactingPlayer.SetPickup(null, null);
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