using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Flui.Bootstrap
{
    internal class ColorPack
    {
        public ColorPack(params SubColor[] subColors)
        {
            SubColors = subColors.ToList();
        }

        private List<SubColor> SubColors { get; set; }

        public static ColorPack Create(string inNormal)
        {
            var normal = new SimpleColor(inNormal);
            return Create(normal);
        }

        public static ColorPack Create(SimpleColor normal)
        {
            return new ColorPack(
                new SubColor("normal", normal),
                new SubColor("hovered", HslColor.ScaleHsl(normal, 1, 1, 0.85f)),
                new SubColor("clicked", HslColor.ScaleHsl(normal, 1, 1, 0.7f)),
                new SubColor("border", HslColor.ScaleHsl(normal, 1, 1, 1.6f)),
                new SubColor("disabled", HslColor.ScaleHsl(normal, 1, 1, 1.5f)),
                new SubColor("disabled-border", HslColor.ScaleHsl(normal, 1, 1, 1.71f)));
        }

        public string ToVariableString(string prefix)
        {
            var sb = new StringBuilder();
            foreach (var subColor in SubColors)
            {
                sb.AppendLine($"    {prefix}-{subColor.Name}: {subColor.SimpleColor};");
            }

            return sb.ToString();
        }

        public ColorPack GenerateColorPack(string newNormal)
        {
            var normal = new SimpleColor(newNormal);
            var oldNormal = SubColors[0].SimpleColor;
            var cols = SubColors.Select(x => new SubColor(x.Name, ComputeNewColor(x.SimpleColor))).ToArray();
            return new ColorPack(cols);

            SimpleColor ComputeNewColor(SimpleColor other)
            {
                var r = (byte)Mathf.Clamp((float)other.R / oldNormal.R * normal.R, 0, 255);
                var g = (byte)Mathf.Clamp((float)other.G / oldNormal.G * normal.G, 0, 255);
                var b = (byte)Mathf.Clamp((float)other.B / oldNormal.B * normal.B, 0, 255);

                return SimpleColor.FromArgb(r, g, b);
            }
        }

        internal static SimpleColor ScaleColor(SimpleColor simpleColor, float scaleFactor)
        {
            return SimpleColor.FromArgb(
                (byte)(simpleColor.R * scaleFactor),
                (byte)(simpleColor.G * scaleFactor),
                (byte)(simpleColor.B * scaleFactor));
        }

        internal class SubColor
        {
            public string Name { get; }
            public SimpleColor SimpleColor { get; }

            public SubColor(string name, SimpleColor simpleColor)
            {
                Name = name;
                SimpleColor = simpleColor;
            }

            public SubColor(string name, string color)
            {
                Name = name;
                SimpleColor = new SimpleColor(color);
            }
        }
    }
}