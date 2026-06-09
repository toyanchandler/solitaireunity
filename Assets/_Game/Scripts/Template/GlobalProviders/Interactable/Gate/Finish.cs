using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.Core;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public class Finish : MonoBehaviour, IInteractableAction
    {
        #region Public Methods

        public void OnInteract()
        {
            TDebug.Log("Finish interacted with");
            EventManager.InGameEvents.LevelSuccess?.Invoke();
        }
        
        #endregion
    }
}