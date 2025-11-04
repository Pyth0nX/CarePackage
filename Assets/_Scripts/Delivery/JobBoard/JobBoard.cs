using System.Collections;
using System.Collections.Generic;
using CarePackage.Interaction;
using CarePackage.Interaction.Delivery;
using CarePackage.Main;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CarePackage.Delivery
{
    [System.Serializable]
    public class ScriptedJob
    {
        [SerializeField] private int dayToAppear;
        [SerializeField] private SO_Package deliveryToAppear;
            
        public int TargetDay => dayToAppear;
        public Package TargetPackage => DeliveryUitilities.ToPackage(deliveryToAppear);
    }
    
    public class JobBoard : MonoBehaviour
    {
        [SerializeField] private GameObject jobListing;
        [SerializeField] private GameObject jobPrefab;
        [SerializeField] private Transform jobsContainer;
        [SerializeField] private List<Button> jobNotes;
        [SerializeField] private float maxOffset = 5f;
        
        [SerializeField] private List<ScriptedJob> scriptedJobs = new();

        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private GameObject packageConveyerBelt;
        
        private List<Button> _jobButtons = new();
        private HashSet<GameObject> _movingPackages = new();
        private Package _displayedJob;
        private GameObject _lastClickedButton;
        private List<GameObject> _spawnedPackages = new();
        private TextMeshProUGUI _jobTitle;
        private TextMeshProUGUI _jobDescription;
        private ConveyorBeltController _conveyorController;

        private void Awake()
        {
            FetchJobListedElements();
            if (_conveyorController != null) _conveyorController.SetSpeed(0f);
        }

        private void FetchJobListedElements()
        {
            _jobTitle = jobListing.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            _jobDescription = jobListing.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
            _conveyorController = packageConveyerBelt.GetComponentInChildren<ConveyorBeltController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                var randomIndex = Random.Range(0, _spawnedPackages.Count);
                RemovePackageFromConveyerBelt(randomIndex);
            }
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("There are " + GetScriptedJobsByDay(GameManager.Instance.CurrentDay) + " scripted deliveries for the current day: " + GameManager.Instance.CurrentDay);
            }
        }

        private void RemovePackageFromConveyerBelt(int index)
        {
            GameObject package = _spawnedPackages[index];
            _spawnedPackages.Remove(package);
            Destroy(package);
        }
        
        public void RemovePackageFromConveyerBelt(GameObject objToFind)
        {
            GameObject package = _spawnedPackages.Find(obj => obj == objToFind);
            _spawnedPackages.Remove(package);
            //Destroy(package);
        }

        private void OnEnable()
        {
            foreach (var button in _jobButtons)
            {
                button.onClick.AddListener((() => OnJobClicked(button.gameObject)));
            }
        }

        private void OnDisable()
        {
            foreach (var button in _jobButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        public void SetJobListing(Package job)
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

        public void OnExitBoardClicked()
        {
            //var popupsToClose = new List<GameObject> {gameObject, button};
            UIManager.Instance.CloseAllPopupWindows();
        }

        public void OnAcceptJobClicked()
        {
            if (_displayedJob == null) return;
            var newPackage = Instantiate(packagePrefab, packageConveyerBelt.transform.GetChild(0).position, Quaternion.identity);
            var package = newPackage.GetComponent<PackageObject>();
            var packageInteractable = newPackage.GetComponent<Interactable>();
            packageInteractable.InteractAction = new PackageAction(_displayedJob);
            
            _spawnedPackages.Add(newPackage);
            OnExitJobClicked(_lastClickedButton);
            MovePackagesAlong();
            UIManager.Instance.ClosePopupWindow(jobListing);
            package.TogglePhysics(false);
        }

        public void CreateJob(Package job)
        {
            var newJob = Instantiate(jobPrefab, jobsContainer);
            var rect = newJob.GetComponent<RectTransform>();
            var screenHeight = 380;
            var screenWidth = 780;
            rect.anchoredPosition = new Vector2(Random.Range(-screenWidth, screenWidth), Random.Range(-screenHeight, screenHeight));
            
            var interactable = newJob.GetComponent<Interactable>();
            interactable.InteractAction = new SetListedJobAction();
            var interactAction = interactable.InteractAction;
            if (interactAction is SetListedJobAction jobListingAction)
            {
                jobListingAction.SetParent(newJob);
                jobListingAction.SetJob(job);
            }
            _jobButtons.Add(newJob.GetComponent<Button>());
        }

        public void InitRandomJobsForPackages(List<Package> packages)
        {
            for (int i = 0; i < jobNotes.Count - GetScriptedJobsByDay(GameManager.Instance.CurrentDay); i++)
            {
                InitJob(packages[i]);
            }
        }

        public void InitJob(Package job)
        {
            var jobNote = GetAvailableJobNote();
            if (jobNote == null) return;
            
            var interactable = jobNote.GetComponent<Interactable>();
            interactable.InteractAction = new SetListedJobAction();
            var interactAction = interactable.InteractAction;
            if (interactAction is SetListedJobAction jobListingAction)
            {
                jobListingAction.SetParent(jobNote.gameObject);
                jobListingAction.SetJob(job);
            }
            _jobButtons.Add(jobNote);
            jobNote.gameObject.SetActive(true);
        }

        private Button GetAvailableJobNote()
        {
            foreach (var jobNote in jobNotes)
            {
                var avaiable = !jobNote.gameObject.activeSelf;
                if (avaiable) return jobNote;
            }
            return null;
        }
        
        public void CheckScriptedJobs()
        {
            var day = GameManager.Instance.CurrentDay;
            foreach (var scriptedJob in scriptedJobs)
            {
                if (scriptedJob.TargetDay != day)
                {
                    Debug.Log($"Skipping job {scriptedJob.TargetDay} as it is not the current day: {day}");
                    continue;
                }
                InitJob(scriptedJob.TargetPackage);
            }
        }

        public int GetScriptedJobsByDay(int day)
        {
            return scriptedJobs.FindAll(j => j.TargetDay == day).Count;
        }
        
        private Vector3 CalculateTargetPositionFromEnd(int index, float spacing)
        {
            var startPoint = packageConveyerBelt.transform.GetChild(0).position;
            var direction = packageConveyerBelt.transform.GetChild(0).forward;
            
            var nextPackage = _spawnedPackages[index];
            var collider = nextPackage.GetComponent<Collider>();
            if (collider == null) return startPoint;
            
            float offset = index * (GetPackageDepth(collider) + spacing);
            return startPoint + direction * (maxOffset - offset);
        }

        private float GetPackageDepth(Collider package)
        {
            var bounds = package.bounds;
            return bounds.size.z;
        }
        
        private void MovePackagesAlong()
        {
            if (_movingPackages.Count == 0 && _conveyorController != null) 
                _conveyorController.SetSpeed(1f);
            
            for (int i = 0; i < _spawnedPackages.Count; i++)
            {
                var package = _spawnedPackages[i];
                var targetPosition = CalculateTargetPositionFromEnd(i, 0.5f);

                if (Vector3.Distance(package.transform.position, targetPosition) > 0.01f) 
                    StartCoroutine(MovePackageToPosition(package, targetPosition));
            }
        }
        
        private IEnumerator MovePackageToPosition(GameObject package, Vector3 targetPosition)
        {
            if (_movingPackages.Contains(package)) yield break;
            
            _movingPackages.Add(package);
            var speed = 1f; // Adjust as needed
            while (Vector3.Distance(package.transform.position, targetPosition) > 0.01f)
            {
                package.transform.position = Vector3.MoveTowards(package.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            package.transform.position = targetPosition;
            _movingPackages.Remove(package);
            
            if (_movingPackages.Count == 0 && _conveyorController != null) 
                _conveyorController.SetSpeed(0f);
        }
    }
}