using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Managers.Core
{
    public static partial class EventManager
    {
        public static class SolitaireEvents
        {
            public static UnityAction<SolitaireBoardCameraController> BoardCameraReady;
            public static UnityAction<Transform> DragLayerReady;
            public static UnityAction<CardView> CardRegistered;
            public static UnityAction<SolitaireSlotAnchor> SlotRegistered;
            public static UnityAction BoardViewportSizeChanged;
            public static UnityAction<SolitaireModuleControllerBundle> ControllerHostReady;
            public static UnityAction Ready;
            public static UnityAction DealStarted;
            public static UnityAction DealCompleted;
            public static UnityAction<SolitaireMove> MoveCompleted;
            public static UnityAction InvalidMove;
            public static UnityAction<int> CardFlipped;
            public static UnityAction<int> StockDrawn;
            public static UnityAction WasteRecycled;
            public static UnityAction<int, int> FoundationProgressChanged;
            public static UnityAction<int> MoveCountChanged;
            public static UnityAction<bool> UndoAvailabilityChanged;
            public static UnityAction CardHoldStarted;
            public static UnityAction WasteCardClicked;
            public static UnityAction StockDrawClicked;
            public static UnityAction CardDropSucceeded;
            public static UnityAction CardDropFailed;
            public static UnityAction<SolitaireScoreAction> ScoreActionPerformed;
            public static UnityAction<SolitaireHint> HintShown;
            public static UnityAction<int> AutoCompleteCompleted;
            public static UnityAction GameWon;

            public static void Reset()
            {
                BoardCameraReady = null;
                DragLayerReady = null;
                CardRegistered = null;
                SlotRegistered = null;
                BoardViewportSizeChanged = null;
                ControllerHostReady = null;
                Ready = null;
                DealStarted = null;
                DealCompleted = null;
                MoveCompleted = null;
                InvalidMove = null;
                CardFlipped = null;
                StockDrawn = null;
                WasteRecycled = null;
                FoundationProgressChanged = null;
                MoveCountChanged = null;
                UndoAvailabilityChanged = null;
                CardHoldStarted = null;
                WasteCardClicked = null;
                StockDrawClicked = null;
                CardDropSucceeded = null;
                CardDropFailed = null;
                ScoreActionPerformed = null;
                HintShown = null;
                AutoCompleteCompleted = null;
                GameWon = null;
            }
        }
    }
}
