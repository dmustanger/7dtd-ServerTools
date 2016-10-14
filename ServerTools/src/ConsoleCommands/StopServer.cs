using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    public class StopServer : ConsoleCmdAbstract
    {
        private static Thread th;
        private static int _mins;

        public override string GetDescription()
        {
            return "Stops the game server with a warning countdown every minute.";
        }

        public override string GetHelp()
        {
            return "Usage: stopserver <minutes>";
        }

        public override string[] GetCommands()
        {
            return new string[] { "stopserver", string.Empty };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                
                if (!int.TryParse(_params[0], out _mins))
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid time specified: {0}", _mins));
                    return;
                }
                Start();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in StopServer.Run: {0}.", e));
            }
        }

        private static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
            Log.Out("[SERVERTOOLS] InfoTicker has started.");
        }

        private static void StatusCheck()
        {
            SdtdConsole.Instance.Output(string.Format("Stopping server in {0} mins!", _mins));
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Restarting In {Minutes} Minutes.";
            }
            while (_mins != 1)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                ClientInfo _cInfo = _cInfoList.RandomObject();
                _phrase450 = _phrase450.Replace("{Minutes}", _mins.ToString());
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
                int _oldMins = _mins;
                _mins = _mins - 1;
                _phrase450 = _phrase450.Replace(_oldMins.ToString(), _mins.ToString());
                Thread.Sleep(60000);
            }
            string _phrase451;
            if (!Phrases.Dict.TryGetValue(451, out _phrase451))
            {
                _phrase451 = "Saving World Now.";
            }
            List<ClientInfo> _cInfoList1 = ConnectionManager.Instance.GetClients();
            ClientInfo _cInfo1 = _cInfoList1.RandomObject();
            _phrase450 = _phrase450.Replace("{Minutes}", _mins.ToString());
            GameManager.Instance.GameMessageServer(_cInfo1, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer(_cInfo1, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer(_cInfo1, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer(_cInfo1, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase451), "Server", false, "", false);
            SdtdConsole.Instance.ExecuteSync("saveworld", _cInfo1);
            Thread.Sleep(60000);
            List<ClientInfo> _cInfoList2 = ConnectionManager.Instance.GetClients();
            ClientInfo _cInfo2 = _cInfoList2.RandomObject();
            SdtdConsole.Instance.ExecuteSync("shutdown", _cInfo2);
        }
    }
}