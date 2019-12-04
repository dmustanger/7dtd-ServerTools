using System.Collections.Generic;

namespace ServerTools
{
    class BloodmoonWarrior
    {
        public static bool IsEnabled = false, BloodmoonStarted = false;
        public static int Zombies_Killed = 10;
        public static List<int> WarriorList = new List<int>();
        public static Dictionary<int, int> KilledZombies = new Dictionary<int, int>();

        public static void Exec()
        {
            if (!BloodmoonStarted)
            {
                if (PersistentOperations.BloodMoonSky())
                {
                    BloodmoonStarted = true;
                    for (int i = 0; i < API.Players.Count; i++)
                    {
                        ClientInfo _warrior = API.Players[i];
                        EntityAlive _player = GameManager.Instance.World.Players.dict[_warrior.entityId];
                        if (_player != null && _player.IsSpawned() && _player.IsAlive())
                        {
                            WarriorList.Add(_warrior.entityId);
                            KilledZombies.Add(_warrior.entityId, 0);
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_warrior.entityId);
                            if (_cInfo != null)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Hades has called upon you. Survive this night and kill ten zombies to be rewarded by the king of the underworld." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            else if (!PersistentOperations.BloodMoonSky())
            {
                BloodmoonStarted = false;
                RewardWarriors();
            }
        }

        public static void RewardWarriors()
        {
            List<int> _warriors = WarriorList;
            for (int i = 0; i < _warriors.Count; i++)
            {
                int _warrior = _warriors[i];
                EntityAlive _player = GameManager.Instance.World.Players.dict[_warrior];
                if (_player != null && _player.IsAlive())
                {
                    int _killedZ;
                    KilledZombies.TryGetValue(_warrior, out _killedZ);
                    if (_killedZ >= Zombies_Killed && _player.Died > 0)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_warrior);
                        if (_cInfo != null)
                        {
                            int _deathCount = _player.Died - 1;
                            _player.Died = _deathCount;
                            _player.bPlayerStatsChanged = true;
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(_player));
                            WarriorList.Remove(_warrior);
                            KilledZombies.Remove(_warrior);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have survived and been rewarded by hades himself. Your death count was reduced by one." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                if (WarriorList.Contains(_warrior))
                {
                    WarriorList.Remove(_warrior);
                    KilledZombies.Remove(_warrior);
                }
            }
        }
    }
}
