using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.UI.Core.AnimationMetrics;
using WinIsland.PopoutPages;

namespace WinIsland
{
    /// <summary>
    /// Interaction logic for PopoutWindow.xaml
    /// </summary>
    public partial class PopoutWindow : Window
    {
        public PopoutWindow()
        {
            InitializeComponent();
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void maxRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                mainWindowGrid.Margin = new Thickness(0);
                maxRestoreButton.Content = "\xE922";
                WindowState = WindowState.Normal;
            }
            else
            {
                mainWindowGrid.Margin = new Thickness(7);
                maxRestoreButton.Content = "\xE923";
                WindowState = WindowState.Maximized;
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bg.Source = Helper.ConvertToImageSource(Settings.instance.thumbnail);
            Helper.setBorderColor(this, Settings.instance.borderColor, Helper.ConvertToABGR(Settings.instance.borderColor.R, Settings.instance.borderColor.G, Settings.instance.borderColor.B));
        }

        private void contentFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            DoubleAnimation ta = new DoubleAnimation
            {
                From = Width,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 500),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            frameAnimation.BeginAnimation(TranslateTransform.XProperty, ta);
        }

        private void contentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            changeTitle(((Page)contentFrame.Content).Title);
        }
        private void changeTitle(string title)
        {
            windowTitle.Content = title;
            DoubleAnimation sa = new DoubleAnimation
            {
                From = -100,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation sa1 = new DoubleAnimation
            {
                From = -100,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            windowTitleTransform.BeginAnimation(TranslateTransform.YProperty, sa);
            windowTitleTransform.BeginAnimation(TranslateTransform.XProperty, sa1);
        }
        public void Navigate(object content)
        {
            DoubleAnimation sa = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = new TimeSpan(0, 0, 0, 0, 300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            DoubleAnimation sa1 = new DoubleAnimation
            {
                From = 0,
                To = -100,
                Duration = new TimeSpan(0, 0, 0, 0, 300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            

            DoubleAnimation ta = new DoubleAnimation
            {
                From = 0,
                To = -Width,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseIn }
            };
            ta.Completed += (sender, e) =>
            {
                contentFrame.Navigate(content);
            };
            windowTitleTransform.BeginAnimation(TranslateTransform.YProperty, sa);
            windowTitleTransform.BeginAnimation(TranslateTransform.XProperty, sa1);
            frameAnimation.BeginAnimation(TranslateTransform.XProperty, ta);

        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                mainWindowGrid.Margin = new Thickness(7);
                maxRestoreButton.Content = "\xE923";
            }
            else if (WindowState == WindowState.Normal)
            {
                mainWindowGrid.Margin = new Thickness(0);
                maxRestoreButton.Content = "\xE922";
            }
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(main.IsSelected) Navigate(new SettingsPage());
            if(advanced.IsSelected) Navigate(new AdvancedSettingsPage());
        }
    }
}
