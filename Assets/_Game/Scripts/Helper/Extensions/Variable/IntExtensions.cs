using UnityEngine;

namespace Handler.Extensions
{
    public static class IntExtensions
    {
        public static bool IsZero(this int i) => i == 0;
        
        public static bool ToBool(this int value) => value == 1;
        
        public static float ToFloat(this int value) => value;
        
        public static string ToString(this int value) => value.ToString();
        
        public static int Clamp(this int value, int min, int max) => Mathf.Clamp(value, min, max);
        
        public static int Clamp01(this int value) => Mathf.Clamp(value, 0, 1);
        
        public static int Lerp(this int value, int min, int max) => (int)Mathf.Lerp(min, max, value);
        
        public static bool IsGreaterThan(this int value, int other) => value > other;
        
        public static bool IsSmallerThan(this int value, int other) => value < other;
        
        public static bool IsEqualTo(this int value, int other) => value == other;
    }
}