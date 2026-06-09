using System;
using System.Collections;
using _Game.Scripts.Helper.Pooling;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using Cinemachine;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public sealed class CollectableFeedbackOnCollect : MonoBehaviour
    {
        [Header("Collectable")]
        [SerializeField] private CollectableType collectableType = CollectableType.Coin;

        [Header("Sparkle")]
        [SerializeField] private ParticleSystem sparkleParticlePrefab;
        [SerializeField] private Transform sparkleSpawnTarget;
        [SerializeField] private Vector3 sparklePositionOffset = new Vector3(0f, 0.6f, 0f);
        [SerializeField] private float sparkleDespawnPadding = 0.25f;

        [Header("Camera Shake")]
        [SerializeField] private CinemachineVirtualCamera[] shakeCameras = Array.Empty<CinemachineVirtualCamera>();
        [SerializeField] private float shakeAmplitude = 0.45f;
        [SerializeField] private float shakeFrequency = 9f;
        [SerializeField] private float shakeDuration = 0.12f;

        private void OnEnable()
        {
            EventManager.CollectableEvents.Collect += HandleCollect;
        }

        private void OnDisable()
        {
            EventManager.CollectableEvents.Collect -= HandleCollect;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            sparkleDespawnPadding = Mathf.Max(0f, sparkleDespawnPadding);
            shakeAmplitude = Mathf.Max(0f, shakeAmplitude);
            shakeFrequency = Mathf.Max(0f, shakeFrequency);
            shakeDuration = Mathf.Max(0f, shakeDuration);
        }
#endif

        private void HandleCollect(CollectableData collectableData)
        {
            if (collectableData.CollectableType != collectableType)
            {
                return;
            }

            SpawnSparkle(collectableData);
            ShakeCamera();
        }

        private void SpawnSparkle(CollectableData collectableData)
        {
            if (sparkleParticlePrefab == null)
            {
                return;
            }

            Vector3 spawnPosition = ResolveSparklePosition(collectableData) + sparklePositionOffset;
            GameObject spawnedParticleObject = GameObjectPool.Spawn(
                sparkleParticlePrefab.gameObject,
                spawnPosition,
                sparkleParticlePrefab.transform.rotation);

            if (spawnedParticleObject == null || !spawnedParticleObject.TryGetComponent(out ParticleSystem spawnedParticle))
            {
                return;
            }

            spawnedParticle.Clear(true);
            spawnedParticle.Play(true);
            StartCoroutine(DespawnWhenDone(spawnedParticleObject, spawnedParticle));
        }

        private Vector3 ResolveSparklePosition(CollectableData collectableData)
        {
            if (sparkleSpawnTarget != null)
            {
                return sparkleSpawnTarget.position;
            }

            return collectableData.CollectedPosition;
        }

        private void ShakeCamera()
        {
            CinemachineVirtualCamera virtualCamera = ResolveShakeCamera();
            if (virtualCamera == null || virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() == null)
            {
                return;
            }

            virtualCamera.SafeStartNoise(shakeAmplitude, shakeFrequency, shakeDuration, this);
        }

        private CinemachineVirtualCamera ResolveShakeCamera()
        {
            if (shakeCameras == null || shakeCameras.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < shakeCameras.Length; i++)
            {
                CinemachineVirtualCamera virtualCamera = shakeCameras[i];
                if (virtualCamera != null && virtualCamera.isActiveAndEnabled)
                {
                    return virtualCamera;
                }
            }

            for (int i = 0; i < shakeCameras.Length; i++)
            {
                if (shakeCameras[i] != null)
                {
                    return shakeCameras[i];
                }
            }

            return null;
        }

        private IEnumerator DespawnWhenDone(GameObject particleObject, ParticleSystem particleSystem)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            yield return new WaitForSeconds(main.duration + main.startLifetime.constantMax + sparkleDespawnPadding);
            GameObjectPool.Despawn(particleObject);
        }
    }
}
