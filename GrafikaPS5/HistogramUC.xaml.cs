using LiveCharts;
using LiveCharts.Wpf;
using System.Drawing;
using System.Windows.Controls;

namespace GrafikaPS4
{
    /// <summary>
    /// Interaction logic for HistogramUC.xaml
    /// </summary>
    public partial class HistogramUC : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        //public string[] Labels { get; set; }

        public Bitmap Bitmap { get; set; }

        public HistogramUC()
        {
            InitializeComponent();

            var histogramData = new int[256];

            for (int i = 0; i < Bitmap.Height; i++)
            {
                for (int j = 0; j < Bitmap.Width; j++)
                {
                    var color = Bitmap.GetPixel(j, i);
                    var index = (color.R + color.G + color.B) / 3;
                    histogramData[index]++;
                }
            }

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title="RGB",
                    Values=new ChartValues<int> (histogramData)
                }
            };

                      

            DataContext = this;
        }

    }
}
