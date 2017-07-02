using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = false;

        public static void GetInfo(ClientInfo _cInfo, bool _announce)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            int _playerCount = ConnectionManager.Instance.ClientCount();
            int _zombies = 0;
            int _animals = 0;
            int _RadiatedZombies = 0;
            int _feralZombies = 0;
            int _supplyCrates = 0;
            int _miniBikes = 0;
            int _stag = 0;
            int _bear = 0;
            int _rabbit = 0;
            int _chicken = 0;
            int _pig = 0;
            int _dog = 0;
            int _vulture = 0;
            int _screamer = 0;
            int _snake = 0;
            int _wolf = 0;
            int _daysUntil7 = 7 - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % 7;
            List<Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                string _name = EntityClass.list[_e.entityClass].entityClassName;
                if (_name == "playerMale" || _name == "playerFemale" || _name == "item" || _name == "fallingBlock" || _name == "Backpack" || _name == "fallingTree" || _name == "npcTraderJimmy" || _name == "npcTraderBob" || _name == "DroppedLootContainer" || _name == "npcTraderRekt" || _name == "invisibleAnimal" || _name == "npcTraderJoel" || _name == "EvisceratedRemains" || _name == "npcTraderHugh")
                {
                    continue;
                }
                else if (_name == "zombieUtilityWorker" || _name == "zombieSkateboarder" || _name == "zombieSnow" || _name == "zombieStripper" || _name == "zombieBusinessMan" || _name == "zombieBurnt" || _name == "zombieOldTimer" || _name == "zombieBiker" || _name == "zombieCheerleader" || _name == "zombieFarmer" || _name == "zombieFemaleFat" || _name == "zombieSoldier" || _name == "zombieFootballPlayer" || _name == "zombieFatHawaiian" || _name == "zombieMarlene" || _name == "zombieSteveCrawler" || _name == "zombieSteve" || _name == "zombieMoe" || _name == "zombieDarlene" || _name == "zombieYo" || _name == "zombieBoe" || _name == "zombieMaleHazmat" || _name == "zombieJoe" || _name == "zombieArlene" || _name == "zombieNurse" || _name == "zombieFatCop" || _name == "zombieSpider") 
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                    }
                    continue;
                }
                else if (_name == "animalRabbit")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _rabbit = _rabbit + 1;
                    }
                    continue;
                }
                else if (_name == "animalChicken")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _chicken = _chicken + 1;
                    }
                    continue;
                }
                else if (_name == "animalStag")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _stag = _stag + 1;
                    }
                    continue;
                }
                else if (_name == "animalBoar")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _pig = _pig + 1;
                    }
                    continue;
                }
                else if (_name == "animalWolf" || _name == "animalDireWolf")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _wolf = _wolf + 1;
                    }
                    continue;
                }
                else if (_name == "animalZombieBear")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _bear = _bear + 1;
                    }
                    continue;
                }
                else if (_name == "animalBear")
                {
                    if (_e.IsAlive())
                    {
                        _animals = _animals + 1;
                        _bear = _bear + 1;
                    }
                    continue;
                }
                else if (_name == "animalZombieDog")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _dog = _dog + 1;
                    }
                    continue;
                }
                else if (_name == "zombieScreamer")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _screamer = _screamer + 1;
                    }
                    continue;
                }
                else if (_name == "zombieScreamerFeral")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _feralZombies = _feralZombies + 1;
                        _screamer = _screamer + 1;
                    }
                    continue;
                }
                else if (_name == "zombieSpiderFeral" || _name == "zombieSteveFeral" || _name == "zombieDarleneFeral" || _name == "zombieFatCopFeral" || _name == "zombieBoeFeral" || _name == "zombieYoFeral" || _name == "zombieFemaleFatFeral" || _name == "zombieMarleneFeral" || _name == "zombieSkateboarderFeral" || _name == "zombieFatHawaiianFeral" || _name == "zombieNurseFeral" || _name == "zombieBusinessManFeral" || _name == "zombieMoeFeral" || _name == "zombieWightFeral" || _name == "zombieSteveCrawlerFeral" || _name == "zombieArleneFeral" || _name == "zombieJoeFeral" || _name == "zombieFarmerFeral" || _name == "zombieUtilityWorkerFeral" || _name == "zombieSnowFeral" || _name == "zombieSoldierFeral" || _name == "zombieCheerleaderFeral" || _name == "zombieOldTimerFeral" || _name == "zombieStripperFeral")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _feralZombies = _feralZombies + 1;
                    }
                    continue;
                }
                else if (_name == "zombieFatCopFeralRadiated" || _name == "zombieWightFeralRadiated" || _name == "zombieSpiderFeralRadiated")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _RadiatedZombies = _RadiatedZombies + 1;
                    }
                    continue;
                }
                else if (_name == "animalZombieVulture")
                {
                    if (_e.IsAlive())
                    {
                        _zombies = _zombies + 1;
                        _vulture = _vulture + 1;
                    }
                    continue;
                }
                else if (_name == "minibike")
                {
                    _miniBikes = _miniBikes + 1;
                    continue;
                }
                else if (_name == "animalSnake")
                {
                    _animals = _animals + 1;
                    _snake = _snake + 1;
                    continue;
                }
                else if (_name == "sc_General")
                {
                    _supplyCrates = _supplyCrates + 1;
                    continue;
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Entity Class Name is: {0}", _name));
                }  
            }
            string _phrase300;
            string _phrase301;
            string _phrase302;
            string _phrase303;
            string _phrase304;
            string _phrase305;
            string _phrase306;
            if (!Phrases.Dict.TryGetValue(300, out _phrase300))
            {
                _phrase300 = "Server FPS: {Fps}";
            }
            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next 7th day is in {DaysUntil7} days";
            }
            if (!Phrases.Dict.TryGetValue(302, out _phrase302))
            {
                _phrase302 = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
            }
            if (!Phrases.Dict.TryGetValue(303, out _phrase303))
            {
                _phrase303 = "Feral Zombies:{Ferals} Radiated Zombies:{Radiated} Dogs:{Dogs} Vultures:{Vultures} Screamers:{Screamers}";
            }
            if (!Phrases.Dict.TryGetValue(304, out _phrase304))
            {
                _phrase304 = "Bears:{Bears} Stags:{Stags} Boars:{Boars} Rabbits:{Rabbits} Chickens:{Chickens} Snakes:{Snakes} Wolves:{Wolves}";
            }
            if (!Phrases.Dict.TryGetValue(305, out _phrase305))
            {
                _phrase305 = "Total Supply Crates:{SupplyCrates} Total Mini Bikes:{MiniBikes}";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next 7th day is today";
            }
            _phrase300 = _phrase300.Replace("{Fps}", _fps);
            _phrase301 = _phrase301.Replace("{DaysUntil7}", _daysUntil7.ToString());
            _phrase302 = _phrase302.Replace("{Players}", _playerCount.ToString());
            _phrase302 = _phrase302.Replace("{Zombies}", _zombies.ToString());
            _phrase302 = _phrase302.Replace("{Animals}", _animals.ToString());
            _phrase303 = _phrase303.Replace("{Ferals}", _feralZombies.ToString());
            _phrase303 = _phrase303.Replace("{Radiated}", _RadiatedZombies.ToString());
            _phrase303 = _phrase303.Replace("{Dogs}", _dog.ToString());
            _phrase303 = _phrase303.Replace("{Vultures}", _vulture.ToString());
            _phrase303 = _phrase303.Replace("{Screamers}", _screamer.ToString());
            _phrase304 = _phrase304.Replace("{Bears}", _bear.ToString());
            _phrase304 = _phrase304.Replace("{Stags}", _stag.ToString());
            _phrase304 = _phrase304.Replace("{Boars}", _pig.ToString());
            _phrase304 = _phrase304.Replace("{Rabbits}", _rabbit.ToString());
            _phrase304 = _phrase304.Replace("{Chickens}", _chicken.ToString());
            _phrase304 = _phrase304.Replace("{Snakes}", _snake.ToString());
            _phrase304 = _phrase304.Replace("{Wolves}", _wolf.ToString());
            _phrase305 = _phrase305.Replace("{SupplyCrates}", _supplyCrates.ToString());
            _phrase305 = _phrase305.Replace("{MiniBikes}", _miniBikes.ToString());
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase300, CustomCommands.ChatColor), "Server", false, "", false);
                if (_daysUntil7 == 7)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, CustomCommands.ChatColor), "Server", false, "", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false);
                }
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase302, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase303, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase304, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase305, CustomCommands.ChatColor), "Server", false, "", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase300, CustomCommands.ChatColor), "Server", false, "", false));
                if (_daysUntil7 == 7)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false));
                }  
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase302, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase303, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase304, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase305, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }
    }
}