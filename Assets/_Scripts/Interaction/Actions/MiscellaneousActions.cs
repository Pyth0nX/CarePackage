using System;
using UnityEngine;
using CarePackage.Main;
using UnityEngine.Scripting.APIUpdating;

namespace CarePackage.Interaction.Miscellaneous
{
    [MovedFrom("CarePackage.Interaction")]
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

    [MovedFrom("CarePackage.Interaction")]
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

    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class SwitchSceneAction : InteractAction
    {
        [SerializeField] private string sceneName;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            SceneController.Instance.LoadScene(sceneName);
        }
    }
}