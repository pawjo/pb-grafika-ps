using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaPS4
{
    public class PointTransforms
    {
        public static Bitmap AddAsync(Bitmap bitmap, int value)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);

                    var r = color.R + value;
                    var g = color.G + value;
                    var b = color.B + value;

                    if (r > 255)
                        r = 255;

                    if (g > 255)
                        g = 255;

                    if (b > 255)
                        b = 255;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }

        public static Bitmap SubtractAsync(Bitmap bitmap, int value)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);

                    var r = color.R - value;
                    var g = color.G - value;
                    var b = color.B - value;

                    if (r < 0)
                        r = 0;

                    if (g < 0)
                        g = 0;

                    if (b < 0)
                        b = 0;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }

        public static Bitmap MultiplyAsync(Bitmap bitmap, int value)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);

                    var r = color.R * value;
                    var g = color.G * value;
                    var b = color.B * value;

                    if (r > 255)
                        r = 255;

                    if (g > 255)
                        g = 255;

                    if (b > 255)
                        b = 255;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }

        public static Bitmap DivideAsync(Bitmap bitmap, int value)
        {

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);

                    var r = color.R / value;
                    var g = color.G / value;
                    var b = color.B / value;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            return bitmap;
        }

        public static Bitmap BrighterAsync(Bitmap bitmap, int value)
        {
            var lut = new int[256];

            for (int i = 0; i < 256; i++)
            {
                lut[i] = i + value;

                if (lut[i] > 255)
                {
                    lut[i] = 255;
                }
            }

            bitmap = SetBitmapFromLut(bitmap, lut);

            return bitmap;
        }

        public static Bitmap DarkerAsync(Bitmap bitmap, int value)
        {
            var lut = new int[256];

            for (int i = 0; i < 256; i++)
            {
                lut[i] = i - value;

                if (lut[i] < 0)
                {
                    lut[i] = 0;
                }
            }

            bitmap = SetBitmapFromLut(bitmap, lut);

            return bitmap;
        }

        private static Bitmap SetBitmapFromLut(Bitmap bitmap, int[] lut)
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

        public static Bitmap GrayScaleAsync(Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var r = (color.R + color.G + color.B) / 3;
                    var g = (color.R + color.G + color.B) / 3;
                    var b = (color.R + color.G + color.B) / 3;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }
            return bitmap;
        }

        public static Bitmap GrayScaleYUVAsync(Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var r = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
                    var g = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
                    var b = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, (int)r, (int)g, (int)b));
                }
            }
            return bitmap;
        }
    }
}
