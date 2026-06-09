using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Audio;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.ScriptableObjects.Saveable;
using Handler.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class AudioManager : SerializedMonoBehaviour
    {
        #region Serialized Variables

        [SerializeField] private AudioSource audioSource;

        [OdinSerialize, ShowInInspector] private Dictionary<int, AudioClip> audioClips;

        [SerializeField] private SettingsDataSO settingsData;

        [Header("Solitaire SFX")]
        [SerializeField] private AudioClip dealAudioClip;
        [SerializeField] private AudioClip holdAudioClip;
        [SerializeField] private AudioClip dropAudioClip;
        [SerializeField] private AudioClip dropFailedAudioClip;
        [SerializeField] private AudioClip successAudioClip;
        [SerializeField] private AudioClip foundationSuccessAudioClip;
        [SerializeField] private AudioClip failAudioClip;

        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusicClip;

        #endregion

        #region Private Variables

        private float _currentVolume = 1f;
        private Coroutine _backgroundMusicRoutine;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ValidateReferences();
            audioClips ??= new Dictionary<int, AudioClip>();
            PreloadAudioClips();
        }

        private void Start() => ApplyAudioSettings();

        private void OnEnable() => SubscribeToEvents();

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            StopBackgroundMusicRoutine();
        }

        #endregion

        #region Private Methods

        private void SubscribeToEvents()
        {
            EventManager.AudioEvents.AudioStop += StopAudio;
            EventManager.AudioEvents.AudioPlay += PlayAudio;
            EventManager.AudioEvents.VolumeChange += SetAudioVolume;
            EventManager.AudioEvents.AudioChanged += FadeToNewClip;
            EventManager.AudioEvents.AudioAdded += AddAudioClip;
            EventManager.AudioEvents.AudioLoopToggleChanged += ToggleLooping;
            EventManager.AudioEvents.AudioEnabled += UpdateSettingsData;
            
            EventManager.InGameEvents.GameStarted += HandleGameStarted;

            EventManager.SolitaireEvents.DealStarted += PlayDealAudio;
            EventManager.SolitaireEvents.CardHoldStarted += PlayHoldAudio;
            EventManager.SolitaireEvents.WasteCardClicked += PlayHoldAudio;
            EventManager.SolitaireEvents.StockDrawClicked += PlayHoldAudio;
            EventManager.SolitaireEvents.CardDropSucceeded += PlayDropAudio;
            EventManager.SolitaireEvents.CardDropFailed += PlayDropFailedAudio;
            EventManager.SolitaireEvents.ScoreActionPerformed += HandleScoreAction;
            EventManager.InGameEvents.LevelSuccess += PlaySuccessAudio;
            EventManager.InGameEvents.LevelFail += PlayFailAudio;

            EventManager.SaveEvents.DataLoaded += ApplyAudioSettings;
        }
        
        private void UnsubscribeFromEvents()
        {
            EventManager.AudioEvents.AudioStop -= StopAudio;
            EventManager.AudioEvents.AudioPlay -= PlayAudio;
            EventManager.AudioEvents.VolumeChange -= SetAudioVolume;
            EventManager.AudioEvents.AudioChanged -= FadeToNewClip;
            EventManager.AudioEvents.AudioAdded -= AddAudioClip;
            EventManager.AudioEvents.AudioLoopToggleChanged -= ToggleLooping;
            EventManager.AudioEvents.AudioEnabled -= UpdateSettingsData;
            
            EventManager.InGameEvents.GameStarted -= HandleGameStarted;

            EventManager.SolitaireEvents.DealStarted -= PlayDealAudio;
            EventManager.SolitaireEvents.CardHoldStarted -= PlayHoldAudio;
            EventManager.SolitaireEvents.WasteCardClicked -= PlayHoldAudio;
            EventManager.SolitaireEvents.StockDrawClicked -= PlayHoldAudio;
            EventManager.SolitaireEvents.CardDropSucceeded -= PlayDropAudio;
            EventManager.SolitaireEvents.CardDropFailed -= PlayDropFailedAudio;
            EventManager.SolitaireEvents.ScoreActionPerformed -= HandleScoreAction;
            EventManager.InGameEvents.LevelSuccess -= PlaySuccessAudio;
            EventManager.InGameEvents.LevelFail -= PlayFailAudio;

            EventManager.SaveEvents.DataLoaded -= ApplyAudioSettings;
        }

        private void UpdateSettingsData(bool isEnabled)
        {
            if (settingsData == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} cannot update sound settings because SettingsDataSO is missing.");
                return;
            }

            settingsData.SetSoundEnabled(isEnabled);
        }

        private void HandleGameStarted() => ApplyAudioSettings();

        private void ApplyAudioSettings()
        {
            SetAudioVolume(IsSoundEnabled() ? 1f : 0f);
            StartBackgroundMusic();
        }

        private void StartBackgroundMusic()
        {
            if (backgroundMusicClip == null || audioSource == null)
                return;

            StopBackgroundMusicRoutine();
            _backgroundMusicRoutine = StartCoroutine(PlayBackgroundMusicWhenReady());
        }

        private IEnumerator PlayBackgroundMusicWhenReady()
        {
            if (backgroundMusicClip.loadState == AudioDataLoadState.Unloaded)
                backgroundMusicClip.LoadAudioData();

            while (backgroundMusicClip.loadState == AudioDataLoadState.Loading)
                yield return null;

            if (backgroundMusicClip.loadState != AudioDataLoadState.Loaded)
                yield break;

            if (audioSource.clip == backgroundMusicClip && audioSource.isPlaying && audioSource.loop)
                yield break;

            audioSource.clip = backgroundMusicClip;
            audioSource.loop = true;
            audioSource.volume = IsSoundEnabled() ? _currentVolume : 0f;
            audioSource.Play();
            TDebug.LogGreen("[AudioManager] Background music started.");
        }

        private void StopBackgroundMusicRoutine()
        {
            if (_backgroundMusicRoutine == null)
                return;

            StopCoroutine(_backgroundMusicRoutine);
            _backgroundMusicRoutine = null;
        }

        private void PlayDealAudio() => PlaySolitaireOneShot(dealAudioClip);

        private void PlayHoldAudio() => PlaySolitaireOneShot(holdAudioClip);

        private void PlayDropAudio() => PlaySolitaireOneShot(dropAudioClip);

        private void PlayDropFailedAudio() => PlaySolitaireOneShot(dropFailedAudioClip);

        private void PlaySuccessAudio() => PlaySolitaireOneShot(successAudioClip);

        private void PlayFoundationSuccessAudio() => PlaySolitaireOneShot(foundationSuccessAudioClip ?? successAudioClip);

        private void PlayFailAudio() => PlaySolitaireOneShot(failAudioClip);

        private void HandleScoreAction(SolitaireScoreAction action)
        {
            if (action == SolitaireScoreAction.MoveToFoundation)
                PlayFoundationSuccessAudio();
        }

        private void PlaySolitaireOneShot(AudioClip clip)
        {
            if (audioSource == null || clip == null || !IsSoundEnabled() || _currentVolume <= 0f)
                return;

            audioSource.PlayOneShot(clip, _currentVolume);
        }

        private void PreloadAudioClips()
        {
            LoadAudioClip(dealAudioClip);
            LoadAudioClip(holdAudioClip);
            LoadAudioClip(dropAudioClip);
            LoadAudioClip(dropFailedAudioClip);
            LoadAudioClip(successAudioClip);
            LoadAudioClip(foundationSuccessAudioClip);
            LoadAudioClip(failAudioClip);
            LoadAudioClip(backgroundMusicClip);
        }

        private static void LoadAudioClip(AudioClip clip)
        {
            if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
        }

        private void StopAudio()
        {
            if (audioSource == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} cannot stop audio because AudioSource is missing.");
                return;
            }

            audioSource.Stop();
        }

        private void PlayAudio(int audioClipId)
        {
            AudioClip audioClip = GetAudioClip(audioClipId);
            if (audioClip == null || audioSource == null)
            {
                return;
            }
            
            SetAudioClipToSource(audioClip);
            
            audioSource.PlayOneShot(audioClip);
        }
        
        private void SetAudioVolume(float initialVolume)
        {
            if (audioSource == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} cannot set volume because AudioSource is missing.");
                return;
            }

            _currentVolume = Mathf.Clamp01(initialVolume);
            audioSource.volume = IsSoundEnabled() ? _currentVolume : 0f;
        }

        private AudioClip GetAudioClip(int audioClipId)
        {
            if (audioClips == null || !audioClips.TryGetValue(audioClipId, out AudioClip audioClip))
            {
                TDebug.LogWarning($"{nameof(AudioManager)} has no AudioClip registered for id '{audioClipId}'.");
                return null;
            }

            return audioClip;
        }

        private void AddAudioClip(int audioClipId, AudioClip audioClip)
        {
            if (audioClip == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} cannot add a null AudioClip for id '{audioClipId}'.");
                return;
            }

            audioClips ??= new Dictionary<int, AudioClip>();
            audioClips[audioClipId] = audioClip;
        }
        
        private void SetAudioClipToSource(AudioClip audioClip) => audioSource.clip = audioClip;
        
        private void FadeToNewClip(int newAudioClipId, float fadeDuration)
        {
            AudioClip audioClip = GetAudioClip(newAudioClipId);
            if (audioClip == null || audioSource == null)
            {
                return;
            }

            StartCoroutine(CrossFadeAudioService.CrossFadeCoroutine(audioClip, fadeDuration, audioSource));
        }

        private void ToggleLooping(bool shouldLoop)
        {
            if (audioSource == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} cannot toggle loop because AudioSource is missing.");
                return;
            }

            audioSource.loop = shouldLoop;
        }

        private bool IsSoundEnabled()
        {
            if (settingsData == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} requires SettingsDataSO to resolve sound state.");
                return false;
            }

            return settingsData.IsSoundEnabled;
        }

        private void ValidateReferences()
        {
            if (audioSource == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} requires an AudioSource.");
            }

            if (settingsData == null)
            {
                TDebug.LogWarning($"{nameof(AudioManager)} requires SettingsDataSO.");
            }

            if (dealAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a deal AudioClip.");

            if (holdAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a hold AudioClip.");

            if (dropAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a drop AudioClip.");

            if (dropFailedAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a drop failed AudioClip.");

            if (successAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a success AudioClip.");

            if (failAudioClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a fail AudioClip.");

            if (backgroundMusicClip == null)
                TDebug.LogWarning($"{nameof(AudioManager)} requires a background music AudioClip.");
        }

        #endregion
    }
}
