using System;
using System.Drawing;

namespace ReadPoolTableFromImage
{
    /// <summary>
    /// CIE-LAB color space
    /// </summary>
    public struct LabColor
    {
        private Color? _color;
        public LabColor(float lStar, float aStar, float bStar, byte alpha = 255)
        {
            if (lStar < 0 || lStar > 100 ||
                aStar < -128 || aStar > 128 ||
                bStar < -128 || bStar > 128)
                throw new ArgumentOutOfRangeException("lStar must be within 0 and 100. aStar/bStar must be within -128 and 128");
            this.L = lStar;
            this.A = aStar;
            this.B = bStar;
            this.Alpha = alpha;
            this._color = null;
        }
        public LabColor(Color color) : this(color.R, color.G, color.B, color.A) { }
        public LabColor(byte red, byte green, byte blue, byte alpha = 255)
        {
            LabColor labColor = LabColor.FromArgb(red, green, blue, alpha);

            this.L = labColor.L;
            this.A = labColor.A;
            this.B = labColor.B;
            this.Alpha = labColor.Alpha;
            this._color = Color.FromArgb(alpha, red, green, blue);
        }
        /// <summary>
        /// The L* value representing Lightness
        /// </summary>
        public float L { get; private set; }
        /// <summary>
        /// The a* value representing Red<->Green axis
        /// </summary>
        public float A { get; private set; }
        /// <summary>
        /// The b* value representing Blue<->Yellow axis
        /// </summary>
        public float B { get; private set; }
        /// <summary>
        /// Alpha channel value preserved when converting from ARGB. 
        /// This channel is not represented or considered in CIE-LAB Color Space
        /// </summary>
        public byte Alpha { get; private set; }
        /// <summary>
        /// Gets the CIE-LAB equivalent. If Alpha channel was specified, it will be included.
        /// </summary>
        /// <returns></returns>
        public Color ToColor()
        {
            if (!this._color.HasValue)
                return LabColor.ToColor(this);
            return this._color.Value;
        }
        /// <summary>
        /// Converts the CIE-LAB to its Color equivalent. If Alpha channel was specified, it will be included.
        /// </summary>
        /// <returns></returns>
        public static Color ToColor(LabColor labColor)
        {
            // implement conversion and return it
            return Color.Transparent;
        }
        /// <summary>
        /// Converts a Color to its CIE-LAB color equivalent. 
        /// If Alpha channel is used, it is preserved but not converted.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static LabColor FromColor(Color color)
        {
            return LabColor.FromArgb(color.R, color.G, color.B, color.A);
        }
        /// <summary>
        /// Create a CIE-LAB color using RGB values. 
        /// If Alpha is specified, it is preserved but not converted.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <remarks>https://stackoverflow.com/questions/58952430/rgb-xyz-and-xyz-lab-color-space-conversion-algorithm</remarks>
        /// <returns></returns>
        public static LabColor FromArgb(byte red, byte green, byte blue, byte alpha)
        {
            float[] xyz = new float[3];
            float[] lab = new float[3];
            float[] rgb = new float[] { red, green, blue, alpha };

            for (int index = 0; index < rgb.Length; index++)
            {
                rgb[index] = rgb[index] / byte.MaxValue; // normalize to produce value in range 0 to 1

                // get gamma-expanded values
                if (rgb[index] > 0.04045f)
                    rgb[index] = (float)Math.Pow((rgb[index] + 0.055) / 1.055, 2.4);
                else
                    rgb[index] = rgb[index] / 12.92f;

                rgb[index] = rgb[index] * 100.0f;
            }

            // Get CIE-XYZ by using the gamma-expanded values multiplied by matrix values
            xyz[0] = ((rgb[0] * .412453f) + (rgb[1] * .357580f) + (rgb[2] * .180423f));
            xyz[1] = ((rgb[0] * .212671f) + (rgb[1] * .715160f) + (rgb[2] * .072169f));
            xyz[2] = ((rgb[0] * .019334f) + (rgb[1] * .119193f) + (rgb[2] * .950227f));

            xyz[0] = xyz[0] / 95.047f;
            xyz[1] = xyz[1] / 100.0f;
            xyz[2] = xyz[2] / 108.883f;

            for (int index = 0; index < xyz.Length; index++)
            {
                if (xyz[index] > .008856f)
                    xyz[index] = (float)Math.Pow(xyz[index], 1.0 / 3.0);
                else
                    xyz[index] = (xyz[index] * 7.787f) + (16.0f / 116.0f);
            }

            lab[0] = (116.0f * xyz[1]) - 16.0f;
            lab[1] = 500.0f * (xyz[0] - xyz[1]);
            lab[2] = 200.0f * (xyz[1] - xyz[2]);
            // sometimes black gives l* value less than 0, which is invalid
            return new LabColor(Math.Max(lab[0], 0), lab[1], lab[2], alpha);
        }

