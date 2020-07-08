using System.Diagnostics;
using System.Timers;

namespace ServerTools
{
    class StopServer
    {
        public static bool NoEntry = false, ShuttingDown = false, CountingDown = false;
        public static int Delay = 5;

        public static void StartShutdown()
        {
            if (Lottery.OpenLotto)
            {
                Lottery.StartLotto();
            }
            Lottery.ShuttingDown = true;
            CountingDown = true;
            if (Delay < 1)
            {
                Delay = 1;
            }
            Timers.StopServerMinutes = Delay;
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minute.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", Delay.ToString());
            Alert(_phrase450, Shutdown.Alert_Count);
        }

        public static void StartShutdown2(int _newCount)
        {
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minute.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", _newCount.ToString());
            Alert(_phrase450, Shutdown.Alert_Count);
        }

        public static void StartShutdown3()
        {
            NoEntry = true;
            string _phrase451;
            if (!Phrases.Dict.TryGetValue(451, out _phrase451))
            {
                _phrase451 = "Saving World Now. Do not exchange items from inventory or build.";
            }
            string _phrase450;
            if (!Phrases.Dict.TryGetValue(450, out _phrase450))
            {
                _phrase450 = "Server Shutdown In {Minutes} Minute.";
            }
            _phrase450 = _phrase450.Replace("{Minutes}", "1");
            Alert(_phrase451, 1);
            Alert(_phrase450, 1);
            SdtdConsole.Instance.ExecuteSync("saveworld", (ClientInfo)null);
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.Update();
            }
        }

        public static void Stop()
        {
            Log.Out("[SERVERTOOLS] Running shutdown.");
            SdtdConsole.Instance.ExecuteSync("st-sd", (ClientInfo)null);
        }

        public static void Kick30()
        {
            string _phrase453;
            if (!Phrases.Dict.TryGetValue(453, out _phrase453))
            {
                _phrase453 = "Shutdown is in 30 seconds. Please come back after the server restarts.";
            }
            SdtdConsole.Instance.ExecuteSync(string.Format("kickall \"{0}\"", _phrase453), (ClientInfo)null);
            PersistentContainer.Instance.Save();
        }

        public static void Alert(string _message, int _count)
        {
            ChatHook.ChatMessage(null, "[FF0000]" + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            if (_count > 1)
            {
                Alert(_message, _count - 1);
            }
        }

        public static void FailSafe(object sender, ElapsedEventArgs e)
        {
            Log.Out("[SERVERTOOLS] Failsafe activated. Server detected operating past shutdown. Forcing process kill.");
            Process process = Process.GetCurrentProcess();
            if (process != null)
            {
                process.Kill();
            }
        }
    }
}
