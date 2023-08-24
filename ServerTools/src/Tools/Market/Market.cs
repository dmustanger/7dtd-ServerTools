using System;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Market
    {
        public static bool IsEnabled = false, Return = false, Reserved_Only = false, PvE = true, Bloodmoon = false, 
            Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static string Market_Position = "0,0,0", Command_marketback = "marketback", Command_mback = "mback", 
            Command_set = "setmarket", Command_market = "market";

        public static float[] MarketBounds = new float[6];

        public static void SetBounds(string _position)
        {
            if (_position != "0,0,0" && _position != "0 0 0" && _position != "" && _position.Contains(","))
            {
                string[] cords = _position.Split(',').ToArray();
                if (int.TryParse(cords[0], out int x))
                {
                    if (int.TryParse(cords[1], out int y))
                    {
                        if (int.TryParse(cords[2], out int z))
                        {
                            Market_Position = _position;
                            Bounds bounds = new Bounds();
                            bounds.center = new Vector3(x, y, z);
                            int size = Market_Size * 2;
                            bounds.size = new Vector3(size, size, size);
                            MarketBounds[0] = bounds.min.x;
                            MarketBounds[1] = bounds.min.y;
                            MarketBounds[2] = bounds.min.z;
                            MarketBounds[3] = bounds.max.x;
                            MarketBounds[4] = bounds.max.y;
                            MarketBounds[5] = bounds.max.z;
                            Log.Out(string.Format("[SERVERTOOLS] Market has been set to position '{0}'", _position));
                            return;
                        }
                    }
                }
            }
            Log.Out(string.Format("[SERVERTOOLS] Unable to set market bounds using position '{0}'", _position));
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (!Bloodmoon && GeneralOperations.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Market13", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.IsReserved(_cInfo))
            {
                Phrases.Dict.TryGetValue("Market8", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    MarketTele(_cInfo);
                }
            }
            else
            {
                DateTime lastMarket = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastMarket;
                TimeSpan varTime = DateTime.Now - lastMarket;
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
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, timepassed, delay);
                                return;
                            }
                        }
                    }
                }
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                {
                    int delay = Delay_Between_Uses / 2;
                    Time(_cInfo, timepassed, delay);
                    return;
                }
                Time(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost > 0)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    MarketTele(_cInfo);
                }
            }
            else
            {
                int timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("Market1", out string phrase);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_market}", Command_market);
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
                MarketTele(_cInfo);
            }
            else
            {
                Phrases.Dict.TryGetValue("Market9", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MarketTele(ClientInfo _cInfo)
        {
            if (Market_Position != "0,0,0" || Market_Position != "0 0 0" || Market_Position != "")
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (IsMarket(player.position))
                    {
                        Phrases.Dict.TryGetValue("Market11", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    if (Player_Check)
                    {
                        if (Teleportation.PCheck(_cInfo, player))
                        {
                            return;
                        }
                    }
                    if (Zombie_Check)
                    {
                        if (Teleportation.ZCheck(_cInfo, player))
                        {
                            return;
                        }
                    }
                    string[] cords = Market_Position.Split(',').ToArray();
                    if (int.TryParse(cords[0], out int i))
                    {
                        if (int.TryParse(cords[1], out int j))
                        {
                            if (int.TryParse(cords[2], out int k))
                            {
                                if (Return)
                                {
                                    Vector3 position = player.GetPosition();
                                    Phrases.Dict.TryGetValue("Market2", out string phrase);
                                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    phrase = phrase.Replace("{Command_mback}", Command_mback);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    int x = (int)position.x;
                                    int y = (int)position.y;
                                    int z = (int)position.z;
                                    string mposition = x + "," + y + "," + z;
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos = mposition;
                                }
                                if (!TeleportDetector.Omissions.Contains(_cInfo.entityId))
                                {
                                    TeleportDetector.Omissions.Add(_cInfo.entityId);
                                }
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastMarket = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Market4", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                if (IsMarket(player.position))
                {
                    string lastPos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos;
                    if (lastPos != null && lastPos != "")
                    {
                        string[] returnCoords = lastPos.Split(',');
                        int.TryParse(returnCoords[0], out int x);
                        int.TryParse(returnCoords[1], out int y);
                        int.TryParse(returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Market3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Market5", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static bool IsMarket(Vector3 _position)
        {
            if (_position.x >= MarketBounds[0] && _position.y >= MarketBounds[1] && _position.z >= MarketBounds[2] &&
                _position.x <= MarketBounds[3] && _position.y <= MarketBounds[4] && _position.z <= MarketBounds[5])
            {
                return true;
            }
            return false;
        }

        public static void PvEViolation(ClientInfo _cInfo)
        {
            try
            {
                if (GeneralOperations.PvEViolations.ContainsKey(_cInfo.entityId))
                {
                    GeneralOperations.PvEViolations.TryGetValue(_cInfo.entityId, out int _violations);
                    GeneralOperations.PvEViolations[_cInfo.entityId] += 1;
                    if (TooManyViolations(_cInfo, _violations + 1))
                    {
                        return;
                    }
                }
                else
                {
                    GeneralOperations.PvEViolations.Add(_cInfo.entityId, 1);
                    if (TooManyViolations(_cInfo, 1))
                    {
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("Market10", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Market.PvEViolation: {0}", e.Message);
            }
        }

        public static bool TooManyViolations(ClientInfo _cInfo, int _violations)
        {
            if (GeneralOperations.Jail_Violation > 0 && _violations == GeneralOperations.Jail_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                GeneralOperations.JailPlayer(_cInfo);
                return true;
            }
            if (GeneralOperations.Kill_Violation > 0 && _violations == GeneralOperations.Kill_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                GeneralOperations.KillPlayer(_cInfo, 1);
                return true;
            }
            if (GeneralOperations.Kick_Violation > 0 && _violations == GeneralOperations.Kick_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Market15", out string phrase);
                GeneralOperations.KickPlayer(_cInfo, phrase);
                return true;
            }
            else if (GeneralOperations.Ban_Violation > 0 && _violations == GeneralOperations.Ban_Violation)
            {
                GeneralOperations.PvEViolations.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("Market16", out string phrase);
                GeneralOperations.BanPlayer(_cInfo, phrase);
                return true;
            }
            return false;
        }
    }
}
