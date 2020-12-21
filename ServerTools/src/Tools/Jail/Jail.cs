using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsEnabled = false, Jail_Shock = false;
        public static int Jail_Size = 8;
        public static string Command26 = "set jail", Command27 = "jail", Command28 = "unjail", Command55 = "forgive";
        public static string Jail_Position = "0,0,0";
        public static SortedDictionary<string, Vector3> JailReleasePosition = new SortedDictionary<string, Vector3>();
        public static List<string> Jailed = new List<string>();

        public static void SetJail(ClientInfo _cInfo)
        {
            string[] _command1 = { Command26 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command1, _cInfo))
            {
                Phrases.Dict.TryGetValue(199, out string _phrase199);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase199 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    int _x = (int)_player.position.x;
                    int _y = (int)_player.position.y;
                    int _z = (int)_player.position.z;
                    string _sposition = _x + "," + _y + "," + _z;
                    Jail_Position = _sposition;
                    LoadConfig.WriteXml();
                    Phrases.Dict.TryGetValue(192, out string _phrase192);
                    _phrase192 = _phrase192.Replace("{JailPosition}", Jail_Position);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase192 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void PutInJail(ClientInfo _cInfo, string _playerName)
        {
            string[] _command2 = { Command27 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command2, _cInfo))
            {
                Phrases.Dict.TryGetValue(199, out string _phrase199);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase199 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                if (Jail_Position == "0,0,0" || Jail_Position == "0 0 0" || Jail_Position == "")
                {
                    Phrases.Dict.TryGetValue(193, out string _phrase193);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase193 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    _playerName = _playerName.Replace(Command27 + " ", "");
                    ClientInfo _PlayertoJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoJail == null)
                    {
                        Phrases.Dict.TryGetValue(200, out string _phrase200);
                        _phrase200 = _phrase200.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase200 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (!Jailed.Contains(_PlayertoJail.playerId))
                        {
                            PutPlayerInJail(_cInfo, _PlayertoJail);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(194, out string _phrase194);
                            _phrase194 = _phrase194.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase194 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        private static void PutPlayerInJail(ClientInfo _cInfo, ClientInfo _PlayertoJail)
        {
            string[] _cords = Jail_Position.Split(',');
            int _x, _y, _z;
            int.TryParse(_cords[0], out _x);
            int.TryParse(_cords[1], out _y);
            int.TryParse(_cords[2], out _z);
            _PlayertoJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
            Jailed.Add(_PlayertoJail.playerId);
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailTime = 60;
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailName= _PlayertoJail.playerName;
            PersistentContainer.Instance.Players[_PlayertoJail.playerId].JailDate = DateTime.Now;
            Phrases.Dict.TryGetValue(190, out string _phrase190);
            ChatHook.ChatMessage(_PlayertoJail, LoadConfig.Chat_Response_Color + _phrase190 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            if (Jail_Shock)
            {
                Phrases.Dict.TryGetValue(197, out string _phrase197);
                ChatHook.ChatMessage(_PlayertoJail, LoadConfig.Chat_Response_Color + _phrase197 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            Phrases.Dict.TryGetValue(195, out string _phrase195);
            _phrase195 = _phrase195.Replace("{PlayerName}", _PlayertoJail.playerName);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase195 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void RemoveFromJail(ClientInfo _cInfo, string _playerName)
        {
            string[] _command3 = { Command28 };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_command3, _cInfo))
            {
                Phrases.Dict.TryGetValue(199, out string _phrase199);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase199 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                _playerName = _playerName.Replace("unjail ", "");
                ClientInfo _PlayertoUnJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoUnJail == null)
                {
                    Phrases.Dict.TryGetValue(200, out string _phrase200);
                    _phrase200 = _phrase200.Replace("{PlayerName}", _playerName);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase200 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    int _jailTime = PersistentContainer.Instance.Players[_PlayertoUnJail.playerId].JailTime;
                    if (_jailTime == 0)
                    {
                        Phrases.Dict.TryGetValue(196, out string _phrase196);
                        _phrase196 = _phrase196.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase196 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (!Jailed.Contains(_PlayertoUnJail.playerId))
                        {
                            Phrases.Dict.TryGetValue(196, out string _phrase196);
                            _phrase196 = _phrase196.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase196 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Jailed.Remove(_PlayertoUnJail.playerId);
                            PersistentContainer.Instance.Players[_PlayertoUnJail.playerId].JailTime = 0;
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_PlayertoUnJail.entityId];
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0 && (PersistentOperations.ClaimedByAllyOrSelf(_PlayertoUnJail.playerId, _position.GetPos()) || PersistentOperations.ClaimedByNone(_PlayertoUnJail.playerId, _position.GetPos())))
                            {

                                _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                if (PersistentOperations.ClaimedByAllyOrSelf(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)) || PersistentOperations.ClaimedByNone(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)))
                                {
                                    _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                }
                                else
                                {
                                    _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    if (PersistentOperations.ClaimedByAllyOrSelf(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)) || PersistentOperations.ClaimedByNone(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)))
                                    {
                                        _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                    }
                                    else
                                    {
                                        _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                        if (PersistentOperations.ClaimedByAllyOrSelf(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)) || PersistentOperations.ClaimedByNone(_PlayertoUnJail.playerId, new Vector3i(_pos[0].x, _pos[0].y, _pos[0].z)))
                                        {
                                            _PlayertoUnJail.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                                        }
                                    }
                                }
                            }
                            Phrases.Dict.TryGetValue(191, out string _phrase191);
                            ChatHook.ChatMessage(_PlayertoUnJail, LoadConfig.Chat_Response_Color + _phrase191 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        if (_player.Spawned && _player.IsAlive())
                        {
                            string[] _cords = Jail_Position.Split(',');
                            int.TryParse(_cords[0], out int _x);
                            int.TryParse(_cords[1], out int _y);
                            int.TryParse(_cords[2], out int _z);
                            Vector3 _vector3 = _player.position;
                            if ((_x - _vector3.x) * (_x - _vector3.x) + (_z - _vector3.z) * (_z - _vector3.z) >= Jail_Size * Jail_Size) 
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                if (Jail_Shock)
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("buff buffShocked", true));
                                    Phrases.Dict.TryGetValue(198, out string _phrase198);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase198 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue(200, out string _phrase200);
                    _phrase200 = _phrase200.Replace("{PlayerName}", _killId.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase200 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (!Jailed.Contains(_cInfoKiller.playerId))
                    {
                        Zones.Forgive.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue(196, out string _phrase196);
                        _phrase196 = _phrase196.Replace("{PlayerName}", _cInfoKiller.playerName);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase196 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoKiller.entityId];
                        if (_player.IsSpawned())
                        {
                            Zones.Forgive.Remove(_cInfo.entityId);
                            Jailed.Remove(_cInfoKiller.playerId);
                            PersistentContainer.Instance.Players[_cInfoKiller.playerId].JailTime = 0;
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
                            Phrases.Dict.TryGetValue(201, out string _phrase201);
                            _phrase201 = _phrase201.Replace("{PlayerName}", _cInfoKiller.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase201 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            Phrases.Dict.TryGetValue(202, out string _phrase202);
                            _phrase202 = _phrase202.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase202 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(203, out string _phrase203);
                            _phrase203 = _phrase203.Replace("{PlayerName}", _cInfoKiller.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase203 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    if (_jailTime > 0)
                    {
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
                                    Phrases.Dict.TryGetValue(191, out string _phrase191);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase191 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Jailed.Remove(_id);
                                PersistentContainer.Instance.Players[_cInfo.playerId].JailTime = 0;
                            }
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
                            }
                        }
                    }
                }
            }
        }
    }
}