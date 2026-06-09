using _Game.Scripts.Template.GlobalProviders.Interactable;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}