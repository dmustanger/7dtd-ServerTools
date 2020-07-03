using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Waypoints
    {
        public static bool IsEnabled = false, PvP_Check = false, Zombie_Check = false, Vehicle = false;
        public static int Delay_Between_Uses = 0, Max_Waypoints = 2, Donator_Max_Waypoints = 4, Command_Cost = 0;
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
                        ListResult(_cInfo, Donator_Max_Waypoints);
                        return;
                    }
                }
                ListResult(_cInfo, Max_Waypoints);
            }
            else
            {
                string _phrase585;
                if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                {
                    _phrase585 = "You have no waypoints saved.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase585 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _message = string.Format("Waypoint {0} @ {1} {2} {3}", _waypoint.Key, _x, _y, _z);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _count++;
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
                        string _phrase585;
                        if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                        {
                            _phrase585 = "You have no waypoints saved.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase585 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _phrase585;
                            if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                            {
                                _phrase585 = "You have no waypoints saved.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase585 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use waypoint commands while in a event.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                string _phrase575;
                if (!Phrases.Dict.TryGetValue(575, out _phrase575))
                {
                    _phrase575 = "You can only use {CommandPrivate}{Command106} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase575 = _phrase575.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase575 = _phrase575.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase575 = _phrase575.Replace("{Command106}", Command106);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase575 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _phrase587;
                        if (!Phrases.Dict.TryGetValue(587, out _phrase587))
                        {
                            _phrase587 = "You can not teleport to a waypoint with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase587 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
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
                if (PersistentOperations.ClaimedByNone(_cInfo.playerId, _vec3i))
                {
                    CommandCost(_cInfo, _waypoint, _position, _friends);
                }
                else
                {
                    string _phrase576;
                    if (!Phrases.Dict.TryGetValue(576, out _phrase576))
                    {
                        _phrase576 = "You can only use a waypoint that is outside of a claimed space.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase576 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
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
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "You do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                if (_friends)
                {
                    FriendInvite(_cInfo, _position, _waypointPos);
                }
                string _phrase577;
                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = "Traveling to waypoint {Waypoint}.";
                }
                _phrase577 = _phrase577.Replace("{Waypoint}", _waypoint);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase577 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string[] _cords = _waypointPos.Split(',');
                int.TryParse(_cords[0], out int _x);
                int.TryParse(_cords[1], out int _y);
                int.TryParse(_cords[2], out int _z);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint = DateTime.Now;
                PersistentContainer.Instance.Save();
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = "This waypoint was not found on your list.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase578 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        string _phrase586;
                        if (!Phrases.Dict.TryGetValue(586, out _phrase586))
                        {
                            _phrase586 = "You can only save a waypoint that is outside of a claimed space.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase586 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use waypoint commands while in a event.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    SavePoint(_cInfo, _waypoint, Donator_Max_Waypoints);
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
                            PersistentContainer.Instance.Save();
                            string _message = string.Format("Saved waypoint name as {0} to position {1}.", _waypoint, _wposition);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have a waypoint with this name already. Choose another name." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    string _phrase579;
                    if (!Phrases.Dict.TryGetValue(579, out _phrase579))
                    {
                        _phrase579 = "You have a maximum {Count} waypoints.";
                    }
                    _phrase579 = _phrase579.Replace("{Count}", _waypointTotal.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase579 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    PersistentContainer.Instance.Save();
                    string _message = string.Format("Saved waypoint name as {0} to position {1}.", _waypoint, _wposition);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                PersistentContainer.Instance.Save();
                string _phrase583;
                if (!Phrases.Dict.TryGetValue(583, out _phrase583))
                {
                    _phrase583 = "Waypoint {Name} has been deleted.";
                }
                _phrase583 = _phrase583.Replace("{Name}", _waypoint);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase583 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = "This waypoint was not found on your list.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase578 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _message = "Your friend {PlayerName} has invited you to their saved waypoint. Type {CommandPrivate}{Command10} to accept the request.";
                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                            _message = _message.Replace("{Command10}", Command10);
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            _message = "Invited your friend {PlayerName} to your saved waypoint.";
                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            DateTime _dt;
            Invite.TryGetValue(_cInfo.entityId, out _dt);
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
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Sending you to your friend's waypoint.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have run out of time to accept your friend's waypoint invitation.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
