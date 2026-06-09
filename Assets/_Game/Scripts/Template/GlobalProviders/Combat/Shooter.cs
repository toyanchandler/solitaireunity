using System;
using _Game.Scripts.Helper.Services;
using _Game.Scripts.Helper.Pooling;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using _Game.Scripts.Template.GlobalProviders.Upgrade;
using Fluxy;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    public sealed class Shooter : ShooterProvider
    {
        #region Public Variables
        private CoroutineService CoroutineService { get; set; }

        [SerializeField] private PlayerUpgradeData _playerUpgradeDataSO;
        
        private ProjectileStructData _projectileStructData = new ProjectileStructData();
        
        [SerializeField] private WeaponStructData _weaponStructData = new WeaponStructData();

        [SerializeField] private Transform _defaultMuzzle;

        public FluxyTarget target;
        
        #endregion

        #region Private Variables

        private Coroutine _fireCoroutine;

        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            CoroutineService = new CoroutineService(this);
            
            InitWeaponData();
        }

        internal void OnEnable() => Subscribe();

        internal void OnDisable() => UnSubscribe();

        #endregion

        #region Private Methods

        private void Subscribe()
        {
            EventManager.InGameEvents.LevelStart += StartFire;
            EventManager.InGameEvents.LevelSuccess += StopFire;
            EventManager.InGameEvents.LevelFail += StopFire;
            EventManager.InteractableEvents.GateInteract += GateInteractRestartFire;
            EventManager.ShootableEvents.Shoot += LoadFluidTargetOnShoot;
        }
        
        private void UnSubscribe()
        {
            EventManager.InGameEvents.LevelStart -= StartFire;
            EventManager.InGameEvents.LevelSuccess -= StopFire;
            EventManager.InGameEvents.LevelFail -= StopFire;
            EventManager.InteractableEvents.GateInteract -= GateInteractRestartFire;
            EventManager.ShootableEvents.Shoot -= LoadFluidTargetOnShoot;
        }

        private void StartFire()
        {
            StopFire();
            InitProjectileData();
            _fireCoroutine = CoroutineService.StartIntervalRoutine(Shoot, _weaponDataSO.GetFireRate(_playerUpgradeDataSO.GetUpgradeLevel(_weaponDataSO.UpgradeType)), ()=> true);
        }
        
        private void StopFire()
        {
            CoroutineService.Stop(_fireCoroutine);
            _fireCoroutine = null;
        }

        private void GateInteractRestartFire(GateInteractableData data)
        {
            StopFire();
            StartFire();
        }
        
        private void LoadFluidTargetOnShoot(WeaponStructData data)
        {
            if (target == null) return;
            EventManager.ShootableEvents.FluidOnShoot?.Invoke(target);
        }
        
        private void InitWeaponData()
        {
            if (_weaponStructData.bulletObject == null)
            {
                var bullet = CreateProjectileInstancePrimitive();
                _weaponStructData.bulletObject = bullet;
                bullet.SetActive(false);
            }
            
            if (_weaponStructData.weaponObject == null)
                _weaponStructData.weaponObject = gameObject;
            
            if (_weaponStructData.muzzle == null)
                _weaponStructData.muzzle = ResolveMuzzle();
        }

        private Transform ResolveMuzzle()
        {
            if (_defaultMuzzle != null) return _defaultMuzzle;

            var namedMuzzle = FindMuzzleByName();
            if (namedMuzzle != null) return namedMuzzle;

            Debug.LogWarning($"{nameof(Shooter)} on {name} has no muzzle assigned. Falling back to shooter transform.");
            return transform;
        }

        private Transform FindMuzzleByName()
        {
            var childTransforms = GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in childTransforms)
            {
                if (childTransform == transform) continue;
                if (childTransform.name.IndexOf("muzzle", StringComparison.OrdinalIgnoreCase) >= 0)
                    return childTransform;
            }

            return null;
        }
        
        private void InitProjectileData()
        {
            _projectileStructData = new ProjectileStructData
            {
                speed = _bulletDataSO.GetSpeed(_playerUpgradeDataSO.GetUpgradeLevel(_bulletDataSO.UpgradeType)),
                range = _bulletDataSO.GetRange(_playerUpgradeDataSO.GetUpgradeLevel(_bulletDataSO.UpgradeType)),
                damage = _bulletDataSO.GetDamage(_playerUpgradeDataSO.GetUpgradeLevel(_bulletDataSO.UpgradeType))
            };
        }

        #region Bullet Initiliaze

        private GameObject CreateProjectileInstancePrimitive()
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.name = "Projectile Clone";
            primitive.transform.localScale = Vector3.one * 0.5f;
            primitive.GetComponent<Collider>().isTrigger = true;
            primitive.AddComponent<Rigidbody>().isKinematic = true;
            primitive.AddComponent<Projectile>();
            return primitive;
        }

        private GameObject SpawnPooledProjectileInstance()
        {
            var bullet = GameObjectPool.Spawn(
                _weaponStructData.bulletObject, 
                _weaponStructData.muzzle.position, 
                transform.rotation);

            return bullet;
        }

        #endregion
        private void CallShootEvent()
        {
            if (!CanShoot) return;

            EventManager.ShootableEvents.Shoot?.Invoke(new WeaponStructData
            {
                fireRate = _weaponDataSO.GetFireRate(_playerUpgradeDataSO.GetUpgradeLevel(_bulletDataSO.UpgradeType)),
            });
        }

        private void InitProjectile()
        {
            var projectile = SpawnPooledProjectileInstance().GetComponent<Projectile>();

            target = projectile.GetComponent<FluxyTarget>();
            
            projectile.OnSpawn();
            
            projectile.Initialize(_projectileStructData);
        }

        public override void Shoot()
        {
            base.Shoot();
            CallShootEvent();
            InitProjectile();
            OnFire();
        }

        #endregion
    }
}
