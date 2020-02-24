using System.Collections.Generic;
using System;

namespace ServerTools
{
    public static class AutoShutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false, Bloodmoon = false, BloodmoonOver = false, Kick_Login = false;
        public static int Countdown_Timer = 2, Delay = 120;
        public static string Command47 = "shutdown";
        public static List<DateTime> timerStart = new List<DateTime>();

        public static void BloodmoonCheck()
        {
            try
            {
                if (Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime())) == 0)
                {
                    Bloodmoon = true;
                    return;
                }
                else if (Bloodmoon)
                {
                    Bloodmoon = false;
                    BloodmoonOver = true;
                    return;
                }
                Shutdown();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdown.BloodmoonCheck: {0}", e.Message));
            }
        }

        public static void BloodmoonOverAlert()
        {
            ChatHook.ChatMessage(null, "[FF0000]Auto shutdown detected the bloodmoon has ended. Fifteen minutes remain before auto shutdown initiates[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void Shutdown()
        {
            try
            {
                Log.Out("[SERVERTOOLS] Running auto shutdown.");
                ChatHook.ChatMessage(null, "[FF0000]Auto shutdown initiated[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", Countdown_Timer), (ClientInfo)null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdown.Shutdown: {0}", e.Message));
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
                            if (!StopServer.StopServerCountingDown)
                            {
                                int _time;
                                if (BloodmoonOver)
                                {
                                    _time = Delay - (Timers._autoShutdownBloodmoonOver / 60);
                                }
                                else
                                {
                                    _time = Delay - (Timers._autoShutdown / 60);
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
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " the server has already started the shutdown process.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " a bloodmoon is currently active. The server is set to shutdown after it finishes.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " a event is currently active. The server is set to shutdown after it finishes.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdown.NextShutdown: {0}", e.Message));
            }
        }
    }
}