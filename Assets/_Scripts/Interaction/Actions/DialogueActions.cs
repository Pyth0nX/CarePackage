using CarePackage.Main;
using UnityEngine;
using System;

namespace CarePackage.Interaction.Dialogue
{
    [Serializable]
    public class DialogueAction : InteractAction, IActivatable
    {
        [SerializeField] private string nodeName;
        [SerializeField] private int familyID;

        public int id;

        private PlayerController playerController;
        private PlayerState _interactingPlayer;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer != null) _interactingPlayer = interactingPlayer;
            if (_interactingPlayer.DeliveryManager.GetCurrentDelivery().Id != id) return;
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueAction] DialogueRunner not in the scene");
                return;
            }
            
            var _dialogueRunner = DialogueManager.Instance.dialogueRunner;

            if (_dialogueRunner == null)
            {
                Debug.LogError("[DialogueAction] DialogueRunner not in the instance of GameManager");
                return;
            }

            if (_dialogueRunner.IsDialogueRunning)
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

            var delivery = _interactingPlayer.DeliveryManager.GetCurrentDelivery();

            if (_interactingPlayer.DeliveryManager.GetCurrentDelivery().ItemGUID == "Items/Uniform") DialogueManager.Instance.SetYarnBool("$clothes", true);

            DialogueManager.Instance.dialogueRunner.VariableStorage.SetValue("$family", familyID);

            _dialogueRunner.StartDialogue(nodeName);
        }

        public void OnEnable()
        {
            var _dialogueRunner = DialogueManager.Instance.dialogueRunner;
            if (_dialogueRunner == null)
            {
                    Debug.LogError("[DialogueAction] DialogueRunner not in the scene");
                    return;
            }

            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }

        public void OnDisable()
        {
            var _dialogueRunner = DialogueManager.Instance.dialogueRunner;
            if (_dialogueRunner != null)
            {
                _dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
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