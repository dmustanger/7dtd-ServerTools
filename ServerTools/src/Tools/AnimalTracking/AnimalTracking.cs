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

        private static readonly System.Random rnd = new System.Random();

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
                    DateTime _lastAnimals = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal != null)
                    {
                        _lastAnimals = PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal;
                    }
                    TimeSpan varTime = DateTime.Now - _lastAnimals;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                            if (DateTime.Now < _dt)
                            {
                                int _delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, _timepassed, Delay_Between_Uses);
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
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("AnimalTracking1", out string _phrase);
                    _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                if (_currentCoins >= Command_Cost)
                {
                    GiveAnimals(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("AnimalTracking2", out string _phrase);
                    _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    int r = rnd.Next(1, animalId.Count + 1);
                    int.TryParse(animalId[r], out int randomId);
                    int nextRadius = rnd.Next(minRad, maxRad + 1);
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
                            EntityPlayer entityPlayer = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                            if (entityPlayer != null)
                            {
                                if (SpawnAnimal(_cInfo, entityPlayer, nextRadius, randomId))
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal = DateTime.Now;
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
                    Phrases.Dict.TryGetValue("AnimalTracking4", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Animals.GiveAnimals: {0}", e.Message));
            }
        }

        public static bool SpawnAnimal(ClientInfo _cInfo, EntityPlayer entityPlayer, int _radius, int _animalId)
        {
            PersistentOperations.EntityId.TryGetValue(_animalId, out int entityId);
            bool posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(entityPlayer.position, 15, out int x, out int y, out int z, new Vector3(_radius, _radius, _radius), true);
            if (!posFound)
            {
                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(entityPlayer.position, 15, out x, out y, out z, new Vector3(_radius + 5, _radius + 50, _radius + 5), true);
            }
            if (posFound)
            {
                Entity entity = EntityFactory.CreateEntity(entityId, new Vector3(x, y, z));
                GameManager.Instance.World.SpawnEntityInWorld(entity);
                Phrases.Dict.TryGetValue("AnimalTracking3", out string phrase);
                phrase = phrase.Replace("{Radius}", _radius.ToString());
                float angle = Vector3.Angle(entityPlayer.position, new Vector3(x, y, z));
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