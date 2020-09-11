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
                                Phrases.Dict.TryGetValue(421, out string _phrase421);
                                _phrase421 = _phrase421.Replace("{TimeLeft}", TimeLeft);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase421 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(422, out string _phrase422);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase422 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(423, out string _phrase423);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase423 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(424, out string _phrase424);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase424 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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