using CarePackage.Main;
using Yarn.Unity;
using UnityEngine;
using System;

namespace CarePackage.Interaction.Dialogue
{
    [Serializable]
    public class DialogueAction : InteractAction, IActivatable
    {
        [SerializeField] private string nodeName;
        [SerializeField] private DialogueRunner dialogueRunner;
        
        public int id;

        private PlayerController playerController;
        private PlayerState _interactingPlayer;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer != null) _interactingPlayer = interactingPlayer;
            if (_interactingPlayer.DeliveryManager.GetCurrentDelivery().Id != id) return;
            if (dialogueRunner == null)
            {
                dialogueRunner = UnityEngine.Object.FindFirstObjectByType<DialogueRunner>();
                if (dialogueRunner == null)
                {
                    Debug.LogError("[DialogueAction] DialogueRunner not in the scene");
                    return;
                }
            }

            if (dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but one is already running!");
                return;
            }

            playerController = _interactingPlayer.SwitchMode.FirstPersonPlayer.GetComponentInChildren<PlayerController>();

            Debug.Log($"[DialogueAction] {interactingPlayer.name} initiates dialogue '{nodeName}' with {interactingObject.name}");

            if (playerController == null)
            {
                Debug.Log("Player Controller not found");
                return;
            }
            playerController.LockInput(true);

            dialogueRunner.StartDialogue(nodeName);
        }

        public void OnEnable()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = UnityEngine.Object.FindFirstObjectByType<DialogueRunner>();
                if (dialogueRunner == null)
                {
                    Debug.LogError("[DialogueAction] DialogueRunner not in the scene");
                    return;
                }
            }

            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }

        public void OnDisable()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
            }
        }

        private void OnDialogueComplete()
        {
            Debug.Log("[DialogueAction] Dialogue finished — input");

            if (playerController == null)
                return;
            playerController.LockInput(false); 
            
            var packageToDeliver = _interactingPlayer.DeliveryManager.GetCurrentDelivery();
            _interactingPlayer.DeliveryManager.DeliverPackage(packageToDeliver);
        }
    }
}