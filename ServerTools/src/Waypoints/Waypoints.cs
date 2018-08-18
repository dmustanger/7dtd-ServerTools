using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class Waypoint
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 0, Waypoints = 2, Donator_Waypoints = 4, Command_Cost = 0;
        public static Dictionary<int, DateTime> SavingPoint = new Dictionary<int, DateTime>();
        public static Dictionary<int, int> SavingPointNumber = new Dictionary<int, int>();      

        public static void List(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null && p.Waypoints != null)
            {
                int _Pts = 1;
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        DonRepeat:
                        if (p.Waypoints[_Pts] != null && p.WaypointName[_Pts] != null)
                        {
                            string _name = p.WaypointName[_Pts];
                            int x, y, z;
                            string[] _cords = p.Waypoints[_Pts].Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1}: {2} @ {3} {4} {5}[-]", Config.Chat_Response_Color, _Pts, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                            if (_Pts != Donator_Waypoints)
                            {
                                _Pts++;
                                goto DonRepeat;
                            }
                        }
                        else if (_Pts != Donator_Waypoints)
                        {
                            _Pts++;
                            goto DonRepeat;
                        }
                    }
                    else
                    {
                        Repeat:
                        if (p.Waypoints[_Pts] != null && p.WaypointName[_Pts] != null)
                        {
                            string _name = p.WaypointName[_Pts];
                            int x, y, z;
                            string[] _cords = p.Waypoints[_Pts].Split(',');
                            int.TryParse(_cords[0], out x);
                            int.TryParse(_cords[1], out y);
                            int.TryParse(_cords[2], out z);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1}: {2} @ {3} {4} {5}[-]", Config.Chat_Response_Color, _Pts, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                            if (_Pts != Waypoints)
                            {
                                _Pts++;
                                goto Repeat;
                            }
                        }
                        else if (_Pts != Waypoints)
                        {
                            _Pts++;
                            goto Repeat;
                        }
                    }
                }
                else
                {
                    Repeat:
                    if (p.Waypoints[_Pts] != null && p.WaypointName[_Pts] != null)
                    {
                        string _name = p.WaypointName[_Pts];
                        int x, y, z;
                        string[] _cords = p.Waypoints[_Pts].Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Waypoint {1}: {2} @ {3} {4} {5}[-]", Config.Chat_Response_Color, _Pts, _name, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                        if (_Pts != Waypoints)
                        {
                            _Pts++;
                            goto Repeat;
                        }
                    }
                    else if (_Pts != Waypoints)
                    {
                        _Pts++;
                        goto Repeat;
                    }
                }
            }
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

        public static void ClaimCheck(ClientInfo _cInfo, string _waypoint)
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
                CommandCost(_cInfo, _waypoint);
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

        public static void CommandCost(ClientInfo _cInfo, string _waypoint)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                Exec(_cInfo, _waypoint);
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

        public static void Exec(ClientInfo _cInfo, string _waypoint)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int _wp;
            if (int.TryParse(_waypoint, out _wp))
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        if (_wp >= 1 && _wp <= Donator_Waypoints)
                        {
                            string _cords = p.Waypoints[_wp];
                            if (_cords != null)
                            {
                                int x, y, z;
                                string[] _cordsplit = _cords.Split(',');
                                int.TryParse(_cordsplit[0], out x);
                                int.TryParse(_cordsplit[1], out y);
                                int.TryParse(_cordsplit[2], out z);
                                Players.NoFlight.Add(_cInfo.entityId);
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                 string _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                SQL.FastQuery(_sql);
                                string _phrase577;
                                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                                {
                                    _phrase577 = "{PlayerName}, traveling to waypoint number {Waypoint}.";
                                }
                                _phrase577 = _phrase577.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase577 = _phrase577.Replace("{Waypoint}", _wp.ToString());
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
                        }
                        else
                        {
                            string _phrase581;
                            if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                            {
                                _phrase581 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {DonatorCount} waypoints.";
                            }
                            _phrase581 = _phrase581.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase581 = _phrase581.Replace("{DonatorCount}", Donator_Waypoints.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase581), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        if (_wp >= 1 && _wp <= Waypoints)
                        {
                            string _cords = p.Waypoints[_wp];
                            if (_cords != null)
                            {
                                int x, y, z;
                                string[] _cordsplit = _cords.Split(',');
                                int.TryParse(_cordsplit[0], out x);
                                int.TryParse(_cordsplit[1], out y);
                                int.TryParse(_cordsplit[2], out z);
                                Players.NoFlight.Add(_cInfo.entityId);
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                string _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                SQL.FastQuery(_sql);
                                string _phrase577;
                                if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                                {
                                    _phrase577 = "{PlayerName}, traveling to waypoint number {Waypoint}.";
                                }
                                _phrase577 = _phrase577.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase577 = _phrase577.Replace("{Waypoint}", _wp.ToString());
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
                        }
                        else
                        {
                            string _phrase579;
                            if (!Phrases.Dict.TryGetValue(579, out _phrase579))
                            {
                                _phrase579 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {Count} waypoints.";
                            }
                            _phrase579 = _phrase579.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase579 = _phrase579.Replace("{Count}", Waypoints.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase579), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
                else
                {
                    if (_wp >= 1 && _wp <= Waypoints)
                    {
                        string _cords = p.Waypoints[_wp];
                        if (_cords != null)
                        {
                            int x, y, z;
                            string[] _cordsplit = _cords.Split(',');
                            int.TryParse(_cordsplit[0], out x);
                            int.TryParse(_cordsplit[1], out y);
                            int.TryParse(_cordsplit[2], out z);
                            Players.NoFlight.Add(_cInfo.entityId);
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            string _sql = string.Format("UPDATE Players SET lastWaypoint = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            string _phrase577;
                            if (!Phrases.Dict.TryGetValue(577, out _phrase577))
                            {
                                _phrase577 = "{PlayerName}, traveling to waypoint number {Waypoint}.";
                            }
                            _phrase577 = _phrase577.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase577 = _phrase577.Replace("{Waypoint}", _wp.ToString());
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
                    }
                    else
                    {
                        string _phrase579;
                        if (!Phrases.Dict.TryGetValue(579, out _phrase579))
                        {
                            _phrase579 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {Count} waypoints.";
                        }
                        _phrase579 = _phrase579.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase579 = _phrase579.Replace("{Count}", Waypoints.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase579), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                string _phrase580;
                if (!Phrases.Dict.TryGetValue(580, out _phrase580))
                {
                    _phrase580 = "{PlayerName}, this is not a valid waypoint number.";
                }
                _phrase580 = _phrase580.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase580), Config.Server_Response_Name, false, "ServerTools", false));
            }
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

        public static void SetPoint(ClientInfo _cInfo, string _waypoint)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                int _wp;
                if (int.TryParse(_waypoint, out _wp))
                {
                    if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now < _dt)
                        {
                            if (_wp >= 1 && _wp <= Donator_Waypoints)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _wposition = x + "," + y + "," + z;
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].Waypoints[_wp] = _wposition;
                                PersistentContainer.Instance.Save();
                                SavingPoint.Add(_cInfo.entityId, DateTime.Now);
                                SavingPointNumber.Add(_cInfo.entityId, _wp);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, enter a name for this point in chat.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                string _phrase581;
                                if (!Phrases.Dict.TryGetValue(581, out _phrase581))
                                {
                                    _phrase581 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {DonatorCount} waypoints.";
                                }
                                _phrase581 = _phrase581.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase581 = _phrase581.Replace("{DonatorCount}", Donator_Waypoints.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase581), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                        else
                        {
                            if (_wp >= 1 && _wp <= Waypoints)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _wposition = x + "," + y + "," + z;
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].Waypoints[_wp] = _wposition;
                                PersistentContainer.Instance.Save();
                                SavingPoint.Add(_cInfo.entityId, DateTime.Now);
                                SavingPointNumber.Add(_cInfo.entityId, _wp);
                                string _phrase582;
                                if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                                {
                                    _phrase582 = "{PlayerName}, enter a name for this point in chat.";
                                }
                                _phrase582 = _phrase582.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase582), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                string _phrase579;
                                if (!Phrases.Dict.TryGetValue(579, out _phrase579))
                                {
                                    _phrase579 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {Count} waypoints.";
                                }
                                _phrase579 = _phrase579.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase579 = _phrase579.Replace("{Count}", Waypoints.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase579), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                    else
                    {
                        if (_wp >= 1 && _wp <= Waypoints)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            Vector3 _position = _player.GetPosition();
                            int x = (int)_position.x;
                            int y = (int)_position.y;
                            int z = (int)_position.z;
                            string _wposition = x + "," + y + "," + z;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].Waypoints[_wp] = _wposition;
                            PersistentContainer.Instance.Save();
                            SavingPoint.Add(_cInfo.entityId, DateTime.Now);
                            SavingPointNumber.Add(_cInfo.entityId, _wp);
                            string _phrase582;
                            if (!Phrases.Dict.TryGetValue(582, out _phrase582))
                            {
                                _phrase582 = "{PlayerName}, enter a name for this point in chat.";
                            }
                            _phrase582 = _phrase582.Replace("{PlayerName}", _cInfo.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase582), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            string _phrase579;
                            if (!Phrases.Dict.TryGetValue(579, out _phrase579))
                            {
                                _phrase579 = "{PlayerName}, this is an invalid waypoint number. You have a maximum {Count} waypoints.";
                            }
                            _phrase579 = _phrase579.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase579 = _phrase579.Replace("{Count}", Waypoints.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase579), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
                else
                {
                    string _phrase580;
                    if (!Phrases.Dict.TryGetValue(580, out _phrase580))
                    {
                        _phrase580 = "{PlayerName}, this is not a valid waypoint number.";
                    }
                    _phrase580 = _phrase580.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase580), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void SetPointName(ClientInfo _cInfo, string _message)
        {
            DateTime _dt;
            SavingPoint.TryGetValue(_cInfo.entityId, out _dt);
            TimeSpan varTime = DateTime.Now - _dt;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (_timepassed <= 2)
            {
                int _wp;
                SavingPointNumber.TryGetValue(_cInfo.entityId, out _wp);
                PersistentContainer.Instance.Players[_cInfo.playerId, true].WaypointName[_wp] = _message;
                PersistentContainer.Instance.Save();
                SavingPoint.Remove(_cInfo.entityId);
                SavingPointNumber.Remove(_cInfo.entityId);
                string _phrase584;
                if (!Phrases.Dict.TryGetValue(584, out _phrase584))
                {
                    _phrase584 = "{PlayerName}, waypoint name set to: {Name}.";
                }
                _phrase584 = _phrase584.Replace("{PlayerName}", _cInfo.playerName);
                _phrase584 = _phrase584.Replace("{Name}", _message);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase584), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                SavingPoint.Remove(_cInfo.entityId);
                SavingPointNumber.Remove(_cInfo.entityId);
                string _phrase585;
                if (!Phrases.Dict.TryGetValue(585, out _phrase585))
                {
                    _phrase585 = "{PlayerName}, you have waited too long to set a waypoint name.";
                }
                _phrase585 = _phrase585.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase585), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void DelPoint(ClientInfo _cInfo, string _waypoint)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                int _wp;
                if (int.TryParse(_waypoint, out _wp))
                {
                    string _cords = p.Waypoints[_wp];
                    if (_cords != null)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].Waypoints[_wp] = null;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].WaypointName[_wp] = null;
                        PersistentContainer.Instance.Save();
                        string _phrase583;
                        if (!Phrases.Dict.TryGetValue(583, out _phrase583))
                        {
                            _phrase583 = "{PlayerName}, waypoint number {Number} has been deleted.";
                        }
                        _phrase583 = _phrase583.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase583 = _phrase583.Replace("{Number}", _wp.ToString());
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
                }
                else
                {
                    string _phrase580;
                    if (!Phrases.Dict.TryGetValue(580, out _phrase580))
                    {
                        _phrase580 = "{PlayerName}, this is not a valid waypoint number.";
                    }
                    _phrase580 = _phrase580.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase580), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}
