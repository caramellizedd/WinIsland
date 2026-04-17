using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace WinIsland
{
    public class Logger
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WI_Latest.log";
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
            using(var stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
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
        }
        public void log(string message) 
        {
            DateTime currentDateTime = DateTime.Now;
            Console.WriteLine("[" + currentDateTime + "] " + message);
            using (var stream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
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
        public List<Stopwatch> counters = new List<Stopwatch>();
        public Stopwatch startCounter()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            counters.Add(stopwatch);
            return stopwatch;
        }
        public void stopCounter(Stopwatch stopwatch, string name)
        {
            foreach(Stopwatch stp in counters)
            {
                if(stp == stopwatch)
                {
                    stp.Stop();
                    log(name + " took " + stp.Elapsed);
                }
            }
        }
    }
}
