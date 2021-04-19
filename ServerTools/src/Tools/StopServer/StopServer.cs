using System;
using System.Diagnostics;
using System.Timers;

namespace ServerTools
{
    class StopServer
    {
        public static bool NoEntry = false, ShuttingDown = false;
        public static int Delay = 5;

        public static void PrepareShutdown()
        {
            if (Shutdown.Bloodmoon() || Event.Open)
            {
                EventSchedule.Add("Shutdown", DateTime.Now.AddMinutes(10));
                if (Event.Open && !Event.OperatorWarned)
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Event.Operator);
                    if (_cInfo != null)
                    {
                        Event.OperatorWarned = true;
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "A scheduled shutdown is set to begin but is on hold until the event ends" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                EventSchedule.Remove("Shutdown");
                Shutdown.Stop();
            }
        }

        public static void StartShutdown()
        {
            if (Lottery.OpenLotto)
            {
                Lottery.StartLotto();
            }
            Lottery.ShuttingDown = true;
            if (ExitCommand.IsEnabled)
            {
                ExitCommand.Players.Clear();
            }
            if (Delay < 1)
            {
                Delay = 1;
            }
            Timers.StopServerMinutes = Delay;
            ShuttingDown = true;
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", Delay.ToString());
            Alert(_phrase170, Shutdown.Alert_Count);
        }

        public static void TimeRemaining(int _newCount)
        {
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", _newCount.ToString());
            Alert(_phrase170, Shutdown.Alert_Count);
        }

        public static void OneMinuteRemains()
        {
            NoEntry = true;
            Phrases.Dict.TryGetValue(171, out string _phrase171);
            Alert(_phrase171, 1);
            Phrases.Dict.TryGetValue(170, out string _phrase170);
            _phrase170 = _phrase170.Replace("{Value}", 1.ToString());
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
            ChatHook.ChatMessage(null, "[FF0000]" + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
