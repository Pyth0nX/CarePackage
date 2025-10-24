using UnityEngine;
using Yarn.Unity;

public class DialogueManager : MonoBehaviour
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
        DontDestroyOnLoad(gameObject);

        if (dialogueRunner == null)
            dialogueRunner = GetComponentInChildren<DialogueRunner>();

        if (variableStorage == null && dialogueRunner != null)
            variableStorage = dialogueRunner.VariableStorage as InMemoryVariableStorage;
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
}