using _Game.Scripts.Project.SolitaireModule.Data;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal static class Validation
        {
            public static bool CanCalculateResponsive(Camera camera, SolitaireDeckConfigSO config) =>
                camera != null && camera.orthographic && config != null;
        }

        internal static class Frustum
        {
            public static void GetLayoutFrustum(Camera camera, out float halfWidth, out float halfHeight)
            {
                halfHeight = camera.orthographicSize;
                float aspect = Orientation.GetAspectRatio(camera);
                halfWidth = halfHeight * aspect;
            }
        }

        internal static class Orientation
        {
            public static bool IsLandscape(Camera camera) =>
                GetPixelWidth(camera) > GetPixelHeight(camera);

            public static float GetAspectRatio(Camera camera) =>
                (float)GetPixelWidth(camera) / GetPixelHeight(camera);

            public static int GetPixelWidth(Camera camera) =>
                Mathf.Max(1, camera.pixelWidth);

            public static int GetPixelHeight(Camera camera) =>
                Mathf.Max(1, camera.pixelHeight);
        }

        internal readonly struct ViewportBounds
        {
            public readonly float Left;
            public readonly float Right;
            public readonly float Top;
            public readonly float Bottom;
            public readonly float AvailableWidth;
            public readonly float AvailableHeight;

            public ViewportBounds(
                float left,
                float right,
                float top,
                float bottom,
                float availableWidth,
                float availableHeight)
            {
                Left = left;
                Right = right;
                Top = top;
                Bottom = bottom;
                AvailableWidth = availableWidth;
                AvailableHeight = availableHeight;
            }
        }

        internal static class Viewport
        {
            private const float MinAvailableAxis = 0.1f;

            public static ViewportBounds Calculate(Camera camera, SolitaireDeckConfigSO config)
            {
                Frustum.GetLayoutFrustum(camera, out float halfWidth, out float halfHeight);
                Vector3 cameraPosition = camera.transform.position;
                bool isLandscape = Orientation.IsLandscape(camera);

                float left = cameraPosition.x - halfWidth + config.BoardHorizontalPadding;
                float right = cameraPosition.x + halfWidth - config.BoardHorizontalPadding;
                float topPadding = ResolveTopPadding(isLandscape, config);
                float bottomPadding = ResolveBottomPadding(isLandscape, config);
                float top = cameraPosition.y + halfHeight - topPadding;
                float bottom = cameraPosition.y - halfHeight + bottomPadding;

                return new ViewportBounds(
                    left,
                    right,
                    top,
                    bottom,
                    Mathf.Max(MinAvailableAxis, right - left),
                    Mathf.Max(MinAvailableAxis, top - bottom));
            }

            private static float ResolveTopPadding(bool isLandscape, SolitaireDeckConfigSO config) =>
                isLandscape ? LayoutConstants.LandscapeTopPadding : config.BoardTopHudPadding;

            private static float ResolveBottomPadding(bool isLandscape, SolitaireDeckConfigSO config) =>
                isLandscape ? LayoutConstants.LandscapeBottomPadding : config.BoardBottomPadding;
        }
    }
}
