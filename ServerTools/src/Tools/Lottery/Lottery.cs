using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Lottery
    {
        public static bool IsEnabled = false;
        public static int Entry_Cost = 50;
        public static string Command_lottery = "lottery", Command_lottery_enter = "lottery enter";
        public static DateTime DrawTime = new DateTime();

        public static Dictionary<int, string> Entries = new Dictionary<int, string>();

        private static int LastNumber = 98;

        public static void Exec(ClientInfo _cInfo)
        {
            int total;
            if (Entries.Count == 0)
            {
                total = (Entry_Cost * 2) + PersistentContainer.Instance.LotteryPot;
            }
            else
            {
                total = (Entries.Count + 1) * Entry_Cost + PersistentContainer.Instance.LotteryPot;
            }
            Phrases.Dict.TryGetValue("Lottery1", out string phrase);
            phrase = phrase.Replace("{Value1}", total.ToString());
            phrase = phrase.Replace("{Value2}", Entry_Cost.ToString());
            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void EnterLottery(ClientInfo _cInfo)
        {
            int currency = 0, bankCurrency = 0, cost = Entry_Cost;
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
                int newTicket = NewTicket();
                if (newTicket < 1000)
                {
                    Entries.Add(newTicket, _cInfo.CrossplatformId.CombinedString);
                    if (Entries.Count == 1)
                    {
                        DrawTime = DateTime.Now.AddHours(1);
                        EventSchedule.AddToSchedule("Lottery", DrawTime);
                    }
                    TimeSpan varTime = DrawTime - DateTime.Now;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timeRemaining = (int)fractionalMinutes;
                    Phrases.Dict.TryGetValue("Lottery2", out string phrase);
                    phrase = phrase.Replace("{Value}", newTicket.ToString());
                    phrase = phrase.Replace("{Time}", timeRemaining.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Lottery8", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Lottery3", out string phrase);
                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void DrawLottery()
        {
            int winningNumbers = new Random().Next(100, LastNumber + 1);
            string numbers = winningNumbers.ToString();
            Phrases.Dict.TryGetValue("Lottery4", out string phrase);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                DrawOne(winningNumbers);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
            LastNumber = 98;
        }

        public static void DrawOne(int _winningNumbers)
        {
            string numbers = _winningNumbers.ToString();
            string number1 = numbers[0].ToString();
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + number1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                DrawTwo(_winningNumbers);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void DrawTwo(int _winningNumbers)
        {
            string numbers = _winningNumbers.ToString();
            string number2 = numbers[1].ToString();
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + number2 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                DrawThree(_winningNumbers);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void DrawThree(int _winningNumbers)
        {
            string numbers = _winningNumbers.ToString();
            string number3 = numbers[2].ToString();
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + number3 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            System.Timers.Timer singleUseTimer = new System.Timers.Timer(2000)
            {
                AutoReset = false
            };
            singleUseTimer.Start();
            singleUseTimer.Elapsed += (sender, e) =>
            {
                FinishDraw(_winningNumbers);
                singleUseTimer.Stop();
                singleUseTimer.Close();
                singleUseTimer.Dispose();
            };
        }

        public static void FinishDraw(int _winningNumbers)
        {
            if (Entries.ContainsKey(_winningNumbers))
            {
                Phrases.Dict.TryGetValue("Lottery5", out string phrase);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Entries.TryGetValue(_winningNumbers, out string id);
                Wallet.AddCurrency(id, (Entries.Count + 1) * Entry_Cost + PersistentContainer.Instance.LotteryPot, true);
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(id);
                if (cInfo != null)
                {
                    Phrases.Dict.TryGetValue("Lottery6", out phrase);
                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                PersistentContainer.Instance.LotteryPot += (Entries.Count + 1) * Entry_Cost;
                Phrases.Dict.TryGetValue("Lottery7", out string phrase);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            Entries.Clear();
            PersistentContainer.Instance.LotteryPot = 0;
            PersistentContainer.DataChange = true;
        }

        public static void DrawLotteryFast()
        {
            if (Entries.Count > 0)
            {
                int winningNumbers = new Random().Next(100, LastNumber + 1);
                string numbers = winningNumbers.ToString();
                Phrases.Dict.TryGetValue("Lottery4", out string phrase);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + numbers + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                if (Entries.ContainsKey(winningNumbers))
                {
                    Entries.TryGetValue(winningNumbers, out string id);
                    Wallet.AddCurrency(id, (Entries.Count + 1) * Entry_Cost + PersistentContainer.Instance.LotteryPot, true);
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(id);
                    if (cInfo != null)
                    {
                        Phrases.Dict.TryGetValue("Lottery6", out phrase);
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                LastNumber = 98;
                Entries.Clear();
                PersistentContainer.Instance.LotteryPot = 0;
                PersistentContainer.DataChange = true;
            }
        }

        public static void Alert()
        {
            Phrases.Dict.TryGetValue("Lottery9", out string phrase);
            phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
            phrase = phrase.Replace("{Command_lottery_enter}", Command_lottery_enter);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static int NewTicket()
        {
            int newTicket = LastNumber + 3;
            LastNumber = newTicket;
            return newTicket;
        }
    }
}