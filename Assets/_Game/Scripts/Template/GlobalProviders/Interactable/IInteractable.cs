namespace _Game.Scripts.Template.GlobalProviders.Interactable
{
    public interface IInteractable
    {
        void Interact();
    
        bool CanInteract { get; }
    }
}