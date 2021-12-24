using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Config
    {
        public const string Version = "20.0.1";
        public static bool Upgrade = false;
        public static string Server_Response_Name = "[FFCC00]ServerTools", Chat_Response_Color = "[00FF00]";
        public static string ConfigFilePath = string.Format("{0}/{1}", API.ConfigPath, ConfigFile);

        private const string ConfigFile = "ServerToolsConfig.xml";
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, ConfigFile);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void LoadXml()
        {
            if (!File.Exists(ConfigFilePath))
            {
                WriteXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(ConfigFilePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", ConfigFilePath, e.Message));
                return;
            }
            Upgrade = false;
            XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode childNode = nodeList[i];
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Version' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement line = (XmlElement)subChild;
                        if (!line.HasAttribute("Version"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring config entry in ServerToolsConfig.xml because of missing 'Version' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else if (line.GetAttribute("Version") != Version)
                        {
                            Log.Out("[SERVERTOOLS] Detected ServerTools upgrade");
                            string version = line.GetAttribute("Version");
                            string[] files = Directory.GetFiles(API.ConfigPath, "*.xml");
                            if (!Directory.Exists(API.ConfigPath + "/XMLBackups"))
                            {
                                Directory.CreateDirectory(API.ConfigPath + "/XMLBackups");
                            }
                            if (files != null && files.Length > 0)
                            {
                                if (!Directory.Exists(API.ConfigPath + "/XMLBackups/" + version))
                                {
                                    Directory.CreateDirectory(API.ConfigPath + "/XMLBackups/" + version);
                                    for (int j = 0; j < files.Length; j++)
                                    {
                                        string fileName = files[j];
                                        string fileNameShort = fileName.Substring(fileName.IndexOf("ServerTools") + 11);
                                        File.Copy(fileName, API.ConfigPath + "/XMLBackups/" + version + fileNameShort);
                                    }
                                    Log.Out("[SERVERTOOLS] Created a backup of Xml files for version {0}", version);
                                }
                            }
                            Upgrade = true;
                        }
                    }
                }
                if (childNode.Name == "Tools" && !Upgrade)
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
                        XmlElement line = (XmlElement)subChild;
                        if (!line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring tool entry in ServerToolsConfig.xml because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        switch (line.GetAttribute("Name"))
                        {
                            case "Admin_Chat_Commands":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Chat_Commands entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AdminChat.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Chat_Commands entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Admin_List":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AdminList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out AdminList.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Moderator_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of missing 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Moderator_Level"), out AdminList.Mod_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Animal_Tracking":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AnimalTracking.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out AnimalTracking.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Minimum_Spawn_Radius"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of missing 'Minimum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Minimum_Spawn_Radius"), out AnimalTracking.Minimum_Spawn_Radius))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Minimum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Maximum_Spawn_Radius"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of missing 'Maximum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Maximum_Spawn_Radius"), out AnimalTracking.Maximum_Spawn_Radius))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Maximum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Animal_Ids"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry in ServerToolsConfig.xml because of missing 'Animal_Ids' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    AnimalTracking.Animal_Ids = line.GetAttribute("Animal_Ids");
                                }
                                break;
                            case "Animal_Tracking_Extended":
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking_Extended entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out AnimalTracking.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auction":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Auction.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("No_Admins"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of missing 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("No_Admins"), out Auction.No_Admins))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of invalid (True/False) value for 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out Auction.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Total_Items"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of missing 'Total_Items' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Total_Items"), out Auction.Total_Items))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Total_Items' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Tax"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of missing 'Tax' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Tax"), out Auction.Tax))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Tax' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auto_Backup":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AutoBackup.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of missing 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay_Between_Saves"))
                                {
                                    AutoBackup.Delay = line.GetAttribute("Delay_Between_Saves");
                                }
                                if (!line.HasAttribute("Destination"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of missing 'Destination' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Destination"))
                                {
                                    AutoBackup.Destination = line.GetAttribute("Destination");
                                }
                                if (!line.HasAttribute("Compression_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of missing 'Compression_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Compression_Level"), out AutoBackup.Compression_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Compression_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Backup_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of missing 'Backup_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Backup_Count"), out AutoBackup.Backup_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Backup_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (AutoBackup.IsEnabled)
                                {
                                    AutoBackup.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("AutoBackup"))
                                {
                                    EventSchedule.Remove("AutoBackup");
                                }
                                break;
                            case "Auto_Party_Invite":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Party_Invite entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AutoPartyInvite.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Party_Invite entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auto_Save_World":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out AutoSaveWorld.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry in ServerToolsConfig.xml because of missing 'Delay_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay_Between_Saves"))
                                {
                                    AutoSaveWorld.Delay = line.GetAttribute("Delay_Between_Saves");
                                }
                                if (AutoSaveWorld.IsEnabled)
                                {
                                    AutoSaveWorld.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("AutoSaveWorld"))
                                {
                                    EventSchedule.Remove("AutoSaveWorld");
                                }
                                break;
                            case "Bad_Word_Filter":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Badwords.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Invalid_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry in ServerToolsConfig.xml because of missing 'Invalid_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Invalid_Name"), out Badwords.Invalid_Name))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Invalid_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bank":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!PersistentOperations.No_Currency)
                                {
                                    if (!bool.TryParse(line.GetAttribute("Enable"), out Bank.IsEnabled))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                        continue;
                                    }
                                }
                                if (!line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Inside_Claim"), out Bank.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Deposit_Fee_Percent"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of missing 'Deposit_Fee_Percent' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Deposit_Fee_Percent"), out Bank.Deposit_Fee_Percent))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Deposit_Fee_Percent' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Transfers"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of missing 'Player_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Transfers"), out Bank.Player_Transfers))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Block_Logger":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Logger entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out BlockLogger.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Logger entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bloodmoon":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Bloodmoon.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry in ServerToolsConfig.xml because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay"))
                                {
                                    Bloodmoon.Delay = line.GetAttribute("Delay");
                                }
                                if (!line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry in ServerToolsConfig.xml because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Show_On_Respawn"), out Bloodmoon.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (Bloodmoon.IsEnabled)
                                {
                                    Bloodmoon.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("Bloodmoon"))
                                {
                                    EventSchedule.Remove("Bloodmoon");
                                }
                                break;
                            case "Bloodmoon_Warrior":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out BloodmoonWarrior.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Kills"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of missing 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Zombie_Kills"), out BloodmoonWarrior.Zombie_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Chance"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of missing 'Chance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Chance"), out BloodmoonWarrior.Chance))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Chance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reduce_Death_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of missing 'Reduce_Death_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Reduce_Death_Count"), out BloodmoonWarrior.Reduce_Death_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Reduce_Death_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reward_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of missing 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reward_Count"), out BloodmoonWarrior.Reward_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon_Warrior entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bot_Response":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bot_Response entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out BotResponse.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bot_Response entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Bounties":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Bounties.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Minimum_Bounty"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of missing 'Minimum_Bounty' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Minimum_Bounty"), out Bounties.Minimum_Bounty))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Minimum_Bounty' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Kill_Streak"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of missing 'Kill_Streak' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Kill_Streak"), out Bounties.Kill_Streak))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Kill_Streak' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of missing 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Bonus"), out Bounties.Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Break_Reminder":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out BreakTime.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Break_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry in ServerToolsConfig.xml because of missing 'Break_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Break_Time"))
                                {
                                    BreakTime.Delay = line.GetAttribute("Break_Time");
                                }
                                if (!line.HasAttribute("Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Break_Reminder entry in ServerToolsConfig.xml because of missing 'Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Message"))
                                {
                                    BreakTime.Message = line.GetAttribute("Message");
                                }
                                if (BreakTime.IsEnabled)
                                {
                                    BreakTime.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("BreakTime"))
                                {
                                    EventSchedule.Remove("BreakTime");
                                }
                                break;
                            case "Chat_Color":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatColor.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Rotate"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of missing 'Rotate' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Rotate"), out ChatColor.Rotate))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Rotate' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Custom_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of missing 'Custom_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Custom_Color"), out ChatColor.Custom_Color))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Custom_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Command_Log":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Log entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatCommandLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Log entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Command_Response":
                                if (!line.HasAttribute("Server_Response_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry in ServerToolsConfig.xml because of missing 'Server_Response_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Main_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry in ServerToolsConfig.xml because of missing 'Main_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Chat_Command_Prefix1"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry in ServerToolsConfig.xml because of missing 'Chat_Command_Prefix1' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Chat_Command_Prefix2"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry in ServerToolsConfig.xml because of missing 'Chat_Command_Prefix2' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Server_Response_Name"))
                                {
                                    Server_Response_Name = line.GetAttribute("Server_Response_Name");
                                }
                                if (line.HasAttribute("Main_Color"))
                                {
                                    Chat_Response_Color = line.GetAttribute("Main_Color");
                                }
                                if (line.HasAttribute("Chat_Command_Prefix1"))
                                {
                                    ChatHook.Chat_Command_Prefix1 = line.GetAttribute("Chat_Command_Prefix1");
                                }
                                if (line.HasAttribute("Chat_Command_Prefix2"))
                                {
                                    ChatHook.Chat_Command_Prefix2 = line.GetAttribute("Chat_Command_Prefix2");
                                }
                                break;
                            case "Chat_Command_Response_Extended":
                                if (line.HasAttribute("Friend_Chat_Color"))
                                {
                                    ChatHook.Friend_Chat_Color = line.GetAttribute("Friend_Chat_Color");
                                }
                                if (line.HasAttribute("Party_Chat_Color"))
                                {
                                    ChatHook.Party_Chat_Color = line.GetAttribute("Party_Chat_Color");
                                }
                                if (!line.HasAttribute("Passthrough"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of missing 'Passthrough' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Passthrough"), out ChatHook.Passthrough))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Passthrough' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Flood_Protection":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatHook.ChatFlood))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of missing 'Max_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Length"), out ChatHook.Max_Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Messages_Per_Min"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of missing 'Messages_Per_Min' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Messages_Per_Min"), out ChatHook.Messages_Per_Min))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Messages_Per_Min' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Wait_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of missing 'Wait_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Wait_Time"), out ChatHook.Wait_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Wait_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Chat_Logger":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Clan_Manager":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ClanManager.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Name_Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry in ServerToolsConfig.xml because of missing 'Max_Name_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Name_Length"), out ClanManager.Max_Name_Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Name_Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Private_Chat_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry in ServerToolsConfig.xml because of missing 'Private_Chat_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Private_Chat_Color"))
                                {
                                    ClanManager.Private_Chat_Color = line.GetAttribute("Private_Chat_Color");
                                }
                                break;
                            case "Clean_Bin":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out CleanBin.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Auction"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of missing 'Auction' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Auction"), out CleanBin.Auction))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Auction' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bank"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of missing 'Bank' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bank"), out CleanBin.Bank))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bank' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bounties"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of missing 'Bounties' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bounties"), out CleanBin.Bounties))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bounties' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delays"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of missing 'Delays' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delays"), out CleanBin.Delays))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delays' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Clean_Bin_Extended1":
                                if (!line.HasAttribute("Homes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of missing 'Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Homes"), out CleanBin.Homes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Jail"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of missing 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Jail"), out CleanBin.Jail))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Lobby"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of missing 'Lobby' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Lobby"), out CleanBin.Lobby))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Lobby' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Market"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of missing 'Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Market"), out CleanBin.Market))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("New_Spawn_Tele"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of missing 'New_Spawn_Tele' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("New_Spawn_Tele"), out CleanBin.New_Spawn_Tele))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended1 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'New_Spawn_Tele' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Clean_Bin_Extended2":
                                if (!line.HasAttribute("Poll"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of missing 'Poll' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Poll"), out CleanBin.Poll))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Poll' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Protected_Spaces"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of missing 'Protected_Spaces' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Protected_Spaces"), out CleanBin.Protected_Spaces))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Protected_Spaces' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Vehicles"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of missing 'Vehicles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Vehicles"), out CleanBin.Vehicles))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Vehicles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Wallet"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of missing 'Wallet' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Wallet"), out CleanBin.Wallet))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Wallet' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of missing 'Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Waypoints"), out CleanBin.Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clean_Bin_Extended2 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Console_Command_Log":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Console_Command_Log entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ConsoleCommandLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Console_Command_Log entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Country_Ban":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out CountryBan.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry in ServerToolsConfig.xml because of missing 'Countries_Not_Allowed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    CountryBan.Countries_Not_Allowed = line.GetAttribute("Countries_Not_Allowed");
                                }
                                break;
                            case "Custom_Commands":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom_Commands entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out CustomCommands.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom_Commands entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Damage_Detector":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out DamageDetector.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Entity_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of missing 'Entity_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Entity_Damage_Limit"), out DamageDetector.Entity_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Entity_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Block_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of missing 'Block_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Block_Damage_Limit"), out DamageDetector.Block_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Block_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Damage_Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of missing 'Player_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Player_Damage_Limit"), out DamageDetector.Player_Damage_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Player_Damage_Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out DamageDetector.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Damage_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Day7":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Day7.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Died":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Died.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of missing 'Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Time"), out Died.Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Died.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Died.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Died entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Discord_Bot":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out DiscordBot.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Webhook"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot entry in ServerToolsConfig.xml because of missing 'Webhook' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Webhook"))
                                {
                                    DiscordBot.Webhook = line.GetAttribute("Webhook");
                                }
                                break;
                            case "Discord_Bot_Extended":
                                if (!line.HasAttribute("Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot_Extended entry in ServerToolsConfig.xml because of missing 'Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Prefix"))
                                {
                                    DiscordBot.Prefix = line.GetAttribute("Prefix");
                                }
                                if (!line.HasAttribute("Prefix_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot_Extended entry in ServerToolsConfig.xml because of missing 'Prefix_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    DiscordBot.Prefix_Color = line.GetAttribute("Prefix_Color");
                                }
                                if (!line.HasAttribute("Name_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot_Extended entry in ServerToolsConfig.xml because of missing 'Name_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    DiscordBot.Name_Color = line.GetAttribute("Name_Color");
                                }
                                if (!line.HasAttribute("Message_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Discord_Bot_Extended entry in ServerToolsConfig.xml because of missing 'Message_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Message_Color"))
                                {
                                    DiscordBot.Message_Color = line.GetAttribute("Message_Color");
                                }
                                break;
                            case "Dropped_Bag_Protection":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dropped_Bag_Protection entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out DroppedBagProtection.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dropped_Bag_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Friend_Access"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dropped_Bag_Protection entry in ServerToolsConfig.xml because of missing 'Friend_Access' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Friend_Access"), out DroppedBagProtection.Friend_Access))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dropped_Bag_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Friend_Access' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Dupe_Log":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dupe_Log entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out DupeLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dupe_Log entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Entity_Cleanup":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out EntityCleanup.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Falling_Blocks"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Falling_Blocks"), out EntityCleanup.BlockIsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Falling_Tree"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Falling_Tree"), out EntityCleanup.FallingTreeEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Entity_Underground"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Entity_Underground"), out EntityCleanup.Underground))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delete_Bicycles"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Delete_Bicycles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delete_Bicycles"), out EntityCleanup.Bicycles))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delete_Bicycles' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Entity_Cleanup_Extended":
                                if (!line.HasAttribute("Delete_MiniBikes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Delete_MiniBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delete_MiniBikes"), out EntityCleanup.MiniBikes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delete_MiniBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delete_MotorBikes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Delete_MotorBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delete_MotorBikes"), out EntityCleanup.MotorBikes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delete_MotorBikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delete_Jeeps"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Delete_Jeeps' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delete_Jeeps"), out EntityCleanup.Jeeps))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delete_Jeeps' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delete_Gyros"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of missing 'Delete_Gyros' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Delete_Gyros"), out EntityCleanup.Gyros))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Delete_Gyros' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Exit_Command":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ExitCommand.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("All"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of missing 'All' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("All"), out ExitCommand.All))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of invalid (True/False) value for 'All' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Belt"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of missing 'Belt' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Belt"), out ExitCommand.Belt))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Belt' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bag"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of missing 'Bag' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bag"), out ExitCommand.Bag))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bag' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Equipment"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of missing 'Equipment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Equipment"), out ExitCommand.Equipment))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Equipment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Exit_Command_Extended":
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command_Extended entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out ExitCommand.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Exit_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command_Extended entry in ServerToolsConfig.xml because of missing 'Exit_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Exit_Time"), out ExitCommand.Exit_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Exit_Command_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Exit_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Falling_Blocks_Remover":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out FallingBlocks.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Log"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of missing 'Log' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Log"), out FallingBlocks.OutputLog))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Log' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Blocks"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of missing 'Max_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Blocks"), out FallingBlocks.Max_Blocks))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Falling_Blocks_Remover entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "First_Claim_Block":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring First_Claim_Block entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out FirstClaimBlock.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring First_Claim_Block entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Flying_Detector":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out FlyingDetector.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out FlyingDetector.Flying_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Flags"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of missing 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Flags"), out FlyingDetector.Flag_Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flying_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "FPS":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Fps.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Set_Target"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of missing 'Set_Target' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Set_Target"), out Fps.Set_Target))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Set_Target' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Low_FPS"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of missing 'Low_FPS' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Low_FPS"), out Fps.Low_FPS))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Low_FPS' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Friend_Teleport":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out FriendTeleport.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out FriendTeleport.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out FriendTeleport.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out FriendTeleport.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out FriendTeleport.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Gamble":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Gamble.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Gamble.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Gamble.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gamble entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Gimme":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Gimme.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Gimme.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombies"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of missing 'Zombies' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombies"), out Gimme.Zombies))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zombies' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of missing 'Zombie_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    Gimme.Zombie_Id = line.GetAttribute("Zombie_Id");
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Gimme.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Godmode_Detector":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PlayerChecks.GodEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out PlayerChecks.Godmode_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Godmode_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Hardcore":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Hardcore.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Optional"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of missing 'Optional' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Optional"), out Hardcore.Optional))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Optional' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Deaths"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of missing 'Max_Deaths' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Deaths"), out Hardcore.Max_Deaths))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Deaths' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Extra_Lives"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of missing 'Max_Extra_Lives' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Extra_Lives"), out Hardcore.Max_Extra_Lives))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Extra_Lives' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Life_Price"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of missing 'Life_Price' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Life_Price"), out Hardcore.Life_Price))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Life_Price' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "High_Ping_Kicker":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out HighPingKicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Flags"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of missing 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Flags"), out HighPingKicker.Flags))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Ping"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of missing 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Ping"), out HighPingKicker.Max_Ping))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Homes":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Homes.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Homes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of missing 'Max_Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Homes"), out Homes.Max_Homes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reserved_Max_Homes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of missing 'Reserved_Max_Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reserved_Max_Homes"), out Homes.Reserved_Max_Homes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reserved_Max_Homes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Homes.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Homes.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Homes_Extended":
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Homes.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Homes.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Vehicle"), out Homes.Vehicle_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Homes_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Hordes":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Hordes.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Infinite_Ammo":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Infinite_Ammo entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out InfiniteAmmo.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Infinite_Ammo entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Info_Ticker":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out InfoTicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry in ServerToolsConfig.xml because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay"))
                                {
                                    InfoTicker.Delay = line.GetAttribute("Delay");
                                }
                                if (!line.HasAttribute("Random"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry in ServerToolsConfig.xml because of missing 'Random' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Random"), out InfoTicker.Random))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Random' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (InfoTicker.IsEnabled)
                                {
                                    InfoTicker.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("InfoTicker"))
                                {
                                    EventSchedule.Remove("InfoTicker");
                                }
                                break;
                            case "Invalid_Items":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out InvalidItems.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Ban"), out InvalidItems.Ban_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out InvalidItems.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Check_Storage"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of missing 'Check_Storage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Check_Storage"), out InvalidItems.Check_Storage))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Items entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Check_Storage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Invalid_Item_Stack":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out InvalidItems.Invalid_Stack))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Jail":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Jail.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Jail_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of missing 'Jail_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Jail_Size"), out Jail.Jail_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Jail_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Jail_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of missing 'Jail_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Jail_Position"))
                                {
                                    Jail.Jail_Position = line.GetAttribute("Jail_Position");
                                }
                                if (!line.HasAttribute("Jail_Shock"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of missing 'Jail_Shock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Jail_Shock"), out Jail.Jail_Shock))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Jail_Shock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Kick_Vote":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out KickVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Players_Online"), out KickVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Votes_Needed"), out KickVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Kill_Notice":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out KillNotice.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("PvP"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of missing 'PvP' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("PvP"), out KillNotice.PvP))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'PvP' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Kills"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of missing 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Kills"), out KillNotice.Zombie_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zombie_Kills' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Show_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of missing 'Show_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Show_Level"), out KillNotice.Show_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Show_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Show_Damage"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of missing 'Show_Damage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Show_Damage"), out KillNotice.Show_Damage))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Show_Damage' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Level_Up":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Level_Up entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out LevelUp.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Level_Up entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Xml_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Xml_Only entry in ServerToolsConfig.xml because of missing 'Xml_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Xml_Only"), out LevelUp.Xml_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Xml_Only entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Xml_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lobby":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Lobby.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Return"), out Lobby.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Lobby.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Lobby_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Lobby_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Lobby_Size"), out Lobby.Lobby_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Lobby_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Lobby_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Lobby_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Lobby_Position"))
                                {
                                    Lobby.Lobby_Position = line.GetAttribute("Lobby_Position");
                                }
                                break;
                            case "Lobby_Extended":
                                if (!line.HasAttribute("Reserved_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Reserved_Only"), out Lobby.Reserved_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Lobby.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Lobby.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Lobby.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("PvE"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of missing 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("PvE"), out Lobby.PvE))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry in ServerToolsConfig.xml because of invalid (True/False) value for 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Location":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Location entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Loc.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Location entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Login_Notice":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out LoginNotice.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Logs":
                                if (!line.HasAttribute("Days_Before_Log_Delete"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Logs entry in ServerToolsConfig.xml because of missing 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Days_Before_Log_Delete"), out LoadProcess.Days_Before_Log_Delete))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Logs entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lottery":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Lottery.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry in ServerToolsConfig.xml because of missing 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Bonus"), out Lottery.Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Market":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Market.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Return"), out Market.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Market.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Market_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Market_Size"), out Market.Market_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Market_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Market_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    Market.Market_Position = line.GetAttribute("Market_Position");
                                }
                                break;
                            case "Market_Extended":
                                if (!line.HasAttribute("Reserved_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Reserved_Only"), out Market.Reserved_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Reserved_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Market.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Market.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Market.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("PvE"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of missing 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("PvE"), out Market.PvE))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry in ServerToolsConfig.xml because of invalid (True/False) value for 'PvE' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Message_Color":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatHook.Message_Color_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry in ServerToolsConfig.xml because of missing 'Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Message_Color = line.GetAttribute("Color");
                                    if (!ChatHook.Message_Color.StartsWith("[") || !ChatHook.Message_Color.EndsWith("]"))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message_Color entry in ServerToolsConfig.xml because of invalid 'Color' attribute: {0}", subChild.OuterXml));
                                        continue;
                                    }
                                }
                                break;
                            case "Motd":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Motd.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry in ServerToolsConfig.xml because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Show_On_Respawn"), out Motd.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Mute":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Mute.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Block_Commands"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry in ServerToolsConfig.xml because of missing 'Block_Commands' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Block_Commands"), out Mute.Block_Commands))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Block_Commands' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Mute_Vote":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out MuteVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Players_Online"), out MuteVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Votes_Needed"), out MuteVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "New_Player":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out NewPlayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Entry_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry in ServerToolsConfig.xml because of missing a Entry_Message attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Entry_Message"))
                                {
                                    NewPlayer.Entry_Message = line.GetAttribute("Entry_Message");
                                }
                                break;
                            case "New_Player_Extended":
                                if (!line.HasAttribute("Block_During_Bloodmoon"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Extended entry in ServerToolsConfig.xml because of missing 'Block_During_Bloodmoon' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Block_During_Bloodmoon"), out NewPlayer.Block_During_Bloodmoon))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Block_During_Bloodmoon' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "New_Player_Protection":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Protection entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out NewPlayerProtection.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Protection entry in ServerToolsConfig.xml because of missing a Level attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Level"), out NewPlayerProtection.Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player_Protection entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "New_Spawn_Tele":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out NewSpawnTele.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("New_Spawn_Tele_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry in ServerToolsConfig.xml because of missing a New_Spawn_Tele_Position attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("New_Spawn_Tele_Position"))
                                {
                                    NewSpawnTele.New_Spawn_Tele_Position = line.GetAttribute("New_Spawn_Tele_Position");
                                }
                                if (!line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry in ServerToolsConfig.xml because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Return"), out NewSpawnTele.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Night_Alert":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out NightAlert.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry in ServerToolsConfig.xml because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay"))
                                {
                                    NightAlert.Delay = line.GetAttribute("Delay");
                                }
                                if (NightAlert.IsEnabled)
                                {
                                    NightAlert.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("NightAlert"))
                                {
                                    EventSchedule.Remove("NightAlert");
                                }
                                break;
                            case "No_Vehicle_Pickup":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring No_Vehicle_Pickup entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PersistentOperations.No_Vehicle_Pickup))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring No_Vehicle_Pickup entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Normal_Player_Color_Prefix":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ChatHook.Normal_Player_Color_Prefix))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry in ServerToolsConfig.xml because of missing 'Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Prefix = line.GetAttribute("Prefix");
                                }
                                if (!line.HasAttribute("Name_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry in ServerToolsConfig.xml because of missing 'Name_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Name_Color = line.GetAttribute("Name_Color");
                                }
                                if (!line.HasAttribute("Prefix_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Color_Prefix entry in ServerToolsConfig.xml because of missing 'Prefix_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    ChatHook.Normal_Player_Prefix_Color = line.GetAttribute("Prefix_Color");
                                }
                                break;
                            case "Player_List":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PlayerList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Logs":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PlayerLogs.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry in ServerToolsConfig.xml because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Vehicle"), out PlayerLogs.Vehicle))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Interval"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry in ServerToolsConfig.xml because of missing 'Interval' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Interval"))
                                {
                                    PlayerLogs.Delay = line.GetAttribute("Interval");
                                }
                                if (PlayerLogs.IsEnabled)
                                {
                                    PlayerLogs.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("PlayerLogs"))
                                {
                                    EventSchedule.Remove("PlayerLogs");
                                }
                                break;
                            case "Player_Stats":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PlayerStats.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Speed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of missing 'Max_Speed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Speed"), out PlayerStats.Max_Speed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Speed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Health"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of missing 'Health' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Health"), out PlayerStats.Health))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Health' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Stamina"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of missing 'Stamina' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Stamina"), out PlayerStats.Stamina))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Stamina' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Jump_Strength"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of missing 'Jump_Strength' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!double.TryParse(line.GetAttribute("Jump_Strength"), out PlayerStats.Jump_Strength))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Jump_Strength' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Stats_Extended":
                                if (!line.HasAttribute("Height"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of missing 'Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!double.TryParse(line.GetAttribute("Height"), out PlayerStats.Height))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out PlayerStats.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Kick_Enabled"), out PlayerStats.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Ban_Enabled"), out PlayerStats.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stats_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "POI_Protection":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out POIProtection.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of missing 'Bed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bed"), out POIProtection.Bed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of missing 'Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Claim"), out POIProtection.Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring POI_Protection entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Poll":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Poll entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Poll.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Poll entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Prayer":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Prayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Prayer.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Prayer.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Prayer entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Private_Message":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Private_Message entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Whisper.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Private_Message entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Protected_Zones":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Protected_Zones entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ProtectedZones.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Protected_Zones entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Public_Waypoints":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Public_Waypoints entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Waypoints.Public_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Public_Waypoints entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "PvE_Violations":
                                if (!line.HasAttribute("Jail"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of missing 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Jail"), out PersistentOperations.Jail_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Jail' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Kill"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of missing 'Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Kill"), out PersistentOperations.Kill_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Kick"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of missing 'Kick' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Kick"), out PersistentOperations.Kick_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Kick' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Ban"), out PersistentOperations.Ban_Violation))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring PvE_Violations entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Real_World_Time":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out RealWorldTime.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay"))
                                {
                                    RealWorldTime.Delay = line.GetAttribute("Delay");
                                }
                                if (!line.HasAttribute("Time_Zone"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of missing 'Time_Zone' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Time_Zone"))
                                {
                                    RealWorldTime.Time_Zone = line.GetAttribute("Time_Zone");
                                }
                                if (!line.HasAttribute("Adjustment"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of missing 'Adjustment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Adjustment"), out RealWorldTime.Adjustment))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Adjustment' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (RealWorldTime.IsEnabled)
                                {
                                    RealWorldTime.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("RealWorldTime"))
                                {
                                    EventSchedule.Remove("RealWorldTime");
                                }
                                break;
                            case "Report":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Report.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Report.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Length"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of missing 'Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Length"), out Report.Length))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Length' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out Report.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Reserved_Slots":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ReservedSlots.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Session_Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of missing 'Session_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Session_Time"), out ReservedSlots.Session_Time))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Session_Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out ReservedSlots.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reduced_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of missing 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Reduced_Delay"), out ReservedSlots.Reduced_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bonus_Exp"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of missing 'Bonus_Exp' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bonus_Exp"), out ReservedSlots.Bonus_Exp))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bonus_Exp' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Restart_Vote":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out RestartVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Players_Online"), out RestartVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!int.TryParse(line.GetAttribute("Votes_Needed"), out RestartVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out RestartVote.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Scout_Player":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out ScoutPlayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out ScoutPlayer.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out ScoutPlayer.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Scout_Player entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Shop":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Shop.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Inside_Market"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of missing 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Inside_Market"), out Shop.Inside_Market))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Inside_Traders"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of missing 'Inside_Traders' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Inside_Traders"), out Shop.Inside_Traders))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Inside_Traders' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Shutdown":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Shutdown.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Countdown"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of missing 'Countdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Countdown"), out Shutdown.Countdown))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Countdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Time"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of missing 'Time' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Time"))
                                {
                                    Shutdown.Time = line.GetAttribute("Time");
                                }
                                if (!line.HasAttribute("Alert_On_Login"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of missing 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Alert_On_Login"), out Shutdown.Alert_On_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Alert_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of missing 'Alert_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Alert_Count"), out Shutdown.Alert_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Alert_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (Shutdown.IsEnabled)
                                {
                                    Shutdown.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("Shutdown"))
                                {
                                    EventSchedule.Remove("Shutdown");
                                }
                                break;
                            case "Shutdown_Extended":
                                if (!line.HasAttribute("UI_Lock"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown_Extended entry in ServerToolsConfig.xml because of missing 'UI_Lock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("UI_Lock"), out Shutdown.UI_Lock))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shutdown_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'UI_Lock' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Sleeper_Respawn":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Sleeper_Respawn entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out SleeperRespawn.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Sleeper_Respawn entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Spectator_Detector":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out PlayerChecks.SpectatorEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out PlayerChecks.Spectator_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Spectator_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Speed_Detector":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out SpeedDetector.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out SpeedDetector.Speed_Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Flags"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of missing 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Flags"), out SpeedDetector.Total_Flags))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Speed_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Flags' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Starting_Items":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Starting_Items entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out StartingItems.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Starting_Items entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Stuck":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Stuck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Stuck.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Suicide":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Suicide.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Suicide.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Suicide.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Suicide.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Suicide.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Suicide.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Tracking":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Tracking entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Track.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Tracking entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Travel":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Travel.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Travel.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Travel.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Travel.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Travel.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Vehicle_Recall":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out VehicleRecall.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Inside_Claim"), out VehicleRecall.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Distance"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of missing 'Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Distance"), out VehicleRecall.Distance))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Distance' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out VehicleRecall.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out VehicleRecall.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Vehicle_Recall_Extended":
                                if (!line.HasAttribute("Normal_Max"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall_Extended entry in ServerToolsConfig.xml because of missing 'Normal_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Normal_Max"), out VehicleRecall.Normal_Max))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Normal_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reserved_Max"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall_Extended entry in ServerToolsConfig.xml because of missing 'Reserved_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reserved_Max"), out VehicleRecall.Reserved_Max))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Recall_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reserved_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Voting":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out VoteReward.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Your_Voting_Site"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of missing 'Your_Voting_Site' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Your_Voting_Site"))
                                {
                                    VoteReward.Your_Voting_Site = line.GetAttribute("Your_Voting_Site");
                                }
                                if (!line.HasAttribute("API_Key"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of missing 'API_Key' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("API_Key"))
                                {
                                    VoteReward.API_Key = line.GetAttribute("API_Key");
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out VoteReward.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Voting_Extended":
                                if (!line.HasAttribute("Reward_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of missing 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reward_Count"), out VoteReward.Reward_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reward_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reward_Entity"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of missing 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Reward_Entity"), out VoteReward.Reward_Entity))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Entity_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of missing 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Entity_Id"), out VoteReward.Entity_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Weekly_Votes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of missing 'Weekly_Votes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Weekly_Votes"), out VoteReward.Weekly_Votes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Weekly_Votes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Wallet":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!PersistentOperations.No_Currency)
                                {
                                    if (!bool.TryParse(line.GetAttribute("Enable"), out Wallet.IsEnabled))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                        continue;
                                    }
                                }
                                if (!line.HasAttribute("PVP"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of missing 'PVP' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("PVP"), out Wallet.PVP))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of invalid (True/False) value for 'PVP' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Kill"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of missing 'Zombie_Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Zombie_Kill"), out Wallet.Zombie_Kill))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Player_Kill"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of missing 'Player_Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Player_Kill"), out Wallet.Player_Kill))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Player_Kill' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Bank_Transfers"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of missing 'Bank_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Bank_Transfers"), out Wallet.Bank_Transfers))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Bank_Transfers' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Wallet_Extended":
                                if (!line.HasAttribute("Session_Bonus"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet_Extended entry in ServerToolsConfig.xml because of missing 'Session_Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Session_Bonus"), out Wallet.Session_Bonus))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Session_Bonus' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Currency_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet_Extended entry in ServerToolsConfig.xml because of missing 'Currency_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Currency_Name"))
                                {
                                    Wallet.Currency_Name = line.GetAttribute("Currency_Name");
                                    continue;
                                }
                                break;
                            case "Watch_List":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watch_List entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out WatchList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watch_List entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watch_List entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out WatchList.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watch_List entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watch_List entry in ServerToolsConfig.xml because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Delay"))
                                {
                                    WatchList.Delay = line.GetAttribute("Delay");
                                }
                                if (WatchList.IsEnabled)
                                {
                                    WatchList.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("Watchlist"))
                                {
                                    EventSchedule.Remove("Watchlist");
                                }
                                break;
                            case "Waypoints":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Waypoints.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of missing 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Max_Waypoints"), out Waypoints.Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reserved_Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of missing 'Reserved_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reserved_Max_Waypoints"), out Waypoints.Reserved_Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reserved_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Command_Cost"), out Waypoints.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Delay_Between_Uses"), out Waypoints.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Waypoints_Extended":
                                if (!line.HasAttribute("Player_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of missing 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Player_Check"), out Waypoints.Player_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Player_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zombie_Check"), out Waypoints.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Vehicle"), out Waypoints.Vehicle))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Public_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of missing 'Public_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Public_Waypoints"), out Waypoints.Public_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints_Extended entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Public_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Web_API":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_API entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out WebAPI.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_API entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Port"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_API entry in ServerToolsConfig.xml because of missing 'Port' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Port"), out WebAPI.Port))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_API entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Port' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Web_Panel":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_Panel entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out WebPanel.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Web_Panel entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Workstation_Lock":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Workstation_Lock entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out WorkstationLock.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Workstation_Lock entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "World_Radius":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out WorldRadius.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Normal_Player"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of missing 'Normal_Player' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Normal_Player"), out WorldRadius.Normal_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Normal_Player' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reserved"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of missing 'Reserved' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Reserved"), out WorldRadius.Reserved))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Reserved' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(line.GetAttribute("Admin_Level"), out WorldRadius.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            //case "XRay_Detector":
                            //    if (!line.HasAttribute("Enable"))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring XRay_Detector entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    if (!bool.TryParse(line.GetAttribute("Enable"), out XRayDetector.IsEnabled))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring XRay_Detector entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    if (!line.HasAttribute("Admin_Level"))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring XRay_Detector entry in ServerToolsConfig.xml because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    if (!int.TryParse(line.GetAttribute("Admin_Level"), out XRayDetector.Admin_Level))
                            //    {
                            //        Log.Warning(string.Format("[SERVERTOOLS] Ignoring XRay_Detector entry in ServerToolsConfig.xml because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                            //        continue;
                            //    }
                            //    break;
                            case "Zones":
                                if (!line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Enable"), out Zones.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Zone_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of missing 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Zone_Message"), out Zones.Zone_Message))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!line.HasAttribute("Reminder_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of missing 'Reminder_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (line.HasAttribute("Reminder_Delay"))
                                {
                                    Zones.Reminder_Delay = line.GetAttribute("Reminder_Delay");
                                }
                                if (!line.HasAttribute("Set_Home"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of missing 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(line.GetAttribute("Set_Home"), out Zones.Set_Home))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry in ServerToolsConfig.xml because of invalid (True/False) value for 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (Zones.IsEnabled)
                                {
                                    Zones.SetDelay();
                                }
                                else if (EventSchedule.Schedule.ContainsKey("Zones"))
                                {
                                    EventSchedule.Remove("Zones");
                                }
                                break;
                        }
                    }
                }
            }
            if (Upgrade)
            {
                WriteXml();
                Config.UpgradeXml(xmlDoc.DocumentElement.ChildNodes[1].ChildNodes);
            }
        }

        public static void WriteXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(ConfigFilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Version>");
                sw.WriteLine(string.Format("        <Version Version=\"{0}\" />", Version.ToString()));
                sw.WriteLine("    </Version>");
                sw.WriteLine("    <Tools>");
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_Chat_Commands\" Enable=\"{0}\" />", AdminChat.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_List\" Enable=\"{0}\" Admin_Level=\"{1}\" Moderator_Level=\"{2}\" />", AdminList.IsEnabled, AdminList.Admin_Level, AdminList.Mod_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Minimum_Spawn_Radius=\"{2}\" Maximum_Spawn_Radius=\"{3}\" Animal_Ids=\"{4}\" />", AnimalTracking.IsEnabled, AnimalTracking.Delay_Between_Uses, AnimalTracking.Minimum_Spawn_Radius, AnimalTracking.Maximum_Spawn_Radius, AnimalTracking.Animal_Ids));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking_Extended\" Command_Cost=\"{0}\" />", AnimalTracking.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Auction\" Enable=\"{0}\" No_Admins=\"{1}\" Admin_Level=\"{2}\" Total_Items=\"{3}\" Tax=\"{4}\" />", Auction.IsEnabled, Auction.No_Admins, Auction.Admin_Level, Auction.Total_Items, Auction.Tax));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Backup\" Enable=\"{0}\" Delay_Between_Saves=\"{1}\" Destination=\"{2}\" Compression_Level=\"{3}\" Backup_Count=\"{4}\" />", AutoBackup.IsEnabled, AutoBackup.Delay, AutoBackup.Destination, AutoBackup.Compression_Level, AutoBackup.Backup_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Party_Invite\" Enable=\"{0}\" />", AutoPartyInvite.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Save_World\" Enable=\"{0}\" Delay_Between_Saves=\"{1}\" />", AutoSaveWorld.IsEnabled, AutoSaveWorld.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Bad_Word_Filter\" Enable=\"{0}\" Invalid_Name=\"{1}\" />", Badwords.IsEnabled, Badwords.Invalid_Name));
                sw.WriteLine(string.Format("        <Tool Name=\"Bank\" Enable=\"{0}\" Inside_Claim=\"{1}\" Deposit_Fee_Percent=\"{2}\" Player_Transfers=\"{3}\" />", Bank.IsEnabled, Bank.Inside_Claim, Bank.Deposit_Fee_Percent, Bank.Player_Transfers));
                sw.WriteLine(string.Format("        <Tool Name=\"Block_Logger\" Enable=\"{0}\" />", BlockLogger.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon\" Enable=\"{0}\" Delay=\"{1}\" Show_On_Respawn=\"{2}\" />", Bloodmoon.IsEnabled, Bloodmoon.Delay, Bloodmoon.Show_On_Respawn));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon_Warrior\" Enable=\"{0}\" Zombie_Kills=\"{1}\" Chance=\"{2}\" Reduce_Death_Count=\"{3}\" Reward_Count=\"{4}\" />", BloodmoonWarrior.IsEnabled, BloodmoonWarrior.Zombie_Kills, BloodmoonWarrior.Chance, BloodmoonWarrior.Reduce_Death_Count, BloodmoonWarrior.Reward_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Bot_Response\" Enable=\"{0}\" />", BotResponse.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Bounties\" Enable=\"{0}\" Minimum_Bounty=\"{1}\" Kill_Streak=\"{2}\" Bonus=\"{3}\" />", Bounties.IsEnabled, Bounties.Minimum_Bounty, Bounties.Kill_Streak, Bounties.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Break_Reminder\" Enable=\"{0}\" Break_Time=\"{1}\" Message=\"{2}\" />", BreakTime.IsEnabled, BreakTime.Delay, BreakTime.Message));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Color\" Enable=\"{0}\" Rotate=\"{1}\" Custom_Color=\"{2}\" />", ChatColor.IsEnabled, ChatColor.Rotate, ChatColor.Custom_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Log\" Enable=\"{0}\" />", ChatCommandLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response\" Server_Response_Name=\"{0}\" Main_Color=\"{1}\" Chat_Command_Prefix1=\"{2}\" Chat_Command_Prefix2=\"{3}\" />", Server_Response_Name, Chat_Response_Color, ChatHook.Chat_Command_Prefix1, ChatHook.Chat_Command_Prefix2));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response_Extended\" Friend_Chat_Color=\"{0}\" Party_Chat_Color=\"{1}\" Passthrough=\"{2}\" />", ChatHook.Friend_Chat_Color, ChatHook.Party_Chat_Color, ChatHook.Passthrough));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Flood_Protection\" Enable=\"{0}\" Max_Length=\"{1}\" Messages_Per_Min=\"{2}\" Wait_Time=\"{3}\" />", ChatHook.ChatFlood, ChatHook.Max_Length, ChatHook.Messages_Per_Min, ChatHook.Wait_Time));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Logger\" Enable=\"{0}\" />", ChatLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Clan_Manager\" Enable=\"{0}\" Max_Name_Length=\"{1}\" Private_Chat_Color=\"{2}\" />", ClanManager.IsEnabled, ClanManager.Max_Name_Length, ClanManager.Private_Chat_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Clean_Bin\" Enable=\"{0}\" Auction=\"{1}\" Bank=\"{2}\" Bounties=\"{3}\" Delays=\"{4}\" />", CleanBin.IsEnabled, CleanBin.Auction, CleanBin.Bank, CleanBin.Bounties, CleanBin.Delays));
                sw.WriteLine(string.Format("        <Tool Name=\"Clean_Bin_Extended1\" Homes=\"{0}\" Jail=\"{1}\" Lobby=\"{2}\" Market=\"{3}\" New_Spawn_Tele=\"{4}\" />", CleanBin.Homes, CleanBin.Jail, CleanBin.Lobby, CleanBin.Market, CleanBin.New_Spawn_Tele));
                sw.WriteLine(string.Format("        <Tool Name=\"Clean_Bin_Extended2\" Poll=\"{0}\" Protected_Spaces=\"{1}\" Vehicles=\"{2}\" Wallet=\"{3}\" Waypoints=\"{4}\" />", CleanBin.Poll, CleanBin.Protected_Spaces, CleanBin.Vehicles, CleanBin.Wallet, CleanBin.Waypoints));
                sw.WriteLine(string.Format("        <Tool Name=\"Console_Command_Log\" Enable=\"{0}\" />", ConsoleCommandLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Country_Ban\" Enable=\"{0}\" Countries_Not_Allowed=\"CN,IL\" />", CountryBan.IsEnabled, CountryBan.Countries_Not_Allowed));
                sw.WriteLine(string.Format("        <Tool Name=\"Custom_Commands\" Enable=\"{0}\" />", CustomCommands.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Damage_Detector\" Enable=\"{0}\" Entity_Damage_Limit=\"{1}\" Block_Damage_Limit=\"{2}\" Player_Damage_Limit=\"{3}\" Admin_Level=\"{4}\" />", DamageDetector.IsEnabled, DamageDetector.Entity_Damage_Limit, DamageDetector.Block_Damage_Limit, DamageDetector.Player_Damage_Limit, DamageDetector.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Day7\" Enable=\"{0}\" />", Day7.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Died\" Enable=\"{0}\" Time=\"{1}\" Delay_Between_Uses=\"{2}\" Command_Cost=\"{3}\" />", Died.IsEnabled, Died.Time, Died.Delay_Between_Uses, Died.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Discord_Bot\" Enable=\"{0}\" Webhook=\"{1}\" />", DiscordBot.IsEnabled, DiscordBot.Webhook));
                sw.WriteLine(string.Format("        <Tool Name=\"Discord_Bot_Extended\" Prefix=\"{0}\" Prefix_Color=\"{1}\" Name_Color=\"{2}\" Message_Color=\"{3}\" />", DiscordBot.Prefix, DiscordBot.Prefix_Color, DiscordBot.Name_Color, DiscordBot.Message_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Dropped_Bag_Protection\" Enable=\"{0}\" Friend_Access=\"{1}\" />", DroppedBagProtection.IsEnabled, DroppedBagProtection.Friend_Access));
                sw.WriteLine(string.Format("        <Tool Name=\"Dupe_Log\" Enable=\"{0}\" />", DupeLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup\" Enable=\"{0}\" Falling_Blocks=\"{1}\" Falling_Tree=\"{2}\" Entity_Underground=\"{3}\" Delete_Bicycles=\"{4}\" />", EntityCleanup.IsEnabled, EntityCleanup.BlockIsEnabled, EntityCleanup.FallingTreeEnabled, EntityCleanup.Underground, EntityCleanup.Bicycles));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup_Extended\" Delete_MiniBikes=\"{0}\" Delete_MotorBikes=\"{1}\" Delete_Jeeps=\"{2}\" Delete_Gyros=\"{3}\" />", EntityCleanup.MiniBikes, EntityCleanup.MotorBikes, EntityCleanup.Jeeps, EntityCleanup.Gyros));
                sw.WriteLine(string.Format("        <Tool Name=\"Exit_Command\" Enable=\"{0}\" All=\"{1}\" Belt=\"{2}\" Bag=\"{3}\" Equipment=\"{4}\" />", ExitCommand.IsEnabled, ExitCommand.All, ExitCommand.Belt, ExitCommand.Bag, ExitCommand.Equipment));
                sw.WriteLine(string.Format("        <Tool Name=\"Exit_Command_Extended\" Admin_Level=\"{0}\" Exit_Time=\"{1}\" />", ExitCommand.Admin_Level, ExitCommand.Exit_Time));
                sw.WriteLine(string.Format("        <Tool Name=\"Falling_Blocks_Remover\" Enable=\"{0}\" Log=\"{1}\" Max_Blocks=\"{2}\" />", FallingBlocks.IsEnabled, FallingBlocks.OutputLog, FallingBlocks.Max_Blocks));
                sw.WriteLine(string.Format("        <Tool Name=\"First_Claim_Block\" Enable=\"{0}\" />", FirstClaimBlock.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Flying_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" Flags=\"{2}\" />", FlyingDetector.IsEnabled, FlyingDetector.Flying_Admin_Level, FlyingDetector.Flag_Limit));
                sw.WriteLine(string.Format("        <Tool Name=\"FPS\" Enable=\"{0}\" Set_Target=\"{1}\" Low_FPS=\"{2}\" />", Fps.IsEnabled, Fps.Set_Target, Fps.Low_FPS));
                sw.WriteLine(string.Format("        <Tool Name=\"Friend_Teleport\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" Player_Check=\"{3}\" Zombie_Check=\"{4}\" />", FriendTeleport.IsEnabled, FriendTeleport.Delay_Between_Uses, FriendTeleport.Command_Cost, FriendTeleport.Player_Check, FriendTeleport.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Gamble\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", Gamble.IsEnabled, Gamble.Delay_Between_Uses, Gamble.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Gimme\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Zombies=\"{2}\" Zombie_Id=\"{3}\" Command_Cost=\"{4}\" />", Gimme.IsEnabled, Gimme.Delay_Between_Uses, Gimme.Zombies, Gimme.Zombie_Id, Gimme.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Godmode_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" />", PlayerChecks.GodEnabled, PlayerChecks.Godmode_Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Hardcore\" Enable=\"{0}\" Optional=\"{1}\" Max_Deaths=\"{2}\" Max_Extra_Lives=\"{3}\" Life_Price=\"{4}\" />", Hardcore.IsEnabled, Hardcore.Optional, Hardcore.Max_Deaths, Hardcore.Max_Extra_Lives, Hardcore.Life_Price));
                sw.WriteLine(string.Format("        <Tool Name=\"High_Ping_Kicker\" Enable=\"{0}\" Max_Ping=\"{1}\" Flags=\"{2}\" />", HighPingKicker.IsEnabled, HighPingKicker.Max_Ping, HighPingKicker.Flags));
                sw.WriteLine(string.Format("        <Tool Name=\"Homes\" Enable=\"{0}\" Max_Homes =\"{1}\" Reserved_Max_Homes=\"{2}\" Command_Cost =\"{3}\" Delay_Between_Uses=\"{4}\" />", Homes.IsEnabled, Homes.Max_Homes, Homes.Reserved_Max_Homes, Homes.Command_Cost, Homes.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Homes_Extended\" Player_Check =\"{0}\" Zombie_Check=\"{1}\" Vehicle=\"{2}\" />", Homes.Player_Check, Homes.Zombie_Check, Homes.Vehicle_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Hordes\" Enable=\"{0}\" />", Hordes.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Infinite_Ammo\" Enable=\"{0}\" />", InfiniteAmmo.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Info_Ticker\" Enable=\"{0}\" Delay=\"{1}\" Random=\"{2}\" />", InfoTicker.IsEnabled, InfoTicker.Delay, InfoTicker.Random));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Items\" Enable=\"{0}\" Ban=\"{1}\" Admin_Level=\"{2}\" Check_Storage=\"{3}\" />", InvalidItems.IsEnabled, InvalidItems.Ban_Player, InvalidItems.Admin_Level, InvalidItems.Check_Storage));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Item_Stack\" Enable=\"{0}\" />", InvalidItems.Invalid_Stack));
                sw.WriteLine(string.Format("        <Tool Name=\"Jail\" Enable=\"{0}\" Jail_Size=\"{1}\" Jail_Position=\"{2}\" Jail_Shock=\"{3}\" />", Jail.IsEnabled, Jail.Jail_Size, Jail.Jail_Position, Jail.Jail_Shock));
                sw.WriteLine(string.Format("        <Tool Name=\"Kick_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", KickVote.IsEnabled, KickVote.Players_Online, KickVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Kill_Notice\" Enable=\"{0}\" PvP=\"{1}\" Zombie_Kills=\"{2}\" Show_Level=\"{3}\" Show_Damage=\"{4}\" />", KillNotice.IsEnabled, KillNotice.PvP, KillNotice.Zombie_Kills, KillNotice.Show_Level, KillNotice.Show_Damage));
                sw.WriteLine(string.Format("        <Tool Name=\"Level_Up\" Enable=\"{0}\" Xml_Only=\"{1}\" />", LevelUp.IsEnabled, LevelUp.Xml_Only));
                sw.WriteLine(string.Format("        <Tool Name=\"Lobby\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Lobby_Size=\"{3}\" Lobby_Position=\"{4}\" />", Lobby.IsEnabled, Lobby.Return, Lobby.Delay_Between_Uses, Lobby.Lobby_Size, Lobby.Lobby_Position));
                sw.WriteLine(string.Format("        <Tool Name=\"Lobby_Extended\" Reserved_Only=\"{0}\" Command_Cost=\"{1}\" Player_Check=\"{2}\" Zombie_Check=\"{3}\" PvE=\"{4}\" />", Lobby.Reserved_Only, Lobby.Command_Cost, Lobby.Player_Check, Lobby.Zombie_Check, Lobby.PvE));
                sw.WriteLine(string.Format("        <Tool Name=\"Location\" Enable=\"{0}\" />", Loc.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Login_Notice\" Enable=\"{0}\" />", LoginNotice.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Logs\" Days_Before_Log_Delete=\"{0}\" />", LoadProcess.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Lottery\" Enable=\"{0}\" Bonus=\"{1}\" />", Lottery.IsEnabled, Lottery.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Market\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Market_Size=\"{3}\" Market_Position=\"{4}\" />", Market.IsEnabled, Market.Return, Market.Delay_Between_Uses, Market.Market_Size, Market.Market_Position));
                sw.WriteLine(string.Format("        <Tool Name=\"Market_Extended\" Reserved_Only=\"{0}\" Command_Cost=\"{1}\" Player_Check=\"{2}\" Zombie_Check=\"{3}\" PvE=\"{4}\" />", Market.Reserved_Only, Market.Command_Cost, Market.Player_Check, Market.Zombie_Check, Market.PvE));
                sw.WriteLine(string.Format("        <Tool Name=\"Message_Color\" Enable=\"{0}\" Color=\"{1}\" />", ChatHook.Message_Color_Enabled, ChatHook.Message_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Motd\" Enable=\"{0}\" Show_On_Respawn=\"{1}\" />", Motd.IsEnabled, Motd.Show_On_Respawn));
                sw.WriteLine(string.Format("        <Tool Name=\"Mute\" Enable=\"{0}\" Block_Commands=\"{1}\" />", Mute.IsEnabled, Mute.Block_Commands));
                sw.WriteLine(string.Format("        <Tool Name=\"Mute_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", MuteVote.IsEnabled, MuteVote.Players_Online, MuteVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Player\" Enable=\"{0}\" Entry_Message=\"{1}\" />", NewPlayer.IsEnabled, NewPlayer.Entry_Message));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Player_Extended\" Block_During_Bloodmoon=\"{0}\" />", NewPlayer.Block_During_Bloodmoon));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Player_Protection\" Enable=\"{0}\" Level=\"{1}\" />", NewPlayerProtection.IsEnabled, NewPlayerProtection.Level));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Spawn_Tele\" Enable=\"{0}\" New_Spawn_Tele_Position=\"{1}\" Return=\"{2}\" />", NewSpawnTele.IsEnabled, NewSpawnTele.New_Spawn_Tele_Position, NewSpawnTele.Return));
                sw.WriteLine(string.Format("        <Tool Name=\"Night_Alert\" Enable=\"{0}\" Delay=\"{1}\" />", NightAlert.IsEnabled, NightAlert.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"No_Vehicle_Pickup\" Enable=\"{0}\" />", PersistentOperations.No_Vehicle_Pickup));
                sw.WriteLine(string.Format("        <Tool Name=\"Normal_Player_Color_Prefix\" Enable=\"{0}\" Prefix=\"{1}\" Name_Color=\"{2}\" Prefix_Color=\"{3}\" />", ChatHook.Normal_Player_Color_Prefix, ChatHook.Normal_Player_Prefix, ChatHook.Normal_Player_Name_Color, ChatHook.Normal_Player_Prefix_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_List\" Enable=\"{0}\" />", PlayerList.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Logs\" Enable=\"{0}\" Vehicle=\"{1}\" Interval=\"{2}\" />", PlayerLogs.IsEnabled, PlayerLogs.Vehicle, PlayerLogs.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stats\" Enable=\"{0}\" Max_Speed=\"{1}\" Health=\"{2}\" Stamina=\"{3}\" Jump_Strength=\"{4}\" />", PlayerStats.IsEnabled, PlayerStats.Max_Speed, PlayerStats.Health, PlayerStats.Stamina, PlayerStats.Jump_Strength));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stats_Extended\" Height=\"{0}\" Admin_Level=\"{1}\" Kick_Enabled=\"{2}\" Ban_Enabled=\"{3}\" />", PlayerStats.Height, PlayerStats.Admin_Level, PlayerStats.Kick_Enabled, PlayerStats.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"POI_Protection\" Enable=\"{0}\" Bed=\"{1}\" Claim=\"{2}\" />", POIProtection.IsEnabled, POIProtection.Bed, POIProtection.Claim));
                sw.WriteLine(string.Format("        <Tool Name=\"Poll\" Enable=\"{0}\" />", Poll.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Prayer\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", Prayer.IsEnabled, Prayer.Delay_Between_Uses, Prayer.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Private_Message\" Enable=\"{0}\" />", Whisper.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Protected_Zones\" Enable=\"{0}\" />", ProtectedZones.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Public_Waypoints\" Enable=\"{0}\" />", Waypoints.Public_Waypoints));
                sw.WriteLine(string.Format("        <Tool Name=\"PvE_Violations\" Jail=\"{0}\" Kill=\"{1}\" Kick=\"{2}\" Ban=\"{3}\" />", PersistentOperations.Jail_Violation, PersistentOperations.Kill_Violation, PersistentOperations.Kick_Violation, PersistentOperations.Ban_Violation));
                sw.WriteLine(string.Format("        <Tool Name=\"Real_World_Time\" Enable=\"{0}\" Delay=\"{1}\" Time_Zone=\"{2}\" Adjustment=\"{3}\" />", RealWorldTime.IsEnabled, RealWorldTime.Delay, RealWorldTime.Time_Zone, RealWorldTime.Adjustment));
                sw.WriteLine(string.Format("        <Tool Name=\"Report\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Length=\"{2}\" Admin_Level=\"{3}\" />", Report.IsEnabled, Report.Delay, Report.Length, Report.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Reserved_Slots\" Enable=\"{0}\" Session_Time=\"{1}\" Admin_Level=\"{2}\" Reduced_Delay=\"{3}\" Bonus_Exp=\"{4}\" />", ReservedSlots.IsEnabled, ReservedSlots.Session_Time, ReservedSlots.Admin_Level, ReservedSlots.Reduced_Delay, ReservedSlots.Bonus_Exp));
                sw.WriteLine(string.Format("        <Tool Name=\"Restart_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" Admin_Level=\"{3}\" />", RestartVote.IsEnabled, RestartVote.Players_Online, RestartVote.Votes_Needed, RestartVote.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Scout_Player\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", ScoutPlayer.IsEnabled, ScoutPlayer.Delay_Between_Uses, ScoutPlayer.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Shop\" Enable=\"{0}\" Inside_Market=\"{1}\" Inside_Traders=\"{2}\" />", Shop.IsEnabled, Shop.Inside_Market, Shop.Inside_Traders));
                sw.WriteLine(string.Format("        <Tool Name=\"Shutdown\" Enable=\"{0}\" Countdown=\"{1}\" Time=\"{2}\" Alert_On_Login=\"{3}\" Alert_Count=\"{4}\" />", Shutdown.IsEnabled, Shutdown.Countdown, Shutdown.Time, Shutdown.Alert_On_Login, Shutdown.Alert_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Shutdown_Extended\" UI_Lock=\"{0}\" />", Shutdown.UI_Lock));
                sw.WriteLine(string.Format("        <Tool Name=\"Sleeper_Respawn\" Enable=\"{0}\" />", SleeperRespawn.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Spectator_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" />", PlayerChecks.SpectatorEnabled, PlayerChecks.Spectator_Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Speed_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" Flags=\"{2}\" />", SpeedDetector.IsEnabled, SpeedDetector.Speed_Admin_Level, SpeedDetector.Total_Flags));
                sw.WriteLine(string.Format("        <Tool Name=\"Starting_Items\" Enable=\"{0}\" />", StartingItems.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Stuck\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", Stuck.IsEnabled, Stuck.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Suicide\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Player_Check=\"{2}\" Zombie_Check=\"{3}\" />", Suicide.IsEnabled, Suicide.Delay_Between_Uses, Suicide.Player_Check, Suicide.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Tracking\" Enable=\"{0}\" />", Track.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Travel\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" Player_Check=\"{3}\" Zombie_Check=\"{4}\" />", Travel.IsEnabled, Travel.Delay_Between_Uses, Travel.Command_Cost, Travel.Player_Check, Travel.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Vehicle_Recall\" Enable=\"{0}\" Inside_Claim=\"{1}\" Distance=\"{2}\" Delay_Between_Uses=\"{3}\" Command_Cost=\"{4}\" />", VehicleRecall.IsEnabled, VehicleRecall.Inside_Claim, VehicleRecall.Distance, VehicleRecall.Delay_Between_Uses, VehicleRecall.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Vehicle_Recall_Extended\" Normal_Max=\"{0}\" Reserved_Max=\"{1}\" />", VehicleRecall.Normal_Max, VehicleRecall.Reserved_Max));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting\" Enable=\"{0}\" Your_Voting_Site=\"{1}\" API_Key=\"{2}\" Delay_Between_Uses=\"{3}\" />", VoteReward.IsEnabled, VoteReward.Your_Voting_Site, VoteReward.API_Key, VoteReward.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting_Extended\" Reward_Count=\"{0}\" Reward_Entity=\"{1}\" Entity_Id=\"{2}\" Weekly_Votes=\"{3}\" />", VoteReward.Reward_Count, VoteReward.Reward_Entity, VoteReward.Entity_Id, VoteReward.Weekly_Votes));
                sw.WriteLine(string.Format("        <Tool Name=\"Wallet\" Enable=\"{0}\" PVP=\"{1}\" Zombie_Kill=\"{2}\" Player_Kill=\"{3}\" Bank_Transfers=\"{4}\" />", Wallet.IsEnabled, Wallet.PVP, Wallet.Zombie_Kill, Wallet.Player_Kill, Wallet.Bank_Transfers));
                sw.WriteLine(string.Format("        <Tool Name=\"Wallet_Extended\" Session_Bonus=\"{0}\" Currency_Name=\"{1}\" />", Wallet.Session_Bonus, Wallet.Currency_Name));
                sw.WriteLine(string.Format("        <Tool Name=\"Watch_List\" Enable=\"{0}\" Admin_Level=\"{1}\" Delay=\"{2}\" />", WatchList.IsEnabled, WatchList.Admin_Level, WatchList.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Waypoints\" Enable=\"{0}\" Max_Waypoints =\"{1}\" Reserved_Max_Waypoints=\"{2}\" Command_Cost =\"{3}\" Delay_Between_Uses=\"{4}\" />", Waypoints.IsEnabled, Waypoints.Max_Waypoints, Waypoints.Reserved_Max_Waypoints, Waypoints.Command_Cost, Waypoints.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Waypoints_Extended\" Player_Check =\"{0}\" Zombie_Check=\"{1}\" Vehicle=\"{2}\" Public_Waypoints=\"{3}\" />", Waypoints.Player_Check, Waypoints.Zombie_Check, Waypoints.Vehicle, Waypoints.Public_Waypoints));
                sw.WriteLine(string.Format("        <Tool Name=\"Web_API\" Enable=\"{0}\" Port=\"{1}\" />", WebAPI.IsEnabled, WebAPI.Port));
                sw.WriteLine(string.Format("        <Tool Name=\"Web_Panel\" Enable=\"{0}\" />", WebPanel.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Workstation_Lock\" Enable=\"{0}\" />", WorkstationLock.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"World_Radius\" Enable=\"{0}\" Normal_Player=\"{1}\" Reserved=\"{2}\" Admin_Level=\"{3}\" />", WorldRadius.IsEnabled, WorldRadius.Normal_Player, WorldRadius.Reserved, WorldRadius.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"XRay_Detector\" Enable=\"{0}\" Admin_Level=\"{1}\" />", XRayDetector.IsEnabled, XRayDetector.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Zones\" Enable=\"{0}\" Zone_Message=\"{1}\" Reminder_Delay=\"{2}\" Set_Home=\"{3}\"  />", Zones.IsEnabled, Zones.Zone_Message, Zones.Reminder_Delay, Zones.Set_Home));
                sw.WriteLine("    </Tools>");
                sw.WriteLine("</ServerTools>");
                sw.Flush();
                sw.Close();
                sw.Dispose();
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

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            LoadXml();
            Mods.Load();
            ActiveTools.Exec(false);
        }

        public static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                if (File.Exists(ConfigFilePath))
                {
                    XmlDocument xml = new XmlDocument();
                    try
                    {
                        xml.Load(ConfigFilePath);
                    }
                    catch (XmlException e)
                    {
                        Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", ConfigFilePath, e.Message));
                        return;
                    }
                    XmlNodeList childNodes = xml.DocumentElement.ChildNodes[1].ChildNodes;
                    if (childNodes != null)
                    {
                        for (int i = 0; i < childNodes.Count; i++)
                        {
                            if (childNodes[i].NodeType != XmlNodeType.Comment)
                            {
                                XmlElement line = (XmlElement)childNodes[i];
                                if (line.HasAttributes && line.Name == "Tool")
                                {
                                    for (int j = 0; j < _oldChildNodes.Count; j++)
                                    {
                                        if (_oldChildNodes[j].NodeType != XmlNodeType.Comment)
                                        {
                                            XmlElement oldLine = (XmlElement)_oldChildNodes[j];
                                            if (oldLine.HasAttributes && oldLine.Name == "Tool" && line.Attributes[0].Value == oldLine.Attributes[0].Value)
                                            {
                                                for (int k = 1; k < line.Attributes.Count; k++)
                                                {
                                                    XmlAttribute newAttribute = line.Attributes[k];
                                                    for (int l = 1; l < oldLine.Attributes.Count; l++)
                                                    {
                                                        XmlAttribute oldAttribute = oldLine.Attributes[l];
                                                        if (newAttribute.Name == oldAttribute.Name)
                                                        {
                                                            newAttribute.Value = oldAttribute.Value;
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    xml.Save(ConfigFilePath);
                    Log.Out("[SERVERTOOLS] Completed updating ServerToolsConfig.xml");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Config.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            Upgrade = false;
            LoadXml();
        }
    }
}