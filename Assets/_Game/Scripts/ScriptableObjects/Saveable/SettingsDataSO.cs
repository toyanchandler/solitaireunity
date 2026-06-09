using _Game.Scripts.ScriptableObjects.RunTime;
using UnityEngine;

namespace _Game.Scripts.ScriptableObjects.Saveable
{
    [CreateAssetMenu(fileName = "SettingsDataSO", menuName = "ThisGame/SettingsDataSO", order = 0)]
    public class SettingsDataSO : PersistentSaveManager<SettingsDataSO>, IResettable
    {
        [SerializeField] private bool _isSoundEnabled = true;

        [SerializeField] private bool _isVibrationEnabled;

        [SerializeField] private bool _soundDefaultMigrated;

        public bool IsSoundEnabled => _isSoundEnabled;
        public bool IsVibrationEnabled => _isVibrationEnabled;

        public override void LoadData()
        {
            base.LoadData();
            MigrateLegacyDisabledSoundDefault();
        }

        public void SetSoundEnabled(bool isEnabled)
        {
            _isSoundEnabled = isEnabled;
        }

        private void MigrateLegacyDisabledSoundDefault()
        {
            if (_soundDefaultMigrated)
                return;

            if (!_isSoundEnabled && !_isVibrationEnabled)
                _isSoundEnabled = true;

            _soundDefaultMigrated = true;
        }

        public void SetVibrationEnabled(bool isEnabled)
        {
            _isVibrationEnabled = isEnabled;
        }
    }
}
