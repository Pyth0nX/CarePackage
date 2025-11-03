using System;
using CarePackage.Main;
using CarePackage.Persistance;
using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour, IDataPersistance
{
    public static DialogueManager Instance { get; private set; }

    [Header("Yarn References")]
    public DialogueRunner dialogueRunner;
    public InMemoryVariableStorage variableStorage;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        if (dialogueRunner == null)
            dialogueRunner = GetComponentInChildren<DialogueRunner>();

        if (variableStorage == null && dialogueRunner != null)
            variableStorage = dialogueRunner.VariableStorage as InMemoryVariableStorage;
    }

    private void Update()
    {
        Debug.Log("Day: " + GetYarnFloat("$day"));
    }

    private void OnEnable()
    {
        Invoke("Enable", .01f);
    }

    private void Enable()
    {
        GameManager.Instance.onGameRestart += OnGameRestart_Implementation;
    }

    private void OnDisable()
    {
        GameManager.Instance.onGameRestart -= OnGameRestart_Implementation;
    }
    
    private void OnGameRestart_Implementation()
    {
        SetYarnFloat("$relationshipFamA", 0f);
    }

    public float GetYarnFloat(string varName)
    {
        if (variableStorage == null) return 0f;

        object value;
        if (variableStorage.TryGetValue(varName, out value))
        {
            if (value is float f) return f;
            if (float.TryParse(value.ToString(), out float parsed)) return parsed;
        }
        return 0f;
    }

    public bool GetYarnBool(string varName)
    {
        if (variableStorage == null) return false;

        object value;
        if (variableStorage.TryGetValue(varName, out value))
        {
            if (value is bool b) return b;
            if (bool.TryParse(value.ToString(), out bool parsed)) return parsed;
        }
        return false;
    }

    public string GetYarnString(string varName)
    {
        if (variableStorage == null) return string.Empty;

        object value;
        if (variableStorage.TryGetValue(varName, out value))
        {
            return value.ToString();
        }
        return string.Empty;
    }

    public void SetYarnFloat(string varName, float value)
    {
        if (variableStorage == null) return;
        variableStorage.SetValue(varName, value);
    }

    public void SetYarnBool(string varName, bool value)
    {
        if (variableStorage == null) return;
        variableStorage.SetValue(varName, value);
    }

    public void SetYarnString(string varName, string value)
    {
        if (variableStorage == null) return;
        variableStorage.SetValue(varName, value);
    }

    public void LoadData(GameData loadData)
    {
        if (variableStorage == null) return;
        SetYarnFloat("$relationshipFamA", loadData.famARelationship);
    }

    public void SaveData(GameData saveData)
    {
        if (variableStorage == null) return;
        saveData.famARelationship = GetYarnFloat("$relationshipFamA");
    }
}