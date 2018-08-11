using System.Data;

namespace ServerTools
{
    class Mogul
    {
        public static bool IsEnabled = false;

        public static void TopThree(ClientInfo _cInfo, bool _announce)
        {
            int _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _name1 = "-", _name2 = "-", _name3 = "-";
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                Player p = PersistentContainer.Instance.Players[_id, false];
                {
                    if (p != null)
                    {
                        if (p.PlayerName != null)
                        {
                            string _sql = string.Format("SELECT bank FROM Players WHERE steamid = '{0}'", _id);
                            DataTable _result = SQL.TQuery(_sql);
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _bank);
                            _result.Dispose();
                            int _total = 0;
                            if (Wallet.IsEnabled && Bank.IsEnabled)
                            {
                                _total = p.Wallet + _bank;
                            }
                            else if (Wallet.IsEnabled && !Bank.IsEnabled)
                            {
                                _total = _bank;
                            }
                            else if (!Wallet.IsEnabled && Bank.IsEnabled)
                            {
                                _total = p.Wallet;
                            }
                            if (_total > _topScore1)
                            {
                                _topScore3 = _topScore2;
                                _name3 = _name2;
                                _topScore2 = _topScore1;
                                _name2 = _name1;
                                _topScore1 = _total;
                                _name1 = p.PlayerName;
                            }
                            else
                            {
                                if (_total > _topScore2)
                                {
                                    _topScore3 = _topScore2;
                                    _name3 = _name2;
                                    _topScore2 = _total;
                                    _name2 = p.PlayerName;
                                }
                                else
                                {
                                    if (_total > _topScore3)
                                    {
                                        _topScore3 = _total;
                                        _name3 = p.PlayerName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            string _phrase965;
            if (!Phrases.Dict.TryGetValue(965, out _phrase965))
            {
                _phrase965 = "Richest Players:";
            }
            string _phrase966;
            if (!Phrases.Dict.TryGetValue(966, out _phrase966))
            {
                _phrase966 = "{Name1}, valued at {Top1}. {Name2}, valued at {Top2}. {Name3}, valued at {Top3}";
            }
            _phrase966 = _phrase966.Replace("{Name1}", _name1);
            _phrase966 = _phrase966.Replace("{Top1}", _topScore1.ToString());
            _phrase966 = _phrase966.Replace("{Name2}", _name2);
            _phrase966 = _phrase966.Replace("{Top2}", _topScore2.ToString());
            _phrase966 = _phrase966.Replace("{Name3}", _name3);
            _phrase966 = _phrase966.Replace("{Top3}", _topScore3.ToString());
            if (_announce)
            {
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase965), Config.Server_Response_Name, false, "ServerTools", false);
                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase966), Config.Server_Response_Name, false, "ServerTools", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase965), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase966), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}