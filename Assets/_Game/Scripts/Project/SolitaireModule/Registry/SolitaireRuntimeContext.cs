namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireRuntimeContext
    {
        public SolitaireRuntimeContext(SolitaireBoardState boardState, SolitaireViewRegistry viewRegistry)
        {
            BoardState = boardState;
            ViewRegistry = viewRegistry;
            MoveHistory = new SolitaireMoveHistory();
            SelectionState = new SolitaireSelectionState();
            LayoutMetrics = new SolitaireRuntimeLayoutMetrics();
        }

        public SolitaireBoardState BoardState { get; }
        public SolitaireViewRegistry ViewRegistry { get; }
        public SolitaireMoveHistory MoveHistory { get; }
        public SolitaireSelectionState SelectionState { get; }
        public SolitaireRuntimeLayoutMetrics LayoutMetrics { get; }
        public bool IsAnimationLocked { get; private set; }
        public bool IsDragging { get; private set; }

        public void BeginAnimationLock()
        {
            IsAnimationLocked = true;
        }

        public void EndAnimationLock()
        {
            IsAnimationLocked = false;
        }

        public void BeginDrag()
        {
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;
        }
    }

    public sealed class SolitaireMoveHistory
    {
        private readonly System.Collections.Generic.Stack<SolitaireBoardSnapshot> _snapshots =
            new System.Collections.Generic.Stack<SolitaireBoardSnapshot>();

        public int Count => _snapshots.Count;
        public bool CanUndo => _snapshots.Count > 0;

        public void Push(SolitaireBoardSnapshot snapshot)
        {
            if (snapshot != null)
                _snapshots.Push(snapshot);
        }

        public bool TryUndo(SolitaireBoardState boardState)
        {
            if (_snapshots.Count == 0)
                return false;

            boardState.RestoreSnapshot(_snapshots.Pop());
            return true;
        }

        public void Clear()
        {
            _snapshots.Clear();
        }
    }

    public sealed class SolitaireSelectionState
    {
        public int SelectedCardId { get; private set; } = -1;
        public bool HasSelection => SelectedCardId >= 0;

        public void Select(int cardId)
        {
            SelectedCardId = cardId;
        }

        public void Clear()
        {
            SelectedCardId = -1;
        }
    }
}
