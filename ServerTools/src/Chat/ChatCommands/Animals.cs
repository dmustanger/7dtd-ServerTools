using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false, Always_Show_Response = false;
        public static int Delay_Between_Uses = 60, Minimum_Spawn_Radius = 20, Maximum_Spawn_Radius = 30;
        public static List<string> entities = new List<string>();
        public static string Animal_List = "59,60,61";
        private static Random rnd = new Random();

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                GiveAnimals(_cInfo, _announce);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastAnimals == null)
                {
                    GiveAnimals(_cInfo, _announce);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastAnimals;
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
                                _donator = true;
                                int _newDelay = Delay_Between_Uses / 2;
                                if (_timepassed >= _newDelay)
                                {
                                    GiveAnimals(_cInfo, _announce);
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase601;
                                    if (!Phrases.Dict.TryGetValue(601, out _phrase601))
                                    {
                                        _phrase601 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                                    }
                                    _phrase601 = _phrase601.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), Config.Server_Response_Name, false, "", false);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            GiveAnimals(_cInfo, _announce);
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase601;
                            if (!Phrases.Dict.TryGetValue(601, out _phrase601))
                            {
                                _phrase601 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                            }
                            _phrase601 = _phrase601.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), Config.Server_Response_Name, false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        public static void AnimalList()
        {
            string[] _animalList = Animal_List.Split(',').ToArray();
            for (int i = 0; i < _animalList.Length; i++)
            {
                string _ent = _animalList[i];
                entities.Add(_ent);
            }
        }

        public static void GiveAnimals(ClientInfo _cInfo, bool _announce)
        {
            if (entities.Count > 0)
            {
                int minRad = 0;
                int maxRad = 0;
                if (Maximum_Spawn_Radius < Minimum_Spawn_Radius)
                {
                    minRad = 20;
                    maxRad = 30;
                }
                else
                {
                    minRad = Minimum_Spawn_Radius;
                    maxRad = Maximum_Spawn_Radius;
                }
                var r = rnd.Next(entities.Count);
                string _newId = entities[r];
                var _id = int.Parse(_newId);
                int _nextRadius = rnd.Next(minRad, maxRad + 1);
                Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Keys;

                int counter = 1;
                foreach (int i in entityTypesCollection)
                {
                    EntityClass eClass = EntityClass.list[i];
                    if (!eClass.bAllowUserInstantiate)
                    {
                        continue;
                    }
                    if (_id == counter)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, _nextRadius, _id), (ClientInfo)null);
                    }
                    counter++;
                }
                if (_announce)
                {
                    string _phrase715;
                    if (!Phrases.Dict.TryGetValue(715, out _phrase715))
                    {
                        _phrase715 = "{PlayerName} has tracked down an animal to within {Radius} metres.";
                    }
                    _phrase715 = _phrase715.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase715 = _phrase715.Replace("{Radius}", _nextRadius.ToString());
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase715), Config.Server_Response_Name, false, "", false);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                else
                {
                    string _phrase715;
                    if (!Phrases.Dict.TryGetValue(715, out _phrase715))
                    {
                        _phrase715 = "{PlayerName} has tracked down an animal to within {Radius} metres.";
                    }
                    _phrase715 = _phrase715.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase715 = _phrase715.Replace("{Radius}", _nextRadius.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase715), Config.Server_Response_Name, false, "ServerTools", false));
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
            else
            {
                AnimalList();
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Animal list was empty. Attempting to rebuild. Type /track again. If this repeats, contact an administrator.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}