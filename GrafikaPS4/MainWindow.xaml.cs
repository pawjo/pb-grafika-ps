using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrafikaPS4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Point _origin;  // Original Offset of MainImage
        private System.Windows.Point _start;   // Original Position of the mouse

        private WriteableBitmap _bitmap;

        private int _currentImageFormat = 0;

        private string _windowTitle = "Netpbm Viewer PTPW";

        public bool IsLoading { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            MainViewerWindow.DataContext = this;

            MainViewerWindow.Title = _windowTitle;
            CommentsListBox.ItemsSource = new List<string>() { "No comments" };
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {

            var openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open an image file";
            openFileDialog.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";


            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmapImage = new BitmapImage(new System.Uri(openFileDialog.FileName));
                    _bitmap = new WriteableBitmap(bitmapImage);
                    MainImage.Source = _bitmap;
                }
                catch
                {
                    MessageBox.Show("Open file error");
                }
            }
        }

        private void ReadFile(OpenFileDialog openFileDialog)
        {
            //using (var ppm = new NetpbmReader(openFileDialog))
            //{

            //    if (!ppm.ReadFile())
            //    {
            //        MessageBox.Show("Open file error");
            //        _bitmap = null;
            //    }

            //    MainImage.RenderTransform = new MatrixTransform();

            //    if (ppm.Width / ppm.Height > ImageStackPanel.ActualWidth / ImageStackPanel.ActualHeight)
            //    {
            //        MainImage.Width = ImageStackPanel.ActualWidth;
            //    }
            //    else
            //    {
            //        MainImage.Height = ImageStackPanel.ActualHeight;
            //    }

            //    if (ppm.Comments.Count == 0)
            //    {
            //        CommentsListBox.ItemsSource = new List<string>() { "No comments" };

            //    }
            //    else
            //    {
            //        CommentsListBox.ItemsSource = ppm.Comments;
            //    }

            //    int format = (ppm.Format[1] - 48) % 3;
            //    _currentImageFormat = format == 0 ? 3 : format;

            //    MainViewerWindow.Title = $"{_windowTitle} - {openFileDialog.FileName}";

            //    _bitmap = ppm.Bitmap;
            //}
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
            if (MainImage.IsMouseCaptured) return;
            MainImage.CaptureMouse();

            _start = e.GetPosition(ImageStackPanel);
            _origin.X = MainImage.RenderTransform.Value.OffsetX;
            _origin.Y = MainImage.RenderTransform.Value.OffsetY;
        }



        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MainImage.IsMouseCaptured) return;
            System.Windows.Point p = e.MouseDevice.GetPosition(ImageStackPanel);

            Matrix m = MainImage.RenderTransform.Value;
            m.OffsetX = _origin.X + (p.X - _start.X);
            m.OffsetY = _origin.Y + (p.Y - _start.Y);

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

        private void MainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainImage.ReleaseMouseCapture();
        }

        private Bitmap GetWritableBitmap()
        {
            if (_bitmap == null)
            {
                return null;
            }

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(_bitmap));
                enc.Save(outStream);
                var bmp = new Bitmap(outStream);
                return bmp;
            }
        }

        private void GrayScale(object sender, RoutedEventArgs e)
        {
            //if (EditableImage == null)
            //{
            //    MessageBox.Show("Podano złą wartość");
            //    return;
            //}

            var bitmap = GetWritableBitmap();

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    var r = (color.R + color.G + color.B) / 3;
                    var g = (color.R + color.G + color.B) / 3;
                    var b = (color.R + color.G + color.B) / 3;

                    bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(color.A, r, g, b));
                }
            }

            SetNewWriteableBitmap(bitmap);
        }

        private void SetNewWriteableBitmap(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            _bitmap = new WriteableBitmap(bitmapSource);
            MainImage.Source = _bitmap;
        }
    }
}
