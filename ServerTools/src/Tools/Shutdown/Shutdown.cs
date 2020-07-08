using System;

namespace ServerTools
{
    public static class Shutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false, Bloodmoon = false, BloodmoonOver = false;
        public static int Countdown_Timer = 2, Delay = 120, Alert_Count = 2;
        public static string Command47 = "shutdown";

        public static void BloodmoonCheck()
        {
            try
            {
                if (PersistentOperations.BloodMoonSky() || PersistentOperations.BloodMoonDuskSky())
                {
                    Bloodmoon = true;
                    return;
                }
                else if (Bloodmoon && PersistentOperations.BloodMoonOver())
                {
                    Bloodmoon = false;
                    BloodmoonOver = true;
                    return;
                }
                else
                {
                    Stop();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shutdown.BloodmoonCheck: {0}", e.Message));
            }
        }

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
                    if (!Event.Open)
                    {
                        if (!Bloodmoon)
                        {
                            if (!StopServer.CountingDown)
                            {
                                int _time;
                                if (BloodmoonOver)
                                {
                                    _time = Delay - (Timers._shutdownBloodmoonOver / 60);
                                }
                                else
                                {
                                    _time = Delay - (Timers._shutdown / 60);
                                }
                                if (_time < 0)
                                {
                                    _time = 0;
                                }
                                string TimeLeft;
                                TimeLeft = string.Format("{0:00} H : {1:00} M", _time / 60, _time % 60);
                                string _phrase730;
                                if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                                {
                                    _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                                }
                                _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase730 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The server has already started the shutdown process.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "A bloodmoon is currently active. The server is set to shutdown after it finishes.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "A event is currently active. The server is set to shutdown after it finishes.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shutdown.NextShutdown: {0}", e.Message));
            }
        }
    }
}