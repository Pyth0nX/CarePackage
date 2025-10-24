using CarePackage.Main;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Yarn.Unity;

namespace CarePackage.Interaction.Dialogue
{
    [MovedFrom("CarePackage.Interaction")]
    [Serializable]
    public class DialogueAction : InteractAction, IActivatable
    {
        [SerializeField] private string nodeName;
        [SerializeField] private DialogueRunner dialogueRunner;
        
        public int id;

        private PlayerController playerController;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
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

            if (dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but one is already running!");
                return;
            }

            playerController = interactingPlayer.GetComponent<ModeSwitcher>().FirstPersonPlayer.GetComponentInChildren<PlayerController>();

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
        }
    }
}