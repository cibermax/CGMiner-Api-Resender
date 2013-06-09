using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace CGMiner_Api_Resender
{
    class Parser
    {
        public static object ToObject(string dataStr)
        {
            var data = new Dictionary<string, Dictionary<string, string>>();
            var objs0 = dataStr.Split('|');

            foreach (var obj in objs0)
            {
                if (obj.Length <= 1 || obj == "\u0000") continue;
                var objs1 = obj.Split(',');
                string name = null;

                foreach (var elem in from obj1 in objs1 where obj1.Length > 1 && obj != "\u0000" select obj1.Split('='))
                {
                    if (name != null) data[name].Add(elem[0], elem[1]);
                    else
                    {
                        if (elem.Length < 2 || elem[0] == "STATUS") name = elem[0];
                        else name = elem[0] + elem[1];

                        data.Add(name, new Dictionary<string, string>());
                    }
                }
            }

            return data;
        }

        public static string ToJson(string dataStr)
        {
            var dict = (Dictionary<string, Dictionary<string, string>>) ToObject(dataStr);
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(dict);
            return json;
        }
    }
}
