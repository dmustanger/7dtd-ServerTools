namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false;
        public static string _message = "Welcome to Your ServerNameHere. If this is your first time here, please read the rules!";

        public static void Send(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _message), "Server"));
        }
    }
}
