using System.Collections.Generic;

namespace ServerTools
{
    class BloodmoonWarrior
    {
        public static bool IsEnabled = false, BloodmoonStarted = false;
        public static int Zombie_Kills = 10;
        public static List<int> WarriorList = new List<int>();
        public static Dictionary<int, int> KilledZombies = new Dictionary<int, int>();

        public static void Exec()
        {
            if (!BloodmoonStarted)
            {
                if (PersistentOperations.BloodMoonSky())
                {
                    BloodmoonStarted = true;
                    List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                    if (_cInfoList != null)
                    {
                        for (int i = 0; i < _cInfoList.Count; i++)
                        {
                            ClientInfo _cInfo = _cInfoList[i];
                            if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.playerId) && _cInfo.entityId > 0)
                            {
                                EntityAlive _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null && _player.IsSpawned() && _player.IsAlive())
                                {
                                    WarriorList.Add(_cInfo.entityId);
                                    KilledZombies.Add(_cInfo.entityId, 0);
                                    if (_cInfo != null)
                                    {
                                        string _response = "Hades has called upon you. Survive this night and kill {ZombieCount} zombies to be rewarded by the king of the underworld.";
                                        _response = _response.Replace("{ZombieCount}", Zombie_Kills.ToString());
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
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
                    if (_killedZ >= Zombie_Kills && _player.Died > 0)
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
