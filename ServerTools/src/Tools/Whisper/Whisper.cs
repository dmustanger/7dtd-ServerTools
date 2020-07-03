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
            string _nameId = _message.Split(' ').First();
            _message = _message.Replace(_nameId, "");
            if (string.IsNullOrEmpty(_nameId) || string.IsNullOrEmpty(_message))
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Invalid name, id or message used to whisper" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            ClientInfo _recipientInfo = ConsoleHelper.ParseParamIdOrName(_nameId);
            if (_recipientInfo == null)
            {
                string _phrase14;
                if (!Phrases.Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = "Player {TargetName} was not found.";
                }
                _phrase14 = _phrase14.Replace("{TargetName}", _nameId);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase14 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_recipientInfo, LoadConfig.Chat_Response_Color + "(Whisper) " + _message + "[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_recipientInfo.playerId].LastWhisper = _cInfo.playerId;
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
            if (string.IsNullOrEmpty(_lastwhisper))
            {
                string _phrase15;
                if (!Phrases.Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = "No one has pm'd you.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase15 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_lastwhisper);
                if (_cInfo2 == null)
                {
                    string _phrase16;
                    if (!Phrases.Dict.TryGetValue(16, out _phrase16))
                    {
                        _phrase16 = "The player is not online.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase16 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + "(Whisper) " + _message + "[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
                    PersistentContainer.Instance.Players[_cInfo2.playerId].LastWhisper = _cInfo.playerId;
                    PersistentContainer.Instance.Save();
                }
            }
        }
    }
}