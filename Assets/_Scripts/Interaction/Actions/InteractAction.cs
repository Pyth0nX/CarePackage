using CarePackage.Main;
using UnityEngine;
using System;

namespace CarePackage.Interaction
{
    public interface InteractAction
    {
        void PerformAction(PlayerState interactingPlayer, GameObject interactingObject);
    }
}