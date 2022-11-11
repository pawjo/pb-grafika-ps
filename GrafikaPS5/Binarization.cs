using System;
using System.Drawing;

namespace GrafikaPS4
{
    public static class Binarization
    {
        public static Bitmap ApplyBinarization(Bitmap bitmap, int value)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var avg = (color.R + color.G + color.B) / 3;
                    var newColor = avg >= value ? Color.White : Color.Black;
                    bitmap.SetPixel(i, j, newColor);
                }
            }
            return bitmap;
        }

        public static Bitmap PercentBlackSelection(Bitmap bitmap, int percent, Histogram histogram)
        {
            var maxSum = bitmap.Width * bitmap.Height * percent / 100;
            var treshold = 0;
            var sum = 0;

            for (; treshold < 256; treshold++)
            {
                var pixel = histogram.GetAverageHistogramValue(treshold);
                sum += pixel;
                if (sum >= maxSum)
                {
                    break;
                }
            }

            return ApplyBinarization(bitmap, treshold);
        }

        public static Bitmap EntropySelection(Bitmap bitmap, Histogram histogram)
        {
            double treshold = 0;
            var allPixelCount = bitmap.Width * bitmap.Height;

            for (int i = 0; i < 256; i++)
            {
                var pixel = histogram.GetAverageHistogramValue(i);
                if (pixel > 0)
                {
                    var probability = (double)pixel / allPixelCount;
                    var log = Math.Log2(probability);
                    treshold -= probability * log;
                }
            }

            return ApplyBinarization(bitmap, (int)treshold);
        }
    }
}
