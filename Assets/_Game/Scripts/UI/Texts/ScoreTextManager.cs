using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Data;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Texts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ScoreTextManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private SolitaireScoreConfigSO scoreConfig;
        [SerializeField] private string prefix = "SCORE ";

        private int _currentScore;

        public int CurrentScore => _currentScore;

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.DealStarted += ResetScore;
            EventManager.SolitaireEvents.ScoreActionPerformed += ApplyScoreAction;
            Render();
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.DealStarted -= ResetScore;
            EventManager.SolitaireEvents.ScoreActionPerformed -= ApplyScoreAction;
        }

        private void ResetScore()
        {
            _currentScore = 0;
            Render();
        }

        private void ApplyScoreAction(SolitaireScoreAction action)
        {
            if (scoreConfig == null)
            {
                Debug.LogWarning($"{nameof(ScoreTextManager)} on {name} is missing {nameof(SolitaireScoreConfigSO)}.", this);
                Render();
                return;
            }

            _currentScore = scoreConfig.ClampScore(_currentScore + scoreConfig.GetDelta(action));
            Render();
        }

        private void Render()
        {
            if (textMesh != null)
                textMesh.text = prefix + _currentScore;
        }
    }
}
