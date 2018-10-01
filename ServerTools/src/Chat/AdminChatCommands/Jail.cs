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
        private static string[] _cmd = { "jail" };
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
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase502 = "{PlayerName} you have set the jail position as {JailPosition}.";
                }
                _phrase502 = _phrase502.Replace("{PlayerName}", _cInfo.playerName);
                _phrase502 = _phrase502.Replace("{JailPosition}", Jail_Position);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase502), Config.Server_Response_Name, false, "ServerTools", false));
                Config.UpdateXml();
            }
        }

        public static void PutInJail(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                if (Jail_Position == "0,0,0" || Jail_Position == "0 0 0" || Jail_Position == "")
                {
                    string _phrase503;
                    if (!Phrases.Dict.TryGetValue(503, out _phrase503))
                    {
                        _phrase503 = "{PlayerName} the jail position has not been set.";
                    }
                    _phrase503 = _phrase503.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase503), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    _playerName = _playerName.Replace("jail ", "");
                    ClientInfo _PlayertoJail = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoJail == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                                _phrase504 = "{AdminPlayerName} player {PlayerName} is already in jail.";
                            }
                            _phrase504 = _phrase504.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase504 = _phrase504.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase504, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
            _PlayertoJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
            Jailed.Add(_PlayertoJail.playerId);
            string _sql = string.Format("UPDATE Players SET jailTime = 60, jailName = '{0}', jailDate = '{1}' WHERE steamid = '{2}'", _PlayertoJail.playerName, DateTime.Now, _PlayertoJail.playerId);
            SQL.FastQuery(_sql);
            string _phrase500;
            if (!Phrases.Dict.TryGetValue(500, out _phrase500))
            {
                _phrase500 = "{PlayerName} you have been sent to jail.";
            }
            _phrase500 = _phrase500.Replace("{PlayerName}", _PlayertoJail.playerName);
            _PlayertoJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase500), Config.Server_Response_Name, false, "ServerTools", false));
            if (Jail_Shock)
            {
                string _phrase507;
                if (!Phrases.Dict.TryGetValue(507, out _phrase507))
                {
                    _phrase507 = "{PlayerName} the jail is electrified. Do not try to leave it.";
                }
                _PlayertoJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase507), Config.Server_Response_Name, false, "ServerTools", false));
            }
            string _phrase505;
            if (!Phrases.Dict.TryGetValue(505, out _phrase505))
            {
                _phrase505 = "{AdminPlayerName} you have put {PlayerName} in jail.";
            }
            _phrase505 = _phrase505.Replace("{AdminPlayerName}", _cInfo.playerName);
            _phrase505 = _phrase505.Replace("{PlayerName}", _PlayertoJail.playerName);
            _PlayertoJail.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase505), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void RemoveFromJail(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _phrase201 = "{AdminPlayerName} player {PlayerName} was not found online.";
                    }
                    _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_PlayertoUnJail.playerId, false];
                    if (p == null)
                    {
                        string _phrase506;
                        if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                        {
                            _phrase506 = "{AdminPlayerName} player {PlayerName} is not in jail.";
                        }
                        _phrase506 = _phrase506.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase506 = _phrase506.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase506, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        if (!Jailed.Contains(_PlayertoUnJail.playerId))
                        {
                            string _phrase506;
                            if (!Phrases.Dict.TryGetValue(506, out _phrase506))
                            {
                                _phrase506 = "{AdminPlayerName} player {PlayerName} is not in jail.";
                            }
                            _phrase506 = _phrase506.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase506 = _phrase506.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase506, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                                _PlayertoUnJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _PlayertoUnJail.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), false));
                            }
                            string _phrase501;
                            if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                            {
                                _phrase501 = "{PlayerName} you have been released from jail.";
                            }
                            _phrase501 = _phrase501.Replace("{PlayerName}", _PlayertoUnJail.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase501, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(Jailed[i]);
                    if (_cInfo != null)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player.Spawned)
                        {
                            int x, y, z;
                            string[] _cords = Jail_Position.Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            Vector3 _vector3 = _player.position;
                            if ((x - _vector3.x) * (x - _vector3.x) + (z - _vector3.z) * (z - _vector3.z) >= Jail_Size * Jail_Size) 
                            {
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                if (Jail_Shock)
                                {
                                    _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff " + "shockedBuff", true));
                                    string _phrase508;
                                    if (!Phrases.Dict.TryGetValue(508, out _phrase508))
                                    {
                                        _phrase508 = "{PlayerName} don't pee on the electric fence.";
                                    }
                                    _phrase508 = _phrase508.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase508), Config.Server_Response_Name, false, "ServerTools", false));
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
            if (Players.Forgive.TryGetValue(_cInfo.entityId, out _killId))
            {
                ClientInfo _cInfoKiller = ConnectionManager.Instance.GetClientInfoForEntityId(_killId);
                if (_cInfoKiller != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this player is not online and so can not be forgiven or removed from jail.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    if (!Jailed.Contains(_cInfoKiller.playerId))
                    {
                        Players.Forgive.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this player is not in jail.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        Players.Forgive.Remove(_cInfo.entityId);
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoKiller.entityId];
                        if (_player.IsSpawned())
                        {
                            Jailed.Remove(_cInfoKiller.playerId);
                            Players.NoFlight.Add(_cInfoKiller.entityId);
                            EntityBedrollPositionList _position = _player.SpawnPoints;
                            if (_position.Count > 0)
                            {
                                _cInfoKiller.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), false));
                            }
                            else
                            {
                                Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                _cInfoKiller.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), false));
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have forgiven {2} and released them from jail.[-]", Config.Chat_Response_Color, _cInfo.playerName, _cInfoKiller.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been forgiven and released from jail by {2}.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            string _sql = string.Format("UPDATE Players SET jailTime = 0 WHERE steamid = '{0}'", _cInfoKiller.playerId);
                            SQL.FastQuery(_sql);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this player is not spawned and so can not be forgiven or removed from jail.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
            else
            {
                Players.Forgive.Remove(_cInfo.entityId);
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
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_id);
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
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, _position[0].y + 1, _position[0].z), false));
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, _pos[0].y + 1, _pos[0].z), false));
                                }
                                string _phrase501;
                                if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                                {
                                    _phrase501 = "{PlayerName} you have been released from jail.";
                                }
                                _phrase501 = _phrase501.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase501, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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