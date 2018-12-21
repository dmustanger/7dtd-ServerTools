﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Waypoint
    {
        public static bool IsEnabled = false, PvP_Check, Zombie_Check = false, Vehicle = false;
        public static int Delay_Between_Uses = 0, Max_Waypoints = 2, Donator_Max_Waypoints = 4, Command_Cost = 0;
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();

        public static void List(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT steamid FROM Waypoints WHERE steamid = '{0}' LIMIT 1", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        _sql = string.Format("SELECT steamid, wayPointName, position FROM Waypoints WHERE steamid = '{0}' LIMIT {1}", _cInfo.playerId, Donator_Max_Waypoints);
                        DataTable _result1 = SQL.TQuery(_sql);
                        foreach (DataRow row in _result1.Rows)
                        {
                            string _name = row[1].ToString();
                            int x, y, z;
                            string[] _cords = row[2].ToString().Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            string _message = "Waypoint {Name} @ {X} {Y} {Z}";
                            _message = _message.Replace("{Name}", _name);
                            _message = _message.Replace("{X}", x.ToString());
                            _message = _message.Replace("{Y}", y.ToString());
                            _message = _message.Replace("{Z}", z.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        _result1.Dispose();
                    }
                    else
                    {
                        _sql = string.Format("SELECT steamid, wayPointName, position FROM Waypoints WHERE steamid = '{0}' LIMIT {1}", _cInfo.playerId, Max_Waypoints);
                        DataTable _result1 = SQL.TQuery(_sql);
                        foreach (DataRow row in _result1.Rows)
                        {
                            string _name = row[1].ToString();
                            int x, y, z;
                            string[] _cords = row[2].ToString().Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            string _message = "Waypoint {Name} @ {X} {Y} {Z}";
                            _message = _message.Replace("{Name}", _name);
                            _message = _message.Replace("{X}", x.ToString());
                            _message = _message.Replace("{Y}", y.ToString());
                            _message = _message.Replace("{Z}", z.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        _result1.Dispose();
                    }
                }
                else
                {
                    _sql = string.Format("SELECT steamid, wayPointName, position FROM Waypoints WHERE steamid = '{0}' LIMIT {1}", _cInfo.playerId, Max_Waypoints);
                    DataTable _result1 = SQL.TQuery(_sql);
                    foreach (DataRow row in _result1.Rows)
                    {
                        string _name = row[1].ToString();
                        int x, y, z;
                        string[] _cords = row[2].ToString().Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        string _message = "Waypoint {Name} @ {X} {Y} {Z}";
                        _message = _message.Replace("{Name}", _name);
                        _message = _message.Replace("{X}", x.ToString());
                        _message = _message.Replace("{Y}", y.ToString());
                        _message = _message.Replace("{Z}", z.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    _result1.Dispose();
                }
            }
            else
            {
                string _phrase585;
                if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                {
                    _phrase585 = " you have no waypoints saved to list.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase585 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void Delay(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                if (Vehicle)
                {
                    Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Entity _attachedEntity = _player.AttachedToEntity;
                    if (_attachedEntity != null)
                    {
                        string _phrase587;
                        if (!Phrases.Dict.TryGetValue(587, out _phrase587))
                        {
                            _phrase587 = " you can not teleport to a waypoint with a vehicle.";
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase587 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                bool _donator = false;
                if (Delay_Between_Uses < 1)
                {
                    ClaimCheck(_cInfo, _waypoint);
                }
                else
                {
                    string _sql = string.Format("SELECT lastWaypoint FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    DateTime _lastWaypoint;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastWaypoint);
                    _result.Dispose();
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (_lastWaypoint.ToString() == "10/29/2000 7:30:00 AM")
                    {
                        ClaimCheck(_cInfo, _waypoint);
                    }
                    else
                    {
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
                                    _donator = true;
                                    int _newDelay = Delay_Between_Uses / 2;
                                    if (_timepassed >= _newDelay)
                                    {
                                        ClaimCheck(_cInfo, _waypoint);
                                    }
                                    else
                                    {
                                        int _timeleft = _newDelay - _timepassed;
                                        string _phrase575;
                                        if (!Phrases.Dict.TryGetValue(575, out _phrase575))
                                        {
                                            _phrase575 = " you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                        }
                                        _phrase575 = _phrase575.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase575 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                        if (!_donator)
                        {
                            if (_timepassed >= Delay_Between_Uses)
                            {
                                ClaimCheck(_cInfo, _waypoint);
                            }
                            else
                            {
                                int _timeleft = Delay_Between_Uses - _timepassed;
                                string _phrase575;
                                if (!Phrases.Dict.TryGetValue(575, out _phrase575))
                                {
                                    _phrase575 = " you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase575 = _phrase575.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase575 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you can not use waypoint commands while signed up for or in an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void ClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            Vector3i _vec3i = new Vector3i(x, y, z);
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
            EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
            if (_owner == EnumLandClaimOwner.None)
            {
                CommandCost(_cInfo, _waypoint, _player);
            }
            else
            {
                string _phrase576;
                if (!Phrases.Dict.TryGetValue(576, out _phrase576))
                {
                    _phrase576 = " you can only use a waypoint that is outside of a claimed space.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase576 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _waypoint, EntityPlayer _player)
        {
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                if (_currentCoins >= Command_Cost)
                {
                    Exec(_cInfo, _waypoint, _player);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Exec(_cInfo, _waypoint, _player);
            }
        }

        private static void Exec(ClientInfo _cInfo, string _waypoint, EntityPlayer _player)
        {
            string _sql = string.Format("SELECT position FROM Waypoints WHERE steamid = '{0}' AND wayPointName = '{1}'", _cInfo.playerId, _waypoint);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
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
                int x, y, z;
                string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
                string[] _cordsplit = _position.Split(',');
                int.TryParse(_cordsplit[0], out x);
                int.TryParse(_cordsplit[1], out y);
                int.TryParse(_cordsplit[2], out z);
                Players.NoFlight.Add(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql);
                string _phrase577;
                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = "Traveling to waypoint {Waypoint}.";
                }
                _phrase577 = _phrase577.Replace("{Waypoint}", _waypoint);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _phrase577 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = " you have not saved this waypoint.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _phrase578 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
                EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.None)
                {
                    SetPoint(_cInfo, _waypoint);
                }
                else
                {
                    string _phrase586;
                    if (!Phrases.Dict.TryGetValue(586, out _phrase586))
                    {
                        _phrase586 = " you can only save a waypoint that is outside of a claimed space.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase586 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you can not use waypoint commands while signed up for or in an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void SetPoint(ClientInfo _cInfo, string _waypoint)
        {
            string _sql = string.Format("SELECT steamid FROM Waypoints WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        if (_result.Rows.Count < Donator_Max_Waypoints && Donator_Max_Waypoints > 0)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _wposition = x + "," + y + "," + z;
                            _waypoint = SQL.EscapeString(_waypoint);
                            _sql = string.Format("INSERT INTO Waypoints (steamid, wayPointName, position) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _waypoint, _wposition);
                            SQL.FastQuery(_sql);
                            string _message = " saved waypoint name as {Name} to {X} {Y} {Z}.";
                            _message = _message.Replace("{Name}", _waypoint);
                            _message = _message.Replace("{X}", x.ToString());
                            _message = _message.Replace("{Y}", y.ToString());
                            _message = _message.Replace("{Z}", z.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _phrase581;
                            if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                            {
                                _phrase581 = " you have a maximum {Count} waypoints.";
                            }
                            _phrase581 = _phrase581.Replace("{Count}", Donator_Max_Waypoints.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase581 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    if (_result.Rows.Count < Max_Waypoints && Max_Waypoints > 0)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        Vector3 _position = _player.GetPosition();
                        int x = (int)_position.x;
                        int y = (int)_position.y;
                        int z = (int)_position.z;
                        string _wposition = x + "," + y + "," + z;
                        _waypoint = SQL.EscapeString(_waypoint);
                        _sql = string.Format("INSERT INTO Waypoints (steamid, wayPointName, position) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _waypoint, _wposition);
                        SQL.FastQuery(_sql);
                        string _message = " saved waypoint name as {Name} to {X} {Y} {Z}.";
                        _message = _message.Replace("{Name}", _waypoint);
                        _message = _message.Replace("{X}", x.ToString());
                        _message = _message.Replace("{Y}", y.ToString());
                        _message = _message.Replace("{Z}", z.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = " you have a maximum {Count} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{Count}", Max_Waypoints.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase582 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        if (Donator_Max_Waypoints > 0)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _wposition = x + "," + y + "," + z;
                            _waypoint = SQL.EscapeString(_waypoint);
                            _sql = string.Format("INSERT INTO Waypoints (steamid, wayPointName, position) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _waypoint, _wposition);
                            SQL.FastQuery(_sql);
                            string _message = " saved waypoint name as {Name} to {X} {Y} {Z}.";
                            _message = _message.Replace("{Name}", _waypoint);
                            _message = _message.Replace("{X}", x.ToString());
                            _message = _message.Replace("{Y}", y.ToString());
                            _message = _message.Replace("{Z}", z.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _phrase581;
                            if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                            {
                                _phrase581 = " you have a maximum {Count} waypoints.";
                            }
                            _phrase581 = _phrase581.Replace("{Count}", Donator_Max_Waypoints.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase581 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = " you have a maximum {Count} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{Count}", Max_Waypoints.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase582 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    if (Max_Waypoints > 0)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        Vector3 _position = _player.GetPosition();
                        int x = (int)_position.x;
                        int y = (int)_position.y;
                        int z = (int)_position.z;
                        string _wposition = x + "," + y + "," + z;
                        _waypoint = SQL.EscapeString(_waypoint);
                        _sql = string.Format("INSERT INTO Waypoints (steamid, wayPointName, position) VALUES ('{0}', '{1}', '{2}')", _cInfo.playerId, _waypoint, _wposition);
                        SQL.FastQuery(_sql);
                        string _message = " saved waypoint name as {Name} to {X} {Y} {Z}.";
                        _message = _message.Replace("{Name}", _waypoint);
                        _message = _message.Replace("{X}", x.ToString());
                        _message = _message.Replace("{Y}", y.ToString());
                        _message = _message.Replace("{Z}", z.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = " you have a maximum {Count} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{Count}", Max_Waypoints.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase582 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            _result.Dispose();
        }     
        
        public static void DelPoint(ClientInfo _cInfo, string _waypoint)
        {
            string _sql = string.Format("SELECT position FROM Waypoints WHERE steamid = '{0}' AND wayPointName = '{1}'", _cInfo.playerId, _waypoint);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                _sql = string.Format("DELETE FROM Waypoints WHERE steamid = '{0}' AND wayPointName = '{1}'", _cInfo.playerId, _waypoint);
                SQL.FastQuery(_sql);
                string _phrase583;
                if (!Phrases.Dict.TryGetValue(583, out _phrase583))
                {
                    _phrase583 = " waypoint {Name} has been deleted.";
                }
                _phrase583 = _phrase583.Replace("{Name}", _waypoint);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase583 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = " you have not saved this waypoint.";
                }
                _phrase578 = _phrase578.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase578 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose(); 
        }

        public static void FDelay(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.PlayersTeam.ContainsKey(_cInfo.playerId))
            {
                bool _donator = false;
                if (Delay_Between_Uses < 1)
                {
                    FClaimCheck(_cInfo, _waypoint);
                }
                else
                {
                    string _sql = string.Format("SELECT lastWaypoint FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    DateTime _lastWaypoint;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastWaypoint);
                    _result.Dispose();
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (_lastWaypoint.ToString() == "10/29/2000 7:30:00 AM")
                    {
                        FClaimCheck(_cInfo, _waypoint);
                    }
                    else
                    {
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
                                    _donator = true;
                                    int _newDelay = Delay_Between_Uses / 2;
                                    if (_timepassed >= _newDelay)
                                    {
                                        FClaimCheck(_cInfo, _waypoint);
                                    }
                                    else
                                    {
                                        int _timeleft = _newDelay - _timepassed;
                                        string _phrase575;
                                        if (!Phrases.Dict.TryGetValue(575, out _phrase575))
                                        {
                                            _phrase575 = " you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                        }
                                        _phrase575 = _phrase575.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase575 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                        if (!_donator)
                        {
                            if (_timepassed >= Delay_Between_Uses)
                            {
                                FClaimCheck(_cInfo, _waypoint);
                            }
                            else
                            {
                                int _timeleft = Delay_Between_Uses - _timepassed;
                                string _phrase575;
                                if (!Phrases.Dict.TryGetValue(575, out _phrase575))
                                {
                                    _phrase575 = " you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase575 = _phrase575.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase575 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you can not use waypoint commands while signed up for or in an event.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            Vector3i _vec3i = new Vector3i(x, y, z);
            PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
            EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
            if (_owner == EnumLandClaimOwner.None)
            {
                FCommandCost(_cInfo, _waypoint, _player);
            }
            else
            {
                string _phrase576;
                if (!Phrases.Dict.TryGetValue(576, out _phrase576))
                {
                    _phrase576 = " you can only use a waypoint that is outside of a claimed space.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase576 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FCommandCost(ClientInfo _cInfo, string _waypoint, EntityPlayer _player)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                FExec(_cInfo, _waypoint, _player);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void FExec(ClientInfo _cInfo, string _waypoint, EntityPlayer _player)
        {
            string _sql = string.Format("SELECT position FROM Waypoints WHERE steamid = '{0}' AND wayPointName = '{1}'", _cInfo.playerId, _waypoint);
            DataTable _result = SQL.TQuery(_sql);
            string _position = _result.Rows[0].ItemArray.GetValue(0).ToString();
            if (_result.Rows.Count > 0)
            {
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
                FriendInvite(_cInfo, _player.position, _position);
                int x, y, z;
                string[] _cordsplit = _position.Split(',');
                int.TryParse(_cordsplit[0], out x);
                int.TryParse(_cordsplit[1], out y);
                int.TryParse(_cordsplit[2], out z);
                Players.NoFlight.Add(_cInfo.entityId);
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql);
                string _phrase577;
                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = " traveling to waypoint {Waypoint}.";
                }
                _phrase577 = _phrase577.Replace("{Waypoint}", _waypoint);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase577 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = " you have not saved this waypoint.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase578 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            _result.Dispose();
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                EntityPlayer _player2 = world.Players.dict[_cInfo2.entityId];
                if (_player2 != null)
                {
                    if (_player.IsFriendsWith(_player2))
                    {
                        if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                        {
                            string _message = " your friend {PlayerName} has invited you to their saved waypoint. Type /go to accept the request.";
                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            _message = " invited your friend {PlayerName} to your saved waypoint.";
                            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        Players.NoFlight.Add(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", sending you to your friend's waypoint.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you have run out of time to accept your friend's waypoint invitation.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
