using System;
using System.Collections;
using CarePackage.Persistance;
using TMPro;
using UnityEngine;

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
        
        private float _elapsedTime;
        [SerializeField] private bool _survived = true;
        private int _day;
        
        public bool Survived => _survived;
        public int CurrentDay => _day;
        
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
            _elapsedTime = 0f;
            StartCoroutine(DayCoroutine());
            onDayStarted?.Invoke(_day);
        }

        private void EndDay()
        {
            Debug.Log("Day ended");
            _day++;
            
            if (automaticallyLoseAtSpecificDay) 
                if (_day >= dayToLose) LoseGame();
            
            DialogueManager.Instance.SetYarnFloat("$day", _day);
            onDayEnded?.Invoke(_day);
        }

        public void EndDayEarly()
        {
            StopAllCoroutines();
            EndDay();
        }

        private IEnumerator DayCoroutine()
        {
            while (_elapsedTime < dayTime)
            {
                yield return new WaitForSecondsRealtime(1);
                _elapsedTime++;
                var displayedTime = dayTime - _elapsedTime;
                var displayTime = TimeSpan.FromSeconds(displayedTime);
                string displayedTimeString = string.Format("{0:D2}:{1:D2}", displayTime.Minutes, displayTime.Seconds);;
                timeLeftText.text = "Time left: " + displayedTimeString;
            }
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
