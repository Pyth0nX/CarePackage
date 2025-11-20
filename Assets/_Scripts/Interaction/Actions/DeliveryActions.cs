using CarePackage.Main;
using CarePackage.Delivery;
using UnityEngine;
using System;
using TMPro;

namespace CarePackage.Interaction.Delivery
{
    [Serializable]
    public class PackageAction : Pickup, IInteractAction
    {
        [SerializeField] private SO_Package package;
        [SerializeField] private bool addedDelivery;

        public Package Package => _internalPackage;
        
        private Package _internalPackage;
        
        public PackageAction(Package inPackage)
            : this(inPackage, false, new Vector3(0, -0.1f, 0), null) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded)
            : this(inPackage, alreadyAdded, new Vector3(0, -0.1f, 0), null) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension inAdditionalPickupExtension)
            : this(inPackage, alreadyAdded, inOffset, inPickupOwningObject, new[] {inAdditionalPickupExtension}) {}
        
        public PackageAction(Package inPackage, bool alreadyAdded, Vector3 inOffset, GameObject inPickupOwningObject, IPickupExtension[] inAddtioanlPickupExtensions = null) : base(inOffset, inPickupOwningObject)
        {
            _internalPackage = inPackage;
            package = DeliveryUitilities.ToScriptableObject(inPackage);
            addedDelivery = alreadyAdded;

            var packageObj = inPickupOwningObject.GetComponent<PackageBehavior>();
            var packagePickupExtension = new PackagePickupExtension(packageObj);
            //var damagePickup = new DamagableFieldExtension(new Vector3(0f, 0.6f, 1.15f), packageObj);

            int additionalExtensions = inAddtioanlPickupExtensions?.Length ?? 0;
            ExtendedLogic = new IPickupExtension[1 + additionalExtensions];
            ExtendedLogic[0] = packagePickupExtension;
            //ExtendedLogic[1] = damagePickup;
            
            if (additionalExtensions > 0)
                Array.Copy(inAddtioanlPickupExtensions, 0, ExtendedLogic, 1, additionalExtensions);
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (_internalPackage == null) _internalPackage = DeliveryUitilities.ToPackage(package);
            if (package == null) package = DeliveryUitilities.ToScriptableObject(_internalPackage);
            
            interactingPlayer.Pickup(this, interactingObject);
            interactingPlayer.DeliveryManager.SetCurrentHeldDelivery(Package);
            
            if (addedDelivery) return;
            interactingPlayer.DeliveryManager.AddDelivery(Package);
            addedDelivery = true;
        }
    }

    [Serializable]
    public class PackagePickupExtension : IPickupExtension
    {
        private readonly PackageBehavior _packageObj;

        public PackagePickupExtension(PackageBehavior inPackageObj)
        {
            _packageObj = inPackageObj;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            _packageObj.VelocityThreshold = _packageObj.HeldVelocityThreshold;
            _packageObj.TogglePhysics(true);
            _packageObj.SetDamageEnabled(true);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            _packageObj.VelocityThreshold = _packageObj.DefaultVelocityThreshold;
            //_packageObj.TogglePhysics(true);
            //_packageObj.SetDamageEnabled(true);
        }
    }

    [Serializable]
    public class ConveyerBeltPackageExtension : IPickupExtension
    {
        private readonly GameObject _packageObject;
        
        public ConveyerBeltPackageExtension(GameObject inPackageObject)
        {
            _packageObject = inPackageObject;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            interactingPlayer.DeliveryManager.RemovePackageFromConveyerBelt(_packageObject);
        }

        public void ExtendedDropped(PlayerState interactingPlayer) {}
    }
    
    [Serializable]
    public class IndicatorPickupDroppableExtension : IPickupExtension
    {
        private GameObject _goalObject;
        private int _packageId;
        private bool _overridePackageId;

        public IndicatorPickupDroppableExtension(int inPackageId = -1)
        {
            _goalObject = null;
            _overridePackageId = false;
            if (inPackageId == -1) return;
            _packageId = inPackageId;
            _overridePackageId = true;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            if (!_overridePackageId) _packageId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            _goalObject = interactingPlayer.DeliveryManager.FindPostBoxWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, true, false);
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            if (!_overridePackageId) _packageId = interactingPlayer.DeliveryManager.CurrentDeliveryId;
            _goalObject = interactingPlayer.DeliveryManager.FindDeliveryPackageWithId(_packageId);
            interactingPlayer.DeliveryManager.ToggleIndicator(_goalObject, false, false, 0f);
        }
    }
    
    [Serializable]
    public class DamagableFieldExtension : IPickupExtension
    {
        private Vector3 _offset;
        private GameObject _damageFieldPrefab;
        private GameObject _damageFieldObject;
        private DamagableBehavior _damageField;
        private PackageBehavior _packageObject;
        private bool _isInited;
        
        public DamagableFieldExtension() : this(Vector3.zero, null) {}

        public DamagableFieldExtension(Vector3 inOffset, PackageBehavior inPackageObject)
        {
            _offset = inOffset;
            _packageObject = inPackageObject;
            _isInited = false;
        }
        
        public void ExtendedPickUp(PlayerState interactingPlayer)
        {
            if (!_isInited)
            {
                _damageFieldObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/DamagableField"), interactingPlayer.ActivePlayer.transform);
                _damageFieldObject.TryGetComponent<DamagableBehavior>(out _damageField);
                _damageField.VelocityThreshold = _packageObject.HeldVelocityThreshold;
                _isInited = true;
            }
            _damageFieldObject.transform.localPosition = _offset;
            _damageFieldObject.SetActive(true);
            _damageField.OnDamaged += DamagePackage;
        }

        public void ExtendedDropped(PlayerState interactingPlayer)
        {
            _damageField.OnDamaged -= DamagePackage;
            _damageFieldObject.SetActive(false);
        }

        private void DamagePackage()
        {
            Debug.Log("Damaged Package");
            _packageObject.DamagePackage();
        }
    }

    [Serializable]
    public class ReceiveDeliveryAction : IInteractAction
    {
        [SerializeField] private int wantedPackage;
        
        private DeliveryManager _deliveryManager;
        
        public int WantedPackage { get => wantedPackage; set => wantedPackage = value; }

        public ReceiveDeliveryAction(int inWantedPackage)
        {
            wantedPackage = inWantedPackage;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (interactingPlayer.DeliveryManager == null) return;
            _deliveryManager = interactingPlayer.DeliveryManager;
            
            if (!CanReceivePackage()) return;
            var delivery = _deliveryManager.CurrentDelivery;
            interactingPlayer.DropPickup();
            _deliveryManager.DeliverPackage(delivery);
        }

        private bool CanReceivePackage()
        {
            if (_deliveryManager.CurrentDelivery.Id == wantedPackage) return true;
            return false;
        }
    }
    
    [Serializable]
    public class SetListedJobAction : IInteractAction, IActivatable
    {
        [SerializeField] private SO_Package job;
        [SerializeField] private GameObject parent;
        [SerializeField] private bool isLeft;

        public Package Job => _internalPackage;
        public GameObject OwningObject => parent;
        
        private Package _internalPackage;
        
        public SetListedJobAction() : this(null, new Package(), false) {}
        
        public SetListedJobAction(GameObject inParent, Package inPackage) : this(inParent, inPackage, false) {}

        public SetListedJobAction(GameObject inParent, Package inPackage, bool inIsLeft)
        {
            parent = inParent;
            job = DeliveryUitilities.ToScriptableObject(inPackage);
            _internalPackage = inPackage;
            isLeft = inIsLeft;
        }

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var jobManager = interactingPlayer.DeliveryManager;
            if (jobManager == null) return;
            if (job == null || _internalPackage == null) return;
            
            Debug.Log("Clicked: " + parent.name);

            var packageToSet = _internalPackage == null ? DeliveryUitilities.ToPackage(job) : _internalPackage;
            jobManager.SetListedDelivery(this, isLeft);
        }

        public void OnEnable()
        {
            var text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = _internalPackage.PackageData.Title;
            parent.name = _internalPackage.PackageData.Title;
        }

        public void OnDisable() {}
    }

    [Serializable]
    public class SelectDeliveryAction : IInteractAction
    {
        private Package _selectablePackage;
        private DeliveryCheckList _deliveryCheckList;
        private bool _deliveryCheckListInitialized;

        public SelectDeliveryAction() : this(null, null) {}
        
        public SelectDeliveryAction(DeliveryCheckList inDeliveryCheckList, Package inSelectablePackage)
        {
            _deliveryCheckList = inDeliveryCheckList;
            if (_deliveryCheckList != null) _deliveryCheckListInitialized = true;
            _selectablePackage = inSelectablePackage;
        }
        
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (!_deliveryCheckListInitialized) _deliveryCheckList = interactingPlayer.DeliveryManager.CheckList;
            if (_deliveryCheckList == null) return;
            _deliveryCheckList.SelectPackage(_selectablePackage);
            interactingPlayer.DeliveryManager.SetNewJob(_selectablePackage);
        }
    }
}