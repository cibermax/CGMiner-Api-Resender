using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CGMiner_Api_Resender
{
    class HttpWorker
    {
        public string PostUrl { get; private set; }

        public HttpWorker(string url)
        {
            Uri myUri;
            if(Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri))
            {
                PostUrl = url;
            }
            else
            {
                throw new Exception("Wrong URL!");
            }
            _cw("Ready");
        }

        private void _cw(string text)
        {
            Program.CW("HTTP Worker: " + text);
        }

        private byte[] preparePostData(Dictionary<string, string> postData)
        {
            string postStr = "";
            foreach (var postPair in postData)
            {
                postStr += postPair.Key + "=" + postPair.Value + "&";
            }
            postStr = postStr.Substring(0, postStr.Length - 1);
            var postBytes = Encoding.UTF8.GetBytes(postStr);
            return postBytes;
        }

        public string SendPost(Dictionary<string, string> data)
        {
            string res = "false";

            var postData = preparePostData(data);

            try
            {
                Stream webpageStream;
                
                var _webRequest = WebRequest.Create(PostUrl);
                _webRequest.Method = "POST";
                _webRequest.ContentType = "application/x-www-form-urlencoded";
                _webRequest.ContentLength = postData.Length;
                _webRequest.Timeout = 2000;

                webpageStream = _webRequest.GetRequestStream();
                webpageStream.Write(postData, 0, postData.Length);
                webpageStream.Close();
                _cw("Post request sent");
                res = "true";
            }
            catch (Exception e)
            {
                
            }
            return res;
        }
    }
}
