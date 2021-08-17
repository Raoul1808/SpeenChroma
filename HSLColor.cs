using System;

namespace SpeenChroma
{
    public struct HSLColor
    {
        private float hue;
        private float saturation;
        private float lightness;

        private const float KEEP_CURRENT_VALUE_CHECK_VALUE = -2f; // TODO: Find a better name

        public float H
        {
            get => hue;
            set
            {
                if (value == KEEP_CURRENT_VALUE_CHECK_VALUE || hue == KEEP_CURRENT_VALUE_CHECK_VALUE) hue = KEEP_CURRENT_VALUE_CHECK_VALUE;
                if (value > 1f)
                    hue = value - 1f;
                else if (value < 0f)
                    hue = value + 1f;
                else
                    hue = value;
            }
        }

        public float S
        {
            get => saturation;
            set
            {
                if (value == KEEP_CURRENT_VALUE_CHECK_VALUE || saturation == KEEP_CURRENT_VALUE_CHECK_VALUE) saturation = KEEP_CURRENT_VALUE_CHECK_VALUE;
                if (value > 1f)
                    saturation = 1f;
                else if (value < 0f)
                    saturation = 0f;
                else
                    saturation = value;
            }
        }

        public float L
        {
            get => lightness;
            set
            {
                if (value == KEEP_CURRENT_VALUE_CHECK_VALUE || lightness == KEEP_CURRENT_VALUE_CHECK_VALUE) lightness = KEEP_CURRENT_VALUE_CHECK_VALUE;
                if (value > 1f)
                    lightness = 1f;
                else if (value < 0f)
                    lightness = 0f;
                else
                    lightness = value;
            }
        }

        public HSLColor(float hue, float saturation, float lightness)
        {
            if (hue > 1f) hue -= 1f;
            if (hue < 0f) hue += 1f;
            if (saturation > 1f) saturation = 1f;
            if (saturation < 0f) saturation = 0f;
            if (lightness > 1f) lightness = 1f;
            if (lightness < 0f) lightness = 0f;
            this.hue = hue;
            this.saturation = saturation;
            this.lightness = lightness;
        }

        // RGB->HSL conversion formula obtained from http://www.easyrgb.com/en/math.php
        public static HSLColor FromRGB(int red, int green, int blue)
        {
            double r = red / 255;
            double g = green / 255;
            double b = blue / 255;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));
            var diff = max - min;
            double l = (max + min) / 2;

            if (diff == 0) return new HSLColor(0, 0, (float)l);

            double s;
            if (l < 0.5) s = diff / (max + min);
            else s = diff / (2 - max - min);

            double r2 = (((max - r) / 6) + (max / 2)) / diff;
            double g2 = (((max - g) / 6) + (max / 2)) / diff;
            double b2 = (((max - b) / 6) + (max / 2)) / diff;

            double h = 0;
            if (r == max) h = b2 - g2;
            else if (g == max) h = (1 / 3) + r2 - b2;
            else if (b == max) h = (2 / 3) + g2 - r2;

            if (h < 0) h += 1;
            if (h > 1) h -= 1;
            return new HSLColor((float)h, (float)s, (float)l);
        }
    }
}
