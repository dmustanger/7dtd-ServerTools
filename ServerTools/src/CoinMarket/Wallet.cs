
namespace ServerTools
{
    class Wallet
    {
        public static bool Negative_Wallet = false;
        public static string Coin_Name = "coin";

        public static void AddWorldSeed(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            int worldSeed = world.Seed;
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.WorldSeedCoins != worldSeed)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId, true].WorldSeedCoins = worldSeed;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                PersistentContainer.Instance.Save();
            }
        }

        public static void WalletCheck(ClientInfo _cInfo, string _playerName)
        {            
            World world = GameManager.Instance.World;
            int worldSeed = world.Seed;
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null)
            {
                if (p.WorldSeedCoins == worldSeed)
                {
                    WalletValue(_cInfo, _playerName, p);
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].WorldSeedCoins = worldSeed;
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                    PersistentContainer.Instance.Save();
                    WalletValue(_cInfo, _playerName, p);
                }
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId, true].WorldSeedCoins = worldSeed;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                PersistentContainer.Instance.Save();
                WalletValue(_cInfo, _playerName, p);
            }
        }

        public static void WalletValue(ClientInfo _cInfo, string _playerName, Player p)
        {
            World world = GameManager.Instance.World;
            int worldSeed = world.Seed;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int spentCoins = p.PlayerSpentCoins;
            int gameMode = world.GetGameMode();
            if (gameMode == 7)
            {
                int currentCoins = (_player.KilledZombies * 10) + (_player.KilledPlayers * 50) - (XUiM_Player.GetDeaths(_player) * -25) + p.PlayerSpentCoins;
                if (!Negative_Wallet)
                {
                    if (currentCoins < 0)
                    {
                        currentCoins = 0;
                    }
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, currentCoins, Coin_Name), "Server", false, "", false));
            }
            else
            {
                int currentCoins = (_player.KilledZombies * 10) - (XUiM_Player.GetDeaths(_player) * -25) + p.PlayerSpentCoins;
                if (!Negative_Wallet)
                {
                    if (currentCoins < 0)
                    {
                        currentCoins = 0;
                    }
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, currentCoins, Coin_Name), "Server", false, "", false));
            }
        }
    }
}
