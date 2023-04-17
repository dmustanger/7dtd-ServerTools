using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class AnimalTracking
    {
        public static bool IsEnabled = false;
        public static string Command_trackanimal = "trackanimal", Command_track = "track";
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 40, Maximum_Spawn_Radius = 60, Command_Cost = 0;
        public static string Animal_Ids = "85,86,87,88";

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        GiveAnimals(_cInfo);
                    }
                }
                else
                {
                    DateTime lastAnimals = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastAnimal != null)
                    {
                        lastAnimals = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastAnimal;
                    }
                    TimeSpan varTime = DateTime.Now - lastAnimals;
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
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay);
                                    return;
                                }
                            }
                        }
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        int delay = Delay_Between_Uses / 2;
                        Time(_cInfo, timepassed, delay);
                        return;
                    }
                    Time(_cInfo, timepassed, Delay_Between_Uses);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.Exec: {0}", e.Message));
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        GiveAnimals(_cInfo);
                    }
                }
                else
                {
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("AnimalTracking1", out string phrase);
                    phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.Time: {0}", e.Message));
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            try
            {
                int currency = 0;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (currency >= Command_Cost)
                {
                    GiveAnimals(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("AnimalTracking2", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.CommandCost: {0}", e.Message));
            }
        }

        public static void GiveAnimals(ClientInfo _cInfo)
        {
            try
            {
                if (Animal_Ids.Length > 0)
                {
                    string[] animalList = { };
                    List<string> animalId = new List<string>();
                    if (Animal_Ids.Contains(","))
                    {
                        string[] animals = Animal_Ids.Split(',');
                        for (int i = 0; i < animals.Length; i++)
                        {
                            animalId.Add(animals[i]);
                        }
                    }
                    else
                    {
                        animalId.Add(Animal_Ids);
                    }
                    int minRad = 0;
                    int maxRad = 0;
                    if (Maximum_Spawn_Radius < Minimum_Spawn_Radius)
                    {
                        minRad = 40;
                        maxRad = 60;
                    }
                    else
                    {
                        minRad = Minimum_Spawn_Radius;
                        maxRad = Maximum_Spawn_Radius;
                    }
                    int r = new System.Random().Next(1, animalId.Count + 1);
                    int.TryParse(animalId[r], out int randomId);
                    int nextRadius = new System.Random().Next(minRad, maxRad + 1);
                    Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Dict.Keys;
                    int counter = 1;
                    foreach (int i in entityTypesCollection)
                    {
                        EntityClass eClass = EntityClass.list[i];
                        if (!eClass.bAllowUserInstantiate)
                        {
                            continue;
                        }
                        if (randomId == counter)
                        {
                            EntityPlayer entityPlayer = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                            if (entityPlayer != null)
                            {
                                if (SpawnAnimal(_cInfo, entityPlayer, nextRadius, randomId))
                                {
                                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                                    {
                                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastAnimal = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                    break;
                                }
                            }
                        }
                        counter++;
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("AnimalTracking4", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.GiveAnimals: {0}", e.Message));
            }
        }

        public static bool SpawnAnimal(ClientInfo _cInfo, EntityPlayer _entityPlayer, int _radius, int _animalId)
        {
            GeneralOperations.EntityId.TryGetValue(_animalId, out int entityId);
            bool posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_entityPlayer.position, 15, out int x, out int y, out int z, new Vector3(_radius, _radius, _radius), true);
            if (!posFound)
            {
                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_entityPlayer.position, 15, out x, out y, out z, new Vector3(_radius + 10, _radius + 50, _radius + 10), true);
            }
            if (posFound)
            {
                Entity entity = EntityFactory.CreateEntity(entityId, new Vector3(x, y, z));
                GameManager.Instance.World.SpawnEntityInWorld(entity);
                Phrases.Dict.TryGetValue("AnimalTracking3", out string phrase);
                phrase = phrase.Replace("{Radius}", _radius.ToString());
                float angle = Vector2.Angle(_entityPlayer.position, new Vector2(x, z));
                if (angle >= 0 && angle < 45)
                {
                    phrase = phrase.Replace("{Direction}", "North");
                }
                else if (angle >= 45 && angle < 135)
                {
                    phrase = phrase.Replace("{Direction}", "East");
                }
                else if (angle >= 135 && angle < 225)
                {
                    phrase = phrase.Replace("{Direction}", "South");
                }
                else if (angle >= 225 && angle < 315)
                {
                    phrase = phrase.Replace("{Direction}", "West");
                }
                else if (angle >= 315 && angle <= 360)
                {
                    phrase = phrase.Replace("{Direction}", "North");
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return true;
            }
            else
            {
                Phrases.Dict.TryGetValue("AnimalTracking5", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return false;
            }
        }
    }
}