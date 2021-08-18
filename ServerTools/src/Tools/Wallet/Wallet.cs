
namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Lose_On_Death = false, Bank_Transfers = false, PVP = false;
        public static string Coin_Name = "coin", Command_wallet = "wallet";
        public static int Zombie_Kills = 10, Player_Kills = 25, Deaths = 25, Session_Bonus = 5;

        public static void CurrentValue(ClientInfo _cInfo)
        {
            int _currentCoins = GetCurrentCoins(_cInfo.playerId);
            Phrases.Dict.TryGetValue("Wallet1", out string _phrase);
            _phrase = _phrase.Replace("{Value}", _currentCoins.ToString());
            _phrase = _phrase.Replace("{CoinName}", Coin_Name);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static int GetCurrentCoins(string _steamid)
        {
            int _walletValue = PersistentContainer.Instance.Players[_steamid].PlayerWallet;
            if (_walletValue < 0)
            {
                _walletValue = 0;
                PersistentContainer.Instance.Players[_steamid].PlayerWallet = _walletValue;
                PersistentContainer.DataChange = true;
            }
            return _walletValue;
        }

        public static void AddCoinsToWallet(string _steamid, int _amount)
        {
            int _walletValue = PersistentContainer.Instance.Players[_steamid].PlayerWallet;
            PersistentContainer.Instance.Players[_steamid].PlayerWallet = _walletValue + _amount;
            PersistentContainer.DataChange = true;
        }

        public static void SubtractCoinsFromWallet(string _steamid, int _amount)
        {
            int _newValue = PersistentContainer.Instance.Players[_steamid].PlayerWallet - _amount;
            if (_newValue < 0)
            {
                _newValue = 0;
            }
            PersistentContainer.Instance.Players[_steamid].PlayerWallet = _newValue;
            PersistentContainer.DataChange = true;
        }

        public static void ClearWallet(ClientInfo _cInfo)
        {
            PersistentContainer.Instance.Players[_cInfo.playerId].PlayerWallet = 0;
            PersistentContainer.DataChange = true;
        }
    }
}
