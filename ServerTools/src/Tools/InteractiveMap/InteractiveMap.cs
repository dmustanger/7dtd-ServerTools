using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class InteractiveMap
    {
        public static bool IsEnabled = false, IsRunning = false, Disable = false;
        public static int RegionMax = 0;
        public static string Command_imap = "imap", Map_Directory = "";

        public static Dictionary<string, int> Access = new Dictionary<string, int>();
        public static Dictionary<string, int> Dict = new Dictionary<string, int>();

        private const string file = "IMapPermission.xml";
        private static readonly string NumSet = "1928374650";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static FileSystemWatcher FolderWatcher;

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
            InitFolderWatcher();
            IsRunning = true;
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            FolderWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Rule") && line.HasAttribute("Tier"))
                                {
                                    string rule = line.GetAttribute("Rule");
                                    string tier = line.GetAttribute("Tier");
                                    if (!int.TryParse(tier, out int value))
                                    {
                                        if (!Dict.ContainsKey(rule))
                                        {
                                            Dict.Add(rule, value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing IMapPermission.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in InteractiveMap.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<IMapPermission>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.ContainsKey("Regions"))
                    {
                        Dict.TryGetValue("Regions", out int tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Regions\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Regions\" Tier=\"0\" />"));
                    }
                    if (Dict.ContainsKey("Players"))
                    {
                        Dict.TryGetValue("Players", out int tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Players\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Players\" Tier=\"0\" />"));
                    }
                    if (Dict.ContainsKey("Claims"))
                    {
                        Dict.TryGetValue("Claims", out int tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Claims\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Claims\" Tier=\"0\" />"));
                    }
                    if (Dict.ContainsKey("Hostiles"))
                    {
                        Dict.TryGetValue("Hostiles", out int tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Hostiles\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Hostiles\" Tier=\"0\" />"));
                    }
                    if (Dict.ContainsKey("Animals"))
                    {
                        Dict.TryGetValue("Animals", out int tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Animals\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Animals\" Tier=\"0\" />"));
                    }
                    sw.WriteLine("</IMapPermission>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InteractiveMap.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFolderWatcher()
        {
            FolderWatcher = new FileSystemWatcher(Map_Directory);
            FolderWatcher.NotifyFilter = NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastWrite;
            FolderWatcher.Changed += new FileSystemEventHandler(OnFolderChanged);
            FolderWatcher.Created += new FileSystemEventHandler(OnFolderChanged);
            FolderWatcher.Deleted += new FileSystemEventHandler(OnFolderChanged);
            FolderWatcher.IncludeSubdirectories = true;
            FolderWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        private static void OnFolderChanged(object source, FileSystemEventArgs e)
        {
            //Log.Out(string.Format("[SERVERTOOLS] OnFolderChanged"));
            //Log.Out(string.Format("[SERVERTOOLS] FullPath: '{0}'", e.FullPath));
            //Log.Out(string.Format("[SERVERTOOLS] ChangeType: '{0}'", e.ChangeType));
        }

        private static void UpgradeXml()
        {
            try
            {
                Dictionary<string,string> oldEntries = new Dictionary<string,string>();
                for (int i = 0; i < OldNodeList.Count; i++)
                {
                    if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                    {
                        XmlElement line = (XmlElement)OldNodeList[i];
                        if (line.HasAttributes && line.Name == "Permission")
                        {
                            string rule = "", tier = "";
                            if (line.HasAttribute("Rule"))
                            {
                                rule = line.GetAttribute("Rule");
                            }
                            if (line.HasAttribute("Tier"))
                            {
                                tier = line.GetAttribute("Tier");
                            }
                            if (!oldEntries.ContainsKey(rule))
                            {
                                oldEntries.Add(rule, tier);
                            }
                        }
                    }
                }
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<LoginNotice>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    if (oldEntries.ContainsKey("Regions"))
                    {
                        oldEntries.TryGetValue("Regions", out string tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Regions\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Regions\" Tier=\"0\" />"));
                    }
                    if (oldEntries.ContainsKey("Players"))
                    {
                        oldEntries.TryGetValue("Players", out string tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Players\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Players\" Tier=\"0\" />"));
                    }
                    if (oldEntries.ContainsKey("Claims"))
                    {
                        oldEntries.TryGetValue("Claims", out string tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Claims\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Claims\" Tier=\"0\" />"));
                    }
                    if (oldEntries.ContainsKey("Hostiles"))
                    {
                        oldEntries.TryGetValue("Hostiles", out string tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Hostiles\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Hostiles\" Tier=\"0\" />"));
                    }
                    if (oldEntries.ContainsKey("Animals"))
                    {
                        oldEntries.TryGetValue("Animals", out string tier);
                        sw.WriteLine(string.Format("    <Permission Rule=\"Animals\" Tier=\"{0}\" />", tier));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Permission Rule=\"Animals\" Tier=\"0\" />"));
                    }
                    sw.WriteLine("</LoginNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InteractiveMap.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }

        public static void SetWorldSize()
        {
            string gameWorld = GamePrefs.GetString(EnumGamePrefs.GameWorld);
            if (gameWorld.ToLower() == "navezgane")
            {
                RegionMax = (int)Math.Truncate(3000f / 512) + 1;
            }
            else
            {
                IChunkProvider chunkProvider = GameManager.Instance.World.ChunkCache.ChunkProvider;
                float worldGenSize = chunkProvider.GetWorldSize().x;
                RegionMax = (int)Math.Truncate(worldGenSize / 512) + 1;
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
                            Map_Directory = files1[i].Replace("mapinfo.json", "");
                            Log.Out(string.Format("[SERVERTOOLS] Interactive_Map directory set to '{0}'", Map_Directory));
                            return;
                        }
                    }
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate the required map files @ '{0}'", Map_Directory));
                }
            }
            string saveGameDir = GameIO.GetSaveGameDir();
            string[] files2 = Directory.GetFiles(saveGameDir, "*", SearchOption.AllDirectories);
            if (files2.Length > 0)
            {
                for (int i = 0; i < files2.Length; i++)
                {
                    if (files2[i].Contains("mapinfo.json"))
                    {
                        Map_Directory = files2[i].Replace("mapinfo.json", "");
                        Log.Out(string.Format("[SERVERTOOLS] Interactive_Map directory set to '{0}'", Map_Directory));
                        return;
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
