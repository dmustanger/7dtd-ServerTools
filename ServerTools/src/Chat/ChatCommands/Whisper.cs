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
            ClientInfo _targetInfo = ConsoleHelper.ParseParamIdOrName(_strings[0]);
            if (_targetInfo == null)
            {
                string _phrase14;
                if (!Phrases.Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = "{SenderName} player {TargetName} was not found.";
                }
                _phrase14 = _phrase14.Replace("{SenderName}", _cInfo.playerName);
                _phrase14 = _phrase14.Replace("{TargetName}", _strings[0]);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase14, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                _targetInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("(PM) {0}", _strings[1]), _cInfo.playerName, false, "", false));
                PersistentContainer.Instance.Players[_targetInfo.playerId, true].LastWhisper = _cInfo.playerId;
                PersistentContainer.Instance.Save();
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
            if (PersistentContainer.Instance.Players[_cInfo.playerId, true].LastWhisper == null)
            {
                string _phrase15;
                if (!Phrases.Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = "{SenderName} no one has pm'd you.";
                }
                _phrase15 = _phrase15.Replace("{SenderName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase15, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(PersistentContainer.Instance.Players[_cInfo.playerId, true].LastWhisper);
                if (_cInfo1 == null)
                {
                    string _phrase16;
                    if (!Phrases.Dict.TryGetValue(16, out _phrase16))
                    {
                        _phrase16 = "{SenderName} the player is not online.";
                    }
                    _phrase16 = _phrase16.Replace("{SenderName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase16, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("(PM) {0}", _message), _cInfo.playerName, false, "", false));
                    PersistentContainer.Instance.Players[_cInfo1.playerId, true].LastWhisper = _cInfo.playerId;
                    PersistentContainer.Instance.Save();
                }
            }
        }
    }
}