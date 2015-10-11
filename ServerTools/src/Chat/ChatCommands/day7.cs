using System.Collections.Generic;

namespace ServerTools
{
    public class Day7
    {
        public static bool IsEnabled = true;

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
            int _pig = 0;
            int _dog = 0;
            int _hornet = 0;
            int _cop = 0;
            ulong _worldTime = GameManager.Instance.World.GetWorldTime();
            int _daysUntil7 = 7 - GameUtils.WorldTimeToDays(_worldTime) % 7;
            List <Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                string _name = EntityClass.list[_e.entityClass].entityClassName;
                if (_name == "zombie04" || _name == "zombie05" || _name == "zombie06" || _name == "zombie07" || _name == "zombieBoe" || _name == "zombieJoe" || _name == "zombieMoe" || _name == "zombieYo" || _name == "zombieSteve" || _name == "zombieSteveCrawler" || _name == "zombie01" || _name == "zombiecrawler" || _name == "snowzombie01" || _name == "snowzombie02" || _name == "snowzombie03" || _name == "spiderzombie" || _name == "burntzombie" || _name == "zombiegal01" || _name == "zombiegal02" || _name == "zombiegal03" || _name == "zombiegal04" || _name == "zombie02" || _name == "fatzombie" || _name == "zombieUMAfemale" || _name == "zombieUMAmale")
                {
                    _zombies = _zombies + 1;
                }
                else if (_name == "zombieferal")
                {
                    _zombies = _zombies + 1;
                    _feralZombies = _feralZombies + 1;
                }
                else if (_name == "zombiedog")
                {
                    _zombies = _zombies + 1;
                    _dog = _dog + 1;
                }
                else if (_name == "hornet")
                {
                    _zombies = _zombies + 1;
                    _hornet = _hornet + 1;
                }
                else if (_name == "fatzombiecop")
                {
                    _zombies = _zombies + 1;
                    _cop = _cop + 1;
                }
                else if (_name == "animalStag")
                {
                    _animals = _animals + 1;
                    _stag = _stag + 1;
                }
                else if (_name == "animalBear")
                {
                    _animals = _animals + 1;
                    _bear = _bear + 1;
                }
                else if (_name == "animalRabbit")
                {
                    _animals = _animals + 1;
                    _rabbit = _rabbit + 1;
                }
                else if (_name == "animalPig")
                {
                    _animals = _animals + 1;
                    _pig = _pig + 1;
                }
                else if (_name == "sc_General")
                {
                    _supplyCrates = _supplyCrates + 1;
                }
                else if (_name == "minibike")
                {
                    _miniBikes = _miniBikes + 1;
                }
                else if (_name == "item" || _name == "playerMale" || _name == "playerFemale" || _name.StartsWith("car_"))
                {
                    continue;
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Entity Class Name is: {0}", _name));
                }
            }
            string _phrase300 = "Server FPS: {Fps}";
            if (Phrases._Phrases.TryGetValue(300, out _phrase300))
            {
                _phrase300 = _phrase300.Replace("{Fps}", _fps);
            }
            string _phrase301 = "Next 7th day is in {DaysUntil7} days";
            if (Phrases._Phrases.TryGetValue(301, out _phrase301))
            {
                _phrase301 = _phrase301.Replace("{DaysUntil7}", _daysUntil7.ToString());
            }
            string _phrase302 = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
            if (Phrases._Phrases.TryGetValue(302, out _phrase302))
            {
                _phrase302 = _phrase302.Replace("{Players}", _playerCount.ToString());
                _phrase302 = _phrase302.Replace("{Zombies}", _zombies.ToString());
                _phrase302 = _phrase302.Replace("{Animals}", _animals.ToString());
            }
            string _phrase303 = "Feral Zombies:{Ferals} Cops:{Cops} Dogs:{Dogs} Bees:{Bees}";
            if (Phrases._Phrases.TryGetValue(303, out _phrase303))
            {
                _phrase303 = _phrase303.Replace("{Ferals}", _feralZombies.ToString());
                _phrase303 = _phrase303.Replace("{Cops}", _cop.ToString());
                _phrase303 = _phrase303.Replace("{Dogs}", _dog.ToString());
                _phrase303 = _phrase303.Replace("{Bees}", _hornet.ToString());
            }
            string _phrase304 = "Bears:{Bears} Stags:{Stags} Pigs:{Pigs} Rabbits:{Rabbits}";
            if (Phrases._Phrases.TryGetValue(304, out _phrase304))
            {
                _phrase304 = _phrase304.Replace("{Bears}", _bear.ToString());
                _phrase304 = _phrase304.Replace("{Stags}", _stag.ToString());
                _phrase304 = _phrase304.Replace("{Pigs}", _pig.ToString());
                _phrase304 = _phrase304.Replace("{Rabbits}", _rabbit.ToString());
            }
            string _phrase305 = "Total Supply Crates:{SupplyCrates} Total Mini Bikes:{MiniBikes}";
            if (Phrases._Phrases.TryGetValue(305, out _phrase305))
            {
                _phrase305 = _phrase305.Replace("{SupplyCrates}", _supplyCrates.ToString());
                _phrase305 = _phrase305.Replace("{MiniBikes}", _miniBikes.ToString());
            }
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase300, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase301, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase302, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase303, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase304, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}{0}[-]", _phrase305, CustomCommands._chatcolor), "Server");
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase300, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase301, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase302, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase303, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase304, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase305, CustomCommands._chatcolor), "Server"));
            }
        }
    }
}