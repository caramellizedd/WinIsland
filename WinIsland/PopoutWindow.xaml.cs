using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinIsland.PopoutPages;
using static WinIsland.PInvoke;
using MessageBox = System.Windows.MessageBox;

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

        /**private void maxRestoreButton_Click(object sender, RoutedEventArgs e)
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
        }**/

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, PInvoke.GWL_STYLE, GetWindowLong(hwnd, PInvoke.GWL_STYLE) & ~PInvoke.WS_SYSMENU);
            Helper.EnableBlur(this);
            //bg.Source = Helper.ConvertToImageSource(Settings.instance.thumbnail);
            //mainWindowGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x11, Settings.instance.borderColor.R, Settings.instance.borderColor.G, Settings.instance.borderColor.B));
            Helper.setBorderColor(this, Settings.instance.borderColor, Helper.ConvertToABGR(Settings.instance.borderColor.R, Settings.instance.borderColor.G, Settings.instance.borderColor.B), windowBorder);
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
            this.Title = title;
            DoubleAnimation sa1 = new DoubleAnimation
            {
                From = Width,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            windowTitleTransform.BeginAnimation(TranslateTransform.XProperty, sa1);
        }
        public void Navigate(object content)
        {
            DoubleAnimation sa1 = new DoubleAnimation
            {
                From = 0,
                To = -300,
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
            windowTitleTransform.BeginAnimation(TranslateTransform.XProperty, sa1);
            frameAnimation.BeginAnimation(TranslateTransform.XProperty, ta);

        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /**if (WindowState == WindowState.Maximized)
            {
                mainWindowGrid.Margin = new Thickness(7);
                maxRestoreButton.Content = "\xE923";
            }
            else if (WindowState == WindowState.Normal)
            {
                mainWindowGrid.Margin = new Thickness(0);
                maxRestoreButton.Content = "\xE922";
            }**/
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            
        }
    }
}
