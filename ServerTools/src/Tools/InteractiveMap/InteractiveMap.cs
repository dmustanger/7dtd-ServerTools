using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    class InteractiveMap
    {
        public static bool IsEnabled = false, Disable = false;
        public static int RegionMax = 0;
        public static string Command_imap = "imap", Map_Directory = "";

        public static Dictionary<string, int> Access = new Dictionary<string, int>();

        private static readonly string NumSet = "1928374650";

        public static void SetWorldSize()
        {
            string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);
            if (gameWorld.ToLower() == "navezgane")
            {
                RegionMax = (int)Math.Truncate(2500f / 512) + 2;
            }
            else
            {
                IChunkProvider chunkProvider = GameManager.Instance.World.ChunkCache.ChunkProvider;
                float worldGenSize = chunkProvider.GetWorldSize().x;
                RegionMax = (int)Math.Truncate(worldGenSize / 512) + 2;
            }
        }

        public static void LocateMapFolder()
        {
            if (!string.IsNullOrEmpty(Map_Directory) && Directory.Exists(Map_Directory))
            {
                string[] files1 = Directory.GetFiles(Map_Directory, "*", SearchOption.AllDirectories);
                if (files1.Length > 0)
                {
                    for (int i = 0; i < files1.Length; i++)
                    {
                        if (files1[i].Contains("mapinfo.json"))
                        {
                            Map_Directory = files1[i].Replace("mapinfo.json", "2");
                        }
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate the required map files @ '{0}'", Map_Directory));
                }
                return;
            }
            string saveGameDir = GameIO.GetSaveGameDir();
            string[] files2 = Directory.GetFiles(saveGameDir, "*", SearchOption.AllDirectories);
            if (files2.Length > 0)
            {
                for (int i = 0; i < files2.Length; i++)
                {
                    if (files2[i].Contains("mapinfo.json"))
                    {
                        Map_Directory = files2[i].Replace("mapinfo.json", "2");
                    }
                }
            }
            else
            {
                Log.Out(string.Format("[SERVERTOOLS] Unable to locate the required map files @ '{0}'", Map_Directory));
            }
        }

        public static void SetLink()
        {
            try
            {
                if (File.Exists(GeneralFunction.XPathDir + "XUi/windows.xml"))
                {
                    string link = string.Format("http://{0}:{1}/imap.html", WebAPI.BaseAddress, WebAPI.Port);
                    List<string> lines = File.ReadAllLines(GeneralFunction.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserIMap"))
                        {
                            if (!lines[i + 7].Contains(link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link);
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
                            lines.Add("  <window name=\"browserIMap\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Interactive Map\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"mapIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_map\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
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

        public static void Exec(ClientInfo _cInfo)
        {
            if (WebAPI.IsEnabled && WebAPI.Connected && Map_Directory != "")
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Overlay is set off. Steam browser is disabled" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                string ip = _cInfo.ip;
                bool duplicate = false;
                List<ClientInfo> clientList = GeneralFunction.ClientList();
                if (clientList != null && clientList.Count > 1)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo = clientList[i];
                        if (cInfo != null && cInfo.entityId != _cInfo.entityId && ip == cInfo.ip)
                        {
                            duplicate = true;
                            break;
                        }
                    }
                }
                long ipLong = GeneralFunction.ConvertIPToLong(_cInfo.ip);
                if (duplicate || (ipLong >= GeneralFunction.ConvertIPToLong("10.0.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("10.255.255.255")) ||
                    (ipLong >= GeneralFunction.ConvertIPToLong("172.16.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("172.31.255.255")) ||
                    (ipLong >= GeneralFunction.ConvertIPToLong("192.168.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("192.168.255.255")) ||
                    _cInfo.ip == "127.0.0.1")
                {
                    string securityId = "";
                    for (int i = 0; i < 10; i++)
                    {
                        string pass = CreatePassword(4);
                        if (!Access.ContainsKey(pass))
                        {
                            securityId = pass;
                            if (!Access.ContainsValue(_cInfo.entityId))
                            {
                                Access.Add(securityId, _cInfo.entityId);
                                WebAPI.AuthorizedTime.Add(securityId, DateTime.Now.AddMinutes(5));
                            }
                            break;
                        }
                    }
                    Phrases.Dict.TryGetValue("IMap1", out string phrase);
                    phrase = phrase.Replace("{Value}", securityId);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserIMap", true));
                }
                else
                {
                    if (Access.Count > 0 && Access.ContainsValue(_cInfo.entityId))
                    {
                        var clients = Access.ToArray();
                        for (int i = 0; i < clients.Length; i++)
                        {
                            if (clients[i].Value == _cInfo.entityId && clients[i].Key != ip)
                            {
                                Access.Remove(clients[i].Key);
                                Access.Add(ip, _cInfo.entityId);
                                break;
                            }
                        }
                    }
                    else if (Access.ContainsKey(ip))
                    {
                        Access[ip] = _cInfo.entityId;
                    }
                    else
                    {
                        Access.Add(ip, _cInfo.entityId);
                    }
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserIMap", true));
                }
            }
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += NumSet.ElementAt(rnd.Next(0, 10));
            }
            return pass;
        }
    }
}
