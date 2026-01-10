using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinIsland
{
    public class Settings
    {
        // DO NOT SAVE THESE VALUES.
        public System.Windows.Media.Color borderColor;
        public Bitmap thumbnail;
        public static Settings instance;

        public SettingsConfig config;

        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WI_config.cfg";

        public Settings()
        {
            instance = this;
            loadConfig();
        }
        private void loadConfig()
        {
            if (!File.Exists(path))
            {
                loadDefaultValues();
            }
            else
            {
                string temp = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<SettingsConfig>(temp);
            }
        }
        public void saveConfig()
        {
            string temp = JsonConvert.SerializeObject(config);
            File.WriteAllText(path, temp);
            MainWindow.logger.log("Saved config.");
        }
        private void loadDefaultValues()
        {
            config = new SettingsConfig
            {
                blurEverywhere = false,
                ambientBGBlur = 40,
                cornerRadius = 10,
                clockHidden = true,
                batteryHidden = true
            };
            saveConfig();
        }
        public class SettingsConfig()
        {
            public bool blurEverywhere { get; set; }
            public bool clockHidden { get; set; }
            public bool batteryHidden { get; set; }

            public float ambientBGBlur { get; set; }

            public int cornerRadius { get; set; }
        }
    }
}
