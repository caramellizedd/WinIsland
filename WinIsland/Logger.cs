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
        public FileStream stream;
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
            stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            String log = "Start LOG\nRunning WinIsland " + StaticStrings.version + "\n";
            byte[] info = new UTF8Encoding(true).GetBytes(log);
            try
            {
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
            catch (Exception ex)
            {

            }

        }
        public void log(string message) 
        {
            stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            Console.WriteLine("[" + currentDateTime + "] " + message);
            String log = "[" + currentDateTime + "] " + message + "\n";
            byte[] info = new UTF8Encoding(true).GetBytes(log);
            try
            {
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
