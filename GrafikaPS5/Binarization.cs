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
            var threshold = 0;
            var sum = 0;

            for (; threshold < 256; threshold++)
            {
                var pixel = histogram.GetAverageHistogramValue(threshold);
                sum += pixel;
                if (sum >= maxSum)
                {
                    break;
                }
            }

            return ApplyBinarization(bitmap, threshold);
        }

        public static Bitmap EntropySelection(Bitmap bitmap, Histogram histogram)
        {
            double threshold = 0;
            var allPixelCount = bitmap.Width * bitmap.Height;

            for (int i = 0; i < 256; i++)
            {
                var pixel = histogram.GetAverageHistogramValue(i);
                if (pixel > 0)
                {
                    var probability = (double)pixel / allPixelCount;
                    var log = Math.Log2(probability);
                    threshold -= probability * log;
                }
            }

            return ApplyBinarization(bitmap, (int)threshold);
        }

        public static Bitmap MeanIterativeSelection(Bitmap bitmap, Histogram histogram)
        {
            var avgHistogram = new int[256];
            var multipliedSum = 0;
            var sum = 0;
            for (int i = 0; i < 256; i++)
            {
                avgHistogram[i] = histogram.GetAverageHistogramValue(i);
                multipliedSum += i * avgHistogram[i];
                sum += avgHistogram[i];
            }

            var multipliedSumBelow = 0;
            var multipliedSumAbove = 0;
            var sumBelow = 0;
            var sumAbove = 0;
            var grayLevels = new int[256];
            grayLevels[0] = 128;
            var threshold = 0;

            for (int i = 1; i < 256; i++)
            {
                var previousGrayLevel = grayLevels[i - 1];
                multipliedSumBelow += i * avgHistogram[i];
                sumBelow += avgHistogram[i];
                multipliedSumAbove = multipliedSum - multipliedSumBelow;
                sumAbove = sum - sumBelow;

                if (sumBelow == 0 || sumAbove == 0)
                    continue;

                var grayLevelBelow = (double)multipliedSumBelow / sumBelow;
                var grayLevelAbove = (double)multipliedSumAbove / sumAbove;
                grayLevels[i] = (int)(grayLevelBelow + grayLevelAbove) / 2;

                if (grayLevelBelow == grayLevelAbove || grayLevels[i] == previousGrayLevel)
                {
                    threshold = grayLevels[i];
                    break;
                }
            }

            return ApplyBinarization(bitmap, threshold);
        }

        //public Bitmap Otsu(Bitmap bitmap, Histogram histogram)
        //{

        //}
    }
}
