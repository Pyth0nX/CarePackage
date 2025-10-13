using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface InteractAction
    {
        void PerformAction(PlayerState interactingPlayer, GameObject interactingObject);
    }
}