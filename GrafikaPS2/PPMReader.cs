using Microsoft.Win32;
using System;
using System.Drawing;

namespace GrafikaPS2
{
    public class PPMReader
    {
        public string Format { get; set; }
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int MaxColor { get; set; } = -1;

        public Bitmap Bitmap { get; set; }

        private readonly OpenFileDialog _dialog;

        private const string asciiFormat = "P3";

        private const string binaryFormat = "P6";

        public PPMReader(OpenFileDialog dialog)
        {
            _dialog = dialog;
        }

        public bool ReadFile()
        {
            try
            {
                using (var lineReader = new FileLineReader(_dialog.FileName))
                {
                    if (!SetFormat(lineReader))
                    {
                        return false;
                    }

                    Width = lineReader.GetNextIntValue();
                    Height = lineReader.GetNextIntValue();
                    MaxColor = lineReader.GetNextIntValue();

                    Bitmap = new Bitmap(Width, Height);

                    if (Format == "P3")
                    {
                        ReadASCIIFormat(lineReader);
                    }
                    else
                    {
                        ReadBinaryFormat();
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        protected bool SetFormat(FileLineReader lineReader)
        {
            Format = lineReader.GetNextStringValue();
            return Format == asciiFormat || Format == binaryFormat;
        }

        private void ReadASCIIFormat(FileLineReader lineReader)
        {
            var is16Bit = MaxColor > 255;

            for (int i = 0; i < Height; i++)
            {
                var pixelsValues = new string[3 * Width];
                var counter = 0;
                var position = 0;

                while (counter != 3 * Width)
                {
                    pixelsValues[counter++] = lineReader.GetNextStringValue();

                    if (counter % 3 == 0)
                    {
                        if (is16Bit)
                            Bitmap.SetPixel(position, i, Color.FromArgb(Int32.Parse(pixelsValues[counter - 3]) >> 8, Int32.Parse(pixelsValues[counter - 2]) >> 8, Int32.Parse(pixelsValues[counter - 1]) >> 8));
                        else
                            Bitmap.SetPixel(position, i, Color.FromArgb(Int32.Parse(pixelsValues[counter - 3]), Int32.Parse(pixelsValues[counter - 2]), Int32.Parse(pixelsValues[counter - 1])));

                        position++;
                    }
                }
            }
        }

        private void ReadBinaryFormat()
        {
            var stream = _dialog.OpenFile();
            stream.Position = stream.Length - (Width * Height * 3);

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Bitmap.SetPixel(j, i, Color.FromArgb(stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
                }
            }
        }
    }
}
