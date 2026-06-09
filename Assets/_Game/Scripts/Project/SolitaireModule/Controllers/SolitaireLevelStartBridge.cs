using _Game.Scripts.Managers.Core;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireLevelStartBridge : MonoBehaviour
    {
        private SolitaireModuleBootstrap _bootstrap;

        public void Initialize(SolitaireModuleBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
        }

        private void OnEnable()
        {
            EventManager.InGameEvents.LevelStart += HandleLevelStart;
        }

        private void OnDisable()
        {
            EventManager.InGameEvents.LevelStart -= HandleLevelStart;
        }

        private void HandleLevelStart()
        {
            _bootstrap?.StartDeal();
        }
    }
}
