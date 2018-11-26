using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                "  5. event load <id>\n" +
                "  6. event delete \"event name\"\n" +
                "  7. event remove <steamId>\n" +
                "1. Starts a new event setup with this event name.\n" +
                "2. Shows the settings and player list of the running event.\n" +
                "3. Stops the current event and sends players back to their return point or stop an open invitation.\n" +
                "4. Shows a list of saved events. Unique to each admin.\n" +
                "5. Loads an event from your saved list. After loading, type event start to begin the event.\n" +
                "6. Delete an event from your event list.\n" +
                "7. Remove a single player from a running event, sending them back to their return point.\n";
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
                    string _sql = string.Format("SELECT eventName FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _params[1]);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count == 0)
                    {
                        _result.Dispose();
                        string _name = SQL.EscapeString(_params[1]);
                        _sql = string.Format("INSERT INTO Events (eventAdmin, eventName) VALUES ('{0}', '{1}')", _steamId, _name);
                        SQL.FastQuery(_sql);
                        if (Event.SetupStage.ContainsKey(_steamId))
                        {
                            Event.SetupName[_steamId] = _params[1];
                            Event.SetupStage[_steamId] = 1;
                        }
                        else
                        {
                            Event.SetupName.Add(_steamId, _params[1]);
                            Event.SetupStage.Add(_steamId, 1);
                        }
                        if (Event.Open)
                        {
                            SdtdConsole.Instance.Output(string.Format("There is an event being run by {0}.", Event.Admin));
                        }
                        SdtdConsole.Instance.Output("You have started to open a new event setup.");
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
                        Event.SetupStage.TryGetValue(_steamId, out _stage);
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
                            string _sql = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                            DataTable _result1 = SQL.TQuery(_sql);
                            int _eventid;
                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                            int _eventTeams;
                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                            _result1.Dispose();
                            _sql = string.Format("SELECT eventSpawn FROM EventSpawns WHERE eventid = {0}", _eventid);
                            DataTable _result2 = SQL.TQuery(_sql);
                            int _count = _result2.Rows.Count + 1;
                            _result2.Dispose();
                            string _pos = SQL.EscapeString(_sposition);
                            if (_count == _eventTeams)
                            {
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, _count, _pos);
                                SQL.FastQuery(_sql);
                                Event.SetupStage[_steamId] = 4;
                                SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", _count, x, y, z));
                                SdtdConsole.Instance.Output("Stand where you would like the respawn for team 1 if they die during the event, then type event save.");
                                return;
                            }
                            else
                            {
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, _count, _pos);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", _count, x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Stand where you would like the spawn for team {0} when the event starts and type event save.", _count + 1));
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
                            string _sql = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                            DataTable _result1 = SQL.TQuery(_sql);
                            int _eventid;
                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                            int _eventTeams;
                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                            _result1.Dispose();
                            _sql = string.Format("SELECT eventRespawn FROM EventSpawns WHERE eventid = {0} AND eventRespawn != null", _eventid);
                            DataTable _result2 = SQL.TQuery(_sql);
                            int _count = _result2.Rows.Count + 1;
                            _result2.Dispose();
                            string _pos = SQL.EscapeString(_sposition);
                            if (_count == _eventTeams)
                            {
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventRespawn) VALUES ({0}, {1}, '{2}')", _eventid, _count, _pos);
                                SQL.FastQuery(_sql);
                                Event.SetupStage[_steamId] = 5;
                                SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", _count, x, y, z));
                                SdtdConsole.Instance.Output("Setup is complete. Type event start to send out the invitation to players.");
                                return;
                            }
                            else
                            {
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventRespawn) VALUES ({0}, {1}, '{2}')", _eventid, _count, _pos);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", _count, x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Stand where you would like the respawn for team {0} when the event starts and type event save.", _count + 1));
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
                else if (_params[0].ToLower() == ("check"))
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
                                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
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
                else if (_params[0].ToLower() == ("cancel"))
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
                                Event.SetupStage.Remove(_steamId);
                                Event.SetupName.Remove(_steamId);
                                foreach (var _player in Event.PlayersTeam)
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                                    if (_cInfo != null)
                                    {
                                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if (_player2.IsSpawned())
                                        {
                                            string _sql = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _steamId);
                                            DataTable _result = SQL.TQuery(_sql);
                                            string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                            _result.Dispose();
                                            int x, y, z;
                                            string[] _cords = _position.Split(',');
                                            int.TryParse(_cords[0], out x);
                                            int.TryParse(_cords[1], out y);
                                            int.TryParse(_cords[2], out z);
                                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                                            Event.PlayersTeam.Remove(_player.Key);
                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", the event has ended. Thank you for playing.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        else
                                        {
                                            string _sql = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _steamId);
                                            SQL.FastQuery(_sql);
                                            Event.PlayersTeam.Remove(_player.Key);
                                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} was not spawned but they have been removed from the event and set to go back to their return point.", _player.Key));
                                        }
                                    }
                                    else
                                    {
                                        string _sql = string.Format("UPDATE Players SET return = 'true' WHERE steamid = '{0}'", _steamId);
                                        SQL.FastQuery(_sql);
                                        Event.PlayersTeam.Remove(_player.Key);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed from the event and set to go back to their return point.", _player.Key));
                                    }
                                }
                                string _sql2 = string.Format("UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventAdmin = '{0}'", Event.Admin);
                                SQL.FastQuery(_sql2);
                                Event.Cancel = false;
                                Event.Open = false;
                                Event.Admin = null;
                                SdtdConsole.Instance.Output("The current event has been cancelled and event players have been sent back to their return points.");
                            }
                        }
                        else
                        {
                            if (Event.Invited)
                            {
                                Event.Invited = false;
                                Event.Admin = null;
                                if (Event.PlayersTeam.Count > 0)
                                {
                                    foreach (var _player in Event.PlayersTeam)
                                    {
                                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                                        if (_cInfo != null)
                                        {
                                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", the current event has been cancelled.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        Event.PlayersTeam.Remove(_player.Key);
                                    }
                                }
                                SdtdConsole.Instance.Output("The current setup has been cancelled and all signed up players were removed.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                else if (_params[0].ToLower() == ("list"))
                {
                    string _sql = string.Format("SELECT eventid, eventName, eventInvite, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventAdmin = '{0}'", _steamId);
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
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("You have no saved event data.");
                    }
                    _result.Dispose();
                    if (Event.Open || Event.Invited)
                    {
                        SdtdConsole.Instance.Output(string.Format("An event is running by admin {0}", Event.Admin));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("Type event load <eventid> to load that event. After loading, type event start to send the invitation to players.");
                    }
                }
                else if (_params[0].ToLower() == ("load"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        string _sql = string.Format("SELECT eventName, eventInvite, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventAdmin = '{0}' AND eventid = {1}", _steamId, _id);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
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
                            if (Event.SetupStage.ContainsKey(_steamId))
                            {
                                Event.SetupStage[_steamId] = 5;
                                Event.SetupName[_steamId] = _eventName;
                            }
                            else
                            {
                                Event.SetupStage.Add(_steamId, 5);
                                Event.SetupName.Add(_steamId, _eventName);
                            }
                            SdtdConsole.Instance.Output(string.Format("Event id: {0} has been loaded. Type event start to send the invitation out to players.", _id));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Could not find this event id: {0}", _id));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}.", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower() == ("extend"))
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
                else if (_params[0].ToLower() == ("remove"))
                {
                    if (Event.Admin == _steamId)
                    {
                        if (Event.Open)
                        {
                            if (Event.PlayersTeam.ContainsKey(_params[1]))
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_params[1]);
                                if (_cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player.IsSpawned())
                                    {
                                        string _sql = string.Format("SELECT eventReturn FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                        DataTable _result = SQL.TQuery(_sql);
                                        string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                        _result.Dispose();
                                        int x, y, z;
                                        string[] _cords = _position.Split(',');
                                        int.TryParse(_cords[0], out x);
                                        int.TryParse(_cords[1], out y);
                                        int.TryParse(_cords[2], out z);
                                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                                        _sql = string.Format("UPDATE Players SET eventReturn = 'Unknown', eventSpawn = 'false', eventRespawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                        SQL.FastQuery(_sql);
                                        Event.PlayersTeam.Remove(_params[1]);
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you have been removed from the event and sent to your return point.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was removed from the event and sent to their return point.", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        string _sql = string.Format("UPDATE Players SET return = 'true', eventSpawn = 'false', eventRespawn = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                                        SQL.FastQuery(_sql);
                                        Event.PlayersTeam.Remove(_params[1]);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was not spawned but they were removed from the event and set to return to their return point.", _params[1]));
                                    }
                                }
                                else
                                {
                                    string _sql = string.Format("UPDATE Players SET return = 'true', eventSpawn = 'false', eventRespawn = 'false' WHERE steamid = '{0}'", _params[1]);
                                    SQL.FastQuery(_sql);
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
                            SdtdConsole.Instance.Output("There is no event open.");
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower() == ("start"))
                {
                    if (!Event.Invited)
                    {
                        if (Event.SetupStage.ContainsKey(_steamId))
                        {
                            int _stage;
                            if (Event.SetupStage.TryGetValue(_steamId, out _stage))
                            {
                                string _eventName;
                                if (Event.SetupName.TryGetValue(_steamId, out _eventName))
                                {
                                    if (_stage == 5)
                                    {
                                        if (!Event.Open)
                                        {
                                            Event.Invited = true;
                                            Event.Admin = _steamId;
                                            Event.SetupStage.Remove(_steamId);
                                            Event.SetupName.Remove(_steamId);
                                            string _sql = string.Format("SELECT eventid, eventInvite FROM Events WHERE eventAdmin = '{0}' AND eventName = '{1}'", _steamId, _eventName);
                                            DataTable _result = SQL.TQuery(_sql);
                                            int _eventid;
                                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                                            string _eventInvite = _result.Rows[0].ItemArray.GetValue(1).ToString();
                                            _result.Dispose();
                                            _sql = string.Format("UPDATE Events SET eventActive = 'true' WHERE eventid = {0} AND eventAdmin = '{1}'", _eventid, _steamId);
                                            SQL.FastQuery(_sql);
                                            string _msg1 = "Event: {EventName}[-]";
                                            string _msg2 = _eventInvite;
                                            string _msg3 = "Type /event if you want to join the event. You will return to where you are when it ends.[-]";
                                            _msg1 = _msg1.Replace("{EventName}", _eventName);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg1, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg2, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg3, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output("The event has already started.");
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
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is an event invitation open already.");
                        return;
                    }
                }
                else if (_params[0].ToLower() == ("delete") || _params[0].ToLower() == ("del"))
                {
                    int _id;
                    if (int.TryParse(_params[1], out _id))
                    {
                        string _sql = string.Format("SELECT eventid FROM Events WHERE eventAdmin = '{0}'", _steamId);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            _sql = string.Format("Delete FROM Events WHERE eventid = {0}", _id);
                            SQL.FastQuery(_sql);
                            _sql = string.Format("Delete FROM EventSpawns WHERE eventid = {0}", _id);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("Deleted the event with id: {0}.", _id));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}. This id is not attached to you. Can not delete it.", _params[1]));
                        }
                        _result.Dispose();
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}.", _params[1]));
                        return;
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
                            if (Event.SetupName.TryGetValue(_steamId, out _eventName))
                            {
                                if (_stage == 1)
                                {
                                    string _invite = SQL.EscapeString(_params[0]);
                                    string _sql = string.Format("UPDATE Events SET eventInvite = '{0}' WHERE eventAdmin = '{1}' AND eventName = '{2}'", _invite, _steamId, _eventName);
                                    SQL.FastQuery(_sql);
                                    Event.SetupStage[_steamId] = 2;
                                    SdtdConsole.Instance.Output(string.Format("The event invitation has been set to {0}.", _params[0]));
                                    SdtdConsole.Instance.Output("How many teams, total players, and time in minutes will the event last? Type event <TeamCount> <TotalPlayers> <TimeInMin>.");
                                    return;
                                }
                                else if (_stage == 2)
                                {
                                    if (_params.Count == 3)
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
                                                        SdtdConsole.Instance.Output("Stand where you would like the team 1 to spawn when the event starts and type event save.");
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}.", _params.Count));
                                        return;
                                    }
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventCommandsConsole.Run: {0}.", e));
            }
        }
    }
}
