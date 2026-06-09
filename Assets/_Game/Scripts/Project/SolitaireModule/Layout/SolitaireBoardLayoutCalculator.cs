using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class SolitaireBoardLayoutResult
    {
        public Vector2 CardSize;
        public float TableauBottomPlayableY;
        public float WasteStackXOffset;
        public float FaceDownTableauYOffset;
        public float FaceUpTableauYOffset;
        public float MinCompressedFaceUpYOffset;
        public float CardScale;
        public Vector3 StockPosition;
        public Vector3 WastePosition;
        public Vector3[] FoundationPositions;
        public Vector3[] TableauPositions;
    }

    public static partial class SolitaireBoardLayoutCalculator
    {
        public static SolitaireBoardLayoutResult CreateFromConfig(SolitaireDeckConfigSO config) =>
            ResultFactory.FromConfig(config);

        public static bool TryCalculateResponsive(
            Camera camera,
            SolitaireDeckConfigSO config,
            out SolitaireBoardLayoutResult result)
        {
            result = default;

            if (!Validation.CanCalculateResponsive(camera, config))
                return false;

            ViewportBounds bounds = Viewport.Calculate(camera, config);
            result = Orientation.IsLandscape(camera)
                ? Landscape.Calculate(config, bounds)
                : Portrait.Calculate(config, camera.transform.position, bounds);

            return true;
        }

        public static void GetLayoutFrustum(Camera camera, out float halfWidth, out float halfHeight) =>
            Frustum.GetLayoutFrustum(camera, out halfWidth, out halfHeight);

        public static float GetCenteredRowStartX(float centerX, int columnCount, float cardWidth, float gap) =>
            RowLayout.GetCenteredRowStartX(centerX, columnCount, cardWidth, gap);
    }
}
