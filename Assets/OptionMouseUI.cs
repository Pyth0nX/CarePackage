using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class OptionMouseUI : DialogueViewBase
{
    [Header("Referencias")]
    public GameObject buttonPrefab;
    public Transform buttonParent;

    // Lista interna para controlar los botones
    private List<GameObject> currentOptionButtons = new List<GameObject>();

    public void Start()
    {
        // Limpiamos el contenedor al arrancar por si acaso
        if (buttonParent != null)
        {
            foreach (Transform child in buttonParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public override void RunOptions(DialogueOption[] options, Action<int> onOptionSelected)
    {
        // 1. Borramos botones viejos
        ClearButtons();

        // 2. Creamos uno nuevo por cada opción
        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            int optionIndex = option.DialogueOptionID;

            // Crear botón
            GameObject newButton = Instantiate(buttonPrefab, buttonParent);

            // Poner texto (Yarn 3)
            var textComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = option.Line.TextWithoutCharacterName.ToString();
            }

            // Asignar clic
            var btnComponent = newButton.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.AddListener(() =>
                {
                    onOptionSelected(optionIndex);
                    ClearButtons();
                });
            }

            currentOptionButtons.Add(newButton);
        }
    }

    public override void DialogueComplete()
    {
        ClearButtons();
    }

    private void ClearButtons()
    {
        foreach (var btn in currentOptionButtons)
        {
            if (btn != null) Destroy(btn);
        }
        currentOptionButtons.Clear();
    }
}