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
        public static string Command77 = "bike", Command78 = "minibike", Command79 = "motorbike", Command80 = "jeep", Command81 = "gyro";

        public static void Exec(ClientInfo _cInfo, int _vehicle)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Entity _attachedEntity = _player.AttachedToEntity;
            if (_attachedEntity == null)
            {
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, _player, _vehicle);
                    }
                    else
                    {
                        TeleVehicle(_cInfo, _player, _vehicle);
                    }
                }
                else
                {
                    DateTime _lastVehicle = new DateTime();
                    if (_vehicle == 1)
                    {
                        _lastVehicle = PersistentContainer.Instance.Players[_cInfo.playerId].LastBike;
                    }
                    else if (_vehicle == 2)
                    {
                        _lastVehicle = PersistentContainer.Instance.Players[_cInfo.playerId].LastMiniBike;
                    }
                    else if (_vehicle == 3)
                    {
                        _lastVehicle = PersistentContainer.Instance.Players[_cInfo.playerId].LastMotorBike;
                    }
                    else if (_vehicle == 4)
                    {
                        _lastVehicle = PersistentContainer.Instance.Players[_cInfo.playerId].LastJeep;
                    }
                    else if (_vehicle == 5)
                    {
                        _lastVehicle = PersistentContainer.Instance.Players[_cInfo.playerId].LastGyro;
                    }
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
                                int _delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _player, _vehicle, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, _player, _vehicle, _timepassed, Delay_Between_Uses);
                }
            }
            else
            {
                SaveVehicle(_cInfo, _player, _vehicle, _attachedEntity.entityId);
            }
        }

        public static void Time(ClientInfo _cInfo, Entity _player, int _vehicle, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _player, _vehicle);
                }
                else
                {
                    TeleVehicle(_cInfo, _player, _vehicle);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                string _phrase786;
                if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                {
                    _phrase786 = " you can only use vehicle teleport once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase786 = _phrase786.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase786 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }


        public static void CommandCost(ClientInfo _cInfo, Entity _player, int _vehicle)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                TeleVehicle(_cInfo, _player, _vehicle);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SaveVehicle(ClientInfo _cInfo, Entity _player, int _vehicle, int _vehicleId)
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
                if (!(_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally))
                {
                    string _phrase780;
                    if (!Phrases.Dict.TryGetValue(780, out _phrase780))
                    {
                        _phrase780 = " you have not claimed this space or a friend. You can only save your vehicle inside a claimed space.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase780 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            string _entityName = "", _vehicleName = "";
            if (_vehicle == 1)
            {
                _entityName = "vehicleBicycle";
                _vehicleName = "bike";
            }
            else if (_vehicle == 2)
            {
                _entityName = "vehicleMinibike";
                _vehicleName = "minibike";
            }
            else if (_vehicle == 3)
            {
                _entityName = "vehicleMotorcycle";
                _vehicleName = "motorbike";
            }
            else if (_vehicle == 4)
            {
                _entityName = "vehicle4x4Truck";
                _vehicleName = "jeep";
            }
            else if (_vehicle == 5)
            {
                _entityName = "vehicleGyrocopter";
                _vehicleName = "gyro";
            }
            if (_player.AttachedToEntity.EntityClass.entityClassName.ToString() == _entityName)
            {
                if (_vehicle == 1)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].BikeId = _vehicleId;
                }
                else if (_vehicle == 2)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].MiniBikeId = _vehicleId;
                }
                else if (_vehicle == 3)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].MotorBikeId = _vehicleId;
                }
                else if (_vehicle == 4)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].JeepId = _vehicleId;
                }
                else if (_vehicle == 5)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].GyroId = _vehicleId;
                }
                PersistentContainer.Instance.Save();
                string _phrase781;
                if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                {
                    _phrase781 = " saved your current {Vehicle} for retrieval.";
                    _phrase781 = _phrase781.Replace("{Vehicle}", _vehicleName);
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase781 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase787;
                if (!Phrases.Dict.TryGetValue(787, out _phrase787))
                {
                    _phrase787 = " you are on the wrong vehicle to save it with this command. You are using a {Vehicle}.";
                }
                _phrase787 = _phrase787.Replace("{Vehicle}", _vehicleName);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase787 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void TeleVehicle(ClientInfo _cInfo, Entity _player, int _vehicle)
        {
            int _vehicleId = 0;
            if (_vehicle == 1)
            {
                _vehicleId = PersistentContainer.Instance.Players[_cInfo.playerId].BikeId;
            }
            else if (_vehicle == 2)
            {
                _vehicleId = PersistentContainer.Instance.Players[_cInfo.playerId].MiniBikeId;
            }
            else if (_vehicle == 3)
            {
                _vehicleId = PersistentContainer.Instance.Players[_cInfo.playerId].MotorBikeId;
            }
            else if (_vehicle == 4)
            {
                _vehicleId = PersistentContainer.Instance.Players[_cInfo.playerId].JeepId;
            }
            else if (_vehicle == 5)
            {
                _vehicleId = PersistentContainer.Instance.Players[_cInfo.playerId].GyroId;
            }
            if (_vehicleId != 0)
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (!_entity.IsClientControlled())
                    {
                        if (_entity.entityId == _vehicleId)
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
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase782 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);

                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    if (_vehicle == 1)
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].LastBike = DateTime.Now;
                                    }
                                    if (_vehicle == 2)
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].LastMiniBike = DateTime.Now;
                                    }
                                    if (_vehicle == 3)
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].LastMotorBike = DateTime.Now;
                                    }
                                    if (_vehicle == 4)
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].LastJeep = DateTime.Now;
                                    }
                                    if (_vehicle == 5)
                                    {
                                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGyro = DateTime.Now;
                                    }
                                    PersistentContainer.Instance.Save();
                                    return;
                                }
                                else
                                {
                                    string _phrase785;
                                    if (!Phrases.Dict.TryGetValue(785, out _phrase785))
                                    {
                                        _phrase785 = " found your vehicle but someone else is on it.";
                                    }
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase785 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase784 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _phrase783;
                if (!Phrases.Dict.TryGetValue(783, out _phrase783))
                {
                    _phrase783 = " you do not have this vehicle type saved.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase783 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
