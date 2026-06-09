using UnityEngine;

namespace _Game.Scripts.Rules
{
    internal static class CameraStateMapper
    {
        public static CameraMode ToCameraMode(GameState state)
        {
            return state switch
            {
                GameState.LevelLoaded => CameraMode.Intro,
                GameState.LevelStart => CameraMode.Gameplay,
                GameState.LevelEnd => CameraMode.Success,
                GameState.Fail => CameraMode.Fail,
                GameState.EndMetaStart => CameraMode.Meta,
                _ => CameraMode.Gameplay
            };
        }
    }

    internal static class PlayerAnimationHashes
    {
        public static readonly int Pose = Animator.StringToHash("Pose");
        public static readonly int Speed = Animator.StringToHash("Speed");
    }

    internal static class PlayerAnimationApplier
    {
        public static void Apply(Animator animator, PlayerAnimationPose pose, float speed)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(PlayerAnimationHashes.Pose, (int)pose);
            animator.SetFloat(PlayerAnimationHashes.Speed, speed);
        }
    }
}
