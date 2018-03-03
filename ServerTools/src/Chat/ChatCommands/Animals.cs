using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool Always_Show_Response = false;
        public static int Delay_Between_Uses = 60;
        public static int Minimum_Spawn_Radius = 20;
        public static int Maximum_Spawn_Radius = 30;
        public static List<string> entities = new List<string>();
        public static string Animal_List = "59,60,61";

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (IsEnabled)
            {
                if (Delay_Between_Uses < 1)
                {
                    _GiveAnimals(_cInfo, _announce);
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p == null || p.LastAnimals == null)
                    {
                        _GiveAnimals(_cInfo, _announce);
                    }
                    else
                    {
                        TimeSpan varTime = DateTime.Now - p.LastAnimals;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            _GiveAnimals(_cInfo, _announce);
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
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), "Server", false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase601), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static void _GiveAnimals(ClientInfo _cInfo, bool _announce)
        {
            entities.Clear();
            var a = Animal_List.Split(',');
            foreach (var animal in a)
            {
                entities.Add(animal);
            }
            if (Maximum_Spawn_Radius < Minimum_Spawn_Radius)
            {
                Random rnd = new Random();
                int minRad = 20;
                int maxRad = 30;
                int randomAnimal = rnd.Next(entities.Count);
                string nextAnimal = entities[randomAnimal - 1];
                int nextRadius = rnd.Next(minRad, maxRad);
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, nextRadius, nextAnimal), (ClientInfo)null);                
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", Config.Chat_Response_Color, _cInfo.playerName, nextRadius), "Server", false, "", false);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    return;
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", Config.Chat_Response_Color, _cInfo.playerName, nextRadius), "Server", false, "", false));
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
            else
            {
                Random rnd = new Random();
                int randomAnimal = rnd.Next(entities.Count);
                string nextAnimal = entities[randomAnimal - 1];
                int nextRadius = rnd.Next(Minimum_Spawn_Radius, Maximum_Spawn_Radius);
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, nextRadius, nextAnimal), _cInfo);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", Config.Chat_Response_Color, _cInfo.playerName, nextRadius), "Server", false, "", false);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    return;
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", Config.Chat_Response_Color, _cInfo.playerName, nextRadius), "Server", false, "", false));
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }               
            }
        }
    }
}