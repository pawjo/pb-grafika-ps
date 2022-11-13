using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
            double varianceMax = 0;

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

        //public unsafe static Bitmap Niblack(Bitmap bitmap)
        //{
        //    var data = bitmap.LockBits(
        //        new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
        //        ImageLockMode.ReadWrite,
        //        bitmap.PixelFormat);


        //    var height = data.Height;
        //    var width = data.Width;
        //    var dataLength = height * width;
        //    var radius = 15;
        //    var boxSize = radius * 2;
        //    var stride = data.Stride;
        //    var pixelSize = Image.GetPixelFormatSize(data.PixelFormat) / 8;
        //    //var offset = stride - width * pixelSize;
        //    var pixelBuffer = new byte[data.Stride * data.Height];

        //    Marshal.Copy(data.Scan0, pixelBuffer, 0, pixelBuffer.Length);

        //    for (int i = 0; i < dataLength; i += 4)
        //    {
        //        var avg = (byte)((pixelBuffer[i] + pixelBuffer[i + 1] + pixelBuffer[i + 2]) / 3);
        //        pixelBuffer[i] = pixelBuffer[i + 1] = pixelBuffer[i + 2] = avg;
        //    }

        //    var ptr = (byte*)data.Scan0.ToPointer();

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            var sum = 0;
        //            var count = 0;

        //            for (int i = 0; i < boxSize; i++)
        //            {
        //                int boxY = i - radius;
        //                int actualIndex = y + boxY;

        //                if (actualIndex < 0)
        //                    continue;

        //                if (actualIndex >= height)
        //                    break;

        //                for (int j = 0; j < boxSize; j++)
        //                {
        //                    int boxX = j - radius;
        //                    actualIndex = x + boxX;

        //                    if (actualIndex < 0)
        //                        continue;
        //                    if (actualIndex >= width)
        //                        continue;

        //                    sum += pixelBuffer[boxY * stride + boxX * pixelSize];
        //                    count++;
        //                }
        //            }

        //            var mean = (double)sum / count;
        //            var variance = 0.0;

        //            for (int i = 0; i < boxSize; i++)
        //            {
        //                int boxY = i - radius;
        //                int actualY = y + boxY;

        //                if (actualY < 0)
        //                    continue;

        //                if (actualY >= height)
        //                    break;

        //                for (int j = 0; j < boxSize; j++)
        //                {
        //                    int boxX = j - radius;
        //                    actualY = x + boxX;

        //                    if (actualY < 0)
        //                        continue;
        //                    if (actualY >= width)
        //                        continue;

        //                    var value = pixelBuffer[boxY * stride + boxX * pixelSize];
        //                    variance += (value - mean) * (value - mean);
        //                }
        //            }

        //            variance /= count - 1;
        //            var threshold = mean + 0.2 * Math.Sqrt(variance);

        //            var index = y * stride + x * pixelSize;
        //            var binaryValue = (byte)((pixelBuffer[index] > threshold) ? 255 : 0);
        //            ptr[index] = ptr[index + 1] = ptr[index + 2] = binaryValue;
        //            if (pixelSize == 4)
        //                ptr[index + 3] = pixelBuffer[index + 4];
        //        }
        //    }

        //    bitmap.UnlockBits(data);
        //    return bitmap;
        //}

        public unsafe static Bitmap Niblack(Bitmap bitmap)
        {
            var data = bitmap.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat);

            var height = data.Height;
            var width = data.Width;
            var dataLength = height * width;
            var radius = 15;
            var boxSize = radius * 2;
            var stride = data.Stride;
            var pixelSize = Image.GetPixelFormatSize(data.PixelFormat) / 8;
            //var offset = stride - width * pixelSize;
            var pixelBuffer = new byte[data.Stride * data.Height];

            Marshal.Copy(data.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            for (int i = 0; i < dataLength; i += 4)
            {
                var avg = (byte)((pixelBuffer[i] + pixelBuffer[i + 1] + pixelBuffer[i + 2]) / 3);
                pixelBuffer[i] = pixelBuffer[i + 1] = pixelBuffer[i + 2] = avg;
            }

            var ptr = (byte*)data.Scan0.ToPointer();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var sum = 0;
                    var count = 0;
                    var byteOffset = y * stride + x * pixelSize;

                    for (int boxY = -radius; boxY < boxSize; boxY++)
                    {
                        int calcOffset = byteOffset + boxY * stride;

                        if (calcOffset < 0)
                            continue;

                        if (calcOffset >= height)
                            break;

                        for (int boxX = -radius; boxX < boxSize; boxX++)
                        {
                            calcOffset += boxX * pixelSize;

                            if (calcOffset < 0)
                                continue;

                            if (calcOffset >= width)
                                continue;

                            sum += pixelBuffer[calcOffset];
                            count++;
                        }
                    }

                    var mean = (double)sum / count;
                    var variance = 0.0;

                    for (int boxY = -radius; boxY < boxSize; boxY++)
                    {
                        int calcOffset = byteOffset + boxY * stride;

                        if (calcOffset < 0)
                            continue;

                        if (calcOffset >= height)
                            break;

                        for (int boxX = -radius; boxX < boxSize; boxX++)
                        {
                            calcOffset += boxX * pixelSize;

                            if (calcOffset < 0)
                                continue;

                            if (calcOffset >= width)
                                continue;

                            var value = pixelBuffer[calcOffset];
                            variance += (value - mean) * (value - mean);
                        }
                    }

                    variance /= count - 1;
                    var threshold = mean + 0.2 * Math.Sqrt(variance);

                    var binaryValue = (byte)((pixelBuffer[byteOffset] > threshold) ? 255 : 0);
                    ptr[byteOffset] = ptr[byteOffset + 1] = ptr[byteOffset + 2] = binaryValue;
                    if (pixelSize == 4)
                        ptr[byteOffset + 3] = pixelBuffer[byteOffset + 3];
                }
            }

            bitmap.UnlockBits(data);
            return bitmap;
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
