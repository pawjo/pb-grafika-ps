using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace GrafikaPS3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Windows.Point _origin;  // Original Offset of MainImage
        private System.Windows.Point _start;   // Original Position of the mouse
        public MainWindow()
        {
            InitializeComponent();
            var uri = new Uri("test.png", UriKind.Relative);

            int max = 255;
            var bitmap = new Bitmap(max+1, max+1);
            for (int i = 0; i <= max; i++)
            {
                for (int j = 0; j <= max; j++)
                {
                    var r = j > i ? j - i : 0;
                    var g = i > j ? i - j : 0;
                    var b = max - i;
                    bitmap.SetPixel(j, i, System.Drawing.Color.FromArgb(r, g, b));
                }
            }

            bitmap.SetPixel(0, 0, System.Drawing.Color.Orange);
            bitmap.SetPixel(0, 1, System.Drawing.Color.Orange);
            bitmap.SetPixel(0, 2, System.Drawing.Color.Orange);
            bitmap.SetPixel(0, 3, System.Drawing.Color.Orange);
            bitmap.SetPixel(1, 0, System.Drawing.Color.Orange);
            bitmap.SetPixel(1, 1, System.Drawing.Color.Orange);
            bitmap.SetPixel(1, 2, System.Drawing.Color.Orange);
            bitmap.SetPixel(1, 3, System.Drawing.Color.Orange);
            bitmap.SetPixel(2, 0, System.Drawing.Color.Orange);
            bitmap.SetPixel(2, 1, System.Drawing.Color.Orange);
            bitmap.SetPixel(2, 2, System.Drawing.Color.Orange);
            bitmap.SetPixel(2, 3, System.Drawing.Color.Orange);

            var bitmapImage = GetBitmapImage(bitmap);
            testMaterial.Brush = new ImageBrush(bitmapImage);
        }

        public BitmapImage GetBitmapImage(Bitmap bitmap)
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
            if (CubeViewport.IsMouseCaptured) return;
            CubeViewport.CaptureMouse();

            _start = e.GetPosition(ImageStackPanel);
            _origin.X = CubeViewport.RenderTransform.Value.OffsetX;
            _origin.Y = CubeViewport.RenderTransform.Value.OffsetY;
        }



        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!CubeViewport.IsMouseCaptured) return;
            System.Windows.Point p = e.MouseDevice.GetPosition(ImageStackPanel);

            var dX = p.X - _start.X;
            var dY = p.Y - _start.Y;


            CubeRotation.Angle = Math.Sqrt(dX * dX + dY * dY);
            CubeRotation.Axis = new Vector3D(dY, 0, -dX);

            //Matrix m = MainImage.RenderTransform.Value;
            //m.OffsetX = _origin.X + (p.X - _start.X);
            //m.OffsetY = _origin.Y + (p.Y - _start.Y);

            //MainImage.RenderTransform = new MatrixTransform(m);
        }

        //private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    System.Windows.Point p = e.MouseDevice.GetPosition(MainImage);

        //    Matrix m = MainImage.RenderTransform.Value;
        //    if (e.Delta > 0)
        //        m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
        //    else
        //        m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

        //    MainImage.RenderTransform = new MatrixTransform(m);
        //}

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CubeViewport.ReleaseMouseCapture();
        }
    }
}
