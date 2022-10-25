using Microsoft.Win32;
using System.Collections.Generic;
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
        private System.Windows.Point _origin;  // Original Offset of MainImage
        private System.Windows.Point _start;   // Original Position of the mouse

        private Bitmap _bitmap;

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

            openFileDialog.Filter = "Netpbm files(*.pbm;*.pgm;*.ppm)|*.pbm;*.pgm;*.ppm";
            openFileDialog.Title = "Open an NetPbm image file";
            Loading.IsBusy = true;

            if (openFileDialog.ShowDialog() == true)
            {
                ReadFile(openFileDialog);
                if (_bitmap != null)
                {
                    MainImage.Source = GetBitmapImage(_bitmap);
                }
            }
            Loading.IsBusy = false;
        }

        private void ReadFile(OpenFileDialog openFileDialog)
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

                if (ppm.Comments.Count == 0)
                {
                    CommentsListBox.ItemsSource = new List<string>() { "No comments" };

                }
                else
                {
                    CommentsListBox.ItemsSource = ppm.Comments;
                }

                int format = (ppm.Format[1] - 48) % 3;
                _currentImageFormat = format == 0 ? 3 : format;

                MainViewerWindow.Title = $"{_windowTitle} - {openFileDialog.FileName}";

                _bitmap = ppm.Bitmap;
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

        private void Show_Comments(object sender, RoutedEventArgs e)
        {
            if (CommentsListBox.Visibility == Visibility.Hidden)
            {
                CommentsListBox.Visibility = Visibility.Visible;

            }
            else
            {
                CommentsListBox.Visibility = Visibility.Hidden;
            }

        }

        private void HandleSaveFile(string format, string asciiFormat, string binaryFormat)
        {
            if (_bitmap == null)
            {
                MessageBox.Show("No open file");
                return;
            }

            var formatResult = MessageBox.Show("Would you like to save this image in binary?", "Save", MessageBoxButton.YesNoCancel);

            if (formatResult == MessageBoxResult.Cancel)
            {
                return;
            }

            var isAscii = formatResult == MessageBoxResult.No;

            Loading.IsBusy = true;

            var dialog = new SaveFileDialog();
            var upperFormat = format.ToUpper();
            dialog.Title = $"Save as {upperFormat} file";
            dialog.Filter = $"{upperFormat} file (*.{format})|*.{format}";

            if (dialog.ShowDialog() == false)
            {
                Loading.IsBusy = false;
                return;
            }

            using (var writer = new NetpbmWriter(_bitmap, format, dialog.FileName, _currentImageFormat, isAscii ? asciiFormat : binaryFormat))
            {
                if (writer.Write())
                {
                    MessageBox.Show("Successful save");
                }
                else
                {
                    MessageBox.Show("Save error");
                }
            }

            Loading.IsBusy = false;
        }

        private void SavePBMButton_Click(object sender, RoutedEventArgs e)
        {
            HandleSaveFile("pbm", "P1", "P4");
        }

        private void SavePGMButton_Click(object sender, RoutedEventArgs e)
        {
            HandleSaveFile("pgm", "P2", "P5");
        }

        private void SavePPMButton_Click(object sender, RoutedEventArgs e)
        {
            HandleSaveFile("ppm", "P3", "P6");
        }
    }
}
