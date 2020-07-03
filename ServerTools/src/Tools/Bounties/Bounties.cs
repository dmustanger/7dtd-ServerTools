using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ServerTools
{
    class Bounties
    {
        public static bool IsEnabled = false;
        public static int Minimum_Bounty = 5, Kill_Streak = 0, Bonus = 25;
        public static string Command83 = "bounty";
        public static Dictionary<int, int> KillStreak = new Dictionary<int, int>();
        private static string file = string.Format("Bounty_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/Logs/BountyLogs/{1}", API.ConfigPath, file);

        public static void BountyList(ClientInfo _cInfo)
        {
            List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = ClientInfoList[i];
                if (_cInfo1 != null)
                {
                    int _currentbounty = PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty;
                    if (_currentbounty > 0)
                    {
                        string _phrase911;
                        if (!Phrases.Dict.TryGetValue(911, out _phrase911))
                        {
                            _phrase911 = "{PlayerName}, # {EntityId}. Current bounty: {CurrentBounty}. Minimum bounty is {Minimum} {CoinName}.";
                        }
                        _phrase911 = _phrase911.Replace("{PlayerName}", _cInfo1.playerName);
                        _phrase911 = _phrase911.Replace("{EntityId}", _cInfo1.entityId.ToString());
                        _phrase911 = _phrase911.Replace("{CurrentBounty}", _currentbounty.ToString());
                        _phrase911 = _phrase911.Replace("{Minimum}", Minimum_Bounty.ToString());
                        _phrase911 = _phrase911.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase911 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            string _phrase910;
            if (!Phrases.Dict.TryGetValue(910, out _phrase910))
            {
                _phrase910 = "Type {CommandPrivate}{Command83} Id# Value or {CommandPrivate}{Command83} Id# for the minimum bounty against this player.";
            }
            _phrase910 = _phrase910.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase910 = _phrase910.Replace("{Command83}", Command83);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase910 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void NewBounty(ClientInfo _cInfo, string _message)
        {
            int _id;
            string[] _idAndBounty = { };
            int _cost;
            if (_message.Contains(" "))
            {
                _idAndBounty = _message.Split(' ').ToArray();
                if (int.TryParse(_idAndBounty[0], out _id))
                {
                    if (int.TryParse(_idAndBounty[1], out _cost))
                    {
                        ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.ForEntityId(_id);
                        if (_cInfo1 != null)
                        {
                            if (_cost < Minimum_Bounty)
                            {
                                _cost = Minimum_Bounty;
                            }
                            int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                            if (_currentCoins >= _cost)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                                int _currentbounty = PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty;
                                PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty = _currentbounty + _cost;
                                PersistentContainer.Instance.Save();
                                string _message1 = "You have added {Value} bounty to {PlayerName}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                string _message1 = "You do not have enough in your wallet for this bounty: {Value}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "To add a custom bounty value, type " + ChatHook.Command_Private + Command83 + " Id# Value, or the minimum with " + ChatHook.Command_Private + Command83 + " Id#.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                if (int.TryParse(_message, out _id))
                {
                    ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.ForEntityId(_id);
                    if (_cInfo1 != null)
                    {
                        int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                        if (_currentCoins >= Minimum_Bounty)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Minimum_Bounty);
                            int _currentbounty = PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty;
                            PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty = _currentbounty + Minimum_Bounty;
                            PersistentContainer.Instance.Save();
                            string _message1 = "You have added {Value} bounty to {PlayerName}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _message1 = "You do not have enough in your wallet for this bounty: {Value}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void PlayerKilled(EntityPlayer _player1, EntityPlayer _player2, ClientInfo _cInfo1, ClientInfo _cInfo2)
        {
            if (!_player1.IsFriendsWith(_player2) && !_player2.IsFriendsWith(_player1) && !_player1.Party.ContainsMember(_player2) && !_player2.Party.ContainsMember(_player1))
            {
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.ClanMember.Contains(_cInfo1.playerId) && ClanManager.ClanMember.Contains(_cInfo2.playerId))
                    {
                        string _clanName1 = PersistentContainer.Instance.Players[_cInfo1.playerId].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_cInfo2.playerId].ClanName;
                        if (string.IsNullOrEmpty(_clanName1) && string.IsNullOrEmpty(_clanName2))
                        {
                            if (_clanName1 == _clanName2)
                            {
                                return;
                            }
                        }
                    }
                }
                int _victimBounty = PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty;
                int _victimBountyHunter = PersistentContainer.Instance.Players[_cInfo1.playerId].BountyHunter;
                if (_victimBounty > 0)
                {
                    int _killerWallet = PersistentContainer.Instance.Players[_cInfo2.playerId].PlayerWallet;
                    int _killerBounty = PersistentContainer.Instance.Players[_cInfo2.playerId].Bounty;
                    int _killerBountyHunter = PersistentContainer.Instance.Players[_cInfo2.playerId].BountyHunter;
                    if (Kill_Streak > 0)
                    {
                        int _victimBountyPlus = _victimBounty + Bonus;
                        if (_killerBountyHunter + 1 > Kill_Streak)
                        {
                            PersistentContainer.Instance.Players[_cInfo2.playerId].PlayerWallet = _killerWallet + _victimBountyPlus;
                            PersistentContainer.Instance.Players[_cInfo2.playerId].BountyHunter = _killerBountyHunter + 1;
                            PersistentContainer.Instance.Players[_cInfo2.playerId].Bounty = _killerBounty + Bonus;
                            string _phrase913;
                            if (!Phrases.Dict.TryGetValue(913, out _phrase913))
                            {
                                _phrase913 = "{PlayerName} has collected {Kill_Streak} bounties without dying! Their bounty has increased.";
                            }
                            _phrase913 = _phrase913.Replace("{PlayerName}", _cInfo2.playerName);
                            _phrase913 = _phrase913.Replace("{Kill_Streak}", _killerBountyHunter + 1.ToString());
                            ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _phrase913, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has collected {2} bounties without dying. Their bounty has increased.", DateTime.Now, _cInfo2.playerName, _killerBountyHunter + 1));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        else if (_killerBountyHunter + 1 == Kill_Streak)
                        {
                            PersistentContainer.Instance.Players[_cInfo2.playerId].PlayerWallet = _killerWallet + _victimBountyPlus;
                            PersistentContainer.Instance.Players[_cInfo2.playerId].BountyHunter = _killerBountyHunter + 1;
                            PersistentContainer.Instance.Players[_cInfo2.playerId].Bounty = _killerBounty + Bonus;
                            string _phrase913;
                            if (!Phrases.Dict.TryGetValue(913, out _phrase913))
                            {
                                _phrase913 = "{PlayerName} has collected {Kill_Streak} bounties without dying! Their bounty has increased.";
                            }
                            _phrase913 = _phrase913.Replace("{PlayerName}", _cInfo2.playerName);
                            _phrase913 = _phrase913.Replace("{Kill_Streak}", _killerBountyHunter + 1.ToString());
                            ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _phrase913, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: {1} has collected {2} bounties without dying. Their bounty has increased.", DateTime.Now, _cInfo2.playerName, _killerBountyHunter + 1));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        else if (_killerBountyHunter + 1 < Kill_Streak)
                        {
                            PersistentContainer.Instance.Players[_cInfo2.playerId].PlayerWallet = _killerWallet + _victimBounty;
                            PersistentContainer.Instance.Players[_cInfo2.playerId].BountyHunter = _killerBountyHunter + 1;
                        }
                        if (_victimBountyHunter >= Kill_Streak)
                        {
                            string _message = "Player {Victim}' kill streak has come to an end by {Killer}.";
                            _message = _message.Replace("{Victim}", _cInfo1.playerName);
                            _message = _message.Replace("{Killer}", _cInfo2.playerName);
                            ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            using (StreamWriter sw = new StreamWriter(filepath, true))
                            {
                                sw.WriteLine(string.Format("{0}: Player {1}' kill streak has come to an end by {2}.", DateTime.Now, _cInfo1.playerName, _cInfo2.playerName));
                                sw.WriteLine();
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                    else if (Kill_Streak <= 0)
                    {
                        PersistentContainer.Instance.Players[_cInfo2.playerId].PlayerWallet = _killerWallet + _victimBounty;
                    }
                    PersistentContainer.Instance.Players[_cInfo1.playerId].Bounty = 0;
                    PersistentContainer.Instance.Players[_cInfo1.playerId].BountyHunter = 0;
                    PersistentContainer.Instance.Save();
                    string _message2 = "Player {Killer}' has collected the bounty of {Victim}.";
                    _message2 = _message2.Replace("{Victim}", _cInfo1.playerName);
                    _message2 = _message2.Replace("{Killer}", _cInfo2.playerName);
                    ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _message2, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
        }

        public static void ConsoleEdit(string _id, int _value)
        {
            int _oldBounty = PersistentContainer.Instance.Players[_id].Bounty;
            if (_value > 0)
            {
                PersistentContainer.Instance.Players[_id].Bounty = _oldBounty + _value;
                PersistentContainer.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for {0}. The new value is set to {1}", _id, _oldBounty + _value));
            }
            else
            {
                if (_oldBounty - _value < 0)
                {
                    PersistentContainer.Instance.Players[_id].Bounty = 0;
                    SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for {0}. The new value is {1}", _id, 0));
                }
                else
                {
                    PersistentContainer.Instance.Players[_id].Bounty = _oldBounty - _value;
                    SdtdConsole.Instance.Output(string.Format("Bounty edit was successful for {0}. The new value is {1}", _id, _oldBounty - _value));
                }
                PersistentContainer.Instance.Save();
            }
        }

        public static void ConsoleRemoveBounty(string _id)
        {
            PersistentPlayer p = PersistentContainer.Instance.Players[_id];
            if (p != null)
            {
                p.Bounty = 0;
                PersistentContainer.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("Bounty was removed successfully for {0}", _id));
            }
        }

        public static void ConsoleBountyList()
        {
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfo = ClientInfoList[i];
                if (_cInfo != null)
                {
                    int _currentbounty = PersistentContainer.Instance.Players[_cInfo.playerId].Bounty;
                    if (_currentbounty > 0)
                    {
                        SdtdConsole.Instance.Output(string.Format("{0}, # {1}. Current bounty: {2}.", _cInfo.playerName, _cInfo.entityId, _currentbounty));
                    }
                }
            }
        }
    }
}
