using System;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public sealed class GateDamageHandler : MonoBehaviour
    {
        public void ApplyDamage(
            ref GateInteractableData gateData,
            ref GuardedGateData guardedGateData,
            bool isGuardedGate,
            Action openDoors,
            Action<bool> setCanInteract)
        {
            if (isGuardedGate)
            {
                ApplyGuardedDamage(ref gateData, ref guardedGateData, openDoors, setCanInteract);
                return;
            }

            IncreaseGateAmount(ref gateData);
            NormalizeGateAmount(ref gateData);
        }

        private static void ApplyGuardedDamage(
            ref GateInteractableData gateData,
            ref GuardedGateData guardedGateData,
            Action openDoors,
            Action<bool> setCanInteract)
        {
            if (IsGateOpen(guardedGateData))
            {
                IncreaseGateAmount(ref gateData);
                NormalizeGateAmount(ref gateData);
                return;
            }

            guardedGateData.CurrentAmount += 1;
            openDoors?.Invoke();
            setCanInteract?.Invoke(IsGateOpen(guardedGateData));
        }

        private static bool IsGateOpen(GuardedGateData guardedGateData)
        {
            return guardedGateData.CurrentAmount == guardedGateData.MaxAmount;
        }

        private static void IncreaseGateAmount(ref GateInteractableData gateData)
        {
            switch (gateData.mathType)
            {
                case MathType.Add:
                    gateData.Amount += 1;
                    break;
                case MathType.Subtract:
                    gateData.Amount -= 1;
                    break;
                case MathType.Multiply:
                    gateData.Amount += 1;
                    break;
                case MathType.Divide:
                    gateData.Amount -= 1;
                    break;
                default:
                    gateData.Amount -= 1;
                    break;
            }
        }

        private static void NormalizeGateAmount(ref GateInteractableData gateData)
        {
            if (gateData.Amount != 0) return;

            switch (gateData.mathType)
            {
                case MathType.Subtract:
                    gateData.mathType = MathType.Add;
                    gateData.Amount = 1;
                    break;
                case MathType.Divide:
                    gateData.mathType = MathType.Multiply;
                    gateData.Amount = 1;
                    break;
            }
        }
    }
}
