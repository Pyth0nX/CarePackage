using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CarePackage.Persistance
{
    public class FileDataHandler
    {
        private readonly string _dataDirPath = "";
        private readonly string _dataFileName = "";
        private readonly bool _useEncryption = false;
        private const string EncryptionCodeWord = "word"; 
        private const string BackupExtension = ".bak";

        public FileDataHandler(string inDataDirPath, string inDataFileName, bool inUseEncryption)
        {
            _dataDirPath = inDataDirPath;
            _dataFileName = inDataFileName;
            _useEncryption = inUseEncryption;
        }

        public GameData Load(string profileId, bool allowRestoreFromBackup = true)
        {
            if (profileId == null) return null;

            string fullPath = Path.Combine(_dataDirPath, profileId, _dataFileName);
            GameData loadedData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                    
                    if (_useEncryption) dataToLoad = EncryptDecrypt(dataToLoad);
                    
                    loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
                }
                catch (Exception e)
                {
                    if (allowRestoreFromBackup)
                    {
                        Debug.LogWarning("Failed to load data, attempting to restore from backup: " + e.Message);
                        bool rollbackSuccess = AttemptRollback(fullPath);
                        if (rollbackSuccess)
                        {
                            loadedData = Load(profileId, false);
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to load data: " + e.Message);
                    }
                }
            }
            return loadedData;
        }

        public void Save(GameData data, string profileId)
        {
            if (profileId == null) return;

            string fullPath = Path.Combine(_dataDirPath, profileId, _dataFileName);
            string backupFilePath = fullPath + BackupExtension;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                
                string dataToStore = JsonUtility.ToJson(data, true);

                if (_useEncryption) dataToStore = EncryptDecrypt(dataToStore);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }

                GameData verifiedGameData = Load(profileId);
                if (verifiedGameData != null) File.Copy(fullPath, backupFilePath, true);
                else throw new Exception("Failed to verify save data and could not create backup");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save data to file: " + fullPath + "\n" + e);
            }
        }

        public void Delete(string profileId)
        {
            if (profileId == null) return;
            
            string fullPath = Path.Combine(_dataDirPath, profileId, _dataFileName);
            try
            {
                if (File.Exists(fullPath))
                {
                    Directory.Delete(Path.GetDirectoryName(fullPath), true);
                }
                else
                {
                    Debug.LogWarning("Failed to delete data for profile: " + profileId + " because no data was found at path: " + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete data for profile: " + profileId + " at path: " + fullPath + "\n" + e);
            }
        }

        public Dictionary<string, GameData> LoadAllProfiles()
        {
            Dictionary<string, GameData> profileDictionary = new();

            IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(_dataDirPath).EnumerateDirectories();
            foreach (var dirInfo in dirInfos)
            {
                string profileId = dirInfo.Name;
                
                string fullPath = Path.Combine(_dataDirPath, profileId, _dataFileName);
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning("Skipping Directory when loading all profiles because " + profileId + " had no data");
                    continue;
                }
                
                GameData profileData = Load(profileId);
                if (profileData != null) profileDictionary.Add(profileId, profileData);
                else Debug.LogError("Failed to load data for profile: " + profileId);
            }
            return profileDictionary;
        }

        public string GetMostRecentlyUpdatedProfileId()
        {
            string mostRecentUpdatedProfileId = null;
            
            Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
            foreach (var kvp in profilesGameData)
            {
                string profileId = kvp.Key;
                GameData gameData = kvp.Value;
                
                if (gameData == null) continue;

                if (mostRecentUpdatedProfileId == null) mostRecentUpdatedProfileId = profileId;
                else
                {
                    DateTime mostRecentDataTime = DateTime.FromBinary(profilesGameData[mostRecentUpdatedProfileId].lastUpdated);
                    DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                    if (newDateTime > mostRecentDataTime)
                    {
                        mostRecentUpdatedProfileId = profileId;
                    }
                }
            }

            return mostRecentUpdatedProfileId;
        }

        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char) (data[i] ^ EncryptionCodeWord[i % EncryptionCodeWord.Length]);
            }
            return modifiedData;
        }

        private bool AttemptRollback(string fullPath)
        {
            bool success = false;
            string backupFilePath = fullPath + BackupExtension;
            try
            {
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, fullPath, true);
                    success = true;
                    Debug.LogWarning("Rolled back to backup file at: " + backupFilePath);
                }
                else
                {
                    throw new Exception("Failed to rollback because no backup was found");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to roll back to file at: " + backupFilePath + "\n" + e);
            }
            return success;
        }
    }
}