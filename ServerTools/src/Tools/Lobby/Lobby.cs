using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Lobby
    {
        public static bool IsEnabled = false, Return = false, Player_Check = false, Zombie_Check = false, Reserved_Only = false, PvE = false;
        public static int Delay_Between_Uses = 5, Lobby_Size = 25, Command_Cost = 0, Player_Killing_Mode = 0;
        public static string Lobby_Position = "0,0,0", Command_lobbyback = "lobbyback", Command_lback = "lback", Command_set = "setlobby", Command_lobby = "lobby";
        public static List<int> LobbyPlayers = new List<int>();

        public static void Set(ClientInfo _cInfo)
        {
            string[] _command = { Command_set };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo))
            {
                Phrases.Dict.TryGetValue(248, out string _phrase248);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase248 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Config.WriteXml();
                    Phrases.Dict.TryGetValue(242, out string _phrase242);
                    _phrase242 = _phrase242.Replace("{LobbyPosition}", Lobby_Position);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase242 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            if (Reserved_Only && ReservedSlots.IsEnabled && !ReservedSlots.ReservedCheck(_cInfo.playerId))
            {
                Phrases.Dict.TryGetValue(249, out string _phrase249);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase249 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                DateTime _lastLobby = DateTime.Now;
                if (PersistentContainer.Instance.Players[_cInfo.playerId].LastLobby != null)
                {
                    _lastLobby = PersistentContainer.Instance.Players[_cInfo.playerId].LastLobby;
                }
                TimeSpan varTime = DateTime.Now - _lastLobby;
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
                Phrases.Dict.TryGetValue(241, out string _phrase241);
                _phrase241 = _phrase241.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase241 = _phrase241.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase241 = _phrase241.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase241 = _phrase241.Replace("{Command_lobby}", Command_lobby);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase241 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue(244, out string _phrase244);
                _phrase244 = _phrase244.Replace("{CoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase244 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void LobbyTele(ClientInfo _cInfo)
        {
            if (Lobby_Position != "0,0,0" && Lobby_Position != "0 0 0" && Lobby_Position != "")
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (!LobbyPlayers.Contains(_cInfo.entityId))
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
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue(243, out string _phrase243);
                            _phrase243 = _phrase243.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase243 = _phrase243.Replace("{Command_lobbyback}", Command_lobbyback);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase243 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        string[] _cords = Lobby_Position.Split(',').ToArray();
                        if (int.TryParse(_cords[0], out int _x))
                        {
                            if (int.TryParse(_cords[1], out int _y))
                            {
                                if (int.TryParse(_cords[2], out int _z))
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.playerId].LastLobby = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(250, out string _phrase250);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase250 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(245, out string _phrase245);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase245 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        string[] _returnCoords = _lastPos.Split(',');
                        int.TryParse(_returnCoords[0], out int x);
                        int.TryParse(_returnCoords[1], out int y);
                        int.TryParse(_returnCoords[2], out int z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        LobbyPlayers.Remove(_cInfo.entityId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos = "";
                        PersistentContainer.DataChange = true;
                    }
                    else
                    {
                        LobbyPlayers.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue(246, out string _phrase246);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase246 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(246, out string _phrase246);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase246 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void LobbyCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            if (!InsideLobby(_player.position.x, _player.position.z))
            {
                LobbyPlayers.Remove(_cInfo.entityId);
                PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos = "";
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue(247, out string _phrase247);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase247 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool InsideLobby(float _x, float _z)
        {
            string[] _cords = Lobby_Position.Split(',').ToArray();
            int.TryParse(_cords[0], out int x);
            int.TryParse(_cords[2], out int z);
            if ((x - _x) * (x - _x) + (z - _z) * (z - _z) <= Lobby_Size * Lobby_Size)
            {
                return true;
            }
            return false;
        }

        public static bool PvEViolation(ClientInfo _cInfo2)
        {
            try
            {
                Phrases.Dict.TryGetValue(260, out string _phrase260);
                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase260 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Lobby.PvEViolation: {0}", e.Message));
            }
            return true;
        }
    }
}
