using _Game.Scripts.Managers.Core;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class SolitaireDeckRippleFx : MonoBehaviour
    {
        [SerializeField] private SolitairePulseRingView pulseRing;
        [SerializeField] private ParticleSystem rippleParticles;

        private void Awake()
        {
            ResolveRippleParticles();
        }

        private void OnEnable()
        {
            EventManager.SolitaireEvents.StockDrawClicked += HandleStockDrawClicked;
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.StockDrawClicked -= HandleStockDrawClicked;
        }

        private void HandleStockDrawClicked()
        {
            Debug.Log("[SolitaireFx] temp — deck ripple played at stock slot");

            if (rippleParticles != null)
            {
                rippleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                rippleParticles.Play(true);
            }

            pulseRing?.Play();
        }

        private void ResolveRippleParticles()
        {
            if (rippleParticles != null)
                return;

            SolitaireDeckRippleParticleView particleView = GetComponentInChildren<SolitaireDeckRippleParticleView>(true);

            if (particleView != null)
            {
                rippleParticles = particleView.RippleParticles;
                return;
            }

            rippleParticles = GetComponentInChildren<ParticleSystem>(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (pulseRing == null)
                pulseRing = GetComponentInChildren<SolitairePulseRingView>(true);

            ResolveRippleParticles();
        }
#endif
    }
}
