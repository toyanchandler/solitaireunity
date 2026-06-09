using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerSpriteRenderer
    {
        #region Color Extensions
        
        public static void SetColor(this SpriteRenderer self, Color color)
        {
            self.color = color;
        }

        public static void SetColorRed(this SpriteRenderer self, float value)
        {
            self.color = new Color(value, self.color.g, self.color.b, self.color.a);
        }

        public static void SetColorGreen(this SpriteRenderer self, float value)
        {
            self.color = new Color(self.color.r, value, self.color.b, self.color.a);
        }

        public static void SetColorBlue(this SpriteRenderer self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, value, self.color.b);
        }

        public static void SetColorAlpha(this SpriteRenderer self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, self.color.b, value);
        }

        #endregion

        #region Sprite Extensions

        public static void SetSprite(this SpriteRenderer self, Sprite sprite)
        {
            self.sprite = sprite;
        }
        
        public static void SetFlipX(this SpriteRenderer self, bool value)
        {
            self.flipX = value;
        }
        
        public static void SetFlipY(this SpriteRenderer self, bool value)
        {
            self.flipY = value;
        }
        
        public static void SetOrderInLayer(this SpriteRenderer self, int value)
        {
            self.sortingOrder = value;
        }

        #endregion
        
    }
}