using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
            Program.Cw("HTTP Worker: " + text);
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
            string res = null;
            byte[] postData = null;
            Stream webpageStream;
            HttpWebRequest _webRequest = null;

            try
            {
                postData = preparePostData(data);
            }
            catch (Exception e)
            {
                _cw("Prepairing post data error: " + e.Message);
                _cw("Exception data: " + e.Data);

                return res;
            }
            
            

            try
            {
                _webRequest = (HttpWebRequest) WebRequest.Create(PostUrl);
            }
            catch (Exception e)
            {
                _cw("Socket create error: " + e.Message);
                _cw("Exception data: " + e.Data);

                return res;
            }

            _webRequest.Method = "POST";
            _webRequest.ContentType = "application/x-www-form-urlencoded";
            _webRequest.ContentLength = postData.Length;
            _webRequest.Timeout = 2000;

            try
            {
                webpageStream = _webRequest.GetRequestStream();
                webpageStream.Write(postData, 0, postData.Length);
                webpageStream.Close();
            }
            catch (Exception e)
            {
                _cw("Writing http request error: " + e.Message);
                _cw("Exception data: " + e.Data);

                return res;
            }
            
            _cw("Post request sent");

            try
            {
                var response = (HttpWebResponse)_webRequest.GetResponse();
                res = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                _cw("Getting http response error: " + e.Message);
                _cw("Exception data: " + e.Data);

                return res;
            }
            
            res = res.Trim(' ', ' ', '\n', '\r');
 
            return res;
        }
    }
}
