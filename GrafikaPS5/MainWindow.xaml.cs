using LiveCharts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrafikaPS4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsLoading { get; set; }

        public bool IsImageLoaded { get => _writeableBitmap != null; }

        //public Bitmap CurrentBitmap { get => _writeableBitmap != null ? GetBitmapFromWritableBitmap() : new Bitmap(10, 10); }

        public SeriesCollection SeriesCollection { get => _series; }

        public string[] Labels { get; set; }

        private System.Windows.Point _origin;  // Original Offset of MainImage
        private System.Windows.Point _start;   // Original Position of the mouse

        private WriteableBitmap _writeableBitmap;

        private int _currentImageFormat = 0;

        private string _windowTitle = "PT PW PS5";

        private SeriesCollection _series;

        private Histogram _histogram;

        public MainWindow()
        {
            InitializeComponent();

            _histogram = new Histogram();

            MainViewerWindow.DataContext = this;

            MainViewerWindow.Title = _windowTitle;
            CommentsListBox.ItemsSource = new List<string>() { "No comments" };

            Labels = new string[256];
            for (int i = 0; i < 256; i++)
            {
                Labels[i] = i.ToString();
            }

            HitOrMissInput.Text = "1, 1, -1\r\n1, 0, -1\r\n1, -1, 0";
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {

            var openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open an image file";
            openFileDialog.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|JPG|*.jpg;*.jpeg|PNG|*.png|BMP|*.bmp|GIF|*.gif|TIFF|*.tif;*.tiff";


            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmapImage = new BitmapImage(new System.Uri(openFileDialog.FileName));
                    _writeableBitmap = new WriteableBitmap(bitmapImage);
                    MainImage.Source = _writeableBitmap;

                    var bitmap = GetBitmapFromWritableBitmap();
                    _series = _histogram.GetRefreshedSeriesCollection(bitmap);
                    OnPropertyChanged(nameof(SeriesCollection));
                }
                catch
                {
                      MessageBox.Show("Open file error");
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        private void SetNewWriteableBitmap(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            _writeableBitmap = new WriteableBitmap(bitmapSource);
            MainImage.Source = _writeableBitmap;
            _series = _histogram.GetRefreshedSeriesCollection(bitmap);
            OnPropertyChanged(nameof(SeriesCollection));
        }

        private async void GrayScaleAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => PointTransforms.GrayScaleAsync(bitmap));
        }

        private async void GrayScaleYUVAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => PointTransforms.GrayScaleYUVAsync(bitmap));
        }

        private async void AddAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.AddAsync(bitmap, value));
        }

        private async void SubtractAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.SubtractAsync(bitmap, value));
        }

        private async void MultiplyAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.MultiplyAsync(bitmap, value));
        }

        private async void DivideAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.DivideAsync(bitmap, value));
        }

        private async void BrighterAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.BrighterAsync(bitmap, value));
        }

        private async void DarkerAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetPointTransformValue();
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => PointTransforms.DarkerAsync(bitmap, value));
        }

        private async void SmoothAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.Smooth(bitmap));
        }

        private async void SharpenAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.Sharpen(bitmap));
        }

        private async void MedianAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Filters.Median(bitmap));
        }

        private async void SobelHorizontalAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.SobelHorizontal(bitmap));
        }

        private async void SobelVerticalAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.SobelVertical(bitmap));
        }

        private async void HighPassSharpenAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.HighPassSharpen(bitmap));
        }

        private async void GaussAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => ConvolutionFilters.Gauss(bitmap));
        }

        private async void CustomConvolutionAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            var mask = GetMatrixFromInput(CustomConvolutionInput);
            if (bitmap == null || mask == null)
            {
                return;
            }

            var maskSize = mask.GetLength(0);
            await RunAction(() => ConvolutionFilters.ApplyFilter(bitmap, mask, maskSize));
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

        private int GetInputValue(TextBox input)
        {
            if (int.TryParse(input.Text, out int value) && value > 0)
            {
                return value;
            }

            MessageBox.Show("Wrong value");
            return 0;
        }

        private int GetInputValueInRange(TextBox input, int min, int max)
        {
            if (int.TryParse(input.Text, out int value) && value >= min && value <= max)
            {
                return value;
            }

            MessageBox.Show("Wrong value");
            return 0;
        }

        private void ShowHistogram_Click(object sender, RoutedEventArgs e)
        {
            HistogramGrid.Width = 200;
        }
        private void HideHistogram_Click(object sender, RoutedEventArgs e)
        {
            HistogramGrid.Width = 0;
        }

        private async void StretchHistogramAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => _histogram.Stretch(bitmap));
        }

        private async void AlignHistogramAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => _histogram.Align(bitmap));
        }

        private async void ManualBinarizationAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetInputValueInRange(ManualBinarizationTreshold, 0, 255);
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Binarization.ApplyBinarization(bitmap, value));
        }

        private async Task RunAction(Func<Bitmap> action)
        {
            Loading.IsBusy = true;

            var bitmap = await Task.Run(action);

            SetNewWriteableBitmap(bitmap);
            Loading.IsBusy = false;
        }

        private async void PercentBlackSelectionAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            int value = GetInputValueInRange(ManualBinarizationTreshold, 0, 255);
            if (bitmap == null || value == 0)
            {
                return;
            }

            await RunAction(() => Binarization.PercentBlackSelection(bitmap, value, _histogram));
        }

        private async void EntropySelectionAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Binarization.EntropySelection(bitmap, _histogram));
        }

        private async void MeanIterativeSelectionAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Binarization.MeanIterativeSelection(bitmap, _histogram));
        }

        private async void OtsuAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Binarization.Otsu(bitmap, _histogram));
        }
        private async void NiblackAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Binarization.Niblack(bitmap));
        }

        private async void DilatationAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Filters.Dilatation(bitmap));
        }

        private async void ErosionAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Filters.Erosion(bitmap));
        }

        private async void OpeningAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Filters.Opening(bitmap));
        }

        private async void ClosingAsync(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            if (bitmap == null)
            {
                return;
            }

            await RunAction(() => Filters.Closing(bitmap));
        }

        private async void ThinningAsync(object sender, RoutedEventArgs e)
        {
            await HitOrMissAsync(true);
        }

        private async void ThickingAsync(object sender, RoutedEventArgs e)
        {
            await HitOrMissAsync(false);
        }

        private async Task HitOrMissAsync(bool isThinning)
        {
            var bitmap = GetBitmapFromWritableBitmap();
            var matrixSize = 3;
            var matrix = GetMatrixFromInput(HitOrMissInput, matrixSize);
            if (bitmap == null || matrix == null)
            {
                return;
            }

            var se = new int[matrixSize * matrixSize];
            for (int i = 0, k = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    se[k++] = (int)matrix[i, j];
                }
            }

            await RunAction(() => Filters.ApplyHitOrMiss(bitmap, isThinning, se));
        }

        private double[,]? GetMatrixFromInput(TextBox textBox, int? expectedMatrixSize = null)
        {
            var lines = textBox.Text.Split("\r\n");
            var matrixSize = lines.Length;
            if (expectedMatrixSize.HasValue && expectedMatrixSize.Value != matrixSize)
            {
                return null;
            }

            var matrix = new double[matrixSize, matrixSize];

            for (int i = 0; i < matrixSize; i++)
            {
                var splittedLine = lines[i].Split(',', ' ');
                var filteredSplittedLine = splittedLine.Where(x => x != "").ToList();
                if (filteredSplittedLine.Count != matrixSize)
                {
                    MessageBox.Show("Matrix format error - wrong numbers of values in line");
                    return null;
                }

                for (int j = 0; j < matrixSize; j++)
                {
                    if (!double.TryParse(filteredSplittedLine[j], NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    {
                        MessageBox.Show("Matrix format error - value cannot be parsed");
                        return null;
                    }

                    matrix[i, j] = result;
                }
            }

            return matrix;
        }
    }
}
