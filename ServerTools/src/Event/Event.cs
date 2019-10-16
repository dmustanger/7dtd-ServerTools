using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Event
    {
        public static bool Open = false, Invited = false, Cancel = false, Extend = false, Return = false;
        public static string Command100 = "join";
        public static Dictionary<string, int> SetupStage = new Dictionary<string, int>();
        public static Dictionary<string, int> SaveSlot = new Dictionary<string, int>();
        public static Dictionary<string, int> PlayersTeam = new Dictionary<string, int>();
        public static Dictionary<string, string> SetupName = new Dictionary<string, string>();
        public static string Admin = null, OpenEventName = "";
        private static int TeamCount = 1;

        public static void CheckOpen()
        {
            if (!Invited)
            {
                return;
            }
            if (!Open)
            {
                Invited = false;
                PlayersTeam.Clear();
                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(Admin);
                Admin = null;
                if (_cInfo != null)
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " not enough players signed up for the event. The setup has been cleared.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + "The event did not get enough players signed up to begin and has been cancelled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }

        public static void AddPlayer(ClientInfo _cInfo)
        {
            if (Invited)
            {
                if (!PlayersTeam.ContainsKey(_cInfo.playerId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    string _sposition = x + "," + y + "," + z;
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSignUpPosition = _sposition;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you have signed up for the event and your current location has been saved for return.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    string _sql = string.Format("SELECT eventid, eventName, eventTeams, eventPlayerCount, eventTime FROM Events WHERE eventAdmin = '{0}' AND eventActive = 'true'", Admin);
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
                    string _message = " you are on team {Team}.";
                    _message = _message.Replace("{Team}", TeamCount.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    if (TeamCount == _eventTeams)
                    {
                        TeamCount = 1;
                    }
                    if (PlayersTeam.Count == _eventPlayerCount)
                    {
                        Invited = false;
                        foreach (var _eventPlayer in PlayersTeam)
                        {
                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_eventPlayer.Key);
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
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + " you have been sent to your event spawn point.[-]", _cInfo2.entityId, _cInfo2.playerName, EChatType.Whisper, null);
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[_cInfo2.playerId].EventSpawn = true;
                                    PersistentContainer.Instance.Save();
                                }
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo2.playerId].EventSpawn = true;
                                PersistentContainer.Instance.Save();
                            }
                        }
                        int _eventTime = _time * 60;
                        Timers._eventTime = _eventTime;
                        Open = true;
                        Invited = false;
                        _message = "{EventName} is full and has now started.";
                        _message = _message.Replace("{EventName}", _eventName);
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else
                    {
                        _message = "{EventName} still has space for more players. Type " + ChatHook.Command_Private + Command100 + ".";
                        _message = _message.Replace("{EventName}", _eventName);
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        _message = "{Count} of {Total} have signed up.";
                        _message = _message.Replace("{Count}", PlayersTeam.Count.ToString());
                        _message = _message.Replace("{Total}", _eventPlayerCount.ToString());
                        ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you are already signed up for this event. It will start when enough players sign up.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " there is no open invitation for an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
            }
        }

        public static void HalfTime()
        {
            ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            if (_cInfo1 != null)
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _cInfo1.playerName + " the event is at half time.[-]", _cInfo1.entityId, _cInfo1.playerName, EChatType.Whisper, null);
            }
            foreach (var _player in PlayersTeam)
            {
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                if (_cInfo2 != null)
                {
                    if (_cInfo2.entityId != _cInfo1.entityId)
                    {
                        ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + " the event is at half time.[-]", _cInfo1.entityId, _cInfo1.playerName, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void FiveMin()
        {
            ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            if (_cInfo1 != null)
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _cInfo1.playerName + " the event has five minutes remaining. If you need to extend the time remaining, in the console type event extend <time>. The time is in minutes.[-]", _cInfo1.entityId, _cInfo1.playerName, EChatType.Whisper, null);
            }
            Extend = true;
            foreach (var _player in PlayersTeam)
            {
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                if (_cInfo2 != null)
                {
                    if (_cInfo2.entityId != _cInfo1.entityId)
                    {
                        ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + " the event has five minutes remaining.[-]", _cInfo1.entityId, _cInfo1.playerName, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void EndEvent()
        {
            ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.GetForPlayerName(Admin);
            if (_cInfo1 != null)
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _cInfo1.playerName + " the current event has ended and event players have been sent back to their return points.[-]", _cInfo1.entityId, _cInfo1.playerName, EChatType.Global, null);
            }
            foreach (var _player in PlayersTeam)
            {
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player.Key);
                if (_cInfo2 != null)
                {
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                    if (_player2.IsSpawned())
                    {
                        string _pos = PersistentContainer.Instance.Players[_cInfo2.playerId].EventSignUpPosition;
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        PlayersTeam.Remove(_player.Key);
                        ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + " the current event has ended and event players have been sent back to their return points.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_player.Key].EventOver = true;
                        PersistentContainer.Instance.Save();
                        PlayersTeam.Remove(_player.Key);
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[_player.Key].EventOver = true;
                    PersistentContainer.Instance.Save();
                    PlayersTeam.Remove(_player.Key);
                }
            }
            string _sql2 = string.Format("UPDATE Events SET eventActive = 'false' WHERE eventAdmin = '{0}'", Admin);
            SQL.FastQuery(_sql2, "Event");
            Open = false;
            Admin = null;
        }

        public static void EventOver(ClientInfo _cInfo)
        {
            string _pos = PersistentContainer.Instance.Players[_cInfo.playerId].EventSignUpPosition;
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
            PersistentContainer.Instance.Players[_cInfo.playerId].EventOver = false;
            PersistentContainer.Instance.Save();
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " the event ended while you were offline or not spawned. You have been sent to your return point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void EventSpawn(ClientInfo _cInfo)
        {
            if (Open)
            {
                int _team;
                if (PlayersTeam.TryGetValue(_cInfo.playerId, out _team))
                {
                    string _sql = string.Format("SELECT eventid FROM Events WHERE eventActive = 'true'");
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                    PersistentContainer.Instance.Save();
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you have been sent to your event spawn point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].EventSpawn = false;
                PersistentContainer.Instance.Save();
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " the event ended while you were offline or not spawned.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
