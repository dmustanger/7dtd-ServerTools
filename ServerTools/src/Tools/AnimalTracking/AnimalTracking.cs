using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    public class AnimalTracking
    {
        public static bool IsEnabled = false;
        public static string Command_animal = "animal", Command_track = "track";
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 40, Maximum_Spawn_Radius = 60, Command_Cost = 0;
        public static string Animal_Ids = "87,88,89,90";

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Animal_Ids == "")
                {
                    return;
                }
                EntityPlayer entityPlayer = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (entityPlayer == null)
                {
                    return;
                }
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, entityPlayer);
                    }
                    else
                    {
                        GiveAnimals(_cInfo, entityPlayer);
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
                                    Time(_cInfo, timepassed, delay, entityPlayer);
                                    return;
                                }
                            }
                            else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                            {
                                if (DateTime.Now < dt)
                                {
                                    int delay = Delay_Between_Uses / 2;
                                    Time(_cInfo, timepassed, delay, entityPlayer);
                                    return;
                                }
                            }
                        }
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        int delay = Delay_Between_Uses / 2;
                        Time(_cInfo, timepassed, delay, entityPlayer);
                        return;
                    }
                    Time(_cInfo, timepassed, Delay_Between_Uses, entityPlayer);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Animals.Exec: {0}", e.Message);
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay, EntityPlayer entityPlayer)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Command_Cost >= 1)
                    {
                        CommandCost(_cInfo, entityPlayer);
                    }
                    else
                    {
                        GiveAnimals(_cInfo, entityPlayer);
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
                Log.Out("[SERVERTOOLS] Error in Animals.Time: {0}", e.Message);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, EntityPlayer entityPlayer)
        {
            try
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
                    GiveAnimals(_cInfo, entityPlayer);
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
                Log.Out("[SERVERTOOLS] Error in Animals.CommandCost: {0}", e.Message);
            }
        }

        public static void GiveAnimals(ClientInfo _cInfo, EntityPlayer entityPlayer)
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
                        if (randomId == counter)
                        {
                            if (SpawnAnimal(_cInfo, entityPlayer, nextRadius, randomId))
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastAnimal = DateTime.Now;
                                PersistentContainer.DataChange = true;
                                break;
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
                Log.Out("[SERVERTOOLS] Error in Animals.GiveAnimals: {0}", e.Message);
            }
        }

        public static bool SpawnAnimal(ClientInfo _cInfo, EntityPlayer _entityPlayer, int _radius, int _animalId)
        {
            bool posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_entityPlayer.position, 15, out int x, out int y, out int z, new Vector3(_radius, _radius, _radius), true);
            if (!posFound)
            {
                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_entityPlayer.position, 15, out x, out y, out z, new Vector3(_radius + 10, _radius + 50, _radius + 10), true);
            }
            if (posFound)
            {
                int count = 1;
                var entities = EntityClass.list.Dict.ToArray();
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].Value.userSpawnType != EntityClass.UserSpawnType.None)
                    {
                        if (count == _animalId)
                        {
                            Entity entity = EntityFactory.CreateEntity(entities[i].Key, new Vector3(x, y, z));
                            GameManager.Instance.World.SpawnEntityInWorld(entity);
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Spawned a '{0}' at '{1} x, {2} y, {3} z'", entity.EntityClass.entityClassName, x, y, z);
                            Phrases.Dict.TryGetValue("AnimalTracking3", out string phrase);
                            phrase = phrase.Replace("{Radius}", _radius.ToString());
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                            return true;
                        }
                        count++;
                    }
                }
                Log.Out("[SERVERTOOLS] Unable to find entity matching id '{0}'", _animalId);
                return false;
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