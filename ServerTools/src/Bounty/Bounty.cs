using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ServerTools
{
    class Bounties
    {
        public static bool IsEnabled = false;
        public static int Bounty = 5, Kill_Streak = 0;

        public static void BountyList(ClientInfo _cInfo, string _playerName)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                if (_cInfo1 != null)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                    int _cost = _player.Level * Bounty;
                    string _sql = string.Format("SELECT bounty FROM Players WHERE steamid = '{0}'", _cInfo1.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    int _currentbounty;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _currentbounty);
                    _result.Dispose();
                    string _phrase911;
                    if (!Phrases.Dict.TryGetValue(911, out _phrase911))
                    {
                        _phrase911 = "{PlayerName}, # {EntityId}. Current bounty: {CurrentBounty}. Minimum buy in {Cost} {CoinName}.";
                    }
                    _phrase911 = _phrase911.Replace("{PlayerName}", _cInfo1.playerName);
                    _phrase911 = _phrase911.Replace("{EntityId}", _cInfo1.entityId.ToString());
                    _phrase911 = _phrase911.Replace("{CurrentBounty}", _currentbounty.ToString());
                    _phrase911 = _phrase911.Replace("{Cost}", _cost.ToString());
                    _phrase911 = _phrase911.Replace("{CoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase911), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            string _phrase910;
            if (!Phrases.Dict.TryGetValue(910, out _phrase910))
            {
                _phrase910 = "Type /bounty Id# Value or /bounty Id# for the minimum bounty against this player.";
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase910), Config.Server_Response_Name, false, "ServerTools", false));
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
                        ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                        if (_cInfo1 != null)
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                            int _minimum = _player.Level * Bounty;
                            if (_cost < _minimum)
                            {
                                _cost = _minimum;
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
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have added {2} bounty to {3}.[-]", Config.Chat_Response_Color, _playerName, _cost, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you do not have enough in your wallet for this bounty: {3}.[-]", Config.Chat_Response_Color, _playerName, _cost), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} to add a custom bounty type /bounty Id# Value, or minimum with /bounty Id#.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                if (int.TryParse(_message, out _id))
                {
                    ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                    if (_cInfo1 != null)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                        _cost = _player.Level * Bounty;
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
                            
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have added {2} bounty to {3}.[-]", Config.Chat_Response_Color, _playerName, _cost, _cInfo1.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you do not have enough {2} to the bounty.[-]", Config.Chat_Response_Color, _playerName, Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }
    }
}
