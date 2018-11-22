
namespace ServerTools
{
    class StopServer
    {
        public static bool Ten_Second_Countdown = false, stopServerCountingDown = false, Kick_30_Seconds = false, NoEntry = false;
        public static int Alert_Count = 2;

        public static void StartShutdown()
        {
            if (Lottery.OpenLotto)
            {
                Lottery.StartLotto();
            }
            Lottery.ShuttingDown = true;
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
            string _message = _phrase450;
            Alert(_message, Alert_Count);
        }

        public static void StartShutdown2(int _newCount)
        {
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minutes.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", _newCount.ToString());
            string _message = _phrase450;
            Alert(_message, Alert_Count);
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
            string _message = _phrase451;
            string _message2 = _phrase450;
            Alert(_message, 1);
            Alert(_message, 1);
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
        }

        public static void StartShutdown4()
        {
            string _message = "10 seconds until shutdown";
            Alert(_message, 1);
        }

        public static void StartShutdown5()
        {
            string _message = "5";
            Alert(_message, 1);
        }

        public static void StartShutdown6()
        {
            string _message = "4";
            Alert(_message, 1);
        }

        public static void StartShutdown7()
        {
            string _message = "3";
            Alert(_message, 1);
        }

        public static void StartShutdown8()
        {
            string _message = "2";
            Alert(_message, 1);
        }

        public static void StartShutdown9()
        {
            string _message = "1";
            Alert(_message, 1);
        }

        public static void Stop()
        {
            Timers.TimerStop();
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

        public static void Alert(string _message, int _count)
        {
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
            if (_count > 1)
            {
                int _newCount = _count - 1;
                Alert(_message, _newCount);
            }
        }
    }
}
