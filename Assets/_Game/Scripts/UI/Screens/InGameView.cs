using System.Collections;
using _Game.Scripts.UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Screens
{
    public class InGameView : MonoBehaviour
    {
        [SerializeField] private SolitaireUndoButton undoButton;

        private Coroutine _resolveUndoRoutine;

        private void OnEnable()
        {
            if (TryEnsureUndoUiActive())
                return;

            if (_resolveUndoRoutine != null)
                StopCoroutine(_resolveUndoRoutine);

            _resolveUndoRoutine = StartCoroutine(ResolveUndoButtonNextFrame());
        }

        private void OnDisable()
        {
            if (_resolveUndoRoutine == null)
                return;

            StopCoroutine(_resolveUndoRoutine);
            _resolveUndoRoutine = null;
        }

        private IEnumerator ResolveUndoButtonNextFrame()
        {
            yield return null;
            _resolveUndoRoutine = null;

            if (!TryEnsureUndoUiActive())
                Debug.LogWarning($"[SolitaireUndo] {nameof(InGameView)} could not find {nameof(SolitaireUndoButton)} in the active scene.", this);
        }

        private bool TryEnsureUndoUiActive()
        {
            if (undoButton == null)
                undoButton = GetComponentInChildren<SolitaireUndoButton>(true);

            if (undoButton == null)
                undoButton = FindLoadedUndoButton();

            if (undoButton == null)
                return false;

            if (!undoButton.gameObject.activeSelf)
                undoButton.gameObject.SetActive(true);

            if (undoButton.TryGetComponent(out Button button))
                button.interactable = true;

            return true;
        }

        private SolitaireUndoButton FindLoadedUndoButton()
        {
            SolitaireUndoButton[] buttons = Resources.FindObjectsOfTypeAll<SolitaireUndoButton>();

            for (int i = 0; i < buttons.Length; i++)
            {
                SolitaireUndoButton candidate = buttons[i];

                if (candidate == null || !candidate.gameObject.scene.IsValid())
                    continue;

                if (candidate.gameObject.scene != gameObject.scene)
                    continue;

                return candidate;
            }

            return null;
        }
    }
}
