using System.Runtime.InteropServices;
using _Game.Scripts.Template.Runner.Inputs;
using UnityEngine;
using Sirenix.OdinInspector;

namespace _Game.Scripts.Template.GlobalProviders.Upgrade.UpgradeInjectedMono
{
    public class UpgradableRunnerObjectTranslating : RunnerObjectTranslating
    {
        [SerializeField] private PlayerUpgradeData _playerUpgradeData;
        [SerializeField] private SpeedUpgradeSO _upgradableSO;
        [SerializeField] private bool _clampSpeed;
        
        [ShowIf("_clampSpeed")]
        [SerializeField] float minSpeed = 1f;
        [ShowIf("_clampSpeed")]
        [SerializeField] float maxSpeed = 10f;
        protected override void StartTranslate()
        {
            if (_playerUpgradeData != null && _upgradableSO != null)
            {
                _speed = _upgradableSO.GetValue(_playerUpgradeData.GetUpgradeLevel(_upgradableSO.UpgradeType));
                if (_clampSpeed)
                {
                    _speed = Mathf.Clamp(_speed, minSpeed, maxSpeed);
                }
                
            }
            base.StartTranslate();
        }
    }
}
