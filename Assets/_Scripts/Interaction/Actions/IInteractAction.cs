using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface IInteractAction
    {
        void PerformAction(PlayerState interactingPlayer, GameObject interactingObject);
        bool ConditionMet(PlayerState interactingPlayer, GameObject interactingObject) { return true; }
    }
}