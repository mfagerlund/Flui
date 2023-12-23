namespace Flui.Bootstrap
{
    public class HslColor
    {
        // https://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double _hue = 1.0;
        private double _saturation = 1.0;
        private double _luminosity = 1.0;

        private const double scale = 240.0;

        public double Hue
        {
            get => _hue * scale;
            set => _hue = CheckRange(value / scale);
        }

        public double Saturation
        {
            get => _saturation * scale;
            set => _saturation = CheckRange(value / scale);
        }

        public double Luminosity
        {
            get => _luminosity * scale;
            set => _luminosity = CheckRange(value / scale);
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString() => $"H: {Hue:#0.##} S: {Saturation:#0.##} L: {Luminosity:#0.##}";

        public string ToRGBString()
        {
            SimpleColor simpleColor = (SimpleColor)this;
            return $"R: {simpleColor.R:#0.##} G: {simpleColor.G:#0.##} B: {simpleColor.B:#0.##}";
        }

        #region Casts to/from System.Drawing.Color

        public SimpleColor ToColor()
        {
            double r = 0, g = 0, b = 0;
            if (_luminosity != 0)
            {
                if (_saturation == 0)
                    r = g = b = _luminosity;
                else
                {
                    double temp2 = GetTemp2(this);
                    double temp1 = 2.0 * _luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, _hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, _hue);
                    b = GetColorComponent(temp1, temp2, _hue - 1.0 / 3.0);
                }
            }

            return SimpleColor.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }


        public static implicit operator SimpleColor(HslColor hslColor) => hslColor.ToColor();

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            if (temp3 < 0.5)
                return temp2;
            if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            return temp1;
        }

        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }

        private static double GetTemp2(HslColor hslColor)
        {
            double temp2;
            if (hslColor._luminosity < 0.5) //<=??
                temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            else
                temp2 = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            return temp2;
        }

        public static implicit operator HslColor(SimpleColor simpleColor)
        {
            HslColor hslColor = new HslColor();
            hslColor._hue = simpleColor.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor._luminosity = simpleColor.GetBrightness();
            hslColor._saturation = simpleColor.GetSaturation();
            return hslColor;
        }

        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            HslColor hslColor = SimpleColor.FromArgb(red, green, blue);
            _hue = hslColor._hue;
            _saturation = hslColor._saturation;
            _luminosity = hslColor._luminosity;
        }

        public HslColor()
        {
        }

        public HslColor(SimpleColor simpleColor) => SetRGB(simpleColor.R, simpleColor.G, simpleColor.B);

        public HslColor(int red, int green, int blue) => SetRGB(red, green, blue);

        public HslColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        public static SimpleColor ScaleHsl(SimpleColor simpleColor, float h, float s, float l)
        {
            var hslColor = new HslColor(simpleColor);
            hslColor._hue *= h;
            hslColor._saturation *= s;
            hslColor._luminosity *= l;
            return hslColor.ToColor();
        }
    }
}