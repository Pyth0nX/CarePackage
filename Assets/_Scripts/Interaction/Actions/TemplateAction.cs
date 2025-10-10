using System;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    [Serializable]
    public class TemplateAction : InteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            Debug.Log($"{interactingPlayer} interacted with {interactingObject}");
        }
    }
}