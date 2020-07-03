using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EventCommandsConsole : ConsoleCmdAbstract
    {
        public static Dictionary<string, int> Stage = new Dictionary<string, int>();
        public static Dictionary<string, List<string>> Setup = new Dictionary<string, List<string>>();

        public override string GetDescription()
        {
            return "[ServerTools] - List and coordinate events.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. event new <name>\n" +
                "  2. event check\n" +
                "  3. event extend <time>\n" +
                "  4. event stop\n" +
                "  5. event list\n" +
                "  6. event save\n" +
                "  7. event load <number>\n" +
                "  8. event delete <number>\n" +
                "  9. event remove <steamId>\n" +
                "1. Starts a new event setup with this event name.\n" +
                "2. Shows the settings and player list of the running event.\n" +
                "3. Extends the current event by the time given in minutes.\n" +
                "4. Stops a open event and sends players back to their return point. Stops a open invitation.\n" +
                "5. Shows a list of your saved events.\n" +
                "6. Saves the event setup to your list.\n" +
                "7. Load a event from the list.\n" +
                "8. Delete a event from the list.\n" +
                "9. Remove a single player from a running event, sending them back to their return point.\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Event", "Event", "event", "st-ev" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                string _steamId = _senderInfo.RemoteClientInfo.playerId;
                int _entityId = _senderInfo.RemoteClientInfo.entityId;
                if (_params[0].ToLower() == "new")
                {
                    if (Event.Open)
                    {
                        SdtdConsole.Instance.Output(string.Format("A event has already started. Event admin is {0}. The event is named {1}", Event.Admin, Event.EventName));
                        return;
                    }
                    if (_params.Count < 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or more, found {0}", _params.Count));
                        return;
                    }
                    if (_senderInfo.RemoteClientInfo.playerId == null)
                    {
                        SdtdConsole.Instance.Output("Unable to find a valid steam id to attach as the event operator. New events must be setup while in game");
                        return;
                    }
                    _params.RemoveAt(0);
                    string _name = string.Join(" ", _params.ToArray());
                    if (PersistentContainer.Instance.Players[_steamId].Events != null && PersistentContainer.Instance.Players[_steamId].Events.Count > 0)
                    {
                        List<List<string>> _events = PersistentContainer.Instance.Players[_steamId].Events;
                        for (int i = 0; i < _events.Count; i++)
                        {
                            List<string> _event = _events[i];
                            if (_name == _event[0])
                            {
                                SdtdConsole.Instance.Output("A event has already been saved to your list with this name. Setup a new event with a different name");
                                return;
                            }
                        }
                    }
                    if (Stage.ContainsKey(_steamId))
                    {
                        Stage.Remove(_steamId);
                    }
                    if (Setup.ContainsKey(_steamId))
                    {
                        Setup.Remove(_steamId);
                    }
                    Stage.Add(_steamId, 1);
                    List<string> _setup = new List<string>();
                    _setup.Add(_name);
                    Setup.Add(_steamId, _setup);
                    SdtdConsole.Instance.Output(string.Format("You have started to setup a new event. The name has been set to {0}", _name));
                    SdtdConsole.Instance.Output("What would you like players to receive for an invitation to the event? Type event <invitation>");
                    return;
                }
                else if (_params[0].ToLower() == "start")
                {
                    if (Event.Open)
                    {
                        SdtdConsole.Instance.Output("There is a event currently open");
                        return;
                    }
                    if (Event.Invited)
                    {
                        SdtdConsole.Instance.Output("There is a open invitation for an event. Wait for it to complete or stop it if you are the event organizer");
                        return;
                    }
                    if (Stage.ContainsKey(_steamId))
                    {
                        Stage.TryGetValue(_steamId, out int _stage);
                        if (_stage == 5)
                        {
                            Setup.TryGetValue(_steamId, out List<string> _setup);
                            Event.Invited = true;
                            Event.Admin = _steamId;
                            Event.EventName = _setup[0];
                            int.TryParse(_setup[2], out int _teams);
                            Event.TeamCount = _teams;
                            Event.TeamSetup = _teams;
                            int.TryParse(_setup[3], out int _players);
                            Event.PlayerCount = _players;
                            int.TryParse(_setup[4], out int _time);
                            Event.Time = _time;
                            string _msg1 = "Event: {EventName}[-]";
                            string _msg2 = _setup[1] + "[-]";
                            string _msg3 = "Type " + ChatHook.Command_Private + Event.Command100 + " if you want to join the event. You will return to where you first signed up when it completes.[-]";
                            _msg1 = _msg1.Replace("{EventName}", _setup[0]);
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg1, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg2, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _msg3, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You are not at the right setup stage. Please complete it before starting the event or load one from the list first");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("No event setup found. Start a new setup or load one from the list before proceeding");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "check")
                {
                    if (Event.Open)
                    {
                        Setup.TryGetValue(Event.Admin, out List<string> _setup);
                        SdtdConsole.Instance.Output(string.Format("Event: {0}", _setup[0]));
                        SdtdConsole.Instance.Output(string.Format("Invitation: {0}", _setup[1]));
                        SdtdConsole.Instance.Output(string.Format("Info: Teams {0}, Players {1}, Time {2} minutes.", _setup[2], _setup[3], _setup[4]));
                        SdtdConsole.Instance.Output(string.Format("Operator: {0}", Event.Admin));
                        foreach (var _player in Event.Teams)
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                            if (_cInfo != null)
                            {
                                Event.Teams.TryGetValue(_player.Key, out int _team);
                                SdtdConsole.Instance.Output(string.Format("Online player: Name {0}, Id {1}, is on team {2}.", _cInfo.playerName, _cInfo.playerId, _team));
                            }
                            else
                            {
                                Event.Teams.TryGetValue(_player.Key, out int _team);
                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_player.Key);
                                if (_pdf != null)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Offline player: Name {0}, Id {1}, is on team {2}.", _pdf.ecd.entityName, _player.Key, _team));
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is no event open");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "extend")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _time))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid time: {0}", _params[1]));
                        return;
                    }
                    if (Event.Open)
                    {
                        if (Event.Admin == _steamId)
                        {
                            int _addTime = Timers._eventTime + (_time * 60);
                            Timers._eventTime = _addTime;
                            SdtdConsole.Instance.Output(string.Format("The event time has been extended {0} minutes", _time));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You are not the event operator. Only the operator can extend the event");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is no event open");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "stop")
                {
                    if (Event.Open)
                    {
                        if (Event.Admin == _steamId)
                        {
                            foreach (var _eventPlayer in Event.Teams)
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_eventPlayer.Key);
                                if (_cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player != null && _player.IsSpawned())
                                    {
                                        string _returnPos = PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition;
                                        int _x, _y, _z;
                                        string[] _cords = _returnPos.Split(',');
                                        int.TryParse(_cords[0], out _x);
                                        int.TryParse(_cords[1], out _y);
                                        int.TryParse(_cords[2], out _z);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                        Event.Teams.Remove(_cInfo.playerId);
                                        ChatHook.ChatMessage(null, "The event has ended. Thank you for playing.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].EventOver = true;
                                        PersistentContainer.Instance.Save();
                                        Event.Teams.Remove(_cInfo.playerId);
                                    }
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[_eventPlayer.Key].EventOver = true;
                                    PersistentContainer.Instance.Save();
                                    Event.Teams.Remove(_eventPlayer.Key);
                                }
                            }
                            Event.Open = false;
                            Event.Admin = "";
                            Event.EventName = "";
                            SdtdConsole.Instance.Output("The current event has been cancelled and players have been sent back to their return points or setup for automatic return");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You are not the event operator. Only the operator can stop the event");
                            return;
                        }
                    }
                    else
                    {
                        if (Event.Invited)
                        {
                            if (Event.Admin == _steamId)
                            {
                                Event.Invited = false;
                                Event.Admin = "";
                                Event.EventName = "";
                                Event.Teams.Clear();
                                SdtdConsole.Instance.Output("The event invitation has been stopped");
                                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event invitation has been stopped", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("You are not the event operator. Only the operator can stop the invitation");
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("There is no event open");
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower() == "list")
                {
                    if (PersistentContainer.Instance.Players[_steamId].Events != null && PersistentContainer.Instance.Players[_steamId].Events.Count > 0)
                    {
                        List<List<string>> _events = PersistentContainer.Instance.Players[_steamId].Events;
                        for (int i = 0; i < _events.Count; i++)
                        {
                            List<string> _event = _events[i];
                            SdtdConsole.Instance.Output(string.Format("Id: {0}", i + 1));
                            SdtdConsole.Instance.Output(string.Format("Name: {0}", _event[0]));
                            SdtdConsole.Instance.Output(string.Format("Invitation: {0}", _event[1]));
                            SdtdConsole.Instance.Output(string.Format("Teams: {0}, Players = {1}, Alloted Time = {2}", _event[2], _event[3], _event[4]));
                            int.TryParse(_event[2], out int _teams);
                            _event.RemoveRange(0, 4);
                            for (int j = 0; j < _teams - 1; j++)
                            {
                                SdtdConsole.Instance.Output(string.Format("Spawn position {0} = {1}", j + 1, _event[j]));
                            }
                            _event.RemoveRange(0, _teams - 1);
                            for (int k = 0; k < _teams - 1; k++)
                            {
                                SdtdConsole.Instance.Output(string.Format("Respawn position {0} = {1}", k + 1, _event[k]));
                            }
                            SdtdConsole.Instance.Output("-----------------------------------------------------------------------");
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There are no saved events in your list");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "save")
                {
                    if (Stage.ContainsKey(_steamId))
                    {
                        Stage.TryGetValue(_steamId, out int _stage);
                        if (_stage == 5)
                        {
                            Setup.TryGetValue(_steamId, out List<string> _eventData);
                            if (PersistentContainer.Instance.Players[_steamId].Events != null && PersistentContainer.Instance.Players[_steamId].Events.Count > 0)
                            {
                                List<List<string>> _events = PersistentContainer.Instance.Players[_steamId].Events;
                                _events.Add(_eventData);
                                PersistentContainer.Instance.Players[_steamId].Events = _events;
                            }
                            else
                            {
                                List<List<string>> _events = new List<List<string>>();
                                _events.Add(_eventData);
                                PersistentContainer.Instance.Players[_steamId].Events = _events;
                            }
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output("The event setup has been saved to the list");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You are not at the right setup stage. Please complete the setup before starting the event or load one from the list first");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("No open setup found. Start a new setup or open one from your list before proceeding");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "load")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _id))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid id: {0}", _params[1]));
                        return;
                    }
                    if (PersistentContainer.Instance.Players[_steamId].Events != null)
                    {
                        if (PersistentContainer.Instance.Players[_steamId].Events.Count >= _id)
                        {
                            List<string> _event = PersistentContainer.Instance.Players[_steamId].Events[_id];
                            if (Stage.ContainsKey(_steamId))
                            {
                                Stage.Remove(_steamId);
                            }
                            if (Setup.ContainsKey(_steamId))
                            {
                                Setup.Remove(_steamId);
                            }
                            Stage.Add(_steamId, 5);
                            Setup.Add(_steamId, _event);
                            SdtdConsole.Instance.Output("Event has been loaded");
                            SdtdConsole.Instance.Output(string.Format("Name: {0}", _event[0]));
                            SdtdConsole.Instance.Output(string.Format("Invitation: {0}", _event[1]));
                            SdtdConsole.Instance.Output(string.Format("Teams: {0}, Players = {1}, Alloted Time = {2}", _event[2], _event[3], _event[4]));
                            int.TryParse(_event[2], out int _teams);
                            _event.RemoveRange(0, 4);
                            for (int j = 0; j < _teams - 1; j++)
                            {
                                SdtdConsole.Instance.Output(string.Format("Spawn position {0} = {1}", j + 1, _event[j]));
                            }
                            _event.RemoveRange(0, _teams - 1);
                            for (int k = 0; k < _teams - 1; k++)
                            {
                                SdtdConsole.Instance.Output(string.Format("Respawn position {0} = {1}", k + 1, _event[k]));
                            }
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Your list does not contain id: {0}", _id));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("You have no saved events on your list. Unable to load");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "delete")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _id))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid id: {0}", _params[1]));
                        return;
                    }
                    if (PersistentContainer.Instance.Players[_steamId].Events != null)
                    {
                        if (PersistentContainer.Instance.Players[_steamId].Events.Count >= _id)
                        {
                            List<List<string>> _events = PersistentContainer.Instance.Players[_steamId].Events;
                            _events.RemoveAt(_id - 1);
                            PersistentContainer.Instance.Players[_steamId].Events = _events;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Removed event id: {0}", _id));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Your list does not contain id: {0}", _id));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("You have no saved events on your list. Unable to load");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "remove")
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (Event.Open)
                    {
                        if (Event.Admin == _steamId)
                        {
                            if (Event.Teams.ContainsKey(_params[1]))
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_params[1]);
                                if (_cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player != null && _player.IsSpawned())
                                    {
                                        Event.Teams.Remove(_params[1]);
                                        string _returnPos = PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition;
                                        int _x, _y, _z;
                                        string[] _cords = _returnPos.Split(',');
                                        int.TryParse(_cords[0], out _x);
                                        int.TryParse(_cords[1], out _y);
                                        int.TryParse(_cords[2], out _z);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                        PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition = "";
                                        PersistentContainer.Instance.Save();
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have been removed from the event and sent to your return point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        SdtdConsole.Instance.Output(string.Format("Removed event player with id: {0}. They have been sent to their return point", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].EventOver = true;
                                        PersistentContainer.Instance.Save();
                                        SdtdConsole.Instance.Output(string.Format("Unable to find event player with id: {0}. They have been set to auto return", _params[1]));
                                        return;
                                    }
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[_params[1]].EventOver = true;
                                    PersistentContainer.Instance.Save();
                                    SdtdConsole.Instance.Output(string.Format("Unable to find event player with id: {0}. They have been set to auto return", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Unable to find event player with id: {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("You are not the event operator. Only the operator can stop the invitation");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("There is no event open");
                        return;
                    }
                }
                else if (_params.Count == 0)
                {
                    if (Stage.ContainsKey(_steamId))
                    {
                        Stage.TryGetValue(_steamId, out int _stage);
                        if (_stage == 1)//New setup. Name set. Setting invitation
                        {
                            _params.RemoveAt(0);
                            string _invitation = string.Join(" ", _params.ToArray());
                            Setup.TryGetValue(_steamId, out List<string> _setup);
                            _setup.Add(_invitation);
                            Setup[_steamId] = _setup;
                            Stage[_steamId] = 2;
                            SdtdConsole.Instance.Output(string.Format("The invitation has been set to {0}", _invitation));
                            SdtdConsole.Instance.Output("How many teams and players in total? Type event <teams> <players>");
                            return;
                        }
                        else if (_stage == 2)//Invite set. Setting team and player count
                        {
                            if (!int.TryParse(_params[0], out int _teams))
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid team count {0}", _params[0]));
                                return;
                            }
                            if (!int.TryParse(_params[1], out int _players))
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid player count {0}", _params[1]));
                                return;
                            }
                            if (!int.TryParse(_params[2], out int _time))
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid event time {0}", _params[1]));
                                return;
                            }
                            Setup.TryGetValue(_steamId, out List<string> _setup);
                            _setup.Add(_params[0]);
                            _setup.Add(_params[1]);
                            _setup.Add(_params[2]);
                            Setup[_steamId] = _setup;
                            Stage[_steamId] = 3;
                            SdtdConsole.Instance.Output(string.Format("The team count has been set to {0}, player count to {1}, event time to {2} minutes", _params[0], _params[1], _params[2]));
                            SdtdConsole.Instance.Output("Where would you like team 1 to spawn? Stand where you want it and type event");
                            return;
                        }
                        else if (_stage == 3)//Team count and player count set. Setting spawn points
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_entityId];
                            if (_player != null)
                            {
                                Vector3 _position = _player.GetPosition();
                                int _x = (int)_position.x;
                                int _y = (int)_position.y;
                                int _z = (int)_position.z;
                                string _sposition = _x + "," + _y + "," + _z;
                                Setup.TryGetValue(_steamId, out List<string> _setup);
                                _setup.Add(_sposition);
                                Setup[_steamId] = _setup;
                                int.TryParse(_setup[2], out int _teamCount);
                                if (_setup.Count - 4 != _teamCount)
                                {
                                    SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4, _x, _y, _z));
                                    SdtdConsole.Instance.Output(string.Format("Where would you like team {0} to spawn? Stand where you want it and type event", _setup.Count - 4 + 1));
                                    return;
                                }
                                else
                                {
                                    Stage[_steamId] = 4;
                                    SdtdConsole.Instance.Output(string.Format("The spawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4, _x, _y, _z));
                                    SdtdConsole.Instance.Output(string.Format("Where would you like team {0} to respawn? Stand where you want it and type event", _setup.Count - 4 - _teamCount + 1));
                                    return;
                                }
                            }
                        }
                        else if (_stage == 4)//Spawn points set. Setting respawn points
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_entityId];
                            if (_player != null)
                            {
                                Vector3 _position = _player.GetPosition();
                                int _x = (int)_position.x;
                                int _y = (int)_position.y;
                                int _z = (int)_position.z;
                                string _sposition = _x + "," + _y + "," + _z;
                                Setup.TryGetValue(_steamId, out List<string> _setup);
                                int.TryParse(_setup[2], out int _teamCount);
                                if (_setup.Count - 4 - _teamCount != _teamCount)
                                {
                                    SdtdConsole.Instance.Output(string.Format("The respawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4 - _teamCount, _x, _y, _z));
                                    SdtdConsole.Instance.Output(string.Format("Where would you like team {0} to respawn? Stand where you want it and type event", _setup.Count - 4 - _teamCount + 1));
                                    return;
                                }
                                else
                                {
                                    Stage[_steamId] = 5;
                                    SdtdConsole.Instance.Output("Setup is complete. You can start the event with command event start or event save to record it to the list. You can save it while it is running.");
                                    SdtdConsole.Instance.Output("This will allow you to replay the event or start it at a later date");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("No event setup found. Start a new setup before proceeding");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventCommandsConsole.Execute: {0}", e.Message));
            }
        }
    }
}
