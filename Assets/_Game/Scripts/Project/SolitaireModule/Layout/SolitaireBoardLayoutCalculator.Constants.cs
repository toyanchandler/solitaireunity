namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public static partial class SolitaireBoardLayoutCalculator
    {
        internal static class LayoutConstants
        {
            public const float PortraitMinTableauStackHeightFactor = 3.5f;
            public const float LandscapeMinTableauStackHeightFactor = 0.85f;
            public const float LandscapeTopGroupGapRatio = 0.85f;
            public const float LandscapeMaxCardWidth = 1.45f;
            public const float LandscapeTopPadding = 0.95f;
            public const float LandscapeBottomPadding = 0.25f;

            public const float PortraitVerticalRowCount = 3f;
            public const float PortraitVerticalGapMultiplier = 2f;
            public const int StockWasteColumnCount = 2;
            public const float HalfCardCenterOffset = 0.5f;
            public const float MinCardSizeDivisor = 0.01f;

            public const float LandscapeTopRowCardCount = 6f;
            public const float LandscapeFoundationGapCount = 4f;
            public const float LandscapeVerticalRowCount = 2f;
            public const float LandscapeTableauVerticalAnchor = 0.75f;
            public const float LandscapeTableauVerticalBlend = 0.70f;
            public const float LandscapeFoundationGapMaxRatio = 0.45f;
            public const int LandscapeTopGroupOverlapColumnCount = 6;
        }
    }
}
