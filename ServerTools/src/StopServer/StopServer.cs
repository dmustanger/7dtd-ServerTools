
namespace ServerTools
{
    class StopServer
    {
        public static bool Ten_Second_Countdown = false, stopServerCountingDown = false, Kick_30_Seconds = false, NoEntry = false;

        public static void StartShutdown()
        {
            if (AutoShutdown.Kick_Login)
            {
                NoEntry = true;
            }
            if (Timers.Stop_Server_Time < 1)
            {
                Timers.Stop_Server_Time = 1;
            }
            Timers._sSC = Timers.Stop_Server_Time;
            stopServerCountingDown = true;
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", Timers.Stop_Server_Time.ToString());
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), Config.Server_Response_Name, false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown2(int _newCount)
        {
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", _newCount.ToString());
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), Config.Server_Response_Name, false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown3()
        {
            string _phrase451;
            if (!Phrases.Dict.TryGetValue(451, out _phrase451))
            {
                _phrase451 = "Saving World Now. Do not exchange items from inventory or build.";
            }
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", "1");
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase451), Config.Server_Response_Name, false, "", false);
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("[FF0000]{0}[-]", _phrase450), Config.Server_Response_Name, false, "", false);
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
        }

        public static void StartShutdown4()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]10 seconds until shutdown[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown5()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]5[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown6()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]4[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown7()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]3[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown8()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]2[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void StartShutdown9()
        {
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]1[-]"), Config.Server_Response_Name, false, "", false);
        }

        public static void Stop()
        {
            Log.Out("[SERVERTOOLS] Running shutdown.");
            SdtdConsole.Instance.ExecuteSync("shutdown", (ClientInfo)null);
        }

        public static void Kick30()
        {
            NoEntry = true;
            string _phrase453;
            if (!Phrases.Dict.TryGetValue(453, out _phrase453))
            {
                _phrase453 = "Shutdown is in 30 seconds. Please come back after the server restarts.";
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kickall \"{0}\"", _phrase453), (ClientInfo)null);
        }
    }
}
