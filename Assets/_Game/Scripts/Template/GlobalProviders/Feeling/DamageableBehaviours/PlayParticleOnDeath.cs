using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using SRDebugger.UI.Other;
using UnityEngine;
using DG.Tweening;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.DamageableBehaviours
{
    public class PlayParticleOnDeath : ParticleProvider, IDamageableAction
    {
        public void Initialize(DamageableObject damageableObject)
        {
            
        }
        public void TakeDamage(float damage)
        {
        }
        public void HealthChanged(float currentHealth)
        {
        }
        public void Death()
        {
            PlayParticle();
            
            _particleSystem.gameObject.transform.parent = null;
            
            ObjectTween();
        }
        
        private void ObjectTween()
        {
            //transform.DOScale(Vector3.zero, _particleSystem.main.duration+2);
            //Destroy(gameObject, _particleSystem.main.duration+2);
        }
    }
}
