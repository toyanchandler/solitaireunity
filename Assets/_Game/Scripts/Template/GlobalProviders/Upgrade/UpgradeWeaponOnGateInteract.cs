using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Combat;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Upgrade
{
    public class UpgradeWeaponOnGateInteract : MonoBehaviour
    {
        #region Private Variables

        [SerializeField] private PlayerUpgradeData playerUpgradeData;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            EventManager.InteractableEvents.GateInteract += UpgradeWeaponSystem;
        }

        private void OnDisable()
        {
            EventManager.InteractableEvents.GateInteract -= UpgradeWeaponSystem;
        }

        #endregion

        #region Private Methods

        private void UpgradeWeaponSystem(GateInteractableData data)
        {
            playerUpgradeData.IncreaseUpgradeLevel(UpgradeType.BulletData, data.Amount);
        }

        #endregion

    }
}