using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text;
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

        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Console.OutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            _logFile = System.AppDomain.CurrentDomain.FriendlyName + ".log";
            if (!File.Exists(_logFile))
            {
                _logWriter = new StreamWriter(_logFile);
            }
            else
            {
                var info = new FileInfo(_logFile);
                if (info.Length > 1024)
                {
                    info.MoveTo(Path.GetFileNameWithoutExtension(info.Name) + "_" +
                                DateTime.Now.ToString("dd_MMM_HH_mm_ss") + ".log");
                    _logWriter = new StreamWriter(_logFile);
                }
                else _logWriter = File.AppendText(_logFile);
            }

            Cw("CGMiner API Resender started.");

            var minerHost = ConfigurationManager.AppSettings["miner_host"];
            var minerPort = ConfigurationManager.AppSettings["miner_port"];
            var postUrl = ConfigurationManager.AppSettings["post_url"];
            var timeout = ConfigurationManager.AppSettings["resend_timeout"];
            var cmdsStr = ConfigurationManager.AppSettings["commands"];


            cmdsStr = cmdsStr.Trim(' ', '.');
            var commands = cmdsStr.Split(',');

            Cw("Config file loaded.");
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
