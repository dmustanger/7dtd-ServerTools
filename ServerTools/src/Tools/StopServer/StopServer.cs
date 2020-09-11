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
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", Delay.ToString());
            Alert(_phrase170, Shutdown.Alert_Count);
        }

        public static void StartShutdown2(int _newCount)
        {
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", Delay.ToString());
            Alert(_phrase170, Shutdown.Alert_Count);
        }

        public static void StartShutdown3()
        {
            NoEntry = true;
            BattleLogger.Exit.Clear();
            Phrases.Dict.TryGetValue(171, out string _phrase171);
            Alert(_phrase171, 1);
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", Delay.ToString());
            Alert(_phrase170, 1);
            SdtdConsole.Instance.ExecuteSync("saveworld", null);
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.Update();
            }
        }

        public static void Stop()
        {
            Log.Out("[SERVERTOOLS] Running shutdown.");
            SdtdConsole.Instance.ExecuteSync("shutdown", null);
        }

        public static void Kick30()
        {
            PersistentContainer.Instance.Save();
            Phrases.Dict.TryGetValue(172, out string _phrase172);
            SdtdConsole.Instance.ExecuteSync(string.Format("kickall \"{0}\"", _phrase172), null);
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
