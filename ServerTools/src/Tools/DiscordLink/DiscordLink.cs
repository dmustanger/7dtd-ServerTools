using System;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class DiscordLink
    {
        public static bool IsEnabled = false;
        public static string Command_discord = "discord", Invitation_Link = "http://discord.gg/linkHere";

        public static void Exec(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserDiscord", true));
        }

        public static void SetLink(string _inviteLink)
        {
            try
            {
                if (File.Exists(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml"))
                {
                    string[] arrLines = File.ReadAllLines(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml");
                    int lineNumber = 0;
                    for (int i = 0; i < arrLines.Length; i++)
                    {
                        if (arrLines[i].Contains("browserDiscord"))
                        {
                            lineNumber = i + 7;
                            if (arrLines[lineNumber].Contains(_inviteLink))
                            {
                                return;
                            }
                            break;
                        }
                    }
                    arrLines[lineNumber] = string.Format("<label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />", _inviteLink);
                    File.WriteAllLines(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml", arrLines);
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml", e.Message));
                return;
            }
        }
    }
}
