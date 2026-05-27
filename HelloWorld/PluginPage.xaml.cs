using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HelloWorld
{
    /// <summary>
    /// 
    /// Welcome to the main Hello World plugin page!
    /// This is a simple plugin that demonstrates how to create a page and basic code for WinIsland.
    /// It serves as a starting point for your own plugin code.
    /// You can customize these however you want.
    /// Any external .NET libraries are supported, like EdgeWebView2 if you want to use that for some reason.
    /// 
    /// TODO: Create a code-only version of the plugin.
    /// 
    /// </summary>
    public partial class PluginPage : Page
    {
        public PluginPage()
        {
            InitializeComponent();
        }
    }
}
