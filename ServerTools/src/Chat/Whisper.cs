using System.Data;
using System.Linq;

namespace ServerTools
{
    public class Whisper
    {
        public static bool IsEnabled = false;
        public static string Command120 = "pmessage", Command121 = "pm", Command122 = "rmessage", Command123 = "rm";

        public static void Send(ClientInfo _cInfo, string _message)
        {
            if (_message.StartsWith(Command120 + " "))
            {
                _message = _message.Replace(Command120 + " ", "");
            }
            if (_message.StartsWith(Command121 + " "))
            {
                _message = _message.Replace(Command121 + " ", "");
            }
            string _nameId = _message.ElementAt(0).ToString();
            string _msg = _message.ElementAt(1).ToString();
            ClientInfo _cInfo1 = ConsoleHelper.ParseParamIdOrName(_nameId);
            if (_cInfo1 == null)
            {
                string _phrase14;
                if (!Phrases.Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = " player {TargetName} was not found.";
                }
                _phrase14 = _phrase14.Replace("{TargetName}", _nameId);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase14 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _msg + "[-]", _cInfo.entityId, _cInfo.playerName, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_cInfo1.playerId].LastWhisper = _cInfo.playerId;
                PersistentContainer.Instance.Save();
            }
        }

        public static void Reply(ClientInfo _cInfo, string _message)
        {
            if (_message.StartsWith(Command122 + " "))
            {
                _message = _message.Replace(Command122 + " ", "");
            }
            if (_message.StartsWith(Command123 + " "))
            {
                _message = _message.Replace(Command123 + " ", "");
            }
            string _lastwhisper = PersistentContainer.Instance.Players[_cInfo.playerId].LastWhisper;
            if (_lastwhisper == "")
            {
                string _phrase15;
                if (!Phrases.Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = " no one has pm'd you.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase15 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {

                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_lastwhisper);
                if (_cInfo2 == null)
                {
                    string _phrase16;
                    if (!Phrases.Dict.TryGetValue(16, out _phrase16))
                    {
                        _phrase16 = " the player is not online.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase16 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, _cInfo.playerName, EChatType.Whisper, null);
                    PersistentContainer.Instance.Players[_cInfo2.playerId].LastWhisper = _cInfo.playerId;
                    PersistentContainer.Instance.Save();
                }
            }
        }
    }
}