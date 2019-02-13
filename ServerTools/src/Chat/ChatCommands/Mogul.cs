using System.Data;

namespace ServerTools
{
    class Mogul
    {
        public static bool IsEnabled = false;
        public static string Command101 = "mogul";

        public static void TopThree(ClientInfo _cInfo, bool _announce)
        {
            int _topScore1 = 0, _topScore2 = 0, _topScore3 = 0;
            string _name1 = "-", _name2 = "-", _name3 = "-";
            string _sql = "SELECT playername, bank, wallet FROM Players";
            DataTable _result = SQL.TQuery(_sql);
            int _bank;
            int _wallet;
            foreach (DataRow row in _result.Rows)
            {
                int.TryParse(row[0].ToString(), out _bank);
                int.TryParse(row[1].ToString(), out _wallet);
                int _total = 0;
                if (Wallet.IsEnabled && Bank.IsEnabled)
                {
                    _total = _wallet + _bank;
                }
                if (Wallet.IsEnabled && !Bank.IsEnabled)
                {
                    _total = _bank;
                }
                if (!Wallet.IsEnabled && Bank.IsEnabled)
                {
                    _total = _wallet;
                }
                if (_total > _topScore1)
                {
                    _topScore3 = _topScore2;
                    _name3 = _name2;
                    _topScore2 = _topScore1;
                    _name2 = _name1;
                    _topScore1 = _total;
                    _name1 = _result.Rows[0].ItemArray.GetValue(0).ToString();
                }
                else
                {
                    if (_total > _topScore2)
                    {
                        _topScore3 = _topScore2;
                        _name3 = _name2;
                        _topScore2 = _total;
                        _name2 = _result.Rows[0].ItemArray.GetValue(0).ToString();
                    }
                    else
                    {
                        if (_total > _topScore3)
                        {
                            _topScore3 = _total;
                            _name3 = _result.Rows[0].ItemArray.GetValue(0).ToString();
                        }
                    }
                }
            }
            _result.Dispose();
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
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase965 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase966 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase965 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase966 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}