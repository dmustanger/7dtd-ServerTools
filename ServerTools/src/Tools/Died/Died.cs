using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Died
    {
        public static bool IsEnabled = false;
        public static int Time = 2, Delay_Between_Uses = 120, Command_Cost = 0, Min_Level = 0, Max_Level = 0;
        public static string Command_died = "died";
        public static Dictionary<int, DateTime> DeathTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> LastDeathPos = new Dictionary<int, string>();

        public static void Exec(ClientInfo _cInfo)
        {
            if (Delay_Between_Uses < 1)
            {
                if (Command_Cost >= 1 && Wallet.IsEnabled)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    PlayerLevel(_cInfo);
                }
            }
            else
            {
                DateTime lastDied = DateTime.Now;
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastDied != null)
                {
                    lastDied = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastDied;
                }
                TimeSpan varTime = DateTime.Now - lastDied;
                double fractionalMinutes = varTime.TotalMinutes;
                int timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Delay(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Delay(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                    }
                }
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                {
                    int delay = Delay_Between_Uses / 2;
                    Delay(_cInfo, timepassed, delay);
                    return;
                }
                Delay(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        public static void Delay(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    PlayerLevel(_cInfo);
                }
            }
            else
            {
                int timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("Died1", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_died}", Command_died);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            int currency = 0, bankCurrency = 0, cost = Command_Cost;
            if (Wallet.IsEnabled)
            {
                currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
            }
            if (Bank.IsEnabled && Bank.Direct_Payment)
            {
                bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
            }
            if (currency + bankCurrency >= cost)
            {
                if (currency > 0)
                {
                    if (currency < cost)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                        cost -= currency;
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    else
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                }
                else
                {
                    Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                }
                PlayerLevel(_cInfo);
            }
            else
            {
                Phrases.Dict.TryGetValue("Died3", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void PlayerLevel(ClientInfo _cInfo)
        {

            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player == null)
            {
                return;
            }
            if (Min_Level > 0 && player.Progression.Level < Min_Level)
            {
                Phrases.Dict.TryGetValue("Died5", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Max_Level > 0 && player.Progression.Level > Max_Level)
            {
                Phrases.Dict.TryGetValue("Died6", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            TeleportPlayer(_cInfo);
        }

        private static void TeleportPlayer(ClientInfo _cInfo)
        {
            if (DeathTime.ContainsKey(_cInfo.entityId))
            {
                if (DeathTime.TryGetValue(_cInfo.entityId, out DateTime _time))
                {
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int newTime = timepassed / 2;
                                    timepassed = newTime;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int newTime = timepassed / 2;
                                    timepassed = newTime;
                                }
                            }
                        }
                    }
                    if (timepassed < Time)
                    {
                        if (LastDeathPos.TryGetValue(_cInfo.entityId, out string _value))
                        {
                            string[] cords = _value.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y + 1, z), null, false));
                            DeathTime.Remove(_cInfo.entityId);
                            LastDeathPos.Remove(_cInfo.entityId);
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastDied = DateTime.Now;
                            PersistentContainer.DataChange = true;
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Died2", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Died4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void PlayerKilled(Entity _entity2)
        {
            if (!DeathTime.ContainsKey(_entity2.entityId))
            {
                Vector3 position = _entity2.GetPosition();
                int x = (int)position.x;
                int y = (int)position.y;
                int z = (int)position.z;
                string dposition = x + "," + y + "," + z;
                DeathTime.Add(_entity2.entityId, DateTime.Now);
                LastDeathPos.Add(_entity2.entityId, dposition);
            }
            else
            {
                Vector3 position = _entity2.GetPosition();
                int x = (int)position.x;
                int y = (int)position.y;
                int z = (int)position.z;
                string dposition = x + "," + y + "," + z;
                DeathTime[_entity2.entityId] = DateTime.Now;
                LastDeathPos[_entity2.entityId] = dposition;
            }
        }
    }
}
