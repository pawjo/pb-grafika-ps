using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace GrafikaPS2
{
        public class PPM
        {
            public string Format { get; set; }
            public int Width { get; set; } = 0;
            public int Height { get; set; } = 0;
            public int MaxColor { get; set; } = -1;
            public RGB[,] Pixels { get; set; }
            public bool Is16Bit = false;
            public Bitmap Bitmap { get; set; }

            public PPM(OpenFileDialog openJPEGDialong)
            {
                var file = openJPEGDialong.FileName;
                var stream = openJPEGDialong.OpenFile();
                var arg = -1;

                using (var streamReader = new StreamReader(file))
                {
                    var line = streamReader.ReadLine();
                    var ppmFormat = line;
                    if (ppmFormat != "P3" && ppmFormat != "P6")
                        return;

                    Format = line;

                    while (MaxColor == -1)
                    {
                        line = streamReader.ReadLine();

                        if (line.StartsWith("#") || line.Length == 0)
                            continue;

                        if (line.Contains("#"))
                            line = line.Substring(0, line.IndexOf('#') - 1);

                        var values = line.Split(new char[] { ' ', '\t' }).Where(v => v != "" && v != "\t");

                        foreach (var value in values)
                        {
                            if (Int32.TryParse(value, out var val))
                            {
                                if (Width == 0)
                                    Width = val;
                                else if (Height == 0)
                                    Height = val;
                                else if (MaxColor == -1)
                                    MaxColor = val;
                                else
                                    arg = val;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }

                    Pixels = new RGB[Width, Height];
                    Is16Bit = MaxColor > 255 ? true : false;

                    stream.Position = stream.Length - (Width * Height * 3);
                    Bitmap = new Bitmap(Width, Height);

                    if (ppmFormat == "P3")
                    {
                        for (int i = 0; i < Height; i++)
                        {
                            var pixelsValues = new string[3 * Width];
                            var counter = 0;
                            var position = 0;

                            while (counter != 3 * Width)
                            {
                                if (arg != -1 && i == 0)
                                {
                                    pixelsValues[counter] = arg.ToString();
                                    counter++;
                                    continue;
                                }

                                line = streamReader.ReadLine();

                                if (line.StartsWith("#") || line.Length == 0)
                                    continue;

                                if (line.Contains("#"))
                                    line = line.Substring(0, line.IndexOf('#') - 1);

                                var values = line.Split(new char[] { ' ', '\t' }).Where(v => v != "" && v != "\t");

                                foreach (var item in values)
                                {
                                    pixelsValues[counter] = item;
                                    counter++;

                                    if (counter % 3 == 0)
                                    {
                                        if (Is16Bit)
                                            Bitmap.SetPixel(position, i, System.Drawing.Color.FromArgb(Int32.Parse(pixelsValues[counter - 3]) >> 8, Int32.Parse(pixelsValues[counter - 2]) >> 8, Int32.Parse(pixelsValues[counter - 1]) >> 8));
                                        else
                                            Bitmap.SetPixel(position, i, System.Drawing.Color.FromArgb(Int32.Parse(pixelsValues[counter - 3]), Int32.Parse(pixelsValues[counter - 2]), Int32.Parse(pixelsValues[counter - 1])));

                                        position++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Height; i++)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                try
                                {
                                    Bitmap.SetPixel(j, i, System.Drawing.Color.FromArgb(stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Nieprawidłowy format pliku");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
