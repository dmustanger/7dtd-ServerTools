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
                "  1. event new" +
                "  2. event check" +
                "  3. event cancel" +
                "  4. event list" +
                "  5. event remove <Id>" +
                "1. Starts a new event. No other admin can be running an event at the same time\n" +
                "2. Shows the settings and player list of the running event\n" +
                "3. Stops the current event and sends players back to their return points\n" +
                "4. Shows a list of past event settings and starts a setup with them.\n" +
                "5. Remove a single player from a running event, sending them back to their return point.\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Event", "event", "ev" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (!Event.Setup)
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                }
                if (_params[0] == ("new"))
                {
                    if (Event.Open || Event.Setup)
                    {
                        SdtdConsole.Instance.Output(string.Format("There is an event open or being setup already. It is being run by {0}.", Event.Admin));
                        return;
                    }
                    else
                    {
                        Event.Name = false;
                        Event.Invite = false;
                        Event.Info = false;
                        Event.Spawn = false;
                        Event.Respawn = false;
                        Event.Complete = false;
                        Event.Invited = false;
                        Event.Setup = true;
                        Event.PlayersTeam.Clear();
                        Event.Players.Clear();
                        Event.PlayersReturn.Clear();
                        Event.SpawnList.Clear();
                        Event.Spawning.Clear();
                        Event.Respawning.Clear();
                        Event.Admin = _senderInfo.RemoteClientInfo.playerName;
                        string _sql = string.Format("INSERT INTO Events (eventAdmin) VALUES ('{0}')", Event.Admin);
                        SQL.FastQuery(_sql);
                        SdtdConsole.Instance.Output("You have started to open a new event. You must complete the setup within 15 minutes.");
                        SdtdConsole.Instance.Output("What would you like to name your new event? Type event name {Name}.");
                        return;
                    }
                }
                if (_params[0] == ("name"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (!Event.Name && !Event.Invite && !Event.Info && !Event.Spawn && !Event.Respawn)
                        {
                            Event.Name = true;
                            string _name = string.Join(" ", _params.ToArray());
                            _name = _name.Replace("event name ", "");
                            string _sql = string.Format("UPDATE Events SET eventName = '{0}' WHERE eventAdmin = '{1}'", _name, Event.Admin);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("The event name has been set to {0}.", _name));
                            SdtdConsole.Instance.Output("What would you like the invitation for players to say? Type event invite {Invitation}.");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You have already set the event name. If you have made a mistake, type event cancel and start again.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("invite"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Name && !Event.Invite && !Event.Info && !Event.Spawn && !Event.Respawn)
                        {
                            Event.Invite = true;
                            string _invite = string.Join(" ", _params.ToArray());
                            _invite = _invite.Replace("event invite ", "");
                            string _sql = string.Format("UPDATE Events SET eventInvite = '{0}' WHERE eventAdmin = '{1}'", _invite, Event.Admin);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("The event invitation has been set to {0}.", _invite));
                            SdtdConsole.Instance.Output("How many teams, total players, and time in minutes will the event last? Type event info <TeamCount> <TotalPlayers> <TimeInMin>.");
                            return;
                        }
                        else if (Event.Invite)
                        {
                            SdtdConsole.Instance.Output("You have already set the invitation. If you have made a mistake, type event cancel and start again.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("info"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Name && Event.Invite && !Event.Info && !Event.Spawn && !Event.Respawn)
                        {
                            int _teamCount;
                            if (int.TryParse(_params[1], out _teamCount))
                            {
                                if (_teamCount < 1)
                                {
                                    _teamCount = 1;
                                }
                                int _playerCount;
                                if (int.TryParse(_params[2], out _playerCount))
                                {
                                    if (_playerCount < 1)
                                    {
                                        _playerCount = 1;
                                    }
                                    int _eventTime;
                                    if (int.TryParse(_params[3], out _eventTime))
                                    {
                                        if (_eventTime < 1)
                                        {
                                            _eventTime = 1;
                                        }
                                        Event.Info = true;
                                        string _sql = string.Format("UPDATE Events SET eventTeams = {0}, eventPlayerCount = {1}, eventTime = {2} WHERE eventAdmin = '{1}'", _teamCount, _playerCount, _eventTime, Event.Admin);
                                        SQL.FastQuery(_sql);
                                        SdtdConsole.Instance.Output(string.Format("The event info has been set: team count {0}, total players {1}, event time {2}.", _teamCount, _playerCount, _eventTime));
                                        if (_teamCount == 1)
                                        {
                                            SdtdConsole.Instance.Output("Stand where you would like players to spawn when the event starts and type event spawn.");
                                            return;
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output("Stand where you would like the first team to spawn when the event starts and type event spawn.");
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else if (Event.Info)
                        {
                            SdtdConsole.Instance.Output("You have already setup the team count, total players and event time. If you have made a mistake, type event cancel and start again.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("spawn"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Name && Event.Invite && Event.Info && !Event.Spawn && !Event.Respawn)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _sposition = x + "," + y + "," + z;
                            Event.Spawning.Add(Event.Spawning.Count, _sposition);
                            string _sql = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}'", Event.Admin);
                            DataTable _result = SQL.TQuery(_sql);
                            int _eventid;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                            int _eventTeams;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                            _result.Dispose();
                            if (Event.Spawning.Count == _eventTeams)
                            {
                                Event.Spawn = true;
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, Event.Spawning.Count, _sposition);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", Event.Spawning.Count, x, y, z));
                                SdtdConsole.Instance.Output("Stand where you would like the respawn for team 1 if they die during the event, then type event respawn.");
                                return;
                            }
                            else
                            {
                                _sql = string.Format("INSERT INTO EventSpawns (eventid, eventTeam, eventSpawn) VALUES ({0}, {1}, '{2}')", _eventid, Event.Spawning.Count, _sposition);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}.", Event.Spawning.Count, x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Stand where you would like the spawn for team {0} when the event starts and type event spawn.", Event.Spawning.Count + 1));
                                return;
                            }
                        }
                        else if (Event.Spawn)
                        {
                            SdtdConsole.Instance.Output("You have already setup the spawn points. If you have made a mistake, type event cancel and start again.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("respawn"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Name && Event.Invite && Event.Info && Event.Spawn && !Event.Respawn)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _sposition = x + "," + y + "," + z;
                            Event.Respawning.Add(Event.Respawning.Count, _sposition);
                            string _sql = string.Format("SELECT eventid, eventTeams FROM Events WHERE eventAdmin = '{0}'", Event.Admin);
                            DataTable _result = SQL.TQuery(_sql);
                            int _eventid;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                            int _eventTeams;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _eventTeams);
                            _result.Dispose();
                            if (Event.Respawning.Count == _eventTeams)
                            {
                                Event.Respawn = true;
                                Event.Complete = true;
                                _sql = string.Format("UPDATE EventSpawns SET eventRespawn = '{0}' WHERE eventid = {1} AND eventTeam = {2}", _sposition, _eventid, Event.Respawning.Count);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", Event.Respawning.Count, x, y, z));
                                SdtdConsole.Instance.Output("Setup is complete. Type event start to send out the invitation.");
                                return;
                            }
                            else
                            {
                                _sql = string.Format("UPDATE EventSpawns SET eventRespawn = '{0}' WHERE eventid = {1} AND eventTeam = {2}", _sposition, _eventid, Event.Respawning.Count);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}.", Event.Respawning.Count, x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Stand where you would like the respawn for team {0} if they die during the event and type event respawn.", Event.Respawning.Count + 1));
                                return;
                            }
                        }
                        else if (Event.Respawn)
                        {
                            SdtdConsole.Instance.Output("You have already setup the respawn points. If you have made a mistake, type event cancel and start again.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("start"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Complete)
                        {
                            if (!Event.Open)
                            {
                                Event.Setup = false;
                                Event.Invited = true;
                                string _sql = string.Format("SELECT eventid, eventName, eventInvite FROM Events WHERE eventAdmin = '{0}'", Event.Admin);
                                DataTable _result = SQL.TQuery(_sql);
                                int _eventid;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _eventid);
                                string _eventName = _result.Rows[0].ItemArray.GetValue(1).ToString();
                                string _eventInvite = _result.Rows[0].ItemArray.GetValue(2).ToString();
                                _result.Dispose();
                                _sql = string.Format("UPDATE Events SET eventActive = 'true' WHERE eventid = {0} AND eventAdmin = '{1}'", _eventid, Event.Admin);
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
                        else
                        {
                            SdtdConsole.Instance.Output("You have not completed the setup. Finish each step before starting.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("check"))
                {
                    if (Event.Open)
                    {
                        if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
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
                            SdtdConsole.Instance.Output(string.Format("Event: {0}.", _eventName));
                            SdtdConsole.Instance.Output(string.Format("Invitation: {0}.", _eventInvite));
                            SdtdConsole.Instance.Output(string.Format("Info: Teams {0}, Players {1}, Time {2} minutes.", _eventTeams, _eventPlayerCount, _eventTime));
                            for (int i = 0; i < Event.Players.Count; i++)
                            {
                                int _playerEntId = Event.Players[i];
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                                if (_cInfo != null)
                                {
                                    int _team;
                                    Event.PlayersTeam.TryGetValue(_playerEntId, out _team);
                                    SdtdConsole.Instance.Output(string.Format("Player name {0}, Id {1}, is on team {2}.", _cInfo.playerName, _cInfo.entityId, _team));
                                }
                                else
                                {
                                    int _team;
                                    Event.PlayersTeam.TryGetValue(_playerEntId, out _team);
                                    SdtdConsole.Instance.Output(string.Format("Offline player: Player name unknown, Id {0}, is on team {1}.", _playerEntId, _team));
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
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
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
                                for (int i = 0; i < Event.Players.Count; i++)
                                {
                                    int _playerEntId = Event.Players[i];
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                                    if (_cInfo != null)
                                    {
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if (_player.IsSpawned())
                                        {
                                            string _position;
                                            Event.PlayersReturn.TryGetValue(_playerEntId, out _position);
                                            int x, y, z;
                                            string[] _cords = _position.Split(',');
                                            int.TryParse(_cords[0], out x);
                                            int.TryParse(_cords[1], out y);
                                            int.TryParse(_cords[2], out z);
                                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                            Event.PlayersReturn.Remove(_playerEntId);
                                            Event.Players.Remove(_playerEntId);
                                            Event.SpawnList.Remove(_playerEntId);
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has ended. Thank you for playing.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                    else
                                    {
                                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_playerEntId);
                                        string _steamId = _persistentPlayerData.PlayerId;
                                        string _pos;
                                        Event.PlayersReturn.TryGetValue(_playerEntId, out _pos);
                                        string _sql = string.Format("UPDATE Players SET eventReturn = '{0}' WHERE steamid = '{1}'", _pos, _steamId);
                                        SQL.FastQuery(_sql);
                                        Event.Players.Remove(_playerEntId);
                                        Event.PlayersTeam.Remove(_playerEntId);
                                        Event.PlayersReturn.Remove(_playerEntId);
                                        Event.SpawnList.Remove(_playerEntId);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed and set for their return point.", _params[1]));
                                    }
                                }
                                if (Event.PlayersReturn.Count == 0)
                                {
                                    string _sql = string.Format("UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventAdmin = '{0}'", Event.Admin);
                                    SQL.FastQuery(_sql);
                                    Event.Return = false;
                                    Event.Open = false;
                                    Event.PlayersTeam.Clear();
                                    Event.Players.Clear();
                                    Event.PlayersReturn.Clear();
                                    Event.SpawnList.Clear();
                                    Event.Spawning.Clear();
                                    Event.Respawning.Clear();
                                    Event.Admin = null;
                                    SdtdConsole.Instance.Output("The current event has been cancelled and event players have been sent back to their return points.");
                                    return;
                                }
                                else
                                {
                                    Event.Return = true;
                                    SdtdConsole.Instance.Output("The current event has been cancelled and event players have been sent back to their return points.");
                                    SdtdConsole.Instance.Output("Not all players were spawned. Let them respawn and type event return.");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Event.Setup = false;
                            Event.Name = false;
                            Event.Invite = false;
                            Event.Info = false;
                            Event.Spawn = false;
                            Event.Respawn = false;
                            Event.Complete = false;
                            Event.Invited = false;
                            Event.Admin = null;
                            Event.PlayersTeam.Clear();
                            Event.Players.Clear();
                            Event.PlayersReturn.Clear();
                            Event.SpawnList.Clear();
                            Event.Spawning.Clear();
                            Event.Respawning.Clear();
                            string _sql = string.Format("UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventAdmin = '{0}'", Event.Admin);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output("The current setup has been cancelled.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You are not the organizer for this event. Contact {0}.", Event.Admin));
                        return;
                    }
                }
                if (_params[0] == ("return"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Return)
                        {
                            for (int i = 0; i < Event.Players.Count; i++)
                            {
                                int _playerEntId = Event.Players[i];
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_playerEntId);
                                if (_cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player.IsSpawned())
                                    {
                                        string _position;
                                        Event.PlayersReturn.TryGetValue(_playerEntId, out _position);
                                        int x, y, z;
                                        string[] _cords = _position.Split(',');
                                        int.TryParse(_cords[0], out x);
                                        int.TryParse(_cords[1], out y);
                                        int.TryParse(_cords[2], out z);
                                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                        Event.PlayersReturn.Remove(_playerEntId);
                                        Event.Players.Remove(_playerEntId);
                                        Event.SpawnList.Remove(_playerEntId);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the event has ended. Thank you for playing.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                else
                                {
                                    PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_playerEntId);
                                    string _steamId = _persistentPlayerData.PlayerId;
                                    string _pos;
                                    Event.PlayersReturn.TryGetValue(_playerEntId, out _pos);
                                    string _sql = string.Format("UPDATE Players SET eventReturn = '{0}' WHERE steamid = '{1}'", _pos, _steamId);
                                    SQL.FastQuery(_sql);
                                    Event.Players.Remove(_playerEntId);
                                    Event.PlayersTeam.Remove(_playerEntId);
                                    Event.PlayersReturn.Remove(_playerEntId);
                                    Event.SpawnList.Remove(_playerEntId);
                                    SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed and set for their return point.", _params[1]));
                                }
                            }
                            if (Event.Players.Count == 0)
                            {
                                Event.Return = false;
                                Event.Open = false;
                                Event.PlayersTeam.Clear();
                                Event.Players.Clear();
                                Event.SpawnList.Clear();
                                Event.Admin = null;
                                string _sql = string.Format("UPDATE Events SET eventAdmin = null, eventActive = 'false' WHERE eventAdmin = '{0}'", Event.Admin);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output("All players have been sent to their return point and the event has been cleaned up.");
                                return;
                            }
                            else
                            {
                                for (int i = 0; i < Event.Players.Count; i++)
                                {
                                    int _playerEntId = Event.Players[i];
                                    SdtdConsole.Instance.Output("Player with entity Id {0} is still in the event.");
                                }
                                SdtdConsole.Instance.Output("Type event return after they respawn or kick them and run event cancel.");
                                return;
                            }
                        }
                    }
                }
                if (_params[0] == ("list"))
                {
                    if (!Event.Open)
                    {
                        if (!Event.Setup)
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
                            SdtdConsole.Instance.Output("There is an event already being setup. Cancel it before using event last.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is an event running already.");
                        return;
                    }
                }
                if (_params[0] == ("extend"))
                {
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
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
                    if (Event.Admin == _senderInfo.RemoteClientInfo.playerName)
                    {
                        if (Event.Open)
                        {
                            int _id;
                            if (int.TryParse(_params[1], out _id))
                            {
                                if (Event.Players.Contains(_id))
                                {
                                    ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                                    if (_cInfo != null)
                                    {
                                        string _pos;
                                        Event.PlayersReturn.TryGetValue(_cInfo.entityId, out _pos);
                                        int x, y, z;
                                        string[] _cords = _pos.Split(',');
                                        int.TryParse(_cords[0], out x);
                                        int.TryParse(_cords[1], out y);
                                        int.TryParse(_cords[2], out z);
                                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                        Event.Players.Remove(_id);
                                        Event.PlayersTeam.Remove(_id);
                                        Event.PlayersReturn.Remove(_id);
                                        Event.SpawnList.Remove(_id);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have been removed from the event and sent to your return point.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was removed from the event and sent to their return point.", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_id);
                                        string _steamId = _persistentPlayerData.PlayerId;
                                        string _pos;
                                        Event.PlayersReturn.TryGetValue(_id, out _pos);
                                        string _sql = string.Format("UPDATE Players SET eventReturn = '{0}' WHERE steamid = '{1}'", _pos, _steamId);
                                        SQL.FastQuery(_sql);
                                        Event.Players.Remove(_id);
                                        Event.PlayersTeam.Remove(_id);
                                        Event.PlayersReturn.Remove(_id);
                                        Event.SpawnList.Remove(_id);
                                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} was offline but they have been removed and set for their return point.", _params[1]));
                                        return;
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Invalid Id: {0}.", _params[1]));
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventCommandsConsole.Run: {0}.", e));
            }
        }
    }
}
