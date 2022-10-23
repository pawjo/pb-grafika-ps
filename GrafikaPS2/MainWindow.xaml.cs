using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrafikaPS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Point origin;  // Original Offset of MainImage
        private System.Windows.Point start;   // Original Position of the mouse

        private Bitmap _bitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MainImage files(*.pbm;*.ppm)|*.pbm;*.ppm";
            openFileDialog.Title = "Open an NetPbm image file";

            if (openFileDialog.ShowDialog() == true)
            {
                ReadBitmapFromPPM(openFileDialog);
                if (_bitmap != null)
                {
                    MainImage.Source = ToBitmapMainImage(_bitmap);
                }
            }
        }

        private void ReadBitmapFromPPM(OpenFileDialog openFileDialog)
        {
            using (var ppm = new NetpbmReader(openFileDialog))
            {
                if (!ppm.ReadFile())
                {
                    MessageBox.Show("Open file error");
                    _bitmap = null;
                }

                MainImage.RenderTransform = new MatrixTransform();

                if (ppm.Width / ppm.Height > ImageStackPanel.ActualWidth / ImageStackPanel.ActualHeight)
                {
                    MainImage.Width = ImageStackPanel.ActualWidth;
                }
                else
                {
                    MainImage.Height = ImageStackPanel.ActualHeight;
                }

                CommentsListBox.ItemsSource = ppm.Comments;

                _bitmap = ppm.Bitmap;
            }
        }

        public BitmapImage ToBitmapMainImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapMainImage = new BitmapImage();

                bitmapMainImage.BeginInit();
                bitmapMainImage.StreamSource = memory;
                bitmapMainImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapMainImage.EndInit();

                return bitmapMainImage;
            }
        }

        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MainImage.IsMouseCaptured) return;
            MainImage.CaptureMouse();

            start = e.GetPosition(border);
            origin.X = MainImage.RenderTransform.Value.OffsetX;
            origin.Y = MainImage.RenderTransform.Value.OffsetY;
        }



        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MainImage.IsMouseCaptured) return;
            System.Windows.Point p = e.MouseDevice.GetPosition(border);

            Matrix m = MainImage.RenderTransform.Value;
            m.OffsetX = origin.X + (p.X - start.X);
            m.OffsetY = origin.Y + (p.Y - start.Y);

            MainImage.RenderTransform = new MatrixTransform(m);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point p = e.MouseDevice.GetPosition(MainImage);

            Matrix m = MainImage.RenderTransform.Value;
            if (e.Delta > 0)
                m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
            else
                m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

            MainImage.RenderTransform = new MatrixTransform(m);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainImage.ReleaseMouseCapture();
        }

        private void SavePBMButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Save as PBM file";
            if (_bitmap != null && dialog.ShowDialog() == true)
            {
                var writer = new NetpbmWriter(_bitmap);

                if (writer.Write(dialog.FileName))
                {
                    MessageBox.Show("Successful save");
                }
                else
                {
                    MessageBox.Show("Save error");
                }
            }
        }
    }
}
