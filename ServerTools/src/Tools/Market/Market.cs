using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Market
    {
        public static bool IsEnabled = false, Return = false, Reserved_Only = false, PvE = true, Damage_Z = true, Bloodmoon = false, 
            Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static string Market_Position = "0,0,0", Command_marketback = "marketback", Command_mback = "mback", 
            Command_set = "setmarket", Command_market = "market";

        public static List<int> MarketPlayers = new List<int>();
        public static Bounds MarketBounds = new Bounds();

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
                            MarketBounds.center = new Vector3(x, y, z);
                            int size = Market_Size * 2;
                            MarketBounds.size = new Vector3(size, size, size);
                        }
                    }
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (!Bloodmoon && GeneralFunction.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Market13", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.PlatformId, _cInfo.CrossplatformId))
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
                Time(_cInfo, timepassed, Delay_Between_Uses);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Command_Cost >= 1 && Wallet.IsEnabled)
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
            if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= Command_Cost)
            {
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
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (!MarketPlayers.Contains(_cInfo.entityId))
                    {
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
                                    MarketPlayers.Add(_cInfo.entityId);
                                    if (Return)
                                    {
                                        Vector3 position = player.GetPosition();
                                        int x = (int)position.x;
                                        int y = (int)position.y;
                                        int z = (int)position.z;
                                        string mposition = x + "," + y + "," + z;
                                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos = mposition;
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));
                                        Phrases.Dict.TryGetValue("Market2", out string phrase);
                                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                        phrase = phrase.Replace("{Command_marketback}", Command_marketback);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(i, j, k), null, false));

                                    }
                                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                                    {
                                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastMarket = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Market11", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            if (MarketPlayers.Contains(_cInfo.entityId))
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    string lastPos = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos;
                    if (lastPos != "")
                    {
                        string[] returnCoords = lastPos.Split(',');
                        int.TryParse(returnCoords[0], out int x);
                        int.TryParse(returnCoords[1], out int y);
                        int.TryParse(returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        MarketPlayers.Remove(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Market5", out string phrase);
                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        phrase = phrase.Replace("{Command_marketback}", Command_marketback);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Market3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void InsideMarket(ClientInfo _cInfo, EntityAlive _player, List<Entity> _entityList)
        {
            if (!IsMarket(_player.position))
            {
                MarketPlayers.Remove(_cInfo.entityId);
                if (Return)
                {
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].MarketReturnPos = "";
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Market5", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_marketback}", Command_marketback);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                if (_entityList != null && _entityList.Count > 0)
                {
                    for (int i = 0; i < _entityList.Count; i++)
                    {
                        if (IsMarket(_entityList[i].position))
                        {
                            GameManager.Instance.World.RemoveEntity(_entityList[i].entityId, EnumRemoveEntityReason.Despawned);
                            Log.Out(string.Format("[SERVERTOOLS] Removed a hostile from the market @ '{0}'", _entityList[i].position));
                        }
                    }
                }
            }
        }

        public static bool IsMarket(Vector3 _position)
        {
            if (MarketBounds.Contains(_position))
            {
                return true;
            }
            return false;
        }

        public static bool PvEViolation(ClientInfo _cInfo2)
        {
            try
            {
                Phrases.Dict.TryGetValue("Market10", out string phrase);
                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                if (GeneralFunction.PvEViolations.ContainsKey(_cInfo2.entityId))
                {
                    GeneralFunction.PvEViolations.TryGetValue(_cInfo2.entityId, out int violations);
                    violations++;
                    GeneralFunction.PvEViolations[_cInfo2.entityId] = violations;
                    if (GeneralFunction.Jail_Violation > 0 && violations == GeneralFunction.Jail_Violation)
                    {
                        GeneralFunction.JailPlayer(_cInfo2);
                    }
                    if (GeneralFunction.Kill_Violation > 0 && violations == GeneralFunction.Kill_Violation)
                    {
                        GeneralFunction.KillPlayer(_cInfo2);
                    }
                    if (GeneralFunction.Kick_Violation > 0 && violations == GeneralFunction.Kick_Violation)
                    {
                        GeneralFunction.KickPlayer(_cInfo2);
                    }
                    else if (GeneralFunction.Ban_Violation > 0 && violations == GeneralFunction.Ban_Violation)
                    {
                        GeneralFunction.BanPlayer(_cInfo2);
                    }
                }
                else
                {
                    GeneralFunction.PvEViolations.Add(_cInfo2.entityId, 1);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Market.PvEViolation: {0}", e.Message));
            }
            return true;
        }
    }
}
