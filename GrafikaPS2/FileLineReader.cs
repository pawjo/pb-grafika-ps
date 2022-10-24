using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrafikaPS2
{
    public class FileLineReader : IDisposable
    {
        public List<string> Comments { get; set; }

        public Stream Stream { get => _streamReader.BaseStream; }

        private readonly StreamReader _streamReader;
        private string[] _lineValues;
        private int _lineValueIndex = 35;
        private int _singleBitIndex = 70;
        private bool _isEndOfFile = false;

        public FileLineReader(string fileName)
        {
            _streamReader = new StreamReader(fileName);
            Comments = new List<string>();
            GetNextLine();
        }

        public string GetNextStringValue()
        {
            if (_lineValueIndex >= _lineValues.Count())
            {
                GetNextLine();
            }

            return _lineValues[_lineValueIndex++];

        }

        public int GetNextIntValue()
        {
            var str = GetNextStringValue();
            return int.Parse(str);
        }

        public bool GetNextSingleBitValue()
        {
            if (_lineValueIndex >= _lineValues.Count())
            {
                GetNextLine();
                _singleBitIndex = 0;
            }

            if (_singleBitIndex == _lineValues[_lineValueIndex].Length)
            {
                _lineValueIndex++;
                _singleBitIndex = 0;
                return GetNextSingleBitValue();
            }
            

            var val = _lineValues[_lineValueIndex][_singleBitIndex++];

            if (val == '1')
            {
                return true;
            }
            else if (val == '0')
            {
                return false;
            }
            else
            {
                throw new Exception("It is not bit value");
            }
        }

        public void Dispose()
        {
            _streamReader.Dispose();
        }

        private void GetNextLine()
        {
            if (_streamReader.EndOfStream)
            {
                throw new Exception("End of file");
            }

            _lineValueIndex = 0;
            _singleBitIndex = 0;
            var line = _streamReader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                GetNextLine();
                return;
            }

            if (line.StartsWith('#'))
            {
                Comments.Add(line.Substring(1));
                GetNextLine();
                return;
            }

            var commentSignIndex = line.IndexOf('#');
            if (commentSignIndex != -1)
            {
                Comments.Add(line.Substring(commentSignIndex + 1));
                line = line.Substring(0, commentSignIndex);
            }

            _lineValues = line.Split(' ', '\t').Where(v => v != "" && v != "\t").ToArray();
        }
    }
}
