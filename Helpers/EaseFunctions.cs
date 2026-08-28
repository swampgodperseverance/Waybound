using System;

namespace Waybound.Helpers
{
    public static class EaseFunctions
    {
        public static float EaseIn(float t) => t * t;
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2;
        
        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseOutCubic(float t) => 1 - (float)Math.Pow(1 - t, 3);
        public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
        
        public static float EaseInQuint(float t) => (float)Math.Pow(t, 5);
        public static float EaseOutQuint(float t) => 1 - (float)Math.Pow(1 - t, 5);
        public static float EaseInOutQuint(float t) => t < 0.5f ? 16 * (float)Math.Pow(t, 5) : 1 - (float)Math.Pow(-2 * t + 2, 5) / 2;

        public static float EaseInSine(float t) => 1 - (float)Math.Cos((t * Math.PI) / 2);
        public static float EaseOutSine(float t) => (float)Math.Sin((t * Math.PI) / 2);
        public static float EaseInOutSine(float t) => -(float)(Math.Cos(Math.PI * t) - 1) / 2;

        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1 + c3 * (float)Math.Pow(t - 1, 3) + c1 * (float)Math.Pow(t - 1, 2);
        }

        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f
                ? (float)(Math.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                : (float)(Math.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        }

        public delegate float Easing(float t);

        public static readonly System.Collections.Generic.Dictionary<string, Easing> Functions = new() {
            ["EaseIn"] = EaseIn,
            ["EaseOutQuad"] = EaseOutQuad,
            ["EaseInOutQuad"] = EaseInOutQuad,
            ["EaseInCubic"] = EaseInCubic,
            ["EaseOutCubic"] = EaseOutCubic,
            ["EaseInOutCubic"] = EaseInOutCubic,
            ["EaseInQuint"] = EaseInQuint,
            ["EaseOutQuint"] = EaseOutQuint,
            ["EaseInOutQuint"] = EaseInOutQuint,
            ["EaseInSine"] = EaseInSine,
            ["EaseOutSine"] = EaseOutSine,
            ["EaseInOutSine"] = EaseInOutSine,
            ["EaseInBack"] = EaseInBack,
            ["EaseOutBack"] = EaseOutBack,
            ["EaseInOutBack"] = EaseInOutBack
        };
    }
}
