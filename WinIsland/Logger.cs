using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WinIsland
{
    public class Logger
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WI_Latest.log";
        DateTime currentDateTime = DateTime.Now;
        public Logger()
        {
            StartFileWriter();
        }
        private void StartFileWriter()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, "Start LOG\nRunning WinIsland " + StaticStrings.version + "\n");
        }
        public void log(string message) 
        {
            Console.WriteLine("[" + currentDateTime + "] " + message);
            File.AppendAllText(path, "[" + currentDateTime + "] " + message + "\n");
        }
    }
}
