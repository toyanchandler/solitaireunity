using UnityEngine;

namespace Handler.Extensions
{
    public static class Vector2Extensions
    {
        #region Vector2 Value Extensions
        
        public static bool IsZero(this Vector2 self) => self == Vector2.zero;
        
        public static Vector2 DirectionTo(this Vector3 source, Vector3 destination) => (destination - source).normalized;

        public static Vector2 Divide(this Vector3 source, Vector3 dividedWith) => new Vector2(source.x / dividedWith.x, source.y / dividedWith.y);

        public static string Vector3ToString(this Vector3 self) => $"{self.x}~{self.y}~{self.z}";

        public static Vector2 Half => new Vector2(0.5f, 0.5f);

        public static Vector2 Quarter => new Vector2(0.25f, 0.25f);
        
        public static Vector2 SetX(this Vector2 self, float x) => new(x, self.y);
        
        public static Vector2 SetY(this Vector2 self, float y) => new(self.x, y);
        
        public static Vector2 Set(this Vector2 self, float? x = null, float? y = null) => new(x ?? self.x, y ?? self.y);
        
        public static Vector2 Flat(this Vector2 self, float yValue = 0) => new(self.x, yValue);
        
        public static Vector2Int ToVector2Int(this Vector2 self) => new((int)self.x, (int)self.y);
        
        public static Vector3 ToVector3(this Vector2 self) => new(self.x, self.y, 0);
        
        public static Vector2 GetMidPoint(this Vector2 start, Vector2 end)
        {
            Vector2 toTarget = end - start;
            float midDist = toTarget.magnitude * 0.5f;
            return start + toTarget.normalized * midDist;
        }
        
        public static float DotProduct(this Vector2 vector, Vector2 target)
        {
            return Vector2.Dot(vector, target);
        }
        
        public static bool HasPassed(this Vector2 start, Vector2 end, Vector2 moveDirection)
        {
            var toPointDirection = end - start;
            return Vector2.Dot(toPointDirection.normalized, moveDirection.normalized) < 0.0f;
        }
        
        public static Vector2 GetPointWithNormalizedDistance(this Vector2 pt, float normalizedDistance, Vector2 target)
        {
            var toTarget = target - pt;
            var dist = toTarget.magnitude * normalizedDistance;
            return pt + toTarget.normalized * dist;
        }
        
        public static Vector2 GetPointWithDistance(this Vector2 pt, float distance, Vector2 target)
        {
            var toTarget = target - pt;
            return pt + toTarget.normalized * distance;
        }
        
        #endregion
    }
}