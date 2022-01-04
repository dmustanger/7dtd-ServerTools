

namespace ServerTools
{
    class AllocsMap
    {
        public static bool IsEnabled = false;
        public static string Command_map = "map";

        public static void Exec(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserMap", true));
        }
    }
}
