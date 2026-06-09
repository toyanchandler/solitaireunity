using System;
using System.Collections.Generic;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Project.SolitaireModule.Controllers
{
    public sealed class SolitaireDebugScenarioPanel : MonoBehaviour
    {
        private const float ScenarioButtonStartY = -330f;
        private const float ScenarioButtonHeight = 72f;
        private const float ScenarioButtonSpacing = 12f;
        private const float ScenarioPanelHeight = 1180f;

        [SerializeField] private SolitaireModuleBootstrap moduleBootstrap;
        [SerializeField] private Button debugToggleButton;
        [SerializeField] private GameObject scenarioPanelRoot;
        [SerializeField] private Button[] scenarioButtons = Array.Empty<Button>();
        [SerializeField] private TextMeshProUGUI selectedScenarioLabel;
        [SerializeField] private TextMeshProUGUI panelTitleLabel;

        private bool _isPanelOpen;

        private void Awake()
        {
            ResolveModuleBootstrap();
            ApplyLayoutFixes();
            EnsureScenarioButtons();
            WireButtons();
            SetPanelOpen(false);
            RefreshSelectedLabel(SolitaireDebugScenarioId.None);
        }

        private void ResolveModuleBootstrap()
        {
            if (moduleBootstrap != null)
                return;

            moduleBootstrap = FindFirstObjectByType<SolitaireModuleBootstrap>();

            if (moduleBootstrap == null)
                Debug.LogError("[SolitaireDebugScenarioPanel] SolitaireModuleBootstrap was not found in the scene.");
        }

        private void ApplyLayoutFixes()
        {


            if (panelTitleLabel != null)
            {
                RectTransform titleRect = panelTitleLabel.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.offsetMin = new Vector2(24f, -108f);
                titleRect.offsetMax = new Vector2(-24f, -36f);
            }

            if (scenarioPanelRoot != null)
            {
                RectTransform panelRect = scenarioPanelRoot.GetComponent<RectTransform>();
                if (panelRect != null && panelRect.sizeDelta.y < ScenarioPanelHeight)
                    panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, ScenarioPanelHeight);
            }
        }

        private void EnsureScenarioButtons()
        {
            if (scenarioPanelRoot == null)
                return;

            int requiredCount = SolitaireDebugScenarioApplier.OrderedScenarios.Length;
            var buttons = new List<Button>(scenarioButtons ?? Array.Empty<Button>());
            Button template = null;

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == null)
                    continue;

                template = buttons[i];
                break;
            }

            if (template == null)
                return;

            Transform panelTransform = scenarioPanelRoot.transform;

            for (int i = buttons.Count; i < requiredCount; i++)
            {
                float y = ScenarioButtonStartY - (i * (ScenarioButtonHeight + ScenarioButtonSpacing));
                Button button = Instantiate(template, panelTransform);
                button.gameObject.name = $"ScenarioButton_{i + 1:00}";

                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0f, y);
                buttons.Add(button);
            }

            scenarioButtons = buttons.ToArray();
            RefreshScenarioButtonLabels();
        }

        private void RefreshScenarioButtonLabels()
        {
            for (int i = 0; i < scenarioButtons.Length; i++)
            {
                if ((uint)i >= (uint)SolitaireDebugScenarioApplier.OrderedScenarios.Length)
                    break;

                Button button = scenarioButtons[i];
                if (button == null)
                    continue;

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label == null)
                    continue;

                label.text = SolitaireDebugScenarioApplier.GetButtonLabel(
                    SolitaireDebugScenarioApplier.OrderedScenarios[i]);
            }
        }

        private void WireButtons()
        {
            if (debugToggleButton != null)
            {
                debugToggleButton.onClick.RemoveListener(ToggleScenarioPanel);
                debugToggleButton.onClick.AddListener(ToggleScenarioPanel);
            }

            for (int i = 0; i < scenarioButtons.Length; i++)
            {
                int scenarioIndex = i;
                Button button = scenarioButtons[i];

                if (button == null)
                    continue;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectScenario(scenarioIndex));
            }
        }

        public void ToggleScenarioPanel()
        {
            SetPanelOpen(!_isPanelOpen);
        }

        public void SelectScenario(int scenarioIndex)
        {
            if ((uint)scenarioIndex >= (uint)SolitaireDebugScenarioApplier.OrderedScenarios.Length)
                return;

            SolitaireDebugScenarioId scenarioId = SolitaireDebugScenarioApplier.OrderedScenarios[scenarioIndex];

            if (moduleBootstrap == null)
            {
                Debug.LogError("[SolitaireDebugScenarioPanel] Missing SolitaireModuleBootstrap reference.");
                return;
            }

            moduleBootstrap.ApplyDebugScenarioInPlayMode(scenarioId);
            Debug.Log($"[SolitaireDebugScenarioPanel] temp — {SolitaireDebugScenarioApplier.GetButtonLabel(scenarioId)} uygulandı ({scenarioId}).");
            RefreshSelectedLabel(scenarioId);
            SetPanelOpen(false);
        }

        private void SetPanelOpen(bool isOpen)
        {
            _isPanelOpen = isOpen;

            if (scenarioPanelRoot != null)
                scenarioPanelRoot.SetActive(isOpen);
        }

        private void RefreshSelectedLabel(SolitaireDebugScenarioId scenarioId)
        {
            if (panelTitleLabel != null)
                panelTitleLabel.text = "Solitaire Debug Senaryoları";

            if (selectedScenarioLabel == null)
                return;

            if (scenarioId == SolitaireDebugScenarioId.None)
            {
                selectedScenarioLabel.text = "Seçili senaryo yok";
                return;
            }

            selectedScenarioLabel.text =
                $"Seçili: {SolitaireDebugScenarioApplier.GetButtonLabel(scenarioId)}\n{SolitaireDebugScenarioApplier.GetInstructions(scenarioId)}";
        }
    }
}
