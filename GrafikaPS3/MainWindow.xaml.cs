﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GrafikaPS3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static StackPanel mainPanel = new StackPanel();

        public MainWindow()
        {
            InitializeComponent();
            StackPanel.Children.Add(mainPanel);
        }
        private void ConvertRGB(object sender, RoutedEventArgs e)
        {
            mainPanel.Children.Clear();
            mainPanel.Children.Add(new ConverterRGB());
        }

    }
}
