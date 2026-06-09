using UnityEngine;

namespace Handler.Extensions
{
    public static class FloatExtensions
    {
        public static bool IsZero(this float f) => Mathf.Approximately(f, 0f);
        
        public static int ToInt(this float _arg) => Mathf.RoundToInt(_arg);
        
        public static bool ToBool(this float value) => value == 1;
        
        public static Vector3 ToVector3(this float input) => Vector3.one * input;
        
        public static Vector2 ToVector2(this float input) => Vector2.one * input;
        
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
        
        public static float Clamp01(this float value) => Mathf.Clamp01(value);
        
        public static float Lerp(this float value, float min, float max) => Mathf.Lerp(min, max, value);
        
        public static bool IsGreaterThan(this float value, float other) => value > other;
        
        public static bool IsSmallerThan(this float value, float other) => value < other;
    }
}