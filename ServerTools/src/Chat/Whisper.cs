using System.Data;

namespace ServerTools
{
    public class Whisper
    {
        public static void Send(ClientInfo _cInfo, string _message)
        {
            if (_message.StartsWith("w "))
            {
                _message = _message.Replace("w ", "");
            }
            if (_message.StartsWith("W "))
            {
                _message = _message.Replace("W ", "");
            }
            if(_message.StartsWith("pm "))
            {
                _message = _message.Replace("pm ", "");
            }
            if(_message.StartsWith("PM "))
            {
                _message = _message.Replace("PM ", "");
            }
            string[] _strings = _message.Split(new char[] { ' ' }, 2);
            _strings[1] = _strings[1].TrimStart();
            ClientInfo _cInfo1 = ConsoleHelper.ParseParamIdOrName(_strings[0]);
            if (_cInfo1 == null)
            {
                string _phrase14;
                if (!Phrases.Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = "player {TargetName} was not found.";
                }
                _phrase14 = _phrase14.Replace("{TargetName}", _strings[0]);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase14 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _strings[1] + "[-]", _cInfo.entityId, _cInfo.playerName, EChatType.Whisper, null);
                string _sql = string.Format("UPDATE Players SET lastwhisper = '{0}' WHERE steamid = '{1}'", _cInfo.playerId, _cInfo1.playerId);
                SQL.FastQuery(_sql);
            }
        }

        public static void Reply(ClientInfo _cInfo, string _message)
        {
            if (_message.StartsWith("R "))
            {
                _message = _message.Replace("R ", "");
            }
            if (_message.StartsWith("r "))
            {
                _message = _message.Replace("r ", "");
            }
            if (_message.StartsWith("re "))
            {
                _message = _message.Replace("re ", "");
            }
            if (_message.StartsWith("RE "))
            {
                _message = _message.Replace("RE ", "");
            }
            string _sql = string.Format("SELECT lastwhisper FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _lastwhisper = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_lastwhisper == "Unknown")
            {
                string _phrase15;
                if (!Phrases.Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = "no one has pm'd you.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase15 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {

                ClientInfo _cInfo1 = ConnectionManager.Instance.Clients.ForPlayerId(_lastwhisper);
                if (_cInfo1 == null)
                {
                    string _phrase16;
                    if (!Phrases.Dict.TryGetValue(16, out _phrase16))
                    {
                        _phrase16 = "the player is not online.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase16 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, _cInfo.playerName, EChatType.Whisper, null);
                    _sql = string.Format("UPDATE Players SET lastwhisper = '{0}' WHERE steamid = '{1}'", _cInfo.playerId, _cInfo1.playerId);
                    SQL.FastQuery(_sql);
                }
            }
        }
    }
}