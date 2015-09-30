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
            List <Entity> _entities = GameManager.Instance.World.Entities.list;
            ulong _worldTime = GameManager.Instance.World.GetWorldTime();
            int daysUntil7 = 7 - GameUtils.WorldTimeToDays(_worldTime) % 7;
            foreach (Entity _e in _entities)
            {
                
                if (_e.entityType.ToString() == "Zombie")
                {
                    Zombies = Zombies + 1;
                }
                else if (_e.entityType.ToString() == "Animal")
                {
                    Animals = Animals + 1;
                }
                else if (_e.entityType.ToString() == "Player")
                {
                }
                else
                {
                    //Log.Out(string.Format("[Day7] Entity Type is: {0}", _e.entityType.ToString()));
                    //Log.Out(string.Format("[Day7] Entity Class is: {0}", _e.entityClass.ToString()));
                }
            }
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Next 7th day is in {0} days[-]", daysUntil7, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Players: {0}.[-]", _playerCount, CustomCommands._chatcolor), "Server");
                GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server");
                //GameManager.Instance.GameMessageServer(_cInfo, string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server");
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Server FPS: {0}[-]", _fps, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Next 7th day is in {0} days[-]", daysUntil7, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Players: {0}[-]", _playerCount, CustomCommands._chatcolor), "Server"));
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Zombies: {0}[-]", Zombies, CustomCommands._chatcolor), "Server"));
                //_cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}Total Animals: {0}[-]", Animals, CustomCommands._chatcolor), "Server"));
            }
        }
    }
}