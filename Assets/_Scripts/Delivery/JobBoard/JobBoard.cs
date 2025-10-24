using System.Collections;
using System.Collections.Generic;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Delivery
{
    public class JobBoard : MonoBehaviour
    {
        [SerializeField] private Renderer material;
        [SerializeField] private Button[] jobButtons;
        [SerializeField] private GameObject jobListing;

        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private GameObject packageConveyerBelt;

        private SO_Package _displayedJob;
        private GameObject _lastClickedButton;
        private List<GameObject> _spawnedPackages = new();
        
        private TextMeshProUGUI _jobTitle;
        private TextMeshProUGUI _jobDescription;

        private void Start()
        {
            material.material.SetFloat(Shader.PropertyToID("T_Speed"), 0f); 
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
        
        public void SetJobListing(SO_Package job)
        {
            _displayedJob = job;
            _jobTitle.text = job.PackageData.Title;
            _jobDescription.text = job.PackageData.Description;
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
            _spawnedPackages.Add(newPackage);
            OnExitJobClicked(_lastClickedButton);
            MovePackagesAlong();
        }
        
        private Vector3 CalculateTargetPosition(int index, float spacing)
        {
            var startPoint = packageConveyerBelt.transform.GetChild(0).position;
            var direction = packageConveyerBelt.transform.GetChild(0).forward;

            float offset = 0f;
            for (int i = 0; i < index; i++)
            {
                var prevPackage = _spawnedPackages[i];
                var collider = prevPackage.GetComponent<Collider>();
                if (collider != null)
                {
                    offset += collider.bounds.size.z + spacing;
                }
            }

            var currentPackage = _spawnedPackages[index];
            var currentCollider = currentPackage.GetComponent<Collider>();
            if (currentCollider != null)
            {
                offset += currentCollider.bounds.size.z / 2f;
            }

            return startPoint + direction * offset;
        }
        
        private void MovePackagesAlong()
        {
            for (int i = 0; i < _spawnedPackages.Count; i++)
            {
                var package = _spawnedPackages[i];
                var targetPosition = CalculateTargetPosition(i, .5f);

                if (Vector3.Distance(package.transform.position, targetPosition) > 0.01f)
                {
                    StartCoroutine(MovePackageToPosition(package, targetPosition));
                }
            }
        }
        
        private IEnumerator MovePackageToPosition(GameObject package, Vector3 targetPosition)
        {
            
            float speed = 2f; // Adjust as needed
            while (Vector3.Distance(package.transform.position, targetPosition) > 0.01f)
            {
                package.transform.position = Vector3.MoveTowards(package.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            package.transform.position = targetPosition;
        }
    }
}