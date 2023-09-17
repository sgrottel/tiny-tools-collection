//
// ColorVariantsWheel.cs
// https://github.com/sgrottel/tiny-tools-collection
//
// MIT License
//
// Copyright (c) 2023 Sebastian Grottel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

namespace ColorVariantsWheel
{

    /// <summary>
    /// Color variants will be computed from a HSL-variant color wheel.
    /// The first colors will be selected by numerically equidistant hues.
    /// </summary>
    /// <remarks>
    /// The used color space differes from the normal HSL space as it does not use Lightness but Luminance.
    /// In addition, there is a lot of deviation from any color space model operations, just performing phenomenological adjustments.
    /// </remarks>
    public sealed class ColorVariantsWheel
    {
        private int numBaseHues = 3;

        /// <summary>
        /// The number of base hues of numerically equidistant values
        /// </summary>
        /// <remarks>
        /// The minimum valid value is 2.
        /// </remarks>
        public int NumberOfBaseHues
        {
            get => numBaseHues;
            set
            {
                if (numBaseHues != value)
                {
                    if (value < 2) throw new ArgumentException("Number of Base Hues must be two or larger");
                    numBaseHues = value;
                }
            }
        }

        /// <summary>
        /// The hue shift of the base colors in degree
        /// </summary>
        /// <remarks>
        /// The first base hue is 0° => Red-Orange
        /// Use this shift to rotate the whole color wheel to place the first base hue on a different value.
        /// </remarks>
        /// <example>
        /// When shift is set to 120°, the first base hue will be Green.
        /// </example>
        public double BaseHueShift { get; set; } = 5.0;

        /// <summary>
        /// The base saturation in percent.
        /// Default: 90%
        /// </summary>
        public double BaseSaturation { get; set; } = 0.9;

        /// <summary>
        /// The base color luminance in percent.
        /// Default: 100%
        /// 100% luminance is bright color / white (0% saturation)
        /// 0% is black
        /// >100% will fade to white even with higher saturations
        /// </summary>
        public double BaseLuminance { get; set; } = 0.9;

        /// <summary>
        /// Output RGB (red, green, blue) color struct.
        /// Valid color components are to be in [0..1].
        /// Values might, however be outside this range, and need to be clamped before usage or conversion
        /// </summary>
        public struct RGB
        {
            public double R;
            public double G;
            public double B;
        }

        /// <summary>
        /// Produces a color variant
        /// </summary>
        /// <param name="col">The base color index.
        /// 0 or higher.</param>
        /// <param name="var">The variant index.
        /// Positive values create brighter variants.
        /// Negative values create darker variants.
        /// </param>
        /// <returns>The generated color variant</returns>
        public RGB MakeColorVariant(int col, int var)
        {
            double h;
            int i = col;
            if (i < numBaseHues)
            {
                h = i * 360.0 / numBaseHues;
            }
            else
            {
                i -= numBaseHues;
                int c = numBaseHues;
                double o = 180.0 / numBaseHues;

                while (i >= c)
                {
                    i -= c;
                    c *= 2;
                    o /= 2;
                }

                h = o + i * 360 / c;
            }
            h += BaseHueShift;

            h = RemapHue(h);

            RGB rgb = FromHue(h);

            double saturation = Math.Clamp(BaseSaturation, 0.0, 1.0);
            double luminanceDown = 0.0;
            double luminanceUp = 0.0;

            if (var < 0)
            {
                luminanceDown = 1.0 - 1.0 / (-var / 2.0 + 1.0);
                saturation *= Math.Clamp(1.0 - luminanceDown, 0.0, 1.0);
            }
            else if (var > 0)
            {
                luminanceUp = 1.0 - 1.0 / (var / 1.75 + 1.0);
                saturation *= Math.Clamp(1.0 - luminanceUp, 0.0, 1.0);
            }

            AdjustSaturation(ref rgb, saturation);
            AdjustLuminance(ref rgb, luminanceDown, luminanceUp);

            Overshoot(ref rgb);

            return rgb;
        }

