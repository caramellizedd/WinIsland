using iNKORE.UI.WPF.Modern.Controls;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinIsland.IslandPages;
using static System.Net.WebRequestMethods;
using Page = System.Windows.Controls.Page;

namespace WinIsland.PopoutPages
{
    /// <summary>
    /// Interaction logic for WeatherPage.xaml
    /// </summary>
    public partial class WeatherPage : Page
    {
        public WeatherPage()
        {
            InitializeComponent();
        }

        private async void getLocButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.logger.log("Getting location...");
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36");
                    string url = "https://nominatim.openstreetmap.org/search?q="+ locationTextBox.Text+"&format=jsonv2";
                    HttpResponseMessage response = await client.GetAsync(url);
                    MainWindow.logger.log("Getting location information from " + url);
                    string resp = await response.Content.ReadAsStringAsync();
                    MainWindow.logger.log("Received location information: " + resp);

                    //List<locData> locations = JsonConvert.DeserializeObject<List<locData>>(resp);
                    //MainWindow.logger.log("Received JSON");
                    //JsonNode node = JsonNode.Parse(JsonConvert.SerializeObject(locations));
                    //MainWindow.logger.log("Converted to JsonNode: \n" + JsonConvert.SerializeObject(locations));
                    bool gotData = false;

                    JArray nodes = JArray.Parse(resp);

                    foreach (JToken node in nodes)
                    {
                        if (!gotData)
                        {
                            string name = node["name"]?.ToString();
                            string displayName = node["display_name"]?.ToString();
                            double lat = node["lat"]?.Value<double>() ?? 0;
                            double lon = node["lon"]?.Value<double>() ?? 0;


                            //Console.WriteLine($"{name}, {state} ({countryCode}) - {lat}, {lon}");
                            MainWindow.logger.log($"{name}, {displayName} - {lat}, {lon}");
                            ContentDialog dialog = new ContentDialog
                            {
                                Title = "Confirm Location",
                                Content = "Is the provided location correct?\n" + displayName,
                                PrimaryButtonText = "Yea",
                                SecondaryButtonText = "Nah",
                                IsPrimaryButtonEnabled = true,
                                IsSecondaryButtonEnabled = true
                            };
                            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                            {
                                Settings.instance.config.city = name.ToString();
                                Settings.instance.config.weatherCityName = displayName.ToString();
                                Settings.instance.config.lat = lat.ToString();
                                Settings.instance.config.lon = lon.ToString();
                                latLabel.Content = Settings.instance.config.lat;
                                lonLabel.Content = Settings.instance.config.lon;

                                gotData = true;

                                if(MainWindow.instance.islandContent.Content is Weather)
                                    MainWindow.instance.islandContent.Navigate(new Weather());
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    MainWindow.logger.log("Error getting location information: " + ex.StackTrace);
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "An error occurred while getting location information. Please try again.\nError message: " + ex.Message,
                        PrimaryButtonText = "OK",
                        IsPrimaryButtonEnabled = true
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
        public class locData
        {
            public string name { get; set; }
            public string state { get; set; }
            public string country_code { get; set; }
            public class coordinates
            {
                public string latitude { get; set; }
                public string longitude { get; set; }
            }
        }
    }
}
