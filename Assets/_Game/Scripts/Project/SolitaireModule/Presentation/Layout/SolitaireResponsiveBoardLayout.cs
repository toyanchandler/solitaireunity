using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Presentation
{
    public sealed class SolitaireResponsiveBoardLayout
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;

        public void Initialize(SolitaireDeckConfigSO config, SolitaireRuntimeContext context)
        {
            _config = config;
            _context = context;
        }

        public void Apply(Camera camera)
        {
            if (camera == null || !camera.orthographic ||
                !SolitaireBoardLayoutCalculator.TryCalculateResponsive(camera, _config, out SolitaireBoardLayoutResult layout))
            {
                _context.LayoutMetrics.ResetToConfig(_config);
                ApplySlotSizes(_context.LayoutMetrics.CardSize);
                return;
            }

            _context.LayoutMetrics.Apply(layout);
            PositionSlot(_context.ViewRegistry.Stock, layout.StockPosition);
            PositionSlot(_context.ViewRegistry.Waste, layout.WastePosition);

            for (int i = 0; i < SolitaireCardUtility.FoundationCount; i++)
                PositionSlot(_context.ViewRegistry.Foundations[i], layout.FoundationPositions[i]);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
                PositionSlot(_context.ViewRegistry.Tableaus[i], layout.TableauPositions[i]);

            ApplySlotSizes(layout.CardSize);
        }

        private void PositionSlot(SolitaireSlotAnchor slot, Vector3 position)
        {
            if (slot != null)
                slot.transform.position = position;
        }

        private void ApplySlotSizes(Vector2 cardSize)
        {
            _context.ViewRegistry.Stock?.ApplyLayoutSize(cardSize);
            _context.ViewRegistry.Waste?.ApplyLayoutSize(cardSize);

            for (int i = 0; i < _context.ViewRegistry.Foundations.Length; i++)
                _context.ViewRegistry.Foundations[i]?.ApplyLayoutSize(cardSize);

            float columnBottomY = _context.LayoutMetrics.TableauBottomPlayableY;

            for (int i = 0; i < _context.ViewRegistry.Tableaus.Length; i++)
                _context.ViewRegistry.Tableaus[i]?.ApplyTableauColumnDropArea(cardSize, columnBottomY);
        }
    }
}
