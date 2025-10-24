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
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3))
            {
                Debug.Log($"Loaded Scene {scene.name}");
                Invoke("Stupid", .01f);
            }
        }

        private void Stupid()
        {
            GameManager.Instance.OnDayStarted?.Invoke();
            GameManager.Instance.Player.DeliveryManager.AssignMailBoxesRandom();
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}