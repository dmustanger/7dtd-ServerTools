using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false, Always_Show_Response = false;
        public static string Command30 = "trackanimal", Command31 = "track";
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 40, Maximum_Spawn_Radius = 60, Command_Cost = 0;
        public static string Animal_List = "81,82,83,84";
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
                DateTime _lastAnimals = PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal;
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
                string _phrase601;
                if (!Phrases.Dict.TryGetValue(601, out _phrase601))
                {
                    _phrase601 = " you have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                _phrase601 = _phrase601.Replace("{TimeRemaining}", _timeleft.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase601 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                GiveAnimals(_cInfo);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                string _phrase715;
                if (!Phrases.Dict.TryGetValue(715, out _phrase715))
                {
                    _phrase715 = " you have tracked down an animal to within {Radius} metres.";
                }
                _phrase715 = _phrase715.Replace("{Radius}", _nextRadius.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase715 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastAnimal = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " animal list is empty. Contact an administrator.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}