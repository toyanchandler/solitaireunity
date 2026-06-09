using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders;
using _Game.Scripts.Template.GlobalProviders.Input;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.ClickableBehaviours
{
    public class SpawnObjectsOnClick : ObjectSpawnProvider, IClickableAction
    {
        [SerializeField] private int spawnCount = 5;
        [SerializeField] private float spawnedScale = 0.35f;

        public void OnClickDown()
        {
            if (_damageEffectPrefab == null)
            {
                TDebug.LogWarning($"{nameof(SpawnObjectsOnClick)} requires a reward prefab.");
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject rewardObject = DamageEffectObject(transform.position);
                rewardObject.transform.localScale = Vector3.one * spawnedScale;
                CreateCurvyMovement(rewardObject);
            }
        }

        public void OnClickHold()
        {
        }

        public void OnClickUp()
        {
        }
    }
}
