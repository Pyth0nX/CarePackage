namespace CarePackage.Interaction
{
    public interface IInteractable
    {
        void Interact();
        EInteractionType Type { get; }
        IInteractionActivationType ActivationType { get; }
        string InteractMessage { get; }
        bool ShowMessage { get; }
        void OnHovered(bool toggle);
    }
}