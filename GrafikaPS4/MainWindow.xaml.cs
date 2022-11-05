using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
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

        private WriteableBitmap _writeableBitmap;

        private int _currentImageFormat = 0;

        private string _windowTitle = "Netpbm Viewer PTPW";

        public bool IsLoading { get; set; }

        public bool IsImageLoaded { get => _writeableBitmap != null; }

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
            openFileDialog.Filter = "JPG|*.jpg;*.jpeg|PNG|*.png|BMP|*.bmp|GIF|*.gif|TIFF|*.tif;*.tiff|" +
                "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";


            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmapImage = new BitmapImage(new System.Uri(openFileDialog.FileName));
                    _writeableBitmap = new WriteableBitmap(bitmapImage);
                    MainImage.Source = _writeableBitmap;
                }
                catch
                {
                    MessageBox.Show("Open file error");
                }
            }
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

        private Bitmap GetBitmapFromWritableBitmap()
        {
            if (_writeableBitmap == null)
            {
                MessageBox.Show("Not loaded image");
                return null;
            }

            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(_writeableBitmap));
                enc.Save(outStream);
                var bmp = new Bitmap(outStream);
                return bmp;
            }
        }

        private Bitmap GetBitmapFromWritableBitmap(WriteableBitmap writeableBitmap)
        {
            if (writeableBitmap == null)
            {
                return null;
            }

            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeableBitmap));
                enc.Save(outStream);
                var bmp = new Bitmap(outStream);
                return bmp;
            }
        }

        private void SetNewWriteableBitmap(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            _writeableBitmap = new WriteableBitmap(bitmapSource);
            MainImage.Source = _writeableBitmap;
        }

        private WriteableBitmap GetWriteableBitmapFormBitmap(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            var writeableBitmap = new WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        private async void GrayScaleAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            bitmap = await Task.Run(() => PointTransforms.GrayScaleAsync(bitmap));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void GrayScaleYUVAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            bitmap = await Task.Run(() => PointTransforms.GrayScaleYUVAsync(bitmap));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void AddAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.AddAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void SubtractAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.SubtractAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void MultiplyAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.MultiplyAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void DivideAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.DivideAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void BrighterAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.BrighterAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void DarkerAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            Loading.IsBusy = true;

            bitmap = await Task.Run(() => PointTransforms.DarkerAsync(bitmap, value));

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }



        private int GetPointTransformValue()
        {
            if (int.TryParse(PointTransformsValue.Text, out int value) && value > 0)
            {
                return value;
            }

            MessageBox.Show("Wrong value");
            return 0;
        }
    }
}
