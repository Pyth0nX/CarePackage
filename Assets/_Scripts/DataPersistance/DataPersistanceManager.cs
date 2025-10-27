using CarePackage.Main;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarePackage.Persistance
{
    public class DataPersistanceManager : MonoBehaviour
    {
        [SerializeField] private bool disableDataPersistance = false;
        [SerializeField] private bool initializeDataIfNull = false;
        [SerializeField] private bool overrideSelectedProfileId = false;
        [SerializeField] private string overrideProfileId = "test";
        
        [SerializeField] private string fileName;
        [SerializeField] private bool useEncryption;
        
        [SerializeField] private float autoSaveTimeSeconds = 60f;
        
        private GameData _gameData;
        private List<IDataPersistance> _dataPersistenceObjects;
        private FileDataHandler _dataHandler;

        private string selectedProfileId = "";
        
        private Coroutine _autoSaveCoroutine;
        
        public static DataPersistanceManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one instance of DataPersistanceManager found!");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (disableDataPersistance)
            {
                Debug.LogWarning("Data persistance is disabled!");
            }

            _dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

            InitializeSelectedProfileId();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _dataPersistenceObjects = FindAllDataPersistanceObjects();
            LoadGame();

            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }

            _autoSaveCoroutine = StartCoroutine(AutoSave());
        }

        public void ChangeSelectedProfileId(string newProfileId)
        {
            selectedProfileId = newProfileId;
            LoadGame();
        }

        public void DeleteProfileData(string profileId)
        {
            _dataHandler.Delete(profileId);
            InitializeSelectedProfileId();
            LoadGame();
        }

        private void InitializeSelectedProfileId()
        {
            selectedProfileId = _dataHandler.GetMostRecentlyUpdatedProfileId();
            if (overrideSelectedProfileId)
            {
                selectedProfileId = overrideProfileId;
                Debug.LogWarning("Overrode selected profile id to " + overrideProfileId);
            }
        }

        public void NewGame()
        {
            _gameData = new GameData();
        }

        public void LoadGame()
        {
            if (disableDataPersistance) return;
            
            _gameData = _dataHandler.Load(selectedProfileId);

            if (_gameData == null && initializeDataIfNull)
            {
                NewGame();
            }

            if (_gameData == null)
            {
                Debug.LogWarning("No game data found! You need to create a new game.");
                return;
            }

            foreach (var dataPersistanceObj in _dataPersistenceObjects)
            {
                dataPersistanceObj.LoadData(_gameData);
            }
        }

        public void SaveGame()
        {
            if (disableDataPersistance) return;

            if (_gameData == null)
            {
                Debug.LogWarning("No game data found! You need to create a new game before saving.");
                return;
            }
            
            foreach (var dataPersistanceObj in _dataPersistenceObjects)
            {
                dataPersistanceObj.SaveData(_gameData);
            }
            
            _gameData.lastUpdated = System.DateTime.Now.ToBinary();
            _dataHandler.Save(_gameData, selectedProfileId);
        }

        private void OnApplicationQuit()
        {
            GameManager.Instance.RestartGame();
            SaveGame();
        }
        
        private List<IDataPersistance> FindAllDataPersistanceObjects()
        {
            IEnumerable<IDataPersistance> dataPersistanceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).OfType<IDataPersistance>();
            return new List<IDataPersistance>(dataPersistanceObjects);
        }
        
        public bool HasGameData => _gameData != null;

        public Dictionary<string, GameData> GetAllProfilesGameData() => _dataHandler.LoadAllProfiles();

        private IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveTimeSeconds);
                SaveGame();
                Debug.Log("Auto saved");
            }
        }
    }
}