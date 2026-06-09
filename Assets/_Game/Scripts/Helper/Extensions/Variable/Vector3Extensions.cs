using UnityEngine;

namespace Handler.Extensions
{
    public static class Vector3Extensions
    {
        #region Vector3 Value Extensions
        
        public static Vector3 SetX(this Vector3 self, float x) => new(x, self.y, self.z);
        
        public static Vector3 SetY(this Vector3 self, float y) => new(self.x, y, self.z);
        
        public static Vector3 SetZ(this Vector3 self, float z) => new(self.x, self.y, z);
        
        public static Vector3 Set(this Vector3 self, float? x = null, float? y = null, float? z = null) => new(x ?? self.x, y ?? self.y, z ?? self.z);
        
        public static Vector3 Flat(this Vector3 self, float yValue = 0) => new(self.x, yValue, self.z);
        
        public static Vector3Int ToVector3Int(this Vector3 self) => new((int)self.x, (int)self.y, (int)self.z);
        
        public static Vector2 ToVector2(this Vector3 self) => new(self.x, self.y);
        
        public static Vector3 DirectionTo(this Vector3 source, Vector3 destination) => Vector3.Normalize(destination - source);
        
        public static string Vector3ToString(this Vector3 self) => $"{self.x}~{self.y}~{self.z}";
        
        public static Vector3 Divide(this Vector3 source, Vector3 dividedWith) => new(source.x / dividedWith.x,
            source.y / dividedWith.y, source.z / dividedWith.z);
        
        public static Vector3 Half => new(0.5f, 0.5f, 0.5f);
        
        public static Vector3 Quarter => new(0.25f, 0.25f, 0.25f);
        
        public static bool IsZero(this Vector3 self) => self == Vector3.zero;
        
        public static Vector3 GetMidPoint(this Vector3 start, Vector3 end)
        {
            Vector3 toTarget = end - start;
            float midDist = toTarget.magnitude * 0.5f;
            return start + toTarget.normalized * midDist;
        }
        
        public static float DotProduct(this Vector3 vector, Vector3 target)
        {
            return Vector3.Dot(vector, target);
        }
        
        public static bool HasPassed(this Vector3 start, Vector3 end, Vector3 moveDirection)
        {
            var toPointDirection = end - start;
            return Vector3.Dot(toPointDirection.normalized, moveDirection.normalized) < 0.0f;
        }
        
        public static Vector3 GetPointWithNormalizedDistance(this Vector3 pt, float normalizedDistance, Vector3 target)
        {
            var toTarget = target - pt;
            var dist = toTarget.magnitude * normalizedDistance;
            return pt + toTarget.normalized * dist;
        }
        
        public static Vector3 GetPointWithDistance(this Vector3 pt, float distance, Vector3 target)
        {
            var toTarget = target - pt;
            return pt + toTarget.normalized * distance;
        }
        
        #endregion
    }
}