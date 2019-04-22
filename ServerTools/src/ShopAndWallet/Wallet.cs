using System.Data;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Lose_On_Death = false, Bank_Transfers = false;
        public static string Coin_Name = "coin", Command56 = "wallet";
        public static int Zombie_Kills = 10;
        public static int Player_Kills = 50;
        public static int Deaths = 25;

        public static void WalletValue(ClientInfo _cInfo, string _playerName)
        {
            int _currentCoins = GetcurrentCoins(_cInfo);
            string _message = " your wallet contains: {Value} {Name}.";
            _message = _message.Replace("{Value}", _currentCoins.ToString());
            _message = _message.Replace("{Name}", Wallet.Coin_Name);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            SQL.FastQuery(_sql, "Wallet");
        }

        public static void SubtractCoinsFromWallet(string _steamid, int _amount)
        {
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _steamid);
            DataTable _result = SQL.TQuery(_sql);
            int _playerSpentCoins;
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
            _result.Dispose();
            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _amount, _steamid);
            SQL.FastQuery(_sql, "Wallet");
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
                SQL.FastQuery(_sql, "Wallet");
            }
        }

        public static void PlayerKilled(Entity _entity2, ClientInfo _cInfo2)
        {
            if (IsEnabled && Lose_On_Death)
            {
                World world = GameManager.Instance.World;
                string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _playerSpentCoins;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                _result.Dispose();
                int _currentCoins = GetcurrentCoins(_cInfo2);
                if (_currentCoins >= 1)
                {
                    _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - _currentCoins, _cInfo2.playerId);
                    SQL.FastQuery(_sql, "Wallet");
                }
            }
        }
    }
}
