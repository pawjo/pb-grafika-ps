using Microsoft.Win32;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GrafikaPS2
{
    public class NetpbmReader
    {
        public string Format { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MaxColor { get; set; }

        public Bitmap Bitmap { get; set; }

        public List<string> Comments { get => lineReader.Comments; }

        private readonly OpenFileDialog _dialog;

        private readonly string[] _asciiFormats = { "P1", "P2", "P3" };

        private readonly string[] _binaryFormats = { "P4", "P5", "P6" };

        private int _formatIndex;

        private bool _isAscii;

        private readonly bool _isMaxColorNeeded;

        private bool _is16bit;

        private FileLineReader lineReader;

        private Stream _stream;

        private delegate Color ColorGetter();

        public NetpbmReader(OpenFileDialog dialog)
        {
            _dialog = dialog;
        }

        public bool ReadFile()
        {
            try
            {
                lineReader = new FileLineReader(_dialog.FileName);

                if (!SetFormat(lineReader))
                {
                    lineReader.Dispose();
                    return false;
                }

                Width = lineReader.GetNextIntValue();
                Height = lineReader.GetNextIntValue();

                if (_formatIndex > 1)
                {
                    MaxColor = lineReader.GetNextIntValue();
                    _is16bit = MaxColor > 255;
                }

                Bitmap = new Bitmap(Width, Height);
                ColorGetter getter = null;

                if (!_isAscii)
                {
                    _stream = _dialog.OpenFile();
                    _stream.Position = _stream.Length - (Width * Height);
                }

                switch (_formatIndex, _isAscii, _is16bit)
                {
                    case (0, true, false): // PBM ASCII
                        getter = () => lineReader.GetNextSingleBitValue() ? Color.Black : Color.White;
                        break;
                    case (0, false, false): // PBM binary
                        int bitsCount = Width * Height;
                        int bytesCount = bitsCount / 8;
                        if (bitsCount % 8 > 0)
                        {
                            bytesCount++;
                        }
                        SetStreamPositon(bytesCount);
                        getter = () => lineReader.GetNextSingleBitValue() ? Color.Black : Color.White;
                        break;
                    case (1, true, false): // PGM ASCII
                        getter = () =>
                        {
                            var value = lineReader.GetNextIntValue();
                            return Color.FromArgb(value, value, value);
                        };
                        break;
                    case (1, true, true): // PGM ASCII 16 bit
                        getter = () =>
                        {
                            var value = lineReader.GetNextIntValue() >> 8;
                            return Color.FromArgb(value, value, value);
                        };
                        break;
                    case (1, false, false): // PGM binary
                        SetStreamPositon(Width * Height);
                        getter = () =>
                        {
                            var value = _stream.ReadByte();
                            return Color.FromArgb(value, value, value);
                        };
                        break;
                    case (1, false, true): // PGM binary 16 bit
                        SetStreamPositon(Width * Height * 2);
                        getter = () =>
                        {
                            var value = Read16BitFromStream();
                            return Color.FromArgb(value, value, value);
                        };
                        break;
                    case (2, true, false): // PPM ASCII
                        getter = () => Color.FromArgb(lineReader.GetNextIntValue(), lineReader.GetNextIntValue(), lineReader.GetNextIntValue());
                        break;
                    case (2, true, true): // PPM ASCII 16 bit
                        getter = () => Color.FromArgb(lineReader.GetNextIntValue() >> 8, lineReader.GetNextIntValue() >> 8, lineReader.GetNextIntValue() >> 8);
                        break;
                    case (2, false, false): // PPM binary
                        SetStreamPositon(Width * Height * 3);
                        getter = () => Color.FromArgb(_stream.ReadByte(), _stream.ReadByte(), _stream.ReadByte());
                        break;
                    case (2, false, true): // PPM binary 16 bit
                        SetStreamPositon(Width * Height * 6);
                        getter = () => Color.FromArgb(Read16BitFromStream(), Read16BitFromStream(), Read16BitFromStream());
                        break;
                }

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        Bitmap.SetPixel(j, i, getter());
                    }
                }

                if (!_isAscii)
                {
                    _stream.Dispose();
                }

                lineReader.Dispose();
                return true;
            }
            catch
            {
                if (_stream != null)
                    _stream.Dispose();
                lineReader.Dispose();
                return false;
            }
        }

        private bool SetFormat(FileLineReader lineReader)
        {
            Format = lineReader.GetNextStringValue();

            for (int i = 0; i < 3; i++)
            {
                if (Format == _asciiFormats[i])
                {
                    _isAscii = true;
                    _formatIndex = i;
                    return true;
                }
                else if (Format == _binaryFormats[i])
                {
                    _formatIndex = i;
                    return true;
                }
            }
            return false;
        }

        private int Read16BitFromStream()
        {
            _stream.ReadByte();
            return _stream.ReadByte();
        }

        private void SetStreamPositon(int offset)
        {
            _stream.Position = _stream.Length - offset;
        }


        //private void ReadPPMASCIIFormat(FileLineReader lineReader)
        //{
        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            Bitmap.SetPixel(j, i, Color.FromArgb(lineReader.GetNextIntValue(), lineReader.GetNextIntValue(), lineReader.GetNextIntValue()));
        //        }
        //    }
        //}

        //private void ReadPPMASCII16BitFormat(FileLineReader lineReader)
        //{
        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            Bitmap.SetPixel(j, i, Color.FromArgb(lineReader.GetNextIntValue() >> 8, lineReader.GetNextIntValue() >> 8, lineReader.GetNextIntValue() >> 8));
        //        }
        //    }
        //}

        //private void ReadPGMASCIIFormat(FileLineReader lineReader)
        //{
        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            var value = lineReader.GetNextIntValue() >> 8;
        //            Bitmap.SetPixel(j, i, Color.FromArgb(value, value, value));
        //        }
        //    }
        //}

        //private void ReadPGMASCII16BitFormat(FileLineReader lineReader)
        //{
        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            var value = lineReader.GetNextIntValue() >> 8;
        //            Bitmap.SetPixel(j, i, Color.FromArgb(value, value, value));
        //        }
        //    }
        //}

        //private void ReadPBMASCIIFormat(FileLineReader lineReader)
        //{
        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            var color = lineReader.GetNextIntValue() == 1 ? Color.Black : Color.White;
        //            Bitmap.SetPixel(j, i, color);
        //        }
        //    }
        //}

        //private void ReadBinaryFormat()
        //{
        //    var stream = _dialog.OpenFile();
        //    stream.Position = stream.Length - (Width * Height * 3);

        //    for (int i = 0; i < Height; i++)
        //    {
        //        for (int j = 0; j < Width; j++)
        //        {
        //            Bitmap.SetPixel(j, i, Color.FromArgb(stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
        //        }
        //    }
        //}
    }
}