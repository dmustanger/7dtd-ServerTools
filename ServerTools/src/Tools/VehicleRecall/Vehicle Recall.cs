using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleRecall
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Delay_Between_Uses = 120, Distance = 50, Normal_Max = 2, Reserved_Max = 4, Command_Cost = 0;
        public static string Command_recall = "recall";

        public static void List(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles.Count > 0)
            {
                Dictionary<int, string[]> _vehicles = PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles;
                foreach (var _vehicle in _vehicles)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall1", out string _phrase);
                    _phrase = _phrase.Replace("{Id}", _vehicle.Key.ToString());
                    _phrase = _phrase.Replace("{Type}", _vehicle.Value[0]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall2", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _vehicle)
        {
            try
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (_vehicle != "")
                    {
                        if (!int.TryParse(_vehicle, out int _vehicleId))
                        {
                            Phrases.Dict.TryGetValue("VehicleRecall3", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        if (Delay_Between_Uses < 1)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, _player, _vehicleId);
                            }
                            else
                            {
                                TeleVehicle(_cInfo, _player, _vehicleId);
                            }
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles.ContainsKey(_vehicleId))
                            {
                                string[] _vehicleInfo = PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles[_vehicleId];
                                DateTime.TryParse(_vehicleInfo[1], out DateTime _delay);
                                if (DateTime.Now >= _delay)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, _player, _vehicleId);
                                    }
                                    else
                                    {
                                        TeleVehicle(_cInfo, _player, _vehicleId);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("VehicleRecall4", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_recall}", Command_recall);
                                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("VehicleRecall13", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        EntityVehicle _attachedVehicle = (EntityVehicle)_player.AttachedToEntity;
                        if (_attachedVehicle == null)
                        {
                            List(_cInfo);
                        }
                        else
                        {
                            int _maxVehicles = Normal_Max;
                            if (ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _maxVehicles = Reserved_Max;
                                    }
                                }
                            }
                            SaveVehicle(_cInfo, _player, _attachedVehicle, _maxVehicles);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleport.Exec: {0}", e.Message));
            }
        }

        public static void CommandCost(ClientInfo _cInfo, Entity _player, int _vehicleId)
        {
            if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
            {
                TeleVehicle(_cInfo, _player, _vehicleId);
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall5", out string _phrase);
                _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SaveVehicle(ClientInfo _cInfo, EntityPlayer _player, EntityVehicle _attachedVehicle, int _max)
        {
            try
            {
                if (Inside_Claim)
                {
                    World world = GameManager.Instance.World;
                    Vector3 _position = _player.GetPosition();
                    int x = (int)_position.x;
                    int y = (int)_position.y;
                    int z = (int)_position.z;
                    Vector3i _vec3i = new Vector3i(x, y, z);
                    if (!PersistentOperations.ClaimedByAllyOrSelf(_cInfo.playerId, _vec3i))
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall6", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                Dictionary<int, string[]> _vehicles;
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles.Count > 0)
                {
                    _vehicles = PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles;
                }
                else
                {
                    _vehicles = new Dictionary<int, string[]>();
                }
                if (!_vehicles.ContainsKey(_attachedVehicle.entityId))
                {
                    if (_vehicles.Count < _max)
                    {
                        _vehicles.Add(_attachedVehicle.entityId, new string[] { _attachedVehicle.EntityClass.entityClassName, new DateTime().ToString() });
                        PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles = _vehicles;
                        Phrases.Dict.TryGetValue("VehicleRecall7", out string _phrase);
                        _phrase = _phrase.Replace("{Type}", _attachedVehicle.EntityClass.entityClassName);
                        _phrase = _phrase.Replace("{Value}", _attachedVehicle.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall8", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("VehicleRecall9", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleport.SaveVehicle: {0}", e.Message));
            }
        }

        public static void TeleVehicle(ClientInfo _cInfo, Entity _player, int _vehicleId)
        {
            Entity _entity = PersistentOperations.GetEntity(_vehicleId);
            if (_entity != null && _entity is EntityVehicle)
            {
                if (_entity.AttachedToEntity != null)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall10", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityVehicle _vehicle = (EntityVehicle)_entity;
                int _x = (int)_vehicle.position.x;
                int _y = (int)_vehicle.position.x;
                int _z = (int)_vehicle.position.x;
                if ((_player.position.x - _x) * (_player.position.x - _x) + (_player.position.z - _z) * (_player.position.z - _z) <= Distance * Distance)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                    }
                    int _delay = Delay_Between_Uses;
                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_x, _y, _z);
                    if (_chunk == null)
                    {
                        
                            _vehicle.SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _delay = Delay_Between_Uses / 2;
                                    }
                                }
                            }
                            PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles[_vehicleId] = new string[] { _vehicle.EntityClass.entityClassName, DateTime.Now.AddMinutes(_delay).ToString() };
                            Phrases.Dict.TryGetValue("VehicleRecall11", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);

                    }
                    else
                    {
                        _vehicle.SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
                        PersistentContainer.Instance.Players[_cInfo.playerId].Vehicles[_vehicleId] = new string[] { _vehicle.EntityClass.entityClassName, DateTime.Now.AddMinutes(_delay).ToString() };
                        Phrases.Dict.TryGetValue("VehicleRecall11", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    PersistentContainer.DataChange = true;
                }
                else
                {
                    Phrases.Dict.TryGetValue("VehicleRecall12", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall14", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
