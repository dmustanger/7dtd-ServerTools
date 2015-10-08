namespace ServerTools
{
    public class Whisper
    {
        private static bool IsEnabled = true;
        public static void Send(ClientInfo _cInfo, string _message)
        {
            string[] _strings = _message.Split(new char[] { ' ' }, 2);
            _strings[1] = _strings[1].TrimStart();
            ClientInfo _targetInfo = ConsoleHelper.ParseParamIdOrName(_strings[0]);
            if (_targetInfo == null)
            {
                string _phrase23 = "Player not found.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase23, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                _targetInfo.SendPackage(new NetPackageGameMessage(_strings[1], _cInfo.playerName + " (PM)"));
            }
        }
    }
}