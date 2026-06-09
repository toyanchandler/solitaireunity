using System.Collections.Generic;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Views
{
    public sealed class SolitaireDebugGizmos : MonoBehaviour
    {
        [SerializeField] private Color slotColor = new Color(0.2f, 0.75f, 1f, 0.35f);

        private readonly List<SolitaireSlotAnchor> _slots = new List<SolitaireSlotAnchor>(13);

        private void OnEnable()
        {
            EventManager.SolitaireEvents.SlotRegistered += HandleSlotRegistered;

            IReadOnlyList<SolitaireSlotAnchor> existing = SolitaireFeatureRegistration.GetRegisteredSlotsSnapshot();

            for (int i = 0; i < existing.Count; i++)
                HandleSlotRegistered(existing[i]);
        }

        private void OnDisable()
        {
            EventManager.SolitaireEvents.SlotRegistered -= HandleSlotRegistered;
            _slots.Clear();
        }

        private void HandleSlotRegistered(SolitaireSlotAnchor slot)
        {
            if (slot == null || _slots.Contains(slot))
                return;

            _slots.Add(slot);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = slotColor;

            for (int i = 0; i < _slots.Count; i++)
            {
                SolitaireSlotAnchor slot = _slots[i];

                if (slot == null)
                    continue;

                BoxCollider2D box = slot.BoxCollider;

                if (box == null)
                    continue;

                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
            }
        }
    }
}
