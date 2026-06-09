using _Game.Scripts.Project.SolitaireModule.Data;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireHapticFeedbackProvider : MonoBehaviour
    {
        [SerializeField] private SolitaireDeckConfigSO deckConfig;

        public void Initialize(SolitaireDeckConfigSO config)
        {
            deckConfig = config;
        }

        public void PlayLight()
        {
            if (!CanPlay())
                return;

            MMVibrationManager.Haptic(HapticTypes.LightImpact, false, true, this);
        }

        public void PlayMedium()
        {
            if (!CanPlay())
                return;

            MMVibrationManager.Haptic(HapticTypes.MediumImpact, false, true, this);
        }

        public void PlayWarning()
        {
            if (!CanPlay())
                return;

            MMVibrationManager.Haptic(HapticTypes.Warning, false, true, this);
        }

        public void PlaySuccess()
        {
            if (!CanPlay())
                return;

            MMVibrationManager.Haptic(HapticTypes.Success, false, true, this);
        }

        private bool CanPlay()
        {
            return deckConfig != null && deckConfig.HapticsEnabled;
        }
    }
}
