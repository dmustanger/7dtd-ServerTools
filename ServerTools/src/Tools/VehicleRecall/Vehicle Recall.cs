using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleRecall
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Delay_Between_Uses = 120, Distance = 50, Command_Cost = 0;
        public static string Command_vehicle = "vehicle";

        private static AccessTools.FieldRef<VehicleManager, List<EntityVehicle>> vehiclesActive = AccessTools.FieldRefAccess<VehicleManager, List<EntityVehicle>>("vehiclesActive");
        private static AccessTools.FieldRef<VehicleManager, List<EntityCreationData>> vehiclesUnloaded = AccessTools.FieldRefAccess<VehicleManager, List<EntityCreationData>>("vehiclesUnloaded");

        public static void List(ClientInfo _cInfo)
        {
            bool found = false;
            List<Entity> entityList = GameManager.Instance.World.Entities.list;
            if (entityList != null)
            {
                for (int i = 0; i < entityList.Count; i++)
                {
                    Entity entity = entityList[i];
                    if (entity != null && entity is EntityVehicle)
                    {
                        EntityVehicle vehicle = (EntityVehicle)entity;
                        if (vehicle.IsOwner(_cInfo.CrossplatformId))
                        {
                            found = true;
                            Phrases.Dict.TryGetValue("VehicleRecall1", out string phrase);
                            phrase = phrase.Replace("{Id}", vehicle.entityId.ToString());
                            phrase = phrase.Replace("{Type}", vehicle.EntityClass.entityClassName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            List<EntityCreationData> vehicleListData = vehiclesUnloaded(VehicleManager.Instance);
            if (vehicleListData != null && vehicleListData.Count > 0)
            {
                for (int i = 0; i < vehicleListData.Count; i++)
                {
                    EntityVehicle vehicle = EntityFactory.CreateEntity(vehicleListData[i]) as EntityVehicle;
                    if (vehicle.IsOwner(_cInfo.CrossplatformId))
                    {
                        found = true;
                        Phrases.Dict.TryGetValue("VehicleRecall1", out string phrase);
                        phrase = phrase.Replace("{Id}", vehicle.entityId.ToString());
                        phrase = phrase.Replace("{Type}", vehicle.EntityClass.entityClassName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            if (!found)
            {
                Phrases.Dict.TryGetValue("VehicleRecall2", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _vehicle)
        {
            try
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (!int.TryParse(_vehicle, out int vehicleId))
                    {
                        Phrases.Dict.TryGetValue("VehicleRecall3", out string phrase1);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    if (Delay_Between_Uses < 1)
                    {
                        if (Command_Cost >= 1 && Wallet.IsEnabled)
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
                        DateTime lastVehicle = DateTime.Now;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle != null)
                        {
                            lastVehicle = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle;
                        }
                        if (DateTime.Now >= lastVehicle)
                        {
                            if (Command_Cost >= 1 && Wallet.IsEnabled)
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
                            Phrases.Dict.TryGetValue("VehicleRecall4", out string phrase2);
                            phrase2 = phrase2.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase2 = phrase2.Replace("{Command_vehicle}", Command_vehicle);
                            phrase2 = phrase2.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            TimeSpan varTime = lastVehicle - DateTime.Now;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int timepassed = (int)fractionalMinutes;
                            phrase2 = phrase2.Replace("{TimeRemaining}", timepassed.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            int currency = 0;
            if (Wallet.IsEnabled)
            {
                currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
            }
            if (currency >= Command_Cost)
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
            Entity entity = GeneralFunction.GetEntity(_vehicleId);
            if (entity != null && entity is EntityVehicle)
            {
                EntityVehicle vehicle = (EntityVehicle)entity;
                if (!vehicle.IsOwner(_cInfo.CrossplatformId))
                {
                    Phrases.Dict.TryGetValue("VehicleRecall6", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (entity.AttachedToEntity != null)
                {
                    Phrases.Dict.TryGetValue("VehicleRecall7", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (entity.GetDistance(_player) <= Distance)
                {
                    if (Command_Cost >= 1 && Wallet.IsEnabled)
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
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle = DateTime.Now.AddMinutes(delay);
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("VehicleRecall8", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                else
                {
                    Phrases.Dict.TryGetValue("VehicleRecall9", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            else
            {
                List<EntityVehicle> vehicleList = vehiclesActive(VehicleManager.Instance);
                if (vehicleList != null && vehicleList.Count > 0)
                {
                    for (int i = 0; i < vehicleList.Count; i++)
                    {
                        if (!vehicleList[i].IsOwner(_cInfo.CrossplatformId) || vehicleList[i].entityId != _vehicleId)
                        {
                            continue;
                        }
                        else if (vehicleList[i].GetDistance(_player) <= Distance)
                        {
                            if (Command_Cost >= 1 && Wallet.IsEnabled)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                            }
                            int delay = Delay_Between_Uses;
                            vehicleList[i].SetPosition(new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1));
                            if (Delay_Between_Uses > 0 && ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
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
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle = DateTime.Now.AddMinutes(delay);
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("VehicleRecall8", out string phrase2);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("VehicleRecall9", out string phrase2);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
                List<EntityCreationData> vehicleListData = vehiclesUnloaded(VehicleManager.Instance);
                if (vehicleListData != null && vehicleListData.Count > 0)
                {
                    for (int i = 0; i < vehicleListData.Count; i++)
                    {
                        EntityVehicle vehicle = EntityFactory.CreateEntity(vehicleListData[i]) as EntityVehicle;
                        if (!vehicle.IsOwner(_cInfo.CrossplatformId) || vehicle.entityId != _vehicleId)
                        {
                            continue;
                        }
                        else if (Vector3.Distance(vehicleListData[i].pos, _player.position) <= Distance)
                        {
                            if (Command_Cost >= 1 && Wallet.IsEnabled)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                            }
                            int delay = Delay_Between_Uses;
                            vehicle.position = new Vector3(_player.position.x + 1, _player.position.y, _player.position.z + 1);
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
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle = DateTime.Now.AddMinutes(delay);
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("VehicleRecall8", out string phrase2);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("VehicleRecall9", out string phrase2);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
            }
        }
    }
}
