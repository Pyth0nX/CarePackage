using CarePackage.Main;
using UnityEngine;

namespace CarePackage.Interaction
{
    public interface IPickup
    {
        Vector3 Offset { get; }
        GameObject OwningObject { get; set; }
        void OnPickedUp(PlayerState interactingPlayer);
        void OnDropped(PlayerState interactingPlayer);
    }
}