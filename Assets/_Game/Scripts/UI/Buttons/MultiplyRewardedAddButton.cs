using System;
using _Game.Scripts.Helper.Services;
using _Game.Scripts.Managers.Core;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using UnityEngine;

namespace _Game.Scripts.UI.Buttons
{
    public class MultiplyRewardedAddButton : ButtonBase
    {
        [SerializeField] private MultiplyRewardedAdData multiplyRewardedAdData;
        [SerializeField] private GameObject buttonToOpen;
        [SerializeField] private float secondsToWait = 3f;
        private CoroutineService _coroutineService;

        protected override void OnClicked()
        {
            EventManager.AdEvents.RewardedShow?.Invoke(OnRewardedAddSuccessful);
        }

        private void OnEnable()
        {
            _coroutineService = new CoroutineService(this);

            WaitForSecondsToWaitAndActivateButtonToOpen();
        }

        private void WaitForSecondsToWaitAndActivateButtonToOpen()
        {
            _coroutineService.StartDelayedRoutine(ActivateButtonToOpen, secondsToWait);
            buttonToOpen.SetActive(false);
        }

        private void ActivateButtonToOpen()
        {
            buttonToOpen.SetActive(true);
        }
        
        private void OnRewardedAddSuccessful()
        {
            EventManager.CurrencySystem.MultiplyRewardedAd?.Invoke(new MultiplyRewardedAdData
            {
                CollectableType = multiplyRewardedAdData.CollectableType,
                MultiplyCount = multiplyRewardedAdData.MultiplyCount
            });


            //TODO : CLAIM
            Invoke("NextLevelMethod", 3.0f);
        }

        //TODO: CLAIM BUTTON
        private void NextLevelMethod()
        {
            buttonToOpen.GetComponent<NextLevelButton>().HandleClick();
        }
    }

    [Serializable]
    public struct MultiplyRewardedAdData
    {
        public int MultiplyCount;
        public CollectableType CollectableType;
    }
}