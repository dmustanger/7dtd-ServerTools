using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Config
    {
        public static string _gamepath = GamePrefs.GetString(EnumGamePrefs.SaveGameFolder);
        public static string _configpath = string.Format("{0}/ServerTools", _gamepath);
        public static string _datapath = string.Format("{0}/Data", _configpath);
        private static string _file = "ServerToolsConfig.xml";
        private static string _filepath = string.Format("{0}/{1}", _configpath, _file);
        private static FileSystemWatcher _fileWatcher = new FileSystemWatcher(_configpath, _file);

        public static void Init()
        {
            Phrases.Init();
            LoadConfig();
            InitFileWatcher();
        }

        private static void LoadConfig()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateConfig();
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file , e.Message));
                return;
            }
            XmlNode _ConfigXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ConfigXml.ChildNodes)
            {
                if (childNode.Name == "Tools")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Tools' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring tool entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        switch (_line.GetAttribute("Name"))
                        {
                            case "Killme":
                                if (!_line.HasAttribute("DelayBetweenKillmeUses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring killme entry because of missing 'DelayBetweenKillmeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("DelayBetweenKillmeUses"), out KillMe.DelayBetweenUses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring killme entry because of invalid (non-numeric) value for 'DelayBetweenKillmeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring killme entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out KillMe.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring killme entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "HighPingKicker":
                                if (!_line.HasAttribute("SamplesNeeded"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ping entry because of missing 'SamplesNeeded' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("SamplesNeeded"), out HighPingKicker.SamplesNeeded))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ping entry because of invalid (non-numeric) value for 'SamplesNeeded' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Maxping"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ping entry because of missing 'Maxping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Maxping"), out HighPingKicker.MAXPING))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ping entry because of invalid (non-numeric) value for 'maxping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring HighPingKicker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out HighPingKicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring HighPingKicker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "InvalidItemKicker":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemKicker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InventoryCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemKicker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemKicker entry because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban"), out InventoryCheck.BanPlayer))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemKicker entry because of invalid (true/false) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "AnnounceInvalidItemStack":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemStack entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InventoryCheck.AnounceInvalidStack))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InvalidItemStack entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "ChatLogger":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatLogger entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatLogger entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "SetHome":
                                if (!_line.HasAttribute("DelayBetweenSetHomeUses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring SetHome entry because of missing 'DelayBetweenSetHomeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("DelayBetweenSetHomeUses"), out TeleportHome.DelayBetweenUses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring SetHome entry because of invalid (non-numeric) value for 'DelayBetweenSetHomeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring SetHome entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out TeleportHome.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring SetHome entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "CustomCommands":
                                if (!_line.HasAttribute("ChatColor"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring CustomCommands entry because of missing 'ChatColor' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring CustomCommands entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out CustomCommands.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring CustomCommands entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "BadWordFilter":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring BadWordFilter entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Badwords.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring BadWordFilter entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "InfoTicker":
                                if (!_line.HasAttribute("DelayBetweenMessages"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InfoTicker entry because of missing 'DelayBetweenMessages' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("DelayBetweenMessages"), out InfoTicker.DelayBetweenMessages))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InfoTicker entry because of invalid (non-numeric) value for 'DelayBetweenMessages' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InfoTicker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                bool _old = InfoTicker.IsEnabled;
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InfoTicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring InfoTicker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "AutoSaveWorld":
                                if (!_line.HasAttribute("DelayBetweenWorldSaves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoSaveWorld entry because of missing 'DelayBetweenWorldSaves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("DelayBetweenWorldSaves"), out SaveWorld.DelayBetweenWorldSaves))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoSaveWorld entry because of invalid (non-numeric) value for 'DelayBetweenWorldSaves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoSaveWorld entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out SaveWorld.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoSaveWorld entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Motd":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Motd.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Gimme":
                                if (!_line.HasAttribute("DelayBetweenGimmeUses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring gimme entry because of missing 'DelayBetweenGimmeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("DelayBetweenGimmeUses"), out Gimme.DelayBetweenUses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring gimme entry because of invalid (non-numeric) value for 'DelayBetweenGimmeUses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Gimme.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("AlwaysShowResponse"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'AlwaysShowResponse' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("AlwaysShowResponse"), out Gimme.AlwaysShowResponse))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (true/false) value for 'AlwaysShowResponse' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "ClanManager":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ClanManager entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ClanManager.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ClanManager entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "AdminChatCommands":
                                if (!_line.HasAttribute("PermissionLevelForMute"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AdminChatCommands entry because of missing 'PermissionLevelForMute' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("PermissionLevelForMute"), out AdminChat.PermLevelNeededforMute))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AdminChatCommands entry because of invalid (non-numeric) value for 'PermissionLevelForMute' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AdminChatCommands entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AdminChat.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AdminChatCommands entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                        }
                    }
                    Mods.Init();
                }
            }
        }

        public static void UpdateConfig()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Tools>");
                sw.WriteLine(string.Format("        <Tool Name=\"CustomCommands\" Enable=\"{0}\" ChatColor=\"{1}\" />", CustomCommands.IsEnabled, CustomCommands._chatcolor));
                sw.WriteLine(string.Format("        <Tool Name=\"Killme\" Enable=\"{0}\" DelayBetweenKillmeUses=\"{1}\" />", KillMe.IsEnabled, KillMe.DelayBetweenUses));
                sw.WriteLine(string.Format("        <Tool Name=\"Gimme\" Enable=\"{0}\" DelayBetweenGimmeUses=\"{1}\" AlwaysShowResponse=\"{2}\" />", Gimme.IsEnabled, Gimme.DelayBetweenUses, Gimme.AlwaysShowResponse));
                sw.WriteLine(string.Format("        <Tool Name=\"SetHome\" Enable=\"{0}\" DelayBetweenSetHomeUses=\"{1}\" />", TeleportHome.IsEnabled, TeleportHome.DelayBetweenUses));
                sw.WriteLine(string.Format("        <Tool Name=\"HighPingKicker\" Enable=\"{0}\" Maxping=\"{1}\" SamplesNeeded=\"{2}\" />", HighPingKicker.IsEnabled, HighPingKicker.MAXPING, HighPingKicker.SamplesNeeded));
                sw.WriteLine(string.Format("        <Tool Name=\"InvalidItemKicker\" Enable=\"{0}\" Ban=\"{1}\" />", InventoryCheck.IsEnabled, InventoryCheck.BanPlayer));
                sw.WriteLine(string.Format("        <Tool Name=\"AnnounceInvalidItemStack\" Enable=\"{0}\" />", InventoryCheck.AnounceInvalidStack));
                sw.WriteLine(string.Format("        <Tool Name=\"ChatLogger\" Enable=\"{0}\" />", ChatLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"BadWordFilter\" Enable=\"{0}\" />", Badwords.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"InfoTicker\" Enable=\"{0}\" DelayBetweenMessages=\"{1}\" />", InfoTicker.IsEnabled, InfoTicker.DelayBetweenMessages));
                sw.WriteLine(string.Format("        <Tool Name=\"Motd\" Enable=\"{0}\" />", Motd.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"AutoSaveWorld\" Enable=\"{0}\" DelayBetweenWorldSaves=\"{1}\" />", SaveWorld.IsEnabled, SaveWorld.DelayBetweenWorldSaves));
                sw.WriteLine(string.Format("        <Tool Name=\"ClanManager\" Enable=\"{0}\" />", ClanManager.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"AdminChatCommands\" Enable=\"{0}\" PermissionLevelForMute=\"{1}\" />", AdminChat.IsEnabled, AdminChat.PermLevelNeededforMute));
                sw.WriteLine("    </Tools>");
                sw.WriteLine("</ServerTools>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateConfig();
            }
            LoadConfig();
        }
    }
}