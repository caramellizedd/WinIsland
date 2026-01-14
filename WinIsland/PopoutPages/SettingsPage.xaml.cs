using Microsoft.Win32;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinIsland.PopoutPages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        Settings settings;
        public SettingsPage()
        {
            settings = Settings.instance;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.logger.log("Loading settings page...");

            abbrSlider.Value = settings.config.ambientBGBlur;
            blurEverywhere.IsChecked = settings.config.blurEverywhere;
			abbrValueLabel.Content = (int)settings.config.ambientBGBlur + "px (Def: 40px)";
			blurEverywhere.Click += blurEverywhere_Click;
            startupCheck.Click += StartupCheck_Click;
            MainWindow.logger.log("Checking if startup registry is correct...");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key == null || key.GetValue("WinIsland") == null)
                {
                    startupCheck.IsChecked = false;
                }
                else
                {
                    
                    if(key.GetValue("WinIsland").Equals('"' + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe\""))
                        startupCheck.IsChecked = true;
                    else
                        startupCheck.IsChecked = false;
                    key.Close(); // The 'using' statement handles this, but good practice
                }
            }
        }

        private void StartupCheck_Click(object sender, RoutedEventArgs e)
        {
            var rWrite = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if ((bool)startupCheck.IsChecked)
            {
                rWrite.SetValue("WinIsland",
                                  '"' + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe\"");

                rWrite.Close();
            }
            else
            {
                rWrite.DeleteValue("WinIsland");
            }
        }

        private void blurEverywhere_Click(object sender, RoutedEventArgs e)
        {
            settings.config.blurEverywhere = (bool)blurEverywhere.IsChecked;
        }

        private void abbrSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(e.NewValue != e.OldValue)
            {
                settings.config.ambientBGBlur = (float)e.NewValue;
                abbrValueLabel.Content = (int)settings.config.ambientBGBlur + "px (Def: 40px)";
            }
		}

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.instance.saveConfig();
        }

        private void abbrSlider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        private void reloadBG_Click(object sender, RoutedEventArgs e)
        {
            if (settings.thumbnail != null)
                MainWindow.instance.renderGradient(settings.thumbnail);
        }
    }
}
