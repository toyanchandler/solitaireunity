using System;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public abstract class ShooterProvider : MonoBehaviour, IShooter
    {
        #region Public Variables

        [OdinSerialize] public WeaponDataSO _weaponDataSO;
        [OdinSerialize] public BulletDataSO _bulletDataSO;

        public bool CanShoot { get; set; } = true;

        #endregion

        #region Unity Methods
        
        private void OnEnable() => Subscribe();

        private void OnDisable() => UnSubscribe();

        #endregion

        #region Private Methods
        
        private void Subscribe()
        {
            // Subscribe to events
        }

        private void UnSubscribe()
        {
            // Unsubscribe from events
        }

        #endregion

        #region Public Methods

        public void OnFire()
        {
            // Fire
        }

        #endregion

        #region Virtual Methods

        public virtual void Shoot()
        {
            OnFire();
        }

        #endregion
    }
    
    [Serializable]
    public struct ProjectileStructData
    {
        public float speed;
        public float range;
        public float damage;
    }
    
    [Serializable]
    public struct WeaponStructData
    {
        internal float fireRate;
        public GameObject weaponObject;
        public GameObject bulletObject;
        public Transform muzzle;
    }
}