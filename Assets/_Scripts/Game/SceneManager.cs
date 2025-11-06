using CarePackage.Persistance;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarePackage.Main
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded_Implementation;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded_Implementation;
        }

        private void OnSceneLoaded_Implementation(Scene scene, LoadSceneMode sceneMode)
        {
            Debug.Log($"Loaded Scene {scene.name}");
            // PostOffice
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(2))
            {
                Invoke("StartTheDay", .05f);
            }
            // Neighbourhood
            else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3))
            {
                Invoke("SetMailBoxes", .01f);
            }
            // Ending
            else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(6))
            {
                if (!GameManager.Instance.Survived)
                {
                    GameObject.Find("UI_Failed").GetComponentInChildren<TextMeshProUGUI>().text =
                    "You Failed to reach the required Amount: " + EconomyManager.Instance.GetRequiredMoney + " and your store shutdown.";
                }
                else
                {
                    GameObject.Find("UI_Failed").SetActive(false);
                }
            }
        }

        private void SetMailBoxes()
        {
            GameManager.Instance.StartDay();
            GameManager.Instance.Player.DeliveryManager.AssignRandomAddressesForDelivery();
            GoalIndicator.Instance.Camera = GameManager.Instance.Player.SwitchMode.CarCamera;
        }

        private void StartTheDay()
        {
            GameManager.Instance.StartGame();
        }

        public void LoadScene(string sceneName)
        {
            if (DataPersistanceManager.Instance != null) DataPersistanceManager.Instance.SaveGame();
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}