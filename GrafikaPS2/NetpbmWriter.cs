using System;
using System.Drawing;
using System.IO;

namespace GrafikaPS2
{
    public class NetpbmWriter : IDisposable
    {
        private readonly Bitmap _bitmap;
        private readonly string _fileName;
        private readonly string _format;
        private readonly bool _isAscii;

        private const int _maxColor = 255;

        private StreamWriter _streamWriter;

        private delegate string ColorStringGetter(Color color);

        private ColorStringGetter _colorStringGetter;

        private int _colorStringLength;

        private string _colorValueSeparator;

        public NetpbmWriter(Bitmap bitmap, string extension, string fileName, string format, bool isAscii = false)
        {
            _bitmap = bitmap;
            _fileName = fileName;
            _format = format;
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
                _streamWriter.WriteLine(_format);
                _streamWriter.WriteLine($"{_bitmap.Width} {_bitmap.Height}");

                _colorValueSeparator = " ";
                SetFormatProperties();

                var maxLineLength = 70 - _colorStringLength;
                var lineLength = 0;
                for (int i = 0; i < _bitmap.Height; i++)
                {
                    for (int j = 0; j < _bitmap.Width; j++)
                    {
                        var color = _bitmap.GetPixel(j, i);
                        var colorString = _colorStringGetter(color);

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
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetFormatProperties()
        {
            switch (_format)
            {
                case "P1":
                    _colorStringGetter = color =>
                    {
                        var avg = (color.R + color.G + color.B) / 3;
                        return avg >= 128 ? "0" : "1";
                    };
                    _colorStringLength = 1;
                    break;
            }
        }

        private bool IsEndOfBitmap(int x, int y)
        {
            return x == _bitmap.Width - 1 && y == _bitmap.Height - 1;
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}