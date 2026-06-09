using UnityEngine;
using UnityEngine.UI;

namespace Handler.Extensions
{
    public static class HandlerGraphic
    {
        public static void SetColorRed(this Graphic self, float value)
        {
            self.color = new Color(value, self.color.g, self.color.b, self.color.a);
        }

        public static void SetColorGreen(this Graphic self, float value)
        {
            self.color = new Color(self.color.r, value, self.color.b, self.color.a);
        }

        public static void SetColorBlue(this Graphic self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, value, self.color.b);
        }

        public static void SetColorAlpha(this Graphic self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, self.color.b, value);
        }
        
        public static void SetColor(this Graphic self, float r, float g, float b, float a)
        {
            self.color = new Color(r, g, b, a);
        }
    }
}