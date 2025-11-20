using System.Collections.Generic;
using System.Linq;
using CarePackage.Interaction.Delivery;
using CarePackage.Interaction;
using CarePackage.Main;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
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
        [SerializeField] private GameObject jobListingElemets;
        [SerializeField] private GameObject jobPrefab;
        [SerializeField] private Transform jobsContainer;
        [SerializeField] private List<GameObject> jobNotes;
        [SerializeField] private float maxOffset = 5f;

        [SerializeField] private List<ScriptedJob> scriptedJobs = new();

        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private GameObject packageConveyerBelt;

        private List<GameObject> _jobButtons = new();
        private HashSet<GameObject> _movingPackages = new();
        private Package _displayedJob;
        private GameObject _lastClickedButton;
        private List<GameObject> _spawnedPackages = new();
        private TextMeshProUGUI _jobTitle;
        private TextMeshProUGUI _jobDescription;
        private Image _jobImage;
        private ConveyorBeltController _conveyorController;
        private float _joblistingX = 550f;

        private void Awake()
        {
            FetchJobListedElements();
            if (_conveyorController != null) _conveyorController.SetSpeed(0f);
        }

        private void FetchJobListedElements()
        {
            _jobTitle = jobListing.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            _jobDescription = jobListingElemets.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            _jobImage = jobListingElemets.transform.GetChild(0).GetComponentInChildren<Image>();
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
                Debug.Log("There are " + GetScriptedJobsByDay(GameManager.Instance.CurrentDay) +
                          " scripted deliveries for the current day: " + GameManager.Instance.CurrentDay);
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
            if (package == null) return;

            Tween.StopAll(package.transform);
            _spawnedPackages.Remove(package);
            MovePackagesAlong();
        }

        public void SetJobListing(SetListedJobAction job, bool isLeft = false)
        {
            _displayedJob = job.Job;
            _jobTitle.text = _displayedJob.PackageData.Title;
            _jobDescription.text = _displayedJob.PackageData.Description;
            _jobImage.gameObject.SetActive(false);
            
            var dispJobSrciptable = DeliveryUitilities.ToScriptableObject(_displayedJob);
            if (dispJobSrciptable == null) return;
            
            if (dispJobSrciptable.Item != null)
            {
                _jobImage.gameObject.SetActive(true);
                _jobImage.sprite = dispJobSrciptable.Item.ItemData.icon;
            }
            _joblistingX = isLeft ? -550f : 550f;
            OnJobClicked(job.OwningObject);
        }

        private void OnJobClicked(GameObject button)
        {
            UIManager.Instance.OpenPopupWindow(jobListing);
            _lastClickedButton = button;
            jobListing.GetComponent<RectTransform>().anchoredPosition = new Vector2(_joblistingX, 0);
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
            CreatePackageForConveyerBelt(_displayedJob);
            OnExitJobClicked(_lastClickedButton);
            UIManager.Instance.ClosePopupWindow(jobListing);
        }

        private void CreatePackageForConveyerBelt(Package packageData)
        {
            if (packageData == null) return;
            var package = packageData;

            var newPackage = Instantiate(packagePrefab, packageConveyerBelt.transform.GetChild(0).position, Quaternion.identity);
            var packageInteractable = newPackage.GetComponent<Interactable>();
            var packageObj = newPackage.GetComponent<PackageBehavior>();

            var extendedPickups = new IPickupExtension[]
            {
                new ConveyerBeltPackageExtension(newPackage)
            };
            var packageAction =
                new PackageAction(package, false, new Vector3(0, -0.1f, 0), newPackage, extendedPickups);
            packageInteractable.InteractAction = packageAction;

            _spawnedPackages.Add(newPackage);
            Tween.Delay(.33f).OnComplete(() => packageObj.TogglePhysics(false));
            MovePackagesAlong();
        }

        public void CreateJob(Package job)
        {
            var newJob = Instantiate(jobPrefab, jobsContainer);
            var rect = newJob.GetComponent<RectTransform>();
            var screenHeight = 380;
            var screenWidth = 780;
            rect.anchoredPosition = new Vector2(Random.Range(-screenWidth, screenWidth),
                Random.Range(-screenHeight, screenHeight));

            var interactable = newJob.GetComponent<Interactable>();
            interactable.InteractAction = new SetListedJobAction(newJob, job);
            _jobButtons.Add(newJob);
        }

        public void InitRandomJobsForPackages(List<Package> packages)
        {/*
            for (int i = 0; i < packages.Count; i++)
            {
                InitJob(packages[i]);
            }*/
            
            if (packages == null || packages.Count == 0)
                return;

            var scriptedPackages = GetScriptedJobsByDayCount(GameManager.Instance.CurrentDay);
            int totalPackages = packages.Count - scriptedPackages;
            int availableJobNotes = jobNotes.Count - scriptedPackages;
            int maxAssignable = Mathf.Min(availableJobNotes, totalPackages);
            
            float biasFactor = Mathf.Clamp01((float)totalPackages / 15f);
            int scaledJobs = Mathf.RoundToInt(Mathf.Lerp(3, maxAssignable, biasFactor));

            int jobsToAssign = Mathf.Clamp(scaledJobs, 2, Mathf.Min(10, maxAssignable));
            
            List<Package> shuffledPackages = new List<Package>(packages);
            for (int i = 0; i < shuffledPackages.Count; i++)
            {
                int swapIndex = Random.Range(i, shuffledPackages.Count);
                (shuffledPackages[i], shuffledPackages[swapIndex]) = (shuffledPackages[swapIndex], shuffledPackages[i]);
            }
            
            for (int i = 0; i < shuffledPackages.Count; i++)
            {
                if (i < jobsToAssign) InitJob(shuffledPackages[i]);
                else
                {
                    var p = shuffledPackages[i];
                    //Tween.Delay(1f).OnComplete(() => CreatePackageForConveyerBelt(p));
                    CreatePackageForConveyerBelt(p);
                }
            }
        }

        public void InitJob(Package job)
        {
            var jobNote = GetAvailableJobNote();/*
            if (jobNote == null)
            {
                CreatePackageForConveyerBelt(job);
                return;
            }*/
            
            if (jobNote == null)
            {
                Debug.LogError("No available job note found. All notes may be active.");
                return;
            }
            
            var interactable = jobNote.GetComponent<Interactable>();
            var positionX = jobNote.GetComponent<RectTransform>().anchoredPosition.x;
            var setJobListingAction = new SetListedJobAction(jobNote.gameObject, job, positionX > 0);
            interactable.InteractAction = setJobListingAction;

            _jobButtons.Add(jobNote);
            jobNote.SetActive(true);
        }

        private GameObject GetAvailableJobNote()
        {/*
            foreach (var jobNote in jobNotes)
            {
                var avaiable = !jobNote.gameObject.activeSelf;
                if (avaiable) return jobNote;
            }
            return null;*/
            
            int attempts = 0;
            int maxAttempts = jobNotes.Count;
            while (attempts < maxAttempts)
            {
                int randomIndex = Random.Range(0, maxAttempts);
                var jobNote = jobNotes[randomIndex];

                if (!jobNote.gameObject.activeSelf)
                {
                    return jobNote;
                }

                attempts++;
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

        public int GetScriptedJobsByDayCount(int day)
        {
            var scriptedJobsForDay = GetScriptedJobsByDay(day);
            return scriptedJobsForDay.Count;
        }

        public Package[] GetScriptedJobsDeliveriesByDay(int day)
        {
            var packagesForDay = GetScriptedJobsByDay(day).Select(delivery => delivery.TargetPackage).ToArray();
            /*var scriptedJobsForDay = GetScriptedJobsByDay(day);
            /*var packagesForDay = new Package[scriptedJobsForDay.Count];
            for (int i = 0; i < packagesForDay.Length - 1; i++)
            {
                packagesForDay[i] = scriptedJobsForDay[i].TargetPackage;
            }*/
            return packagesForDay;
        }

        public List<ScriptedJob> GetScriptedJobsByDay(int day)
        {
            return scriptedJobs.FindAll(p => p.TargetDay == day);
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

                if (Vector3.Distance(package.transform.position, targetPosition) < 0.01f && !_movingPackages.Contains(package)) 
                    continue;
                _movingPackages.Add(package);
                package.transform.rotation = Quaternion.identity;
                Tween.PositionAtSpeed(package.transform, targetPosition, 1.2f, Ease.Linear)
                    .OnComplete(() => FinishMovingPackage(package));
            }
        }

        private void FinishMovingPackage(GameObject package)
        {
            _movingPackages.Remove(package);
            if (_movingPackages.Count == 0 && _conveyorController != null) 
                _conveyorController.SetSpeed(0f);
        }
    }
}