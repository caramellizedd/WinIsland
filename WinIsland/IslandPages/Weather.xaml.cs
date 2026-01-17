using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (Settings.instance.lastWeatherInfo != null)
            {
                waitLabel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                waitLabel.Margin = new Thickness(0, 0, 0, 25);
                waitLabel.Content = "Refreshing weather information...";
                parseWeather(Settings.instance.lastWeatherInfo);
            }
            else
            {
                waitLabel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                waitLabel.Margin = new Thickness(0, 0, 0, 0);
                waitLabel.Content = "Loading weather information...";
            }
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
                            waitLabel.Visibility = Visibility.Hidden;
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
                if (i == 0) continue;
                WeatherDataTile temp = new WeatherDataTile();
                string dateString = daily.time[i];
                DayOfWeek day = GetDayOfWeekFromDateString(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                temp.dayOfWeek = day.ToString();
                temp.imageURL = "";
                temp.tempmax = (int)daily.temperature_2m_max[i] + "° Max";
                temp.tempmin = (int)daily.temperature_2m_min[i] + "° Min";
                string[] sunrise = daily.sunrise[i].Split("T");
                string[] sunset = daily.sunset[i].Split("T");
                temp.sunrise = sunrise[1];
				temp.sunset = sunset[1];
                weatherTiles.Add(temp);
			}
            WeatherListView.Items.Clear();
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

                WeatherListView.Items.Add(tile);
			}
            Settings.instance.lastWeatherInfo = node;
        }
        public DayOfWeek GetDayOfWeekFromDateString(string dateString, string format, CultureInfo cultureInfo)
        {
            DateTime dateValue;
            // Use TryParseExact for safer parsing to avoid exceptions if the format is incorrect
            if (DateTime.TryParseExact(dateString, format, cultureInfo, DateTimeStyles.None, out dateValue))
            {
                return dateValue.DayOfWeek;
            }
            else
            {
                // Handle the error case (e.g., throw an exception, return a default value, or show an error in UI)
                throw new FormatException($"Unable to convert {dateString} to a valid date using format {format}.");
            }
        }
        public class WeatherDataTile
        {
            public string dayOfWeek { get; set; }
            public string imageURL { get; set; }
            public string tempmax { get; set; }
			public string tempmin { get; set; }
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
