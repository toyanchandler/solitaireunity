using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Feeling.BaseFeelingProviders
{
    public class ObjectSpawnProvider : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Damage Visuals")]
        [SerializeField] public GameObject _damageEffectPrefab;
        [SerializeField] public float _effectDuration = 2.0f;
        [SerializeField] public Vector2 _randomOffsetRange = new Vector2(1.0f, 1.0f);

        #endregion

        #region Protected Methods

        protected void CreateCurvyMovement(GameObject damageEffect)
        {
            var position = damageEffect.transform.position;
            Vector3 endPosition = position + new Vector3(Random.Range(-_randomOffsetRange.x, _randomOffsetRange.x), 0, Random.Range(-_randomOffsetRange.y, _randomOffsetRange.y));
            endPosition.y = 0;

            // Define a midpoint for the curve. Higher the Y value, more pronounced the curve.
            Vector3 midPoint = (position + endPosition) / 2 + Vector3.up * 2.5f;

            Vector3[] path = new Vector3[] { midPoint, endPosition };

            damageEffect.transform.DOLocalPath(path, _effectDuration, PathType.CatmullRom)
                .SetOptions(false)
                .SetEase(Ease.InOutQuad);
        }

        protected GameObject DamageEffectObject(Vector3 targetPosition)
        {
            Vector3 spawnPosition = targetPosition;
            GameObject damageEffectInstance = Instantiate(_damageEffectPrefab, spawnPosition, Quaternion.identity);

            return damageEffectInstance;
        }

        #endregion
    }
}