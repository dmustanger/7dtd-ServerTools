using System;

namespace ServerTools
{
    public static class Shutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false;
        public static int Countdown_Timer = 2, Delay = 120, Alert_Count = 2;
        public static string Command_shutdown = "shutdown";

        public static void Stop()
        {
            try
            {
                Log.Out("[SERVERTOOLS] Starting shutdown process");
                SdtdConsole.Instance.ExecuteSync(string.Format("st-StopServer {0}", Countdown_Timer), null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shutdown.Stop: {0}", e.Message));
            }
        }

        public static void NextShutdown(ClientInfo _cInfo)
        {
            try
            {
                if (_cInfo != null && _cInfo.playerId != null)
                {
                    EventSchedule.Schedule.TryGetValue("Shutdown", out DateTime _timeLeft);
                    TimeSpan varTime = _timeLeft - DateTime.Now;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _remainingTime = (int)fractionalMinutes;
                    if (_remainingTime <= 10 && Event.Open)
                    {
                        Phrases.Dict.TryGetValue(422, out string _phrase422);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase422 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    if (_remainingTime < 0)
                    {
                        _remainingTime = 0;
                    }
                    string TimeLeft;
                    TimeLeft = string.Format("{0:00} H : {1:00} M", _remainingTime / 60, _remainingTime % 60);
                    Phrases.Dict.TryGetValue(421, out string _phrase421);
                    _phrase421 = _phrase421.Replace("{TimeLeft}", TimeLeft);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase421 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shutdown.NextShutdown: {0}", e.Message));
            }
        }
    }
}