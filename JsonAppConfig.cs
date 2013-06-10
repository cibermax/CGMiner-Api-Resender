using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace CGMiner_Api_Resender
{
    internal class JsonAppConfig
    {
        public static string DefaultFilename = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) +
                                               ".config";

        public static Dictionary<string, string> DefaultSettings = new Dictionary<string, string>();

        private static void _cw(string text)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine(time + " Config reader: " + text);
        }

        public static Dictionary<string, string> Read(string filename)
        {
            if (File.Exists(filename))
            {
                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(filename);
                }
                catch (Exception e)
                {
                    _cw("Error reading config file: " + e.Message);
                    SaveDefault(filename);
                    return DefaultSettings;
                }
                Dictionary<string, string> configObj;
                try
                {
                    configObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(fileContent);
                    if (configObj.Count < DefaultSettings.Count) throw new Exception("Not enough fields.");
                    _cw("Config file loaded");
                }
                catch (Exception e)
                {
                    _cw("Parsing config file failed! Error: " + e.Message);
                    SaveDefault(filename);
                    return DefaultSettings;

                }
                return configObj;
            }

            SaveDefault(filename);
            return DefaultSettings;
        }

        public static Dictionary<string, string> Read()
        {
            return Read(DefaultFilename);
        }

        public static void Save(Dictionary<string, string> settings, string filename)
        {
            _cw("Writing config to " + filename);
            try
            {
                var textToWrite = FormatJson(new JavaScriptSerializer().Serialize(settings));

                var writer = new StreamWriter(filename);
                writer.Write(textToWrite);
                writer.Close();
                _cw("Config saved.");
            }
            catch (Exception e)
            {
                _cw("Error writing config file: " + e.Message);
            }
        }

        public static void Save(Dictionary<string, string> settings)
        {
            Save(settings, DefaultFilename);
        }

        public static void SaveDefault(string filename)
        {
            _cw("Restoring default config...");
            Save(DefaultSettings, filename);
        }

        public static void SaveDefault()
        {
            SaveDefault(DefaultFilename);
        }

        private const string IndentString = "    ";
        private static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(IndentString));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(IndentString));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(IndentString));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }   
    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }
}


