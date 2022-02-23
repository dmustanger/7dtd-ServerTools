using System.IO;
using System.Xml;

namespace ServerTools
{
    class AllocsMap
    {
        public static bool IsEnabled = false;
        public static string Command_map = "map", Link = "http://0.0.0.0:8082";

        public static void Exec(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserMap", true));
        }

        public static void SetLink(string _link)
        {
            try
            {
                if (File.Exists(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml"))
                {
                    string[] arrLines = File.ReadAllLines(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml");
                    int lineNumber = 0;
                    for (int i = 0; i < arrLines.Length; i++)
                    {
                        if (arrLines[i].Contains("browserMap"))
                        {
                            lineNumber = i + 7;
                            if (arrLines[lineNumber].Contains(_link))
                            {
                                return;
                            }
                            break;
                        }
                    }
                    arrLines[lineNumber] = string.Format("<label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />", _link);
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
