using System;
using CarePackage.Persistance;
using UnityEngine;

namespace CarePackage.Main
{
    public class GameManager : MonoBehaviour, IDataPersistance
    {
        // events
        public Action OnStartGame;
        public Action OnDayStarted;
        public Action OnDayEnded;
        
        public PlayerState Player;

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            StartGame();
        }

        private void Start()
        {
            
        }

        public void SetPlayer(PlayerState player)
        {
            Player = player;
        }

        private void StartGame()
        {
            OnStartGame?.Invoke();
        }

        private void EndDay()
        {
            OnDayEnded?.Invoke();
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
