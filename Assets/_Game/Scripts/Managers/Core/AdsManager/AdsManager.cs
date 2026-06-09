using System;
using _Game.Scripts.Helper.Extensions.System;
using _Game.Scripts.Managers.AdsManager;
using Handler.Extensions;
using UnityEngine;

namespace _Game.Scripts.Managers.Core
{
    public sealed class AdsManager : MonoBehaviour
    {
        private IAdService _adService;

        private void Awake() => CreateInstanceAdService();

        private void OnEnable()
        {
            EventManager.AdEvents.RewardedShow += ShowRewardedAd;
            EventManager.AdEvents.InterstitialReward += ShowInterstitialAd;
        }
        
        private void OnDisable()
        {
            EventManager.AdEvents.RewardedShow -= ShowRewardedAd;
            EventManager.AdEvents.InterstitialReward -= ShowInterstitialAd;
        }
        
        private void CreateInstanceAdService()
        {
            _adService = new UnityAdsService();
        }
        
        private void ShowRewardedAd(Action callback)
        {
            if (_adService == null)
            {
                TDebug.LogWarning("Ad Service not initialized.");
                return;
            }

            _adService.ShowRewardedAd(() =>
            {
                TDebug.Log("Ad shown successfully.");
                callback?.Invoke();
            });
        }

        private void ShowInterstitialAd()
        {
            if (_adService == null)
            {
                TDebug.LogWarning("Ad Service not initialized.");
                return;
            }

            _adService.ShowInterstitialAd();
        }
    }
}
