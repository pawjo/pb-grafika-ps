using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace GrafikaPS2
{
    public class NetpbmWriter : IDisposable
    {
        private readonly Bitmap _bitmap;
        private readonly string _fileName;
        private readonly string _destinationFormat;
        private readonly int _sourceFormat;
        private readonly bool _isAscii;

        private const int _maxColor = 255;

        private StreamWriter _streamWriter;

        private Stream _fileStream;

        private delegate string ColorStringGetter(Color color);

        private ColorStringGetter _colorStringGetter;

        private delegate void WritePixelValueToStream(Color color);

        private WritePixelValueToStream _writePixelValueToStream;

        private int _colorStringLength;

        private string _colorValueSeparator;

        public NetpbmWriter(Bitmap bitmap, string extension, string fileName, int sourceFormat, string destinationFormat, bool isAscii = false)
        {
            _bitmap = bitmap;
            _fileName = fileName;
            _destinationFormat = destinationFormat;
            _sourceFormat = sourceFormat;
            _isAscii = isAscii;

            _fileName = fileName;
            extension = '.' + extension;
            if (!_fileName.EndsWith(extension))
            {
                _fileName += extension;
            }
        }

        public bool Write()
        {
            try
            {
                _streamWriter = new StreamWriter(_fileName);
                _streamWriter.NewLine = "\n";
                _streamWriter.WriteLine(_destinationFormat);
                _streamWriter.WriteLine($"{_bitmap.Width} {_bitmap.Height}");

                switch (_destinationFormat)
                {
                    case "P1":
                        _colorStringGetter = color =>
                        {
                            var avg = GetAverageColorValue(color);
                            return avg >= 128 ? "0" : "1";
                        };
                        _colorStringLength = 1;
                        _colorValueSeparator = " ";
                        WriteAsciiPixelsValues();
                        break;
                    case "P4":
                        _streamWriter.Close();
                        WriteP4();
                        break;
                    case "P2":
                        _streamWriter.WriteLine(_maxColor);
                        _colorStringGetter = color =>
                        {
                            int avg = GetAverageColorValue(color);
                            return GetNormalizedString(avg, 3);
                        };
                        _colorStringLength = 3;
                        _colorValueSeparator = " ";
                        WriteAsciiPixelsValues();
                        break;
                    case "P5":
                        _streamWriter.WriteLine(_maxColor);
                        _writePixelValueToStream = color =>
                        {
                            int avg = GetAverageColorValue(color);
                            _fileStream.WriteByte((byte)avg);
                        };
                        WriteBinaryPixelValues();
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private int GetAverageColorValue(Color color)
        {
            return _sourceFormat == 3 ? ((color.R + color.G + color.B) / 3) : color.R;
        }

        private void WriteAsciiPixelsValues()
        {
            var maxLineLength = 70 - 2 * _colorStringLength - _colorValueSeparator.Length;
            var lineLength = 0;
            var counter = 0;
            for (int i = 0; i < _bitmap.Height; i++)
            {
                for (int j = 0; j < _bitmap.Width; j++)
                {
                    var color = _bitmap.GetPixel(j, i);
                    var colorString = _colorStringGetter(color);
                    counter++;
                    //if(counter == 8)
                    //{
                    //    colorString = colorString + " |";
                    //    counter = 0;
                    //}

                    if (lineLength == 0)
                    {
                        _streamWriter.Write(colorString);
                        lineLength = _colorStringLength;
                    }
                    else if (lineLength >= maxLineLength && !IsEndOfBitmap(j, i))
                    {
                        _streamWriter.WriteLine(_colorValueSeparator + colorString);
                        lineLength = 0;
                    }
                    else
                    {
                        _streamWriter.Write(_colorValueSeparator + colorString);
                        lineLength += _colorStringLength + 1;
                    }
                }
            }
        }

        private void WriteP4()
        {
            _fileStream = new FileStream(_fileName, FileMode.Append);
            var currentByte = 0;
            var currentByteLength = 0;
            for (int i = 0; i < _bitmap.Height; i++)
            {
                for (int j = 0; j < _bitmap.Width; j++)
                {
                    var color = _bitmap.GetPixel(j, i);
                    var avg = GetAverageColorValue(color);
                    var val = avg >= 128 ? 0 : 1;
                    currentByte <<= 1;
                    currentByte |= val;
                    currentByteLength++;

                    if (currentByteLength == 8)
                    {
                        WriteAndResetCurrentByte(j, i);
                    }
                }
                if (currentByteLength > 1)
                {
                    currentByte <<= (8 - currentByteLength);
                    WriteAndResetCurrentByte(_bitmap.Width, i);
                }
            }


            void WriteAndResetCurrentByte(int x, int y)
            {
                var m = (x + 1) * (y + 1);
                _fileStream.WriteByte((byte)currentByte);
                currentByte = 0;
                currentByteLength = 0;
            }
            _fileStream.Close();
        }


        private void WriteBinaryPixelValues()
        {
            _streamWriter.Close();
            _fileStream = new FileStream(_fileName, FileMode.Append);
            for (int i = 0; i < _bitmap.Height; i++)
            {
                for (int j = 0; j < _bitmap.Width; j++)
                {
                    var color = _bitmap.GetPixel(j, i);
                    _writePixelValueToStream(color);
                }
            }
        }

        //private void SetFormatProperties()
        //{
        //    switch (_format)
        //    {
        //        case "P1":
        //            _colorStringGetter = color =>
        //            {
        //                var avg = (color.R + color.G + color.B) / 3;
        //                return avg >= 128 ? "0" : "1";
        //            };
        //            _colorStringLength = 1;
        //            break;
        //        case "P4":
        //            _writePixelValueToStream = color =>
        //            {
        //                var avg = (color.R + color.G + color.B) / 3;

        //            }
        //    }
        //}

        private bool IsEndOfBitmap(int x, int y)
        {
            return x == _bitmap.Width - 1 && y == _bitmap.Height - 1;
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _fileStream?.Dispose();
        }

        private string GetNormalizedString(int val, int length)
        {
            var str = val.ToString();
            if (val > 99)
            {
                return str;
            }
            else if (val > 9)
            {
                return " " + str;
            }
            return "  " + str;
        }
    }
}