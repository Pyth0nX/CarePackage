using CarePackage.Main;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CarePackage.Interaction
{
    [Serializable]
    public class DialogActions : InteractAction
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button optionAButton;
        [SerializeField] private Button optionBButton;
        [SerializeField] private TextMeshProUGUI optionAText;
        [SerializeField] private TextMeshProUGUI optionBText;

        [Header("Dialogue Content")]
        [TextArea][SerializeField] private string[] dialogueLines;
        [SerializeField] private string optionATextValue;
        [SerializeField] private string optionBTextValue;

        private int currentLine = 0;

        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            if (dialoguePanel == null || dialogueText == null)
            {
                Debug.LogError("[DialogueAction] UI elements not set!");
                return;
            }

            // Active panel and 1st text
            dialoguePanel.SetActive(true);
            currentLine = 0;
            ShowLine();

            // Deactivate buttons
            optionAButton.gameObject.SetActive(false);
            optionBButton.gameObject.SetActive(false);
        }

        private void ShowLine()
        {
            if (currentLine < dialogueLines.Length)
            {
                dialogueText.text = dialogueLines[currentLine];
                currentLine++;

                // Avanzar con clic o tecla
                dialoguePanel.GetComponent<Button>()?.onClick.RemoveAllListeners();
                dialoguePanel.GetComponent<Button>()?.onClick.AddListener(() => ShowLine());
            }
            else
            {
                
                ShowOptions();
            }
        }

        private void ShowOptions()
        {
            dialogueText.text = "¿Qué quieres hacer?";
            optionAButton.gameObject.SetActive(true);
            optionBButton.gameObject.SetActive(true);

            optionAText.text = optionATextValue;
            optionBText.text = optionBTextValue;

            optionAButton.onClick.RemoveAllListeners();
            optionBButton.onClick.RemoveAllListeners();

            optionAButton.onClick.AddListener(() => ChooseOption("A"));
            optionBButton.onClick.AddListener(() => ChooseOption("B"));
        }

        private void ChooseOption(string option)
        {
            dialogueText.text = option == "A"
                ? "Has elegido la opción A."
                : "Has elegido la opción B.";

            optionAButton.gameObject.SetActive(false);
            optionBButton.gameObject.SetActive(false);

            // close the panel
            dialoguePanel.SetActive(false);
        }
    }
}