using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DarkThemeImage
{
    internal class Program
    {
        #region Private Methods

        private static Color GetDarkThemeColor(Color c, Color light, Color darkTarget)
        {
            var hsl = GetHSL(c);
            double luminosity = 1 - hsl.Item3;
            double haloLuminosity = 1 - GetHSL(light).Item3;
            double themeBackgroundLuminosity = GetHSL(darkTarget).Item3;

            if (luminosity < haloLuminosity)
                return GetRGB(c.A, hsl.Item1, hsl.Item2, themeBackgroundLuminosity * luminosity / haloLuminosity);
            else
                return GetRGB(c.A, hsl.Item1, hsl.Item2, (1.0 - themeBackgroundLuminosity) * (luminosity - 1.0) / (1.0 - haloLuminosity) + 1.0);
        }

        private static (double, double, double) GetHSL(Color c)
        {
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;
            double max = Math.Max(Math.Max(r, g), b);
            double min = Math.Min(Math.Min(r, g), b);
            double lum = (max + min) / 2;
            double satur, hue;
            if (min == max)
                satur = 0;
            else if (lum < .5)
                satur = (max - min) / (max + min);
            else
                satur = (max - min) / (2.0 - max - min);
            if (r == max)
                hue = (g - b) / (max - min);
            else if (g == max)
                hue = 2.0 + (b - r) / (max - min);
            else
                hue = 4.0 + (r - g) / (max - min);
            hue = (hue * 60 + 360) % 360;
            return (hue, satur, lum);
        }

        private static Color GetRGB(byte alpha, double hue, double satur, double lum)
        {
            double v;
            hue /= 360;
            double r, g, b;
            r = lum;   // default to gray
            g = lum;
            b = lum;
            v = (lum <= 0.5) ? (lum * (1.0 + satur)) : (lum + satur - lum * satur);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;
                m = lum + lum - v;
                sv = (v - m) / v;
                hue *= 6.0;
                sextant = (int)hue;
                fract = hue - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;

                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;

                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;

                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;

                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;

                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            var rgb = Color.FromArgb(alpha, Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f), Convert.ToByte(b * 255.0f));
            return rgb;
        }

        private static void Main(string[] args)
        {
            if (args.Length < 3)
                Console.WriteLine(
@"Usage :

    darkthemeimage <hexcode light color> <hexcode dark color> <dir/file paths>");
            else
            {
                byte r = byte.Parse(args[0][0..2], System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(args[0][2..4], System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(args[0][4..6], System.Globalization.NumberStyles.HexNumber);
                var light = Color.FromArgb(r, g, b);
                r = byte.Parse(args[1][0..2], System.Globalization.NumberStyles.HexNumber);
                g = byte.Parse(args[1][2..4], System.Globalization.NumberStyles.HexNumber);
                b = byte.Parse(args[1][4..6], System.Globalization.NumberStyles.HexNumber);
                var dark = Color.FromArgb(r, g, b);
                foreach (var path in args[2..])
                    if (File.Exists(path))
                        MakeDarkTheme(new Bitmap(path), light, dark).Save(Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}_dark.png"), System.Drawing.Imaging.ImageFormat.Png);
                    else if (Directory.Exists(path))
                        foreach (var file in new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).Select(f => f.FullName))
                            MakeDarkTheme(new Bitmap(file), light, dark).Save(Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}_dark.png"), System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static Bitmap MakeDarkTheme(Bitmap bitmap, Color light, Color darkTarget)
        {
            bitmap = new Bitmap(bitmap);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, GetDarkThemeColor(bitmap.GetPixel(x, y), light, darkTarget));
                }
            }
            return bitmap;
        }

        #endregion Private Methods
    }
}