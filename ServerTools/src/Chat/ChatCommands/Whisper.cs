namespace ServerTools
{
    public class Whisper
    {
        public static void Send(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace("PM ", "");
            _message = _message.Replace("pm ", "");
            _message = _message.Replace("W ", "");
            _message = _message.Replace("w ", "");
            string[] _strings = _message.Split(new char[] { ' ' }, 2);
            _strings[1] = _strings[1].TrimStart();
            ClientInfo _targetInfo = ConsoleHelper.ParseParamIdOrName(_strings[0]);
            if (_targetInfo == null)
            {
                string _phrase14 = "{SenderName} player {TargetName} was not found.";
                if (!Phrases.Dict.TryGetValue(14, out _phrase14))
                {
                    Log.Out("[SERVERTOOLS] Phrase 14 not found using default.");
                }
                _phrase14 = _phrase14.Replace("{SenderName}", _cInfo.playerName);
                _phrase14 = _phrase14.Replace("{TargetName}", _strings[0]);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase14, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                _targetInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("(PM) {0}", _strings[1]), _cInfo.playerName, false, "", false));
            }
        }

        public static void Reply(ClientInfo _cInfo, string _message)
        {
            _message = _message.Replace("R ", "");
            _message = _message.Replace("r ", "");
        }
    }
}