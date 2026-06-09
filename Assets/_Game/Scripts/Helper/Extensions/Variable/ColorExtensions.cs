using UnityEngine;

namespace Handler.Extensions
{
    public static class ColorExtensions
    {
        /// Returns Hex code with # of the Color.
        public static string ToHex(this Color self) => "#" + ColorUtility.ToHtmlStringRGB(self);
        /// Returns new Color with same RGBA parameters after changing given r,g,b,a values.
        /// 
        public static Color With(this Color self, float? r = null, float? g = null, float? b = null, float? a = null) => new(r ?? self.r, g ?? self.g, b ?? self.b, a ?? self.a);
        /// Returns a new Color with same gba values with given r value.
        /// 
        public static Color WithR(this Color self, float r) => new Color(r, self.g, self.b, self.a);
        /// Returns a new Color with same gba values with given r value.
        /// 
        public static Color WithG(this Color self, float g) => new Color(self.r, g, self.b, self.a);
        /// Returns a new Color with same gba values with given r value.
        /// 
        public static Color WithB(this Color self, float b) => new Color(self.r, self.g, b, self.a);
        /// Returns a new Color with same gba values with given r value.
        /// 
        public static Color WithA(this Color self, float a) => new Color(self.r, self.g, self.b, a);
        /// Gets a random color.
        /// 
        public static Color random => new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        /// Returns a new Color with same rgba values with given r value.
        /// 
        public static Color With01(this Color self) => new Color(self.r/255f,self.g/255f,self.b/255f,self.a/255f);
        
        public static Color With01Alpha(this Color self, float alpha) => new Color(self.r,self.g,self.b,255 / alpha);
    }
}