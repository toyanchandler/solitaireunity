using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    /// <summary>
    /// Marks the drag parent and announces itself to the feature registration channel.
    /// </summary>
    public sealed class SolitaireDragLayer : MonoBehaviour
    {
        private void OnEnable()
        {
            SolitaireFeatureRegistration.RegisterDragLayer(transform);
        }

        private void OnDisable()
        {
            SolitaireFeatureRegistration.UnregisterDragLayer(transform);
        }
    }
}
