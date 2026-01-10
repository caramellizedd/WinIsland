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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Power;

namespace WinIsland.PopoutPages
{
    /// <summary>
    /// Interaction logic for AdvancedSettingsPage.xaml
    /// </summary>
    public partial class AdvancedSettingsPage : Page
    {
        public AdvancedSettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            corRadSlider.Value = Settings.instance.config.cornerRadius;
            corRadLabel.Content = Settings.instance.config.cornerRadius + "px (Def: 10px)";
            hideBatteryToggle.IsChecked = Settings.instance.config.batteryHidden;
            hideClockToggle.IsChecked = Settings.instance.config.clockHidden;
        }

        private void corRadSlider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void corRadSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(e.OldValue != e.NewValue)
            {
                Settings.instance.config.cornerRadius = (int)e.NewValue;
                MainWindow.instance.mainWindowB.CornerRadius = new CornerRadius((int)e.NewValue);
                MainWindow.instance.windowBorder.CornerRadius = new CornerRadius((int)e.NewValue);
                corRadLabel.Content = (int)e.NewValue + "px (Def: 10px)";
            }
        }

        private void hideBatteryToggle_Click(object sender, RoutedEventArgs e)
        {
            Settings.instance.config.batteryHidden = (bool)hideBatteryToggle.IsChecked;
            MainWindow.instance.battery.Visibility = (bool)hideBatteryToggle.IsChecked ? Visibility.Hidden : Visibility.Visible;

            if (MainWindow.instance.battery.Visibility == Visibility.Hidden)
            {
                MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 15, 0);
                if (MainWindow.instance.clock.Visibility == Visibility.Hidden)
                {
                    MainWindow.instance.islandMini.Margin = new Thickness(0);
                    MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                }
                else
                {
                    MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 15, 0);
                    MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                }
            }
            else
            {
                MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 60, 0);
                MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
        }

        private void hideClockToggle_Click(object sender, RoutedEventArgs e)
        {
            Settings.instance.config.clockHidden = (bool)hideClockToggle.IsChecked;
            MainWindow.instance.clock.Visibility = (bool)hideClockToggle.IsChecked ? Visibility.Hidden : Visibility.Visible;

            if (MainWindow.instance.battery.Visibility == Visibility.Hidden)
            {
                MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 15, 0);
                if (MainWindow.instance.clock.Visibility == Visibility.Hidden)
                {
                    MainWindow.instance.islandMini.Margin = new Thickness(0);
                    MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                }
                else
                {
                    MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 15, 0);
                    MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                }
            }
            else
            {
                MainWindow.instance.islandMini.Margin = new Thickness(0, 0, 60, 0);
                MainWindow.instance.islandMini.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.instance.saveConfig();
        }
    }
}