        /// <summary>
        /// Finds the Delta E(94) of two LabColors
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="applicationType"></param>
        /// <remarks>
        /// formula:
        /// http://www.brucelindbloom.com/index.html?Eqn_DeltaE_CIE94.html
        /// summary:
        /// https://opentextbc.ca/graphicdesign/chapter/4-4-lab-colour-space-and-delta-e-measurements/
        /// constants & values breakdown:
        /// https://www.pac.gr/bcm/uploads/a-guide-of-understanding-color-comunication-part-3.pdf
        /// </remarks>
        /// <returns></returns>
        public static float FindDeltaE94(LabColor color1, LabColor color2, Constants.ApplicationType applicationType = Constants.ApplicationType.GraphicArts)
        {
            Constants constants = Constants.Get(applicationType);
            double K1 = constants.K1; 
            double K2 = constants.K2; 
            double KL = constants.KL;
            double KC = constants.KC; // 1d; 
            double KH = constants.KH; // 1d; 
            // modifying factor to compensate for perceptual distortions in the color space
            double ab = 1d;
            // difference in lightness/darkness value. += lighter | -= darker
            double delta_L = color1.L - color2.L;
            double C1 = Math.Sqrt(Math.Pow(color1.A, 2d) + Math.Pow(color1.B, 2d));
            double C2 = Math.Sqrt(Math.Pow(color2.A, 2d) + Math.Pow(color2.B, 2d));
            // Brightness in chroma. += brighter | -= duller
            double delta_C = C1 - C2;
            // difference on red/green axis. += redder | -= greener
            double delta_a = color1.A - color2.A;
            // difference on yellow/blue axis. += yellower | -= bluer
            double delta_b = color1.B - color2.B;
            double delta_h_radical = (Math.Pow(delta_a, 2d) + Math.Pow(delta_b, 2d) - Math.Pow(delta_C, 2d));
            // In the calculation of ΔH, the value inside the radical is, in theory, always greater than or equal to zero.
            // However in an actual implementation, it may become a very slightly negative value, due to limited arithmetic precision. 
            // Should this occur, the square root will fail.
            // Difference in hue
            double delta_H = Math.Sqrt(delta_h_radical < 0 ? 0 : delta_h_radical);
            double SL = 1d;
            double SC = 1d + (K1 * C1);
            double SH = 1d + (K2 * C1);

            double part1 = Math.Pow(delta_L / (KL * SL), 2d);
            double part2 = Math.Pow(delta_C / (KC * SC), 2d); //  Math.Pow((delta_C * ab) / (KC * SC), 2d);
            double part3 = Math.Pow(delta_H / (KH * SH), 2d); //  Math.Pow((delta_H * ab) / (KH * SH), 2d);
            // Total color difference value
            float delta_E = (float)Math.Sqrt(part1 + part2 + part3);

            return delta_E;
        }

        /// <summary>
        /// Constant values used when calculating Delta E.
        /// The Constants are organized by application type
        /// </summary>
        public class Constants
        {
            public static Constants GraphicArts = new Constants(1f, 0.045f, 0.015f, 1f, 1f);
            public static Constants Textiles = new Constants(2f, 0.048f, 0.014f, 1f, 1f);

            public Constants(float kl, float k1, float k2, float kc, float kh)
            {
                this.KL = kl;
                this.K1 = k1;
                this.K2 = k2;
                this.KC = kc;
                this.KH = kh;
            }
            /// <summary>
            /// Control of lightness. 
            /// 1 = default | 2 = textiles
            /// </summary>
            public float KL { get; private set; }
            /// <summary>
            /// 0.045 = graphic arts | 0.048 = textiles
            /// </summary>
            public float K1 { get; private set; }
            /// <summary>
            /// 0.015 = graphic arts | 0.014 = textiles
            /// </summary>
            public float K2 { get; private set; }
            /// <summary>
            /// Control of chroma ratio.
            /// 1 = default
            /// </summary>
            public float KC { get; private set; }
            /// <summary>
            /// 1 = default
            /// </summary>
            public float KH { get; private set; }
            /// <summary>
            /// Gets the corresponding Constants associated with ApplicationType
            /// </summary>
            /// <param name="applicationType"></param>
            /// <returns></returns>
            public static Constants Get(ApplicationType applicationType)
            {
                if (applicationType == ApplicationType.GraphicArts)
                    return GraphicArts;
                return Textiles;
            }
            /// <summary>
            /// Application Types which are used to determine modifying factors for calculating Delta E of two LabColors
            /// </summary>
            public enum ApplicationType
            {
                GraphicArts,
                Textiles
            }
        }
    }
}
