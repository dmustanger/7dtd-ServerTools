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
            int _feralZombies = 0;
            int _supplyCrates = 0;
            int _miniBikes = 0;
            int _stag = 0;
            int _bear = 0;
            int _rabbit = 0;
            int _chicken = 0;
            int _pig = 0;
            int _dog = 0;
            int _hornet = 0;
            int _cop = 0;
            int _screamer = 0;
            int _daysUntil7 = 7 - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % 7;
            List<Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                string _name = EntityClass.list[_e.entityClass].entityClassName;
                if (_name == "playerMale" || _name == "playerFemale" || _name == "item" || _name == "fallingBlock" || _name == "Backpack" || _name == "traderBob") 
                {
                    continue;
                }
                else if (_name == "zombieMarlene" || _name == "zombieDarlene" || _name == "zombieArlene" || _name == "zombieBoe" || _name == "zombieYo" || _name == "zombieSteveCrawler" || _name == "zombieNurse" || _name == "zombieMoe" || _name == "zombieJoe" || _name == "burntzombie" || _name == "spiderzombie" || _name == "snowzombie" || _name == "zombieSteve" || _name == "ZombieStripper" || _name == "ZombieFarmer" || _name == "ZombieBiker" || _name == "ZombieFemaleUMA" || _name == "ZombieFemaleFatUMA" || _name == "ZombieMaleUMA" || _name == "zombieMaleHazmat" || _name == "ZombieCheerleader" || _name == "ZombieSoldier" || _name == "ZombieSkateboarder" || _name == "zombieFemaleHazmat" || _name == "ZombieMiner" || _name == "ZombieUtilityWorker" || _name == "ZombieFootballPlayer") 
                {
                    _zombies = _zombies + 1;
                    continue;
                }
                else if (_name == "animalRabbit")
                {
                    _animals = _animals + 1;
                    _rabbit = _rabbit + 1;
                    continue;
                }
                else if (_name == "animalChicken")
                {
                    _animals = _animals + 1;
                    _chicken = _chicken + 1;
                    continue;
                }
                else if (_name == "animalStag")
                {
                    _animals = _animals + 1;
                    _stag = _stag + 1;
                    continue;
                }
                else if (_name == "animalPig")
                {
                    _animals = _animals + 1;
                    _pig = _pig + 1;
                    continue;
                }
                else if (_name == "animalBear" || _name == "zombieBear")
                {
                    _animals = _animals + 1;
                    _bear = _bear + 1;
                    continue;
                }
                else if (_name == "zombiedog")
                {
                    _zombies = _zombies + 1;
                    _dog = _dog + 1;
                    continue;
                }
                else if (_name == "zombieScreamer")
                {
                    _zombies = _zombies + 1;
                    _screamer = _screamer + 1;
                    continue;
                }
                else if (_name == "zombieferal")
                {
                    _zombies = _zombies + 1;
                    _feralZombies = _feralZombies + 1;
                    continue;
                }
                else if (_name == "hornet")
                {
                    _zombies = _zombies + 1;
                    _hornet = _hornet + 1;
                    continue;
                }
                else if (_name == "fatzombiecop")
                {
                    _zombies = _zombies + 1;
                    _cop = _cop + 1;
                    continue;
                }
                else if (_name == "minibike")
                {
                    _miniBikes = _miniBikes + 1;
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
                _phrase303 = "Feral Zombies:{Ferals} Cops:{Cops} Dogs:{Dogs} Bees:{Bees} Screamers:{Screamers}";
            }
            if (!Phrases.Dict.TryGetValue(304, out _phrase304))
            {
                _phrase304 = "Bears:{Bears} Stags:{Stags} Pigs:{Pigs} Rabbits:{Rabbits} Chickens:{Chickens}";
            }
            if (!Phrases.Dict.TryGetValue(305, out _phrase305))
            {
                _phrase305 = "Total Supply Crates:{SupplyCrates} Total Mini Bikes:{MiniBikes}";
            }
            _phrase300 = _phrase300.Replace("{Fps}", _fps);
            _phrase301 = _phrase301.Replace("{DaysUntil7}", _daysUntil7.ToString());
            _phrase302 = _phrase302.Replace("{Players}", _playerCount.ToString());
            _phrase302 = _phrase302.Replace("{Zombies}", _zombies.ToString());
            _phrase302 = _phrase302.Replace("{Animals}", _animals.ToString());
            _phrase303 = _phrase303.Replace("{Ferals}", _feralZombies.ToString());
            _phrase303 = _phrase303.Replace("{Cops}", _cop.ToString());
            _phrase303 = _phrase303.Replace("{Dogs}", _dog.ToString());
            _phrase303 = _phrase303.Replace("{Bees}", _hornet.ToString());
            _phrase303 = _phrase303.Replace("{Screamers}", _screamer.ToString());
            _phrase304 = _phrase304.Replace("{Bears}", _bear.ToString());
            _phrase304 = _phrase304.Replace("{Stags}", _stag.ToString());
            _phrase304 = _phrase304.Replace("{Pigs}", _pig.ToString());
            _phrase304 = _phrase304.Replace("{Rabbits}", _rabbit.ToString());
            _phrase304 = _phrase304.Replace("{Chickens}", _chicken.ToString());
            _phrase305 = _phrase305.Replace("{SupplyCrates}", _supplyCrates.ToString());
            _phrase305 = _phrase305.Replace("{MiniBikes}", _miniBikes.ToString());
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase300, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase302, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase303, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase304, CustomCommands.ChatColor), "Server", false, "", false);
                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase305, CustomCommands.ChatColor), "Server", false, "", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase300, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase302, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase303, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase304, CustomCommands.ChatColor), "Server", false, "", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase305, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }
    }
}