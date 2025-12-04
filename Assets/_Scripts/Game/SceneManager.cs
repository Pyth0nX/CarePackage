using CarePackage.Persistance;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace CarePackage.Main
{
    public class SceneController : MonoBehaviour
    {
        private System.DateTime _timeAtEnteringScene;
        private ECarePackageScenes _activeScene;
        private ECarePackageScenes? _lastActiveScene;

        public ECarePackageScenes ActiveScene => _activeScene;
        public ECarePackageScenes LastActiveScene => _lastActiveScene.HasValue ? _lastActiveScene.Value : default;
        
        public static SceneController Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded_Implementation;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded_Implementation;
        }
        
        public void LoadScene(ECarePackageScenes scene)
        {
            if (DataPersistanceManager.Instance != null) DataPersistanceManager.Instance.SaveGame();
            SceneManager.LoadScene((int)scene);
        }
        
        public void LoadSceneFromString(string sceneName)
        {
            if (System.Enum.TryParse<ECarePackageScenes>(sceneName, true, out var sceneEnum))
            {
                LoadScene(sceneEnum);
            }
            else
            {
                Debug.LogWarning($"Invalid scene name '{sceneName}' passed to LoadSceneFromString");
            }
        }
        
        public void LoadSceneByIndex(int sceneIndex)
        {
            if (DataPersistanceManager.Instance != null) DataPersistanceManager.Instance.SaveGame();
            LoadScene((ECarePackageScenes)sceneIndex);
        }
        
        private void OnSceneLoaded_Implementation(Scene scene, LoadSceneMode sceneMode)
        {
            Debug.Log($"Loaded Scene {scene.name} with index: {scene.buildIndex}");
            _activeScene = (ECarePackageScenes)scene.buildIndex;

#if !UNITY_WEBGL
            if (_lastActiveScene.HasValue)
            {
                CompleteTimeSpentInSceneAnalysisForScene(_lastActiveScene.Value);
            }
            StartTimeSpentInSceneAnalysisForScenme(_activeScene);
#endif

            switch (_activeScene)
            {
                case ECarePackageScenes.MainMenu:
                    HandleMainMenuScene();
                    break;
                
                case ECarePackageScenes.Tutorial:
                    HandleTutorialScene();
                    break;
                
                case ECarePackageScenes.PostOffice:
                    HandlePostOfficeScene();
                    //Invoke(nameof(HandlePostOfficeScene), 0.01f);
                    break;
                
                case ECarePackageScenes.NeighbourHood:
                    Invoke(nameof(HandleNeighbourHoodScene), 0.01f);
                    break;
                
                case ECarePackageScenes.Ending:
                    break;
                default:
                    break;
            }
            _lastActiveScene = _activeScene;
        }

        private void HandleMainMenuScene()
        {
            
        }
        
        private void HandleTutorialScene()
        {
            
        }
        
        private void HandleNeighbourHoodScene()
        {
            GameManager.Instance.EnterDay();
        }

        private void HandlePostOfficeScene()
        {
            Debug.Log("Entered Post Office Scene with values: lastActiveScene: " + (_lastActiveScene.HasValue ? _lastActiveScene : "null") + " activeScene: " + _activeScene);
            if (!_lastActiveScene.HasValue || _lastActiveScene.Value != ECarePackageScenes.PostOffice)
            {
                Debug.Log("Starting Day in Post Office");
                GameManager.Instance.StartDay();
            }
            if (GameManager.Instance.CurrentDay == 1) 
                Task.TaskManager.PushTaskUpdate(new Task.Task("Check in at the computer"));
        }
        
#if !UNITY_WEBGL
        private void StartTimeSpentInSceneAnalysisForScenme(ECarePackageScenes scene)
        {
            var sceneCompletableId = "TimeSpent_" + scene;
            Debug.Log($"Starting to track {sceneCompletableId}");
            if (Xasu.XasuTracker.Instance == null) return;
            _timeAtEnteringScene = System.DateTime.Now;
            Xasu.HighLevel.CompletableTracker.Instance.Initialized(sceneCompletableId, Xasu.HighLevel.CompletableTracker.CompletableType.Level);
        }

        private void CompleteTimeSpentInSceneAnalysisForScene(ECarePackageScenes scene)
        {
            var sceneCompletableId = "TimeSpent_" + scene;
            Debug.Log($"Completing {sceneCompletableId}");
            if (Xasu.XasuTracker.Instance == null) return;
            Xasu.HighLevel.CompletableTracker.Instance.Completed(sceneCompletableId, Xasu.HighLevel.CompletableTracker.CompletableType.Level).WithSuccess(true).WithDuration(_timeAtEnteringScene, System.DateTime.Now);
        }
#endif
        
        public static ECarePackageScenes GetActiveScene()
        {
            return (ECarePackageScenes)SceneManager.GetActiveScene().buildIndex;
        }
        
        public void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
#if !UNITY_WEBGL
        private void OnApplicationQuit()
        {
            CompleteTimeSpentInSceneAnalysisForScene(ActiveScene);
        }
#endif
    }
}