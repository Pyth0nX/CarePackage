using System;
using System.Collections.Generic;
using CarePackage.Delivery;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using CarePackage.Utilities;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CarePackage.Main
{
    public class ModeSwitcher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera carCamera;
        [SerializeField] private GameObject idleCarPrefab;
        [SerializeField] private Transform transitPackages;
        [SerializeField] private GameObject packagePrefab;

        public Camera CarCamera => carCamera;
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
        private readonly List<PackageBehavior> _packageObjects = new();

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

        private void Update()
        {
            if (_idleCarInitialized && _idleCarInstance.activeSelf)
            {
                var sourceAnimator = _idleCarInstance.GetComponentInChildren<Animator>();
                var thisAnimator = Car.GetComponentInChildren<Animator>();
                // Iterate through all the parameters in the source animator
                foreach (AnimatorControllerParameter parameter in sourceAnimator.parameters)
                {

                    // Copy the parameter's value based on its type
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            thisAnimator.SetFloat(parameter.name, sourceAnimator.GetFloat(parameter.name));
                            break;
                        case AnimatorControllerParameterType.Int:
                            thisAnimator.SetInteger(parameter.name, sourceAnimator.GetInteger(parameter.name));
                            break;
                        case AnimatorControllerParameterType.Bool:
                            thisAnimator.SetBool(parameter.name, sourceAnimator.GetBool(parameter.name));
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            // Triggers are a bit special, as they reset after being consumed.
                            // We check if the trigger is set on the source and then set it on the destination.
                            if (sourceAnimator.GetBool(parameter.name))
                            {
                                thisAnimator.SetTrigger(parameter.name);
                            }

                            break;
                    }
                }
            } 
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
            
            for (int i = 0; i < count; i++)
            {
                var newPackage = Instantiate(packagePrefab, positions[i], Quaternion.identity);
                //newPackage.transform.SetParent(transitPackages.transform, true);
                var interactable = newPackage.GetComponent<Interactable>();
                var package = newPackage.GetComponent<PackageBehavior>();
                var extendedPickups = new IPickupExtension[]
                {
                    new IndicatorPickupDroppableExtension()//packages[i].Id, newPackage
                };
                var packageAction = new PackageAction(packages[i], true, new Vector3(0, -0.1f, 0), newPackage, extendedPickups);
                interactable.InteractAction = packageAction;
                _packageObjects.Add(package);
            }
            TogglePackagesDamagable(true);
        }

        public void EnterCarMode(Transform originalTransform)
        {
            SavePackageTransforms();
            
            FirstPersonPlayer.SetActive(false);
            CarCamera.gameObject.SetActive(true);
            
            TogglePackagesDamagable(false, 0.1f);
            TransitPackages.SetParent(null, true);
            if (IdleCar != null) IdleCar.SetActive(false);

            var carPosition = originalTransform.root.position;
            var carRotation = originalTransform.root.rotation;
            Car.transform.position = carPosition;
            Car.transform.rotation = carRotation;
            SetCarVisibility(true);
            
            TransitPackages.SetParent(Car.transform.GetChild(3), true);
            RestorePackageTransforms();
            TogglePackagesDamagable(true);
            
            _currentPlayer = Car;
            var deliveryManager = GameManager.Instance.Player.DeliveryManager;
            var postBox = deliveryManager.FindPostBoxWithId(deliveryManager.CurrentDeliveryId);
            GoalIndicator.Instance.Camera = carCamera;
            deliveryManager.ToggleIndicator(postBox, true, false);
        }

        private void SetCarVisibility(bool visibility)
        {
            Car.GetComponent<PrometeoCarController>().enabled = visibility;
            Car.GetComponent<InteractionComponent>().enabled = visibility;
            Car.GetComponent<PlayerInput>().enabled = visibility;
            Car.GetComponent<Rigidbody>().isKinematic = !visibility;
            foreach (var componentsInChild in Car.GetComponentsInChildren<Renderer>())
            {
                componentsInChild.enabled = visibility;
            }
            
            foreach (var componentsInChild in Car.GetComponentsInChildren<Collider>())
            {
                componentsInChild.enabled = visibility;
            }
            
            foreach (var componentsInChild in Car.GetComponentsInChildren<AudioSource>())
            {
                componentsInChild.enabled = visibility;
            }
        }

        public void EnterFirstPersonMode(Transform originalTransform)
        {
            TogglePackagesDamagable(false, 0.1f);
            SavePackageTransforms();
            TransitPackages.SetParent(null, true);

            var carPosition = Car.transform.position;
            SetCarVisibility(false);
            
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
            TogglePackagesDamagable(true);

            var playerStartPos = carPosition + -IdleCar.transform.right * 3f;
            FirstPersonPlayer.transform.position = playerStartPos;
            Vector3 lookDirection = IdleCar.transform.forward;
            lookDirection.y = 0f;
            lookDirection.Normalize();
            FirstPersonPlayer.transform.rotation = Quaternion.LookRotation(lookDirection);
            
            CarCamera.gameObject.SetActive(false);
            FirstPersonPlayer.SetActive(true);
            _currentPlayer = FirstPersonPlayer;

            var deliveryManager = GameManager.Instance.Player.DeliveryManager;
            var wantedPackage = deliveryManager.FindDeliveryPackageWithId(deliveryManager.CurrentDeliveryId);
            GoalIndicator.Instance.Camera = FirstPersonPlayer.GetComponentInChildren<Camera>();
            deliveryManager.ToggleIndicator(wantedPackage, false, true, 0f);
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

        private void TogglePackagesDamagable(bool toggle, float delay = 1f)
        {
            foreach (var package in _packageObjects)
            { 
                Tween.Delay(delay).OnComplete(() => package.TogglePhysics(toggle));
                Tween.Delay(delay).OnComplete(() => package.SetDamageEnabled(toggle));
            }
        }
    }
}