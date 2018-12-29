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
        public static Dictionary<int, int> KillStreak = new Dictionary<int, int>();
        private static string file = string.Format("Bounty_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string filepath = string.Format("{0}/BountyLogs/{1}", API.GamePath, file);

        public static void CreateFolder()
        {
            if (!Directory.Exists(API.GamePath + "/BountyLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/BountyLogs");
            }
        }

        public static void BountyList(ClientInfo _cInfo, string _playerName)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                if (_cInfo1 != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                    string _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    int _currentbounty;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _currentbounty);
                    _result.Dispose();
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
                _phrase910 = "Type /bounty Id# Value or /bounty Id# for the minimum bounty against this player.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase910 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void NewBounty(ClientInfo _cInfo, string _message, string _playerName)
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
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                            if (_cost < Minimum_Bounty)
                            {
                                _cost = Minimum_Bounty;
                            }
                            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                            if (_currentCoins >= _cost)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                                string _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                                DataTable _result = SQL.TQuery(_sql);
                                int _bounty;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bounty);
                                _result.Dispose();
                                _sql = string.Format("UPDATE Players SET bounty = {0} WHERE steamid = '{1}'", _bounty + _cost, _cInfo1.playerId);
                                SQL.FastQuery(_sql);
                                string _message1 = " you have added {Value} bounty to {PlayerName}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                string _message1 = " you do not have enough in your wallet for this bounty: {Value}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", to add a custom bounty type / bounty Id# Value, or minimum with /bounty Id#.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                        int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                        if (_currentCoins >= Minimum_Bounty)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Minimum_Bounty);
                            string _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            int _bounty;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bounty);
                            _result.Dispose();
                            _sql = string.Format("UPDATE Players SET bounty = {0} WHERE steamid = '{1}'", _bounty + Minimum_Bounty, _cInfo1.playerId);
                            SQL.FastQuery(_sql);
                            string _message1 = " you have added {Value} bounty to {PlayerName}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _message1 = " you do not have enough in your wallet for this bounty: {Value}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void PlayerKilled(EntityPlayer _player1, EntityPlayer _player2, ClientInfo _cInfo1, ClientInfo _cInfo2)
        {
            if (!_player1.IsFriendsWith(_player2) && !_player2.IsFriendsWith(_player1) && !_player1.Party.ContainsMember(_player2))
            {
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.ClanMember.Contains(_cInfo1.playerId) && ClanManager.ClanMember.Contains(_cInfo2.playerId))
                    {
                        string _sql1 = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                        DataTable _result1 = SQL.TQuery(_sql1);
                        string _clanName = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                        _result1.Dispose();
                        _sql1 = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                        DataTable _result2 = SQL.TQuery(_sql1);
                        string _clanName2 = _result2.Rows[0].ItemArray.GetValue(0).ToString();
                        _result2.Dispose();
                        Player p2 = PersistentContainer.Instance.Players[_cInfo2.playerId, false];
                        if (_clanName != "Unknown" && _clanName2 != "Unknown")
                        {
                            if (_clanName == _clanName2)
                            {
                                return;
                            }
                        }
                    }
                }
                string _sql = string.Format("SELECT bounty, bountyHunter FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int _bounty;
                int _hunterCountVictim;
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _bounty);
                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _hunterCountVictim);
                _result.Dispose();
                if (_bounty > 0)
                {
                    _sql = string.Format("SELECT playerSpentCoins, bountyHunter FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                    DataTable _result2 = SQL.TQuery(_sql);
                    int _playerSpentCoins;
                    int _hunterCountKiller;
                    int.TryParse(_result2.Rows[0].ItemArray.GetValue(0).ToString(), out _playerSpentCoins);
                    int.TryParse(_result2.Rows[0].ItemArray.GetValue(1).ToString(), out _hunterCountKiller);
                    _result2.Dispose();
                    if (Bonus > 0 && _hunterCountVictim >= Bonus)
                    {
                        _sql = string.Format("UPDATE Players SET playerSpentCoins = {0}, bountyHunter = {1} WHERE steamid = '{2}'", _playerSpentCoins + _bounty + Bonus, _hunterCountKiller + 1, _cInfo2.playerId);
                        SQL.FastQuery(_sql);
                    }
                    else
                    {
                        _sql = string.Format("UPDATE Players SET playerSpentCoins = {0}, bountyHunter = {1} WHERE steamid = '{2}'", _playerSpentCoins + _bounty, _hunterCountKiller + 1, _cInfo2.playerId);
                        SQL.FastQuery(_sql);
                    }
                    _sql = string.Format("UPDATE Players SET bounty = 0, bountyHunter = 0 WHERE steamid = '{0}'", _cInfo1.playerId);
                    SQL.FastQuery(_sql);
                    string _phrase912;
                    if (!Phrases.Dict.TryGetValue(912, out _phrase912))
                    {
                        _phrase912 = "{PlayerName} is a bounty hunter! {Victim} was snuffed out.";
                    }
                    _phrase912 = _phrase912.Replace("{PlayerName}", _cInfo2.playerName);
                    _phrase912 = _phrase912.Replace("{Victim}", _cInfo1.playerName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase912, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    using (StreamWriter sw = new StreamWriter(filepath, true))
                    {
                        sw.WriteLine(string.Format("{0}: {1} is a bounty hunter! {2} was snuffed out. Bounty was worth {3}", DateTime.Now, _cInfo2.playerName, _cInfo1.playerName, _bounty));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (Kill_Streak > 0)
                {
                    if (KillStreak.ContainsKey(_cInfo1.entityId))
                    {
                        KillStreak.Remove(_cInfo1.entityId);
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: Player {1} kill streak has come to an end by {2}.", DateTime.Now, _cInfo1.playerName, _cInfo2.playerName));
                            sw.WriteLine();
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    if (KillStreak.ContainsKey(_cInfo2.entityId))
                    {
                        int _value;
                        if (KillStreak.TryGetValue(_cInfo2.entityId, out _value))
                        {
                            int _newValue = _value + 1;
                            KillStreak[_cInfo2.entityId] = _newValue;
                            if (_newValue == Bounties.Kill_Streak)
                            {
                                string _phrase913;
                                if (!Phrases.Dict.TryGetValue(913, out _phrase913))
                                {
                                    _phrase913 = "{PlayerName} is on a kill streak! Their bounty has increased.";
                                }
                                _phrase913 = _phrase913.Replace("{PlayerName}", _cInfo2.playerName);
                                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _phrase913, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            if (_newValue >= Bounties.Kill_Streak)
                            {
                                _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                                DataTable _result3 = SQL.TQuery(_sql);
                                int _oldBounty;
                                int.TryParse(_result3.Rows[0].ItemArray.GetValue(0).ToString(), out _oldBounty);
                                _result3.Dispose();
                                _sql = string.Format("UPDATE Players SET bounty = {0} WHERE steamid = '{1}'", _oldBounty + Bounties.Bonus, _cInfo1.playerId);
                                SQL.FastQuery(_sql);
                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0}: {1} is on a kill streak of {2}. Their bounty has increased.", DateTime.Now, _cInfo2.playerName, _newValue));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                        }
                    }
                    else
                    {
                        KillStreak.Add(_cInfo2.entityId, 1);
                    }
                }
            }
        }
    }
}
