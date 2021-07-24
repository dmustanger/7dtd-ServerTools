using System.Collections.Generic;

namespace ServerTools
{
    class Lottery
    {
        public static bool IsEnabled = false, OpenLotto = false, ShuttingDown = false;
        public static int Bonus = 0, LottoValue = 0;
        public static string Command_lottery = "lottery", Command_lottery_enter = "lottery enter";
        public static List<ClientInfo> LottoEntries = new List<ClientInfo>();

        public static void Response(ClientInfo _cInfo)
        {
            if (OpenLotto)
            {
                int _value = LottoValue * LottoEntries.Count;
                Phrases.Dict.TryGetValue(222, out string _phrase222);
                _phrase222 = _phrase222.Replace("{Value}", _value.ToString());
                _phrase222 = _phrase222.Replace("{CoinName}", Wallet.Coin_Name);
                _phrase222 = _phrase222.Replace("{BuyIn}", LottoValue.ToString());
                _phrase222 = _phrase222.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase222 = _phrase222.Replace("{Command_lottery_enter}", Command_lottery_enter);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase222 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(221, out string _phrase221);
                _phrase221 = _phrase221.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase221 = _phrase221.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase221 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void NewLotto(ClientInfo _cInfo, string _message)
        {
            if (!ShuttingDown)
            {
                if (OpenLotto)
                {
                    int _winnings = LottoValue * LottoEntries.Count;
                    Phrases.Dict.TryGetValue(222, out string _phrase222);
                    _phrase222 = _phrase222.Replace("{Value}", _winnings.ToString());
                    _phrase222 = _phrase222.Replace("{CoinName}", Wallet.Coin_Name);
                    _phrase222 = _phrase222.Replace("{BuyIn}", LottoValue.ToString());
                    _phrase222 = _phrase222.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase222 = _phrase222.Replace("{Command_lottery_enter}", Command_lottery_enter);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase222 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (int.TryParse(_message, out int _lottoValue))
                    {
                        if (_lottoValue > 0)
                        {
                            if (Wallet.GetCurrentCoins(_cInfo.playerId) >= _lottoValue)
                            {
                                OpenLotto = true;
                                LottoValue = _lottoValue;
                                LottoEntries.Add(_cInfo);
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, LottoValue);
                                Phrases.Dict.TryGetValue(224, out string _phrase224);
                                _phrase224 = _phrase224.Replace("{Value}", _lottoValue.ToString());
                                _phrase224 = _phrase224.Replace("{CoinName}", Wallet.Coin_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase224 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(225, out string _phrase225);
                                _phrase225 = _phrase225.Replace("{Value}", LottoValue.ToString());
                                _phrase225 = _phrase225.Replace("{CoinName}", Wallet.Coin_Name);
                                _phrase225 = _phrase225.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase225 = _phrase225.Replace("{Command_lottery_enter}", Command_lottery_enter);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase225 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(226, out string _phrase226);
                                _phrase226 = _phrase226.Replace("{CoinName}", Wallet.Coin_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase226 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(223, out string _phrase223);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase223 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(223, out string _phrase223);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase223 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void EnterLotto(ClientInfo _cInfo)
        {
            if (OpenLotto)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= LottoValue)
                {
                    if (!LottoEntries.Contains(_cInfo))
                    {
                        LottoEntries.Add(_cInfo);
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, LottoValue);
                        Phrases.Dict.TryGetValue(227, out string _phrase227);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase227 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        if (LottoEntries.Count == 10)
                        {
                            StartLotto();
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(228, out string _phrase228);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase228 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(226, out string _phrase226);
                    _phrase226 = _phrase226.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase226 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(221, out string _phrase221);
                _phrase221 = _phrase221.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase221 = _phrase221.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase221 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void StartLotto()
        {
            System.Random rnd = new System.Random();
            int _random = rnd.Next(0, LottoEntries.Count + 1);
            ClientInfo _winner = LottoEntries[_random];
            int _winnings;
            if (LottoEntries.Count == 10)
            {
                _winnings = LottoValue * LottoEntries.Count + Bonus;
            }
            else
            {
                _winnings = LottoValue * LottoEntries.Count;
            }
            OpenLotto = false;
            LottoValue = 0;
            LottoEntries.Clear();
            Wallet.AddCoinsToWallet(_winner.playerId, _winnings);
            Phrases.Dict.TryGetValue(230, out string _phrase230);
            _phrase230 = _phrase230.Replace("{PlayerName}", _winner.playerName);
            _phrase230 = _phrase230.Replace("{Value}", _winnings.ToString());
            _phrase230 = _phrase230.Replace("{CoinName}", Wallet.Coin_Name);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase230 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Alert()
        {
            Phrases.Dict.TryGetValue(229, out string _phrase229);
            _phrase229 = _phrase229.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            _phrase229 = _phrase229.Replace("{Command_lottery_enter}", Command_lottery_enter);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase229 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}
