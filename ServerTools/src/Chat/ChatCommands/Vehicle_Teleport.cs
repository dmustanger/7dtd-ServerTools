using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ServerTools
{
    class VehicleTeleport
    {
        public static bool IsEnabled = false, Bike = false, Mini_Bike = false, Motor_Bike = false, Jeep = false, Gyro = false, Inside_Claim = true;
        public static int Delay_Between_Uses = 120, Command_Cost = 0;

        public static void VehicleDelay(ClientInfo _cInfo, string _playerName, int _vehicle)
        {
            string _sql = string.Format("SELECT steamid FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count == 0)
            {
                string _steamid = SQL.EscapeString(_cInfo.playerId);
                _sql = string.Format("INSERT INTO Vehicles (steamid) VALUES ('{0}')", _steamid);
                SQL.FastQuery(_sql);
            }
            _result.Dispose();
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Entity _attachedEntity = _player.AttachedToEntity;
            if (_attachedEntity == null)
            {
                bool _donator = false;
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, _player, _vehicle);
                    }
                    else
                    {
                        Exec2(_cInfo, _player, _vehicle);
                    }
                }
                else
                {
                    if (_vehicle == 1)
                    {
                        _sql = string.Format("SELECT lastBike FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
                    }
                    if (_vehicle == 2)
                    {
                        _sql = string.Format("SELECT lastMiniBike FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
                    }
                    if (_vehicle == 3)
                    {
                        _sql = string.Format("SELECT lastMotorBike FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
                    }
                    if (_vehicle == 4)
                    {
                        _sql = string.Format("SELECT lastJeep FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
                    }
                    if (_vehicle == 5)
                    {
                        _sql = string.Format("SELECT lastGyro FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
                    }
                    _result = SQL.TQuery(_sql);
                    DateTime _lastVehicle;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastVehicle);
                    _result.Dispose();
                    TimeSpan varTime = DateTime.Now - _lastVehicle;
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
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, _player, _vehicle);
                                    }
                                    else
                                    {
                                        Exec2(_cInfo, _player, _vehicle);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase786;
                                    if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                                    {
                                        _phrase786 = " you can only use vehicle teleport once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase786 = _phrase786.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase786 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, _player, _vehicle);
                            }
                            else
                            {
                                Exec2(_cInfo, _player, _vehicle);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase786;
                            if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                            {
                                _phrase786 = " you can only use vehicle teleport once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase786 = _phrase786.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase786 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                Exec1(_cInfo, _player, _vehicle);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, Entity _player, int _vehicle)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                Exec2(_cInfo, _player, _vehicle);
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

        public static void Exec1(ClientInfo _cInfo, Entity _player, int _vehicle)
        {
            if (Inside_Claim)
            {
                World world = GameManager.Instance.World;
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(x, y, z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerDataFromEntityID(_player.entityId);
                EnumLandClaimOwner _owner = world.GetLandClaimOwner(_vec3i, _persistentPlayerData);
                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                {
                    string _vehicleName = "";
                    string _messageName = "";
                    if (_vehicle == 1)
                    {
                        _vehicleName = "vehicleBicycle";
                        _messageName = "bike";
                    }
                    if (_vehicle == 2)
                    {
                        _vehicleName = "vehicleMinibike";
                        _messageName = "minibike";
                    }
                    if (_vehicle == 3)
                    {
                        _vehicleName = "vehicleMotorcycle";
                        _messageName = "motorbike";
                    }
                    if (_vehicle == 4)
                    {
                        _vehicleName = "vehicle4x4Truck";
                        _messageName = "jeep";
                    }
                    if (_vehicle == 5)
                    {
                        _vehicleName = "vehicleGyrocopter";
                        _messageName = "gyro";
                    }
                    if (_player.AttachedToEntity.EntityClass.entityClassName.ToString() == _vehicleName)
                    {
                        string _phrase781;
                        if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                        {
                            _phrase781 = " saved your current vehicle for retrieval.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase781 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        string _sql = "";
                        if (_vehicle == 1)
                        {
                            _sql = string.Format("UPDATE Vehicles SET bikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 2)
                        {
                            _sql = string.Format("UPDATE Vehicles SET miniBikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 3)
                        {
                            _sql = string.Format("UPDATE Vehicles SET motorBikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 4)
                        {
                            _sql = string.Format("UPDATE Vehicles SET jeepId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 5)
                        {
                            _sql = string.Format("UPDATE Vehicles SET gyroId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        SQL.FastQuery(_sql);
                    }
                    else
                    {
                        string _phrase787;
                        if (!Phrases.Dict.TryGetValue(787, out _phrase787))
                        {
                            _phrase787 = " you are on the wrong vehicle to save it with this command. You are using a {Vehicle}.";
                        }
                        _phrase787 = _phrase787.Replace("{Vehicle}", _messageName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase787 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    string _phrase780;
                    if (!Phrases.Dict.TryGetValue(780, out _phrase780))
                    {
                        _phrase780 = " you have not claimed this space or a friend. You can only save your vehicle inside a claimed space.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase780 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _vehicleName = "";
                    string _messageName = "";
                    if (_vehicle == 1)
                    {
                        _vehicleName = "vehicleBicycle";
                        _messageName = "bike";
                    }
                    if (_vehicle == 2)
                    {
                        _vehicleName = "vehicleMinibike";
                        _messageName = "minibike";
                    }
                    if (_vehicle == 3)
                    {
                        _vehicleName = "vehicleMotorcycle";
                        _messageName = "motorbike";
                    }
                    if (_vehicle == 4)
                    {
                        _vehicleName = "vehicle4x4Truck";
                        _messageName = "jeep";
                    }
                    if (_vehicle == 5)
                    {
                        _vehicleName = "vehicleGyrocopter";
                        _messageName = "gyro";
                    }
                    if (_player.AttachedToEntity.name == _vehicleName)
                    {
                        string _phrase781;
                        if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                        {
                            _phrase781 = " saved your current vehicle for retrieval.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase781 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        string _sql = "";
                        if (_vehicle == 1)
                        {
                            _sql = string.Format("UPDATE Vehicles SET bikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 2)
                        {
                            _sql = string.Format("UPDATE Vehicles SET miniBikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 3)
                        {
                            _sql = string.Format("UPDATE Vehicles SET motorBikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 4)
                        {
                            _sql = string.Format("UPDATE Vehicles SET jeepId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        if (_vehicle == 5)
                        {
                            _sql = string.Format("UPDATE Vehicles SET gyroId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        }
                        SQL.FastQuery(_sql);
                    }
                    else
                    {
                    string _phrase787;
                        if (!Phrases.Dict.TryGetValue(787, out _phrase787))
                        {
                            _phrase787 = " you are on the wrong vehicle to save it with this command. You are using a {Vehicle}.";
                        }
                        _phrase787 = _phrase787.Replace("{Vehicle}", _messageName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase787 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
            }
        }

        public static void Exec2(ClientInfo _cInfo, Entity _player, int _vehicle)
        {
            string _sql = "";
            if (_vehicle == 1)
            {
                _sql = string.Format("SELECT bikeId FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            }
            if (_vehicle == 2)
            {
                _sql = string.Format("SELECT miniBikeId FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            }
            if (_vehicle == 3)
            {
                _sql = string.Format("SELECT motorBikeId FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            }
            if (_vehicle == 4)
            {
                _sql = string.Format("SELECT jeepId FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            }
            if (_vehicle == 5)
            {
                _sql = string.Format("SELECT gyroId FROM Vehicles WHERE steamid = '{0}'", _cInfo.playerId);
            }
            DataTable _result = SQL.TQuery(_sql);
            int _Id;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _Id);
            _result.Dispose();
            if (_Id != 0)
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (!_entity.IsClientControlled())
                    {
                        if (_entity.entityId == _Id)
                        {
                            if ((_player.position.x - _entity.position.x) * (_player.position.x - _entity.position.x) + (_player.position.z - _entity.position.z) * (_player.position.z - _entity.position.z) <= 50 * 50)
                            {
                                if (_entity.AttachedToEntity == false)
                                {
                                    _entity.SetPosition(_player.position);
                                    string _phrase782;
                                    if (!Phrases.Dict.TryGetValue(782, out _phrase782))
                                    {
                                        _phrase782 = " found your vehicle and sent it to you.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase782 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);

                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    if (_vehicle == 1)
                                    {
                                        _sql = string.Format("UPDATE Vehicles SET lastBike = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    }
                                    if (_vehicle == 2)
                                    {
                                        _sql = string.Format("UPDATE Vehicles SET lastMiniBike = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    }
                                    if (_vehicle == 3)
                                    {
                                        _sql = string.Format("UPDATE Vehicles SET lastMotorBike = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    }
                                    if (_vehicle == 4)
                                    {
                                        _sql = string.Format("UPDATE Vehicles SET lastJeep = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    }
                                    if (_vehicle == 5)
                                    {
                                        _sql = string.Format("UPDATE Vehicles SET lastGyro = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                    }

                                    SQL.FastQuery(_sql);
                                    return;
                                }
                                else
                                {
                                    string _phrase785;
                                    if (!Phrases.Dict.TryGetValue(785, out _phrase785))
                                    {
                                        _phrase785 = " found your vehicle but someone else is on it.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase785 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                    }
                }
                string _phrase784;
                if (!Phrases.Dict.TryGetValue(784, out _phrase784))
                {
                    _phrase784 = " could not find your vehicle near by.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase784 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase783;
                if (!Phrases.Dict.TryGetValue(783, out _phrase783))
                {
                    _phrase783 = " you do not have this vehicle type saved.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase783 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
