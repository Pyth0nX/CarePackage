using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xasu.HighLevel;
using Yarn.Unity;

public class DialogueVisualsController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private List<BackgroundData> backgrounds = new List<BackgroundData>();


    [Header("Characters")]
    [SerializeField] private List<CharacterPortrait> characters = new List<CharacterPortrait>();

    [Header("Dialogue Canvas Control")]
    //[SerializeField] private Canvas dialogueCanvas;
    //[SerializeField] private DialogueRunner dialogueRunner;

    private static Dictionary<string, Sprite> backgroundLookup = new Dictionary<string, Sprite>();
    private static Dictionary<string, CharacterPortrait> characterLookup = new Dictionary<string, CharacterPortrait>();

    void Awake()
    {
        // Fill dictionaries
        backgroundLookup.Clear();
        characterLookup.Clear();

        foreach (var bg in backgrounds)
            backgroundLookup[bg.name] = bg.sprite;

        foreach (var c in characters)
            characterLookup[c.characterName] = c;

        // Hide everything at start
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(false);

        foreach (var c in characters)
            if (c.image != null)
                c.image.gameObject.SetActive(false);
/*
        if (dialogueCanvas != null)
            dialogueCanvas.enabled = false;*/
    }

    void Start()
    {/*
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            dialogueRunner.onNodeStart.AddListener((node) => CompletableTracker.Instance.Initialized("Node_" + node, CompletableTracker.CompletableType.DialogNode));
            dialogueRunner.onNodeComplete.AddListener((node) => CompletableTracker.Instance.Completed("Node_" + node, CompletableTracker.CompletableType.DialogNode));
            
        }
        else
        {
            Debug.LogWarning("DialogueRunner not assigned in DialogueVisualsController.");
        }*/
    }
/*
    private void OnDialogueStart()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.enabled = true;
        
        CompletableTracker.Instance.Initialized("Dialogue_" + dialogueRunner.Dialogue, CompletableTracker.CompletableType.DialogFragment);
    }

    private void OnDialogueComplete()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.enabled = false;
        
        CompletableTracker.Instance.Completed("Dialogue_" + dialogueRunner.Dialogue, CompletableTracker.CompletableType.DialogFragment);
    }*/

    // --- Background ---
    [YarnCommand("setBackground")]
    public static void SetBackground(string backgroundName)
    {
        Debug.Log($"Yarn command setBackground called with '{backgroundName}'");

        var controller = Object.FindFirstObjectByType<DialogueVisualsController>();
        if (controller == null)
        {
            Debug.LogError("No DialogueVisualsController found in scene!");
            return;
        }

        if (backgroundLookup.TryGetValue(backgroundName, out Sprite sprite))
        {
            controller.backgroundImage.sprite = sprite;
            controller.backgroundImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Background '{backgroundName}' not found in the list.");
        }
    }

    // --- Show character ---
    [YarnCommand("showCharacter")]
    public static void ShowCharacter(string characterName, string emotion)
    {
        Debug.Log($"Yarn command showCharacter called for '{characterName}' with emotion '{emotion}'");

        var controller = Object.FindFirstObjectByType<DialogueVisualsController>();
        if (controller == null)
        {
            Debug.LogError("No DialogueVisualsController found in scene!");
            return;
        }

        if (!characterLookup.TryGetValue(characterName, out CharacterPortrait portrait))
        {
            Debug.LogWarning($"Character '{characterName}' not found.");
            return;
        }

        Sprite sprite = portrait.GetEmotionSprite(emotion);
        if (sprite != null)
        {
            portrait.image.gameObject.SetActive(true);
            portrait.image.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Emotion '{emotion}' not found for character '{characterName}'.");
        }
    }

    // --- Hide character ---
    [YarnCommand("hideCharacter")]
    public static void HideCharacter(string characterName)
    {
        Debug.Log($"Yarn command hideCharacter called for '{characterName}'");

        var controller = Object.FindFirstObjectByType<DialogueVisualsController>();
        if (controller == null)
        {
            Debug.LogError("No DialogueVisualsController found in scene!");
            return;
        }

        if (characterLookup.TryGetValue(characterName, out CharacterPortrait portrait))
        {
            portrait.image.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Character '{characterName}' not found when trying to hide it.");
        }
    }

    [YarnCommand("hideBackground")]
    public static IEnumerator HideBackground()
    {
        var controller = Object.FindFirstObjectByType<DialogueVisualsController>();
        if (controller == null)
        {
            Debug.LogError("No DialogueVisualsController found in scene!");
            yield return null;
            yield break;
        }

        if (controller.backgroundImage == null)
        {
            Debug.LogError("Background Image not assigned in DialogueVisualsController!");
            yield return null;
            yield break;
        }

        controller.backgroundImage.gameObject.SetActive(false);
        Debug.Log("Background hidden successfully.");

        // Wait one frame to signal completion
        yield return null;
        yield break;
    }
}

// --- Helper data classes ---
[System.Serializable]
public class BackgroundData
{
    public string name;
    public Sprite sprite;
}

[System.Serializable]
public class CharacterPortrait
{
    public string characterName;
    public Image image;
    public EmotionSprite[] emotions;


    private Dictionary<string, Sprite> emotionLookup;

    public Sprite GetEmotionSprite(string emotion)
    {
        if (emotionLookup == null)
        {
            emotionLookup = new Dictionary<string, Sprite>();
            foreach (var e in emotions)
                emotionLookup[e.emotionName] = e.sprite;
        }

        if (emotionLookup.TryGetValue(emotion, out Sprite sprite))
            return sprite;

        return null;
    }
}

[System.Serializable]
public class EmotionSprite
{
    public string emotionName;
    public Sprite sprite;
}