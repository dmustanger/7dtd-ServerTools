using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Animals
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool AlwaysShowResponse = false;
        public static int DelayBetweenUses = 60;
        public static int MinimumSpawnRadius = 10;
        public static int MaximumSpawnRadius = 20;
        public static List<int> entities = new List<int>();
        public static string animalList = "59,60,61";

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (IsEnabled)
            {
                if (DelayBetweenUses < 1)
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
                        if (_timepassed > DelayBetweenUses)
                        {
                            _GiveAnimals(_cInfo, _announce);
                        }
                        else
                        {
                            int _timeleft = DelayBetweenUses - _timepassed;
                            string _phrase601;
                            if (!Phrases.Dict.TryGetValue(601, out _phrase601))
                            {
                                _phrase601 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                            }
                            _phrase601 = _phrase601.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase601), "Server", false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase601), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static void _GiveAnimals(ClientInfo _cInfo, bool _announce)
        {
            var a = animalList.Split(',');
            foreach (var animal in a)
            {
                entities.Add(Convert.ToInt32(animal));
            }
            if (MaximumSpawnRadius < MinimumSpawnRadius)
            {
                Random rnd = new Random();
                int minRad = 10;
                int maxRad = 30;
                int randomAnimal = rnd.Next(entities.Count);
                int nextAnimal = entities[randomAnimal - 1];
                int nextRadius = rnd.Next(minRad, maxRad);
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, nextRadius, nextAnimal), _cInfo);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", CustomCommands.ChatColor, _cInfo.playerName, nextRadius), "Server", false, "", false);
                    return;
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", CustomCommands.ChatColor, _cInfo.playerName, nextRadius), "Server", false, "", false));
                }
            }
            else
            {
                Random rnd = new Random();
                int randomAnimal = rnd.Next(entities.Count);
                int nextAnimal = entities[randomAnimal - 1];
                int nextRadius = rnd.Next(MinimumSpawnRadius, MaximumSpawnRadius);
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} {1} @ {2}", _cInfo.entityId, nextRadius, nextAnimal), _cInfo);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", CustomCommands.ChatColor, _cInfo.playerName, nextRadius), "Server", false, "", false);
                    return;
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has tracked down an animal to within {2} metres[-]", CustomCommands.ChatColor, _cInfo.playerName, nextRadius), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].LastAnimals = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
        }
    }
}