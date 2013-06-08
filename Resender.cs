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

        private IAsyncResult repeatAction;
        private bool _resending = false;

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
            Program.CW(text);
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
                    if (_resending) _repeat(true);
                    return;
                }
                var json = Parser.ToJson(api_answer);
                postData.Add(command, json);
            }
            var post_res = _post.SendPost(postData);
            if(_resending) _repeat(false);
        }

        

        private void _repeat(bool more)
        {
            var timeout = more ? Timeout*3: Timeout;
            var repeatFunc = new Action(() =>
            {
                Thread.Sleep(timeout*1000);
                _resend();
            });
            _cw("Waiting " + timeout.ToString() + " sec.");
            repeatFunc.Invoke();
            
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
    }
}
