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
            public static UnityAction DealStarted;
            public static UnityAction CardHoldStarted;
            public static UnityAction WasteCardClicked;
            public static UnityAction StockDrawClicked;
            public static UnityAction CardDropSucceeded;
            public static UnityAction CardDropFailed;
            public static UnityAction<SolitaireScoreAction> ScoreActionPerformed;
            public static UnityAction<SolitaireHint> HintShown;
            public static UnityAction<int> AutoCompleteCompleted;

            public static void Reset()
            {
                BoardCameraReady = null;
                DragLayerReady = null;
                CardRegistered = null;
                SlotRegistered = null;
                BoardViewportSizeChanged = null;
                ControllerHostReady = null;
                DealStarted = null;
                CardHoldStarted = null;
                WasteCardClicked = null;
                StockDrawClicked = null;
                CardDropSucceeded = null;
                CardDropFailed = null;
                ScoreActionPerformed = null;
                HintShown = null;
                AutoCompleteCompleted = null;
            }
        }
    }
}
