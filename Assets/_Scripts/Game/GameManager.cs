using CarePackage.Persistance;
using UnityEngine;
using PrimeTween;
using System;
using CarePackage.Task;
using TMPro;

namespace CarePackage.Main
{
    public interface ISceneController
    {
        void OnEnter();
        void OnExit();
    }
    
    public class GameManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private float dayTime = 120f;
        [SerializeField] private TMP_Text timeLeftText;
        [SerializeField] private GameObject survivedPanel;
        
        [SerializeField] private bool automaticallyEndDayWhenNoPackagesLeft = true;
        [SerializeField] private bool automaticallyLoseAtSpecificDay = true;
        [SerializeField] private int dayToLose = 3;
        
        public bool ShouldAutomaticallyEndDayEarlyIfNoPackagesLeft => automaticallyEndDayWhenNoPackagesLeft;
        public int CurrentDay => _day;
        public bool Survived => _survived;
        public bool tutorialDone = false;
        private bool _isPaused = false;

        private int _elapsedTime;
        private int _currentSecond;
        private int _lastUpdateSecond;
        private int _day;
        private bool _survived;
        
        // events
        public static event Action onStartGame;
        public static event Action onGameRestart;
        public static event Action<int> onDayStarted;
        public static event Action<int> onDayEntered;
        public static event Action<int> onDayEnded;
        
        public PlayerState Player;

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (Player == null) Player = FindFirstObjectByType<PlayerState>(FindObjectsInactive.Include);
        }

        public void SetPlayer(PlayerState player)
        {
            Player = player;
        }

        public void EnterDay()
        {
            onDayEntered?.Invoke(_day);
            _lastUpdateSecond = -1;
            Tween.Custom(0f, dayTime, dayTime, UpdateTimer, Ease.Linear).OnComplete(EndDay);
            if (_day == 1) TaskManager.PushTaskUpdate(new Task.Task("Press V to get an overview of all the packages you need"));
        }

        public void StartGame()
        {
            _day = 1;
            onStartGame?.Invoke();
            SceneController.Instance.LoadScene(ECarePackageScenes.PostOffice);
        }
        
        public void StartDay()
        {
            _survived = false;
            onDayStarted?.Invoke(_day);
        }
        
        void UpdateTimer(float elapsed)
        {
            _currentSecond = Mathf.FloorToInt(elapsed);
            if (_currentSecond != _lastUpdateSecond)
            {
                _lastUpdateSecond = _currentSecond;
                _elapsedTime = Mathf.CeilToInt(dayTime - elapsed);
                var displayTime = TimeSpan.FromSeconds(_elapsedTime);
                string formattedTime = string.Format("{0:D2}:{1:D2}", displayTime.Minutes, displayTime.Seconds);
                timeLeftText.text = "Time left: " + formattedTime;
            }
        }

        private void EndDay()
        {
            _day++;
            if (automaticallyLoseAtSpecificDay) 
                if (_day >= dayToLose) LoseGame();
            
            DialogueManager.Instance.SetYarnFloat("$day", _day);
            onDayEnded?.Invoke(_day);
        }

        public void EndDayEarly()
        {
            Tween.StopAll();
            EndDay();
        }

        public void LoseGame()
        {
            Debug.Log("Lost Game");
            _survived = false;
            SceneController.Instance.LoadScene(ECarePackageScenes.Ending);
        }

        public void WinGame()
        {
            _survived = true;
            SceneController.Instance.LoadScene(ECarePackageScenes.Ending);
        }

        public void RestartGame()
        {
            _survived = false;
            _day = 1;
            onGameRestart?.Invoke();
            StartGame();
            SceneController.Instance.LoadScene(ECarePackageScenes.PostOffice);
        }

        public void Survive()
        {
            survivedPanel.SetActive(true);
            _survived = true;
            Invoke("Restart", 6f);
        }

        private void Restart()
        {
            SceneController.Instance.LoadScene(ECarePackageScenes.PostOffice);
        }

        public void SetGameSetting(UI.CheckBoxGamesSetting.EGameSetting gameSetting, bool newValue)
        {
            switch (gameSetting)
            {
                case UI.CheckBoxGamesSetting.EGameSetting.EndWhenEmpty:
                    automaticallyEndDayWhenNoPackagesLeft = newValue;
                    break;
                case UI.CheckBoxGamesSetting.EGameSetting.LoseAtDay:
                    automaticallyLoseAtSpecificDay = newValue;
                    break;
                default:
                    break;
            }
        }
        
        public void SaveDeliverySetting(UI.CheckBoxGamesSetting.EGameSetting gameSetting, bool newValue)
        {
            PlayerPrefs.SetInt(gameSetting.ToString(), newValue ? 1 : 0);
        }
        
        public bool GetDeliverySetting(UI.CheckBoxGamesSetting.EGameSetting gameSetting)
        {
            var value = PlayerPrefs.GetInt(gameSetting.ToString(), 0);
            return value == 0 ? false : true;
        }

        public void LoadData(GameData loadData)
        {
            _survived = loadData.survived;
            if (loadData.day > 0)
            {
                _day = loadData.day;
                if (DialogueManager.Instance != null) DialogueManager.Instance.SetYarnFloat("$day", _day);
            }

            tutorialDone = loadData.doneTutorial;
        }

        public void SaveData(GameData saveData)
        {
            saveData.survived = _survived;
            saveData.day = _day;
            saveData.doneTutorial = tutorialDone;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SceneController.Instance != null && SceneController.Instance.ActiveScene == ECarePackageScenes.NeighbourHood)
                {
                    TogglePause();
                }
            }
        }
           


        public void TogglePause()
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        private void PauseGame()
        {
            _isPaused = true;
            Time.timeScale = 0f;
        }

        private void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
        }

    }
}
