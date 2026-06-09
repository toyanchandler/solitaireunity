using _Game.Scripts.Managers.Core;
using _Game.Scripts.ScriptableObjects.Saveable;
using _Game.Scripts.Template.GlobalProviders.Interactable;
using _Game.Scripts.Template.GlobalProviders.Interactable.Collectables;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Texts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CollectableCounterText : MonoBehaviour
    {
        [SerializeField]
        private CollectableType textAssigner;

        [SerializeField]
        private CollectableValuesSO collectableValuesSO;

        private TextMeshProUGUI textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            
        }
        
        private void OnEnable()
        {
            EventManager.CollectableEvents.UICollectAnimation += UpdateCounterText;
            EventManager.CurrencySystem.CollectableSpent += UpdateText;
            UpdateText(textAssigner);
        }

        private void OnDisable()
        {
            EventManager.CollectableEvents.UICollectAnimation -= UpdateCounterText;
            EventManager.CurrencySystem.CollectableSpent -= UpdateText;
        }
        
        private void UpdateText(CollectableType collectableType)
        {
            if (collectableType == textAssigner)
            {
                int value = collectableValuesSO.GetValue(collectableType);
            
                textMesh.text = value.ToString();
            }
        }

        private void UpdateCounterText(CollectableData collectableData)
        {
            if (collectableData.CollectableType == textAssigner)
            {
                int value = collectableValuesSO.GetValue(collectableData.CollectableType);
            
                textMesh.text = value.ToString();
            }
        }
    }
}
