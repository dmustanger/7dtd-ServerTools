using System.Collections.Generic;

namespace ServerTools
{
    class Lottery
    {
        public static bool IsEnabled = false, OpenLotto = false, ShuttingDown = false;
        public static int Time = 10, Bonus = 0, LottoValue = 0;
        public static string Command_lottery = "lottery", Command_lottery_enter = "lottery enter";
        public static List<ClientInfo> LottoEntries = new List<ClientInfo>();

        public static void Response(ClientInfo _cInfo)
        {
            if (OpenLotto)
            {
                int value = LottoValue * LottoEntries.Count;
                Phrases.Dict.TryGetValue("Lottery2", out string phrase);
                phrase = phrase.Replace("{Value1}", value.ToString());
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                phrase = phrase.Replace("{Value2}", LottoValue.ToString());
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Lottery1", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void NewLotto(ClientInfo _cInfo, string _message)
        {
            if (!ShuttingDown)
            {
                if (OpenLotto)
                {
                    int winnings = LottoValue * LottoEntries.Count;
                    Phrases.Dict.TryGetValue("Lottery2", out string phrase);
                    phrase = phrase.Replace("{Value1}", winnings.ToString());
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    phrase = phrase.Replace("{Value2}", LottoValue.ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    if (int.TryParse(_message, out int _lottoValue))
                    {
                        if (_lottoValue > 0)
                        {
                            int currency = 0;
                            if (Wallet.IsEnabled)
                            {
                                currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                            }
                            if (currency >= _lottoValue)
                            {
                                OpenLotto = true;
                                LottoValue = _lottoValue;
                                LottoEntries.Add(_cInfo);
                                if (LottoValue >= 1 && Wallet.IsEnabled)
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, LottoValue);
                                }
                                Phrases.Dict.TryGetValue("Lottery4", out string phrase);
                                phrase = phrase.Replace("{Value}", _lottoValue.ToString());
                                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Lottery5", out phrase);
                                phrase = phrase.Replace("{Value}", LottoValue.ToString());
                                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Lottery6", out string phrase);
                                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Lottery3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lottery3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }

        public static void EnterLotto(ClientInfo _cInfo)
        {
            if (OpenLotto)
            {
                int currency = 0;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (currency >= LottoValue)
                {
                    if (!LottoEntries.Contains(_cInfo))
                    {
                        LottoEntries.Add(_cInfo);
                        if (LottoValue >= 1 && Wallet.IsEnabled)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, LottoValue);
                        }
                        Phrases.Dict.TryGetValue("Lottery7", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        if (LottoEntries.Count == 8)
                        {
                            StartLotto();
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Lottery8", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Lottery6", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Lottery1", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{Command_lottery}", Command_lottery);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void StartLotto()
        {
            System.Random rnd = new System.Random();
            int random = rnd.Next(0, LottoEntries.Count + 1);
            ClientInfo winner = LottoEntries[random];
            int winnings;
            if (LottoEntries.Count == 8)
            {
                winnings = LottoValue * LottoEntries.Count + Bonus;
            }
            else
            {
                winnings = LottoValue * LottoEntries.Count;
            }
            LottoValue = 0;
            LottoEntries.Clear();
            Wallet.AddCurrency(winner.CrossplatformId.CombinedString, winnings, true);
            Phrases.Dict.TryGetValue("Lottery10", out string phrase);
            phrase = phrase.Replace("{PlayerName}", winner.playerName);
            phrase = phrase.Replace("{Value}", winnings.ToString());
            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void Alert()
        {
            Phrases.Dict.TryGetValue("Lottery9", out string phrase);
            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }
    }
}
