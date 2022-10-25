using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
