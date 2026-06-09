using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public abstract class ParticleProvider : MonoBehaviour
    {
        #region Private Variables

        [SerializeField] internal ParticleSystem _particleSystem;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void PlayParticle()
        {
            _particleSystem.Play();
        }

        protected virtual void StopParticle()
        {
            _particleSystem.Stop();
        }

        #endregion
    }
}
