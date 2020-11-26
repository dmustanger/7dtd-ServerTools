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
            if (string.IsNullOrEmpty(_nameId))
            {
                Phrases.Dict.TryGetValue(51, out string _phrase51);
                _phrase51 = _phrase51.Replace("{PlayerName}", _nameId);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase51 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (string.IsNullOrEmpty(_message))
            {
                Phrases.Dict.TryGetValue(53, out string _phrase53);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase53 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            ClientInfo _recipientInfo = ConsoleHelper.ParseParamIdOrName(_nameId);
            if (_recipientInfo == null)
            {
                Phrases.Dict.TryGetValue(51, out string _phrase51);
                _phrase51 = _phrase51.Replace("{PlayerName}", _nameId);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase51 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                PersistentContainer.Instance.Players[_recipientInfo.playerId].LastWhisper = _cInfo.playerId;
                ChatHook.ChatMessage(_recipientInfo, LoadConfig.Chat_Response_Color + "(Whisper) " + _message + "[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue(52, out string _phrase52);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase52 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_lastwhisper);
                if (_cInfo2 == null)
                {
                    Phrases.Dict.TryGetValue(54, out string _phrase54);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase54 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo2.playerId].LastWhisper = _cInfo.playerId;
                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + "(Whisper) " + _message + "[-]", -1, _cInfo.playerName, EChatType.Whisper, null);
                }
            }
        }
    }
}