using System;

namespace ServerTools
{
    class DiscordInvite
    {
        public static bool IsEnabled = false;
        public static string Command_discord = "discord";

        public static void Exec(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserDiscord", true));
        }
    }
}
