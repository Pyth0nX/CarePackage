using System;
using System.Collections.Generic;
using CarePackage.Delivery;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using CarePackage.Utilities;
using UnityEngine;

namespace CarePackage.Main
{
    public class ModeSwitcher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject carCamera;
        [SerializeField] private GameObject idleCarPrefab;
        [SerializeField] private Transform transitPackages;
        [SerializeField] private GameObject packagePrefab;

        public GameObject CarCamera => carCamera;
        public GameObject Car { get => _car; set => _car = value; }
        public GameObject IdleCar { get => _idleCarInstance; set => _idleCarInstance = value; }
        public GameObject FirstPersonPlayer { get => _firstPersonPlayer; set => _firstPersonPlayer = value; }
        public Transform TransitPackages => transitPackages;
        public GameObject ActivePlayer => _currentPlayer;

        private List<Vector3> _savedLocalPositions = new();
        private List<Quaternion> _savedLocalRotations = new();
        private GameObject _firstPersonPlayer;
        private GameObject _car;
        private GameObject _idleCarInstance;
        private GameObject _currentPlayer;
        private bool _idleCarInitialized;

        private void Awake()
        {
            var playerController = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            var carController = FindFirstObjectByType<PrometeoCarController>(FindObjectsInactive.Include);

            FirstPersonPlayer = playerController != null ? playerController.gameObject : null;
            Car = carController != null ? carController.gameObject : null;
            _currentPlayer = FirstPersonPlayer != null && FirstPersonPlayer.activeInHierarchy ? FirstPersonPlayer : Car;
        }

        private void Start()
        {
            GameManager.Instance.onDayStarted += OnDayStarted_Implementation;
        }

        private void OnDisable()
        {
            GameManager.Instance.onDayStarted -= OnDayStarted_Implementation;
        }

        private void OnDayStarted_Implementation(int day)
        {
            var offset = .8f;
            var count = GameManager.Instance.Player.DeliveryManager.DeliveriesToMake;
            SpawnInitalPackages(count, offset);
        }

        private void SpawnInitalPackages(int count, float offset)
        {
            var center = TransitPackages.position;
            var positions = PositionUtilities.GenerateStrict2x3Grid(center, count, offset);
            var packages = GameManager.Instance.Player.DeliveryManager.Deliveries;
            List<PackageObject> packageObjects = new();
            
            for (int i = 0; i < count; i++)
            {
                var newPackage = Instantiate(packagePrefab, positions[i], Quaternion.identity);
                //newPackage.transform.SetParent(transitPackages.transform, true);
                var interactable = newPackage.GetComponent<Interactable>();
                interactable.InteractAction = new PackageInSceneAction(packages[i], true, new Vector3(0, -0.1f, 0));
                var package = newPackage.GetComponent<PackageObject>();
                packageObjects.Add(package);
            }
            
            foreach (var package in packageObjects)
            {
                package.TogglePhysics(true);
                package.SetDamageEnabled(true);
            }
        }

        public void EnterCarMode(Transform originalTransform)
        {
            SavePackageTransforms();
            
            FirstPersonPlayer.SetActive(false);
            CarCamera.SetActive(true);
            
            SetPackagesRigid(false);
            TransitPackages.SetParent(null, true);
            if (IdleCar != null) IdleCar.SetActive(false);

            var carPosition = originalTransform.root.position;
            var carRotation = originalTransform.root.rotation;
            Car.transform.position = carPosition;
            Car.transform.rotation = carRotation;
            Car.SetActive(true);
            
            TransitPackages.SetParent(Car.transform.GetChild(3), true);
            RestorePackageTransforms();
            SetPackagesRigid(true);
            
            _currentPlayer = Car;
            var deliveryManager = GameManager.Instance.Player.DeliveryManager;
            var postBox = deliveryManager.FindPostBoxWithId(deliveryManager.CurrentDeliveryId);
            deliveryManager.ToggleIndicator(postBox);
        }

        public void EnterFirstPersonMode(Transform originalTransform)
        {
            SetPackagesRigid(false);
            SavePackageTransforms();
            TransitPackages.SetParent(null, true);

            var carPosition = Car.transform.position;
            Car.SetActive(false);
            
            // spawn IdleCar
            if (!_idleCarInitialized)
            {
                IdleCar = Instantiate(idleCarPrefab, carPosition, Quaternion.identity);
                _idleCarInitialized = true;
            }

            IdleCar.transform.position = carPosition;
            IdleCar.transform.rotation = originalTransform.root.rotation;
            IdleCar.SetActive(true);
            
            var packageLocation = IdleCar.transform.GetChild(3);
            TransitPackages.SetParent(packageLocation, true);
            //RestorePackageTransforms();
            SetPackagesRigid(true);

            var playerStartPos = carPosition + -IdleCar.transform.right * 3f;
            FirstPersonPlayer.transform.position = playerStartPos;
            Vector3 lookDirection = IdleCar.transform.forward;
            lookDirection.y = 0f;
            lookDirection.Normalize();
            FirstPersonPlayer.transform.rotation = Quaternion.LookRotation(lookDirection);
            
            CarCamera.SetActive(false);
            FirstPersonPlayer.SetActive(true);
            _currentPlayer = FirstPersonPlayer;

            var deliveryManager = GameManager.Instance.Player.DeliveryManager;
            var wantedPackage = deliveryManager.FindDeliveryPackageWithId(deliveryManager.CurrentDeliveryId);
            deliveryManager.ToggleIndicator(wantedPackage);
        }
        
        private void SavePackageTransforms()
        {
            _savedLocalPositions.Clear();
            _savedLocalRotations.Clear();
            
            foreach (Transform package in TransitPackages)
            {
                _savedLocalPositions.Add(package.localPosition);
                _savedLocalRotations.Add(package.localRotation);
            }
        }

        private void RestorePackageTransforms()
        {
            for (int i = 0; i < TransitPackages.childCount; i++)
            {
                var package = TransitPackages.GetChild(i);
                package.localPosition = _savedLocalPositions[i];
                package.localRotation = _savedLocalRotations[i];
            }
        }

        private void SetPackagesRigid(bool rigidBodyEnabled)
        {
            foreach (Transform packageTransform in TransitPackages)
            {
                var package = packageTransform.GetComponent<PackageObject>();
                if (package == null) continue;
                package.TogglePhysics(rigidBodyEnabled);
            }
        }
    }
}