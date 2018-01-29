namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false;
        public static bool MOTD2IsEnabled = false;
        public static bool ShowOnRespawn = false;
        public static bool ShowOnRespawn2 = false;
        public static string Message = "Welcome to YourServerNameHere {PlayerName}. If this is your first time here, please read the rules!";        
        public static string Message2 = "Extra server info...!";

        public static void Send(ClientInfo _cInfo)
        {
            bool _replaceName = false;
            if (Message.Contains("{PlayerName}"))
            {
                Message = Message.Replace("{PlayerName}", _cInfo.playerName);
                _replaceName = true;
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatColor, Message), "Server", false, "", false));
            if (_replaceName)
            {
                Message = Message.Replace(_cInfo.playerName, "{PlayerName}");
            }
        }

        public static void Send2(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatColor, Message2), "Server", false, "", false));
        }
    }
}