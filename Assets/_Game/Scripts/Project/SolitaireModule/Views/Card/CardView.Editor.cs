using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed partial class CardView
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            identity = GetComponent<CardRuntimeIdentity>();
            visualStateMachine = GetComponent<CardVisualStateMachine>();
            cardRenderer = GetComponent<SpriteRenderer>();
            sortingGroup = GetComponent<SortingGroup>();
            motionPresenter = GetComponent<CardMotionPresenter>();
            dragShadowRenderer = CardViewLogic.ChildRenderer.Find(transform, CardViewLogic.Constants.DragShadowChildName);
            selectionHighlightRenderer = CardViewLogic.ChildRenderer.Find(transform, CardViewLogic.Constants.SelectionHighlightChildName);
        }
#endif
    }
}
