using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Market
    {
        public static bool IsEnabled = false, Return = false, Player_Check = false, Zombie_Check = false, Reserved_Only = false, PvE = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static string Market_Position = "0,0,0", Command_marketback = "marketback", Command_mback = "mback", Command_set = "setmarket", Command_market = "market";
        public static List<int> MarketPlayers = new List<int>();

        public static void Set(ClientInfo _cInfo)
        {
            string[] _command = { Command_set };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo))
            {
                Phrases.Dict.TryGetValue("Market7", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _mposition = x + "," + y + "," + z;
                Market_Position = _mposition;
                Config.WriteXml();
                Phrases.Dict.TryGetValue("Market6", out string _phrase);
                _phrase = _phrase.Replace("{MarketPosition}", Market_Position);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.playerId))
            {
                Phrases.Dict.TryGetValue("Market8", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                DateTime _lastMarket = PersistentContainer.Instance.Players[_cInfo.playerId].LastMarket;
                TimeSpan varTime = DateTime.Now - _lastMarket;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
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
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue("Market1", out string _phrase);
                _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_market}", Command_market);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.GetCurrency(_cInfo.playerId) >= Command_Cost)
            {
                MarketTele(_cInfo);
            }
            else
            {
                Phrases.Dict.TryGetValue("Market9", out string _phrase);
                _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void MarketTele(ClientInfo _cInfo)
        {
            if (Market_Position != "0,0,0" || Market_Position != "0 0 0" || Market_Position != "")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (!MarketPlayers.Contains(_cInfo.entityId))
                    {
                        if (Player_Check)
                        {
                            if (Teleportation.PCheck(_cInfo, _player))
                            {
                                return;
                            }
                        }
                        if (Zombie_Check)
                        {
                            if (Teleportation.ZCheck(_cInfo, _player))
                            {
                                return;
                            }
                        }
                        if (!Teleportation.Teleporting.Contains(_cInfo.entityId))
                        {
                            Teleportation.Teleporting.Add(_cInfo.entityId);
                        }
                        if (Return)
                        {
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _mposition = x + "," + y + "," + z;
                            MarketPlayers.Add(_cInfo.entityId);
                            PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = _mposition;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Market2", out string _phrase);
                            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase = _phrase.Replace("{Command_marketback}", Command_marketback);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        string[] _cords = Market_Position.Split(',').ToArray();
                        if (int.TryParse(_cords[0], out int _x))
                        {
                            if (int.TryParse(_cords[1], out int _y))
                            {
                                if (int.TryParse(_cords[2], out int _z))
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.RemoveCurrency(_cInfo.playerId, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.playerId].LastMarket = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Market11", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Market4", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            if (MarketPlayers.Contains(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    string _lastPos = PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos;
                    if (_lastPos != "")
                    {
                        string[] _returnCoords = _lastPos.Split(',');
                        int.TryParse(_returnCoords[0], out int x);
                        int.TryParse(_returnCoords[1], out int y);
                        int.TryParse(_returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        MarketPlayers.Remove(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Market5", out string _phrase);
                        _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase = _phrase.Replace("{Command_marketback}", Command_marketback);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Market3", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void InsideMarket(ClientInfo _cInfo, EntityAlive _player)
        {
            if (!InsideMarket(_player.position.x, _player.position.z))
            {
                MarketPlayers.Remove(_cInfo.entityId);
                if (Return)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = "";
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Market5", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_marketback}", Command_marketback);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static bool InsideMarket(float _x, float _z)
        {
            string[] cords = Market_Position.Split(',').ToArray();
            int.TryParse(cords[0], out int x);
            int.TryParse(cords[2], out int z);
            if ((x - _x) * (x - _x) + (z - _z) * (z - _z) <= Market_Size * Market_Size)
            {
                return true;
            }
            return false;
        }

        public static bool PvEViolation(ClientInfo _cInfo2)
        {
            try
            {
                Phrases.Dict.TryGetValue("Market10", out string _phrase);
                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                if (PersistentOperations.PvEViolations.ContainsKey(_cInfo2.entityId))
                {
                    PersistentOperations.PvEViolations.TryGetValue(_cInfo2.entityId, out int _violations);
                    _violations++;
                    PersistentOperations.PvEViolations[_cInfo2.entityId] = _violations;
                    if (PersistentOperations.Jail_Violation > 0 && _violations == PersistentOperations.Jail_Violation)
                    {
                        PersistentOperations.Jail(_cInfo2);
                    }
                    if (PersistentOperations.Kill_Violation > 0 && _violations == PersistentOperations.Kill_Violation)
                    {
                        PersistentOperations.Kill(_cInfo2);
                    }
                    if (PersistentOperations.Kick_Violation > 0 && _violations == PersistentOperations.Kick_Violation)
                    {
                        PersistentOperations.Kick(_cInfo2);
                    }
                    else if (PersistentOperations.Ban_Violation > 0 && _violations == PersistentOperations.Ban_Violation)
                    {
                        PersistentOperations.Ban(_cInfo2);
                    }
                }
                else
                {
                    PersistentOperations.PvEViolations.Add(_cInfo2.entityId, 1);
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
