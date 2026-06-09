using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Template.GlobalProviders.Interactable.Gate
{
    public sealed class GateView : MonoBehaviour
    {
        private static readonly Dictionary<MathType, string> GateSymbols = new()
        {
            { MathType.Divide, "÷" },
            { MathType.Multiply, "x" },
            { MathType.Subtract, "-" },
            { MathType.Add, "+" }
        };

        private static readonly Dictionary<MathType, Color> GateColors = new()
        {
            { MathType.Divide, Color.red },
            { MathType.Multiply, Color.green },
            { MathType.Subtract, Color.red },
            { MathType.Add, Color.green }
        };

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private const float GateAlpha = 80f / 255f;

        [SerializeField] private TextMeshPro textMeshPro;
        [SerializeField] private MeshRenderer meshRenderer;

        private MaterialPropertyBlock materialPropertyBlock;

        public void Configure(TextMeshPro fallbackText, MeshRenderer fallbackRenderer)
        {
            if (textMeshPro == null) textMeshPro = fallbackText;
            if (meshRenderer == null) meshRenderer = fallbackRenderer;
            materialPropertyBlock ??= new MaterialPropertyBlock();
        }

        public void Refresh(GateInteractableData data)
        {
            SetText(data);
            SetColor(data);
        }

        private void SetText(GateInteractableData data)
        {
            if (textMeshPro == null) return;

            if (!GateSymbols.TryGetValue(data.mathType, out var symbol))
                throw new ArgumentOutOfRangeException(nameof(data.mathType), data.mathType, null);

            textMeshPro.text = symbol + data.Amount;
        }

        private void SetColor(GateInteractableData data)
        {
            if (meshRenderer == null) return;

            if (!GateColors.TryGetValue(data.mathType, out var color))
                throw new ArgumentOutOfRangeException(nameof(data.mathType), data.mathType, null);

            materialPropertyBlock ??= new MaterialPropertyBlock();
            var runtimeColor = new Color(color.r, color.g, color.b, GateAlpha);
            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor(ColorId, runtimeColor);
            materialPropertyBlock.SetColor(BaseColorId, runtimeColor);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
