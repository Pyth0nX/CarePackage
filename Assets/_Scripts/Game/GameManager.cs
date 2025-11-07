using CarePackage.Persistance;
using PrimeTween;
using UnityEngine;
using System;
using TMPro;

namespace CarePackage.Main
{
    public class GameManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private float dayTime = 120f;
        [SerializeField] private TMP_Text timeLeftText;
        [SerializeField] private GameObject survivedPanel;
        
        [SerializeField] private bool automaticallyEndDayWhenNoPackagesLeft = true;
        [SerializeField] private bool automaticallyLoseAtSpecificDay = true;
        [SerializeField] private int dayToLose = 3;
        
        public bool ShouldAutomaticallyEndDayEarlyIfNoPackagesLeft => automaticallyEndDayWhenNoPackagesLeft;
        public bool Survived => _survived;
        public int CurrentDay => _day;
        
        private int _elapsedTime;
        private int _currentSecond;
        private int _lastUpdateSecond;
        private bool _survived;
        private int _day;
        
        // events
        public event Action onStartGame;
        public event Action onGameRestart;
        public event Action<int> onDayStarted;
        public event Action<int> onDayEnded;
        
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

        public void StartGame()
        {
            _day = 1;
            onStartGame?.Invoke();
        }
        
        public void StartDay()
        {
            _survived = false;
            _lastUpdateSecond = -1;
            Tween.Custom(0f, dayTime, dayTime, UpdateTimer, Ease.Linear).OnComplete(EndDay);
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
            SceneController.Instance.LoadScene("Ending");
        }

        public void WinGame()
        {
            _survived = true;
            SceneController.Instance.LoadScene("Ending");
        }

        public void RestartGame()
        {
            _survived = false;
            _day = 1;
            onGameRestart?.Invoke();
            StartGame();
            SceneController.Instance.LoadScene("PostOffice");
        }

        public void Survive()
        {
            survivedPanel.SetActive(true);
            _survived = true;
            Invoke("Restart", 6f);
        }

        private void Restart()
        {
            SceneController.Instance.LoadScene("PostOffice");
        }

        public void LoadData(GameData loadData)
        {
            _survived = loadData.survived;
            if (loadData.day > 0)
            {
                _day = loadData.day;
                if (DialogueManager.Instance != null) DialogueManager.Instance.SetYarnFloat("$day", _day);
            }
        }

        public void SaveData(GameData saveData)
        {
            saveData.survived = _survived;
            saveData.day = _day;
        }
    }
}
