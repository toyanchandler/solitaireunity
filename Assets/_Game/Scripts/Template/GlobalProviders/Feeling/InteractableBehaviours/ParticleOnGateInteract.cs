using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Helper.Pooling;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.InteractableBehaviours
{
    public class ParticleManagerOnGateInteract : SerializedMonoBehaviour
    {
        #region Serialized Variables

        [OdinSerialize] private Dictionary<MathType, ParticleSystem> gateParticles;

        #endregion

        #region Unity Methods
        
        private void OnEnable()
        {
            EventManager.InteractableEvents.GateInteract += InstantiateParticleOnGateInteract;
        }

        private void OnDisable()
        {
            EventManager.InteractableEvents.GateInteract -= InstantiateParticleOnGateInteract;
        }

        #endregion

        #region Private Methods

        private void InstantiateParticleOnGateInteract(GateInteractableData data)
        {
            try
            {
                if (gateParticles.TryGetValue(data.mathType, out ParticleSystem particleSystemPrefab))
                {
                    var spawnedParticleGameObject = GameObjectPool.Spawn(
                        particleSystemPrefab.gameObject,
                        data.InteractableGameObject.transform.position,
                        Quaternion.identity
                    );

                    var spawnedParticle = spawnedParticleGameObject.GetComponent<ParticleSystem>();
                    spawnedParticle.Play();

                    StartCoroutine(DespawnWhenDone(spawnedParticleGameObject, spawnedParticle.main.duration));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e + "Particle not found");
                throw;
            }
        }

        private IEnumerator DespawnWhenDone(GameObject particleGameObject, float duration)
        {
            yield return new WaitForSeconds(duration);
            GameObjectPool.Despawn(particleGameObject);
        }

        #endregion
    }
}
