using System;
using System.Collections;
using CarePackage.Persistance;
using UnityEngine;

namespace CarePackage.Main
{
    public class GameManager : MonoBehaviour, IDataPersistance
    {
        [SerializeField] private float dayTime = 120f;
        
        private float _elapsedTime;
        [SerializeField] private bool _survived;
        
        // events
        public Action OnStartGame;
        public Action OnDayStarted;
        public Action OnDayEnded;
        
        public PlayerState Player;

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (Player == null) Player = FindFirstObjectByType<PlayerState>(FindObjectsInactive.Include);
            StartGame();
        }

        public void SetPlayer(PlayerState player)
        {
            Player = player;
        }

        private void StartGame()
        {
            _elapsedTime = 0f;
            StartCoroutine(DayCoroutine());
            OnStartGame?.Invoke();
        }

        private void EndDay()
        {
            OnDayEnded?.Invoke();
        }

        private IEnumerator DayCoroutine()
        {
            while (_elapsedTime < dayTime)
            {
                yield return new WaitForSecondsRealtime(1);
                _elapsedTime++;
            }
            EndDay();
        }

        public void LoseGame()
        {
            _survived = false;
            SceneController.Instance.LoadScene("LoseScene");
        }

        public void WinGame()
        {
            
        }

        public void LoadData(GameData loadData)
        {
            // load persistance data
        }

        public void SaveData(GameData saveData)
        {
            // save persistance data
        }
    }
}
