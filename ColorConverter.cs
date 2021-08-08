using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeenChroma
{
    public static class ColorConverter
    {
        // RGB->HSL conversion formula obtained from http://www.easyrgb.com/en/math.php
        public static Tuple<float, float, float> RGBToHSL(int red, int green, int blue)
        {
            double r = red / 255;
            double g = green / 255;
            double b = blue / 255;

            var min = Math.Min(r, Math.Min(g, b));
            var max = Math.Max(r, Math.Max(g, b));
            var diff = max - min;
            double l = (max + min) / 2;

            if (diff == 0) return new Tuple<float, float, float>(0, 0, (float)l);

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

            return new Tuple<float, float, float>((float)h, (float)s, (float)l);
        }
    }
}
