using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Buttons
{
    public abstract class ButtonBase : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Button targetButton;
        [SerializeField] private bool usePointerDownInsteadOfClick = false;

        protected Button TargetButton => targetButton;

        protected virtual void Awake()
        {
            InitializeButton();
        }

        protected void InitializeButton()
        {
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
            }

            if (targetButton == null)
                return;

            if (usePointerDownInsteadOfClick)
            {
                // Remove all listeners from OnClick to disable it.
                targetButton.onClick.RemoveAllListeners();
            }
            else
            {
                targetButton.onClick.AddListener(HandleClick);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (usePointerDownInsteadOfClick)
            {
                HandleClick();
            }
        }

        public void HandleClick()
        {
            OnClicked();
        }

        protected abstract void OnClicked();
    }
}
