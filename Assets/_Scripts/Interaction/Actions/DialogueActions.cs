using CarePackage.Interaction.Delivery;
using CarePackage.Main;
using UnityEngine;
using System.Linq;
using Yarn.Unity;
using System;

namespace CarePackage.Interaction.Dialogue
{
    [Serializable]
    public class DialogueAction : IInteractAction, IActivatable
    {
        [SerializeField] private string nodeName;
        [SerializeField] private int familyID;
        
        public int id;

        private PlayerController _playerController;
        private PlayerState _interactingPlayer;
        private GameObject _interactingObject;
        private DialogueRunner _dialogueRunner;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer != null) _interactingPlayer = interactingPlayer;
            var package = _interactingPlayer.DeliveryManager.CurrentDelivery;
            _interactingObject = interactingObject;
            if (package == null) return;

            if (package.Id != id) return;
            if (DialogueManager.Instance == null) return;

            _dialogueRunner = DialogueManager.Instance.dialogueRunner;
            if (_dialogueRunner == null) return;

            if (_dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but one is already running!");
                return;
            }

            _playerController =
                _interactingPlayer.SwitchMode.FirstPersonPlayer.GetComponentInChildren<PlayerController>();

            if (_playerController == null) return;
            _playerController.LockInput(true);
            
            DialogueManager.Instance.SetYarnFloat("$family", familyID);
            Debug.Log("[DialogueAction] FamilyID: " + familyID + " yarnFamId: " + DialogueManager.Instance.GetYarnFloat("$family"));

            //if (_interactingPlayer.DeliveryManager.GetCurrentDelivery().ItemGUID == "Items/Uniform")
                DialogueManager.Instance.SetYarnBool("$clothes", true);
                DialogueManager.Instance.SetYarnString("$package", _interactingPlayer.DeliveryManager.GetCurrentDelivery().ItemGUID.Split('/').Last());
                Debug.Log("[DialogueAction] ItemGUID: " + _interactingPlayer.DeliveryManager.GetCurrentDelivery().ItemGUID.Split('/').Last());
                Debug.Log("[DialogueAction] Clothes: " + DialogueManager.Instance.GetYarnBool("$clothes"));
            DialogueManager.Instance.SetYarnFloat("$Damage", (int)package.PackageData.State);

            var stateName = package.PackageData.State.ToString();
            DialogueManager.Instance.SetYarnString("$packageState", stateName);

            _dialogueRunner.StartDialogue(nodeName);
#if UNITY_EDITOR
            DialogueManager.Instance.shouldDebugDialogueNode = true;
#endif
        }

        public void OnEnable()
        {
            DialogueManager.OnPackageRecieved += OnDialogueComplete;
        }

        public void OnDisable()
        {
            DialogueManager.OnPackageRecieved -= OnDialogueComplete;
        }

        private void OnDialogueComplete(bool packageRecieved)
        {
            Debug.Log("[DialogueAction] Dialogue finished — input");
            DialogueManager.Instance.shouldDebugDialogueNode = false;

            if (_playerController == null) return;
            _playerController.LockInput(false);

            if (packageRecieved)
            {
                var receivePackageAction = new ReceiveDeliveryAction(id);
                receivePackageAction.PerformAction(_interactingPlayer, _interactingObject);
            }
        }
    }
}