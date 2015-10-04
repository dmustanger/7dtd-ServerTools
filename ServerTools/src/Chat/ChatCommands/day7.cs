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
            int Zombies = 0;
            int Animals = 0;
            int FeralZombies = 0;
            int SupplyCrates = 0;
            int MiniBikes = 0;
            int Stag = 0;
            int Bear = 0;
            int Rabbit = 0;
            int Pig = 0;
            int Dog = 0;
            int Hornet = 0;
            int Cop = 0;
            ulong _worldTime = GameManager.Instance.World.GetWorldTime();
            int daysUntil7 = 7 - GameUtils.WorldTimeToDays(_worldTime) % 7;
            List <Entity> _entities = GameManager.Instance.World.Entities.list;
            foreach (Entity _e in _entities)
            {
                string _name = EntityClass.list[_e.entityClass].entityClassName;
                if (_name == "zombie04" || _name == "zombie05" || _name == "zombie06" || _name == "zombie07" || _name == "zombieBoe" || _name == "zombieJoe" || _name == "zombieMoe" || _name == "zombieYo" || _name == "zombieSteve" || _name == "zombieSteveCrawler" || _name == "zombie01" || _name == "zombiecrawler" || _name == "snowzombie01" || _name == "snowzombie02" || _name == "snowzombie03" || _name == "spiderzombie" || _name == "burntzombie" || _name == "zombiegal01" || _name == "zombiegal02" || _name == "zombiegal03" || _name == "zombiegal04" || _name == "zombie02" || _name == "fatzombie" || _name == "zombieUMAfemale" || _name == "zombieUMAmale")
                {
                    Zombies = Zombies + 1;
                }
                else if (_name == "zombieferal")
                {
                    Zombies = Zombies + 1;
                    FeralZombies = FeralZombies + 1;
                }
                else if (_name == "zombiedog")
                {
                    Zombies = Zombies + 1;
                    Dog = Dog + 1;
                }
                else if (_name == "hornet")
                {
                    Zombies = Zombies + 1;
                    Hornet = Hornet + 1;
                }
                else if (_name == "fatzombiecop")
                {
                    Zombies = Zombies + 1;
                    Cop = Cop + 1;
                }
                else if (_name == "animalStag")
                {
                    Animals = Animals + 1;
                    Stag = Stag + 1;
                }
                else if (_name == "animalBear")
                {
                    Animals = Animals + 1;
                    Bear = Bear + 1;
                }
                else if (_name == "animalRabbit")
                {
                    Animals = Animals + 1;
                    Rabbit = Rabbit + 1;
                }
                else if (_name == "animalPig")
                {
                    Animals = Animals + 1;
                    Pig = Pig + 1;
                }
                else if (_name == "sc_General")
                {
                    SupplyCrates = SupplyCrates + 1;
                }
                else if (_name == "minibike")
                {
                    MiniBikes = MiniBikes + 1;
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Entity Class Name is: {0}", _name));
                }
            }
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Next 7th day is in {0} days[-]", daysUntil7, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Players: {0}.[-]", _playerCount, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{4}Feral Zombies:{0} Cops:{1} Dogs:{2} Bees:{3}[-]", FeralZombies, Cop, Dog, Hornet, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{4}Bears:{0} Stags:{1} Pigs:{2} Rabbits:{3}[-]", Bear, Stag, Pig, Rabbit, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Supply Crates: {0}[-]", SupplyCrates, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Mini Bikes: {0}[-]", MiniBikes, CustomCommands._chatcolor), "Server");
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Next 7th day is in {0} days[-]", daysUntil7, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Players: {0}[-]", _playerCount, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{4}Feral Zombies:{0} Cops:{1} Dogs:{2} Bees:{3}[-]", FeralZombies, Cop, Dog, Hornet, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{4}Bears:{0} Stags:{1} Pigs:{2} Rabbits:{3}[-]", Bear, Stag, Pig, Rabbit, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Supply Crates: {0}[-]", SupplyCrates, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Mini Bikes: {0}[-]", MiniBikes, CustomCommands._chatcolor), "Server"));
            }
        }
    }
}