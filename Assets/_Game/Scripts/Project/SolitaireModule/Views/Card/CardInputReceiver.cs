using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class CardInputReceiver : MonoBehaviour
    {
        [SerializeField] private CardRuntimeIdentity identity;
        [SerializeField] private CardView view;
        [SerializeField] private CardDragBehaviour dragBehaviour;

        public CardRuntimeIdentity Identity => identity;
        public CardView View => view;
        public CardDragBehaviour DragBehaviour => dragBehaviour;

        public bool Validate(out string error)
        {
            if (identity == null)
            {
                error = $"{name} is missing CardRuntimeIdentity.";
                return false;
            }

            if (view == null)
            {
                error = $"{name} is missing CardView.";
                return false;
            }

            if (dragBehaviour == null)
            {
                error = $"{name} is missing CardDragBehaviour.";
                return false;
            }

            error = string.Empty;
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            identity = GetComponent<CardRuntimeIdentity>();
            view = GetComponent<CardView>();
            dragBehaviour = GetComponent<CardDragBehaviour>();
        }
#endif
    }
}
