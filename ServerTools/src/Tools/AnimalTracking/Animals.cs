using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false;
        public static string Command_trackanimal = "trackanimal", Command_track = "track";
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 40, Maximum_Spawn_Radius = 60, Command_Cost = 0;
        public static string Animal_List = "82,83,84,85";

        private static readonly Random rnd = new Random();

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
                if (Animal_List.Length > 0)
                {
                    string[] _animalList = { };
                    if (Animal_List.Contains(","))
                    {
                        if (Animal_List.Contains(" "))
                        {
                            Animal_List.Replace(" ", "");
                        }
                        _animalList = Animal_List.Split(',').ToArray();
                    }
                    else if (Animal_List.Contains(" "))
                    {
                        _animalList = Animal_List.Split(' ').ToArray();
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
                    List<string> _animalId = new List<string>();
                    foreach (string i in _animalList)
                    {
                        _animalId.Add(i);
                    }
                    int _r = rnd.Next(_animalId.Count);
                    int.TryParse(_animalId[_r], out int _newId);
                    int _nextRadius = rnd.Next(minRad, maxRad + 1);
                    Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Dict.Keys;
                    int counter = 1;
                    foreach (int i in entityTypesCollection)
                    {
                        EntityClass eClass = EntityClass.list[i];
                        if (!eClass.bAllowUserInstantiate)
                        {
                            continue;
                        }
                        if (_newId == counter)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, _nextRadius, _newId), (ClientInfo)null);
                        }
                        counter++;
                    }
                    Phrases.Dict.TryGetValue("AnimalTracking3", out string _phrase);
                    _phrase = _phrase.Replace("{Radius}", _nextRadius.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal = DateTime.Now;
                    PersistentContainer.DataChange = true;
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
    }
}