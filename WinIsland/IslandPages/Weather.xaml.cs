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
                        string lat = "-7.9797"; // Latitude
						string lon = "112.6304"; // Longitude
                        string url = "https://api.open-meteo.com/v1/forecast?latitude=" + lat + "&longitude=" + lon + "&daily=weather_code,temperature_2m_max,temperature_2m_min,uv_index_clear_sky_max,sunrise,sunset&current=temperature_2m,is_day,weather_code&timezone=auto";

						HttpResponseMessage response = await client.GetAsync(url);
                        MainWindow.logger.log("Getting weather information from " + url);
                        string resp = await response.Content.ReadAsStringAsync();
                        MainWindow.logger.log("Received weather information: " + resp);

                        MainWindow.logger.log("Attempting to make JSON...");
                        JsonNode node = JsonNode.Parse(resp);
						MainWindow.logger.log("Converted to JsonNode successfully!");
                        if (resp.Contains("\"reason\":"))
                        {
							MainWindow.logger.log("Failed !");
                            MainWindow.logger.log(node["reason"].ToString());
                            return;
						}
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
			MainWindow.logger.log("Parsing JsonNode");
			double elevationD = node["elevation"].GetValue<Double>();
            daily daily = JsonConvert.DeserializeObject<daily>(node["daily"].ToJsonString());
			current current = JsonConvert.DeserializeObject<current>(node["current"].ToJsonString());
			MainWindow.logger.log("Parsed succesfully!");

            List<WeatherDataTile> weatherTiles = new List<WeatherDataTile>();

            for(int i = 0; i <= 6; i++)
            {
                WeatherDataTile temp = new WeatherDataTile();
                temp.imageURL = "";
                temp.tempmax = daily.temperature_2m_max[i];
                temp.tempmin = daily.temperature_2m_min[i];
                temp.sunrise = daily.sunrise[i];
				temp.sunset = daily.sunset[i];
                weatherTiles.Add(temp);
			}
            int j = 0;
            foreach (WeatherDataTile tile in weatherTiles)
            {
                MainWindow.logger.log("Tile " + j + " has these data.");
                MainWindow.logger.log("ImageURL: " + tile.imageURL);
				MainWindow.logger.log("Max Temp: " + tile.tempmax);
				MainWindow.logger.log("Min Temp: " + tile.tempmin);
				MainWindow.logger.log("Sunrise: " + tile.sunrise);
				MainWindow.logger.log("Sunset: " + tile.sunset);
                j++;
			}
		}
        public class WeatherDataTile
        {
            public string imageURL { get; set; }
            public double tempmax { get; set; }
			public double tempmin { get; set; }
            public string sunrise { get; set; }
            public string sunset { get; set; }
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

		public class current
		{
			public string time { get; set; }
			public double interval { get; set; }
			public double temperature_2m { get; set; }
			public double is_day { get; set; }
			public double weather_code { get; set; }
		}
	}
}
