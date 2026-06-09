using System;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Input;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public enum SolitairePointerPhase
    {
        None = 0,
        Down = 1,
        Hold = 2,
        Up = 3
    }

    public sealed class SolitairePointerInputSource : MonoBehaviour
    {
        private readonly ISolitairePointerSampler[] _samplers =
        {
            new SolitaireTouchPointerSampler(),
            new SolitaireMousePointerSampler(),
        };

        private SolitaireBoardCameraController _boardCameraController;

        private void OnEnable()
        {
            EventManager.SolitaireEvents.BoardCameraReady += HandleBoardCameraReady;

            if (SolitaireFeatureRegistration.BoardCamera != null)
                HandleBoardCameraReady(SolitaireFeatureRegistration.BoardCamera);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.BoardCameraReady -= HandleBoardCameraReady;
            _boardCameraController = null;
        }

        private void HandleBoardCameraReady(SolitaireBoardCameraController boardCameraController)
        {
            _boardCameraController = boardCameraController;
        }

        public bool TryGetPointer(out Vector3 pointerWorld, out SolitairePointerPhase phase)
        {
            pointerWorld = default;
            phase = SolitairePointerPhase.None;

            if (_boardCameraController == null)
                throw new InvalidOperationException($"{nameof(SolitairePointerInputSource)} is waiting for a registered board camera.");

            for (int i = 0; i < _samplers.Length; i++)
            {
                if (_samplers[i].TrySample(_boardCameraController, out pointerWorld, out phase))
                    return true;
            }

            return false;
        }
    }
}
