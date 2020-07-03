using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Open = false, Invited = false, Cancel = false, Extend = false, Return = false;
        public static string Command100 = "join";
        public static Dictionary<string, int> Teams = new Dictionary<string, int>();
        public static string Admin = null, EventName = "";
        public static int TeamCount = 0, PlayerCount = 0, TeamSetup = 0, Time = 0;

        public static void CheckOpen()
        {
            if (!Open)
            {
                Invited = false;
                Teams.Clear();
                EventName = "";
                TeamCount = 0;
                PlayerCount = 0;
                TeamSetup = 0;
                Time = 0;
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Admin);
                Admin = "";
                if (_cInfo != null)
                {
                    ChatHook.ChatMessage(_cInfo, "Not enough players signed up for the event. The invitation and setup has been cleared.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event did not have enough players join. It has been cancelled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (!Teams.ContainsKey(_cInfo.playerId))
            {
                if (Teams.Count >= PlayerCount)
                {
                    ChatHook.ChatMessage(_cInfo, "The event is full. Unable to join.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition = _sposition;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, "You have signed up for the event and your current location has been saved for return.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                EventCommandsConsole.Setup.TryGetValue(Event.Admin, out List<string> _setup);
                Teams.Add(_cInfo.playerId, TeamSetup);
                string _message = "You are on team {Team}.";
                _message = _message.Replace("{Team}", TeamSetup.ToString());
                ChatHook.ChatMessage(_cInfo, _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                TeamSetup--;
                if (TeamSetup == 0)
                {
                    TeamSetup = TeamCount;
                }
                if (Teams.Count == PlayerCount)
                {
                    Invited = false;
                    foreach (var _teamPlayer in Teams)
                    {
                        ClientInfo _eventClientInfo = ConnectionManager.Instance.Clients.ForPlayerId(_teamPlayer.Key);
                        if (_eventClientInfo != null)
                        {
                            EntityPlayer _eventPlayer = GameManager.Instance.World.Players.dict[_eventClientInfo.entityId];
                            if (_eventPlayer != null && _eventPlayer.IsAlive())
                            {
                                Teams.TryGetValue(_teamPlayer.Key, out int _teamNumber);
                                string _spawnPosition = _setup[_teamNumber + 4];
                                int _x, _y, _z;
                                string[] _cords = _spawnPosition.Split(',');
                                int.TryParse(_cords[0], out _x);
                                int.TryParse(_cords[1], out _y);
                                int.TryParse(_cords[2], out _z);
                                _eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                ChatHook.ChatMessage(_eventClientInfo, LoadConfig.Chat_Response_Color + "You have been sent to your event spawn point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventSpawn = true;
                                PersistentContainer.Instance.Save();
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventSpawn = true;
                            PersistentContainer.Instance.Save();
                        }
                    }
                    int _eventTime = Time * 60;
                    Timers._eventTime = _eventTime;
                    Open = true;
                    _message = "{EventName} has now started.";
                    _message = _message.Replace("{EventName}", EventName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    _message = "{EventName} still has space for more players. Type " + ChatHook.Command_Private + Command100 + ".";
                    _message = _message.Replace("{EventName}", EventName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    _message = "{Count} of {PlayerTotal} have joined the event.";
                    _message = _message.Replace("{Count}", Teams.Count.ToString());
                    _message = _message.Replace("{PlayerTotal}", PlayerCount.ToString());
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already joined this event. It will start when enough players sign up.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void HalfTime()
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            if (_cInfo != null)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The event is at half time.[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
            }
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event is at half time.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void FiveMin()
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            if (_cInfo != null)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The event has five minutes remaining. If you need to extend the time remaining, use the console to type event extend <time>. The time is in minutes.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event has five minutes remaining.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void EndEvent()
        {
            foreach (var _eventPlayer in Teams)
            {
                ClientInfo _eventClientInfo = ConnectionManager.Instance.Clients.ForPlayerId(_eventPlayer.Key);
                if (_eventClientInfo != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_eventClientInfo.entityId];
                    if (_player.IsSpawned())
                    {
                        string _returnPosition = PersistentContainer.Instance.Players[_eventClientInfo.playerId].EventReturnPosition;
                        string[] _cords = _returnPosition.Split(',');
                        int.TryParse(_cords[0], out int _x);
                        int.TryParse(_cords[1], out int _y);
                        int.TryParse(_cords[2], out int _z);
                        _eventClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                        Teams.Remove(_eventPlayer.Key);
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[_eventPlayer.Key].EventOver = true;
                    Teams.Remove(_eventPlayer.Key);
                }
            }
            PersistentContainer.Instance.Save();
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            Open = false;
            Teams.Clear();
            Admin = "";
            EventName = "";
            TeamCount = 0;
            PlayerCount = 0;
            TeamSetup = 0;
            Time = 0;
            if (_cInfo != null)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The current event has ended and event players have been sent back to their return positions.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event has ended and all players have been sent to their return positions.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        public static void EventOver(ClientInfo _cInfo)
        {
            string _returnPosition = PersistentContainer.Instance.Players[_cInfo.playerId].EventReturnPosition;
            string[] _cords = _returnPosition.Split(',');
            int.TryParse(_cords[0], out int _x);
            int.TryParse(_cords[1], out int _y);
            int.TryParse(_cords[2], out int _z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The event ended while you were offline or not spawned. You have been sent to your return position.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Spawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.playerId, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Event.Admin, out List<string> _setup))
                {
                    string _spawnPosition = _setup[_team + 4];
                    string[] _cords = _spawnPosition.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have been sent to your event spawn point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                PersistentContainer.Instance.Save();
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The event ended while you were offline or not spawned.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Respawn(ClientInfo _cInfo)
        {
            if (Teams.TryGetValue(_cInfo.playerId, out int _team))
            {
                if (EventCommandsConsole.Setup.TryGetValue(Event.Admin, out List<string> _setup))
                {
                    _setup.RemoveRange(0, 4 + _team);
                    string _respawnPosition = _setup[_team - 1];
                    string[] _cords = _respawnPosition.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                }
            }
        }
    }
}
