using _Game.Scripts.ScriptableObjects.RunTime;
using _Game.Scripts.ScriptableObjects.Saveable;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Combat
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "ThisGame/BulletData", order = 0)]
    public class BulletDataSO : UpgradableSO
    {
        #region Serialized Fields

        [SerializeField] private float a_damage = 0.05f;
        [SerializeField] private float b_damage = 1f;
        [SerializeField] private float c_damage = 10f;
        [SerializeField] private float a_range = 0.04f;
        [SerializeField] private float b_range = 0.8f;
        [SerializeField] private float c_range = 8f;
        [SerializeField] private float a_speed = 0.01f;
        [SerializeField] private float b_speed = 0.2f;
        [SerializeField] private float c_speed = 2f;

        #endregion
        
        public float GetDamage(int currentLevel)
        {
            return EvaluateQuadratic(a_damage, b_damage, c_damage, currentLevel);
        }
        
        public float GetRange(int currentLevel)
        {
            return EvaluateQuadratic(a_range, b_range, c_range, currentLevel);
        }
        
        public float GetSpeed(int currentLevel)
        {
            return EvaluateQuadratic(a_speed, b_speed, c_speed, currentLevel);
        }

        private static float EvaluateQuadratic(float a, float b, float c, int level)
        {
            return a * Mathf.Pow(level, 2) + b * level + c;
        }
    }
}
