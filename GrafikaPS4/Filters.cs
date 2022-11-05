using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GrafikaPS4
{
    public class Filters
    {
        public static Bitmap Median(Bitmap bitmap)
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

                    red.Sort();
                    green.Sort();
                    blue.Sort();


                    resultBuffer[byteOffset] = (byte)red[red.Count / 2];
                    resultBuffer[byteOffset + 1] = (byte)green[green.Count / 2];
                    resultBuffer[byteOffset + 2] = (byte)blue[blue.Count / 2];
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
