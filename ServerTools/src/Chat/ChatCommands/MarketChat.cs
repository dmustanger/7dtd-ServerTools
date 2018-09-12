using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class MarketChat
    {
        public static bool IsEnabled = false, Return = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Market_Size = 25, Command_Cost = 0;
        public static List<int> MarketPlayers = new List<int>();

        public static void Delay(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _playerName);
                }
                else
                {
                    Exec(_cInfo, _playerName);
                }
            }
            else
            {
                string _sql = string.Format("SELECT lastMarket FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastMarket;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastMarket);
                _result.Dispose();
                if (_lastMarket.ToString() == "10/29/2000 7:30:00 AM")
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, _playerName);
                    }
                    else
                    {
                        Exec(_cInfo, _playerName);
                    }
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - _lastMarket;
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
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, _playerName);
                                    }
                                    else
                                    {
                                        Exec(_cInfo, _playerName);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase560;
                                    if (!Phrases.Dict.TryGetValue(560, out _phrase560))
                                    {
                                        _phrase560 = "{PlayerName} you can only use /market once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase560 = _phrase560.Replace("{PlayerName}", _playerName);
                                    _phrase560 = _phrase560.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase560 = _phrase560.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase560), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase560), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, _playerName);
                            }
                            else
                            {
                                Exec(_cInfo, _playerName);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase560;
                            if (!Phrases.Dict.TryGetValue(560, out _phrase560))
                            {
                                _phrase560 = "{PlayerName} you can only use /market once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase560 = _phrase560.Replace("{PlayerName}", _playerName);
                            _phrase560 = _phrase560.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase560 = _phrase560.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase560), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase560), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _playerName)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                Exec(_cInfo, _playerName);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            if (SetMarket.Market_Position != "0,0,0")
            {
                int x, y, z;
                string _sql;
                if (Return)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 _position = _player.GetPosition();
                    x = (int)_position.x;
                    y = (int)_position.y;
                    z = (int)_position.z;
                    if (PvP_Check)
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
                    string _mposition = x + "," + y + "," + z;
                    MarketPlayers.Add(_cInfo.entityId);
                    _sql = string.Format("UPDATE Players SET marketReturn = '{0}' WHERE steamid = '{1}'", _mposition, _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    string _phrase561;
                    if (!Phrases.Dict.TryGetValue(561, out _phrase561))
                    {
                        _phrase561 = "{PlayerName} you can go back by typing /return when you are ready to leave the market.";
                    }
                    _phrase561 = _phrase561.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase561), Config.Server_Response_Name, false, "ServerTools", false));
                }
                string[] _cords = SetMarket.Market_Position.Split(',');
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                Players.NoFlight.Add(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                string _phrase562;
                if (!Phrases.Dict.TryGetValue(562, out _phrase562))
                {
                    _phrase562 = "{PlayerName} sending you to the market.";
                }
                _phrase562 = _phrase562.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase562), Config.Server_Response_Name, false, "ServerTools", false));
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                _sql = string.Format("UPDATE Players SET lastMarket = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
            else
            {
                string _phrase563;
                if (!Phrases.Dict.TryGetValue(563, out _phrase563))
                {
                    _phrase563 = "{PlayerName} the market position is not set.";
                }
                _phrase563 = _phrase563.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase563), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void SendBack(ClientInfo _cInfo, string _playerName)
        {
            string _sql = string.Format("SELECT marketReturn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_pos != "Unknown")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int x, y, z;
                string[] _cords = SetMarket.Market_Position.Split(',');
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market_Size * Market_Size)
                {
                    string[] _returnCoords = _pos.Split(',');
                    int.TryParse(_returnCoords[0], out x);
                    int.TryParse(_returnCoords[1], out y);
                    int.TryParse(_returnCoords[2], out z);
                    Players.NoFlight.Add(_cInfo.entityId);
                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                    MarketPlayers.Remove(_cInfo.entityId);
                    string _phrase555;
                    if (!Phrases.Dict.TryGetValue(555, out _phrase555))
                    {
                        _phrase555 = "{PlayerName} sending you back to your saved location.";
                    }
                    _phrase555 = _phrase555.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase555), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    string _phrase564;
                    if (!Phrases.Dict.TryGetValue(564, out _phrase564))
                    {
                        _phrase564 = "{PlayerName} you are outside the market. Get inside it and try again.";
                    }
                    _phrase564 = _phrase564.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase564), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}
