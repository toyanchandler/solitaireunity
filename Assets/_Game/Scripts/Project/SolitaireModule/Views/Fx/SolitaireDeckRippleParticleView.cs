using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    [DisallowMultipleComponent]
    public sealed class SolitaireDeckRippleParticleView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rippleParticles;

        public ParticleSystem RippleParticles
        {
            get
            {
                if (rippleParticles == null)
                    rippleParticles = GetComponent<ParticleSystem>();

                return rippleParticles;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rippleParticles == null)
                rippleParticles = GetComponent<ParticleSystem>();
        }
#endif
    }
}
