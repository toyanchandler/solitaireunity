using System;
using _Game.Scripts.Template.GlobalProviders.Interactable.Gate;

#region Usings
using UnityEngine;
#endregion

namespace Handler.Extensions
{
    public static class MathExtension
    {
        #region Public Methods
        public static int Min(int a, int b) => a > b ? b : a;
        public static int Max(int a, int b) => a > b ? a : b;
        
        public static int IsEven(this int i) => i % 2 == 0 ? i : i + 1;
        
        public static int IsOdd(this int i) => i % 2 != 0 ? i : i + 1;
        
        public static float IsEven(this float f) => f % 2 == 0 ? f : f + 1;
        
        public static float IsOdd(this float f) => f % 2 != 0 ? f : f + 1;
        
        public static int Clamp(this int value, int min, int max) => Min(Max(value, min), max);
        
        public static void FloatRoundTo(this ref float value, float roundTo) => value = Mathf.Round(value / roundTo) * roundTo;
        
        public static void IntRoundTo(this ref int value, int roundTo) => value = Mathf.RoundToInt(value / (float)roundTo) * roundTo;
        
        public static bool IntIsInRange(this int i, int min, int max) => i >= min && i <= max;
        
        public static bool FloatIsInRange(this float f, float min, float max) => f >= min && f <= max;
        
        public static int Map(this int value, int from1, int to1, int from2, int to2) => (int)Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, value));
        
        public static float Square(this float value) => value * value;
        
        public static float Cube(this float value) => value * value * value;
        
        public static bool IsNearlyEqual(this float a, float b, float epsilon = 0.001f) => Mathf.Abs(a - b) < epsilon;
        
        public static bool IsPositive(this int i) => i > 0;
        
        public static bool IsNegative(this int i) => i < 0;
        
        public static float Percent(this float value, float percent) => value * percent * 0.01f;
        
        public static float InversePercent(this float value, float maxValue) => (value / maxValue) * 100f;
        
        public static float ModifyValue(this float value, int amount, MathType mathType) => mathType switch
        {
            MathType.Add => value + amount,
            MathType.Subtract => value - amount,
            MathType.Multiply => value * amount,
            MathType.Divide => value / amount,
            _ => value
        };

        #endregion
    }
}