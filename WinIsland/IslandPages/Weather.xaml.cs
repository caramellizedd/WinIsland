using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WinIsland.Settings;

namespace WinIsland.IslandPages
{
    /// <summary>
    /// Interaction logic for Weather.xaml
    /// </summary>
    public partial class Weather : Page
    {
        public Weather()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            fetchWeather();
        }
        private void fetchWeather()
        {
            new Thread(async () =>
            {
                using(HttpClient client = new HttpClient())
                {
                    try
                    {
                        // The location are hardcoded for testing purposes (Malang, Indonesia)
                        // TODO: Make the user be able to select their own city/country.
                        string url = "https://api.open-meteo.com/v1/forecast?latitude=-7.9797&longitude=112.6304&daily=weather_code,temperature_2m_max,temperature_2m_min,uv_index_clear_sky_max,sunrise,sunset&current=temperature_2m,is_day,weather_code&timezone=auto";

						HttpResponseMessage response = await client.GetAsync(url);
                        MainWindow.logger.log("Getting weather information from " + url);
                        string resp = await response.Content.ReadAsStringAsync();
                        MainWindow.logger.log("Received weather information: " + resp);

                        MainWindow.logger.log("Attempting to make JSON...");
                        JsonNode node = JsonNode.Parse(resp);
                        Dispatcher.Invoke(() =>
                        {
                            parseWeather(node);
                        });
                    }
                    catch(Exception err)
                    {
                        MainWindow.logger.log("Failed !");
                        MainWindow.logger.log(err.Message);
                        MainWindow.logger.log(err.StackTrace);
                    }
                }
            }).Start();
        }
        private void parseWeather(JsonNode node)
        {
            double elevationD = node["elevation"].GetValue<Double>();
            daily daily = JsonConvert.DeserializeObject<daily>(node["daily"].ToJsonString());

            elevation.Content = elevationD;

            foreach(string str in daily.time)
            {
                if (time.Content == "")
                    time.Content = str;
                else
                    time.Content = time.Content + " | " + str;
            }

            foreach (string str in daily.weather_code)
            {
                if (weathercodes.Content == "")
                    weathercodes.Content = str;
                else
                    weathercodes.Content = weathercodes.Content + " | " + str;
            }

        }
        public class daily
        {
            public string[] time { get; set; }
            public string[] weather_code { get; set; }
            public double[] temperature_2m_max { get; set; }
			public double[] temperature_2m_min { get; set; }
            public double[] uv_index_clear_sky_max { get; set; }
			public string[] sunrise { get; set; }
			public string[] sunset { get; set; }
		}
    }
}
