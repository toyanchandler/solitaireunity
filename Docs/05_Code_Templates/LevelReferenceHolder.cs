using UnityEngine;

namespace _Game.Scripts.Level
{
    public sealed class LevelReferenceHolder : MonoBehaviour
    {
        [SerializeField] private Transform _charSpawnPoint;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private Transform _finishPoint;
        [SerializeField] private Transform _collectableRoot;
        [SerializeField] private Transform _obstacleRoot;

        public Transform CharSpawnPoint => _charSpawnPoint;
        public Transform CameraTarget => _cameraTarget;
        public Transform FinishPoint => _finishPoint;
        public Transform CollectableRoot => _collectableRoot;
        public Transform ObstacleRoot => _obstacleRoot;

        public bool Validate(out string error)
        {
            if (_charSpawnPoint == null)
            {
                error = $"{name} is missing CharacterSpawnPoint.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
