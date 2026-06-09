using System;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public sealed class GateAnimator : MonoBehaviour
    {
        [SerializeField] private float openDuration = 0.15f;

        public void OpenDoors(GuardedGateData guardedGateData)
        {
            if (guardedGateData.DoorCovers == null || guardedGateData.DoorCovers.Count < 2) return;

            var leftDoor = guardedGateData.DoorCovers[0];
            var rightDoor = guardedGateData.DoorCovers[1];

            if (leftDoor == null || rightDoor == null) return;

            try
            {
                var doorsSequence = DOTween.Sequence();
                doorsSequence.Join(leftDoor.transform.DOMoveX(leftDoor.transform.position.x - guardedGateData.coverPositionOffset, openDuration));
                doorsSequence.Join(rightDoor.transform.DOMoveX(rightDoor.transform.position.x + guardedGateData.coverPositionOffset, openDuration));
                doorsSequence.Play();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
