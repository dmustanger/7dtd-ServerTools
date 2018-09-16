using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Waypoint
    {
        public static bool IsEnabled = false, PvP_Check, Zombie_Check = false;
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1} @ {2} {3} {4}[-]", Config.Chat_Response_Color, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1} @ {2} {3} {4}[-]", Config.Chat_Response_Color, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1} @ {2} {3} {4}[-]", Config.Chat_Response_Color, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    _result1.Dispose();
                }
            }
            else
            {
                string _phrase585;
                if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                {
                    _phrase585 = "{PlayerName} you have no waypoints saved to list.";
                }
                _phrase585 = _phrase585.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase585), Config.Server_Response_Name, false, "ServerTools", false));
            }
            _result.Dispose();
        }

        public static void Delay(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
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
                                            _phrase575 = "{PlayerName} you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                        }
                                        _phrase575 = _phrase575.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase575 = _phrase575.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase575), Config.Server_Response_Name, false, "ServerTools", false));
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
                                    _phrase575 = "{PlayerName} you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase575 = _phrase575.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase575 = _phrase575.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase575), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use waypoint commands while signed up for or in an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase576 = "{PlayerName} you can only use a waypoint that is outside of a claimed space.";
                }
                _phrase576 = _phrase576.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase576), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _waypoint, EntityPlayer _player)
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
                    _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql);
                string _phrase577;
                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = "{PlayerName}, traveling to waypoint {Waypoint}.";
                }
                _phrase577 = _phrase577.Replace("{PlayerName}", _cInfo.playerName);
                _phrase577 = _phrase577.Replace("{Waypoint}", _waypoint);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase577), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = "{PlayerName}, you have not saved this waypoint.";
                }
                _phrase578 = _phrase578.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase578), Config.Server_Response_Name, false, "ServerTools", false));
            }
            _result.Dispose();
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
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
                        _phrase586 = "{PlayerName} you can only save a waypoint that is outside of a claimed space.";
                    }
                    _phrase586 = _phrase586.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase586), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use waypoint commands while signed up for or in an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, saved waypoint name as {2} to {3} {4} {5}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _waypoint, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            string _phrase581;
                            if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                            {
                                _phrase581 = "{PlayerName}, You have a maximum {DonatorCount} waypoints.";
                            }
                            _phrase581 = _phrase581.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase581 = _phrase581.Replace("{DonatorCount}", Donator_Max_Waypoints.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase581), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, saved waypoint name as {2} to {3} {4} {5}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _waypoint, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = "{PlayerName}, You have a maximum {NormalCount} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase582 = _phrase582.Replace("{NormalCount}", Max_Waypoints.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase582), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, saved waypoint name as {2} to {3} {4} {5}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _waypoint, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            string _phrase581;
                            if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                            {
                                _phrase581 = "{PlayerName}, You have a maximum {DonatorCount} waypoints.";
                            }
                            _phrase581 = _phrase581.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase581 = _phrase581.Replace("{DonatorCount}", Donator_Max_Waypoints.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase581), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = "{PlayerName}, You have a maximum {NormalCount} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase582 = _phrase582.Replace("{NormalCount}", Max_Waypoints.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase582), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, saved waypoint name as {2} to {3} {4} {5}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _waypoint, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        string _phrase582;
                        if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                        {
                            _phrase582 = "{PlayerName}, You have a maximum {NormalCount} waypoints.";
                        }
                        _phrase582 = _phrase582.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase582 = _phrase582.Replace("{NormalCount}", Max_Waypoints.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase582), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase583 = "{PlayerName}, waypoint {Waypoint} has been deleted.";
                }
                _phrase583 = _phrase583.Replace("{PlayerName}", _cInfo.playerName);
                _phrase583 = _phrase583.Replace("{Waypoint}", _waypoint);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase583), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = "{PlayerName}, you have not saved this waypoint.";
                }
                _phrase578 = _phrase578.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase578), Config.Server_Response_Name, false, "ServerTools", false));
            }
            _result.Dispose(); 
        }

        public static void FDelay(ClientInfo _cInfo, string _waypoint)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
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
                                            _phrase575 = "{PlayerName} you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                        }
                                        _phrase575 = _phrase575.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase575 = _phrase575.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                        _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase575), Config.Server_Response_Name, false, "ServerTools", false));
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
                                    _phrase575 = "{PlayerName} you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                }
                                _phrase575 = _phrase575.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase575 = _phrase575.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                _phrase575 = _phrase575.Replace("{TimeRemaining}", _timeleft.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase575), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use waypoint commands while signed up for or in an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase576 = "{PlayerName} you can only use a waypoint that is outside of a claimed space.";
                }
                _phrase576 = _phrase576.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase576), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql);
                string _phrase577;
                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = "{PlayerName}, traveling to waypoint {Waypoint}.";
                }
                _phrase577 = _phrase577.Replace("{PlayerName}", _cInfo.playerName);
                _phrase577 = _phrase577.Replace("{Waypoint}", _waypoint);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase577), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase578;
                if (!Phrases.Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = "{PlayerName}, you have not saved this waypoint.";
                }
                _phrase578 = _phrase578.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase578), Config.Server_Response_Name, false, "ServerTools", false));
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
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
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
                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your friend {2} has invited you to their saved waypoint. Type /go to accept the request.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Invited your friend {1} to your saved waypoint.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} sending you to your friend's waypoint.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have run out of time to accept your friend's waypoint invitation.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}
