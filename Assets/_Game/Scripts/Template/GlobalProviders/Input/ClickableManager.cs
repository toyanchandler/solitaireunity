using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Input
{
    public class ClickableManager : InputProvider
    {
        private Camera _camera;

        private IClickable _activeClickable;

        #region Inherited Methods

        protected override void Initialize()
        {
            base.Initialize();
            _camera = Camera.main;
        }

        protected override void OnClickDown(ClickData clickData)
        {
            _activeClickable = RaycastClickable();
            _activeClickable?.OnClickedDown();
        }

        protected override void OnClickHold(ClickData clickData)
        {
            _activeClickable?.OnClickedHold();
        }

        protected override void OnClickUp(ClickData clickData)
        {
            _activeClickable?.OnClickedUp();
            _activeClickable = null;
        }

        private IClickable RaycastClickable()
        {
            if (_camera == null) return null;

            var ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            return Physics.Raycast(ray, out var hit) ? hit.collider.GetComponent<IClickable>() : null;
        }

        #endregion
    }
}
