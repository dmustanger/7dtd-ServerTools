using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerTools
{
    class Stuck
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60;

        public static void Delay(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                CheckLP(_cInfo);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastStuck == null)
                {
                    CheckLP(_cInfo);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastStuck;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            DateTime _dt;
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                            if (DateTime.Now < _dt)
                            {
                                _donator = true;
                                int _newDelay = Delay_Between_Uses / 2;
                                if (_timepassed >= _newDelay)
                                {
                                    CheckLP(_cInfo);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase920;
                                    if (!Phrases.Dict.TryGetValue(920, out _phrase920))
                                    {
                                        _phrase920 = "{PlayerName} you can only use /stuck once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase920 = _phrase920.Replace("{PlayerName}", _playerName);
                                    _phrase920 = _phrase920.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase920 = _phrase920.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase920), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase920), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            CheckLP(_cInfo);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase920;
                            if (!Phrases.Dict.TryGetValue(920, out _phrase920))
                            {
                                _phrase920 = "{PlayerName} you can only use /stuck once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase920 = _phrase920.Replace("{PlayerName}", _playerName);
                            _phrase920 = _phrase920.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase920 = _phrase920.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase920), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase920), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        public static void CheckLP(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _landOwner = _persistentPlayerList.GetLandProtectionBlockOwner(new Vector3i((int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerData(_cInfo.playerId);
            if (_landOwner != null)
            {
                if (_landOwner != _persistentPlayerData)
                {
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_landOwner.EntityId];
                    if (_player2 != null)
                    {
                        if (_player.IsFriendsWith(_player2))
                        {
                            if (CheckStuck(_player))
                            {
                                Exec(_cInfo, _player);
                            }
                            else
                            {
                                string _phrase923;
                                if (!Phrases.Dict.TryGetValue(923, out _phrase923))
                                {
                                    _phrase923 = "{PlayerName} you do not seem to be stuck.";
                                }
                                _phrase923 = _phrase923.Replace("{PlayerName}", _cInfo.playerName);
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase923), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                        else
                        {
                            string _phrase921;
                            if (!Phrases.Dict.TryGetValue(921, out _phrase921))
                            {
                                _phrase921 = "{PlayerName} you can not use this command here.";
                            }
                            _phrase921 = _phrase921.Replace("{PlayerName}", _cInfo.playerName);
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase921), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                    else
                    {
                        string _phrase921;
                        if (!Phrases.Dict.TryGetValue(921, out _phrase921))
                        {
                            _phrase921 = "{PlayerName} you can not use this command here.";
                        }
                        _phrase921 = _phrase921.Replace("{PlayerName}", _cInfo.playerName);
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase921), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
                else
                {
                    if (CheckStuck(_player))
                    {
                        Exec(_cInfo, _player);
                    }
                    else
                    {
                        string _phrase923;
                        if (!Phrases.Dict.TryGetValue(923, out _phrase923))
                        {
                            _phrase923 = "{PlayerName} you do not seem to be stuck.";
                        }
                        _phrase923 = _phrase923.Replace("{PlayerName}", _cInfo.playerName);
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase923), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
            else
            {
                if (CheckStuck(_player))
                {
                    Exec(_cInfo, _player);
                }
                else
                {
                    string _phrase923;
                    if (!Phrases.Dict.TryGetValue(923, out _phrase923))
                    {
                        _phrase923 = "{PlayerName} you do not seem to be stuck.";
                    }
                    _phrase923 = _phrase923.Replace("{PlayerName}", _cInfo.playerName);
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase923), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
        }

        public static bool CheckStuck(EntityPlayer _player)
        {
            int x = (int)_player.position.x;
            int y = (int)_player.position.y;
            int z = (int)_player.position.z;
            for (int i = x - 1; i <= (x + 1); i++)
            {
                for (int j = z - 1; j <= (z + 1); j++)
                {
                    for (int k = y - 2; k <= (y + 1); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        string _blockName = Block.Block.GetBlockName();
                        if (Block.type == BlockValue.Air.type || _player.IsInElevator() || _player.IsInWater())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void Exec(ClientInfo _cInfo, EntityPlayer _player)
        {
            Players.NoFlight.Add(_cInfo.entityId);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3((int)_player.position.x, -1, (int)_player.position.z), false));
            string _phrase922;
            if (!Phrases.Dict.TryGetValue(922, out _phrase922))
            {
                _phrase922 = "{PlayerName} you have been sent to the world surface. If you are still stuck, contact an administrator.";
            }
            _phrase922 = _phrase922.Replace("{PlayerName}", _cInfo.playerName);
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase922), Config.Server_Response_Name, false, "ServerTools", false));
            }
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastStuck = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}
