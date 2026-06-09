using UnityEngine;

namespace Handler.Extensions
{
    public static class HandlerMaterial
    {
        public static void SetColorRChannel(this Material self, float value)
        {
            self.color = new Color(value, self.color.g, self.color.b, self.color.a);
        }

        public static void SetColorGChannel(this Material self, float value)
        {
            self.color = new Color(self.color.r, value, self.color.b, self.color.a);
        }

        public static void SetColorBChannel(this Material self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, value, self.color.b);
        }

        public static void SetColorAlphaChannel(this Material self, float value)
        {
            self.color = new Color(self.color.r, self.color.g, self.color.b, value);
        }

        public static void SetMaterialColor(this Material self, float r, float g, float b, float a)
        {
            self.color = new Color(r, g, b, a);
        }
        
        public static void SetIdenticalColor(this Material self, Renderer renderer)
        {
            self.SetMaterialColor(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, renderer.material.color.a);
        }
        
        public static void CreateMaterialInstanceAndChangeColor(this Material self, Color color)
        {
            var materialInstance = new Material(self);
            materialInstance.SetMaterialColor(color.r, color.g, color.b, color.a);
        }
    }
}