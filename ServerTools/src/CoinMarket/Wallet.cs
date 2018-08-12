using System.Data;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Lose_On_Death = false;
        public static string Coin_Name = "coin";
        public static int Zombie_Kills = 10;
        public static int Player_Kills = 50;
        public static int Deaths = 25;

        public static void WalletValue(ClientInfo _cInfo, string _playerName)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _playerSpentCoins);
            _result.Dispose();
            int currentCoins = 0;
            int gameMode = world.GetGameMode();
            if (gameMode == 7)
            {
                currentCoins = (_player.KilledZombies * Zombie_Kills) + (_player.KilledPlayers * Player_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + _playerSpentCoins;
            }
            else
            {
                currentCoins = (_player.KilledZombies * Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + _playerSpentCoins;
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, currentCoins, Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
        }
    }
}
