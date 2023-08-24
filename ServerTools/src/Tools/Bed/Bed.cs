using System;
using UnityEngine;

namespace ServerTools
{
    public class Bed
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 10;
        public static string Command_bed = "bed";

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (player.SpawnPoints.Count > 0)
                    {
                        if (Delay_Between_Uses < 1)
                        {
                            if (Wallet.IsEnabled && Command_Cost > 0)
                            {
                                CommandCost(_cInfo, player);
                            }
                            else
                            {
                                Teleport(_cInfo, player.SpawnPoints[0].ToVector3());
                            }
                        }
                        else
                        {
                            DateTime lastBed = DateTime.Now;
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastBed != null)
                            {
                                lastBed = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastBed;
                            }
                            TimeSpan varTime = DateTime.Now - lastBed;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Reduced_Delay && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, player, timepassed, delay);
                                            return;
                                        }
                                    }
                                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, player, timepassed, delay);
                                            return;
                                        }
                                    }
                                }
                            }
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, player, timepassed, delay);
                                return;
                            }
                            Time(_cInfo, player, timepassed, Delay_Between_Uses);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Bed1", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bed.Exec: {0}", e.Message);
            }
        }

        private static void Time(ClientInfo _cInfo, EntityPlayer player, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Command_Cost > 0)
                    {
                        CommandCost(_cInfo, player);
                    }
                    else
                    {
                        Teleport(_cInfo, player.SpawnPoints[0].ToVector3());
                    }
                }
                else
                {
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Bed2", out string phrase);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_bed}", Command_bed);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bed.Time: {0}", e.Message);
            }
        }

        private static void CommandCost(ClientInfo _cInfo, EntityPlayer player)
        {
            try
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
                    Teleport(_cInfo, player.SpawnPoints[0].ToVector3());
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bed3", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bed.CommandCost: {0}", e.Message);
            }
        }

        private static void Teleport(ClientInfo _cInfo, Vector3 position)
        {
            try
            {
                Phrases.Dict.TryGetValue("Bed4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(position, null, false));
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastBed = DateTime.Now;
                PersistentContainer.DataChange = true;
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Bed.Teleport: {0}", e.Message);
            }
        }
    }
}
