using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    public class StopServer : ConsoleCmdAbstract
    {
        private static Thread th;
        private static int _mins;
        private static bool isCountingDown = false;
        public static bool TenSecondCountdownEnabled = false;

        public override string GetDescription()
        {
            return "[ServerTools]-Stops the game server with a warning countdown every minute.";
        }

        public override string GetHelp()
        {
            return "Usage: stopserver <minutes>\n" +
                "Usage: stopserver cancel";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-StopServer", "stopserver", string.Empty };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0] == "cancel")
                {
                    if (!isCountingDown)
                    {
                        SdtdConsole.Instance.Output("Stopserver is not running.");
                    }
                    else
                    {
                        th.Abort();
                        isCountingDown = false;
                        SdtdConsole.Instance.Output("Stopserver has stopped.");
                    }
                }
                else
                {
                    if (isCountingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("Server is already stopping in {0} mins", _mins));
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out _mins))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid time specified: {0}", _mins));
                        }
                        else
                        {
                            Start();
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServer.Run: {0}.", e));
            }
        }

        private static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
        }

        private static void StatusCheck()
        {
            isCountingDown = true;
            SdtdConsole.Instance.Output(string.Format("Stopping server in {0} mins!", _mins));
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Restarting In {Minutes} Minutes.";
            }
            while (_mins != 1)
            {
                _phrase450 = _phrase450.Replace("{Minutes}", _mins.ToString());
                string _red = "[FF0000]";
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red, _phrase450), "Server", false, "", false);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red, _phrase450), "Server", false, "", false);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red, _phrase450), "Server", false, "", false);
                int _oldMins = _mins;
                _mins = _mins - 1;
                _phrase450 = _phrase450.Replace(_oldMins.ToString(), _mins.ToString());
                Thread.Sleep(60000);
            }
            string _phrase451;
            if (!Phrases.Dict.TryGetValue(451, out _phrase451))
            {
                _phrase451 = "Saving World Now. Do not exchange items from inventory or build until after restart.";
            }

            _phrase450 = _phrase450.Replace("{Minutes}", _mins.ToString());
            string _red1 = "[FF0000]";
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red1, _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red1, _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red1, _phrase450), "Server", false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", _red1, _phrase451), "Server", false, "", false);
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
            if (TenSecondCountdownEnabled)
            {
                Thread.Sleep(50000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}10 seconds until shutdown[-]", _red1), "Server", false, "", false);
                Thread.Sleep(5000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}5[-]", _red1), "Server", false, "", false);
                Thread.Sleep(1000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}4[-]", _red1), "Server", false, "", false);
                Thread.Sleep(1000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}3[-]", _red1), "Server", false, "", false);
                Thread.Sleep(1000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}2[-]", _red1), "Server", false, "", false);
                Thread.Sleep(1000);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}1[-]", _red1), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync("shutdown", (ClientInfo)null);
            }
            else
            {
                Thread.Sleep(60000);
                SdtdConsole.Instance.ExecuteSync("shutdown", (ClientInfo)null);
            }
        }
    }
}