        /// <summary>
        /// Additional non-linear color shift of hue to keep green shades further appart
        /// </summary>
        private double RemapHue(double h)
        {
            // h range unknown
            h = h % 360.0;
            if (h < 0) { h += 360.0; }
            // h in [0..360[
            h -= 120.0; // shift green to zero
            // h in [-120..240[
            if (h >= 180.0) { h -= 360.0; }
            // h in [-180..180[

            double fac = (h >= 0) ? 180.0 : -180.0;
            h /= fac;
            // h in [0..1]

            // spread space around green, making shades of green more distinguishable
            h = Math.Pow(h, 0.8);

            h *= fac;
            // h in [-180..180[  --  with green at zero
            h = (h + 120.0) % 360.0;
            if (h < 0) { h += 360.0; }
            // h in [0..360[  --  with red at zero

            return h;
        }

        /// <summary>
        /// Produces an alternating signed variant index.
        /// 0 => 0
        /// 1 => 1
        /// 2 => -1
        /// 3 => 2
        /// 4 => -2
        /// ...
        /// </summary>
        /// <param name="var">The increasing positive variant index.</param>
        /// <returns>The derived variant index with alternating sign</returns>
        static public int AlternatingVariant(int var)
        {
            if (var < 0) throw new ArgumentException("Variant index must be positive");
            if (var == 0) return 0;
            if (var % 2 == 0)
            {
                return -(var / 2);
            }
            else
            {
                return (var + 1) / 2;
            }
        }

        private RGB FromHue(double hue)
        {
            double h = (hue / 120.0) % 3.0;
            if (h < 0.0) h += 3.0;

            // https://www.wolframalpha.com/input?i=plot+max%281+-+x%2C+x+-+2%29%2C+x%3D0..3
            double r = Math.Clamp(Math.Max(1.0 - h, h - 2.0), 0.0, 1.0);
            double g = Math.Clamp(1.0 - Math.Abs(h - 1.0), 0.0, 1.0);
            double b = Math.Clamp(1.0 - Math.Abs(h - 2.0), 0.0, 1.0);

            // use a simplified luminance approach
            double l = Math.Sqrt(0.299 * r * r + 0.587 * g * g + 0.114 * b * b);

            r *= BaseLuminance / l;
            g *= BaseLuminance / l;
            b *= BaseLuminance / l;

            return new() { R = r, G = g, B = b };
        }

        static private void AdjustSaturation(ref RGB rgb, double s)
        {
            double l = (0.299 * rgb.R + 0.587 * rgb.G + 0.114 * rgb.B);
            rgb.R = rgb.R * s + l * (1.0 - s);
            rgb.G = rgb.G * s + l * (1.0 - s);
            rgb.B = rgb.B * s + l * (1.0 - s);
        }

        private void AdjustLuminance(ref RGB rgb, double luminanceDown, double luminanceUp)
        {
            rgb.R = (rgb.R * (1.0 - luminanceUp) + luminanceUp) * (1.0 - luminanceDown);
            rgb.G = (rgb.G * (1.0 - luminanceUp) + luminanceUp) * (1.0 - luminanceDown);
            rgb.B = (rgb.B * (1.0 - luminanceUp) + luminanceUp) * (1.0 - luminanceDown);
        }

        private void Overshoot(ref RGB rgb)
        {
            if (rgb.R > 1.0 || rgb.G > 1.0 || rgb.B > 1.0)
            {
                double w
                    = (Math.Clamp(rgb.R - 1.0, 0.0, 1.0)
                    + Math.Clamp(rgb.G - 1.0, 0.0, 1.0)
                    + Math.Clamp(rgb.B - 1.0, 0.0, 1.0)) / 3.0;
                rgb.R = w + rgb.R * (1.0 - w);
                rgb.G = w + rgb.G * (1.0 - w);
                rgb.B = w + rgb.B * (1.0 - w);
            }
        }

    }

}
