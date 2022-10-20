using System;
using System.IO;
using System.Linq;

namespace GrafikaPS2
{
    public class FileLineReader : IDisposable
    {
        private readonly StreamReader _streamReader;
        private string[] _lineValues;
        private int _lineValueIndex;
        private bool _isEndOfFile = false;

        public FileLineReader(string fileName)
        {
            _streamReader = new StreamReader(fileName);
            GetNextLine();
        }

        public string GetNextStringValue()
        {
            if (_isEndOfFile)
            {
                return null;
            }

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

        public void Dispose()
        {
            _streamReader.Dispose();
        }

        private void GetNextLine()
        {
            _lineValueIndex = 0;
            var line = _streamReader.ReadLine();
            if (line == null)
            {

            }
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                GetNextLine();
                return;
            }

            var commentSignIndex = line.IndexOf('#');
            if (commentSignIndex != -1)
            {
                line = line.Substring(0, commentSignIndex);
            }

            _lineValues = line.Split(' ', '\t').Where(v => v != "" && v != "\t").ToArray();
        }
    }
}
