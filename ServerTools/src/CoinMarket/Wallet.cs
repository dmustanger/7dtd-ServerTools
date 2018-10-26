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
            int _currentCoins = GetcurrentCoins(_cInfo);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, _currentCoins, Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static int GetcurrentCoins(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result.Dispose();
            int _currentCoins = 0;
            int gameMode = world.GetGameMode();
            if (gameMode == 7)
            {
                _currentCoins = (_player.KilledZombies * Zombie_Kills) + (_player.KilledPlayers * Player_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + _playerSpentCoins;
            }
            else
            {
                _currentCoins = (_player.KilledZombies * Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Deaths) + _playerSpentCoins;
            }
            return _currentCoins;
        }

        public static void AddCoinsToWallet(string _steamid, int _amount)
        {
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
            DataTable _result = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result.Dispose();
            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins + _amount, _steamid);
            SQL.FastQuery(_sql);
        }

        public static void SubtractCoinsFromWallet(string _steamid, int _amount)
        {
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
            DataTable _result = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result.Dispose();
            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _amount, _steamid);
            SQL.FastQuery(_sql);
        }

        public static void ClearWallet(ClientInfo _cInfo)
        {
            int _walletValue = GetcurrentCoins(_cInfo);
            if (_walletValue >= 1)
            {
                string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _playerSpentCoins;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                _result.Dispose();
                _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _walletValue, _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
        }
    }
}
