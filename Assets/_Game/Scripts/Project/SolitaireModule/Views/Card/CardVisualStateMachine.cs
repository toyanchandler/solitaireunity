using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class CardVisualStateMachine : MonoBehaviour
    {
        [SerializeField] private CardVisualState currentState = CardVisualState.Inactive;

        public CardVisualState CurrentState => currentState;
        public bool AcceptsInput => currentState != CardVisualState.Locked && currentState != CardVisualState.Moving;

        public void SetState(CardVisualState state)
        {
            currentState = state;
        }
    }
}
