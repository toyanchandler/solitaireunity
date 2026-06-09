using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable
{
    public class CharacterTriggerDetector : MonoBehaviour
    {
        #region Unity Methods

        private void OnTriggerEnter(Collider other)
        {
            HandleInteractions(other.gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            HandleInteractions(other.gameObject);
        }

        #endregion

        #region Private Methods

        private void HandleInteractions(GameObject otherObject)
        {
            // Interactable Objects
            if (otherObject.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }

        #endregion
    }
}