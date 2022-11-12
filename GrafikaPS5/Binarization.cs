using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Runtime.CompilerServices;

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
            var histogramData = GetHistogramData(histogram);
            var multipliedSumBelow = 0;
            var sumBelow = 0;
            var threshold = 128;

            for (int i = 1; i < 256; i++)
            {
                var previousThreshold = threshold;
                multipliedSumBelow += i * histogramData.AvgValue[i];
                sumBelow += histogramData.AvgValue[i];
                if (sumBelow == 0)
                    continue;

                var sumAbove = histogramData.Sum - sumBelow;
                if (sumAbove == 0)
                    break;

                var grayLevelBelow = (double)multipliedSumBelow / sumBelow;
                var grayLevelAbove = (double)(histogramData.MultipliedSum - multipliedSumBelow) / sumAbove;
                threshold = (int)(grayLevelBelow + grayLevelAbove) / 2;

                if (grayLevelBelow == grayLevelAbove || threshold == previousThreshold)
                {
                    break;
                }
            }

            return ApplyBinarization(bitmap, threshold);
        }

        public static Bitmap Otsu(Bitmap bitmap, Histogram histogram)
        {
            var histogramData = GetHistogramData(histogram);
            var multipliedSumBelow = 0;
            var sumBelow = 0;
            var threshold = 128;
            double varianceMax=0;

            for (int i = 1; i < 256; i++)
            {
                multipliedSumBelow += i * histogramData.AvgValue[i];
                sumBelow += histogramData.AvgValue[i];
                if (sumBelow == 0)
                    continue;

                var sumAbove = histogramData.Sum - sumBelow;
                if (sumAbove == 0)
                    break;

                var grayLevelBelow = (double)multipliedSumBelow / sumBelow;
                var grayLevelAbove = (double)(histogramData.MultipliedSum - multipliedSumBelow) / sumAbove;
                var factor = grayLevelBelow - grayLevelAbove;
                var varianceBetween = (double)grayLevelBelow * grayLevelAbove * Math.Pow(factor, 2);

                if (varianceBetween > varianceMax)
                {
                    varianceMax = varianceBetween;
                    threshold = i;
                }
            }

            return ApplyBinarization(bitmap, threshold);
        }

        private class HistogramForBinarizationModel
        {
            public int[] AvgValue = new int[256];

            public int MultipliedSum = 0;

            public int Sum = 0;
        }

        private static HistogramForBinarizationModel GetHistogramData(Histogram histogram)
        {
            var result = new HistogramForBinarizationModel();
            for (int i = 0; i < 256; i++)
            {
                result.AvgValue[i] = histogram.GetAverageHistogramValue(i);
                result.MultipliedSum += i * result.AvgValue[i];
                result.Sum += result.AvgValue[i];
            }
            return result;
        }
    }
}
