namespace Handler.Extensions
{
    public static class BoolExtensions
    {
        public static void ToInt(this bool self, out int value) => value = self ? 1 : 0;
        
        public static void ToFloat(this bool self, out float value) => value = self ? 1f : 0f;
        
        public static void Reverse(this bool self, out bool value) => value = !self;
    }
}