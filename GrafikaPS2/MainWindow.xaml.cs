using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GrafikaPS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openJPEGDialong = new OpenFileDialog();

            openJPEGDialong.Filter = "Image files(*.ppm)|*.ppm";
            openJPEGDialong.Title = "Open an Image File";

            if (openJPEGDialong.ShowDialog() == true)
            {
                var read = ReadBitmapFromPPM(openJPEGDialong);
                if (read != null)
                {
                    MainImage.Source = ToBitmapImage(read);
                }
            }
        }

        private Bitmap ReadBitmapFromPPM(OpenFileDialog openJPEGDialong)
        {
            //var ppm = new PPM(openJPEGDialong);
            var ppm = new PPMReader(openJPEGDialong);

            if (!ppm.ReadFile())
            {
                MessageBox.Show("Open file error");
                return null;
            }

            MainImage.Width = ppm.Width;
            MainImage.Height = ppm.Height;

            //if (ppm.Width < 100)
            //{
            //    MainImage.Width = 20 * ppm.Width;
            //    MainImage.Height = 20 * ppm.Width;
            //}

            //if (ppm.Width > 1000)
            //{
            //    MainImage.Width = ppm.Width / 10;
            //    MainImage.Height = ppm.Width / 10;
            //}

            return ppm.Bitmap;
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
