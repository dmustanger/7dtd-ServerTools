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
                Phrases.Dict.TryGetValue("Lottery2", out string _phrase);
                _phrase = _phrase.Replace("{Value}", _value.ToString());
                _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                _phrase = _phrase.Replace("{BuyIn}", LottoValue.ToString());
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Lottery1", out string _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void NewLotto(ClientInfo _cInfo, string _message)
        {
            if (!ShuttingDown)
            {
                if (OpenLotto)
                {
                    int _winnings = LottoValue * LottoEntries.Count;
                    Phrases.Dict.TryGetValue("Lottery2", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _winnings.ToString());
                    _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    _phrase = _phrase.Replace("{BuyIn}", LottoValue.ToString());
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (int.TryParse(_message, out int _lottoValue))
                    {
                        if (_lottoValue > 0)
                        {
                            if (Wallet.GetCurrency(_cInfo.playerId) >= _lottoValue)
                            {
                                OpenLotto = true;
                                LottoValue = _lottoValue;
                                LottoEntries.Add(_cInfo);
                                Wallet.RemoveCurrency(_cInfo.playerId, LottoValue);
                                Phrases.Dict.TryGetValue("Lottery4", out string _phrase);
                                _phrase = _phrase.Replace("{Value}", _lottoValue.ToString());
                                _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Lottery5", out _phrase);
                                _phrase = _phrase.Replace("{Value}", LottoValue.ToString());
                                _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase = _phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Lottery6", out string _phrase);
                                _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Lottery3", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lottery3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void EnterLotto(ClientInfo _cInfo)
        {
            if (OpenLotto)
            {
                if (Wallet.GetCurrency(_cInfo.playerId) >= LottoValue)
                {
                    if (!LottoEntries.Contains(_cInfo))
                    {
                        LottoEntries.Add(_cInfo);
                        Wallet.RemoveCurrency(_cInfo.playerId, LottoValue);
                        Phrases.Dict.TryGetValue("Lottery7", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        if (LottoEntries.Count == 10)
                        {
                            StartLotto();
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lottery8", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Lottery6", out string _phrase);
                    _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Lottery1", out string _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            Wallet.AddCurrency(_winner.playerId, _winnings);
            Phrases.Dict.TryGetValue("Lottery10", out string _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _winner.playerName);
            _phrase = _phrase.Replace("{Value}", _winnings.ToString());
            _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Alert()
        {
            Phrases.Dict.TryGetValue("Lottery9", out string _phrase);
            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            _phrase = _phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}
