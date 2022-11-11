using LiveCharts;
using LiveCharts.Wpf;
using System.Drawing;
using System.Linq;

namespace GrafikaPS4
{
    public class Histogram
    {
        private int[] _rData;
        private int[] _gData;
        private int[] _bData;

        //public int[] Data { get => _data; }

        public Histogram()
        {
        }

        public void Refresh(Bitmap bitmap)
        {
            _rData = new int[256];
            _gData = new int[256];
            _bData = new int[256];

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    var color = bitmap.GetPixel(j, i);
                    _rData[color.R]++;
                    _gData[color.G]++;
                    _bData[color.B]++;
                }
            }
        }

        public SeriesCollection GetRefreshedSeriesCollection(Bitmap bitmap)
        {
            Refresh(bitmap);
            var result = new SeriesCollection
            {
                GetStackedAreaSeries(System.Windows.Media.Brushes.Red, _rData, "R"),
                GetStackedAreaSeries(System.Windows.Media.Brushes.Green, _gData, "G"),
                GetStackedAreaSeries(System.Windows.Media.Brushes.Blue, _bData, "B"),
            };
            return result;
        }

        private StackedAreaSeries GetStackedAreaSeries(System.Windows.Media.Brush brush, int[] data, string title)
        {
            return new StackedAreaSeries
            {
                Title = title,
                Values = new ChartValues<int>(data),
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2
            };
        }

        public Bitmap Stretch(Bitmap bitmap)
        {
            var minRValue = GetMinIndex(_rData);
            var minGValue = GetMinIndex(_gData);
            var minBValue = GetMinIndex(_bData);
            var maxRValue = GetMaxIndex(_rData);
            var maxGValue = GetMaxIndex(_gData);
            var maxBValue = GetMaxIndex(_bData);

            var factorR = 255.0 / (maxRValue - minRValue);
            var factorG = 255.0 / (maxGValue - minGValue);
            var factorB = 255.0 / (maxBValue - minBValue);

            var lutR = new int[256];
            var lutG = new int[256];
            var lutB = new int[256];

            for (int i = 0; i < 256; i++)
            {
                lutR[i] = (int)(factorR * (i - minRValue));
                lutG[i] = (int)(factorG * (i - minGValue));
                lutB[i] = (int)(factorB * (i - minBValue));
            }

            var result = LutUtils.ApplyRGBLut(bitmap, lutR, lutG, lutB);
            return result;
        }



        public Bitmap Align(Bitmap bitmap)
        {
            var allPixelsCount = bitmap.Width * bitmap.Height;

            var minRValue = _rData.First(x => x != 0);
            var minGValue = _bData.First(x => x != 0);
            var minBValue = _gData.First(x => x != 0);

            var dividendR = allPixelsCount - minRValue;
            var dividendG = allPixelsCount - minGValue;
            var dividendB = allPixelsCount - minBValue;

            var sumR = 0;
            var sumG = 0;
            var sumB = 0;

            var lutR = new int[256];
            var lutG = new int[256];
            var lutB = new int[256];

            for (int i = 0; i < 256; i++)
            {
                sumR += _rData[i];
                sumG += _gData[i];
                sumB += _bData[i];

                if (sumR > minRValue)
                    lutR[i] = (int)(((double)(sumR - minRValue) / dividendR) * 255);

                if (sumG > minGValue)
                    lutG[i] = (int)(((double)(sumG - minGValue) / dividendG) * 255);

                if (sumB > minBValue)
                    lutB[i] = (int)(((double)(sumB - minBValue) / dividendB) * 255);
            }

            var result = LutUtils.ApplyRGBLut(bitmap, lutR, lutG, lutB);
            return result;
        }

        private int GetMinIndex(int[] array)
        {
            for (int i = 0; i < 255; i++)
            {
                if (array[i] != 0)
                    return i;
            }
            return 255;
        }

        private int GetMaxIndex(int[] array)
        {
            for (int i = 255; i >= 0; i--)
            {
                if (array[i] != 0)
                    return i;
            }
            return 0;
        }
    }
}
