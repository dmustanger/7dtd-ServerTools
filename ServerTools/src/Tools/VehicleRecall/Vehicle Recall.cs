using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleRecall
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Delay_Between_Uses = 120, Distance = 50, Normal_Max = 2, Reserved_Max = 4, Command_Cost = 0;
        public static string Command_recall = "recall", Command_recall_del = "recall del";

        public static void List(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Count > 0)
            {
                Dictionary<int, string[]> _vehicles = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles;
                foreach (var _vehicle in _vehicles)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall1", out string phrase);
                    phrase = phrase.Replace("{Id}", _vehicle.Key.ToString());
                    phrase = phrase.Replace("{Type}", _vehicle.Value[0]);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _vehicle)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (_vehicle != "")
                    {
                        if (!int.TryParse(_vehicle, out int _vehicleId))
                        {
                            Phrases.Dict.TryGetValue("VehicleRecall3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        if (Delay_Between_Uses < 1)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, player, _vehicleId);
                            }
                            else
                            {
                                TeleVehicle(_cInfo, player, _vehicleId);
                            }
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.ContainsKey(_vehicleId))
                            {
                                string[] _vehicleInfo = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles[_vehicleId];
                                DateTime.TryParse(_vehicleInfo[1], out DateTime _delay);
                                if (DateTime.Now >= _delay)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, player, _vehicleId);
                                    }
                                    else
                                    {
                                        TeleVehicle(_cInfo, player, _vehicleId);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("VehicleRecall4", out string phrase);
                                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    phrase = phrase.Replace("{Command_recall}", Command_recall);
                                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("VehicleRecall13", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        EntityVehicle attachedVehicle = (EntityVehicle)player.AttachedToEntity;
                        if (attachedVehicle == null)
                        {
                            List(_cInfo);
                        }
                        else
                        {
                            int maxVehicles = Normal_Max;
                            if (ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            maxVehicles = Reserved_Max;
                                        }
                                    }
                                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            maxVehicles = Reserved_Max;
                                        }
                                    }
                                }
                            }
                            SaveVehicle(_cInfo, player, attachedVehicle, maxVehicles);
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
            if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= Command_Cost)
            {
                TeleVehicle(_cInfo, _player, _vehicleId);
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall5", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    if (!PersistentOperations.ClaimedByAllyOrSelf(_cInfo.CrossplatformId, _vec3i))
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall6", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                Dictionary<int, string[]> vehicles;
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Count > 0)
                {
                    vehicles = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles;
                }
                else
                {
                    vehicles = new Dictionary<int, string[]>();
                }
                if (!vehicles.ContainsKey(_attachedVehicle.entityId))
                {
                    if (vehicles.Count < _max)
                    {
                        vehicles.Add(_attachedVehicle.entityId, new string[] { _attachedVehicle.EntityClass.entityClassName, new DateTime().ToString() });
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles = vehicles;
                        Phrases.Dict.TryGetValue("VehicleRecall7", out string phrase);
                        phrase = phrase.Replace("{Type}", _attachedVehicle.EntityClass.entityClassName);
                        phrase = phrase.Replace("{Value}", _attachedVehicle.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall8", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("VehicleRecall9", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleport.SaveVehicle: {0}", e.Message));
            }
        }

        public static void RemoveVehicle(ClientInfo _cInfo, string _vehicleId)
        {
            int.TryParse(_vehicleId, out int id);
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.ContainsKey(id))
            {
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Remove(id);
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("VehicleRecall15", out string phrase);
                phrase = phrase.Replace("{Id}", _vehicleId);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall13", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void TeleVehicle(ClientInfo _cInfo, Entity _player, int _vehicleId)
        {
            Entity entity = PersistentOperations.GetEntity(_vehicleId);
            if (entity != null && entity is EntityVehicle)
            {
                if (entity.AttachedToEntity != null)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall10", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityVehicle vehicle = (EntityVehicle)entity;
                int x = (int)vehicle.position.x;
                int y = (int)vehicle.position.x;
                int z = (int)vehicle.position.x;
                if ((_player.position.x - x) * (_player.position.x - x) + (_player.position.z - z) * (_player.position.z - z) <= Distance * Distance)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.RemoveCurrency(_cInfo.PlatformId.ReadablePlatformUserIdentifier, Command_Cost);
                    }
                    int delay = Delay_Between_Uses;
                    Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, y, z);
                    if (chunk == null)
                    {
                        chunk = (Chunk)GameManager.Instance.World.ChunkCache.GetChunkSync(x, y, z);
                        if (chunk != null)
                        {
                            vehicle.SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            delay = Delay_Between_Uses / 2;
                                        }
                                    }
                                }
                                else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        delay = Delay_Between_Uses / 2;
                                    }
                                }
                            }
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles[_vehicleId] = new string[] { vehicle.EntityClass.entityClassName, DateTime.Now.AddMinutes(delay).ToString() };
                            Phrases.Dict.TryGetValue("VehicleRecall11", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Chunk not found"));
                        }
                    }
                    else
                    {
                        vehicle.SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles[_vehicleId] = new string[] { vehicle.EntityClass.entityClassName, DateTime.Now.AddMinutes(delay).ToString() };
                        Phrases.Dict.TryGetValue("VehicleRecall11", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    PersistentContainer.DataChange = true;
                }
                else
                {
                    Phrases.Dict.TryGetValue("VehicleRecall12", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall14", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
