using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    /// <summary>
    /// Lives on ControllerHost. Resolves sibling controllers on the same GameObject and announces readiness.
    /// </summary>
    public sealed class SolitaireModuleControllerHost : MonoBehaviour
    {
        private SolitaireModuleControllerBundle _bundle;

        private void OnEnable()
        {
            _bundle = SolitaireModuleControllerBundle.FromHost(gameObject);
            SolitaireFeatureRegistration.RegisterControllerHost(_bundle);
            EventManager.SolitaireEvents.ControllerHostReady?.Invoke(_bundle);
        }

        private void OnDisable()
        {
            if (_bundle == null)
                return;

            SolitaireFeatureRegistration.UnregisterControllerHost(_bundle);
            _bundle = null;
        }
    }
}
