using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Data
{
    [CreateAssetMenu(
        fileName = "SolitaireScoreConfig",
        menuName = "GameModules/Solitaire/Score Config")]
    public sealed class SolitaireScoreConfigSO : ScriptableObject
    {
        [SerializeField] private int moveToTableau = 0;
        [SerializeField] private int moveToFoundation = 10;
        [SerializeField] private int revealTableauCard = 5;
        [SerializeField] private int undo = -1;
        [SerializeField] private int stockDraw = 0;
        [SerializeField] private int stockRecycle = 0;
        [SerializeField] private bool clampMinimum = true;
        [SerializeField] private int minimumScore = 0;

        public int GetDelta(SolitaireScoreAction action)
        {
            return action switch
            {
                SolitaireScoreAction.MoveToTableau => moveToTableau,
                SolitaireScoreAction.MoveToFoundation => moveToFoundation,
                SolitaireScoreAction.RevealTableauCard => revealTableauCard,
                SolitaireScoreAction.Undo => undo,
                SolitaireScoreAction.StockDraw => stockDraw,
                SolitaireScoreAction.StockRecycle => stockRecycle,
                _ => 0
            };
        }

        public int ClampScore(int score)
        {
            return clampMinimum && score < minimumScore ? minimumScore : score;
        }
    }
}
