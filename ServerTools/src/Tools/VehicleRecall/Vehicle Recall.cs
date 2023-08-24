using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
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
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
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
                        DateTime lastVehicle = DateTime.Now;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle != null)
                        {
                            lastVehicle = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVehicle;
                        }
                        TimeSpan varTime = DateTime.Now - lastVehicle;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, player, timepassed, delay, vehicleId);
                                        return;
                                    }
                                }
                                else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, player, timepassed, delay, vehicleId);
                                        return;
                                    }
                                }
                            }
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                        {
                            int delay = Delay_Between_Uses / 2;
                            Time(_cInfo, player, timepassed, delay, vehicleId);
                            return;
                        }
                        Time(_cInfo, player, timepassed, Delay_Between_Uses, vehicleId);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleport.Exec: {0}", e.Message));
            }
        }

        public static void Time(ClientInfo _cInfo, EntityPlayer _player, int _timepassed, int _delay, int _vehicleId)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Command_Cost > 0)
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
                    Phrases.Dict.TryGetValue("VehicleRecall4", out string phrase2);
                    phrase2 = phrase2.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase2 = phrase2.Replace("{Command_vehicle}", Command_vehicle);
                    phrase2 = phrase2.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                    phrase2 = phrase2.Replace("{TimeRemaining}", _timepassed.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.Time: {0}", e.Message));
            }
        }

        public static void CommandCost(ClientInfo _cInfo, Entity _player, int _vehicleId)
        {
            int currency = 0, bankCurrency = 0, cost = Command_Cost;
            if (Wallet.IsEnabled)
            {
                currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
            }
            if (Bank.IsEnabled && Bank.Direct_Payment)
            {
                bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
            }
            if (currency + bankCurrency >= cost)
            {
                if (currency > 0)
                {
                    if (currency < cost)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                        cost -= currency;
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    else
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                }
                else
                {
                    Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                }
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
            List<EntityCreationData> vehicleListData = vehiclesUnloaded(VehicleManager.Instance);
            if (vehicleListData != null && vehicleListData.Count > 0)
            {
                for (int i = 0; i < vehicleListData.Count; i++)
                {
                    if (vehicleListData[i].id != _vehicleId)
                    {
                        continue;
                    }
                    if (Vector3.Distance(vehicleListData[i].pos, _player.position) <= Distance)
                    {
                        vehicleListData[i].pos = new Vector3(_player.position.x + 1, _player.position.y + 0.3f, _player.position.z + 1);
                        int delay = Delay_Between_Uses;
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
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                        {
                            delay = Delay_Between_Uses / 2;
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
            Entity entity = GeneralOperations.GetEntity(_vehicleId);
            if (entity != null && entity is EntityVehicle)
            {
                EntityVehicle vehicle = (EntityVehicle)entity;
                if (vehicle == null || !vehicle.IsOwner(_cInfo.CrossplatformId))
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
                    int delay = Delay_Between_Uses;
                    vehicle.SetPosition(new Vector3(_player.position.x, _player.position.y, _player.position.z));
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
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        delay = Delay_Between_Uses / 2;
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
        }
    }
}
