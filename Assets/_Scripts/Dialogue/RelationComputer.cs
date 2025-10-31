using UnityEngine;
using TMPro;

public class RelationComputer : MonoBehaviour
{
    [SerializeField] private TMP_Text relationshipText;

    void Update()
    {
        if (DialogueManager.Instance != null)
        {
            float relationship = DialogueManager.Instance.GetYarnFloat("$relationshipFamA");
            relationshipText.text = $"Your relationship with Family A: {relationship:0.0}";
        }
    }
}