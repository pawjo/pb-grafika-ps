using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaPS4
{
    public static class LutUtils
    {
        public static Bitmap ApplyLut(Bitmap bitmap, int[] lut)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var r = lut[color.R];
                    var g = lut[color.G];
                    var b = lut[color.B];

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }

        public static Bitmap ApplyRGBLut(Bitmap bitmap, int[] lutR, int[] lutG, int[] lutB)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var r = lutR[color.R];
                    var g = lutG[color.G];
                    var b = lutB[color.B];

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }
    }
}
