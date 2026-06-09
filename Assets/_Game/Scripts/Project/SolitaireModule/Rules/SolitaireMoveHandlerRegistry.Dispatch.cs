using System;
using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static partial class SolitaireMoveHandlerRegistry
    {
        internal static class Dispatch
        {
            public static bool TryGetHandler<THandler>(THandler[] handlers, SolitaireMoveType moveType, out THandler handler)
                where THandler : class
            {
                int index = (int)moveType;

                if ((uint)index >= (uint)handlers.Length)
                {
                    handler = null;
                    return false;
                }

                handler = handlers[index];
                return handler != null;
            }

            public static int GetMoveTypeSlotCount()
            {
                int max = 0;
                Array values = Enum.GetValues(typeof(SolitaireMoveType));

                for (int i = 0; i < values.Length; i++)
                    max = Math.Max(max, (int)(SolitaireMoveType)values.GetValue(i));

                return max + 1;
            }
        }

        internal static class HandlerTables
        {
            public static SolitaireMoveValidationHandler[] CreateValidationHandlers()
            {
                var handlers = new SolitaireMoveValidationHandler[Dispatch.GetMoveTypeSlotCount()];
                handlers[(int)SolitaireMoveType.StockToWaste] = Validation.Stock.Validate;
                handlers[(int)SolitaireMoveType.WasteRecycleToStock] = Validation.WasteRecycle.Validate;
                handlers[(int)SolitaireMoveType.FlipTableauTop] = Validation.FlipTableau.Validate;
                handlers[(int)SolitaireMoveType.WasteToTableau] = Validation.CardTransfer.Validate;
                handlers[(int)SolitaireMoveType.WasteToFoundation] = Validation.CardTransfer.Validate;
                handlers[(int)SolitaireMoveType.TableauToTableau] = Validation.CardTransfer.Validate;
                handlers[(int)SolitaireMoveType.TableauToFoundation] = Validation.CardTransfer.Validate;
                handlers[(int)SolitaireMoveType.FoundationToTableau] = Validation.CardTransfer.Validate;
                handlers[(int)SolitaireMoveType.AutoMoveToFoundation] = Validation.CardTransfer.Validate;
                return handlers;
            }

            public static SolitaireMoveExecutionHandler[] CreateExecutionHandlers()
            {
                var handlers = new SolitaireMoveExecutionHandler[Dispatch.GetMoveTypeSlotCount()];
                handlers[(int)SolitaireMoveType.StockToWaste] = Execution.Stock.Execute;
                handlers[(int)SolitaireMoveType.WasteRecycleToStock] = Execution.WasteRecycle.Execute;
                handlers[(int)SolitaireMoveType.FlipTableauTop] = Execution.FlipTableau.Execute;
                handlers[(int)SolitaireMoveType.WasteToTableau] = Execution.CardTransfer.Execute;
                handlers[(int)SolitaireMoveType.WasteToFoundation] = Execution.CardTransfer.Execute;
                handlers[(int)SolitaireMoveType.TableauToTableau] = Execution.CardTransfer.Execute;
                handlers[(int)SolitaireMoveType.TableauToFoundation] = Execution.CardTransfer.Execute;
                handlers[(int)SolitaireMoveType.FoundationToTableau] = Execution.CardTransfer.Execute;
                handlers[(int)SolitaireMoveType.AutoMoveToFoundation] = Execution.CardTransfer.Execute;
                return handlers;
            }
        }
    }
}
