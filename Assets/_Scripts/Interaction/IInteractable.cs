using CarePackage.Main;

namespace CarePackage.Interaction
{
    public interface IInteractable
    {
        void Interact(PlayerState interactingPlayer);
        InteractionType Type { get; }
    }
}