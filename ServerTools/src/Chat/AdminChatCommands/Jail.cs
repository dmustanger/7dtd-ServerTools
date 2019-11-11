using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsEnabled = false, Jail_Shock = false;
        public static int Jail_Size = 8;
        public static string Command26 = "setjail", Command27 = "jail", Command28 = "unjail", Command55 = "forgive";
        public static string Jail_Position = "0,0,0";
        public static SortedDictionary<string, Vector3> JailReleasePosition = new SortedDictionary<string, Vector3>();
        public static List<string> Jailed = new List<string>();

        public static void SetJail(ClientInfo _cInfo)
        {
            string[] _command1 = { Command26 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command1, _cInfo.playerId))
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
                int x = (int)_player.position.x;
                int y = (int)_player.position.y;
                int z = (int)_player.position.z;
                string _sposition = x + "," + y + "," + z;
                Jail_Position = _sposition;
                string _phrase502;
                if (!Phrases.Dict.TryGetValue(502, out _phrase502))
                {
                    _phrase502 = " you have set the jail position as {JailPosition}.";
                }
                _phrase502 = _phrase502.Replace("{JailPosition}", Jail_Position);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase502 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                LoadConfig.WriteXml();
            }
        }

        public static void PutInJail(ClientInfo _cInfo, string _playerName)
        {
            string[] _command2 = { Command27 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command2, _cInfo.playerId))
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
                if (Jail_Position == "0,0,0" || Jail_Position == "0 0 0" || Jail_Position == "")
                {
                    string _phrase503;
                    if (!Phrases.Dict.TryGetValue(503, out _phrase503))
                    {
                        _phrase503 = " the jail position has not been set.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase503 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    _playerName = _playerName.Replace(Command27 + " ", "");
                    ClientInfo _PlayertoJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoJail == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = " player {PlayerName} was not found.";
                        }
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase201 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (!Jailed.Contains(_PlayertoJail.playerId))
                        {
                            PutPlayerInJail(_cInfo, _PlayertoJail);
                        }
                        else
                        {
                            string _phrase504;
                            if (!Phrases.Dict.TryGetValue(504, out _phrase504))
                            {
                                _phrase504 = " player {PlayerName} is already in jail.";
                            }
                            _phrase504 = _phrase504.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase504 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        private static void PutPlayerInJail(ClientInfo _cInfo, ClientInfo _PlayertoJail)
        {
            string[] _cords = { };
            if (Jail_Position.Contains(","))
            {
                if (Jail_Position.Contains(" "))
                {
                    Jail_Position.Replace(" ", "");
                }
                _cords = Jail_Position.Split(',');
            }
            else
            {
                _cords = Jail_Position.Split(' ');
            }
            int x, y, z;
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _PlayertoJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            Jailed.Add(_PlayertoJail.playerId);
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailTime = 60;
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailName= _PlayertoJail.playerName;
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailDate = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase500;
            if (!Phrases.Dict.TryGetValue(500, out _phrase500))
            {
                _phrase500 = " you have been sent to jail.";
            }
            ChatHook.ChatMessage(_PlayertoJail, LoadConfig.Chat_Response_Color + _PlayertoJail.playerName + LoadConfig.Chat_Response_Color + _phrase500 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            if (Jail_Shock)
            {
                string _phrase507;
                if (!Phrases.Dict.TryGetValue(507, out _phrase507))
                {
                    _phrase507 = " the jail is electrified. Do not try to leave it.";
                }
                ChatHook.ChatMessage(_PlayertoJail, LoadConfig.Chat_Response_Color + _PlayertoJail.playerName + LoadConfig.Chat_Response_Color + _phrase507 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            string _phrase505;
            if (!Phrases.Dict.TryGetValue(505, out _phrase505))
            {
                _phrase505 = " you have put {PlayerName} in jail.";
            }
            _phrase505 = _phrase505.Replace("{PlayerName}", _PlayertoJail.playerName);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase505 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void RemoveFromJail(ClientInfo _cInfo, string _playerName)
        {
            string[] _command3 = { Command28 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command3, _cInfo.playerId))
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
                _playerName = _playerName.Replace("unjail ", "");
                ClientInfo _PlayertoUnJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoUnJail == null)
                {
                    string _phrase201;
                    if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                    {
                        _phrase201 = " player {PlayerName} was not found online.";
                    }
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase201 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    int _jailTime = PersistentContainer.Instance.Players[_PlayertoUnJail.playerId].JailTime;
                    if (_jailTime == 0)
                    {
                        string _phrase506;
                        if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                        {
                            _phrase506 = " player {PlayerName} is not in jail.";
                        }
                        _phrase506 = _phrase506.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase506 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (!Jailed.Contains(_PlayertoUnJail.playerId))
                        {
                            string _phrase506;
                            if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                            {
                                _phrase506 = " player {PlayerName} is not in jail.";
                            }
                            _phrase506 = _phrase506.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase506 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Jailed.Remove(_PlayertoUnJail.playerId);
                            PersistentContainer.Instance.Players[_PlayertoUnJail.playerId].JailTime = 0;
                            PersistentContainer.Instance.Save();
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_PlayertoUnJail.entityId];
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                            }
                            string _phrase501;
                            if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                            {
                                _phrase501 = " you have been released from jail.";
                            }
                            ChatHook.ChatMessage(_PlayertoUnJail, LoadConfig.Chat_Response_Color + _PlayertoUnJail.playerName + LoadConfig.Chat_Response_Color + _phrase501 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void StatusCheck()
        {
            if (Jailed.Count > 0)
            {
                for (int i = 0; i < Jailed.Count; i++)
                {
                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(Jailed[i]);
                    if (_cInfo != null)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player.Spawned)
                        {
                            int x, y, z;
                            string[] _cords = { };
                            if (Jail_Position.Contains(","))
                            {
                                if (Jail_Position.Contains(" "))
                                {
                                    Jail_Position.Replace(" ", "");
                                }
                                _cords = Jail_Position.Split(',');
                            }
                            else
                            {
                                _cords = Jail_Position.Split(' ');
                            }
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            Vector3 _vector3 = _player.position;
                            if ((x - _vector3.x) * (x - _vector3.x) + (z - _vector3.z) * (z - _vector3.z) >= Jail_Size * Jail_Size) 
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                                if (Jail_Shock)
                                {
                                    _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup("buff buffShocked", true));
                                    string _phrase508;
                                    if (!Phrases.Dict.TryGetValue(508, out _phrase508))
                                    {
                                        _phrase508 = " don't pee on the electric fence.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase508 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Forgive(ClientInfo _cInfo)
        {
            int _killId;
            if (Zones.Forgive.TryGetValue(_cInfo.entityId, out _killId))
            {
                ClientInfo _cInfoKiller = ConnectionManager.Instance.Clients.ForEntityId(_killId);
                if (_cInfoKiller == null)
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", this player is not online and so can not be forgiven or removed from jail.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (!Jailed.Contains(_cInfoKiller.playerId))
                    {
                        Zones.Forgive.Remove(_cInfo.entityId);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", this player is not in jail.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoKiller.entityId];
                        if (_player.IsSpawned())
                        {
                            Zones.Forgive.Remove(_cInfo.entityId);
                            Jailed.Remove(_cInfoKiller.playerId);
                            PersistentContainer.Instance.Players[_cInfoKiller.playerId].JailTime = 0;
                            PersistentContainer.Instance.Save();
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                _cInfoKiller.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _cInfoKiller.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                            }
                            string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have forgiven {PlayerName} and released them from jail.[-]";
                            _chatMessage = _chatMessage.Replace("{PlayerName}", _cInfoKiller.playerName);
                            ChatHook.ChatMessage(_cInfo, _chatMessage, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have been forgiven and released from jail by {PlayerName}.[-]";
                            _chatMessage = _chatMessage.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfoKiller, _chatMessage, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " this player is not spawned and so can not be forgiven or removed from jail.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < Jailed.Count; i++)
            {
                string _id = Jailed[i];
                if (Jailed.Contains(_id))
                {
                    int _jailTime = PersistentContainer.Instance.Players[_id].JailTime;
                    DateTime _jailDate = PersistentContainer.Instance.Players[_id].JailDate;
                    if (_jailTime == -1)
                    {
                        break;
                    }
                    TimeSpan varTime = DateTime.Now - _jailDate;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= _jailTime)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                        if (_cInfo != null)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            if (_player.IsSpawned())
                            {
                                Jailed.Remove(_id);
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = 0;
                                PersistentContainer.Instance.Save();
                                EntityBedrollPositionList _position = _player.SpawnPoints;
                                if (_position.Count > 0)
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                }
                                string _phrase501;
                                if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                                {
                                    _phrase501 = " you have been released from jail.";
                                }
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase501 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Jailed.Remove(_id);
                            PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = 0;
                            PersistentContainer.Instance.Save();
                        }
                    }
                }
            }
        }

        public static void JailList()
        {
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                {
                    int _jailTime = p.JailTime;
                    if (_jailTime > 0 || _jailTime == -1)
                    {
                        if (_jailTime == -1)
                        {
                            Jailed.Add(_id);
                        }
                        else
                        {
                            DateTime _jailDate = p.JailDate;
                            TimeSpan varTime = DateTime.Now - _jailDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed < _jailTime)
                            {
                                Jailed.Add(_id);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_id].JailTime = 0;
                                PersistentContainer.Instance.Save();
                            }
                        }
                    }
                }
            }
        }
    }
}