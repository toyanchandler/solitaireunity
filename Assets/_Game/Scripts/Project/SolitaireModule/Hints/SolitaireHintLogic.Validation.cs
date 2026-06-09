using System;
using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal static partial class SolitaireHintLogic
    {
        internal static class InputValidation
        {
            public static void RequireCollectInputs(
                SolitaireBoardState board,
                SolitaireDeckConfigSO config,
                SolitaireHint[] target)
            {
                if (board == null)
                    throw new ArgumentNullException(nameof(board));

                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                if (target == null)
                    throw new ArgumentNullException(nameof(target));
            }
        }

        internal static class CycleIndex
        {
            public static bool TryResolveHintAtIndex(
                SolitaireHint[] hints,
                int hintCount,
                int cycleIndex,
                out SolitaireHint hint)
            {
                if (hintCount <= 0)
                    return Fail(out hint);

                hint = hints[Normalize(cycleIndex, hintCount)];
                return true;
            }

            public static int Normalize(int cycleIndex, int count)
            {
                int index = cycleIndex % count;
                return index < 0 ? index + count : index;
            }

            private static bool Fail(out SolitaireHint hint)
            {
                hint = SolitaireHint.None;
                return false;
            }
        }
    }
}
