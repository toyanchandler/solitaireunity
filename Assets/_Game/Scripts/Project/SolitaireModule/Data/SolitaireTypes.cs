using System;

namespace _Game.Scripts.Project.SolitaireModule.Data
{
    public enum CardSuit : byte
    {
        Hearts = 0,
        Diamonds = 1,
        Clubs = 2,
        Spades = 3
    }

    public enum CardColor : byte
    {
        Red = 0,
        Black = 1
    }

    public enum CardRank : byte
    {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13
    }

    public enum SolitairePileType : byte
    {
        Stock = 0,
        Waste = 1,
        Foundation = 2,
        Tableau = 3
    }

    public enum SolitaireDrawMode : byte
    {
        DrawOne = 1,
        DrawThree = 3
    }

    public enum SolitaireMoveType : byte
    {
        None = 0,
        StockToWaste = 1,
        WasteRecycleToStock = 2,
        WasteToTableau = 3,
        WasteToFoundation = 4,
        TableauToTableau = 5,
        TableauToFoundation = 6,
        FoundationToTableau = 7,
        FlipTableauTop = 8,
        AutoMoveToFoundation = 9
    }

    public enum SolitaireScoreAction : byte
    {
        MoveToTableau = 0,
        MoveToFoundation = 1,
        RevealTableauCard = 2,
        Undo = 3,
        StockDraw = 4,
        StockRecycle = 5
    }

    public enum SolitaireHintKind : byte
    {
        None = 0,
        MoveToFoundation = 1,
        RevealTableauByMove = 2,
        WasteToTableau = 3,
        TableauToTableau = 4,
        StockAction = 5
    }

    public enum CardVisualState : byte
    {
        Inactive = 0,
        InStock = 1,
        FaceDown = 2,
        FaceUpIdle = 3,
        Selected = 4,
        Dragging = 5,
        Moving = 6,
        Returning = 7,
        Locked = 8
    }

    [Serializable]
    public struct CardState
    {
        public int Id;
        public CardSuit Suit;
        public CardRank Rank;
        public bool IsFaceUp;
        public SolitairePileType CurrentPileType;
        public int CurrentPileIndex;
        public int IndexInPile;

        public CardColor Color =>
            Suit == CardSuit.Hearts || Suit == CardSuit.Diamonds
                ? CardColor.Red
                : CardColor.Black;
    }

    public readonly struct PileRef
    {
        public readonly SolitairePileType Type;
        public readonly int Index;

        public PileRef(SolitairePileType type, int index)
        {
            Type = type;
            Index = index;
        }

        public bool IsValid => Index >= 0;

        public static PileRef Invalid => new PileRef(SolitairePileType.Stock, -1);
    }

    public readonly struct SolitaireMove
    {
        public readonly SolitaireMoveType Type;
        public readonly int StartCardId;
        public readonly PileRef Source;
        public readonly PileRef Target;

        public SolitaireMove(SolitaireMoveType type, int startCardId, PileRef source, PileRef target)
        {
            Type = type;
            StartCardId = startCardId;
            Source = source;
            Target = target;
        }
    }

    public readonly struct SolitaireHint
    {
        public readonly SolitaireHintKind Kind;
        public readonly SolitaireMove Move;

        public SolitaireHint(SolitaireHintKind kind, SolitaireMove move)
        {
            Kind = kind;
            Move = move;
        }

        public bool IsValid => Kind != SolitaireHintKind.None && Move.Type != SolitaireMoveType.None;

        public static SolitaireHint None => new SolitaireHint(SolitaireHintKind.None, default);
    }

    public readonly struct SolitaireMoveResult
    {
        public readonly bool IsAccepted;
        public readonly int RevealedCardId;
        public readonly string Reason;

        public SolitaireMoveResult(bool isAccepted, int revealedCardId, string reason)
        {
            IsAccepted = isAccepted;
            RevealedCardId = revealedCardId;
            Reason = reason;
        }

        public static SolitaireMoveResult Accepted(int revealedCardId = -1)
        {
            return new SolitaireMoveResult(true, revealedCardId, string.Empty);
        }

        public static SolitaireMoveResult Rejected(string reason)
        {
            return new SolitaireMoveResult(false, -1, reason);
        }
    }
}
