using System;
using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    [Serializable]
    public class TemplateAction : IInteractAction
    {
        public void PerformAction(PlayerState interactingPlayer, GameObject interactingObject)
        {
            Debug.Log($"{interactingPlayer} interacted with {interactingObject}");
        }
    }
}