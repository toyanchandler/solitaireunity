using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Scripts.InGame.ReferenceHolder
{
    public sealed class LevelReferenceHolder : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform _finishPoint;
        [SerializeField] private Transform _charSpawnPoint;

        [SerializeField] [CanBeNull] private Transform _cameraTarget;
        [SerializeField] [CanBeNull] private Transform _collectableRoot;
        [SerializeField] [CanBeNull] private Transform _obstacleRoot;
        [SerializeField] [CanBeNull] private Transform _tutorialRoot;
        [SerializeField] [CanBeNull] private Transform _boundsRoot;

        #endregion

        #region Properties

        public Transform SuccessTrigger => _finishPoint;
        public Transform CharSpawnPoint => _charSpawnPoint;
        public Transform FinishPoint => _finishPoint;
        public Transform CameraTarget => _cameraTarget;
        public Transform CollectableRoot => _collectableRoot;
        public Transform ObstacleRoot => _obstacleRoot;
        public Transform TutorialRoot => _tutorialRoot;
        public Transform BoundsRoot => _boundsRoot;
        public bool HasCameraTarget => _cameraTarget != null;
        public bool HasCollectableRoot => _collectableRoot != null;
        public bool HasObstacleRoot => _obstacleRoot != null;
        public bool HasTutorialRoot => _tutorialRoot != null;
        public bool HasBoundsRoot => _boundsRoot != null;
        #endregion

        public bool Validate(out string error)
        {
            if (_charSpawnPoint == null)
            {
                error = $"{name} is missing required CharSpawnPoint.";
                return false;
            }

            if (_finishPoint == null)
            {
                error = $"{name} is missing required FinishPoint.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
