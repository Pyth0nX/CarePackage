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

        private PlayerController playerController;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = UnityEngine.Object.FindFirstObjectByType<DialogueRunner>();
                if (dialogueRunner == null)
                {
                    Debug.LogError("[DialogueAction] DialogueRunner no asignado ni encontrado en la escena!");
                    return;
                }
            }

            playerController = interactingPlayer.GetComponent<PlayerController>();

            Debug.Log($"[DialogueAction] {interactingPlayer.name} inicia diálogo '{nodeName}' con {interactingObject.name}");

            if (playerController == null)
                return;
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