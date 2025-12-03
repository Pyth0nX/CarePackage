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
        {/*
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
#endif*/
            
            if (interactingPlayer != null) _interactingPlayer = interactingPlayer;
            _interactingObject = interactingObject;
            
            var packageInteractable = interactingObject.GetComponent<Interactable>().InteractAction;
            if (packageInteractable == null)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but package is not interactable!");
                return;
            }
            Debug.Log("[Dialogue Action] PerformAction [] " + interactingObject.name + " " + interactingPlayer.name);

            if (packageInteractable is not PackageAction packageAction)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but package is not a PackageAction!");
                return;
            }
            
            var package = packageAction.Package;
            if (package == null)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but package is null!");
                return;
            }

            if (package.Id != id && package.Id != 42)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but package id does not match!");
                return;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but DialogueManager is null!");
                return;
            }

            _dialogueRunner = DialogueManager.Instance.dialogueRunner;
            if (_dialogueRunner == null) return;

            if (_dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning("[DialogueAction] Tried to start dialogue, but one is already running!");
                return;
            }
            DialogueManager.Instance.dialogueRunner.onDialogueComplete?.AddListener(OnDialogueComplete);

            _playerController = _interactingPlayer.SwitchMode.FirstPersonPlayer.GetComponentInChildren<PlayerController>();
            GoalIndicator.Instance.SetGoalObject(null);

            if (_playerController == null) return;
            _playerController.LockInput(true);
            
            DialogueManager.Instance.SetYarnFloat("$family", familyID);
            //Debug.LogError("[DialogueAction] FamilyID: " + familyID + " yarnFamId: " + DialogueManager.Instance.GetYarnFloat("$family"));

            //if (_interactingPlayer.DeliveryManager.GetCurrentDelivery().ItemGUID == "Items/Uniform")
            DialogueManager.Instance.SetYarnBool("$clothes", true);
            DialogueManager.Instance.SetYarnString("$package", package.ItemGUID.Split('/').Last());
            /*
            Debug.LogError("[DialogueAction] ItemGUID: " + package.ItemGUID.Split('/').Last());
            Debug.LogError("[DialogueAction] Clothes: " + DialogueManager.Instance.GetYarnBool("$clothes"));
            Debug.LogError("[DialogueAction] Package Damage: " + (int)package.PackageData.State);
            Debug.LogError("[DialogueAction] Package State: " + package.PackageData.State);*/
            DialogueManager.Instance.SetYarnFloat("$damage", (int)package.PackageData.State);

            var stateName = package.PackageData.State.ToString();
            DialogueManager.Instance.SetYarnString("$packageState", stateName);

            _dialogueRunner.StartDialogue(nodeName);
#if UNITY_EDITOR
            DialogueManager.Instance.shouldDebugDialogueNode = true;
#endif
        }

        public void OnEnable()
        {
            //DialogueManager.OnPackageRecieved += OnDialogueComplete;
            DialogueManager.Instance.dialogueRunner.onDialogueComplete?.AddListener(OnDialogueComplete);
        }

        public void OnDisable()
        {
            DialogueManager.Instance.dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueComplete);
            //DialogueManager.OnPackageRecieved -= OnDialogueComplete;
        }

        private void OnDialogueComplete(bool packageRecieved)
        {
            Debug.Log("[DialogueAction] Dialogue finished — input");
            DialogueManager.Instance.shouldDebugDialogueNode = false;

            if (_playerController == null)
            {
                return;
            }
            //CarePackage.UI.UIManager.Instance.CloseInterface(false);
            _playerController.LockInput(false);

            if (packageRecieved)
            {
                var receivePackageAction = new ReceiveDeliveryAction(id);
                receivePackageAction.PerformAction(_interactingPlayer, _interactingObject);
            }
        }
        
        private void OnDialogueComplete()
        {
            Debug.Log("[DialogueAction] Dialogue finished — input");
            DialogueManager.Instance.shouldDebugDialogueNode = false;

            if (_playerController == null)
            {
                return;
            }
            CarePackage.UI.UIManager.Instance.CloseInterface(false);
            //_playerController.LockInput(false);

                var receivePackageAction = new ReceiveDeliveryAction(id);
                receivePackageAction.PerformAction(_interactingPlayer, _interactingObject);
        }
    }
}