
namespace ServerTools
{
    class Wallet
    {
        public static bool Negative_Wallet = false;
        public static string Coin_Name = "coin";
        public static int Zombie_Kills = 10;
        public static int Player_Kills = 50;
        public static int Deaths = 25;

        public static void WalletValue(ClientInfo _cInfo, string _playerName)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Zombie_Kills) + (_player.KilledPlayers * Player_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + p.PlayerSpentCoins;
                }
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
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                PersistentContainer.Instance.Save();
                WalletValue(_cInfo, _playerName);
            }
        }
    }
}
