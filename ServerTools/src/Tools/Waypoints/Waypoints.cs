using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Waypoints
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false, Vehicle = false;
        public static int Delay_Between_Uses = 0, Max_Waypoints = 2, Reserved_Max_Waypoints = 4, Command_Cost = 0;
        public static string Command10 = "go way", Command106 = "waypoint", Command107 = "way", Command108 = "wp", Command109 = "fwaypoint", Command110 = "fway", Command111 = "fwp", 
            Command112 = "waypoint save", Command113 = "way save", Command114 = "ws", Command115 = "waypoint del", Command116 = "way del", Command117 = "wd";
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void List(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count > 0)
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        ListResult(_cInfo, Reserved_Max_Waypoints);
                        return;
                    }
                }
                ListResult(_cInfo, Max_Waypoints);
            }
            else
            {
                Phrases.Dict.TryGetValue(279, out string _phrase279);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase279 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ListResult(ClientInfo _cInfo, int _waypointLimit)
        {
            Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
            int _count = 1;
            foreach (var _waypoint in _waypoints)
            {
                if (_count <= _waypointLimit)
                {
                    string[] _cords = _waypoint.Value.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    _count++;
                    Phrases.Dict.TryGetValue(282, out string _phrase282);
                    _phrase282 = _phrase282.Replace("{Name}", _waypoint.Key);
                    _phrase282 = _phrase282.Replace("{Value}", _x.ToString());
                    _phrase282 = _phrase282.Replace("{Value2}", _y.ToString());
                    _phrase282 = _phrase282.Replace("{Value3}", _z.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase282 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void TeleDelay(ClientInfo _cInfo, string _waypoint, bool _friends)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                if (Delay_Between_Uses < 1)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count > 0)
                    {
                        Checks(_cInfo, _waypoint, _friends);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(279, out string _phrase279);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase279 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint != null)
                    {
                        DateTime _lastWaypoint = PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint;
                        TimeSpan varTime = DateTime.Now - _lastWaypoint;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    int _delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, _waypoint, _timepassed, _delay, _friends);
                                    return;
                                }
                            }
                        }
                        Time(_cInfo, _waypoint, _timepassed, Delay_Between_Uses, _friends);
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count > 0)
                        {
                            Checks(_cInfo, _waypoint, false);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(279, out string _phrase279);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase279 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(283, out string _phrase283);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase283 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Time(ClientInfo _cInfo, string _waypoint, int _timepassed, int _delay, bool _friends)
        {
            if (_timepassed >= _delay)
            {
                Checks(_cInfo, _waypoint, _friends);
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(271, out string _phrase271);
                _phrase271 = _phrase271.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase271 = _phrase271.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase271 = _phrase271.Replace("{Value}", _timeleft.ToString());
                _phrase271 = _phrase271.Replace("{Command106}", Command106);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase271 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void Checks(ClientInfo _cInfo, string _waypoint, bool _friends)
        {
            EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
            if (_player != null)
            {
                if (Vehicle)
                {
                    Entity _attachedEntity = _player.AttachedToEntity;
                    if (_attachedEntity != null)
                    {
                        Phrases.Dict.TryGetValue(853, out string _phrase853);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase853 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                if (PvP_Check)
                {
                    if (Teleportation.PCheck(_cInfo, _player))
                    {
                        return;
                    }
                }
                if (Zombie_Check)
                {
                    if (Teleportation.ZCheck(_cInfo, _player))
                    {
                        return;
                    }
                }
                Vector3 _position = _player.GetPosition();
                int _x = (int)_position.x;
                int _y = (int)_position.y;
                int _z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(_x, _y, _z);
                CommandCost(_cInfo, _waypoint, _position, _friends);
            }
        }       

        private static void CommandCost(ClientInfo _cInfo, string _waypoint, Vector3 _position, bool _friends)
        {
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    Exec(_cInfo, _waypoint, _position, _friends);
                }
                else
                {
                    Phrases.Dict.TryGetValue(284, out string _phrase284);
                    _phrase284 = _phrase284.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase284 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Exec(_cInfo, _waypoint, _position, _friends);
            }
        }

        private static void Exec(ClientInfo _cInfo, string _waypoint, Vector3 _position, bool _friends)
        {
            if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint))
            {
                Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                _waypoints.TryGetValue(_waypoint, out string _waypointPos);
                string[] _cords = _waypointPos.Split(',');
                int.TryParse(_cords[0], out int _x);
                int.TryParse(_cords[1], out int _y);
                int.TryParse(_cords[2], out int _z);
                if (PersistentOperations.ClaimedByNone(_cInfo.playerId, new Vector3i(_x, _y, _z)))
                {
                    Phrases.Dict.TryGetValue(272, out string _phrase272);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase272 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (_friends)
                {
                    FriendInvite(_cInfo, _position, _waypointPos);
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint = DateTime.Now;
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(274, out string _phrase274);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase274 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.Teams.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Vector3 _position = _player.GetPosition();
                    if (PersistentOperations.ClaimedByNone(_cInfo.playerId, new Vector3i(_position.x, _position.y, _position.z)))
                    {
                        ReservedCheck(_cInfo, _waypoint);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(280, out string _phrase280);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase280 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(283, out string _phrase283);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase283 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void ReservedCheck(ClientInfo _cInfo, string _waypoint)
        {
            if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
            {
                DateTime _dt;
                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                if (DateTime.Now < _dt)
                {
                    SavePoint(_cInfo, _waypoint, Reserved_Max_Waypoints);
                    return;
                }
            }
            SavePoint(_cInfo, _waypoint, Max_Waypoints);
        }

        private static void SavePoint(ClientInfo _cInfo, string _waypoint, int _waypointTotal)
        {
            if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count > 0)
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count < _waypointTotal)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Vector3 _position = _player.GetPosition();
                        int _x = (int)_position.x;
                        int _y = (int)_position.y;
                        int _z = (int)_position.z;
                        string _wposition = _x + "," + _y + "," + _z;
                        Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                        if (!_waypoints.ContainsKey(_waypoint))
                        {
                            _waypoints.Add(_waypoint, _wposition);
                            PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                            Phrases.Dict.TryGetValue(278, out string _phrase278);
                            _phrase278 = _phrase278.Replace("{Name}", _waypoint);
                            _phrase278 = _phrase278.Replace("{Position}", _wposition);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase278 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(285, out string _phrase285);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase285 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(275, out string _phrase275);
                    _phrase275 = _phrase275.Replace("{Value}", _waypointTotal.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase275 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    Dictionary<string, string> _waypoints = new Dictionary<string, string>();
                    Vector3 _position = _player.GetPosition();
                    int _x = (int)_position.x;
                    int _y = (int)_position.y;
                    int _z = (int)_position.z;
                    string _wposition = _x + "," + _y + "," + _z;
                    _waypoints.Add(_waypoint, _wposition);
                    PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                    Phrases.Dict.TryGetValue(278, out string _phrase278);
                    _phrase278 = _phrase278.Replace("{Name}", _waypoint);
                    _phrase278 = _phrase278.Replace("{Position}", _wposition);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase278 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void DelPoint(ClientInfo _cInfo, string _waypoint)
        {
            if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint))
            {
                Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                _waypoints.Remove(_waypoint);
                PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                Phrases.Dict.TryGetValue(277, out string _phrase277);
                _phrase277 = _phrase277.Replace("{Name}", _waypoint);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase277 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(274, out string _phrase274);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase274 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            } 
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                if (_player2 != null)
                {
                    if (_player.IsFriendsWith(_player2))
                    {
                        if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                        {
                            Phrases.Dict.TryGetValue(286, out string _phrase286);
                            _phrase286 = _phrase286.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase286 = _phrase286.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase286 = _phrase286.Replace("{Command10}", Command10);
                            ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase286 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            Phrases.Dict.TryGetValue(287, out string _phrase287);
                            _phrase287 = _phrase287.Replace("{PlayerName}", _cInfo2.playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase287 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            if (Invite.ContainsKey(_cInfo2.entityId))
                            {
                                Invite.Remove(_cInfo2.entityId);
                                FriendPosition.Remove(_cInfo2.entityId);
                            }
                            Invite.Add(_cInfo2.entityId, DateTime.Now);
                            FriendPosition.Add(_cInfo2.entityId, _destination);
                        }
                    }
                }
            }
        }

        public static void FriendWaypoint(ClientInfo _cInfo)
        {
            Invite.TryGetValue(_cInfo.entityId, out DateTime _dt);
            {
                TimeSpan varTime = DateTime.Now - _dt;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed <= 2)
                {
                    string _pos;
                    FriendPosition.TryGetValue(_cInfo.entityId, out _pos);
                    {
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    Phrases.Dict.TryGetValue(288, out string _phrase288);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase288 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
