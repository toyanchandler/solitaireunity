using _Game.Scripts.Project.SolitaireModule.Controllers;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Texts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class SolitaireMovesTextAssigner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private SolitaireDeckController deckController;
        [SerializeField] private string prefix = "MOVES ";

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (deckController == null)
            {
                Debug.LogWarning($"{nameof(SolitaireMovesTextAssigner)} on {name} is missing {nameof(SolitaireDeckController)}.", this);
                Render(0);
                return;
            }

            deckController.MoveCountChanged += Render;
            Render(deckController.CurrentMoveCount);
        }

        private void OnDisable()
        {
            if (deckController != null)
                deckController.MoveCountChanged -= Render;
        }

        private void Render(int moveCount)
        {
            if (textMesh != null)
                textMesh.text = prefix + moveCount;
        }
    }
}
