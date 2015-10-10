namespace ServerTools
{
    public class Whisper
    {
        public static bool IsEnabled = true;

        public static void Send(ClientInfo _cInfo, string _message)
        {
            string[] _strings = _message.Split(new char[] { ' ' }, 2);
            _strings[1] = _strings[1].TrimStart();
            ClientInfo _targetInfo = ConsoleHelper.ParseParamIdOrName(_strings[0]);
            if (_targetInfo == null)
            {
                string _phrase14 = "{SenderName} player {TargetName} was not found.";
                if (Phrases._Phrases.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = _phrase14.Replace("{SenderName}", _cInfo.playerName);
                    _phrase14 = _phrase14.Replace("{TargetName}", _strings[0]);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase14, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                _targetInfo.SendPackage(new NetPackageGameMessage(_strings[1], string.Format("{0} (PM)", _cInfo.playerName)));
            }
        }
    }
}