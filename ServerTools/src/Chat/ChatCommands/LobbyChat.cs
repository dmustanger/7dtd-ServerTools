using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class LobbyChat
    {
        public static bool IsEnabled = false, Return = false, PvP_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0;
        public static string Command53 = "lobbyback", Command54 = "lback", Command88 = "lobby";
        public static List<int> LobbyPlayers = new List<int>();

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
                string _sql = string.Format("SELECT lastLobby FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime _lastLobby;
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastLobby);
                _result.Dispose();
                if (_lastLobby.ToString() == "10/29/2000 7:30:00 AM")
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
                    TimeSpan varTime = DateTime.Now - _lastLobby;
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
                                    string _phrase550;
                                    if (!Phrases.Dict.TryGetValue(550, out _phrase550))
                                    {
                                        _phrase550 = " you can only use {CommandPrivate}{Command88} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase550 = _phrase550.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase550 = _phrase550.Replace("{TimeRemaining}", _timeleft.ToString());
                                    _phrase550 = _phrase550.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                    _phrase550 = _phrase550.Replace("{Command88}", Command88);
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase550 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase550 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _phrase550;
                            if (!Phrases.Dict.TryGetValue(550, out _phrase550))
                            {
                                _phrase550 = " you can only use {CommandPrivate}{Command88} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase550 = _phrase550.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase550 = _phrase550.Replace("{TimeRemaining}", _timeleft.ToString());
                            _phrase550 = _phrase550.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase550 = _phrase550.Replace("{Command88}", Command88);
                            if (_announce)
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase550 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase550 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            if (SetLobby.Lobby_Position != "0,0,0" || SetLobby.Lobby_Position != "0 0 0" || SetLobby.Lobby_Position != "")
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
                    string _pposition = x + "," + y + "," + z;
                    LobbyPlayers.Add(_cInfo.entityId);
                    _sql = string.Format("UPDATE Players SET lobbyReturn = '{0}' WHERE steamid = '{1}'", _pposition, _cInfo.playerId);
                    SQL.FastQuery(_sql, "LobbyChat");
                    string _phrase552;
                    if (!Phrases.Dict.TryGetValue(552, out _phrase552))
                    {
                        _phrase552 = " you can go back by typing {CommandPrivate}{Command53} when you are ready to leave the lobby.";
                    }
                    _phrase552 = _phrase552.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase552 = _phrase552.Replace("{Command53}", Command53);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase552 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                string[] _cords = { };
                if (SetLobby.Lobby_Position.Contains(","))
                {
                    if (SetLobby.Lobby_Position.Contains(" "))
                    {
                        SetLobby.Lobby_Position.Replace(" ", "");
                    }
                    _cords = SetLobby.Lobby_Position.Split(',').ToArray();
                }
                else
                {
                    _cords = SetLobby.Lobby_Position.Split(' ').ToArray();
                }
                string _phrase553;
                if (!Phrases.Dict.TryGetValue(553, out _phrase553))
                {
                    _phrase553 = " sending you to the lobby.";
                }
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase553 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                _sql = string.Format("UPDATE Players SET lastLobby = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "LobbyChat");
            }
            else
            {
                string _phrase554;
                if (!Phrases.Dict.TryGetValue(554, out _phrase554))
                {
                    _phrase554 = " the lobby position is not set.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase554 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo, string _playerName)
        {
            string _sql = string.Format("SELECT lobbyReturn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_pos != "Unknown")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int x, y, z;
                string[] _cords = { };
                if (SetLobby.Lobby_Position.Contains(","))
                {
                    if (SetLobby.Lobby_Position.Contains(" "))
                    {
                        SetLobby.Lobby_Position.Replace(" ", "");
                    }
                    _cords = SetLobby.Lobby_Position.Split(',').ToArray();
                }
                else
                {
                    _cords = SetLobby.Lobby_Position.Split(' ').ToArray();
                }
                int.TryParse(_cords[0], out x);
                int.TryParse(_cords[1], out y);
                int.TryParse(_cords[2], out z);
                if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Lobby_Size * Lobby_Size)
                {
                    string[] _returnCoords = _pos.Split(',');
                    int.TryParse(_returnCoords[0], out x);
                    int.TryParse(_returnCoords[1], out y);
                    int.TryParse(_returnCoords[2], out z);
                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                    LobbyPlayers.Remove(_cInfo.entityId);
                    _sql = string.Format("UPDATE Players SET lobbyReturn = 'Unknown' WHERE steamid = '{0}'", _cInfo.playerId);
                    SQL.FastQuery(_sql, "LobbyChat");
                    string _phrase555;
                    if (!Phrases.Dict.TryGetValue(555, out _phrase555))
                    {
                        _phrase555 = " sent you back to your saved location.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase555 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _phrase556;
                    if (!Phrases.Dict.TryGetValue(556, out _phrase556))
                    {
                        _phrase556 = " you are outside the lobby. Get inside it and try again.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase556 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you have no saved return point[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
