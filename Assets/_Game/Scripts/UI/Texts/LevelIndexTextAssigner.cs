using _Game.Scripts.ScriptableObjects.Saveable;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Texts
{
    public class LevelIndexTextAssigner : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _textMeshProUGUI;
        [SerializeField] private PlayerSaveableData playerSaveableData;
        private void Awake()
        {
            if(_textMeshProUGUI==null)
                _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }
        private void OnEnable()
        {
            _textMeshProUGUI.text = "Lv " + playerSaveableData.LevelIndex;
        }
   
    }
}
