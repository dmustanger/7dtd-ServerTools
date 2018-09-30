using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class EventCommandsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Start and coordinate an event.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. event new \"event name\"\n" +
                "  2. event check\n" +
                "  3. event cancel\n" +
                "  4. event list\n" +
                "  5. event remove \"SteamId\"\n" +
                "1. Starts a new event with this event name.\n" +
                "2. Shows the settings and player list of the running event.\n" +
                "3. Stops the current event and sends players back to their return points.\n" +
                "4. Shows a list of past event settings and starts a setup with them.\n" +
                "5. Remove a single player from a running event, sending them back to their return point.\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Event", "event" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                string _steamId = _senderInfo.RemoteClientInfo.playerId;
                int _entityId = _senderInfo.RemoteClientInfo.entityId;
                if (_params[0].ToLower() == ("new"))
                {
                    if (_params.Count < 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or more, found {0}.", _params.Count));
                        return;
                    }
                    if (Event.Open)
                    {
                        SdtdConsole.Instance.Output(string.Format("There is an event being run by {0}.", Event.Admin));
                    }
                    string _sql1 = string.Format("SELECT eventName FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _params[1]);
                    DataTable _result = SQL.TQuery(_sql1);
                    if (_result.Rows.Count == 0)
                    {
                        _result.Dispose();
                        string _name = SQL.EscapeString(_params[1]);
                        string _sql2 = string.Format("INSERT INTO Events (eventAdmin, eventName) VALUES ('{0}', '{1}')", _steamId, _name);
                        SQL.FastQuery(_sql2);
                        if (Event.SetupStage.ContainsKey(_steamId))
                        {
                            Event.SetupStage[_steamId] = 1;
                        }
                        else
                        {
                            Event.SetupStage.Add(_steamId, 1);
                        }
                        SdtdConsole.Instance.Output("You have started to open a new event.");
                        SdtdConsole.Instance.Output(string.Format("The event name has been set to {0}.", _params[1]));
                        SdtdConsole.Instance.Output("What would you like the invitation for players to say? Type event \"invitation\".");
                        return;
                    }
                    else
                    {
                        _result.Dispose();
                        SdtdConsole.Instance.Output(string.Format("The event name {0} already exists. Use a new name or delete the old event.", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower() == ("save"))
                {
                    if (Event.SetupStage.ContainsKey(_steamId))
                    {
                        int _stage;
                        if (Event.SetupStage.TryGetValue(_steamId, out _stage))
                        {
                            string _eventName;
                            Event.SetupName.TryGetValue(_steamId, out _eventName);
                            if (_stage == 3)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                string _sql1 = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                                DataTable _result = SQL.TQuery(_sql1);
                                int _eventid;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                                int _eventTeams;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                                int _team = _result.Rows.Count + 1;
                                string _pos = SQL.EscapeString(_sposition);
                                if (_result.Rows.Count == _eventTeams - 1)
                                {
                                    _result.Dispose();
                                    string _sql2 = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, _team, _pos);
                                    SQL.FastQuery(_sql2);
                                    Event.SetupStage[_steamId] = 4;
                                    SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", _team, x, y, z));
                                    SdtdConsole.Instance.Output("Stand where you would like the respawn for team 1 if they die during the event, then type event save.");
                                    return;
                                }
                                else
                                {
                                    _result.Dispose();
                                    string _sql2 = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, _team, _pos);
                                    SQL.FastQuery(_sql2);
                                    SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", _team, x, y, z));
                                    SdtdConsole.Instance.Output(string.Format("Stand where you would like the spawn for team {0} when the event starts and type event save.", _team));
                                    return;
                                }
                            }
                            else if (_stage == 4)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                string _sql1 = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                                DataTable _result = SQL.TQuery(_sql1);
                                int _eventid;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                                int _eventTeams;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                                int _team = _result.Rows.Count + 1;
                                string _pos = SQL.EscapeString(_sposition);
                                if (_result.Rows.Count == _eventTeams - 1)
                                {
                                    _result.Dispose();
                                    string _sql2 = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventRespawn) VALUES ({0}, {1}, '{2}')", _eventid, _team, _pos);
                                    SQL.FastQuery(_sql2);
                                    Event.SetupStage[_steamId] = 5;
                                    SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", _team, x, y, z));
                                    SdtdConsole.Instance.Output("Setup is complete. Type event start to send out the invitation to players.");
                                    return;
                                }
                                else
                                {
                                    _result.Dispose();
                                    string _sql2 = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventRespawn) VALUES ({0}, {1}, '{2}')", _eventid, _team, _pos);
                                    SQL.FastQuery(_sql2);
                                    SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", _team, x, y, z));
                                    SdtdConsole.Instance.Output(string.Format("Stand where you would like the respawn for team {0} when the event starts and type event save.", _team));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("This command is invalid at this stage of setup.");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (Event.SetupStage.ContainsKey(_steamId))
                    {
                        int _stage;
                        if (Event.SetupStage.TryGetValue(_steamId, out _stage))
                        {
                            string _eventName;
                            Event.SetupName.TryGetValue(_steamId, out _eventName);
                            if (_stage == 1)
                            {
                                string _invite = SQL.EscapeString(_params[1]);
                                string _sql = string.Format("UPDATE Events SET eventInvite = '{0}' WHERE eventAdmin = '{1}' AND eventName = '{2}'", _invite, _steamId, _eventName);
                                SQL.FastQuery(_sql);
                                Event.SetupStage[_steamId] = 2;
                                SdtdConsole.Instance.Output(string.Format("The event invitation has been set to {0}.", _params[1]));
                                SdtdConsole.Instance.Output("How many teams, total players, and time in minutes will the event last? Type event <TeamCount> <TotalPlayers> <TimeInMin>.");
                                return;
                            }
                            else if (_stage == 2)
                            {
                                int _teamCount;
                                if (int.TryParse(_params[0], out _teamCount))
                                {
                                    if (_teamCount < 1)
                                    {
                                        _teamCount = 1;
                                    }
                                    int _playerCount;
                                    if (int.TryParse(_params[1], out _playerCount))
                                    {
                                        if (_playerCount < 1)
                                        {
                                            _playerCount = 1;
                                        }
                                        int _eventTime;
                                        if (int.TryParse(_params[2], out _eventTime))
                                        {
                                            if (_eventTime < 1)
                                            {
                                                _eventTime = 1;
                                            }
                                            string _sql = string.Format("UPDATE Events SET eventTeams = {0}, eventPlayerCount = {1}, eventTime = {2} WHERE eventAdmin = '{3}' AND eventName = '{4}'", _teamCount, _playerCount, _eventTime, _steamId, _eventName);
                                            SQL.FastQuery(_sql);
                                            Event.SetupStage[_steamId] = 3;
                                            SdtdConsole.Instance.Output(string.Format("The event info has been set: team count {0}, total players {1}, event time {2}.", _teamCount, _playerCount, _eventTime));
                                            if (_teamCount == 1)
                                            {
                                                SdtdConsole.Instance.Output("Stand where you would like players to spawn when the event starts and type event save.");
                                                return;
                                            }
                                            else
                                            {
                                                SdtdConsole.Instance.Output("Stand where you would like the first team to spawn when the event starts and type event save.");
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (_stage == 5)
                            {
                                if (!Event.Open)
                                {
                                    Event.Invited = true;
                                    string _sql = string.Format("SELECT eventid, eventInvite FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                                    DataTable _result = SQL.TQuery(_sql);
                                    int _eventid;
                                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                                    string _eventInvite = _result.Rows[0].ItemArray.GetValue(1).ToString();
                                    _result.Dispose();
                                    _sql = string.Format("UPDATE Events SET eventActive = 'true' WHERE eventid = {0} AND eventAdmin = '{1}'", _eventid, _steamId);
                                    SQL.FastQuery(_sql);
                                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                                    for (int i = 0; i < _cInfoList.Count; i++)
                                    {
                                        ClientInfo _cInfo = _cInfoList[i];
                                        if (_cInfo != null)
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Event: {1}[-]", Config.Chat_Response_Color, _eventName), Config.Server_Response_Name, false, "ServerTools", false));
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _eventInvite), Config.Server_Response_Name, false, "ServerTools", false));
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} type /event if you want to join the event. You will return to where you are when it ends.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output("The event has already started.");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("You have no event being setup. Setup a new one by typing event new <EventName>.");
                        return;
                    }
                }
                if (_params[0] == ("check"))
                {
                    if (Event.Open)
                    {
                        if (Event.Admin == _steamId)
                        {
                            string _sql = string.Format("SELECT eventName, eventInvite, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventAdmin = '{0}' AND eventActive = 'true'", Event.Admin);
                            DataTable _result = SQL.TQuery(_sql);
                            string _eventName = _result.Rows[0].ItemArray.GetValue(0).ToString();
                            string _eventInvite = _result.Rows[0].ItemArray.GetValue(1).ToString();
                            int _eventTeams;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _eventTeams);
                            int _eventPlayerCount;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _eventPlayerCount);
                            int _eventTime;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(4).ToString(), out _eventTime);
                            _result.Dispose();
                            SdtdConsole.Instance.Output(string.Format("Event: {0}", _eventName));
                            SdtdConsole.Instance.Output(string.Format("Invitation: {0}", _eventInvite));
                            SdtdConsole.Instance.Output(string.Format("Info: Teams {0}, Players {1}, Time {2} minutes.", _eventTeams, _eventPlayerCount, _eventTime));
                            foreach (var _player in Event.PlayersTeam)
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_player.Key);
                                if (_cInfo != null)
                                {
                                    int _team;
                                    Event.PlayersTeam.TryGetValue(_player.Key, out _team);
                                    SdtdConsole.Instance.Output(string.Format("Player name {0}, Id {1}, is on team {2}.", _cInfo.playerName, _cInfo.playerId, _team));
                                }
                                else
                                {
                                    int _team;
                                    Event.PlayersTeam.TryGetValue(_player.Key, out _team);
                                    SdtdConsole.Instance.Output(string.Format("Offline player: Player name unknown, Id {0}, is on team {1}.", _player.Key, _team));
                                }
                            }
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is no event open.");
                        return;
                    }
                }
                if (_params[0] == ("cancel"))
                {
                    if (Event.Admin == _steamId)
                    {
                        if (Event.Open)
                        {
                            if (!Event.Cancel)
                            {
                                Event.Cancel = true;
                                SdtdConsole.Instance.Output("Are you sure you want to cancel the current event? Type event cancel again to stop the event.");
                                return;
                            }
                            else
                            {
                                foreach (var _player in Event.PlayersTeam)
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_player.Key);
                                    if (_cInfo != null)
                                    {
                                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if (_player2.IsSpawned())
                                        {
                                            string _sql1 = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _steamId);
                                            DataTable _result = SQL.TQuery(_sql1);
                                            string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                            _result.Dispose();
                                            int x, y, z;
                                            string[] _cords = _position.Split(',');
                                            int.TryParse(_cords[0], out x);
                                            int.TryParse(_cords[1], out y);
                                            int.TryParse(_cords[2], out z);
                                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                            Event.PlayersTeam.Remove(_player.Key);
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has ended. Thank you for playing.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                        else
                                        {
                                            string _sql1 = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _steamId);
                                            SQL.FastQuery(_sql1);
                                            Event.PlayersTeam.Remove(_player.Key);
                                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} was not spawned but they have been removed from the event and set to go back to their return point.", _player.Key));
                                        }
                                    }
                                    else
                                    {
                                        string _sql1 = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _steamId);
                                        SQL.FastQuery(_sql1);
                                        Event.PlayersTeam.Remove(_player.Key);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed from the event and set to go back to their return point.", _player.Key));
                                    }
                                }
                                string _sql2 = string.Format("UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventAdmin = '{0}'", Event.Admin);
                                SQL.FastQuery(_sql2);
                                Event.Open = false;
                                Event.Admin = null;
                                SdtdConsole.Instance.Output("The current event has been cancelled and event players have been sent back to their return points.");
                            }
                        }
                        else
                        {
                            Event.Invited = false;
                            Event.Admin = null;
                            if (Event.PlayersTeam.Count > 0)
                            {
                                foreach (var _player in Event.PlayersTeam)
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_player.Key);
                                    if (_cInfo != null)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the current event has been cancelled.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    Event.PlayersTeam.Remove(_player.Key);
                                }
                            }
                            SdtdConsole.Instance.Output("The current setup has been cancelled and all signed up players were removed.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("list"))
                {
                    if (!Event.Open)
                    {
                        string _sql = "SELECT eventid, eventName, eventInvite, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventActive = 'false'";
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            foreach (DataRow row in _result.Rows)
                            {
                                SdtdConsole.Instance.Output(string.Format("Event Id = {0} Event Name = {1}", row[0].ToString(), row[1].ToString()));
                                SdtdConsole.Instance.Output(string.Format("Event Invite = {0}", row[2].ToString()));
                                SdtdConsole.Instance.Output(string.Format("Team Count = {0} Allowed Players = {1} Allowed Time = {2}", row[3].ToString(), row[4].ToString(), row[5].ToString()));
                                SdtdConsole.Instance.Output("-----------------------------------------------------------------------");
                                SdtdConsole.Instance.Output("-----------------------------------------------------------------------");
                            }
                            SdtdConsole.Instance.Output("Type event start <eventid> to send the invitation to players or start a new setup by typing event new.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("There is no saved event data.");
                        }
                        _result.Dispose();
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is an event running already.");
                        return;
                    }
                }
                if (_params[0] == ("extend"))
                {
                    if (Event.Admin == _steamId)
                    {
                        if (Event.Open)
                        {
                            if (Event.Extend)
                            {
                                Event.Extend = false;
                                int _time;
                                if (int.TryParse(_params[1], out _time))
                                {
                                    int _addTime = Timers._eventTime + (_time * 60);
                                    Timers._eventTime = _addTime;
                                    SdtdConsole.Instance.Output(string.Format("The event time was extended {0} minutes.", _time));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("The event can only be extended while five minutes remain in the event. Wait for an alert before using the command."));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("There is no event open to extend in time.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("remove"))
                {
                    if (Event.Admin == _steamId)
                    {
                        if (Event.Open)
                        {
                            if (Event.PlayersTeam.ContainsKey(_params[1]))
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[1]);
                                if (_cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player.IsSpawned())
                                    {
                                        string _sql1 = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                        DataTable _result = SQL.TQuery(_sql1);
                                        string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                        _result.Dispose();
                                        int x, y, z;
                                        string[] _cords = _position.Split(',');
                                        int.TryParse(_cords[0], out x);
                                        int.TryParse(_cords[1], out y);
                                        int.TryParse(_cords[2], out z);
                                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));

                                        string _sql2 = string.Format("UPDATE Players SET eventReturn = '{0}' WHERE steamid = '{1}'", "Unknown", _cInfo.playerId);
                                        SQL.FastQuery(_sql2);
                                        Event.PlayersTeam.Remove(_params[1]);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been removed from the event and sent to your return point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was removed from the event and sent to their return point.", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        string _sql1 = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
                                        SQL.FastQuery(_sql1);
                                        Event.PlayersTeam.Remove(_params[1]);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was not spawned but they were removed from the event and set to return to their return point.", _params[1]));
                                    }
                                }
                                else
                                {
                                    string _sql1 = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _params[1]);
                                    SQL.FastQuery(_sql1);
                                    Event.PlayersTeam.Remove(_params[1]);
                                    SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed from the event and set to return to their return point.", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid Id: {0}. They are not signed up for the event.", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}.", _params[1]));
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventCommandsConsole.Run: {0}.", e));
            }
        }
    }
}
