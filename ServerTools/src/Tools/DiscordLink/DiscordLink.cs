using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    class DiscordLink
    {
        public static bool IsEnabled = false;
        public static string Command_discord = "discord", Invitation_Link = "http://discord.gg/linkHere";

        public static void Exec(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Overlay is set off. Steam browser is disabled" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserDiscord", true));
        }

        public static void SetLink(string _link)
        {
            try
            {
                if (File.Exists(GeneralFunction.XPathDir + "XUi/windows.xml"))
                {
                    List<string> lines = File.ReadAllLines(GeneralFunction.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserDiscord"))
                        {
                            if (!lines[i + 7].Contains(_link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", _link);
                                File.WriteAllLines(GeneralFunction.XPathDir + "XUi/windows.xml", lines.ToArray());
                            }
                            return;
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("/append"))
                        {
                            lines.RemoveRange(i, 3);
                            lines.Add("  <window name=\"browserDiscord\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Discord Link\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", _link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"microphoneIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_map\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                            lines.Add("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                            lines.Add("      </panel>");
                            lines.Add("  </window>");
                            lines.Add("");
                            lines.Add("</append>");
                            lines.Add("");
                            lines.Add("</configs>");
                            File.WriteAllLines(GeneralFunction.XPathDir + "XUi/windows.xml", lines.ToArray());
                            return;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", GeneralFunction.XPathDir + "XUi/windows.xml", e.Message));
            }
        }
    }
}
