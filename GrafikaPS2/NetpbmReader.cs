using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GrafikaPS2
{
    public class NetpbmReader : IDisposable
    {
        public string Format { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MaxColor { get; set; }

        public Bitmap Bitmap { get; set; }

        public List<string> Comments { get => _lineReader.Comments; }

        private readonly OpenFileDialog _dialog;

        private readonly string[] _asciiFormats = { "P1", "P2", "P3" };

        private readonly string[] _binaryFormats = { "P4", "P5", "P6" };

        private int _formatIndex;

        private bool _isAscii;

        private readonly bool _isMaxColorNeeded;

        private bool _is16bit;

        private FileLineReader _lineReader;

        private Stream _stream;

        private delegate Color ColorGetter();

        private int _currentPBMBinaryByte;
        private int _currentPBMBinaryByteIndex = -1;
        private int _currentPBMBinaryWidthIndex = 0;

        public NetpbmReader(OpenFileDialog dialog)
        {
            _dialog = dialog;
        }

        public bool ReadFile()
        {
            _lineReader = new FileLineReader(_dialog.FileName);

            if (!SetFormat(_lineReader))
            {
                return false;
            }

            if (_lineReader.Stream.Length == 0)
            {
                return false;
            }

            Width = _lineReader.GetNextIntValue();
            Height = _lineReader.GetNextIntValue();

            if (_formatIndex > 1)
            {
                MaxColor = _lineReader.GetNextIntValue();
                _is16bit = MaxColor > 255;
            }

            Bitmap = new Bitmap(Width, Height);
            ColorGetter getter = null;

            if (!_isAscii)
            {
                _stream = _dialog.OpenFile();
            }

            switch (_formatIndex, _isAscii, _is16bit)
            {
                case (0, true, false): // PBM ASCII
                    getter = () => _lineReader.GetNextSingleBitValue() ? Color.Black : Color.White;
                    break;
                case (0, false, false): // PBM binary
                    int bytesRowCount = Width / 8;
                    if (Width % 8 > 0)
                    {
                        bytesRowCount++;
                    }
                    SetStreamPositon(bytesRowCount * Height);
                    getter = () =>
                    {
                        if (_currentPBMBinaryWidthIndex == Width && _currentPBMBinaryByteIndex == 7)
                        {
                            _currentPBMBinaryWidthIndex = 0;
                        }
                        else if (_currentPBMBinaryWidthIndex == Width)
                        {
                            _currentPBMBinaryWidthIndex = 0;
                            _currentPBMBinaryByte = _stream.ReadByte();
                            _currentPBMBinaryByteIndex = 7;
                        }
                        else if (_currentPBMBinaryByteIndex == -1)
                        {
                            _currentPBMBinaryByte = _stream.ReadByte();
                            _currentPBMBinaryByteIndex = 7;
                        }
                        var val = _currentPBMBinaryByte >> _currentPBMBinaryByteIndex;
                        val %= 2;
                        _currentPBMBinaryByteIndex--;
                        _currentPBMBinaryWidthIndex++;

                        return val == 1 ? Color.Black : Color.White;
                    };
                    break;
                case (1, true, false): // PGM ASCII
                    getter = () =>
                    {
                        var value = _lineReader.GetNextIntValue();
                        return Color.FromArgb(value, value, value);
                    };
                    break;
                case (1, true, true): // PGM ASCII 16 bit
                    getter = () =>
                    {
                        var value = _lineReader.GetNextIntValue() >> 8;
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
                    getter = () => Color.FromArgb(_lineReader.GetNextIntValue(), _lineReader.GetNextIntValue(), _lineReader.GetNextIntValue());
                    break;
                case (2, true, true): // PPM ASCII 16 bit
                    getter = () => Color.FromArgb(_lineReader.GetNextIntValue() >> 8, _lineReader.GetNextIntValue() >> 8, _lineReader.GetNextIntValue() >> 8);
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
                    var color = getter();
                    Bitmap.SetPixel(j, i, color);
                }
            }

            return true;
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

        public void Dispose()
        {
            _lineReader?.Dispose();
            _stream?.Dispose();
        }
    }
}