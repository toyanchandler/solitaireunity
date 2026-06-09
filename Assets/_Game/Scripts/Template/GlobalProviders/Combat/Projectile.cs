using _Game.Scripts.Helper.Services;
using _Game.Scripts.Helper.Pooling;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        #region Private Variables

        private ProjectileStructData projectileData;
        private CoroutineService coroutineService;
        private Coroutine translateCoroutine;
        private float distanceTraveled;  // New variable to track the distance traveled

        #endregion

        #region Unity Methods

        private void Awake() => coroutineService = new CoroutineService(this);

        private void OnEnable()
        {
            distanceTraveled = 0f;  // Reset the distance traveled
        }

        #endregion

        #region Public Methods

        public void OnSpawn()
        {
            // Initialize or reset distanceTraveled when the projectile is actually spawned
            distanceTraveled = 0f;
        }

        public void OnDespawn()
        {
            coroutineService.Stop(translateCoroutine);
            ResetProjectile();  
        } 
        
        public void Initialize(ProjectileStructData data)
        {
            this.projectileData = data;
            translateCoroutine = coroutineService.StartUpdateRoutine(Translate, () => true);
        }

        #endregion

        #region Private Methods

        private void ResetProjectile()
        {
            projectileData.speed = 0f;
            projectileData.damage = 0f;
        }
    
        // Rotate WeaponController for adjusting the angle of the weapon
        private void Translate()
        {
            var translationVector = transform.forward * (projectileData.speed * Time.deltaTime);
            
            // Update distance traveled
            distanceTraveled += translationVector.magnitude;

            // Check if the projectile has reached its range
            if (distanceTraveled >= projectileData.range)
            {
                GameObjectPool.Despawn(gameObject);
                return;
            }

            transform.Translate(translationVector, Space.World);   
        }

        private void OnTriggerEnter(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            
            if (damageable == null) return;
            damageable.TakeDamage(projectileData.damage);
            GameObjectPool.Despawn(gameObject);
        }

        #endregion
    }
}
