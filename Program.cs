using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGMiner_Api_Resender
{
    class Program
    {
        private static StreamWriter _logWriter;
        private static string _logFile;

        public static void CW(string text)
        {
            var datetime = DateTime.Now.ToString();
            var time = DateTime.Now.ToString("HH:mm");
            Console.WriteLine(time + " " + text);

            _logWriter.WriteLine(datetime + " " + text);
            _logWriter.Flush();
        }

        static void Main(string[] args)
        {
            _logFile = System.AppDomain.CurrentDomain.FriendlyName + ".log";
            if (!File.Exists(_logFile))
            {
                _logWriter = new StreamWriter(_logFile);
            }
            else
            {
                var info = new FileInfo(_logFile);
                if (info.Length > 1024 * 1024) _logWriter = new StreamWriter(_logFile);
                else _logWriter = File.AppendText(_logFile);
            }

            CW("CGMiner API Resender started.");

            var minerHost = ConfigurationManager.AppSettings["miner_host"];
            var minerPort = ConfigurationManager.AppSettings["miner_port"];
            var postUrl = ConfigurationManager.AppSettings["post_url"];
            var timeout = ConfigurationManager.AppSettings["resend_timeout"];
            var cmds_str = ConfigurationManager.AppSettings["commands"];

            cmds_str.Trim(' ', '.');
            var commands = cmds_str.Split(',');

            CW("Config file loaded.");
            CW("Host: "+minerHost);
            CW("Port: " + minerPort);
            CW("Post url: " + postUrl);
            CW("Timeout: " + timeout);
            CW("Commands: " + cmds_str);

            var resender = new Resender(minerHost, Int32.Parse(minerPort), postUrl, Int32.Parse(timeout));
            resender.AddCommands(commands);

            resender.Start();
        }
    }
}
