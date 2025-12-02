using UnityEngine;

namespace CarePackage.Main
{
    public class EndingSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private GameObject restartButton;
        [SerializeField] private string badEnding, neutralEnding, goodEnding;
        
        private Yarn.Unity.DialogueRunner _dialogueRunner;
        
        private void Start()
        {
            if (DialogueManager.Instance.dialogueRunner == null) return;
            _dialogueRunner = DialogueManager.Instance.dialogueRunner;
            OnEnter();
        }
        
        public void OnEnter()
        {
            if (_dialogueRunner == null) return;
            _dialogueRunner.onDialogueComplete?.AddListener(OnDialogueComplete_Implementation);
            
            DialogueManager.Instance.SetYarnString("$ending", "Bad Ending");
            
            if (!GameManager.Instance.Survived)
            {
                _dialogueRunner.StartDialogue(badEnding);
                return;
            }
/*
            var score = DialogueManager.Instance.GetYarnFloat("$relationshipFamA");
            if (score < -1.5)
            {
                _dialogueRunner.StartDialogue(badEnding);
            }
            else if (score >= -1.5 && score < 1)
            {
                DialogueManager.Instance.SetYarnString("$ending", "Neutral Ending");
                _dialogueRunner.StartDialogue(neutralEnding);
            }
            else if (score >= 1)
            {
                DialogueManager.Instance.SetYarnString("$ending", "Good Ending");
                _dialogueRunner.StartDialogue(goodEnding);
            }*/
        }

        public void OnExit()
        {
            if (_dialogueRunner == null) return;
            _dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueComplete_Implementation);
        }

        private void OnDialogueComplete_Implementation()
        {
            restartButton.SetActive(true);
        }

        private void OnDisable()
        {
            OnExit();
        }
    }
}