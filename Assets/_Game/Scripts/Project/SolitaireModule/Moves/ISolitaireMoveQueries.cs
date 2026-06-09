using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public interface ISolitaireMoveQueries
    {
        bool CanMoveCardToSlot(int cardId, PileRef target);
        bool CanStartDrag(int cardId);
        bool CanCardReceiveInput(int cardId);
    }
}
