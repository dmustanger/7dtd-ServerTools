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
            int Zombies = 0;
            int Animals = 0;
            List <Entity> _entities = GameManager.Instance.World.Entities.list;
            ulong _worldTime = GameManager.Instance.World.GetWorldTime();
            int _day = GameUtils.WorldTimeToDays(_worldTime);
            foreach (Entity _e in _entities)
            {
                int _c = _e.entityClass;
                
                if (_e.entityType.ToString() == "Zombie")
                {
                    Zombies = Zombies + 1;
                }
                if (_e.entityType.ToString() == "Unknown")
                {
                    Animals = Animals + 1;
                }
            }
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Players: {0}[-]", _playerCount, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server");
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Players: {0}[-]", _playerCount, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server"));
            }
        }
    }
}