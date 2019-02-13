using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    public class Jail
    {
        public static bool IsEnabled = false, Jail_Shock = false;
        public static int Jail_Size = 8;
        public static string Command26 = "setjail", Command27 = "jail", Command28 = "unjail", Command55 = "forgive";
        private static string[] _cmd = { Command27 };
        public static string Jail_Position = "0,0,0";
        public static SortedDictionary<string, Vector3> JailReleasePosition = new SortedDictionary<string, Vector3>();
        public static List<string> Jailed = new List<string>();

        public static void SetJail(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
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
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
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
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
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
            Players.NoFlight.Add(_PlayertoJail.entityId);
            _PlayertoJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
            Jailed.Add(_PlayertoJail.playerId);
            string _sql = string.Format("UPDATE Players SET jailTime = 60, jailName = '{0}', jailDate = '{1}' WHERE steamid = '{2}'", _PlayertoJail.playerName, DateTime.Now, _PlayertoJail.playerId);
            SQL.FastQuery(_sql);
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
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
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
                    Player p = PersistentContainer.Instance.Players[_PlayertoUnJail.playerId, false];
                    if (p == null)
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
                            Players.NoFlight.Add(_PlayertoUnJail.entityId);
                            string _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", _PlayertoUnJail.playerId);
                            SQL.FastQuery(_sql);
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_PlayertoUnJail.entityId];
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                _PlayertoUnJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _PlayertoUnJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
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
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                                if (Jail_Shock)
                                {
                                    _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff buffShocked", true));
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
                            Players.NoFlight.Add(_cInfoKiller.entityId);
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                _cInfoKiller.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _cInfoKiller.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
                            }
                            string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have forgiven {PlayerName} and released them from jail.[-]";
                            _chatMessage = _chatMessage.Replace("{PlayerName}", _cInfoKiller.playerName);
                            ChatHook.ChatMessage(_cInfo, _chatMessage, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have been forgiven and released from jail by {PlayerName}.[-]";
                            _chatMessage = _chatMessage.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfoKiller, _chatMessage, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            string _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", _cInfoKiller.playerId);
                            SQL.FastQuery(_sql);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", this player is not spawned and so can not be forgiven or removed from jail.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                Zones.Forgive.Remove(_cInfo.entityId);
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < Jailed.Count; i++)
            {
                string _id = Jailed[i];
                if (Jailed.Contains(_id))
                {
                    string _sql = string.Format("SELECT jailTime, jailDate FROM Players WHERE steamid = '{0}'", _id);
                    DataTable _result = SQL.TQuery(_sql);
                    int _jailTime;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _jailTime);
                    DateTime _jailDate;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _jailDate);
                    _result.Dispose();
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
                                _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", _cInfo.playerId);
                                SQL.FastQuery(_sql);
                                EntityBedrollPositionList _position = _player.SpawnPoints;
                                if (_position.Count > 0)
                                {
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), null, false));
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), null, false));
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
                            _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                    }
                }
            }
        }

        public static void JailList()
        {
            string _sql = "SELECT steamid, jailTime, jailDate FROM Players WHERE jailTime > 0 OR jailTime = -1";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                int _jailTime;
                DateTime _jailDate;
                foreach (DataRow row in _result.Rows)
                {
                    int.TryParse(row[1].ToString(), out _jailTime);
                    if (_jailTime > 0 || _jailTime == -1)
                    {
                        if (_jailTime == -1)
                        {
                            Jailed.Add(row[0].ToString());
                            break;
                        }
                        else
                        {
                            DateTime.TryParse(row[2].ToString(), out _jailDate);
                            TimeSpan varTime = DateTime.Now - _jailDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed < _jailTime)
                            {
                                Jailed.Add(row[0].ToString());
                            }
                            else
                            {
                                _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", row[0].ToString());
                                SQL.FastQuery(_sql);
                            }
                        }
                    }
                }
            }
            _result.Dispose();
        }
    }
}