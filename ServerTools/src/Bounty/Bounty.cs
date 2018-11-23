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
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase911 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            string _phrase910;
            if (!Phrases.Dict.TryGetValue(910, out _phrase910))
            {
                _phrase910 = "type /bounty Id# Value or /bounty Id# for the minimum bounty against this player.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase910 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
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
                                string _message1 = "you have added {Value} bounty to {PlayerName}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                string _message1 = "you do not have enough in your wallet for this bounty: {Value}.[-]";
                                _message1 = _message1.Replace("{Value}", _cost.ToString());
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                    }
                    else
                    {
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", to add a custom bounty type / bounty Id# Value, or minimum with /bounty Id#.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
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
                            string _message1 = "you have added {Value} bounty to {PlayerName}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            _message1 = _message1.Replace("{PlayerName}", _cInfo1.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            string _message1 = "you do not have enough in your wallet for this bounty: {Value}.[-]";
                            _message1 = _message1.Replace("{Value}", Minimum_Bounty.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _message1 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                        }
                    }
                }
            }
        }
    }
}
