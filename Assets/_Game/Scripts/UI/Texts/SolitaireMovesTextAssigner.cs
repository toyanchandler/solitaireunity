using _Game.Scripts.Managers.Core;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Texts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class SolitaireMovesTextAssigner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private string prefix = "MOVES ";

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.MoveCountChanged += Render;
            EventManager.SolitaireEvents.DealStarted += HandleDealStarted;
            Render(0);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.MoveCountChanged -= Render;
            EventManager.SolitaireEvents.DealStarted -= HandleDealStarted;
        }

        private void HandleDealStarted()
        {
            Render(0);
        }

        private void Render(int moveCount)
        {
            if (textMesh != null)
                textMesh.text = prefix + moveCount;
        }
    }
}
