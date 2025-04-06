using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinIsland
{
    public class Settings
    {
        public bool blurEverywhere = false;
        // DO NOT SAVE THESE VALUES.
        public System.Windows.Media.Color borderColor;
        public Bitmap thumbnail;
        public static Settings instance;
        public Settings()
        {
            instance = this;
        }
    }
}
