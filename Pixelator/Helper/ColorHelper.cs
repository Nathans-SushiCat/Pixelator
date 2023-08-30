using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace Pixelator.Helper
{
    internal class ColorHelper
    {
        public static bool ComparePixelColorWithTolerance(Color color1, Color color2, double tolerance)
        {
            int colorDiff = Math.Abs(color1.R - color2.R) +
                            Math.Abs(color1.G - color2.G) +
                            Math.Abs(color1.B - color2.B);

            return colorDiff <= tolerance;
        }
    }
}
