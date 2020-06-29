using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Lobby
    {
        public static bool IsEnabled = false, Return = false, Player_Check = false, Zombie_Check = false, Donor_Only = false, PvE = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0;
        public static string Lobby_Position = "0,0,0", Command53 = "lobbyback", Command54 = "lback", Command87 = "setlobby", Command88 = "lobby";
        public static List<int> LobbyPlayers = new List<int>();

        public static void Set(ClientInfo _cInfo)
        {
            string[] _command = { Command87 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase107 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _lposition = x + "," + y + "," + z;
                    Lobby_Position = _lposition;
                    string _phrase551;
                    if (!Phrases.Dict.TryGetValue(551, out _phrase551))
                    {
                        _phrase551 = " you have set the lobby position as {LobbyPosition}.";
                    }
                    _phrase551 = _phrase551.Replace("{LobbyPosition}", Lobby_Position);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase551 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    LoadConfig.WriteXml();
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Donor_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.playerId))
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " this command is locked to donors only" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    LobbyTele(_cInfo);
                }
            }
            else
            {
                DateTime _lastLobby = PersistentContainer.Instance.Players[_cInfo.playerId].LastLobby;
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
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    LobbyTele(_cInfo);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase550;
                if (!Phrases.Dict.TryGetValue(550, out _phrase550))
                {
                    _phrase550 = " you can only use {CommandPrivate}{Command88} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase550 = _phrase550.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase550 = _phrase550.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase550 = _phrase550.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase550 = _phrase550.Replace("{Command88}", Command88);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase550 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
            {
                LobbyTele(_cInfo);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void LobbyTele(ClientInfo _cInfo)
        {
            if (Lobby.Lobby_Position != "0,0,0" || Lobby.Lobby_Position != "0 0 0" || Lobby.Lobby_Position != "")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
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
                    int x, y, z;
                    if (Return)
                    {
                        Vector3 _position = _player.GetPosition();
                        x = (int)_position.x;
                        y = (int)_position.y;
                        z = (int)_position.z;
                        string _pposition = x + "," + y + "," + z;
                        LobbyPlayers.Add(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos = _pposition;
                        string _phrase552;
                        if (!Phrases.Dict.TryGetValue(552, out _phrase552))
                        {
                            _phrase552 = " you can go back by typing {CommandPrivate}{Command53} when you are ready to leave the lobby.";
                        }
                        _phrase552 = _phrase552.Replace("{CommandPrivate}", ChatHook.Command_Private);
                        _phrase552 = _phrase552.Replace("{Command53}", Command53);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase552 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    string[] _cords = { };
                    if (Lobby.Lobby_Position.Contains(","))
                    {
                        if (Lobby.Lobby_Position.Contains(" "))
                        {
                            Lobby.Lobby_Position.Replace(" ", "");
                        }
                        _cords = Lobby.Lobby_Position.Split(',').ToArray();
                    }
                    else if (Lobby.Lobby_Position.Contains(" "))
                    {
                        _cords = Lobby.Lobby_Position.Split(' ').ToArray();
                    }
                    string _phrase553;
                    if (!Phrases.Dict.TryGetValue(553, out _phrase553))
                    {
                        _phrase553 = " sending you to the lobby.";
                    }
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase553 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastLobby = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
            else
            {
                string _phrase554;
                if (!Phrases.Dict.TryGetValue(554, out _phrase554))
                {
                    _phrase554 = " the lobby position is not set.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase554 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SendBack(ClientInfo _cInfo)
        {
            if (LobbyPlayers.Contains(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    string _lastPos = PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos;
                    if (_lastPos != "")
                    {
                        int x, y, z;
                        string[] _returnCoords = _lastPos.Split(',');
                        int.TryParse(_returnCoords[0], out x);
                        int.TryParse(_returnCoords[1], out y);
                        int.TryParse(_returnCoords[2], out z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        LobbyPlayers.Remove(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos = "";
                        PersistentContainer.Instance.Save();
                        string _phrase555;
                        if (!Phrases.Dict.TryGetValue(555, out _phrase555))
                        {
                            _phrase555 = " sent you back to your saved location.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase555 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _phrase556;
                        if (!Phrases.Dict.TryGetValue(556, out _phrase556))
                        {
                            _phrase556 = " you are outside the lobby. Get inside it and try again.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase556 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you have no saved return point[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool InsideLobby(float _x, float _z)
        {
            int x, z;
            string[] _cords = Lobby.Lobby_Position.Split(',').ToArray();
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[2], out z);
            if ((x - _x) * (x - _x) + (z - _z) * (z - _z) <= Lobby_Size * Lobby_Size)
            {
                return true;
            }
            return false;
        }
    }
}
