using System;
using CarePackage.Interaction;
using UnityEngine;

namespace CarePackage.Main
{
    public class TutorialSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private GameObject finishedScreen;
        [SerializeField] private int tutorialPackageAmount = 3;
        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private Transform[] initialPackagePositions;
        [SerializeField] private TMPro.TextMeshProUGUI requiredMoneyText, currentMoneyText;

        private int _currentMoney;
        private bool _tutorialFinished = false;

        private void Start() => OnEnter();

        public void OnEnter()
        {
            if (requiredMoneyText != null)
                requiredMoneyText.text = "Required Money: 300";
            GoalIndicator.Instance.Camera = GameManager.Instance.Player.SwitchMode.FirstPersonPlayer.GetComponentInChildren<Camera>();
            GameManager.Instance.Player.SwitchMode.CarCamera = Camera.main;
            
            for (int i = 0; i < tutorialPackageAmount; i++)
            {
                var createdPackage = CreatePackage(i);
                GameManager.Instance.Player.DeliveryManager.AddDelivery(createdPackage);
            }
            GameManager.Instance.Player.DeliveryManager.AssignRandomAddressesForDelivery();
            GameManager.Instance.Player.DeliveryManager.CheckList.InitializePackageList(GameManager.Instance.Player.DeliveryManager.Deliveries);
            Delivery.DeliveryManager.OnPackageDelivered += CheckTutorialFinished;
        }

        private Delivery.Package CreatePackage(int index)
        {
            var createdPackage = Instantiate(packagePrefab, initialPackagePositions[index].position, Quaternion.identity);
            if (createdPackage == null) return null;
            
            var packageInteractable = createdPackage.GetComponent<Interactable>();
            if (packageInteractable == null) return null;
            
            var packageData = new Delivery.Package
            {
                Id = index,
                PackageData = new Delivery.FPackageData(
                    "Package" + index + 1, 
                    "Package to deliver to house " + index + 1,
                    10,
                    100)
            };
            
            var extendedPickups = new IPickupExtension[]
            {
                new Interaction.Delivery.IndicatorPickupDroppableExtension()
            };
            
            var packageAction = new Interaction.Delivery.PackageAction(packageData, true, new Vector3(0, -0.1f, 0), createdPackage, extendedPickups);
            packageInteractable.InteractAction = packageAction;

            if (packageInteractable.InteractAction is Interaction.Delivery.PackageAction asPackageAction)
            {
                return asPackageAction.Package;
            }
            return null;
        }

        private void CheckTutorialFinished(Delivery.Package deliveredPackage)
        {
            if (GameManager.Instance.Player.DeliveryManager.Deliveries.Count <= 0)
                _tutorialFinished = true;
            
            _currentMoney += deliveredPackage.PackageData.MaxPay;

            if (currentMoneyText != null)
                currentMoneyText.text = "Money: " + _currentMoney.ToString();
            
            if (!_tutorialFinished) return;
            UI.UIManager.Instance.OpenPopupWindow(finishedScreen);
        }

        public void OnExit()
        {
            Delivery.DeliveryManager.OnPackageDelivered -= CheckTutorialFinished;
        }

        public void OnReadyToPlayClicked()
        {
            OnExit();
            GameManager.Instance.StartGame();
        }
        
        public void OnRetryClicked()
        {
            OnExit();
            SceneController.Instance.LoadScene(ECarePackageScenes.Tutorial);
        }
    }
}