using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //public Bitmap Align(Bitmap bitmap)
        //{
        //    var allPixelsCount = bitmap.Width * bitmap.Height;

        //    var distributionR = new double[256];
        //    var distributionG = new double[256];
        //    var distributionB = new double[256];

        //    for (int i = 0; i < 255; i++)
        //    {
        //        distributionR[i] = (double)_rData[i] / allPixelsCount;
        //        distributionG[i] = (double)_gData[i] / allPixelsCount;
        //        distributionB[i] = (double)_bData[i] / allPixelsCount;
        //    }

        //    var firstNonZeroDistributionR = distributionR.First(x => x != 0);
        //    var firstNonZeroDistributionG = distributionG.First(x => x != 0);
        //    var firstNonZeroDistributionB = distributionB.First(x => x != 0);

        //    var dividendR = 1 - firstNonZeroDistributionR;
        //    var dividendG = 1 - firstNonZeroDistributionG;
        //    var dividendB = 1 - firstNonZeroDistributionB;

        //    var lutR = new int[256];
        //    var lutG = new int[256];
        //    var lutB = new int[256];

        //    for (int i = 0; i < 256; i++)
        //    {
        //        lutR[i] = (int)(((distributionR[i] - firstNonZeroDistributionR) / dividendR) * 255);
        //        lutG[i] = (int)(((distributionG[i] - firstNonZeroDistributionG) / dividendG) * 255);
        //        lutB[i] = (int)(((distributionB[i] - firstNonZeroDistributionB) / dividendB) * 255);
        //    }

        //    var result = LutUtils.ApplyRGBLut(bitmap, lutR, lutG, lutB);
        //    return result;
        //}

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
    }
}
