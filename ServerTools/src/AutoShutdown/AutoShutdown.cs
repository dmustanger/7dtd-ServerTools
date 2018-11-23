using System.Collections.Generic;
using System;

namespace ServerTools
{
    public static class AutoShutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false, Bloodmoon = false, Kick_Login = false;
        public static int Countdown_Timer = 2, Days_Until_Horde = 7;
        public static List<DateTime> timerStart = new List<DateTime>();

        public static void ShutdownList()
        {
            timerStart.Clear();
            timerStart.Add(DateTime.Now);
        }

        public static void CheckBloodmoon()
        {
            ulong _worldTime = GameManager.Instance.World.worldTime;
            int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(_worldTime) % Days_Until_Horde;
            int _worldHours = (int)(_worldTime / 1000UL) % 24;
            if (_daysUntilHorde == Days_Until_Horde && (_worldHours >= (int)SkyManager.GetDuskTime() - 3) || SkyManager.BloodMoon())
            {
                Bloodmoon = true;
            }
            else
            {
                Bloodmoon = false;
                Auto_Shutdown();
            }
        }

        public static void Auto_Shutdown()
        {
            Log.Out("[SERVERTOOLS] Running auto shutdown.");
            ChatHook.ChatMessage(null, "[FF0000]Auto shutdown initiated[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", Countdown_Timer), (ClientInfo)null);
        }

        public static void CheckNextShutdown(ClientInfo _cInfo, bool _announce)
        {
            if (!Event.Open)
            {
                if (!Bloodmoon)
                {
                    if (!StopServer.stopServerCountingDown)
                    {
                        DateTime _timeStart = timerStart[0];
                        TimeSpan varTime = DateTime.Now - _timeStart;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timeMinutes = (int)fractionalMinutes;
                        int _timeleftMinutes = Timers.Shutdown_Delay - _timeMinutes;
                        if (_timeleftMinutes > 0)
                        {
                            string TimeLeft;
                            TimeLeft = string.Format("{0:00} H : {1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                            if (_announce)
                            {
                                string _phrase730;
                                if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                                {
                                    _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                                }
                                _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase730 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                string _phrase730;
                                if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                                {
                                    _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                                }
                                _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase730 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                    }
                    else
                    {
                        if (_announce)
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", the server has already started the shutdown process.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", the server has already started the shutdown process.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                    }
                }
                else
                {
                    if (_announce)
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", a bloodmoon is currently active. The server is set to shutdown after it finishes.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", a bloodmoon is currently active. The server is set to shutdown after it finishes.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            else
            {
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", a event is currently active. The server can not auto shutdown until it finishes.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", a event is currently active. The server can not auto shutdown until it finishes.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}