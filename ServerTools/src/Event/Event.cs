using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Open = false, Invited = false, Cancel = false, Extend = false, Return = false;
        public static Dictionary<string, int> SetupStage = new Dictionary<string, int>();
        public static Dictionary<string, string> SetupName = new Dictionary<string, string>();
        public static Dictionary<string, int> PlayersTeam = new Dictionary<string, int>();
        public static string Admin = null;
        private static int TeamCount = 1;

        public static void CheckOpen()
        {
            if (!Open)
            {
                Invited = false;
                PlayersTeam.Clear();
                string _sql = "UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventActive = 'true'";
                SQL.FastQuery(_sql);
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Admin);
                Admin = null;
                if (_cInfo != null)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} not enough players signed up for the event. The setup has been cleared.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}The event did not get enough players signed up to begin and has been cancelled.", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false);
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (!PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _sposition = x + "," + y + "," + z;
                string _eventReturn = SQL.EscapeString(_sposition);
                string _sql = string.Format("UPDATE Players SET eventReturn = '{0}' WHERE steamid = {1}", _eventReturn, _cInfo.playerId);
                SQL.FastQuery(_sql);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have signed up for the event and your current location has been saved for return.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                _sql = string.Format("SELECT eventid, eventName, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventAdmin = '{0}' AND eventActive = 'true'", Admin);
                DataTable _result1 = SQL.TQuery(_sql);
                int _eventid;
                int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                string _eventName = _result1.Rows[0].ItemArray.GetValue(1).ToString();
                int _eventTeams;
                int.TryParse(_result1.Rows[0].ItemArray.GetValue(2).ToString(), out _eventTeams);
                int _eventPlayerCount;
                int.TryParse(_result1.Rows[0].ItemArray.GetValue(3).ToString(), out _eventPlayerCount);
                int _time;
                int.TryParse(_result1.Rows[0].ItemArray.GetValue(4).ToString(), out _time);
                _result1.Dispose();
                PlayersTeam.Add(_cInfo.playerId, TeamCount);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are on team {2}.[-]", Config.Chat_Response_Color, _cInfo.playerName, TeamCount), Config.Server_Response_Name, false, "ServerTools", false));
                if (TeamCount == _eventTeams)
                {
                    TeamCount = 1;
                }
                if (PlayersTeam.Count == _eventPlayerCount)
                {
                    Invited = false;
                    foreach (var _eventPlayer in PlayersTeam)
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerId(_eventPlayer.Key);
                        if (_cInfo2 != null)
                        {
                            EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                            if (_player2 != null && _player2.IsAlive())
                            {
                                int _teamNumber;
                                PlayersTeam.TryGetValue(_eventPlayer.Key, out _teamNumber);
                                _sql = string.Format("SELECT eventSpawn FROM EventSpawns WHERE eventid = {0} AND eventTeam = {1}", _eventid, _teamNumber);
                                DataTable _result2 = SQL.TQuery(_sql);
                                string _spawnPos = _result2.Rows[0].ItemArray.GetValue(0).ToString();
                                _result2.Dispose();
                                int _x, _y, _z;
                                string[] _cords = _spawnPos.Split(',');
                                int.TryParse(_cords[0], out _x);
                                int.TryParse(_cords[1], out _y);
                                int.TryParse(_cords[2], out _z);
                                _cInfo2.SendPackage(new NetPackageTeleportPlayer(new Vector3(_x, _y, _z), false));
                                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been sent to your event spawn point.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                _sql = string.Format("UPDATE Players SET eventSpawn = 'true' WHERE steamid = {0}", _eventPlayer.Key);
                                SQL.FastQuery(_sql);
                            }
                        }
                        else
                        {
                            _sql = string.Format("UPDATE Players SET eventSpawn = 'true' WHERE steamid = {0}", _eventPlayer.Key);
                            SQL.FastQuery(_sql);
                        }
                    }
                    int _eventTime = _time * 60;
                    Timers._eventTime = _eventTime;
                    Open = true;
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} is full and has now started.", Config.Chat_Response_Color, _eventName), Config.Server_Response_Name, false, "ServerTools", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} still has space for more players. Type /event.", Config.Chat_Response_Color, _eventName), Config.Server_Response_Name, false, "ServerTools", false);
                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of {2} have signed up.", Config.Chat_Response_Color, PlayersTeam.Count, _eventPlayerCount), Config.Server_Response_Name, false, "ServerTools", false);
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you are already signed up for this event. It will start when enough players sign up.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Died(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int _team;
            if (PlayersTeam.TryGetValue(_cInfo.playerId, out _team))
            {
                string _sql = string.Format("SELECT eventid FROM Events WHERE eventActive = 'true'");
                DataTable _result1 = SQL.TQuery(_sql);
                int _eventid;
                int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                _result1.Dispose();
                _sql = string.Format("SELECT eventRespawn FROM EventSpawns WHERE eventid = {0} AND eventTeam = {1}", _eventid, _team);
                DataTable _result2 = SQL.TQuery(_sql);
                string _respawnPos = _result2.Rows[1].ItemArray.GetValue(0).ToString();
                _result2.Dispose();
                int _x, _y, _z;
                string[] _cords = _respawnPos.Split(',');
                int.TryParse(_cords[0], out _x);
                int.TryParse(_cords[1], out _y);
                int.TryParse(_cords[2], out _z);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_x, _y, _z), false));
            }
        }

        public static void HalfTime()
        {
            foreach (var _player in PlayersTeam)
            {
                ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_player.Key);
                if (_cInfo1 != null)
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event is at half time.[-]", Config.Chat_Response_Color, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event is at half time.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void FiveMin()
        {
            Extend = true;
            foreach (var _player in PlayersTeam)
            {
                ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_player.Key);
                if (_cInfo1 != null)
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has five minutes remaining.[-]", Config.Chat_Response_Color, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has five minutes remaining. If you need to extend the time remaining, in the console type event extend <time>. The time is in minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void EndEvent()
        {
            foreach (var _player1 in PlayersTeam)
            {
                ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_player1.Key);
                if (_cInfo1 != null)
                {
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                    if (_player2.IsSpawned())
                    {
                        string _sql = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
                        _result.Dispose();
                        _sql = string.Format("UPDATE Players SET eventReturn = 'Unknown' WHERE steamid = '{0}'", _player1.Key);
                        SQL.FastQuery(_sql);
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo1.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                        PlayersTeam.Remove(_player1.Key);
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has ended. Thank you for playing.[-]", Config.Chat_Response_Color, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    string _sql = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _player1.Key);
                    SQL.FastQuery(_sql);
                    PlayersTeam.Remove(_player1.Key);
                }
            }
            string _sql2 = string.Format("UPDATE Events SET eventActive = 'false' WHERE eventAdmin = '{0}'", Admin);
            SQL.FastQuery(_sql2);
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForPlayerName(Admin);
            if (_cInfo2 != null)
            {
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the current event has ended and event players have been sent back to their return points.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
            Open = false;
            Admin = null;
        }

        public static void EventReturn(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _pos = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
            _sql = string.Format("UPDATE Players SET eventReturn = 'Unknown', return = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
            SQL.FastQuery(_sql);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event ended while you were offline or not spawned. You have been sent to your return point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void EventSpawn(ClientInfo _cInfo)
        {
            if (Open)
            {
                int _team;
                if (PlayersTeam.TryGetValue(_cInfo.playerId, out _team))
                {
                    string _sql = string.Format("SELECT eventid FROM Events WHERE eventActive = 'true'", _cInfo.playerId);
                    DataTable _result1 = SQL.TQuery(_sql);
                    int _eventid;
                    int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                    _result1.Dispose();
                    _sql = string.Format("SELECT eventSpawn FROM EventSpawns WHERE eventid = {0} AND eventTeam = {1}", _eventid, _team);
                    DataTable _result2 = SQL.TQuery(_sql);
                    string _pos = _result2.Rows[0].ItemArray.GetValue(0).ToString();
                    _result2.Dispose();
                    int x, y, z;
                    string[] _cords = _pos.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                    _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been sent to your event spawn point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                string _sql = string.Format("UPDATE Players SET eventSpawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                SQL.FastQuery(_sql);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event ended while you were offline or not spawned.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
