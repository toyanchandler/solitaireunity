using _Game.Scripts.Project.SolitaireModule.Controllers;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Input
{
    internal interface ISolitairePointerSampler
    {
        bool TrySample(SolitaireBoardCameraController boardCameraController, out Vector3 pointerWorld, out SolitairePointerPhase phase);
    }

    internal sealed class SolitaireTouchPointerSampler : ISolitairePointerSampler
    {
        private static readonly SolitairePointerPhase[] TouchPhaseLookup = CreateTouchPhaseLookup();

        public bool TrySample(SolitaireBoardCameraController boardCameraController, out Vector3 pointerWorld, out SolitairePointerPhase phase)
        {
            pointerWorld = default;
            phase = SolitairePointerPhase.None;

            if (UnityEngine.Input.touchCount <= 0)
                return false;

            Touch touch = UnityEngine.Input.GetTouch(0);

            if (!boardCameraController.TryScreenToWorld(touch.position, out pointerWorld))
                return false;

            phase = TouchPhaseLookup[(int)touch.phase];
            return true;
        }

        private static SolitairePointerPhase[] CreateTouchPhaseLookup()
        {
            var lookup = new SolitairePointerPhase[5];
            lookup[(int)TouchPhase.Began] = SolitairePointerPhase.Down;
            lookup[(int)TouchPhase.Ended] = SolitairePointerPhase.Up;
            lookup[(int)TouchPhase.Canceled] = SolitairePointerPhase.Up;
            lookup[(int)TouchPhase.Stationary] = SolitairePointerPhase.Hold;
            lookup[(int)TouchPhase.Moved] = SolitairePointerPhase.Hold;
            return lookup;
        }
    }

    internal sealed class SolitaireMousePointerSampler : ISolitairePointerSampler
    {
        public bool TrySample(SolitaireBoardCameraController boardCameraController, out Vector3 pointerWorld, out SolitairePointerPhase phase)
        {
            pointerWorld = default;
            phase = SolitairePointerPhase.None;

            bool isDown = UnityEngine.Input.GetMouseButtonDown(0);
            bool isHeld = UnityEngine.Input.GetMouseButton(0);
            bool isUp = UnityEngine.Input.GetMouseButtonUp(0);

            SolitairePointerPhase resolvedPhase = ResolveMousePhase(isDown, isHeld, isUp);

            if (resolvedPhase == SolitairePointerPhase.None)
                return false;

            if (!boardCameraController.TryScreenToWorld(UnityEngine.Input.mousePosition, out pointerWorld))
                return false;

            phase = resolvedPhase;
            return true;
        }

        private static SolitairePointerPhase ResolveMousePhase(bool isDown, bool isHeld, bool isUp)
        {
            if (isDown)
                return SolitairePointerPhase.Down;

            if (isUp)
                return SolitairePointerPhase.Up;

            return isHeld ? SolitairePointerPhase.Hold : SolitairePointerPhase.None;
        }
    }
}
