using System;
using System.Globalization;

namespace RotoMonster.Pages
{
    public static class RmColor
    {
        public static string Vars(string prefix, string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return "";
            hex = hex.Trim().TrimStart('#');
            if (hex.Length != 6) return "";
            int r, g, b;
            try
            {
                r = Convert.ToInt32(hex.Substring(0, 2), 16);
                g = Convert.ToInt32(hex.Substring(2, 2), 16);
                b = Convert.ToInt32(hex.Substring(4, 2), 16);
            }
            catch
            {
                return "";
            }
            double h, s, l;
            ToHsl(r, g, b, out h, out s, out l);
            var strength = Math.Max(0, Math.Min(1, (1 - l) / 0.2));
            var s2 = Math.Min(1, s * strength * 2.2);
            var light = FromHsl(h, s2, 0.36);
            var dark = FromHsl(h, s2, 0.70);
            return "--" + prefix + ":#" + hex
                + ";--" + prefix + "-l:" + Hex(light)
                + ";--" + prefix + "-d:" + Hex(dark)
                + ";--" + prefix + "-lbg:" + Rgba(light, 0.10)
                + ";--" + prefix + "-dbg:" + Rgba(dark, 0.14)
                + ";--" + prefix + "-lbd:" + Rgba(light, 0.45)
                + ";--" + prefix + "-dbd:" + Rgba(dark, 0.40);
        }

        static void ToHsl(int r, int g, int b, out double h, out double s, out double l)
        {
            double rf = r / 255.0, gf = g / 255.0, bf = b / 255.0;
            double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
            l = (max + min) / 2;
            if (max - min < 1e-9)
            {
                h = 0;
                s = 0;
                return;
            }
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == rf) h = (gf - bf) / d + (gf < bf ? 6 : 0);
            else if (max == gf) h = (bf - rf) / d + 2;
            else h = (rf - gf) / d + 4;
            h /= 6;
        }

        static int[] FromHsl(double h, double s, double l)
        {
            if (s < 1e-9)
            {
                var v = (int)Math.Round(l * 255);
                return new[] { v, v, v };
            }
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            return new[] { (int)Math.Round(Hue(p, q, h + 1.0 / 3) * 255), (int)Math.Round(Hue(p, q, h) * 255), (int)Math.Round(Hue(p, q, h - 1.0 / 3) * 255) };
        }

        static double Hue(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        static string Hex(int[] c) => "#" + c[0].ToString("x2") + c[1].ToString("x2") + c[2].ToString("x2");

        static string Rgba(int[] c, double a) => "rgba(" + c[0] + "," + c[1] + "," + c[2] + "," + a.ToString("0.##", CultureInfo.InvariantCulture) + ")";
    }
}
