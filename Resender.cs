using System;
using System.Collections.Generic;
using System.Threading;

namespace CGMiner_Api_Resender
{
    class Resender
    {
        public List<string> Commands { get; private set; }
        public Int32 Timeout { get; private set; }
        public string MinerHost{ get { return _api.Host; } }
        public Int32 MinerPort { get { return _api.Port; } }
        public string PostUrl { get { return _post.PostUrl; } }

        private ApiWorker _api;
        private HttpWorker _post;

        private bool _resending;

        public Resender(string minerHost, Int32 minerPort, string phpHandler, Int32 timeout = 5)
        {
            _api = new ApiWorker(minerHost, minerPort);
            _post = new HttpWorker(phpHandler);
            Commands = new List<string>();

            SetTimeout(timeout);

            _cw("Resender initialized.");
        }

        private void _cw(string text)
        {
            Program.Cw(text);
        }

        public int AddCommands(string[] newCommands)
        {
            int counter = 0;
            string cmdStr = "";
            foreach (var newCommand in newCommands)
            {
                if (!Commands.Contains(newCommand))
                {
                    Commands.Add(newCommand);
                    counter++;
                    cmdStr += newCommand + ", ";
                }
            }
            cmdStr = cmdStr.Substring(0, cmdStr.Length - 2);
            _cw(counter.ToString() + " api commands to resend added: " + cmdStr);
            return counter;
        }

        public bool SetTimeout(Int32 timeout)
        {
            if (timeout > 0)
            {
                Timeout = timeout;
                _cw("Timeout set to " + timeout + " sec.");
                return true;
            }
            else
            {
                Timeout = 5;
                _cw("Wrong timeout value. Setting it to 5 sec.");
                return false;
            }
        }

        private void _resend()
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            foreach (var command in Commands)
            {
                var api_answer = _api.Request(command);
                if (api_answer == null)
                {
                    _repeat(true);
                    return;
                }
                try
                {
                    var json = Parser.ToJson(api_answer);
                    postData.Add(command, json);
                }
                catch (Exception e)
                {
                    _cw("Parsing api data error: " + e.Message);
                    _cw("Exception data: " + e.Data);
                }
                
            }
            if (postData.Count > 1)
            {
                var post_res = _post.SendPost(postData);
                if (!_checkCmd(post_res))
                {
                    _repeat(true);
                    return;
                }
            }
            else
            {
                _cw("Nothing to send...");
                _repeat(true);
                return;
            }
            _repeat(false);
        }

        

        private void _repeat(bool more)
        {
            if (_resending)
            {
                var timeout = more ? Timeout*3 : Timeout;
                var repeatFunc = new Action(() =>
                    {
                        Thread.Sleep(timeout*1000);
                        _resend();
                    });
                _cw("Waiting " + (more ? " more " : "") + timeout.ToString() + " sec.");
                repeatFunc.Invoke();
            }
        }

        public void Start()
        {
            if (!_resending)
            {
                if(Commands.Count < 1) Commands.Add("summary");
    
                _resending = true;
                _cw("Resender STARTED!");
                var resendFunc = new Action(_resend);
                resendFunc.Invoke();
            }
        }

        public void Stop()
        {
            if (_resending)
            {
                _resending = false;
                _cw("Resender is stopping");
            }
        }

        private bool _checkCmd(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                _cw("Incorect http response!");
                return false;
            }
            var parts = str.Split(':');
            if (parts.Length <= 1) return false;
            var type = parts[0];
            var cmd = parts[1];
            switch (type)
            {
                case "result":
                    if (string.Equals(cmd, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        _cw("Server result success");
                        return true;
                    }
                    _cw("Remote server returned error!");
                    return false;
                case "cmd":
                    switch (cmd)
                    {
                        case "reboot":
                            _cw("Server asked for reboot!");
                            Restarer.DelayedRestart(5);
                            return true;
                    }
                    return false;
                default:
                    _cw("Incorect http response!");
                    return false;
            }
        }
    }
}
