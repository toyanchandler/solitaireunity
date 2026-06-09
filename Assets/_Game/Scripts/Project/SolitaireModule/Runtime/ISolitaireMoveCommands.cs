using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public interface ISolitaireMoveCommands
    {
        bool TryDrawOrRecycleStock();
        bool TryMoveCardToSlot(int cardId, PileRef target);
        void ReturnCardToCurrentPile(int cardId);
        void NotifyInvalidMove();
        bool TryAutoMoveToFoundation(int cardId);
        bool TryFlipTableauTop(int cardId);
    }
}
