using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace GrafikaPS4
{
    public class Filters
    {
        private delegate void SetResultBufferSE(byte[] buffer, int byteOffset, List<int> red, List<int> green, List<int> blue);

        private delegate byte GetHitOrMissPixelByteValue(bool isFitted);

        private const int _hitOrMissAreaLength = 9;

        public static Bitmap Median(Bitmap bitmap)
        {
            void action(byte[] buffer, int byteOffset, List<int> red, List<int> green, List<int> blue)
            {
                red.Sort();
                green.Sort();
                blue.Sort();

                buffer[byteOffset] = (byte)red[red.Count / 2];
                buffer[byteOffset + 1] = (byte)green[green.Count / 2];
                buffer[byteOffset + 2] = (byte)blue[blue.Count / 2];
            }

            return ApplySEFilter(bitmap, action);
        }

        public static Bitmap Dilatation(Bitmap bitmap)
        {
            void action(byte[] buffer, int byteOffset, List<int> red, List<int> green, List<int> blue)
            {
                buffer[byteOffset] = (byte)red.Min();
                buffer[byteOffset + 1] = (byte)green.Min();
                buffer[byteOffset + 2] = (byte)blue.Min();
            }

            return ApplySEFilter(bitmap, action);
        }

        public static Bitmap Erosion(Bitmap bitmap)
        {
            void action(byte[] buffer, int byteOffset, List<int> red, List<int> green, List<int> blue)
            {
                buffer[byteOffset] = (byte)red.Max();
                buffer[byteOffset + 1] = (byte)green.Max();
                buffer[byteOffset + 2] = (byte)blue.Max();
            }

            return ApplySEFilter(bitmap, action);
        }

        public static Bitmap Opening(Bitmap bitmap)
        {
            var erosionResult = Erosion(bitmap);

            return Dilatation(erosionResult);
        }

        public static Bitmap Closing(Bitmap bitmap)
        {
            var dilatationResult = Dilatation(bitmap);

            return Erosion(dilatationResult);
        }

        public static Bitmap ApplyHitOrMiss(Bitmap bitmap, bool isThinning, int[] se)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0,
                                        bitmap.Width, bitmap.Height),
                                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            var pixelBuffer = new byte[bitmapData.Stride * bitmapData.Height];
            Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY < bitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < bitmap.Width - filterOffset; offsetX++)
                {
                    var area = new byte[9];
                    var areaIndex = 0;
                    byteOffset = offsetY * bitmapData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * bitmapData.Stride);
                            area[areaIndex++] = pixelBuffer[calcOffset];
                        }
                    }

                    var isFitted = true;
                    for (int i = 0; i < _hitOrMissAreaLength; i++)
                    {
                        if ((se[i] == 1 && area[i] != 255) || se[i] == 0 && area[i] != 0)
                        {
                            isFitted = false;
                            break;
                        }
                    }

                    if (isFitted)
                    {
                        var newValue = isThinning ? 0 : 255;
                        pixelBuffer[byteOffset] = pixelBuffer[byteOffset + 1] = pixelBuffer[byteOffset + 2] = (byte)newValue;
                    }
                }
            }

            Marshal.Copy(pixelBuffer, 0, bitmapData.Scan0, pixelBuffer.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private static Bitmap ApplySEFilter(Bitmap bitmap, SetResultBufferSE action)
        {
            var sourceData = bitmap.LockBits(new Rectangle(0, 0,
                                        bitmap.Width, bitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            var resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            bitmap.UnlockBits(sourceData);

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            for (int offsetY = filterOffset; offsetY < bitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < bitmap.Width - filterOffset; offsetX++)
                {
                    var red = new List<int>();
                    var green = new List<int>();
                    var blue = new List<int>();
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;


                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            red.Add(pixelBuffer[calcOffset]);
                            green.Add(pixelBuffer[calcOffset + 1]);
                            blue.Add(pixelBuffer[calcOffset + 2]);
                        }
                    }

                    action(resultBuffer, byteOffset, red, green, blue);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            var resultBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            var resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                    resultBitmap.Width, resultBitmap.Height),
                                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
    }
}
