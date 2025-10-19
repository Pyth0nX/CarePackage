using CarePackage.Main;
using System;
using TMPro;
using UnityEngine;
using Yarn.Unity; 

namespace CarePackage.Interaction
{
    [Serializable]
    public class DialogueActions : InteractAction
    {
        [SerializeField] private string nodeName; 
        [SerializeField] private DialogueRunner dialogueRunner;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (dialogueRunner == null)
            {
                Debug.LogError("[DialogueAction] DialogueRunner no asignado en el inspector!");
                return;
            }

            Debug.Log($"[DialogueAction] {interactingPlayer.name} inicia diálogo '{nodeName}' con {interactingObject.name}");

            dialogueRunner.StartDialogue(nodeName);
        }
    }
}