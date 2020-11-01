using System;
using System.IO;
using System.Xml;
using ServerTools.AntiCheat;
using ServerTools.Website;

namespace ServerTools
{
    public class LoadConfig
    {
        public const string version = "19.2.6";
        public static string Server_Response_Name = "[FFCC00]ServerTools";
        public static string Chat_Response_Color = "[00FF00]";
        private const string configFile = "ServerToolsConfig.xml";
        public static string configFilePath = string.Format("{0}/{1}", API.ConfigPath, configFile);
        public static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, configFile);
        public static string OldXmlDirectory = "";

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void LoadXml()
        {
            Log.Out("---------------------------------------------------------------");
            Log.Out("[SERVERTOOLS] Verifying configuration file & Saving new entries");
            Log.Out("---------------------------------------------------------------");
            if (!Utils.FileExists(configFilePath))
            {
                WriteXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(configFilePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", configFilePath, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Version")
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
                        if (!_line.HasAttribute("Version"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring config entry because of missing 'Version' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else if (_line.GetAttribute("Version") != version)
                        {
                            Log.Out("[SERVERTOOLS] Detected updated version of ServerTools");
                            string _version = _line.GetAttribute("Version");
                            string[] _files = Directory.GetFiles(API.ConfigPath, "*.xml");
                            if (!Directory.Exists(API.ConfigPath + "/XMLBackups"))
                            {
                                Directory.CreateDirectory(API.ConfigPath + "/XMLBackups");
                            }
                            if (_files != null && _files.Length > 0)
                            {
                                if (!Directory.Exists(API.ConfigPath + "/XMLBackups/" + _version))
                                {
                                    Directory.CreateDirectory(API.ConfigPath + "/XMLBackups/" + _version);
                                    OldXmlDirectory = API.ConfigPath + "/XMLBackups/" + _version;
                                    for (int i = 0; i < _files.Length; i++)
                                    {
                                        string _fileName = _files[i];
                                        string _fileNameShort = _fileName.Substring(_fileName.IndexOf("ServerTools") + 11);
                                        File.Copy(_fileName, API.ConfigPath + "/XMLBackups/" + _version + _fileNameShort);
                                        if (_fileNameShort == "ServerToolsConfig.xml")
                                        {
                                            File.Delete(_fileName);
                                        }
                                    }
                                    WriteXml();
                                    LoadConfig.UpgradeXml();
                                    Log.Out("[SERVERTOOLS] Created backup of xml files for version {0}", _version);
                                }
                            }
                            else
                            {
                                WriteXml();
                            }
                        }
                    }
                }
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
                            case "Animal_Tracking":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Animals.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Animals.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Minimum_Spawn_Radius"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Minimum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Minimum_Spawn_Radius"), out Animals.Minimum_Spawn_Radius))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Minimum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Maximum_Spawn_Radius"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Maximum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Maximum_Spawn_Radius"), out Animals.Maximum_Spawn_Radius))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Maximum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Entity_Id"))
                                {
                                    Animals.Animal_List = _line.GetAttribute("Entity_Id");
                                }
                                break;
                            case "Animal_Tracking_Extended":
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking_Extended entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Animals.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking_Extended entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auction":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Auction.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Admins"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Admins"), out Auction.No_Admins))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (True/False) value for 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out Auction.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Total_Items"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Total_Items' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Total_Items"), out Auction.Total_Items))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (non-numeric) value for 'Total_Items' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Tax"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Tax' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Tax"), out Auction.Tax))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (non-numeric) value for 'Tax' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auto_Backup":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AutoBackup.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Saves"), out AutoBackup.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (non-numeric) value for 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Destination"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Destination' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Destination"))
                                {
                                    AutoBackup.Destination = _line.GetAttribute("Destination");
                                }
                                if (!_line.HasAttribute("Compression_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Compression_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Compression_Level"), out AutoBackup.Compression_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (non-numeric) value for 'Compression_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Backup_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Backup_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Backup_Count"), out AutoBackup.Backup_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (non-numeric) value for 'Backup_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auto_Save_World":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AutoSaveWorld.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of missing 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Saves"), out AutoSaveWorld.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of invalid (non-numeric) value for 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bad_Word_Filter":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Badwords.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Invalid_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of missing 'Invalid_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Invalid_Name"), out Badwords.Invalid_Name))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of invalid (True/False) value for 'Invalid_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bank":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Bank.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Claim"), out Bank.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (True/False) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ingame_Coin"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Ingame_Coin' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Ingame_Coin"))
                                {
                                    Bank.Ingame_Coin = _line.GetAttribute("Ingame_Coin");
                                }
                                if (!_line.HasAttribute("Deposit_Fee_Percent"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Deposit_Fee_Percent' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Deposit_Fee_Percent"), out Bank.Deposit_Fee_Percent))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (non-numeric) value for 'Deposit_Fee_Percent' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Transfers"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Player_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Player_Transfers"), out Bank.Player_Transfers))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (True/False) value for 'Player_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Battle_Loggers":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out BattleLogger.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Distance"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'Player_Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Player_Distance"), out BattleLogger.Player_Distance))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (non-numeric) value for 'Player_Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("All"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'All' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("All"), out BattleLogger.All))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (True/False) value for 'All' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Belt"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'Belt' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Belt"), out BattleLogger.Belt))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (True/False) value for 'Belt' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bag"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'Bag' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Bag"), out BattleLogger.Bag))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (True/False) value for 'Bag' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Equipment"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of missing 'Equipment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Equipment"), out BattleLogger.Equipment))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers entry because of invalid (True/False) value for 'Equipment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Battle_Loggers_Extended":
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers_Extended entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out BattleLogger.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers_Extended entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Exit_time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers_Extended entry because of missing 'Exit_time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Exit_time"), out BattleLogger.Exit_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Battle_Loggers_Extended entry because of invalid (non-numeric) value for 'Exit_time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Block_Logger":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Logger entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out BlockLogger.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Logger entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bloodmoon":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Bloodmoon.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_On_Respawn"), out Bloodmoon.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (True/False) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Auto_Show"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Auto_Show' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Auto_Show"), out Bloodmoon.Auto_Show))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (True/False) value for 'Auto_Show' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out Bloodmoon.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (non-numeric) value for 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bloodmoon_Warrior":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out BloodmoonWarrior.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Kills"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of missing 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Zombie_Kills"), out BloodmoonWarrior.Zombie_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of invalid (non-numeric) value for 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Chance"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of missing 'Chance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Chance"), out BloodmoonWarrior.Chance))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of invalid (non-numeric) value for 'Chance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reduce_Death_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of missing 'Reduce_Death_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reduce_Death_Count"), out BloodmoonWarrior.Reduce_Death_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry because of invalid (True/False) value for 'Reduce_Death_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bounties":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Bounties.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Minimum_Bounty"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of missing 'Minimum_Bounty' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Minimum_Bounty"), out Bounties.Minimum_Bounty))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of invalid (non-numeric) value for 'Minimum_Bounty' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill_Streak"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of missing 'Kill_Streak' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Kill_Streak"), out Bounties.Kill_Streak))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of invalid (non-numeric) value for 'Kill_Streak' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of missing 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Bonus"), out Bounties.Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of invalid (non-numeric) value for 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Break_Reminder":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out BreakTime.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Break_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry because of missing 'Break_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Break_Time"), out BreakTime.Break_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry because of invalid (non-numeric) value for 'Break_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Break_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry because of missing 'Break_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Break_Message"))
                                {
                                    BreakTime.Break_Message = _line.GetAttribute("Break_Message");
                                }
                                break;
                            case "Chat_Color_Prefix":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color_Prefix entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatColorPrefix.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color_Prefix entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Command_Log":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Log entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatCommandLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Log entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Command_Response":
                                if (!_line.HasAttribute("Server_Response_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Server_Response_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Main_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Main_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Chat_Command_Private"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Chat_Command_Private' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Chat_Command_Public"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Chat_Command_Public' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Server_Response_Name"))
                                {
                                    Server_Response_Name = _line.GetAttribute("Server_Response_Name");
                                }
                                if (_line.HasAttribute("Main_Color"))
                                {
                                    Chat_Response_Color = _line.GetAttribute("Main_Color");
                                }
                                if (_line.HasAttribute("Chat_Command_Private"))
                                {
                                    ChatHook.Command_Private = _line.GetAttribute("Chat_Command_Private");
                                }
                                if (_line.HasAttribute("Chat_Command_Public"))
                                {
                                    ChatHook.Command_Public = _line.GetAttribute("Chat_Command_Public");
                                }
                                break;
                            case "Chat_Command_Response_Extended":
                                if (!_line.HasAttribute("Friend_Chat_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response_Extended entry because of missing 'Friend_Chat_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Party_Chat_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response_Extended entry because of missing 'Party_Chat_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Friend_Chat_Color"))
                                {
                                    ChatHook.Friend_Chat_Color = _line.GetAttribute("Friend_Chat_Color");
                                }
                                if (_line.HasAttribute("Party_Chat_Color"))
                                {
                                    ChatHook.Party_Chat_Color = _line.GetAttribute("Party_Chat_Color");
                                }
                                break;
                            case "Chat_Flood_Protection":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.ChatFlood))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of missing 'Max_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Length"), out ChatHook.Max_Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of invalid (non-numeric) value for 'Max_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Messages_Per_Min"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of missing 'Messages_Per_Min' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Messages_Per_Min"), out ChatHook.Messages_Per_Min))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of invalid (non-numeric) value for 'Messages_Per_Min' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Wait_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of missing 'Wait_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Wait_Time"), out ChatHook.Wait_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of invalid (non-numeric) value for 'Wait_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Logger":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Clan_Manager":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ClanManager.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Name_Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of missing 'Max_Name_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Name_Length"), out ClanManager.Max_Name_Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of invalid (non-numeric) value for 'Max_Name_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Private_Chat_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of missing 'Private_Chat_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Private_Chat_Color"))
                                {
                                    ClanManager.Private_Chat_Color = _line.GetAttribute("Private_Chat_Color");
                                }
                                break;
                            case "Console_Command_Log":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Console_Command_Log entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ConsoleCommandLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Console_Command_Log entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Country_Ban":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out CountryBan.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of missing 'Countries_Not_Allowed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    CountryBan.Countries_Not_Allowed = _line.GetAttribute("Countries_Not_Allowed");
                                }
                                break;
                            case "Credentials":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out CredentialCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Family_Share"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Family_Share' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Family_Share"), out CredentialCheck.Family_Share))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (True/False) value for 'No_Family_Share' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Bad_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Bad_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Bad_Id"), out CredentialCheck.Bad_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (True/False) value for 'No_Bad_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Internal"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Internal' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Internal"), out CredentialCheck.No_Internal))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (True/False) value for 'No_Internal' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out CredentialCheck.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Custom_Commands":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom_Commands entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out CustomCommands.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom_Commands entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Damage_Detector":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ProcessDamage.Damage_Detector))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of missing 'Entity_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Entity_Damage_Limit"), out ProcessDamage.Entity_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of invalid (non-numeric) value for 'Entity_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of missing 'Player_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Player_Damage_Limit"), out ProcessDamage.Player_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of invalid (non-numeric) value for 'Player_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Block_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of missing 'Block_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Block_Damage_Limit"), out ProcessDamage.Block_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of invalid (non-numeric) value for 'Block_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out ProcessDamage.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Day7":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Day7.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Death_Spot":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out DeathSpot.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of missing 'Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Time"), out DeathSpot.Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (non-numeric) value for 'Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out DeathSpot.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out DeathSpot.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Dupe_Log":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dupe_Log entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out DupeLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dupe_Log entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Entity_Cleanup":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out EntityCleanup.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Falling_Blocks"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Falling_Blocks"), out EntityCleanup.BlockIsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Falling_Tree"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Falling_Tree"), out EntityCleanup.FallingTreeEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Underground"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Entity_Underground"), out EntityCleanup.Underground))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delete_Bicycles"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_Bicycles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_Bicycles"), out EntityCleanup.Bicycles))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Delete_Bicycles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Entity_Cleanup_Extended":
                                if (!_line.HasAttribute("Delete_MiniBikes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_MiniBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_MiniBikes"), out EntityCleanup.MiniBikes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Delete_MiniBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delete_MotorBikes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_MotorBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_MotorBikes"), out EntityCleanup.MotorBikes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Delete_MotorBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delete_Jeeps"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_Jeeps' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_Jeeps"), out EntityCleanup.Jeeps))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Delete_Jeeps' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delete_Gyros"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_Gyros' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_Gyros"), out EntityCleanup.Gyros))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (True/False) value for 'Delete_Gyros' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "First_Claim_Block":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring First_Claim_Block entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out FirstClaimBlock.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring First_Claim_Block entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Flying_Detector":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerChecks.FlyEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out PlayerChecks.Flying_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Flags"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of missing 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Flags"), out PlayerChecks.Flying_Flags))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry because of invalid (non-numeric) value for 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "FPS":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Fps.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Target"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry because of missing 'Set_Target' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Set_Target"), out Fps.Set_Target))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry because of invalid (non-numeric) value for 'Set_Target' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Friend_Teleport":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out FriendTeleport.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out FriendTeleport.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out FriendTeleport.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out FriendTeleport.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (True/False) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out FriendTeleport.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Gimme":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Gimme.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Gimme.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombies"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Zombies' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombies"), out Gimme.Zombies))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (True/False) value for 'Zombies' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Gimme.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Godmode_Detector":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerChecks.GodEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out PlayerChecks.Godmode_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Hardcore":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Hardcore.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Optional"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Optional' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Optional"), out Hardcore.Optional))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (True/False) value for 'Optional' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Deaths"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Max_Deaths' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Deaths"), out Hardcore.Max_Deaths))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (non-numeric) value for 'Max_Deaths' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Extra_Lives"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Max_Extra_Lives' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Extra_Lives"), out Hardcore.Max_Extra_Lives))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (non-numeric) value for 'Max_Extra_Lives' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Life_Price"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Life_Price' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Life_Price"), out Hardcore.Life_Price))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (non-numeric) value for 'Life_Price' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "High_Ping_Kicker":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out HighPingKicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Flags"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of missing 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Flags"), out HighPingKicker.Flags))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of invalid (non-numeric) value for 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Ping"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of missing 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Ping"), out HighPingKicker.Max_Ping))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of invalid (non-numeric) value for 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Home":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Home.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Home2_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Home2_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Home2_Enabled"), out Home.Home2_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of invalid (True/False) value for 'Home2_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Home2_Reserved_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Home2_Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Home2_Reserved_Only"), out Home.Home2_Reserved_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (True/False) value for 'Home2_Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Home2_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Home2_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Home2_Delay"), out Home.Home2_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of invalid (True/False) value for 'Home2_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Home.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Home.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Home_Extended":
                                if (!_line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Player_Check"), out Home.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Home.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Vehicle_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of missing 'Vehicle_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Vehicle_Check"), out Home.Vehicle_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home_Extended entry because of invalid (non-numeric) value for 'Vehicle_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Hordes":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Hordes.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Info_Ticker":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InfoTicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Messages"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing 'Delay_Between_Messages' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Messages"), out InfoTicker.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of invalid (non-numeric) value for 'Delay_Between_Messages' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Random"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing 'Random' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Random"), out InfoTicker.Random))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of invalid (True/False) value for 'Random' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Invalid_Items":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InvalidItems.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban"), out InvalidItems.Ban_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of invalid (True/False) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out InvalidItems.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Check_Storage"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of missing 'Check_Storage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Check_Storage"), out InvalidItems.Check_Storage))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry because of invalid (True/False) value for 'Check_Storage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Invalid_Item_Stack":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InvalidItems.Invalid_Stack))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Jail":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Jail.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of missing 'Jail_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Jail_Size"), out Jail.Jail_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of invalid (non-numeric) value for 'Jail_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of missing 'Jail_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Jail_Position"))
                                {
                                    Jail.Jail_Position = _line.GetAttribute("Jail_Position");
                                }
                                if (!_line.HasAttribute("Jail_Shock"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of missing 'Jail_Shock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jail_Shock"), out Jail.Jail_Shock))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of invalid (True/False) value for 'Jail_Shock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Kick_Vote":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out KickVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out KickVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out KickVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Kill_Notice":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out KillNotice.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of missing 'Show_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_Level"), out KillNotice.Show_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of invalid (True/False) value for 'Show_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_Damage"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of missing 'Show_Damage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_Damage"), out KillNotice.Show_Damage))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of invalid (True/False) value for 'Show_Damage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lobby":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Lobby.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Return"), out Lobby.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Lobby.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Lobby_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Lobby_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Lobby_Size"), out Lobby.Lobby_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Lobby_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Lobby_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Lobby_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Lobby_Position"))
                                {
                                    Lobby.Lobby_Position = _line.GetAttribute("Lobby_Position");
                                }
                                if (!_line.HasAttribute("Donor_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Donor_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Donor_Only"), out Lobby.Donor_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'Donor_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lobby_Extended":
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Lobby.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Player_Check"), out Lobby.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Lobby.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvE"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvE"), out Lobby.PvE))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (True/False) value for 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Location":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Location entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Loc.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Location entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Login_Notice":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out LoginNotice.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Logs":
                                if (!_line.HasAttribute("Days_Before_Log_Delete"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Logs entry because of missing 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Before_Log_Delete"), out LoadProcess.Days_Before_Log_Delete))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Logs entry because of invalid (non-numeric) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lottery":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Lottery.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry because of missing 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Bonus"), out Lottery.Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry because of invalid (non-numeric) value for 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Market":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Market.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Return"), out Market.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Market.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Market_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Market_Size"), out Market.Market_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Market_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Market_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    Market.Market_Position = _line.GetAttribute("Market_Position");
                                }
                                if (!_line.HasAttribute("Donor_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Donor_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Donor_Only"), out Market.Donor_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'Donor_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Market_Extended":
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Market.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Market.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Market.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvE"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvE"), out Market.PvE))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (True/False) value for 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Message_Color":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Message_Color_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry because of missing 'Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Message_Color = _line.GetAttribute("Color");
                                    if (!ChatHook.Message_Color.StartsWith("[") || !ChatHook.Message_Color.EndsWith("]"))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry because of invalid 'Color' attribute: {0}", subChild.OuterXml));
                                        continue;
                                    }
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_On_Respawn"), out Motd.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of invalid (True/False) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Mute":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Mute.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Block_Commands"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry because of missing 'Block_Commands' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Block_Commands"), out Mute.Block_Commands))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry because of invalid (True/False) value for 'Block_Commands' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Mute_Vote":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out MuteVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out MuteVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out MuteVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Net_Packages":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Net_Packages entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Packages.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Net_Packages entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "New_Player":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out NewPlayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entry_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry because of missing a Entry_Message attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Entry_Message"))
                                {
                                    NewPlayer.Entry_Message = _line.GetAttribute("Entry_Message");
                                }
                                break;
                            case "New_Spawn_Tele":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out NewSpawnTele.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("New_Spawn_Tele_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of missing a New_Spawn_Tele_Position attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("New_Spawn_Tele_Position"))
                                {
                                    NewSpawnTele.New_Spawn_Tele_Position = _line.GetAttribute("New_Spawn_Tele_Position");
                                }
                                if (!_line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Return"), out NewSpawnTele.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Night_Alert":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out NightAlert.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out NightAlert.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of invalid (non-numeric) value for 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Normal_Player_Color_Prefix":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Normal_Player_Color_Prefix))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry because of missing 'Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Prefix = _line.GetAttribute("Prefix");
                                }
                                if (!_line.HasAttribute("Name_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry because of missing 'Name_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Name_Color = _line.GetAttribute("Name_Color");
                                }
                                if (!_line.HasAttribute("Prefix_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry because of missing 'Prefix_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Prefix_Color = _line.GetAttribute("Prefix_Color");
                                }
                                break;
                            //case "Output_Log":
                            //    if (!_line.HasAttribute("Enable"))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Output_Log entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    if (!bool.TryParse(_line.GetAttribute("Enable"), out OutputLog.IsEnabled))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Output_Log entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    break;
                            case "Player_List":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Logs":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerLogs.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Interval"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Interval' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Interval"), out PlayerLogs.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (non-numeric) value for 'Interval' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Stats":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerStats.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Speed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Max_Speed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Speed"), out PlayerStats.Max_Speed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (non-numeric) value for 'Max_Speed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Health"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Health' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Health"), out PlayerStats.Health))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (non-numeric) value for 'Health' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Stamina"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Stamina' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Stamina"), out PlayerStats.Stamina))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (non-numeric) value for 'Stamina' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jump_Strength"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Jump_Strength' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!double.TryParse(_line.GetAttribute("Jump_Strength"), out PlayerStats.Jump_Strength))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (non-numeric) value for 'Jump_Strength' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Height"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of missing 'Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!double.TryParse(_line.GetAttribute("Height"), out PlayerStats.Height))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry because of invalid (non-numeric) value for 'Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Stats_Extended":
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out PlayerStats.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out PlayerStats.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of invalid (True/False) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out PlayerStats.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry because of invalid (True/False) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Poll":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Poll entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Poll.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Poll entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Prayer":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Prayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Prayer.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Prayer.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Private_Message":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Private_Message entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Whisper.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Private_Message entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Protected_Spaces":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Protected_Spaces entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ProtectedSpaces.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Protected_Spaces entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "PvE_Violations":
                                if (!_line.HasAttribute("Jail"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of missing 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Jail"), out PersistentOperations.Jail_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of invalid (True/False) value for 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of missing 'Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Kill"), out PersistentOperations.Kill_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of invalid (True/False) value for 'Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of missing 'Kick' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Kick"), out PersistentOperations.Kick_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of invalid (True/False) value for 'Kick' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Ban"), out PersistentOperations.Ban_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry because of invalid (True/False) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Real_World_Time":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out RealWorldTime.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out RealWorldTime.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of invalid (non-numeric) value for 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Time_Zone"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of missing 'Time_Zone' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Time_Zone"))
                                {
                                    RealWorldTime.Time_Zone = _line.GetAttribute("Time_Zone");
                                }
                                if (!_line.HasAttribute("Adjustment"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of missing 'Adjustment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Adjustment"), out RealWorldTime.Adjustment))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of invalid (non-numeric) value for 'Adjustment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Report":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Report.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Report.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of missing 'Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Length"), out Report.Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of invalid (non-numeric) value for 'Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out Report.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Reserved_Slots":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ReservedSlots.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Session_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Session_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Session_Time"), out ReservedSlots.Session_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (non-numeric) value for 'Session_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out ReservedSlots.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reduced_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reduced_Delay"), out ReservedSlots.Reduced_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (True/False) value for 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Restart_Vote":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out RestartVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out RestartVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out RestartVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out RestartVote.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Scout_Player":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ScoutPlayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out ScoutPlayer.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out ScoutPlayer.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Shop":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Shop.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Market"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Market"), out Shop.Inside_Market))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (True/False) value for 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Traders"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Inside_Traders' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Traders"), out Shop.Inside_Traders))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (True/False) value for 'Inside_Traders' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Shutdown":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Shutdown.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Countdown_Timer"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of missing 'Countdown_Timer' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Countdown_Timer"), out Shutdown.Countdown_Timer))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of invalid (non-numeric) value for 'Countdown_Timer' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Time_Before_Shutdown"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of missing 'Time_Before_Shutdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Time_Before_Shutdown"), out Shutdown.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of invalid (non-numeric) value for 'Time_Before_Shutdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_On_Login"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of missing 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Alert_On_Login"), out Shutdown.Alert_On_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of invalid (True/False) value for 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of missing 'Alert_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Alert_Count"), out Shutdown.Alert_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry because of invalid (True/False) value for 'Alert_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Spectator_Detector":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerChecks.SpectatorEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out PlayerChecks.Spectator_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry because of invalid (True/False) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Starting_Items":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Starting_Items entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out StartingItems.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Starting_Items entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Stuck":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Stuck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Stuck.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Suicide":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Suicide.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Suicide.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out Suicide.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (True/False) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Suicide.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out Suicide.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (True/False) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Suicide.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Tracking":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Tracking entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Track.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Tracking entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Travel":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Travel.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Travel.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Travel.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out Travel.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (True/False) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Travel.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Vehicle_Teleport":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out VehicleTeleport.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Bike"), out VehicleTeleport.Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Mini_Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Mini_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Mini_Bike"), out VehicleTeleport.Mini_Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Mini_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Motor_Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Motor_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Motor_Bike"), out VehicleTeleport.Motor_Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Motor_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jeep"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Jeep' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jeep"), out VehicleTeleport.Jeep))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Jeep' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Gyro"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Gyro' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Gyro"), out VehicleTeleport.Gyro))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Gyro' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Vehicle_Teleport_Extended":
                                if (!_line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Claim"), out VehicleTeleport.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (True/False) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Distance"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Distance"), out VehicleTeleport.Distance))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (non-numeric) value for 'Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out VehicleTeleport.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out VehicleTeleport.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Voting":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out VoteReward.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Your_Voting_Site"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Your_Voting_Site' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Your_Voting_Site"))
                                {
                                    VoteReward.Your_Voting_Site = _line.GetAttribute("Your_Voting_Site");
                                }
                                if (!_line.HasAttribute("API_Key"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'API_Key' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("API_Key"))
                                {
                                    VoteReward.API_Key = _line.GetAttribute("API_Key");
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out VoteReward.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reward_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Reward_Count"), out VoteReward.Reward_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (non-numeric) value for 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Voting_Extended":
                                if (!_line.HasAttribute("Reward_Entity"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of missing 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reward_Entity"), out VoteReward.Reward_Entity))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of invalid (True/False) value for 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of missing 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Entity_Id"), out VoteReward.Entity_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of invalid (non-numeric) value for 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Weekly_Votes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of missing 'Weekly_Votes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Weekly_Votes"), out VoteReward.Weekly_Votes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of invalid (non-numeric) value for 'Weekly_Votes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Wallet":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Wallet.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Coin_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Coin_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                Wallet.Coin_Name = _line.GetAttribute("Coin_Name");
                                if (!_line.HasAttribute("Zombie_Kill_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Zombie_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Zombie_Kill_Value"), out Wallet.Zombie_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (non-numeric) value for 'Zombie_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Kill_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Player_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Player_Kill_Value"), out Wallet.Player_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (non-numeric) value for 'Player_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Death_Penalty_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Death_Penalty_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Death_Penalty_Value"), out Wallet.Deaths))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (non-numeric) value for 'Death_Penalty_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Wallet_Extended":
                                if (!_line.HasAttribute("Lose_On_Death"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Lose_On_Death' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Lose_On_Death"), out Wallet.Lose_On_Death))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (True/False) value for 'Lose_On_Death' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bank_Transfers"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Bank_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Bank_Transfers"), out Wallet.Bank_Transfers))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (True/False) value for 'Bank_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Session_Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Session_Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Session_Bonus"), out Wallet.Session_Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (non-numeric) value for 'Session_Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Watchlist":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Watchlist.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out Watchlist.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of missing 'Alert_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Alert_Delay"), out Watchlist.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of invalid (non-numeric) value for 'Alert_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Waypoints":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Waypoints.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Waypoints"), out Waypoints.Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Donator_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator_Max_Waypoints"), out Waypoints.Donator_Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Donator_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Waypoints.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Waypoints.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out Waypoints.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Waypoints_Extended":
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Waypoints.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Vehicle"), out Waypoints.Vehicle))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Weather_Vote":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Weather_Vote entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out WeatherVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Weather_Vote entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out WeatherVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out WeatherVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Website":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Website entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out WebsiteServer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Website entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "World_Radius":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out WorldRadius.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Normal_Player"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Normal_Player' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Normal_Player"), out WorldRadius.Normal_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (non-numeric) value for 'Normal_Player' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reserved"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Reserved' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Reserved"), out WorldRadius.Reserved))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (non-numeric) value for 'Reserved' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out WorldRadius.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Zones":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Zones.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zone_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zone_Message"), out Zones.Zone_Message))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of invalid (True/False) value for 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reminder_Notice_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing 'Reminder_Notice_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Reminder_Notice_Delay"), out Zones.Reminder_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of invalid (non-numeric) value for 'Reminder_Notice_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Home"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Set_Home"), out Zones.Set_Home))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of invalid (True/False) value for 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                        }
                    }
                }
            }
        }

        public static void WriteXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(configFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Version>");
                sw.WriteLine(string.Format("        <Version Version=\"{0}\" />", version.ToString()));
                sw.WriteLine("    </Version>");
                sw.WriteLine("    <Tools>");
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_Chat_Commands\" Enable=\"{0}\" />", AdminChat.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_List\" Enable=\"{0}\" Admin_Level=\"{1}\" Moderator_Level=\"{2}\" />", AdminList.IsEnabled, AdminList.Admin_Level, AdminList.Mod_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Minimum_Spawn_Radius=\"{2}\" Maximum_Spawn_Radius=\"{3}\" Entity_Id=\"{4}\" />", Animals.IsEnabled, Animals.Delay_Between_Uses, Animals.Minimum_Spawn_Radius, Animals.Maximum_Spawn_Radius, Animals.Animal_List));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking_Extended\" Command_Cost=\"{0}\" />", Animals.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Auction\" Enable=\"{0}\" No_Admins=\"{1}\" Admin_Level=\"{2}\" Total_Items=\"{3}\" Tax=\"{4}\" />", Auction.IsEnabled, Auction.No_Admins, Auction.Admin_Level, Auction.Total_Items, Auction.Tax));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Backup\" Enable=\"{0}\" Delay_Between_Saves=\"{1}\" Destination=\"{2}\" Compression_Level=\"{3}\" Backup_Count=\"{4}\" />", AutoBackup.IsEnabled, AutoBackup.Delay, AutoBackup.Destination, AutoBackup.Compression_Level, AutoBackup.Backup_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Save_World\" Enable=\"{0}\" Delay_Between_Saves=\"{1}\" />", AutoSaveWorld.IsEnabled, AutoSaveWorld.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Bad_Word_Filter\" Enable=\"{0}\" Invalid_Name=\"{1}\" />", Badwords.IsEnabled, Badwords.Invalid_Name));
                sw.WriteLine(string.Format("        <Tool Name=\"Bank\" Enable=\"{0}\" Inside_Claim=\"{1}\" Ingame_Coin=\"{2}\" Deposit_Fee_Percent=\"{3}\" Player_Transfers=\"{4}\" />", Bank.IsEnabled, Bank.Inside_Claim, Bank.Ingame_Coin, Bank.Deposit_Fee_Percent, Bank.Player_Transfers));
                sw.WriteLine(string.Format("        <Tool Name=\"Battle_Loggers\" Enable=\"{0}\" Player_Distance=\"{1}\" All=\"{2}\" Belt=\"{3}\" Bag=\"{4}\" Equipment=\"{5}\"  />", BattleLogger.IsEnabled, BattleLogger.Player_Distance, BattleLogger.All, BattleLogger.Belt, BattleLogger.Bag, BattleLogger.Equipment));
                sw.WriteLine(string.Format("        <Tool Name=\"Battle_Loggers_Extended\" Admin_Level=\"{0}\" Exit_Time=\"{1}\" />", BattleLogger.Admin_Level, BattleLogger.Exit_Time));
                sw.WriteLine(string.Format("        <Tool Name=\"Block_Logger\" Enable=\"{0}\" />", BlockLogger.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon\" Enable=\"{0}\" Show_On_Respawn=\"{1}\" Auto_Show=\"{2}\" Delay=\"{3}\" />", Bloodmoon.IsEnabled, Bloodmoon.Show_On_Respawn, Bloodmoon.Auto_Show, Bloodmoon.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon_Warrior\" Enable=\"{0}\" Zombie_Kills=\"{1}\" Chance=\"{2}\" Reduce_Death_Count=\"{3}\" />", BloodmoonWarrior.IsEnabled, BloodmoonWarrior.Zombie_Kills, BloodmoonWarrior.Chance, BloodmoonWarrior.Reduce_Death_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Bounties\" Enable=\"{0}\" Minimum_Bounty=\"{1}\" Kill_Streak=\"{2}\" Bonus=\"{3}\" />", Bounties.IsEnabled, Bounties.Minimum_Bounty, Bounties.Kill_Streak, Bounties.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Break_Reminder\" Enable=\"{0}\" Break_Time=\"{1}\" Break_Message=\"{2}\" />", BreakTime.IsEnabled, BreakTime.Break_Time, BreakTime.Break_Message));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Color_Prefix\" Enable=\"{0}\" />", ChatColorPrefix.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Log\" Enable=\"{0}\" />", ChatCommandLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response\" Server_Response_Name=\"{0}\" Main_Color=\"{1}\" Chat_Command_Private=\"{2}\" Chat_Command_Public=\"{3}\" />", Server_Response_Name, Chat_Response_Color, ChatHook.Command_Private, ChatHook.Command_Public));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response_Extended\" Friend_Chat_Color=\"{0}\" Party_Chat_Color=\"{1}\" />", ChatHook.Friend_Chat_Color, ChatHook.Party_Chat_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Flood_Protection\" Enable=\"{0}\" Max_Length=\"{1}\" Messages_Per_Min=\"{2}\" Wait_Time=\"{3}\" />", ChatHook.ChatFlood, ChatHook.Max_Length, ChatHook.Messages_Per_Min, ChatHook.Wait_Time));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Logger\" Enable=\"{0}\" />", ChatLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Clan_Manager\" Enable=\"{0}\" Max_Name_Length=\"{1}\" Private_Chat_Color=\"{2}\" />", ClanManager.IsEnabled, ClanManager.Max_Name_Length, ClanManager.Private_Chat_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Console_Command_Log\" Enable=\"{0}\" />", ConsoleCommandLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Country_Ban\" Enable=\"{0}\" Countries_Not_Allowed=\"CN,IL\" />", CountryBan.IsEnabled, CountryBan.Countries_Not_Allowed));
                sw.WriteLine(string.Format("        <Tool Name=\"Credentials\" Enable=\"{0}\" No_Family_Share=\"{1}\" No_Bad_Id=\"{2}\" No_Internal=\"{3}\" Admin_Level=\"{4}\" />", CredentialCheck.IsEnabled, CredentialCheck.Family_Share, CredentialCheck.Bad_Id, CredentialCheck.No_Internal, CredentialCheck.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Custom_Commands\" Enable=\"{0}\" />", CustomCommands.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Damage_Detector\" Enable=\"{0}\" Entity_Damage_Limit=\"{1}\" Block_Damage_Limit=\"{2}\" Player_Damage_Limit=\"{3}\" Admin_Level=\"{4}\" />", ProcessDamage.Damage_Detector, ProcessDamage.Entity_Damage_Limit, ProcessDamage.Block_Damage_Limit, ProcessDamage.Player_Damage_Limit, ProcessDamage.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Day7\" Enable=\"{0}\" />", Day7.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Death_Spot\" Enable=\"{0}\" Time=\"{1}\" Delay_Between_Uses=\"{2}\" Command_Cost=\"{3}\" />", DeathSpot.IsEnabled, DeathSpot.Time, DeathSpot.Delay_Between_Uses, DeathSpot.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Dupe_Log\" Enable=\"{0}\" />", DupeLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup\" Enable=\"{0}\" Falling_Blocks=\"{1}\" Falling_Tree=\"{2}\" Entity_Underground=\"{3}\" Delete_Bicycles=\"{4}\" />", EntityCleanup.IsEnabled, EntityCleanup.BlockIsEnabled, EntityCleanup.FallingTreeEnabled, EntityCleanup.Underground, EntityCleanup.Bicycles));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup_Extended\" Delete_MiniBikes=\"{0}\" Delete_MotorBikes=\"{1}\" Delete_Jeeps=\"{2}\" Delete_Gyros=\"{3}\" />", EntityCleanup.MiniBikes, EntityCleanup.MotorBikes, EntityCleanup.Jeeps, EntityCleanup.Gyros));
                sw.WriteLine(string.Format("        <Tool Name=\"First_Claim_Block\" Enable=\"{0}\" />", FirstClaimBlock.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Flying_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" Flags=\"{2}\" />", PlayerChecks.FlyEnabled, PlayerChecks.Flying_Admin_Level, PlayerChecks.Flying_Flags));
                sw.WriteLine(string.Format("        <Tool Name=\"FPS\" Enable=\"{0}\" Set_Target=\"{1}\" />", Fps.IsEnabled, Fps.Set_Target));
                sw.WriteLine(string.Format("        <Tool Name=\"Friend_Teleport\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" PvP_Check=\"{3}\" Zombie_Check=\"{4}\" />", FriendTeleport.IsEnabled, FriendTeleport.Delay_Between_Uses, FriendTeleport.Command_Cost, FriendTeleport.PvP_Check, FriendTeleport.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Gimme\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Zombies=\"{2}\" Command_Cost=\"{3}\" />", Gimme.IsEnabled, Gimme.Delay_Between_Uses, Gimme.Zombies, Gimme.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Godmode_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" />", PlayerChecks.GodEnabled, PlayerChecks.Godmode_Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Hardcore\" Enable=\"{0}\" Optional=\"{1}\" Max_Deaths=\"{2}\" Max_Extra_Lives=\"{3}\" Life_Price=\"{4}\" />", Hardcore.IsEnabled, Hardcore.Optional, Hardcore.Max_Deaths, Hardcore.Max_Extra_Lives, Hardcore.Life_Price));
                sw.WriteLine(string.Format("        <Tool Name=\"High_Ping_Kicker\" Enable=\"{0}\" Max_Ping=\"{1}\" Flags=\"{2}\" />", HighPingKicker.IsEnabled, HighPingKicker.Max_Ping, HighPingKicker.Flags));
                sw.WriteLine(string.Format("        <Tool Name=\"Home\" Enable=\"{0}\" Home2_Enabled=\"{1}\" Home2_Reserved_Only=\"{2}\" Home2_Delay=\"{3}\" Delay_Between_Uses=\"{4}\" Command_Cost=\"{5}\" />", Home.IsEnabled, Home.Home2_Enabled, Home.Home2_Reserved_Only, Home.Home2_Delay, Home.Delay_Between_Uses, Home.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Home_Extended\" Player_Check=\"{0}\" Zombie_Check=\"{1}\" Vehicle_Check=\"{2}\" />", Home.Player_Check, Home.Zombie_Check, Home.Vehicle_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Hordes\" Enable=\"{0}\" />", Hordes.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Info_Ticker\" Enable=\"{0}\" Delay_Between_Messages=\"{1}\" Random=\"{2}\" />", InfoTicker.IsEnabled, InfoTicker.Delay, InfoTicker.Random));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Items\" Enable=\"{0}\" Ban=\"{1}\" Admin_Level=\"{2}\" Check_Storage=\"{3}\" />", InvalidItems.IsEnabled, InvalidItems.Ban_Player, InvalidItems.Admin_Level, InvalidItems.Check_Storage));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Item_Stack\" Enable=\"{0}\" />", InvalidItems.Invalid_Stack));
                sw.WriteLine(string.Format("        <Tool Name=\"Jail\" Enable=\"{0}\" Jail_Size=\"{1}\" Jail_Position=\"{2}\" Jail_Shock=\"{3}\" />", Jail.IsEnabled, Jail.Jail_Size, Jail.Jail_Position, Jail.Jail_Shock));
                sw.WriteLine(string.Format("        <Tool Name=\"Kick_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", KickVote.IsEnabled, KickVote.Players_Online, KickVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Kill_Notice\" Enable=\"{0}\" Show_Level=\"{1}\" Show_Damage=\"{2}\" />", KillNotice.IsEnabled, KillNotice.Show_Level, KillNotice.Show_Damage));
                sw.WriteLine(string.Format("        <Tool Name=\"Lobby\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Lobby_Size=\"{3}\" Lobby_Position=\"{4}\" Donor_Only=\"{5}\" />", Lobby.IsEnabled, Lobby.Return, Lobby.Delay_Between_Uses, Lobby.Lobby_Size, Lobby.Lobby_Position, Lobby.Donor_Only));
                sw.WriteLine(string.Format("        <Tool Name=\"Lobby_Extended\" Command_Cost=\"{0}\" Player_Check=\"{1}\" Zombie_Check=\"{2}\" PvE=\"{3}\" />", Lobby.Command_Cost, Lobby.Player_Check, Lobby.Zombie_Check, Lobby.PvE));
                sw.WriteLine(string.Format("        <Tool Name=\"Location\" Enable=\"{0}\" />", Loc.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Login_Notice\" Enable=\"{0}\" />", LoginNotice.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Logs\" Days_Before_Log_Delete=\"{0}\" />", LoadProcess.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Lottery\" Enable=\"{0}\" Bonus=\"{1}\" />", Lottery.IsEnabled, Lottery.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Market\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Market_Size=\"{3}\" Market_Position=\"{4}\" Donor_Only=\"{5}\" />", Market.IsEnabled, Market.Return, Market.Delay_Between_Uses, Market.Market_Size, Market.Market_Position, Market.Donor_Only));
                sw.WriteLine(string.Format("        <Tool Name=\"Market_Extended\" Command_Cost=\"{0}\" Player_Check=\"{1}\" Zombie_Check=\"{2}\" PvE=\"{3}\" />", Market.Command_Cost, Market.Player_Check, Market.Zombie_Check, Market.PvE));
                sw.WriteLine(string.Format("        <Tool Name=\"Message_Color\" Enable=\"{0}\" Color=\"{1}\" />", ChatHook.Message_Color_Enabled, ChatHook.Message_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Motd\" Enable=\"{0}\" Show_On_Respawn=\"{1}\" />", Motd.IsEnabled, Motd.Show_On_Respawn));
                sw.WriteLine(string.Format("        <Tool Name=\"Mute\" Enable=\"{0}\" Block_Commands=\"{1}\" />", Mute.IsEnabled, Mute.Block_Commands));
                sw.WriteLine(string.Format("        <Tool Name=\"Mute_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", MuteVote.IsEnabled, MuteVote.Players_Online, MuteVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Net_Packages\" Enable=\"{0}\" />", Packages.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Player\" Enable=\"{0}\" Entry_Message=\"{1}\" />", NewPlayer.IsEnabled, NewPlayer.Entry_Message));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Spawn_Tele\" Enable=\"{0}\" New_Spawn_Tele_Position=\"{1}\" Return=\"{2}\" />", NewSpawnTele.IsEnabled, NewSpawnTele.New_Spawn_Tele_Position, NewSpawnTele.Return));
                sw.WriteLine(string.Format("        <Tool Name=\"Night_Alert\" Enable=\"{0}\" Delay=\"{1}\" />", NightAlert.IsEnabled, NightAlert.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Normal_Player_Color_Prefix\" Enable=\"{0}\" Prefix=\"{1}\" Name_Color=\"{2}\" Prefix_Color=\"{3}\" />", ChatHook.Normal_Player_Color_Prefix, ChatHook.Normal_Player_Prefix, ChatHook.Normal_Player_Name_Color, ChatHook.Normal_Player_Prefix_Color));
                //sw.WriteLine(string.Format("        <Tool Name=\"Output_Log\" Enable=\"{0}\" />", OutputLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_List\" Enable=\"{0}\" />", PlayerList.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Logs\" Enable=\"{0}\" Interval=\"{1}\" />", PlayerLogs.IsEnabled, PlayerLogs.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stats\" Enable=\"{0}\" Max_Speed=\"{1}\" Health=\"{2}\" Stamina=\"{3}\" Jump_Strength=\"{4}\" Height=\"{5}\" />", PlayerStats.IsEnabled, PlayerStats.Max_Speed, PlayerStats.Health, PlayerStats.Stamina, PlayerStats.Jump_Strength, PlayerStats.Height));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stats_Extended\" Admin_Level=\"{0}\" Kick_Enabled=\"{1}\" Ban_Enabled=\"{2}\" />", PlayerStats.Admin_Level, PlayerStats.Kick_Enabled, PlayerStats.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Poll\" Enable=\"{0}\" />", Poll.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Prayer\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", Prayer.IsEnabled, Prayer.Delay_Between_Uses, Prayer.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Private_Message\" Enable=\"{0}\" />", Whisper.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Protected_Spaces\" Enable=\"{0}\" />", ProtectedSpaces.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"PvE_Violations\" Jail=\"{0}\" Kill=\"{1}\" Kick=\"{2}\" Ban=\"{3}\" />", PersistentOperations.Jail_Violation, PersistentOperations.Kill_Violation, PersistentOperations.Kick_Violation, PersistentOperations.Ban_Violation));
                sw.WriteLine(string.Format("        <Tool Name=\"Real_World_Time\" Enable=\"{0}\" Delay=\"{1}\" Time_Zone=\"{2}\" Adjustment=\"{3}\" />", RealWorldTime.IsEnabled, RealWorldTime.Delay, RealWorldTime.Time_Zone, RealWorldTime.Adjustment));
                sw.WriteLine(string.Format("        <Tool Name=\"Report\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Length=\"{2}\" Admin_Level=\"{3}\" />", Report.IsEnabled, Report.Delay, Report.Length, Report.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Reserved_Slots\" Enable=\"{0}\" Session_Time=\"{1}\" Admin_Level=\"{2}\" Reduced_Delay=\"{3}\" />", ReservedSlots.IsEnabled, ReservedSlots.Session_Time, ReservedSlots.Admin_Level, ReservedSlots.Reduced_Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Restart_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" Admin_Level=\"{3}\" />", RestartVote.IsEnabled, RestartVote.Players_Online, RestartVote.Votes_Needed, RestartVote.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Scout_Player\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", ScoutPlayer.IsEnabled, ScoutPlayer.Delay_Between_Uses, ScoutPlayer.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Shop\" Enable=\"{0}\" Inside_Market=\"{1}\" Inside_Traders=\"{2}\" />", Shop.IsEnabled, Shop.Inside_Market, Shop.Inside_Traders));
                sw.WriteLine(string.Format("        <Tool Name=\"Shutdown\" Enable=\"{0}\" Countdown_Timer=\"{1}\" Time_Before_Shutdown=\"{2}\" Alert_On_Login=\"{3}\" Alert_Count=\"{4}\"  />", Shutdown.IsEnabled, Shutdown.Countdown_Timer, Shutdown.Delay, Shutdown.Alert_On_Login, Shutdown.Alert_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Spectator_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" />", PlayerChecks.SpectatorEnabled, PlayerChecks.Spectator_Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Starting_Items\" Enable=\"{0}\" />", StartingItems.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Stuck\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", Stuck.IsEnabled, Stuck.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Suicide\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" PvP_Check=\"{2}\" Zombie_Check=\"{3}\" />", Suicide.IsEnabled, Suicide.Delay_Between_Uses, Suicide.PvP_Check, Suicide.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Tracking\" Enable=\"{0}\" />", Track.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Travel\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" PvP_Check=\"{3}\" Zombie_Check=\"{4}\" />", Travel.IsEnabled, Travel.Delay_Between_Uses, Travel.Command_Cost, Travel.PvP_Check, Travel.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Vehicle_Teleport\" Enable=\"{0}\" Bike=\"{1}\" Mini_Bike=\"{2}\" Motor_Bike=\"{3}\" Jeep=\"{4}\" Gyro=\"{5}\" />", VehicleTeleport.IsEnabled, VehicleTeleport.Bike, VehicleTeleport.Mini_Bike, VehicleTeleport.Motor_Bike, VehicleTeleport.Jeep, VehicleTeleport.Gyro));
                sw.WriteLine(string.Format("        <Tool Name=\"Vehicle_Teleport_Extended\" Inside_Claim=\"{0}\" Distance=\"{1}\" Delay_Between_Uses=\"{2}\" Command_Cost=\"{3}\" />", VehicleTeleport.Inside_Claim, VehicleTeleport.Distance, VehicleTeleport.Delay_Between_Uses, VehicleTeleport.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting\" Enable=\"{0}\" Your_Voting_Site=\"{1}\" API_Key=\"{2}\" Delay_Between_Uses=\"{3}\" Reward_Count=\"{4}\" />", VoteReward.IsEnabled, VoteReward.Your_Voting_Site, VoteReward.API_Key, VoteReward.Delay_Between_Uses, VoteReward.Reward_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting_Extended\" Reward_Entity=\"{0}\" Entity_Id=\"{1}\" Weekly_Votes=\"{2}\" />", VoteReward.Reward_Entity, VoteReward.Entity_Id, VoteReward.Weekly_Votes));
                sw.WriteLine(string.Format("        <Tool Name=\"Wallet\" Enable=\"{0}\" Coin_Name=\"{1}\" PVP=\"{2}\" Zombie_Kill_Value=\"{3}\" Player_Kill_Value=\"{4}\" Death_Penalty_Value=\"{5}\" />", Wallet.IsEnabled, Wallet.Coin_Name, Wallet.PVP, Wallet.Zombie_Kills, Wallet.Player_Kills, Wallet.Deaths));
                sw.WriteLine(string.Format("        <Tool Name=\"Wallet_Extended\" Lose_On_Death=\"{0}\" Bank_Transfers=\"{1}\" Session_Bonus=\"{2}\" />", Wallet.Lose_On_Death, Wallet.Bank_Transfers, Wallet.Session_Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Watchlist\" Enable=\"{0}\" Admin_Level=\"{1}\" Alert_Delay=\"{2}\" />", Watchlist.IsEnabled, Watchlist.Admin_Level, Watchlist.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Waypoints\" Enable=\"{0}\" Max_Waypoints =\"{1}\" Donator_Max_Waypoints=\"{2}\" Command_Cost =\"{3}\" Delay_Between_Uses=\"{4}\" PvP_Check =\"{5}\" />", Waypoints.IsEnabled, Waypoints.Max_Waypoints, Waypoints.Donator_Max_Waypoints, Waypoints.Command_Cost, Waypoints.Delay_Between_Uses, Waypoints.PvP_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Waypoints_extended\" Zombie_Check=\"{0}\" Vehicle=\"{1}\" />", Waypoints.Zombie_Check, Waypoints.Vehicle));
                sw.WriteLine(string.Format("        <Tool Name=\"Weather_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", WeatherVote.IsEnabled, WeatherVote.Players_Online, WeatherVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Website\" Enable=\"{0}\" />", WebsiteServer.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"World_Radius\" Enable=\"{0}\" Normal_Player=\"{1}\" Reserved=\"{2}\" Admin_Level=\"{3}\" />", WorldRadius.IsEnabled, WorldRadius.Normal_Player, WorldRadius.Reserved, WorldRadius.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Zones\" Enable=\"{0}\" Zone_Message=\"{1}\" Reminder_Notice_Delay=\"{2}\" Set_Home=\"{3}\"  />", Zones.IsEnabled, Zones.Zone_Message, Zones.Reminder_Delay, Zones.Set_Home));
                sw.WriteLine("    </Tools>");
                sw.WriteLine("</ServerTools>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            LoadXml();
            Mods.Load();
        }

        public static void UpgradeXml()
        {
            try
            {
                if (OldXmlDirectory != "")
                {
                    if (Utils.FileExists(configFilePath))
                    {
                        XmlDocument _oldXml = new XmlDocument();
                        try
                        {
                            _oldXml.Load(OldXmlDirectory + "/ServerToolsConfig.xml");
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", OldXmlDirectory + "/ServerToolsConfig.xml", e.Message));
                            return;
                        }
                        XmlNode _oldXmlNode = _oldXml.DocumentElement;
                        XmlNodeList _oldNodeList = _oldXmlNode.ChildNodes;
                        XmlDocument _newXml = new XmlDocument();
                        try
                        {
                            _newXml.Load(configFilePath);
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", configFilePath, e.Message));
                            return;
                        }
                        XmlNode _newXmlNode = _newXml.DocumentElement;
                        XmlNodeList _newNodeList = _newXmlNode.ChildNodes;
                        for (int i = 0; i < _oldNodeList.Count; i++)
                        {
                            XmlNode _oldChildNode = _oldNodeList[i];
                            if (_oldChildNode.Name == "Tools")
                            {
                                for (int j = 0; j < _oldChildNode.ChildNodes.Count; j++)
                                {
                                    XmlNode _oldSubChild = _oldChildNode.ChildNodes[j];
                                    if (_oldSubChild.Name == "Tool")
                                    {
                                        XmlElement _oldElement = (XmlElement)_oldSubChild;
                                        XmlAttributeCollection _attributes = _oldElement.Attributes;
                                        string _elementName = _attributes[0].Value;
                                        for (int k = 1; k < _attributes.Count; k++)
                                        {
                                            XmlAttribute _oldAttribute = _attributes[k];
                                            SetXml(_newXml, _newNodeList, _elementName, _oldAttribute);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoadConfig.UpgradeXml: {0}", e.Message));
            }
        }

        private static void SetXml(XmlDocument _newXml, XmlNodeList _newNodeList, string _elementName, XmlAttribute _oldAttribute)
        {
            try
            {
                for (int i = 0; i < _newNodeList.Count; i++)
                {
                    XmlNode _newChildNode = _newNodeList[i];
                    if (_newChildNode.Name == "Tools")
                    {
                        for (int j = 0; j < _newChildNode.ChildNodes.Count; j++)
                        {
                            XmlNode _newSubChild = _newChildNode.ChildNodes[j];
                            if (_newSubChild.Name == "Tool")
                            {
                                XmlElement _newElement = (XmlElement)_newSubChild;
                                XmlAttributeCollection _newAttributes = _newElement.Attributes;
                                if (_newElement.Attributes[0].Value == _elementName)
                                {
                                    for (int k = 1; k < _newElement.Attributes.Count; k++)
                                    {
                                        XmlAttribute _newAttribute = _newElement.Attributes[k];
                                        if (_newAttribute.Name == _oldAttribute.Name)
                                        {
                                            if (_newAttribute.Value != _oldAttribute.Value)
                                            {
                                                _newAttribute.Value = _oldAttribute.Value;
                                                _newXml.Save(configFilePath);
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoadConfig.SetXml: {0}", e.Message));
            }
        }
    }
}