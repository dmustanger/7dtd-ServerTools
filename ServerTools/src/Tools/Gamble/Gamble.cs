using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Gamble
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 10, Command_Cost = 20;
        public static string Command_gamble = "gamble", Command_gamble_bet = "gamble bet", Command_gamble_payout = "gamble payout";
        private static Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();
        private static readonly System.Random Random = new System.Random();

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int[] pot);
                    Phrases.Dict.TryGetValue("Gamble1", out string phrase);
                    phrase = phrase.Replace("{Value1}", pot[0].ToString());
                    phrase = phrase.Replace("{Value2}", pot[1].ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_gamble_payout}", Command_gamble_payout);
                    phrase = phrase.Replace("{Command_gamble_bet}", Command_gamble_bet);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Gamble2", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_gamble_bet}", Command_gamble_bet);
                    phrase = phrase.Replace("{Value}", Command_Cost.ToString());
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gamble.Exec: {0}", e.Message));
            }
        }

        public static void Bet(ClientInfo _cInfo)
        {
            try
            {
                int currency = 0, bankCurrency = 0, cost = Command_Cost;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (Bank.IsEnabled && Bank.Direct_Payment)
                {
                    bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                }
                if (currency + bankCurrency >= cost)
                {
                    if (currency > 0)
                    {
                        if (currency < cost)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                            cost -= currency;
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        else
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                    }
                    else
                    {
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    DateTime lastgamble = new DateTime();
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGamble != null)
                    {
                        lastgamble = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGamble;
                    }
                    TimeSpan varTime = DateTime.Now - lastgamble;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    int delay = Delay_Between_Uses;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                delay = Delay_Between_Uses / 2;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                delay = Delay_Between_Uses / 2;
                            }
                        }
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        delay = Delay_Between_Uses / 2;
                    }
                    if (timepassed >= delay)
                    {
                        int[] pot;
                        if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out pot);
                        }
                        else
                        {
                            pot = new int[] { 1, 0 };
                        }
                        int gamble = Random.Next(pot[0] + 1);
                        if (gamble == 1)
                        {
                            int winnings = Command_Cost * 2;
                            if (pot[1] > 0)
                            {
                                winnings = pot[1] * 2;
                            }
                            if (!Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                Dict.Add(_cInfo.CrossplatformId.CombinedString, new int[] { pot[0] + 1, winnings });
                            }
                            else
                            {
                                Dict[_cInfo.CrossplatformId.CombinedString] = new int[] { pot[0] + 1, winnings };
                            }
                            Phrases.Dict.TryGetValue("Gamble5", out string phrase);
                            phrase = phrase.Replace("{Number}", pot[0].ToString());
                            phrase = phrase.Replace("{Value}", winnings.ToString());
                            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase = phrase.Replace("{Command_gamble_payout}", Command_gamble_payout);
                            phrase = phrase.Replace("{Command_gamble_bet}", Command_gamble_bet);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                Dict.Remove(_cInfo.CrossplatformId.CombinedString);
                            }
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGamble = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gamble6", out string phrase);
                            phrase = phrase.Replace("{Number}", pot[0].ToString());
                            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            phrase = phrase.Replace("{Command_gamble_bet}", Command_gamble_bet);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        int remaining = delay - timepassed;
                        Phrases.Dict.TryGetValue("Gamble7", out string phrase);
                        phrase = phrase.Replace("{DelayBetweenUses}", delay.ToString());
                        phrase = phrase.Replace("{TimeRemaining}", remaining.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Gamble3", out string phrase);
                    phrase = phrase.Replace("{Value}", Command_Cost.ToString());
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gamble.Bet: {0}", e.Message));
            }
        }

        public static void Payout(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int[] pot);
                    Wallet.AddCurrency(_cInfo.CrossplatformId.CombinedString, pot[1], true);
                    Dict.Remove(_cInfo.CrossplatformId.CombinedString);
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGamble = DateTime.Now;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Gamble4", out string phrase);
                    phrase = phrase.Replace("{Value}", pot[1].ToString());
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Gamble2", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gamble.Payout: {0}", e.Message));
            }
        }
    }
}
