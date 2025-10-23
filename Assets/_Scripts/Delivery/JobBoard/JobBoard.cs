using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Delivery
{
    public class JobBoard : MonoBehaviour
    {
        [SerializeField] private Button[] jobButtons;
        [SerializeField] private GameObject jobListing;

        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private GameObject packageConveyerBelt;

        private SO_Package _displayedJob;
        private GameObject _lastClickedButton;
        
        private TextMeshProUGUI _jobTitle;
        private TextMeshProUGUI _jobDescription;

        private void Start()
        {
            FetchJobListedElements();
        }

        private void FetchJobListedElements()
        {
            _jobTitle = jobListing.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            _jobDescription = jobListing.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            foreach (var button in jobButtons)
            {
                button.onClick.AddListener((() => OnJobClicked(button.gameObject)));
            }
        }

        private void OnDisable()
        {
            foreach (var button in jobButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void OnJobClicked(GameObject button)
        {
            Debug.Log("You have clicked " + button);
            UIManager.Instance.OpenPopupWindow(jobListing);
            _lastClickedButton = button;
        }

        public void OnExitJobClicked(GameObject button)
        {
            UIManager.Instance.ClosePopupWindow(button);
        }

        public void OnAcceptJobClicked()
        {
            if (_displayedJob == null) return;
            GameObject newPackage = Instantiate(packagePrefab, packageConveyerBelt.transform.GetChild(0).position, Quaternion.identity);
            var package = newPackage.GetComponent<Interactable>();
            package.InteractAction = new PackageAction(_displayedJob);
            OnExitJobClicked(_lastClickedButton);
        }
        
        public void SetJobListing(SO_Package job)
        {
            _displayedJob = job;
            _jobTitle.text = job.PackageData.Title;
            _jobDescription.text = job.PackageData.Description;
        }
    }
}