namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false;
        public static string _message = "Welcome to YourServerNameHere {PlayerName}. If this is your first time here, please read the rules!";

        public static void Send(ClientInfo _cInfo)
        {
            _message = _message.Replace("{PlayerName}", _cInfo.playerName);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _message), "Server", false, "", false));
        }
    }
}