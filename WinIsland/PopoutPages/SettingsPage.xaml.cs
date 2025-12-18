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
            abbrSlider.Value = settings.ambientBGBlur;
            blurEverywhere.IsChecked = settings.blurEverywhere;
			abbrValueLabel.Content = (int)settings.ambientBGBlur + "px (Def: 40px)";
			blurEverywhere.Click += blurEverywhere_Click;
        }

        private void blurEverywhere_Click(object sender, RoutedEventArgs e)
        {
            settings.blurEverywhere = (bool)blurEverywhere.IsChecked;
        }

        private void abbrSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(e.NewValue != e.OldValue)
            {
                settings.ambientBGBlur = (float)e.NewValue;
                abbrValueLabel.Content = (int)settings.ambientBGBlur + "px (Def: 40px)";
                BlurEffect be = new BlurEffect();
                be.Radius = (float)e.NewValue;
                be.RenderingBias = RenderingBias.Performance;
                MainWindow.instance.bg.Effect = be;
            }
		}
	}
}
