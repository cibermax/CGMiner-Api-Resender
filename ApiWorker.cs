using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace CGMiner_Api_Resender
{
    class ApiWorker
    {
        public string Host { get; private set; }
        public Int32 Port { get; private set; }

        public ApiWorker(string host, Int32 port)
        {
            IPAddress ipAddress;
            if(IPAddress.TryParse(host, out ipAddress) && port > 1023 && port < 65536)
            {
                Host = host;
                Port = port;
            }
            else
            {
                throw new Exception("Wrong Host:Port parameters!");
            }
            _cw("Ready");
        }

        private void _cw(string text)
        {
            Program.CW("API Worker: " + text);
        }

        public string Request(string cmd) {return _request(cmd);}
        private string _request(string cmd)
        {
            string res = null;
            try
            {
                var client = new TcpClient(Host, Port);

                Stream stream = client.GetStream();
                var streamReader = new StreamReader(stream);

                var cmd_byte = Encoding.ASCII.GetBytes(cmd);
                stream.Write(cmd_byte, 0, cmd_byte.Length);

                res = streamReader.ReadLine();
                stream.Close();
                _cw("Api command `" + cmd + "` executed");
            }
            catch (SocketException e)
            {
                _cw("Connection to CGMiner failed!");
            }
            
            return res;
        }

    }
}
