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
                "  1. st-ev new <name>\n" +
                "  2. st-ev check\n" +
                "  3. st-ev extend <time>\n" +
                "  4. st-ev stop\n" +
                "  5. st-ev list\n" +
                "  6. st-ev save\n" +
                "  7. st-ev load <number>\n" +
                "  8. st-ev delete <number>\n" +
                "  9. st-ev remove <steamId>\n" +
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
            return new string[] { "st-Event", "ev", "st-ev" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                string _steamId = _senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier;
                int _entityId = _senderInfo.RemoteClientInfo.entityId;
                if (_params[0].ToLower() == "new")
                {
                    if (Event.Open)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] A event has already started. Event admin is {0}. The event is named {1}", Event.Operator, Event.EventName));
                        return;
                    }
                    if (_params.Count < 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or more, found {0}", _params.Count));
                        return;
                    }
                    if (_senderInfo.RemoteClientInfo.PlatformId.ReadablePlatformUserIdentifier == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Unable to find a valid steam id to attach as the event operator. New events must be setup while in game");
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
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] A event has already been saved to your list with this name. Setup a new event with a different name");
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
                    List<string> _setup = new List<string>
                    {
                        _name
                    };
                    Setup.Add(_steamId, _setup);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You have started to setup a new event. The name has been set to {0}", _name));
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] What would you like players to receive for an invitation to the event? Type st-ev <invitation>");
                    return;
                }
                else if (_params[0].ToLower() == "start")
                {
                    if (Event.Open)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is a event currently open");
                        return;
                    }
                    if (Event.Invited)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is a open invitation for an event. Wait for it to complete or stop it if you are the event organizer");
                        return;
                    }
                    if (Stage.ContainsKey(_steamId))
                    {
                        Stage.TryGetValue(_steamId, out int _stage);
                        if (_stage == 5)
                        {
                            Setup.TryGetValue(_steamId, out List<string> _setup);
                            Event.Invited = true;
                            Event.Operator = _steamId;
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
                            string _msg3 = "Type " + ChatHook.Chat_Command_Prefix1 + Event.Command_join + " if you want to join the event. You will return to where you first signed up when it completes.[-]";
                            _msg1 = _msg1.Replace("{EventName}", _setup[0]);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _msg1, -1, Config.Server_Response_Name, EChatType.Global, null);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _msg2, -1, Config.Server_Response_Name, EChatType.Global, null);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _msg3, -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not at the right setup stage. Please complete it before starting the event or load one from the list first");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No event setup found. Start a new setup or load one from the list before proceeding");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "check")
                {
                    if (Event.Open)
                    {
                        Setup.TryGetValue(Event.Operator, out List<string> _setup);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Event: {0}", _setup[0]));
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invitation: {0}", _setup[1]));
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Info: Teams {0}, Players {1}, Time {2} minutes.", _setup[2], _setup[3], _setup[4]));
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Operator: {0}", Event.Operator));
                        foreach (var _player in Event.Teams)
                        {
                            ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_player.Key);
                            if (cInfo != null)
                            {
                                Event.Teams.TryGetValue(_player.Key, out int _team);
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Online player: Name {0}, Id {1}, is on team {2}.", cInfo.playerName, cInfo.PlatformId.ReadablePlatformUserIdentifier, _team));
                            }
                            else
                            {
                                Event.Teams.TryGetValue(_player.Key, out int _team);
                                PlayerDataFile _pdf = PersistentOperations.GetPlayerDataFileFromId(_player.Key);
                                if (_pdf != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Offline player: Name {0}, Id {1}, is on team {2}.", _pdf.ecd.entityName, _player.Key, _team));
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no event open");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "extend")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int time))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid time: {0}", _params[1]));
                        return;
                    }
                    if (Event.Open)
                    {
                        if (Event.Operator == _steamId)
                        {
                            int addTime = Timers.eventTime + (time * 60);
                            Timers.eventTime = addTime;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The event time has been extended {0} minutes", time));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not the event operator. Only the operator can extend the event");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no event open");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "stop")
                {
                    if (Event.Open)
                    {
                        if (Event.Operator == _steamId)
                        {
                            foreach (var eventPlayer in Event.Teams)
                            {
                                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(eventPlayer.Key);
                                if (cInfo != null)
                                {
                                    EntityPlayer player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                    if (player != null && player.IsSpawned())
                                    {
                                        string returnPos = PersistentContainer.Instance.Players[cInfo.PlatformId.ReadablePlatformUserIdentifier].EventReturnPosition;
                                        string[] cords = returnPos.Split(',');
                                        int.TryParse(cords[0], out int x);
                                        int.TryParse(cords[1], out int y);
                                        int.TryParse(cords[2], out int z);
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                                        Event.Teams.Remove(cInfo.PlatformId.ReadablePlatformUserIdentifier);
                                        ChatHook.ChatMessage(null, "The event has ended. Thank you for playing.[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        PersistentContainer.Instance.Players[cInfo.PlatformId.ReadablePlatformUserIdentifier].EventOver = true;
                                        PersistentContainer.DataChange = true;
                                        Event.Teams.Remove(cInfo.PlatformId.ReadablePlatformUserIdentifier);
                                    }
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[eventPlayer.Key].EventOver = true;
                                    PersistentContainer.DataChange = true;
                                    Event.Teams.Remove(eventPlayer.Key);
                                }
                            }
                            Event.Open = false;
                            Event.Operator = "";
                            Event.EventName = "";
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The current event has been cancelled and players have been sent back to their return points or setup for automatic return");
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not the event operator. Only the operator can stop the event");
                            return;
                        }
                    }
                    else
                    {
                        if (Event.Invited)
                        {
                            if (Event.Operator == _steamId)
                            {
                                Event.Invited = false;
                                Event.Operator = "";
                                Event.EventName = "";
                                Event.Teams.Clear();
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The event invitation has been stopped");
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + "The event invitation has been stopped", -1, Config.Server_Response_Name, EChatType.Global, null);
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not the event operator. Only the operator can stop the invitation");
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no event open");
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id: {0}", i + 1));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Name: {0}", _event[0]));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invitation: {0}", _event[1]));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Teams: {0}, Players = {1}, Alloted Time = {2}", _event[2], _event[3], _event[4]));
                            int.TryParse(_event[2], out int _teams);
                            _event.RemoveRange(0, 4);
                            for (int j = 0; j < _teams - 1; j++)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Spawn position {0} = {1}", j + 1, _event[j]));
                            }
                            _event.RemoveRange(0, _teams - 1);
                            for (int k = 0; k < _teams - 1; k++)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Respawn position {0} = {1}", k + 1, _event[k]));
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("-----------------------------------------------------------------------");
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no saved events in your list");
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
                                PersistentContainer.DataChange = true;
                            }
                            else
                            {
                                List<List<string>> _events = new List<List<string>>
                                {
                                    _eventData
                                };
                                PersistentContainer.Instance.Players[_steamId].Events = _events;
                                PersistentContainer.DataChange = true;
                            }
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The event setup has been saved to the list");
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not at the right setup stage. Please complete the setup before starting the event or load one from the list first");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No open setup found. Start a new setup or open one from your list before proceeding");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "load")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _id))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid id: {0}", _params[1]));
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Event has been loaded");
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Name: {0}", _event[0]));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invitation: {0}", _event[1]));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Teams: {0}, Players = {1}, Alloted Time = {2}", _event[2], _event[3], _event[4]));
                            int.TryParse(_event[2], out int _teams);
                            _event.RemoveRange(0, 4);
                            for (int j = 0; j < _teams - 1; j++)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Spawn position {0} = {1}", j + 1, _event[j]));
                            }
                            _event.RemoveRange(0, _teams - 1);
                            for (int k = 0; k < _teams - 1; k++)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Respawn position {0} = {1}", k + 1, _event[k]));
                            }
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Your list does not contain id: {0}", _id));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You have no saved events on your list. Unable to load");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "delete")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _id))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid id: {0}", _params[1]));
                        return;
                    }
                    if (PersistentContainer.Instance.Players[_steamId].Events != null)
                    {
                        if (PersistentContainer.Instance.Players[_steamId].Events.Count >= _id)
                        {
                            List<List<string>> _events = PersistentContainer.Instance.Players[_steamId].Events;
                            _events.RemoveAt(_id - 1);
                            PersistentContainer.Instance.Players[_steamId].Events = _events;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed event id: {0}", _id));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Your list does not contain id: {0}", _id));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You have no saved events on your list. Unable to load");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "remove")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (Event.Open)
                    {
                        if (Event.Operator == _steamId)
                        {
                            if (Event.Teams.ContainsKey(_params[1]))
                            {
                                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                                if (cInfo != null)
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[cInfo.entityId];
                                    if (_player != null && _player.IsSpawned())
                                    {
                                        Event.Teams.Remove(_params[1]);
                                        string _returnPos = PersistentContainer.Instance.Players[cInfo.PlatformId.ReadablePlatformUserIdentifier].EventReturnPosition;
                                        string[] _cords = _returnPos.Split(',');
                                        int.TryParse(_cords[0], out int _x);
                                        int.TryParse(_cords[1], out int _y);
                                        int.TryParse(_cords[2], out int _z);
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                        PersistentContainer.Instance.Players[cInfo.PlatformId.ReadablePlatformUserIdentifier].EventReturnPosition = "";
                                        PersistentContainer.DataChange = true;
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + "You have been removed from the event and sent to your return point.[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed event player with id: {0}. They have been sent to their return point", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        PersistentContainer.Instance.Players[cInfo.PlatformId.ReadablePlatformUserIdentifier].EventOver = true;
                                        PersistentContainer.DataChange = true;
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find event player with id: {0}. They have been set to auto return", _params[1]));
                                        return;
                                    }
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[_params[1]].EventOver = true;
                                    PersistentContainer.DataChange = true;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find event player with id: {0}. They have been set to auto return", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find event player with id: {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] You are not the event operator. Only the operator can stop the invitation");
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There is no event open");
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The invitation has been set to {0}", _invitation));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] How many teams and players in total? Type st-ev <teams> <players>");
                            return;
                        }
                        else if (_stage == 2)//Invite set. Setting team and player count
                        {
                            if (!int.TryParse(_params[0], out int _teams))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid team count {0}", _params[0]));
                                return;
                            }
                            if (!int.TryParse(_params[1], out int _players))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid player count {0}", _params[1]));
                                return;
                            }
                            if (!int.TryParse(_params[2], out int _time))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid event time {0}", _params[1]));
                                return;
                            }
                            Setup.TryGetValue(_steamId, out List<string> _setup);
                            _setup.Add(_params[0]);
                            _setup.Add(_params[1]);
                            _setup.Add(_params[2]);
                            Setup[_steamId] = _setup;
                            Stage[_steamId] = 3;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The team count has been set to {0}, player count to {1}, event time to {2} minutes", _params[0], _params[1], _params[2]));
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Where would you like team 1 to spawn? Stand where you want it and type st-ev");
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The spawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4, _x, _y, _z));
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Where would you like team {0} to spawn? Stand where you want it and type st-ev", _setup.Count - 4 + 1));
                                    return;
                                }
                                else
                                {
                                    Stage[_steamId] = 4;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The spawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4, _x, _y, _z));
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Where would you like team {0} to respawn? Stand where you want it and type st-ev", _setup.Count - 4 - _teamCount + 1));
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] The respawn position for team {0} has been set to {1} {2} {3}", _setup.Count - 4 - _teamCount, _x, _y, _z));
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Where would you like team {0} to respawn? Stand where you want it and type st-ev", _setup.Count - 4 - _teamCount + 1));
                                    return;
                                }
                                else
                                {
                                    Stage[_steamId] = 5;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Setup is complete. You can start the event with command st-ev start or st-ev save to record it to the list. You can save it while it is running.");
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] This will allow you to replay the event or start it at a later date");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No event setup found. Start a new setup before proceeding");
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
