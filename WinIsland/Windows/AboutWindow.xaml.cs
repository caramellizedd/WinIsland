using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WinIsland.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            versionLabel.Content = StaticStrings.longVersion;
        }

        private void closeButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            closeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(170, 255, 0, 0));
        }

        private void closeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            closeButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(136, 255, 0, 0));
        }

        private void closeButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            closeButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation
            {
                From = 24,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            DoubleAnimation opacityGrid = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            DoubleAnimation opacityImage = new DoubleAnimation
            {
                From = 0.4,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            blurEffect.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, da);
            mainGrid.BeginAnimation(OpacityProperty, opacityGrid);
            backgroundImage.BeginAnimation(OpacityProperty, opacityImage);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation
            {
                From = 0,
                To = 24,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            DoubleAnimation opacityGrid = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            DoubleAnimation opacityImage = new DoubleAnimation
            {
                From = 1,
                To = 0.4,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            blurEffect.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, da);
            mainGrid.BeginAnimation(OpacityProperty, opacityGrid);
            backgroundImage.BeginAnimation(OpacityProperty, opacityImage);
        }
    }
}
