using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static class SolitaireDebugScenarioApplier
    {
        public static readonly SolitaireDebugScenarioId[] OrderedScenarios =
        {
            SolitaireDebugScenarioId.ValidFourCardMerge,
            SolitaireDebugScenarioId.RejectSameColorJunction,
            SolitaireDebugScenarioId.ValidKingSequenceToEmpty,
            SolitaireDebugScenarioId.RejectInvalidInternalSequence,
            SolitaireDebugScenarioId.PartialSequenceTwoCards,
            SolitaireDebugScenarioId.ValidTwoCardMerge,
            SolitaireDebugScenarioId.EndGameSuccess,
            SolitaireDebugScenarioId.EndGameFail,
            SolitaireDebugScenarioId.Restart
        };

        public static bool IsFlowScenario(SolitaireDebugScenarioId scenario) =>
            scenario is SolitaireDebugScenarioId.EndGameSuccess
                or SolitaireDebugScenarioId.EndGameFail
                or SolitaireDebugScenarioId.Restart;

        public static string GetButtonLabel(SolitaireDebugScenarioId scenario) =>
            scenario switch
            {
                SolitaireDebugScenarioId.EndGameSuccess => "Endgame Success",
                SolitaireDebugScenarioId.EndGameFail => "Endgame Fail",
                SolitaireDebugScenarioId.Restart => "Restart",
                _ => $"Senaryo {Array.IndexOf(OrderedScenarios, scenario) + 1}"
            };

        public static void ApplyFlowScenario(SolitaireDebugScenarioId scenario)
        {
            switch (scenario)
            {
                case SolitaireDebugScenarioId.EndGameSuccess:
                    EventManager.InGameEvents.LevelSuccess?.Invoke();
                    break;
                case SolitaireDebugScenarioId.EndGameFail:
                    EventManager.InGameEvents.LevelFail?.Invoke();
                    break;
                case SolitaireDebugScenarioId.Restart:
                    EventManager.InGameEvents.LoadLevel?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Not a flow debug scenario.");
            }
        }
        private readonly struct CardPlacement
        {
            public readonly CardSuit Suit;
            public readonly CardRank Rank;
            public readonly bool IsFaceUp;

            public CardPlacement(CardSuit suit, CardRank rank, bool isFaceUp = true)
            {
                Suit = suit;
                Rank = rank;
                IsFaceUp = isFaceUp;
            }
        }

        public static string GetInstructions(SolitaireDebugScenarioId scenario)
        {
            return scenario switch
            {
                SolitaireDebugScenarioId.ValidFourCardMerge =>
                    "T3 (4. sütun): ♣9'dan tut → T0 (1. sütun) ♥10 üstüne bırak.\nBeklenen: 4 kart taşınır (♣9♦8♠7♥6).",
                SolitaireDebugScenarioId.RejectSameColorJunction =>
                    "T3: ♦9'dan tut → T0 ♥10 üstüne bırak.\nBeklenen: Reddedilir (♦9 + ♥10 ikisi kırmızı).",
                SolitaireDebugScenarioId.ValidKingSequenceToEmpty =>
                    "T5 (6. sütun): ♠K'dan tut → boş T4 (5. sütun) üstüne bırak.\nBeklenen: 4 kart taşınır (♠K♥Q♦J♣10).",
                SolitaireDebugScenarioId.RejectInvalidInternalSequence =>
                    "T2 (3. sütun): ♣9'a tıkla/sürüklemeye çalış.\nBeklenen: Drag başlamaz (♣9♠8 siyah-siyah, seri geçersiz).",
                SolitaireDebugScenarioId.PartialSequenceTwoCards =>
                    "T1 (2. sütun): ♠7'den tut → T0 ♠K üstüne bırak.\nBeklenen: Sadece 2 kart sürüklenir (♠7♥6), drop reddedilir (siyah-siyah).",
                SolitaireDebugScenarioId.ValidTwoCardMerge =>
                    "T3: ♣9'dan tut → T0 ♥10 üstüne bırak.\nBeklenen: 2 kart taşınır (♣9♦8).",
                SolitaireDebugScenarioId.EndGameSuccess =>
                    "Success endgame ekranını açar (LevelSuccess event).",
                SolitaireDebugScenarioId.EndGameFail =>
                    "Fail endgame ekranını açar (LevelFail event).",
                SolitaireDebugScenarioId.Restart =>
                    "Level'ı yeniden yükler (LoadLevel event, RetryLevelButton ile aynı).",
                _ => "Senaryo seçilmedi."
            };
        }

        public static void Apply(SolitaireBoardState board, SolitaireDebugScenarioId scenario)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (scenario == SolitaireDebugScenarioId.None)
                throw new InvalidOperationException("Debug scenario is None.");

            if (IsFlowScenario(scenario))
                throw new InvalidOperationException($"Flow scenario '{scenario}' cannot be applied to board state.");

            board.ClearForDebugSetup();
            board.InitializeCardsForDebugSetup();

            switch (scenario)
            {
                case SolitaireDebugScenarioId.ValidFourCardMerge:
                    SetTableau(board, 0, H10);
                    SetTableau(board, 3, C9, D8, S7, H6);
                    break;

                case SolitaireDebugScenarioId.RejectSameColorJunction:
                    SetTableau(board, 0, H10);
                    SetTableau(board, 3, D9, C8, S7);
                    break;

                case SolitaireDebugScenarioId.ValidKingSequenceToEmpty:
                    SetTableau(board, 5, SK, HQ, DJ, C10);
                    break;

                case SolitaireDebugScenarioId.RejectInvalidInternalSequence:
                    SetTableau(board, 2, C9, S8);
                    break;

                case SolitaireDebugScenarioId.PartialSequenceTwoCards:
                    SetTableau(board, 0, SK);
                    SetTableau(board, 1, C9, D8, S7, H6);
                    break;

                case SolitaireDebugScenarioId.ValidTwoCardMerge:
                    SetTableau(board, 0, H10);
                    SetTableau(board, 3, C9, D8);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown debug scenario.");
            }

            board.ParkUnusedCardsInStock();
        }

        private static readonly CardPlacement H10 = new CardPlacement(CardSuit.Hearts, CardRank.Ten);
        private static readonly CardPlacement C9 = new CardPlacement(CardSuit.Clubs, CardRank.Nine);
        private static readonly CardPlacement D8 = new CardPlacement(CardSuit.Diamonds, CardRank.Eight);
        private static readonly CardPlacement S7 = new CardPlacement(CardSuit.Spades, CardRank.Seven);
        private static readonly CardPlacement H6 = new CardPlacement(CardSuit.Hearts, CardRank.Six);
        private static readonly CardPlacement D9 = new CardPlacement(CardSuit.Diamonds, CardRank.Nine);
        private static readonly CardPlacement C8 = new CardPlacement(CardSuit.Clubs, CardRank.Eight);
        private static readonly CardPlacement SK = new CardPlacement(CardSuit.Spades, CardRank.King);
        private static readonly CardPlacement HQ = new CardPlacement(CardSuit.Hearts, CardRank.Queen);
        private static readonly CardPlacement DJ = new CardPlacement(CardSuit.Diamonds, CardRank.Jack);
        private static readonly CardPlacement C10 = new CardPlacement(CardSuit.Clubs, CardRank.Ten);
        private static readonly CardPlacement S8 = new CardPlacement(CardSuit.Spades, CardRank.Eight);

        private static void SetTableau(SolitaireBoardState board, int columnIndex, params CardPlacement[] cards)
        {
            var target = new PileRef(SolitairePileType.Tableau, columnIndex);

            for (int i = 0; i < cards.Length; i++)
            {
                CardPlacement placement = cards[i];
                int cardId = SolitaireCardUtility.GetCardId(placement.Suit, placement.Rank);
                board.AddCardToPile(cardId, target, placement.IsFaceUp);
            }
        }
    }
}
