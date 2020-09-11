using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false;
        public static string Command30 = "trackanimal", Command31 = "track";
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 40, Maximum_Spawn_Radius = 60, Command_Cost = 0;
        public static string Animal_List = "82,83,84,85";
        private static Random rnd = new Random();

        public static void Exec(ClientInfo _cInfo)
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
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
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

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
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
                Phrases.Dict.TryGetValue(391, out string _phrase391);
                _phrase391 = _phrase391.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase391 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
            if (_currentCoins >= Command_Cost)
            {
                GiveAnimals(_cInfo);
            }
            else
            {
                Phrases.Dict.TryGetValue(392, out string _phrase392);
                _phrase392 = _phrase392.Replace("{CoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase392 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void GiveAnimals(ClientInfo _cInfo)
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
                int _newId;
                int.TryParse(_animalId[_r], out _newId);
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
                Phrases.Dict.TryGetValue(393, out string _phrase393);
                _phrase393 = _phrase393.Replace("{Radius}", _nextRadius.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase393 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                Phrases.Dict.TryGetValue(394, out string _phrase394);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase394 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}