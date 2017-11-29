namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false;
        public static bool ShowOnRespawn = false;
        public static string Message = "Welcome to YourServerNameHere {PlayerName}. If this is your first time here, please read the rules!";

        public static void Send(ClientInfo _cInfo)
        {
            bool _replaceName = false;
            if (Message.Contains("{PlayerName}"))
            {
                Message = Message.Replace("{PlayerName}", _cInfo.playerName);
                _replaceName = true;
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, Message), "Server", false, "", false));
            if (_replaceName)
            {
                Message = Message.Replace(_cInfo.playerName, "{PlayerName}");
            }
        }
    }
}