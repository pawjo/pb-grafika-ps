using System.Drawing;
using System.IO;

namespace GrafikaPS2
{
    public class NetpbmWriter
    {
        public string Format { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MaxColor { get; set; }

        public Bitmap Bitmap { get; set; }

        public NetpbmWriter(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public bool Write(string fileName)
        {
            if (!fileName.EndsWith(".pbm"))
            {
                fileName += ".pbm";
            }
            try
            {
                using (var sw = new StreamWriter(fileName))
                {
                    sw.WriteLine("P1");
                    sw.WriteLine($"{Bitmap.Width} {Bitmap.Height}");
                    var lineLength = 0;
                    for (int i = 0; i < Bitmap.Height; i++)
                    {
                        for (int j = 0; j < Bitmap.Width; j++)
                        {
                            var color = Bitmap.GetPixel(j, i);
                            var avg = (color.R + color.G + color.B) / 3;
                            var value = avg >= 128 ? '0' : '1';

                            if (lineLength == 0)
                            {
                                sw.Write(value);
                                lineLength = 1;
                            }
                            else if (lineLength >= 68 && !IsEndOfBitmap(j, i))
                            {
                                sw.WriteLine($" {value}");
                                lineLength = 0;
                            }
                            else
                            {
                                sw.Write($" {value}");
                                lineLength += 2;
                            }
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private bool IsEndOfBitmap(int x, int y)
        {
            return x == Bitmap.Width - 1 && y == Bitmap.Height - 1;
        }
    }
}