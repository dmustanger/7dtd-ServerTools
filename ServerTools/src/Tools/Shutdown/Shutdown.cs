using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public static class Shutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false, NoEntry = false, ShuttingDown = false, UI_Lock = false, UI_Locked = false, Interrupt_Bloodmoon = false;
        public static int Countdown = 2, Alert_Count = 2;
        public static string Command_shutdown = "shutdown", Time = "240";

        private static string EventDelay = "";

        public static void SetDelay()
        {
            if (EventDelay != Time)
            {
                EventDelay = Time;
                EventSchedule.Clear("Shutdown_");
                if (Time.Contains(",") && Time.Contains(":"))
                {
                    string[] times = Time.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            EventSchedule.Add("Shutdown_" + time);
                        }
                    }
                }
                else if (Time.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Time + ":00", out DateTime time))
                    {
                        EventSchedule.Add("Shutdown_" + time);
                    }
                }
                else
                {
                    if (int.TryParse(Time, out int delay))
                    {
                        EventSchedule.Add("Shutdown_" + DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Shutdown Time detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void PrepareShutdown()
        {
            if (GeneralFunction.IsBloodmoon() && !Interrupt_Bloodmoon)
            {
                EventSchedule.Add("Shutdown_" + DateTime.Now.AddMinutes(10));
                if (Event.Open && !Event.OperatorWarned)
                {
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(Event.Operator);
                    if (cInfo != null)
                    {
                        Event.OperatorWarned = true;
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + "A scheduled shutdown is set to begin but is on hold until the bloodmoon ends" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                return;
            }
            EventSchedule.Remove("Shutdown");
            StartShutdown(Countdown);
        }

        public static void StartShutdown(int _countdown)
        {
            if (Lottery.IsEnabled)
            {
                Lottery.DrawLotteryFast();
            }
            if (ExitCommand.IsEnabled)
            {
                ExitCommand.Players.Clear();
            }
            if (_countdown < 1)
            {
                _countdown = 1;
            }
            Timers.StopServerMinutes = _countdown;
            ShuttingDown = true;
            if (_countdown == 1)
            {
                OneMinute();
            }
            else
            {
                Phrases.Dict.TryGetValue("StopServer1", out string phrase);
                phrase = phrase.Replace("{Value}", _countdown.ToString());
                Alert(phrase, Shutdown.Alert_Count);
            }
        }

        public static void TimeRemaining(int _newCount)
        {
            if (GeneralFunction.IsBloodmoon() && !Interrupt_Bloodmoon)
            {
                EventSchedule.Add("Shutdown_" + DateTime.Now.AddMinutes(10));
                if (Event.Open && !Event.OperatorWarned)
                {
                    ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(Event.Operator);
                    if (cInfo != null)
                    {
                        Event.OperatorWarned = true;
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + "A scheduled shutdown is set to begin but is on hold until the bloodmoon ends" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                ShuttingDown = false;
                return;
            }
            Phrases.Dict.TryGetValue("StopServer1", out string phrase);
            phrase = phrase.Replace("{Value}", _newCount.ToString());
            Alert(phrase, Alert_Count);
        }

        public static void OneMinute()
        {
            NoEntry = true;
            Phrases.Dict.TryGetValue("StopServer2", out string phrase);
            Alert(phrase, 1);
            Phrases.Dict.TryGetValue("StopServer1", out phrase);
            phrase = phrase.Replace("{Value}", 1.ToString());
            Alert(phrase, 1);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("saveworld", null);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("mem clean", null);
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.Update();
            }
            if (UI_Lock)
            {
                Phrases.Dict.TryGetValue("StopServer4", out phrase);
                Alert(phrase, 1);
            }
        }

        public static void Lock()
        {
            UI_Locked = true;
            List<ClientInfo> clients = GeneralFunction.ClientList();
            if (clients != null)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close looting", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close trader", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation_campfire", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation_forge", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation_cementMixer", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation_workbench", true));
                    clients[i].SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close workstation_chemistryStation", true));
                }
            }
            Phrases.Dict.TryGetValue("StopServer5", out string phrase);
            Alert(phrase, 1);
        }

        public static void Kick()
        {
            PersistentContainer.Instance.Save();
            Phrases.Dict.TryGetValue("StopServer3", out string phrase);
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kickall \"{0}\"", phrase), null);
        }

        public static void Alert(string _message, int _count)
        {
            ChatHook.ChatMessage(null, "[FF0000]" + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            if (_count > 1)
            {
                Alert(_message, _count - 1);
            }
        }

        public static void Close()
        {
            Log.Out("[SERVERTOOLS] Running shutdown");
            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("shutdown", null);
        }

        public static void NextShutdown(ClientInfo _cInfo)
        {
            try
            {
                if (_cInfo != null)
                {
                    var schedule = EventSchedule.Schedule.ToArray();
                    for (int i = 0; i < schedule.Length; i++)
                    {
                        if (schedule[i].Key.Contains("Shutdown_") && DateTime.Now <= schedule[i].Value)
                        {
                            TimeSpan varTime = schedule[i].Value - DateTime.Now;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int remainingTime = (int)fractionalMinutes;
                            if (remainingTime < 0)
                            {
                                remainingTime = 0;
                            }
                            Phrases.Dict.TryGetValue("Shutdown1", out string phrase1);
                            phrase1 = phrase1.Replace("{TimeLeft}", string.Format("{0:00} H : {1:00} M", remainingTime / 60, remainingTime % 60));
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
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