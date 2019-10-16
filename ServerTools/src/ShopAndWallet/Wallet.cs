using System.Data;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Lose_On_Death = false, Bank_Transfers = false, PVP = false;
        public static string Coin_Name = "coin", Command56 = "wallet";
        public static int Zombie_Kills = 10, Player_Kills = 50, Deaths = 25, Session_Bonus = 5;

        public static void WalletValue(ClientInfo _cInfo, string _playerName)
        {
            int _currentCoins = GetCurrentCoins(_cInfo);
            string _message = " your wallet contains: {Value} {Name}.";
            _message = _message.Replace("{Value}", _currentCoins.ToString());
            _message = _message.Replace("{Name}", Wallet.Coin_Name);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static int GetCurrentCoins(ClientInfo _cInfo)
        {
            int _walletValue = 0;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                EntityAlive _playerAlive = _player as EntityAlive;
                int _savedzombieKills = PersistentContainer.Instance.Players[_cInfo.playerId].ZombieKills;
                int _savedplayerKills = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerKills;
                int _savedplayerDeaths = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerDeaths;
                _walletValue = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerCoins;

                int _killedZ = _playerAlive.KilledZombies;
                if (_savedzombieKills < _killedZ)
                {
                    int _newKZ = (_killedZ - _savedzombieKills) * Zombie_Kills;
                    _walletValue = _walletValue + _newKZ;
                    PersistentContainer.Instance.Players[_cInfo.playerId].ZombieKills = _killedZ;
                }
                int _killedP = _playerAlive.KilledPlayers;
                if (_savedplayerKills < _killedP)
                {
                    int _newKP = (_killedP - _savedplayerKills) * Player_Kills;
                    if (PVP)
                    {
                        _walletValue = _walletValue + _newKP;
                    }
                    else
                    {
                        _walletValue = _walletValue - _newKP;
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].PlayerKills = _killedP;
                }
                int _playerDeaths = XUiM_Player.GetDeaths(_player);
                if (_savedplayerDeaths < _playerDeaths)
                {
                    int _newPD = (_playerDeaths - _savedplayerDeaths) * Zombie_Kills;
                    _walletValue = _walletValue + _newPD;
                    PersistentContainer.Instance.Players[_cInfo.playerId].PlayerDeaths = _playerDeaths;
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].PlayerCoins = _walletValue;
                PersistentContainer.Instance.Save();
                return _walletValue;
            }
            else
            {
                return _walletValue;
            }
        }

        public static void UpdateCurrentCoins(ClientInfo _cInfo)
        {
            int _walletValue = 0;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player != null)
            {
                EntityAlive _playerAlive = _player as EntityAlive;
                int _savedzombieKills = PersistentContainer.Instance.Players[_cInfo.playerId].ZombieKills;
                int _savedplayerKills = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerKills;
                int _savedplayerDeaths = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerDeaths;
                _walletValue = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerCoins;

                int _killedZ = _playerAlive.KilledZombies;
                if (_savedzombieKills < _killedZ)
                {
                    int _newKZ = (_killedZ - _savedzombieKills) * Zombie_Kills;
                    _walletValue = _walletValue + _newKZ;
                    PersistentContainer.Instance.Players[_cInfo.playerId].ZombieKills = _killedZ;
                }
                int _killedP = _playerAlive.KilledPlayers;
                if (_savedplayerKills < _killedP)
                {
                    int _newKP = (_killedP - _savedplayerKills) * Player_Kills;
                    if (PVP)
                    {
                        _walletValue = _walletValue + _newKP;
                    }
                    else
                    {
                        _walletValue = _walletValue - _newKP;
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].PlayerKills = _killedP;
                }
                int _playerDeaths = XUiM_Player.GetDeaths(_player);
                if (_savedplayerDeaths < _playerDeaths)
                {
                    int _newPD = (_playerDeaths - _savedplayerDeaths) * Zombie_Kills;
                    _walletValue = _walletValue + _newPD;
                    PersistentContainer.Instance.Players[_cInfo.playerId].PlayerDeaths = _playerDeaths;
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].PlayerCoins = _walletValue;
                PersistentContainer.Instance.Save();
            }
        }

        public static void AddCoinsToWallet(string _steamid, int _amount)
        {
            int _playerCoins = PersistentContainer.Instance.Players[_steamid].PlayerCoins;
            PersistentContainer.Instance.Players[_steamid].PlayerCoins = _playerCoins + _amount;
            PersistentContainer.Instance.Save();
        }

        public static void SubtractCoinsFromWallet(string _steamid, int _amount)
        {
            int _playerCoins = PersistentContainer.Instance.Players[_steamid].PlayerCoins;
            PersistentContainer.Instance.Players[_steamid].PlayerCoins = _playerCoins - _amount;
            PersistentContainer.Instance.Save();
        }

        public static void ClearWallet(ClientInfo _cInfo)
        {
            int _walletValue = GetCurrentCoins(_cInfo);
            if (_walletValue >= 1)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].PlayerCoins = 0;
                PersistentContainer.Instance.Save();
            }
        }
    }
}
