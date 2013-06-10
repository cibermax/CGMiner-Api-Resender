using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CGMiner_Api_Resender
{
    class Program
    {
        private static StreamWriter _logWriter;
        private static string _logFile;

        public static void Cw(string text)
        {
            var datetime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine(time + " " + text);

            _logWriter.WriteLine(datetime + " " + text);
            _logWriter.Flush();
        }


        private static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            

            JsonAppConfig.DefaultSettings.Add("max_log_size", "1");
            JsonAppConfig.DefaultSettings.Add("miner_host", "127.0.0.1");
            JsonAppConfig.DefaultSettings.Add("miner_port", "4028");
            JsonAppConfig.DefaultSettings.Add("commands", "summary,devs");
            JsonAppConfig.DefaultSettings.Add("resend_timeout", "5");
            JsonAppConfig.DefaultSettings.Add("post_url", "http://yoursite.com/store_miner_data.php");

            var settings = JsonAppConfig.Read();

            var maxLogSize = Double.Parse(settings["max_log_size"]);
            var minerHost = settings["miner_host"];
            var minerPort = settings["miner_port"];
            var postUrl = settings["post_url"];
            var timeout = settings["resend_timeout"];
            var cmdsStr = settings["commands"];

            _logFile = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + ".log";
            if (!File.Exists(_logFile))
            {
                _logWriter = new StreamWriter(_logFile);
            }
            else
            {
                var info = new FileInfo(_logFile);
                if (info.Length > maxLogSize * 1024 * 1024)
                {
                    info.MoveTo(Path.GetFileNameWithoutExtension(info.Name) + "_" +
                                DateTime.Now.ToString("dd_MMM_HH_mm_ss") + ".log");
                    _logWriter = new StreamWriter(_logFile);
                }
                else _logWriter = File.AppendText(_logFile);
            }


            Cw("CGMiner API Resender started.");

            cmdsStr = cmdsStr.Trim(' ', '.');
            var commands = cmdsStr.Split(',');

            Cw("Host: " + minerHost);
            Cw("Port: " + minerPort);
            Cw("Post url: " + postUrl);
            Cw("Timeout: " + timeout);
            Cw("Commands: " + cmdsStr);

            var resender = new Resender(minerHost, Int32.Parse(minerPort), postUrl, Int32.Parse(timeout));
            resender.AddCommands(commands);

            resender.Start();
        }
    }
}
