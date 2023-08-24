using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerTools
{
    class Bounties
    {
        public static bool IsEnabled = false;
        public static int Minimum_Bounty = 5, Kill_Streak = 0, Bonus = 25;
        public static string Command_bounty = "bounty";

        public static Dictionary<int, int> KillStreak = new Dictionary<int, int>();

        private static readonly string file = string.Format("Bounty_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string filepath = string.Format("{0}/Logs/BountyLogs/{1}", API.ConfigPath, file);

        public static void BountyList(ClientInfo _cInfo)
        {
            List<ClientInfo> clients = GeneralOperations.ClientList();
            if (clients != null && clients.Count > 0)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    ClientInfo cInfo2 = clients[i];
                    if (cInfo2 != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo2.CrossplatformId.CombinedString] != null)
                        {
                            int currentbounty = PersistentContainer.Instance.Players[cInfo2.CrossplatformId.CombinedString].Bounty;
                            if (currentbounty > 0)
                            {
                                Phrases.Dict.TryGetValue("Bounties8", out string phrase);
                                phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                phrase = phrase.Replace("{EntityId}", cInfo2.entityId.ToString());
                                phrase = phrase.Replace("{Value}", currentbounty.ToString());
                                phrase = phrase.Replace("{Minimum}", Minimum_Bounty.ToString());
                                phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                Phrases.Dict.TryGetValue("Bounties7", out string phrase1);
                phrase1 = phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase1 = phrase1.Replace("{Command_bounty}", Command_bounty);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Bounties9", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void NewBounty(ClientInfo _cInfo, string _message)
        {
            try
            {
                if (_message.Contains(" "))
                {
                    string[] idAndBounty = _message.Split(' ').ToArray();
                    if (int.TryParse(idAndBounty[0], out int id))
                    {
                        if (int.TryParse(idAndBounty[1], out int bounty))
                        {
                            ClientInfo cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(id);
                            if (cInfo2 != null)
                            {
                                if (bounty < Minimum_Bounty)
                                {
                                    bounty = Minimum_Bounty;
                                }
                                int currency = 0, bankCurrency = 0, cost = bounty;
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
                                    PersistentContainer.Instance.Players[cInfo2.CrossplatformId.CombinedString].Bounty += bounty;
                                    PersistentContainer.DataChange = true;
                                    using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                    {
                                        sw.WriteLine("{0}: '{1}' named '{2}' added '{3}' bounty to '{4}' '{5}' named '{6}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, bounty, cInfo2.PlatformId.CombinedString, cInfo2.CrossplatformId.CombinedString, cInfo2.playerName);
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                    Phrases.Dict.TryGetValue("Bounties5", out string phrase);
                                    phrase = phrase.Replace("{Value}", bounty.ToString());
                                    phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Bounties4", out string phrase);
                                    phrase = phrase.Replace("{Value}", bounty.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Bounties6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(_message, out int _id))
                    {
                        ClientInfo cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_id);
                        if (cInfo2 != null)
                        {
                            int currency = 0, bankCurrency = 0, cost = Minimum_Bounty;
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
                                PersistentContainer.Instance.Players[cInfo2.CrossplatformId.CombinedString].Bounty += Minimum_Bounty;
                                PersistentContainer.DataChange = true;
                                using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' added '{4}' bounty to '{5}' '{6}' named '{7}'", DateTime.Now, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, Minimum_Bounty, cInfo2.PlatformId.CombinedString, cInfo2.CrossplatformId.CombinedString, cInfo2.playerName));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                Phrases.Dict.TryGetValue("Bounties5", out string phrase);
                                phrase = phrase.Replace("{Value}", Minimum_Bounty.ToString());
                                phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Bounties4", out string phrase);
                                phrase = phrase.Replace("{Value}", Minimum_Bounty.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bounties.NewBounty: {0}", e.Message));
            }
        }

        public static void PlayerKilled(EntityPlayer _player1, EntityPlayer _player2, ClientInfo _cInfo1, ClientInfo _cInfo2)
        {
            try
            {
                if (_cInfo1.CrossplatformId != null && _player1 != null && _player2 != null && _cInfo2.CrossplatformId != null)
                {
                    PersistentPlayerData _ppd1 = GeneralOperations.GetPersistentPlayerDataFromId(_cInfo1.CrossplatformId.CombinedString);
                    PersistentPlayerData _ppd2 = GeneralOperations.GetPersistentPlayerDataFromId(_cInfo2.CrossplatformId.CombinedString);
                    if (_ppd1.ACL != null && !_ppd1.ACL.Contains(_cInfo2.InternalId) && _ppd2.ACL != null && !_ppd2.ACL.Contains(_cInfo1.InternalId))
                    {
                        if (_player1.Party != null && !_player1.Party.ContainsMember(_player2) && _player2.Party != null && !_player2.Party.ContainsMember(_player1))
                        {
                            ProcessPlayerKilled(_cInfo1, _cInfo2);
                        }
                    }
                    else if (_player1.Party != null && !_player1.Party.ContainsMember(_player2) && _player2.Party != null && !_player2.Party.ContainsMember(_player1))
                    {
                        ProcessPlayerKilled(_cInfo1, _cInfo2);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bounties.PlayerKilled: {0}", e.Message));
            }
        }

        private static void ProcessPlayerKilled(ClientInfo _cInfo1, ClientInfo _cInfo2)
        {
            try
            {
                int victimBounty = PersistentContainer.Instance.Players[_cInfo1.CrossplatformId.CombinedString].Bounty;
                int victimBountyHunter = PersistentContainer.Instance.Players[_cInfo1.CrossplatformId.CombinedString].BountyHunter; //victim kill streak
                if (victimBounty > 0)
                {
                    int killerWallet = PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].PlayerWallet;
                    int killerBounty = PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].Bounty;
                    int killerBountyHunter = PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].BountyHunter; //killer kill streak
                    if (Kill_Streak > 0)
                    {
                        int newKillerBountyHunter = killerBountyHunter + 1;
                        if (newKillerBountyHunter >= Kill_Streak)
                        {
                            PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].PlayerWallet = killerWallet + victimBounty + Bonus;
                            PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].BountyHunter = newKillerBountyHunter;
                            PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].Bounty = killerBounty + Bonus;
                            PersistentContainer.DataChange = true;
                            using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                            {
                                sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has collected the bounty on '{4}' '{5}' named '{6}'", DateTime.Now, _cInfo2.PlatformId.CombinedString, _cInfo2.CrossplatformId.CombinedString, _cInfo2.playerName, _cInfo1.PlatformId.CombinedString, _cInfo1.CrossplatformId.CombinedString, _cInfo1.playerName));
                                sw.WriteLine();
                                sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has collected '{4}' bounties without dying. Their kill streak and bounty have increased", DateTime.Now, _cInfo2.PlatformId.CombinedString, _cInfo2.CrossplatformId.CombinedString, _cInfo2.playerName, newKillerBountyHunter));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                            Phrases.Dict.TryGetValue("Bounties1", out string phrase);
                            phrase = phrase.Replace("{PlayerName}", _cInfo2.playerName);
                            phrase = phrase.Replace("{Value}", newKillerBountyHunter.ToString());
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else if (killerBountyHunter + 1 < Kill_Streak)
                        {
                            PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].PlayerWallet = killerWallet + victimBounty;
                            PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].BountyHunter = newKillerBountyHunter;
                            PersistentContainer.DataChange = true;
                        }
                        if (victimBountyHunter >= Kill_Streak)
                        {
                            Phrases.Dict.TryGetValue("Bounties2", out string phrase);
                            phrase = phrase.Replace("{Victim}", _cInfo1.playerName);
                            phrase = phrase.Replace("{Killer}", _cInfo2.playerName);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].PlayerWallet = killerWallet + victimBounty;
                        PersistentContainer.DataChange = true;
                        using (StreamWriter sw = new StreamWriter(filepath, true, Encoding.UTF8))
                        {
                            sw.WriteLine(string.Format("{0}: '{1}' '{2}' named '{3}' has collected the bounty on '{4}' '{5}' named '{6}'", DateTime.Now, _cInfo2.PlatformId.CombinedString, _cInfo2.CrossplatformId.CombinedString, _cInfo2.playerName, _cInfo1.PlatformId.CombinedString, _cInfo1.CrossplatformId.CombinedString, _cInfo1.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    PersistentContainer.Instance.Players[_cInfo1.CrossplatformId.CombinedString].Bounty = 0;
                    PersistentContainer.Instance.Players[_cInfo1.CrossplatformId.CombinedString].BountyHunter = 0;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Bounties3", out string phrase1);
                    phrase1 = phrase1.Replace("{Victim}", _cInfo1.playerName);
                    phrase1 = phrase1.Replace("{Killer}", _cInfo2.playerName);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Bounties.ProcessPlayerKilled: {0}", e.Message));
            }
        }

        public static void ConsoleEdit(string _id, int _value)
        {
            int oldBounty = PersistentContainer.Instance.Players[_id].Bounty;
            if (_value > 0)
            {
                int newBounty = oldBounty + _value;
                PersistentContainer.Instance.Players[_id].Bounty = newBounty;
                PersistentContainer.DataChange = true;
                SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for '{0}'. The new value is set to '{1}'", _id, newBounty));
            }
            else
            {
                int newBounty = oldBounty - _value;
                if (newBounty < 0)
                {
                    PersistentContainer.Instance.Players[_id].Bounty = 0;
                    PersistentContainer.DataChange = true;
                    SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for '{0}'. The new value is '{1}'", _id, 0));
                }
                else
                {
                    PersistentContainer.Instance.Players[_id].Bounty = newBounty;
                    PersistentContainer.DataChange = true;
                    SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for '{0}'. The new value is '{1}'", _id, newBounty));
                }
            }
        }

        public static void ConsoleRemoveBounty(string _id)
        {
            PersistentPlayer p = PersistentContainer.Instance.Players[_id];
            if (p != null)
            {
                p.Bounty = 0;
                PersistentContainer.DataChange = true;
                SdtdConsole.Instance.Output(string.Format("Bounty was removed successfully for '{0}'", _id));
            }
        }

        public static void ConsoleBountyList()
        {
            List<ClientInfo> clientList = GeneralOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null)
                    {
                        int currentbounty = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bounty;
                        if (currentbounty > 0)
                        {
                            SdtdConsole.Instance.Output(string.Format("Entity Id: '{0}' named '{1}'. Bounty total '{2}'", cInfo.entityId, cInfo.playerName, currentbounty));
                        }
                    }
                }
            }
        }
    }
}
