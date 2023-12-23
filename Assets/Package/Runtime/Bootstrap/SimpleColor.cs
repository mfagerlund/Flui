using UnityEngine;

namespace Flui.Bootstrap
{
    public class SimpleColor
    {
        public SimpleColor(int red, int green, int blue)
        {
            R = red;
            G = green;
            B = blue;
        }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color ToColor() => new(R / 255f, G / 255f, B / 255f);

        public static SimpleColor FromArgb(int r, int g, int b) => new(r, g, b);

        public SimpleColor(string hex) => HexToColor(hex);

        public SimpleColor(Color color)
        {
            R = (int)(color.r * 255);
            G = (int)(color.g * 255);
            B = (int)(color.b * 255);
        }

        public override string ToString() =>
            "#" 
            + Mathf.Clamp(R,0,255).ToString("X2") 
            + Mathf.Clamp(G,0,255).ToString("X2") 
            + Mathf.Clamp(B,0,255).ToString("X2");

        private void HexToColor(string hex)
        {
            // Remove the '#' character if it's there
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Parse the string into an integer
            int intVal = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            // Extract the individual channels
            R = (byte)((intVal >> 16) & 255);
            G = (byte)((intVal >> 8) & 255);
            B = (byte)(intVal & 255);
        }

        public float GetBrightness()
        {
            MinMaxRgb(out int min, out int max, R, G, B);
            return (max + min) / (byte.MaxValue * 2f);
        }

        public float GetHue()
        {
            if (R == G && G == B)
                return 0f;

            MinMaxRgb(out int min, out int max, R, G, B);

            float delta = max - min;
            float hue;

            if (R == max)
                hue = (G - B) / delta;
            else if (G == max)
                hue = (B - R) / delta + 2f;
            else
                hue = (R - G) / delta + 4f;

            hue *= 60f;
            if (hue < 0f)
                hue += 360f;

            return hue;
        }

        public float GetSaturation()
        {
            if (R == G && G == B)
                return 0f;

            MinMaxRgb(out int min, out int max, R, G, B);

            int div = max + min;
            if (div > byte.MaxValue)
                div = byte.MaxValue * 2 - max - min;

            return (max - min) / (float)div;
        }

        private static void MinMaxRgb(out int min, out int max, int r, int g, int b)
        {
            if (r > g)
            {
                max = r;
                min = g;
            }
            else
            {
                max = g;
                min = r;
            }

            if (b > max)
            {
                max = b;
            }
            else if (b < min)
            {
                min = b;
            }
        }
    }
}