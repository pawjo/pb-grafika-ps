using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GrafikaPS4
{
    public class ConvolutionFilters
    {
        public static Bitmap Smooth(Bitmap bitmap)
        {
            double value = 1 / 9;
            var mask = new double[3, 3]
            {
                {value, value, value},
                {value, value, value},
                {value, value, value}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap Sharpen(Bitmap bitmap)
        {
            var mask = new double[3, 3]
            {
                {-1, -1, -1},
                {-1, 9, -1},
                {-1, -1, -1}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap SobelHorizontal(Bitmap bitmap)
        {
            var mask = new double[3, 3]
            {
                {1, 2, 1},
                {0, 0, 0},
                {-1, -2, -1}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap SobelVertical(Bitmap bitmap)
        {
            var mask = new double[3, 3]
            {
                {1, 0, -1},
                {2, 0, -2},
                {1, 0, -1}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap HighPassSharpen(Bitmap bitmap)
        {
            var mask = new double[3, 3]
            {
                {1, -2, 1},
                {-2, 5, -2},
                {1, -2, 1}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap Gauss(Bitmap bitmap)
        {
            var mask = new double[3, 3]
            {
                {1, 2, 1},
                {2, 4, 2},
                {1, 2, 1}
            };

            var result = ApplyFilter(bitmap, mask, 3);
            return result;
        }

        public static Bitmap ApplyFilter(Bitmap sourceBitmap, double[,] matrix, int size)
        {
            var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            var resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;

            int filterOffset = (size - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;

            double weightsSum = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    weightsSum += matrix[i, j];
                }
            }

            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;

                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += (double)(pixelBuffer[calcOffset]) * matrix[filterY + filterOffset, filterX + filterOffset];

                            green += (double)(pixelBuffer[calcOffset + 1]) * matrix[filterY + filterOffset, filterX + filterOffset];

                            red += (double)(pixelBuffer[calcOffset + 2]) * matrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    if (weightsSum != 0 && weightsSum != 1)
                    {
                        blue /= weightsSum;
                        green /= weightsSum;
                        red /= weightsSum;
                    }

                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }


                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }


                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }

                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            var resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                    resultBitmap.Width, resultBitmap.Height),
                                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
    }
}
