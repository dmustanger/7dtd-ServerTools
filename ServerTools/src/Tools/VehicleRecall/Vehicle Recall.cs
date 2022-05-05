using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleRecall
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Delay_Between_Uses = 120, Distance = 50, Normal_Max = 2, Reserved_Max = 4, Command_Cost = 0;
        public static string Command_vehicle = "vehicle", Command_vehicle_save = "vehicle save", Command_vehicle_del = "vehicle del";

        private static AccessTools.FieldRef<VehicleManager, List<EntityVehicle>> vehiclesActive = AccessTools.FieldRefAccess<VehicleManager, List<EntityVehicle>>("vehiclesActive");
        private static AccessTools.FieldRef<VehicleManager, List<EntityCreationData>> vehiclesUnloaded = AccessTools.FieldRefAccess<VehicleManager, List<EntityCreationData>>("vehiclesUnloaded");

        public static void List(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Count > 0)
            {
                Dictionary<int, string[]> vehicles = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles;
                foreach (var vehicle in vehicles)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall1", out string phrase);
                    phrase = phrase.Replace("{Id}", vehicle.Key.ToString());
                    phrase = phrase.Replace("{Type}", vehicle.Value[0]);
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
                    if (!int.TryParse(_vehicle, out int vehicleId))
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost(_cInfo, player, vehicleId);
                        }
                        else
                        {
                            TeleVehicle(_cInfo, player, vehicleId);
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.ContainsKey(vehicleId))
                        {
                            string[] vehicleInfo = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles[vehicleId];
                            DateTime.TryParse(vehicleInfo[1], out DateTime delay);
                            if (DateTime.Now >= delay)
                            {
                                if (Wallet.IsEnabled && Command_Cost >= 1)
                                {
                                    CommandCost(_cInfo, player, vehicleId);
                                }
                                else
                                {
                                    TeleVehicle(_cInfo, player, vehicleId);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("VehicleRecall4", out string phrase);
                                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                phrase = phrase.Replace("{Command_vehicle}", Command_vehicle);
                                phrase = phrase.Replace("{DelayBetweenUses}", delay.ToString());
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

        public static void TeleVehicle(ClientInfo _cInfo, Entity _player, int _vehicleId)
        {
            Entity entity = PersistentOperations.GetEntity(_vehicleId);
            if (entity != null && entity is EntityVehicle)
            {
                if (entity.AttachedToEntity != null)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall10", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                EntityVehicle vehicle = (EntityVehicle)entity;
                if (entity.GetDistance(_player) <= Distance)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                    }
                    int delay = Delay_Between_Uses;
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
                List<EntityVehicle> vehicleList = vehiclesActive(VehicleManager.Instance);
                if (vehicleList != null && vehicleList.Count > 0)
                {
                    for (int i = 0; i < vehicleList.Count; i++)
                    {
                        if (vehicleList[i].entityId == _vehicleId)
                        {
                            if (vehicleList[i].GetDistance(_player) <= Distance)
                            {
                                if (Wallet.IsEnabled && Command_Cost >= 1)
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                                }
                                int delay = Delay_Between_Uses;
                                vehicleList[i].SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
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
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles[_vehicleId] = new string[] { vehicleList[i].EntityClass.entityClassName, DateTime.Now.AddMinutes(delay).ToString() };
                                Phrases.Dict.TryGetValue("VehicleRecall11", out string phrase2);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                PersistentContainer.DataChange = true;
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("VehicleRecall12", out string phrase2);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return;
                        }
                    }
                }
                List<EntityCreationData> vehicleListData = vehiclesUnloaded(VehicleManager.Instance);
                if (vehicleListData != null && vehicleListData.Count > 0)
                {
                    for (int i = 0; i < vehicleListData.Count; i++)
                    {
                        if (vehicleListData[i].id == _vehicleId)
                        {
                            if (Vector3.Distance(vehicleListData[i].pos, _player.position) <= Distance)
                            {
                                if (Wallet.IsEnabled && Command_Cost >= 1)
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                                }
                                int delay = Delay_Between_Uses;
                                vehicleListData[i].pos = new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1);
                                EntityVehicle vehicle = EntityFactory.CreateEntity(vehicleListData[i]) as EntityVehicle;
                                vehiclesActive(VehicleManager.Instance).Add(vehicle);
                                GameManager.Instance.World.SpawnEntityInWorld(vehicle);
                                vehiclesUnloaded(VehicleManager.Instance).RemoveAt(i);
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
                                Phrases.Dict.TryGetValue("VehicleRecall11", out string phrase3);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                PersistentContainer.DataChange = true;
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("VehicleRecall12", out string phrase3);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return;
                        }
                    }
                }
                Phrases.Dict.TryGetValue("VehicleRecall14", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SaveVehicle(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (player.AttachedToEntity == null)
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall16", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    if (Inside_Claim)
                    {
                        World world = GameManager.Instance.World;
                        Vector3 position = player.position;
                        int x = (int)position.x;
                        int y = (int)position.y;
                        int z = (int)position.z;
                        Vector3i vec3i = new Vector3i(x, y, z);
                        EnumLandClaimOwner claimOwner = PersistentOperations.ClaimedByWho(_cInfo.CrossplatformId, vec3i);
                        if (claimOwner != EnumLandClaimOwner.Self && claimOwner != EnumLandClaimOwner.Ally)
                        {
                            Phrases.Dict.TryGetValue("VehicleRecall6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
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
                    Dictionary<int, string[]> vehicles;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles.Count > 0)
                    {
                        vehicles = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles;
                    }
                    else
                    {
                        vehicles = new Dictionary<int, string[]>();
                    }
                    EntityVehicle vehicle = (EntityVehicle)player.AttachedToEntity;
                    if (!vehicles.ContainsKey(vehicle.entityId))
                    {
                        if (vehicles.Count < maxVehicles)
                        {
                            vehicles.Add(vehicle.entityId, new string[] { vehicle.EntityClass.entityClassName, new DateTime().ToString() });
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vehicles = vehicles;
                            Phrases.Dict.TryGetValue("VehicleRecall7", out string phrase);
                            phrase = phrase.Replace("{Type}", vehicle.EntityClass.entityClassName);
                            phrase = phrase.Replace("{Value}", vehicle.entityId.ToString());
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleport.SaveVehicle: {0}", e.Message));
            }
        }

        public static void RemoveVehicle(ClientInfo _cInfo, string _vehicleId)
        {
            if (int.TryParse(_vehicleId, out int id))
            {
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
            else
            {
                Phrases.Dict.TryGetValue("VehicleRecall3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
