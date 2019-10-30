
namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Lose_On_Death = false, Bank_Transfers = false, PVP = false;
        public static string Coin_Name = "coin", Command56 = "wallet";
        public static int Zombie_Kills = 10, Player_Kills = 25, Deaths = 25, Session_Bonus = 5;

        public static void CurrentValue(ClientInfo _cInfo)
        {
            int _currentCoins = GetCurrentCoins(_cInfo);
            string _message = " your wallet contains: {Value} {Name}.";
            _message = _message.Replace("{Value}", _currentCoins.ToString());
            _message = _message.Replace("{Name}", Wallet.Coin_Name);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static int GetCurrentCoins(ClientInfo _cInfo)
        {
            int _walletValue = PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet;
            return _walletValue;
        }

        public static void AddCoinsToWallet(string _steamid, int _amount)
        {
            int _walletValue = PersistentContainer.Instance.Players[_steamid].PlayerWallet;
            PersistentContainer.Instance.Players[_steamid].PlayerWallet = _walletValue + _amount;
            PersistentContainer.Instance.Save();
        }

        public static void SubtractCoinsFromWallet(string _steamid, int _amount)
        {
            int _newValue = PersistentContainer.Instance.Players[_steamid].PlayerWallet - _amount;
            if (_newValue < 0)
            {
                _newValue = 0;
            }
            PersistentContainer.Instance.Players[_steamid].PlayerWallet = _newValue;
            PersistentContainer.Instance.Save();
        }

        public static void ClearWallet(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet = 0;
            PersistentContainer.Instance.Save();
        }
    }
}
