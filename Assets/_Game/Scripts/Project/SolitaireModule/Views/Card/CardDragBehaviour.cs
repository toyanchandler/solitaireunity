using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class CardDragBehaviour : MonoBehaviour
    {
        private Transform _previousParent;
        private Vector3 _dragOffset;

        public Transform PreviousParent => _previousParent;

        public void BeginDrag(Transform dragParent, Vector3 pointerWorldPosition)
        {
            _previousParent = transform.parent;
            _dragOffset = transform.position - pointerWorldPosition;
            transform.SetParent(dragParent, true);
        }

        public void MoveToPointer(Vector3 pointerWorldPosition)
        {
            Vector3 target = pointerWorldPosition + _dragOffset;
            target.z = transform.position.z;
            transform.position = target;
        }

        public void EndDrag(Transform targetParent)
        {
            transform.SetParent(targetParent, true);
            _previousParent = null;
        }
    }
}
