using CarePackage.Interaction.Delivery;
using CarePackage.Interaction;
using CarePackage.Persistance;
using CarePackage.Main;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

namespace CarePackage.Delivery
{
    public class DeliveryCheckList : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private GameObject selectablePackagePrefab;
        [SerializeField] private GameObject selectablesContainer;
        [SerializeField] private GameObject checkListUI;

        private List<GameObject> _selectablePackageObjects = new();
        private List<Package> _packages = new();
        private List<Package> _checkedPackages = new();

        private int _selectedPackageIndex = -1;
        private bool _toggled;

        public void InitializePackageList(List<Package> packages)
        {
            if (packages == null || packages.Count <= 0) return;
            _checkedPackages.Clear();
            _packages.Clear();

            _packages = packages;
            _selectedPackageIndex = -1;

            foreach (var selectableObject in _selectablePackageObjects)
                Destroy(selectableObject);
            _selectablePackageObjects.Clear();

            foreach (var package in _packages)
                CreateCheckablePackage(package);

            _selectablePackageObjects[0].GetComponent<Interactable>().Interact();
        }

        private void CreateCheckablePackage(Package package)
        {
            var newSelectable = Instantiate(selectablePackagePrefab, selectablesContainer.transform);
            var interactable = newSelectable.GetComponent<Interactable>();
            if (interactable == null) return;

            var selectableElements = newSelectable.transform.GetChild(1);
            if (selectableElements == null) return;

            var text = selectableElements.GetChild(1).GetComponent<TextMeshProUGUI>();
            text.text = package.PackageData.Title;

            var icon = selectableElements.GetChild(2).GetComponent<Image>();
            if (icon == null) return;
            icon.color = new Color();

            var itemIcon = DeliveryUitilities.ToScriptableObject(package);
            if (itemIcon.Item != null)
            {
                icon.sprite = itemIcon.Item.ItemData.icon;
                icon.color = Color.white;
                icon.gameObject.SetActive(true);
            }

            var selectDeliveryAction = new SelectDeliveryAction(this, package);
            interactable.InteractAction = selectDeliveryAction;
            _selectablePackageObjects.Add(newSelectable);
        }

        public void CheckOffCurrentPackage()
        {
            var selectedPackage = FindSelectableById(_selectedPackageIndex);
            if (selectedPackage == null) return;
            CheckOffPackage(selectedPackage);
        }

        private void CheckOffPackage(Package packageToCheckOff)
        {
            if (packageToCheckOff == null) return;
            var selectedPackage = FindSelectableByPackage(packageToCheckOff);
            if (selectedPackage == null) return;

            var selectableElements = selectedPackage.transform.GetChild(1);
            var checkMark = selectableElements.transform.GetChild(0);
            checkMark.GetChild(0).gameObject.SetActive(true);

            var interactable = selectedPackage.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.InteractAction = null;
                Destroy(interactable);
            }

            var icon = selectableElements.GetChild(2).GetComponent<Image>();
            if (icon != null) icon.color = Color.gray;

            var text = selectableElements.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (text != null) text.color = Color.gray;

            _checkedPackages.Add(packageToCheckOff);
        }

        public void SelectPackage(Package selectedPackage)
        {
            _selectedPackageIndex = FindSelectablePackage(selectedPackage);
        }

        private int FindSelectablePackage(Package selectedPackage)
        {
            if (selectedPackage == null) return -1;
            if (_checkedPackages.Contains(selectedPackage)) return -1;
            return _packages.FindIndex(packageIndex => packageIndex == selectedPackage);
        }

        private Package FindSelectableById(int index)
        {
            if (index == -1) return null;
            return _packages[index];
        }

        private GameObject FindSelectableByPackage(Package packageToFind)
        {
            if (packageToFind == null) return null;
            if (!_packages.Contains(packageToFind)) return null;
            var selectableIndex = FindSelectablePackage(packageToFind);
            if (selectableIndex < 0 || selectableIndex >= _selectablePackageObjects.Count)
                return null;

            return _selectablePackageObjects[selectableIndex];
        }

        private void ToggleCheckList()
        {
            _toggled = !_toggled;
            UI.UIManager.Instance.TogglePopupWindow(checkListUI);
            if (GameManager.Instance.CurrentDay == 1) 
                Task.TaskManager.PushTaskUpdate(new Task.Task("Here you can select what package you want to deliver"));
        }

        public void OnToggleCheckList(InputAction.CallbackContext input)
        {
            if (input.started)
            {
                ToggleCheckList();
            }
        }

        public void LoadData(GameData loadData)
        {
            _checkedPackages = loadData.checkedSelectables;
            if (loadData.deliveries != null || loadData.deliveries.Length > 0) InitializePackageList(loadData.deliveries.ToList());
        }

        public void SaveData(GameData saveData)
        {
            saveData.checkedSelectables = _checkedPackages;
        }
    }
}