using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireRuntimeLayoutMetrics
    {
        public Vector2 CardSize { get; private set; }
        public float TableauBottomPlayableY { get; private set; }
        public float WasteStackXOffset { get; private set; }
        public float FaceDownTableauYOffset { get; private set; }
        public float FaceUpTableauYOffset { get; private set; }
        public float MinCompressedFaceUpYOffset { get; private set; }
        public float CardScale { get; private set; } = 1f;

        public void Apply(SolitaireBoardLayoutResult layoutResult)
        {
            CardSize = layoutResult.CardSize;
            TableauBottomPlayableY = layoutResult.TableauBottomPlayableY;
            WasteStackXOffset = layoutResult.WasteStackXOffset;
            FaceDownTableauYOffset = layoutResult.FaceDownTableauYOffset;
            FaceUpTableauYOffset = layoutResult.FaceUpTableauYOffset;
            MinCompressedFaceUpYOffset = layoutResult.MinCompressedFaceUpYOffset;
            CardScale = layoutResult.CardScale;
        }

        public void ResetToConfig(SolitaireDeckConfigSO config)
        {
            if (config == null)
                return;

            CardSize = config.CardSize;
            TableauBottomPlayableY = config.TableauBottomPlayableY;
            WasteStackXOffset = config.WasteStackXOffset;
            FaceDownTableauYOffset = config.FaceDownTableauYOffset;
            FaceUpTableauYOffset = config.FaceUpTableauYOffset;
            MinCompressedFaceUpYOffset = config.MinCompressedFaceUpYOffset;
            CardScale = 1f;
        }
    }
}
