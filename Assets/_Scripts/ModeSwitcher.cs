using System.Collections.Generic;
using CarePackage.Delivery;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Main
{
    public class ModeSwitcher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject carCamera;
        [SerializeField] private GameObject idleCarPrefab;
        [SerializeField] private Transform transitPackages;

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

            var playerStartPos = carPosition + new Vector3(3f, 0, 0);
            var playerRotation = originalTransform.root.right;
            FirstPersonPlayer.transform.position = playerStartPos;
            FirstPersonPlayer.transform.rotation = Quaternion.Euler(playerRotation);
            
            CarCamera.SetActive(false);
            FirstPersonPlayer.SetActive(true);
            _currentPlayer = FirstPersonPlayer;
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
                package.SetDamageEnabled(rigidBodyEnabled);
            }
        }
    }
}

namespace CarePackage.Interaction.Car
{
    public class EnterCarAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterCarMode(interactingObject.transform);
        }
    }

    public class ExitCarAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            var switchMode = interactingPlayer.SwitchMode;
            switchMode.EnterFirstPersonMode(interactingObject.transform.root);
        }
    }
}