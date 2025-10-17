using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Job
{
    public class JobBoard : MonoBehaviour
    {
        [SerializeField] private Button[] jobButtons;
        [SerializeField] private GameObject jobListing;

        private TextMeshProUGUI _jobTitle;
        private TextMeshProUGUI _jobDescription;

        private void Start()
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
        }

        public void OnExitJobClicked(GameObject button)
        {
            UIManager.Instance.ClosePopupWindow(button);
        }

        public void SetJobListing(IJob job)
        {
            _jobTitle.text = job.GetTitle();
            _jobDescription.text = job.GetDescription();
        }
    }
}