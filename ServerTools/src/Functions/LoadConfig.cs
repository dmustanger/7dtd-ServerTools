using System.IO;
using System.Xml;

namespace ServerTools
{
    public class LoadConfig
    {
        private const string configFile = "ServerToolsConfig.xml";
        private static string configFilePath = string.Format("{0}/{1}", API.ConfigPath, configFile);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, configFile);
        public const double version = 12.9;
        public static string Server_Response_Name = "[FFCC00]ServerTools";
        public static string Chat_Response_Color = "[00FF00]";

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        private static void LoadXml()
        {
            Log.Out("---------------------------------------------------------------");
            Log.Out("[SERVERTOOLS] Verifying configuration file & Saving new entries");
            Log.Out("---------------------------------------------------------------");
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Version entry because of missing 'Version' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        double _oldversion;
                        if (!double.TryParse(_line.GetAttribute("Version"), out _oldversion))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Version entry because of invalid (non-numeric) value for 'Version' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (_oldversion != version)
                        {
                            WriteXml();
                            return;
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
                            case "DataBase":
                                if (!_line.HasAttribute("EnableMySql"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'EnableMySql' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("EnableMySql"), out SQL.IsMySql))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of invalid (true/false) value for 'EnableMySql' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("ServerHost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'ServerHost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Port"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'Port' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Port"), out MySqlDatabase.Port))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of invalid (non-numeric) value for 'Port' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("DatabaseName"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'DatabaseName' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("UserName"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'UserName' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Password"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring DataBase entry because of missing 'Password' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("ServerHost"))
                                {
                                    MySqlDatabase.Server = _line.GetAttribute("ServerHost");
                                }
                                if (_line.HasAttribute("DatabaseName"))
                                {
                                    MySqlDatabase.Database = _line.GetAttribute("DatabaseName");
                                }
                                if (_line.HasAttribute("UserName"))
                                {
                                    MySqlDatabase.UserName = _line.GetAttribute("UserName");
                                }
                                if (_line.HasAttribute("Password"))
                                {
                                    MySqlDatabase.Password = _line.GetAttribute("Password");
                                }
                                break;
                            case "Admin_Chat_Commands":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Chat_Commands entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AdminChat.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Chat_Commands entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Admin_List":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AdminList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out AdminList.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Moderator_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of missing 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Moderator_Level"), out AdminList.Mod_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_List entry because of invalid (non-numeric) value for 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Animal_Tracking":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Animals.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Always_Show_Response"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Always_Show_Response' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Always_Show_Response"), out Animals.Always_Show_Response))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (true/false) value for 'Always_Show_Response' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Animals.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Announce_Invalid_Item_Stack":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InventoryCheck.Anounce_Invalid_Stack))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Stack entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auction":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AuctionBox.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out AuctionBox.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Admins"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Admins"), out AuctionBox.No_Admins))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (true/false) value for 'No_Admins' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out AuctionBox.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auction entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Time_Between_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of missing 'Time_Between_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Time_Between_Saves"), out AutoBackup.Time_Between_Saves))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Backup entry because of invalid (non-numeric) value for 'Time_Between_Saves' attribute: {0}", subChild.OuterXml));
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
                                break;
                            case "Auto_Save_World":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AutoSaveWorld.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_World_Saves"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of missing 'Delay_Between_World_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_World_Saves"), out Timers.Delay_Between_World_Saves))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Save_World entry because of invalid (non-numeric) value for 'Delay_Between_World_Saves' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Auto_Shutdown":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out AutoShutdown.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Countdown_Timer"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of missing 'Countdown_Timer' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Countdown_Timer"), out AutoShutdown.Countdown_Timer))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of invalid (non-numeric) value for 'Countdown_Timer' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Time_Before_Shutdown"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of missing 'Time_Before_Shutdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Time_Before_Shutdown"), out Timers.Shutdown_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of invalid (non-numeric) value for 'Time_Before_Shutdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_On_Login"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of missing 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Alert_On_Login"), out AutoShutdown.Alert_On_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Auto_Shutdown entry because of invalid (true/false) value for 'Alert_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Days_Until_Horde"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoShutdown entry because of missing 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Until_Horde"), out AutoShutdown.Days_Until_Horde))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoShutdown entry because of invalid (non-numeric) value for 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_During_Countdown"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoShutdown entry because of missing 'Kick_During_Countdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_During_Countdown"), out AutoShutdown.Kick_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring AutoShutdown entry because of invalid (true/false) value for 'Kick_During_Countdown' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Invalid_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of missing 'Invalid_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Invalid_Name"), out Badwords.Invalid_Name))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of invalid (true/false) value for 'Invalid_Name' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Claim"), out Bank.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (true/false) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Limit"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Limit"), out Bank.Limit))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (non-numeric) value for 'Limit' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Deposit_Fee"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of missing 'Deposit_Fee' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Deposit_Fee"), out Bank.Deposit_Fee))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bank entry because of invalid (non-numeric) value for 'Deposit_Fee' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Block_Cleanup":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Cleanup entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out EntityCleanup.BlockIsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Block_Cleanup entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_On_Login"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Show_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_On_Login"), out Bloodmoon.Show_On_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (true/false) value for 'Show_On_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_On_Respawn"), out Bloodmoon.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (true/false) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Auto_Show"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Auto_Show' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Auto_Show"), out Bloodmoon.Auto_Show))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (true/false) value for 'Auto_Show' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Auto_Show_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Auto_Show_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Auto_Show_Delay"), out Timers.Auto_Show_Bloodmoon_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (non-numeric) value for 'Auto_Show_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Days_Until_Horde"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Until_Horde"), out Bloodmoon.Days_Until_Horde))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (non-numeric) value for 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bounties entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                            case "Chat_Color_Prefix":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color_Prefix entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatColorPrefix.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Color_Prefix entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Mute_Commands"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Mute_Commands' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Mute_Commands"), out MutePlayer.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of invalid (true/false) value for 'Mute_Commands' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Player_Name_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response_Extended entry because of missing 'Player_Name_Color' attribute: {0}", subChild.OuterXml));
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
                                if (_line.HasAttribute("Player_Name_Color"))
                                {
                                    ChatHook.Player_Name_Color = _line.GetAttribute("Player_Name_Color");
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Flood_Protection entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                break;
                            case "Chat_Logger":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatLog.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Logger entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                            case "Country_Ban":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out CountryBan.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Country_Ban entry because of missing 'Countries_Not_Allowed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                CountryBan.BannedCountries.Clear();
                                if (_line.HasAttribute("Countries_Not_Allowed"))
                                {
                                    string[] _countries = _line.GetAttribute("Countries_Not_Allowed").Split(',');
                                    foreach (string _country in _countries)
                                    {
                                        if (!CountryBan.BannedCountries.Contains(_country))
                                        {
                                            CountryBan.BannedCountries.Add(_country);
                                        }
                                    }
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Family_Share"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Family_Share' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Family_Share"), out CredentialCheck.Family_Share))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (true/false) value for 'No_Family_Share' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Bad_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Bad_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Bad_Id"), out CredentialCheck.Bad_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (true/false) value for 'No_Bad_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("No_Internal"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'No_Internal' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("No_Internal"), out CredentialCheck.No_Internal))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (true/false) value for 'No_Internal' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom_Commands entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Days_Until_Horde"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry because of missing 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Until_Horde"), out Day7.Days_Until_Horde))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Day7 entry because of invalid (non-numeric) value for 'Days_Until_Horde' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Dupe_Log entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Falling_Blocks"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Falling_Blocks"), out EntityCleanup.BlockIsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (true/false) value for 'Falling_Blocks' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Falling_Tree"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Falling_Tree"), out EntityCleanup.FallingTreeEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (true/false) value for 'Falling_Tree' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Underground"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Entity_Underground"), out EntityCleanup.Underground))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (true/false) value for 'Entity_Underground' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delete_Bikes"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of missing 'Delete_Bikes' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Delete_Bikes"), out EntityCleanup.Bikes))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Cleanup entry because of invalid (true/false) value for 'Delete_Bikes' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring First_Claim_Block entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Flight_Check":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out FlightCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out FlightCheck.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Ping"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Ping"), out FlightCheck.Max_Ping))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (non-numeric) value for 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Height"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Max_Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Height"), out FlightCheck.Max_Height))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (non-numeric) value for 'Max_Height' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kill_Enabled"), out FlightCheck.Kill_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Announce"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Announce' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Announce"), out FlightCheck.Announce))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Announce' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jail_Enabled"), out FlightCheck.Jail_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out FlightCheck.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out FlightCheck.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (true/false) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring FPS entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Friend_Teleport entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Always_Show_Response"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Always_Show_Response' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Always_Show_Response"), out Gimme.Always_Show_Response))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (true/false) value for 'Always_Show_Response' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombies"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing 'Zombies' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombies"), out Gimme.Zombies))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (true/false) value for 'Zombies' attribute: {0}", subChild.OuterXml));
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
                            case "Hardcore":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Hardcore.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hardcore entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                break;
                            case "Hatch_Elevator_Detector":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hatch_Elevator_Detector entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out HatchElevator.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hatch_Elevator_Detector entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Samples_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of missing 'Samples_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Samples_Needed"), out HighPingKicker.Samples_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring High_Ping_Kicker entry because of invalid (non-numeric) value for 'Samples_Needed' attribute: {0}", subChild.OuterXml));
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
                            case "Hordes":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Hordes.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Hordes entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Info_Ticker":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                bool _old = InfoTicker.IsEnabled;
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InfoTicker.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Messages"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of missing 'Delay_Between_Messages' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Messages"), out Timers.Infoticker_Delay))
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Info_Ticker entry because of invalid (true/false) value for 'Random' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Invalid_Item_Kicker":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out InventoryCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of missing 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban"), out InventoryCheck.Ban_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of invalid (true/false) value for 'Ban' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out InventoryCheck.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invalid_Item_Kicker entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Jail entry because of invalid (true/false) value for 'Jail_Shock' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kick_Vote entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Kill_Notice entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Lobby":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out LobbyChat.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Return"), out LobbyChat.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (true/false) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out LobbyChat.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Lobby_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Lobby_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Lobby_Size"), out LobbyChat.Lobby_Size))
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
                                    SetLobby.Lobby_Position = _line.GetAttribute("Lobby_Position");
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out LobbyChat.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out LobbyChat.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out LobbyChat.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lobby entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Location entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Logs entry because of invalid (true/false) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Lottery entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out MarketChat.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Return"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Return"), out MarketChat.Return))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (true/false) value for 'Return' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out MarketChat.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Market_Size"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Market_Size"), out MarketChat.Market_Size))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Market_Size' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Market_Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Market_Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Market_Position"))
                                {
                                    SetMarket.Market_Position = _line.GetAttribute("Market_Position");
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out MarketChat.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out MarketChat.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out MarketChat.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Show_On_Respawn"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of missing 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Show_On_Respawn"), out Motd.Show_On_Respawn))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of invalid (true/false) value for 'Show_On_Respawn' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Mute_Vote entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out MuteVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out MuteVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Player entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring New_Spawn_Tele entry because of invalid (true/false) value for 'Return' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out Timers.Night_Time_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Alert entry because of invalid (non-numeric) value for 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Night_Vote":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out NightVote.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Players_Online"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of missing 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Players_Online"), out NightVote.Players_Online))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of invalid (non-numeric) value for 'Players_Online' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Votes_Needed"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of missing 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Votes_Needed"), out NightVote.Votes_Needed))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Night_Vote entry because of invalid (non-numeric) value for 'Votes_Needed' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Normal_Player_Name_Coloring":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Name_Coloring entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Normal_Player_Name_Coloring))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Name_Coloring entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Normal_Player_Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Name_Coloring entry because of missing 'Normal_Player_Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Normal_Player_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Normal_Player_Name_Coloring entry because of missing 'Normal_Player_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (_line.HasAttribute("Normal_Player_Prefix"))
                                {
                                    ChatHook.Normal_Player_Prefix = _line.GetAttribute("Normal_Player_Prefix");
                                }
                                if (_line.HasAttribute("Normal_Player_Color"))
                                {
                                    ChatHook.Normal_Player_Color = _line.GetAttribute("Normal_Player_Color");
                                }
                                break;
                            case "Player_List":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerList.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_List entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Interval"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Interval' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Interval"), out Timers.Player_Log_Interval))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (non-numeric) value for 'Interval' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Position"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Position"), out PlayerLogs.Position))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (true/false) value for 'Position' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inventory"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Inventory' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inventory"), out PlayerLogs.Inventory))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (true/false) value for 'Inventory' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Extra"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Extra' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Extra"), out PlayerLogs.P_Data))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (true/false) value for 'Extra' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Player_Stat_Check":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out PlayerStatCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out PlayerStatCheck.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out PlayerStatCheck.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of invalid (true/false) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out PlayerStatCheck.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Stat_Check entry because of invalid (true/false) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Private_Message entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Real_World_Time entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out Timers.Real_Time_Delay))
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Report entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reserved_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Reserved_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reserved_Check"), out ReservedSlots.Reserved_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (true/false) value for 'Reserved_Check' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Admin_Slots"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Admin_Slots' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Slots"), out ReservedSlots.Admin_Slots))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (non-numeric) value for 'Admin_Slots' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reduced_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of missing 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reduced_Delay"), out ReservedSlots.Reduced_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Reserved_Slots entry because of invalid (true/false) value for 'Reduced_Delay' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                            case "Session":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Session entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Session.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Session entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Set_Home":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out TeleportHome.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Home2_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Set_Home2_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Set_Home2_Enabled"), out TeleportHome.Set_Home2_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (true/false) value for 'Set_Home2_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Home2_Donor_Only"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Set_Home2_Donor_Only' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Set_Home2_Donor_Only"), out TeleportHome.Set_Home2_Donor_Only))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out TeleportHome.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out TeleportHome.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out TeleportHome.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out TeleportHome.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Vehicle"), out TeleportHome.Vehicle))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Set_Home entry because of invalid (non-numeric) value for 'Vehicle' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Market"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Market"), out Shop.Inside_Market))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (true/false) value for 'Inside_Market' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Traders"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Inside_Traders' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Traders"), out Shop.Inside_Traders))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (true/false) value for 'Inside_Traders' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Starting_Items entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Stopserver":
                                if (!_line.HasAttribute("Ten_Second_Countdown"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of missing 'Ten_Second_Countdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ten_Second_Countdown"), out StopServer.Ten_Second_Countdown))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of invalid (true/false) value for 'Ten_Second_Countdown' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_30_Seconds"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of missing 'Kick_30_Seconds' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_30_Seconds"), out StopServer.Kick_30_Seconds))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of invalid (true/false) value for 'Kick_30_Seconds' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_Count"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of missing 'Alert_Count' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Alert_Count"), out StopServer.Alert_Count))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of invalid (true/false) value for 'Alert_Count' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stuck entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
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
                            case "Travel":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Travel.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of invalid (true/false) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
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
                            case "Underground_Check":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out UndergroundCheck.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out UndergroundCheck.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Ping"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Ping"), out UndergroundCheck.Max_Ping))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring UndergroundCheck entry because of invalid (non-numeric) value for 'Max_Ping' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kill_Enabled"), out UndergroundCheck.Kill_Player))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Announce"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Announce' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Announce"), out UndergroundCheck.Announce))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Announce' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jail_Enabled"), out UndergroundCheck.Jail_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out UndergroundCheck.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out UndergroundCheck.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Underground_Check entry because of invalid (true/false) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Bike"), out VehicleTeleport.Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Mini_Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Mini_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Mini_Bike"), out VehicleTeleport.Mini_Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Mini_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Motor_Bike"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Motor_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Motor_Bike"), out VehicleTeleport.Motor_Bike))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Motor_Bike' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jeep"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Jeep' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jeep"), out VehicleTeleport.Jeep))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Jeep' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Gyro"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Gyro' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Gyro"), out VehicleTeleport.Gyro))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Gyro' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Inside_Claim"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of missing 'Inside_Claim' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Inside_Claim"), out VehicleTeleport.Inside_Claim))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vehicle_Teleport entry because of invalid (true/false) value for 'Inside_Claim' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting_Extended entry because of invalid (true/false) value for 'Reward_Entity' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Lose_On_Death"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of missing 'Lose_On_Death' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Lose_On_Death"), out Wallet.Lose_On_Death))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Wallet entry because of invalid (true/false) value for 'Lose_On_Death' attribute: {0}", subChild.OuterXml));
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!int.TryParse(_line.GetAttribute("Alert_Delay"), out Timers.Alert_Delay))
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
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Waypoint.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Waypoints"), out Waypoint.Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Max_Waypoints"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Donator_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator_Max_Waypoints"), out Waypoint.Donator_Max_Waypoints))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Donator_Max_Waypoints' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Command_Cost"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Command_Cost"), out Waypoint.Command_Cost))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Command_Cost' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Waypoint.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("PvP_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("PvP_Check"), out Waypoint.PvP_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'PvP_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zombie_Check"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zombie_Check"), out Waypoint.Zombie_Check))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of invalid (non-numeric) value for 'Zombie_Check' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Vehicle"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Waypoints entry because of missing 'Vehicle' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Vehicle"), out Waypoint.Vehicle))
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
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Weather_Vote entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                            case "World_Radius":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out WorldRadius.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Donator"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of missing 'Donator' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator"), out WorldRadius.Donator))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring World_Radius entry because of invalid (non-numeric) value for 'Donator' attribute: {0}", subChild.OuterXml));
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
                            case "Zone":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Zones.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kill_Enabled"), out Zones.Kill_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jail_Enabled"), out Zones.Jail_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out Zones.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out Zones.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zone_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zone_Message"), out Zones.Zone_Message))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Reminder_Notice_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Reminder_Notice_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Reminder_Notice_Delay"), out Zones.Reminder_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (non-numeric) value for 'Reminder_Notice_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Home"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of missing 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Set_Home"), out Zones.Set_Home))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone entry because of invalid (true/false) value for 'Set_Home' attribute: {0}", subChild.OuterXml));
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
            Log.Out("[SERVERTOOLS] Test 5");
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(configFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Version>");
                sw.WriteLine(string.Format("        <Version Version=\"{0}\" />", version.ToString()));
                sw.WriteLine("    </Version>");
                sw.WriteLine("    <Tools>");
                sw.WriteLine(string.Format("        <Tool Name=\"DataBase\" EnableMySql=\"{0}\" ServerHost=\"{1}\" Port=\"{2}\" DatabaseName=\"{3}\" UserName=\"{4}\" Password=\"{5}\" />", SQL.IsMySql, MySqlDatabase.Server, MySqlDatabase.Port, MySqlDatabase.Database, MySqlDatabase.UserName, MySqlDatabase.Password));
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_Chat_Commands\" Enable=\"{0}\" />", AdminChat.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_List\" Enable=\"{0}\" Admin_Level=\"{1}\" Moderator_Level=\"{2}\" />", AdminList.IsEnabled, AdminList.Admin_Level, AdminList.Mod_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking\" Enable=\"{0}\" Always_Show_Response=\"{1}\" Delay_Between_Uses=\"{2}\" Minimum_Spawn_Radius=\"{3}\" Maximum_Spawn_Radius=\"{4}\" Entity_Id=\"{5}\" Command_Cost=\"{6}\" />", Animals.IsEnabled, Animals.Always_Show_Response, Animals.Delay_Between_Uses, Animals.Minimum_Spawn_Radius, Animals.Maximum_Spawn_Radius, Animals.Animal_List, Animals.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Announce_Invalid_Item_Stack\" Enable=\"{0}\" />", InventoryCheck.Anounce_Invalid_Stack));
                sw.WriteLine(string.Format("        <Tool Name=\"Auction\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" No_Admins=\"{2}\" Admin_Level=\"{3}\" />", AuctionBox.IsEnabled, AuctionBox.Delay_Between_Uses, AuctionBox.No_Admins, AuctionBox.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Backup\" Enable=\"{0}\" Time_Between_Saves=\"{1}\" Destination=\"{2}\" />", AutoBackup.IsEnabled, AutoBackup.Time_Between_Saves, AutoBackup.Destination));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Save_World\" Enable=\"{0}\" Delay_Between_World_Saves=\"{1}\" />", AutoSaveWorld.IsEnabled, Timers.Delay_Between_World_Saves));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Shutdown\" Enable=\"{0}\" Countdown_Timer=\"{1}\" Time_Before_Shutdown=\"{2}\" Alert_On_Login=\"{3}\" Days_Until_Horde=\"{4}\" Kick_During_Countdown=\"{5}\" />", AutoShutdown.IsEnabled, AutoShutdown.Countdown_Timer, Timers.Shutdown_Delay, AutoShutdown.Alert_On_Login, AutoShutdown.Days_Until_Horde, AutoShutdown.Kick_Login));
                sw.WriteLine(string.Format("        <Tool Name=\"Bad_Word_Filter\" Enable=\"{0}\" Invalid_Name=\"{1}\" />", Badwords.IsEnabled, Badwords.Invalid_Name));
                sw.WriteLine(string.Format("        <Tool Name=\"Bank\" Enable=\"{0}\" Inside_Claim=\"{1}\" Ingame_Coin=\"{2}\" Limit=\"{3}\" Deposit_Fee=\"{4}\" />", Bank.IsEnabled, Bank.Inside_Claim, Bank.Ingame_Coin, Bank.Limit, Bank.Deposit_Fee));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon\" Enable=\"{0}\" Show_On_Login=\"{1}\" Show_On_Respawn=\"{2}\" Auto_Show=\"{3}\" Auto_Show_Delay=\"{4}\" Days_Until_Horde=\"{5}\" />", Bloodmoon.IsEnabled, Bloodmoon.Show_On_Login, Bloodmoon.Show_On_Respawn, Bloodmoon.Auto_Show, Timers.Auto_Show_Bloodmoon_Delay, Bloodmoon.Days_Until_Horde));
                sw.WriteLine(string.Format("        <Tool Name=\"Bounties\" Enable=\"{0}\" Minimum_Bounty=\"{1}\" Kill_Streak=\"{2}\" Bonus=\"{3}\" />", Bounties.IsEnabled, Bounties.Minimum_Bounty, Bounties.Kill_Streak, Bounties.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Color_Prefix\" Enable=\"{0}\" />", ChatColorPrefix.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response\" Server_Response_Name=\"{0}\" Main_Color=\"{1}\" Chat_Command_Private=\"{2}\" Chat_Command_Public=\"{3}\" Mute_Commands=\"{4}\" />", Server_Response_Name, Chat_Response_Color, ChatHook.Command_Private, ChatHook.Command_Public, MutePlayer.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response_Extended\" Friend_Chat_Color=\"{0}\" Party_Chat_Color=\"{1}\" Player_Name_Color=\"{2}\" />", ChatHook.Friend_Chat_Color, ChatHook.Party_Chat_Color, ChatHook.Player_Name_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Flood_Protection\" Enable=\"{0}\" Max_Length=\"{1}\" Messages_Per_Min=\"{2}\" />", ChatHook.ChatFlood, ChatHook.Max_Length, ChatHook.Messages_Per_Min));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Logger\" Enable=\"{0}\" />", ChatLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Clan_Manager\" Enable=\"{0}\" Private_Chat_Color=\"{1}\" />", ClanManager.IsEnabled, ClanManager.Private_Chat_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Country_Ban\" Enable=\"{0}\" Countries_Not_Allowed=\"CN,IL\" />", CountryBan.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Credentials\" Enable=\"{0}\" No_Family_Share=\"{1}\" No_Bad_Id=\"{2}\" No_Internal=\"{3}\" Admin_Level=\"{4}\" />", CredentialCheck.IsEnabled, CredentialCheck.Family_Share, CredentialCheck.Bad_Id, CredentialCheck.No_Internal, CredentialCheck.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Custom_Commands\" Enable=\"{0}\" />", CustomCommands.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Day7\" Enable=\"{0}\" Days_Until_Horde=\"{1}\" />", Day7.IsEnabled, Day7.Days_Until_Horde));
                sw.WriteLine(string.Format("        <Tool Name=\"Death_Spot\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" />", DeathSpot.IsEnabled, DeathSpot.Delay_Between_Uses, DeathSpot.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Dupe_Log\" Enable=\"{0}\" />", DupeLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup\" Enable=\"{0}\" Falling_Blocks=\"{1}\" Falling_Tree=\"{2}\" Entity_Underground=\"{3}\" Delete_Bikes=\"{4}\" />", EntityCleanup.IsEnabled, EntityCleanup.BlockIsEnabled, EntityCleanup.FallingTreeEnabled, EntityCleanup.Underground, EntityCleanup.Bikes));
                sw.WriteLine(string.Format("        <Tool Name=\"First_Claim_Block\" Enable=\"{0}\" />", FirstClaimBlock.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Flight_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Max_Ping=\"{2}\" Max_Height=\"{3}\" Kill_Enabled=\"{4}\" Announce=\"{5}\" Jail_Enabled=\"{6}\" Kick_Enabled=\"{7}\" Ban_Enabled=\"{8}\" />", FlightCheck.IsEnabled, FlightCheck.Admin_Level, FlightCheck.Max_Ping, FlightCheck.Max_Height, FlightCheck.Kill_Player, FlightCheck.Announce, FlightCheck.Jail_Enabled, FlightCheck.Kick_Enabled, FlightCheck.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"FPS\" Enable=\"{0}\" Set_Target=\"{1}\" />", Fps.IsEnabled, Fps.Set_Target));
                sw.WriteLine(string.Format("        <Tool Name=\"Friend_Teleport\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" PvP_Check=\"{3}\" Zombie_Check=\"{4}\" />", FriendTeleport.IsEnabled, FriendTeleport.Delay_Between_Uses, FriendTeleport.Command_Cost, FriendTeleport.PvP_Check, FriendTeleport.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Gimme\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Always_Show_Response=\"{2}\" Zombies=\"{3}\" Command_Cost=\"{4}\" />", Gimme.IsEnabled, Gimme.Delay_Between_Uses, Gimme.Always_Show_Response, Gimme.Zombies, Gimme.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Hardcore\" Enable=\"{0}\" Max_Deaths=\"{1}\" />", Hardcore.IsEnabled, Hardcore.Max_Deaths));
                sw.WriteLine(string.Format("        <Tool Name=\"Hatch_Elevator_Detector\" Enable=\"{0}\" />", HatchElevator.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"High_Ping_Kicker\" Enable=\"{0}\" Max_Ping=\"{1}\" Samples_Needed=\"{2}\" />", HighPingKicker.IsEnabled, HighPingKicker.Max_Ping, HighPingKicker.Samples_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Hordes\" Enable=\"{0}\" />", Hordes.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Info_Ticker\" Enable=\"{0}\" Delay_Between_Messages=\"{1}\" Random=\"{2}\" />", InfoTicker.IsEnabled, Timers.Infoticker_Delay, InfoTicker.Random));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Item_Kicker\" Enable=\"{0}\" Ban=\"{1}\" Admin_Level=\"{2}\" />", InventoryCheck.IsEnabled, InventoryCheck.Ban_Player, InventoryCheck.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Jail\" Enable=\"{0}\" Jail_Size=\"{1}\" Jail_Position=\"{2}\" Jail_Shock=\"{3}\" />", Jail.IsEnabled, Jail.Jail_Size, Jail.Jail_Position, Jail.Jail_Shock));
                sw.WriteLine(string.Format("        <Tool Name=\"Kick_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", KickVote.IsEnabled, KickVote.Players_Online, KickVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Kill_Notice\" Enable=\"{0}\" />", KillNotice.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Lobby\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Lobby_Size=\"{3}\" Lobby_Position=\"{4}\" Command_Cost=\"{5}\" PvP_Check=\"{6}\" Zombie_Check=\"{7}\" />", LobbyChat.IsEnabled, LobbyChat.Return, LobbyChat.Delay_Between_Uses, LobbyChat.Lobby_Size, SetLobby.Lobby_Position, LobbyChat.Command_Cost, LobbyChat.PvP_Check, LobbyChat.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Location\" Enable=\"{0}\" />", Loc.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Login_Notice\" Enable=\"{0}\" />", LoginNotice.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Logs\" Days_Before_Log_Delete=\"{0}\" />", LoadProcess.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Lottery\" Enable=\"{0}\" Bonus=\"{1}\" />", Lottery.IsEnabled, Lottery.Bonus));
                sw.WriteLine(string.Format("        <Tool Name=\"Market\" Enable=\"{0}\" Return=\"{1}\" Delay_Between_Uses=\"{2}\" Market_Size=\"{3}\" Market_Position=\"{4}\" Command_Cost=\"{5}\" PvP_Check=\"{6}\" Zombie_Check=\"{7}\" />", MarketChat.IsEnabled, MarketChat.Return, MarketChat.Delay_Between_Uses, MarketChat.Market_Size, SetMarket.Market_Position, MarketChat.Command_Cost, LobbyChat.PvP_Check, LobbyChat.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Motd\" Enable=\"{0}\" Show_On_Respawn=\"{1}\" />", Motd.IsEnabled, Motd.Show_On_Respawn));
                sw.WriteLine(string.Format("        <Tool Name=\"Mute_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", MuteVote.IsEnabled, MuteVote.Players_Online, MuteVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Player\" Enable=\"{0}\" Entry_Message=\"{1}\" />", NewPlayer.IsEnabled, NewPlayer.Entry_Message));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Spawn_Tele\" Enable=\"{0}\" New_Spawn_Tele_Position=\"{1}\" Return=\"{2}\" />", NewSpawnTele.IsEnabled, NewSpawnTele.New_Spawn_Tele_Position, NewSpawnTele.Return));
                sw.WriteLine(string.Format("        <Tool Name=\"Night_Alert\" Enable=\"{0}\" Delay=\"{1}\" />", NightAlert.IsEnabled, Timers.Night_Time_Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Night_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", NightVote.IsEnabled, NightVote.Players_Online, NightVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Normal_Player_Name_Coloring\" Enable=\"{0}\" Normal_Player_Prefix=\"{1}\" Normal_Player_Color=\"{2}\" />", ChatHook.Normal_Player_Name_Coloring, ChatHook.Normal_Player_Prefix, ChatHook.Normal_Player_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_List\" Enable=\"{0}\" />", PlayerList.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Logs\" Enable=\"{0}\" Interval=\"{1}\" Position=\"{2}\" Inventory=\"{3}\" Extra=\"{4}\" />", PlayerLogs.IsEnabled, Timers.Player_Log_Interval, PlayerLogs.Position, PlayerLogs.Inventory, PlayerLogs.P_Data));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stat_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Kick_Enabled=\"{2}\" Ban_Enabled=\"{3}\" />", PlayerStatCheck.IsEnabled, PlayerStatCheck.Admin_Level, PlayerStatCheck.Kick_Enabled, PlayerStatCheck.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Private_Message\" Enable=\"{0}\" />", Whisper.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Real_World_Time\" Enable=\"{0}\" Delay=\"{1}\" Time_Zone=\"{2}\" Adjustment=\"{3}\" />", RealWorldTime.IsEnabled, Timers.Real_Time_Delay, RealWorldTime.Time_Zone, RealWorldTime.Adjustment));
                sw.WriteLine(string.Format("        <Tool Name=\"Report\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Admin_Level=\"{2}\" />", Report.IsEnabled, Report.Delay, Report.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Reserved_Slots\" Enable=\"{0}\" Reserved_Check=\"{1}\" Session_Time=\"{2}\" Admin_Level=\"{3}\" Admin_Slots=\"{4}\" Reduced_Delay=\"{5}\" />", ReservedSlots.IsEnabled, ReservedSlots.Reserved_Check, ReservedSlots.Session_Time, ReservedSlots.Admin_Level, ReservedSlots.Admin_Slots, ReservedSlots.Reduced_Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Restart_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" Admin_Level=\"{3}\" />", RestartVote.IsEnabled, RestartVote.Players_Online, RestartVote.Votes_Needed, RestartVote.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Session\" Enable=\"{0}\" />", Shop.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Set_Home\" Enable=\"{0}\" Set_Home2_Enabled=\"{1}\" Set_Home2_Donor_Only=\"{2}\" Delay_Between_Uses=\"{3}\" Command_Cost=\"{4}\" PvP_Check=\"{5}\" Zombie_Check=\"{6}\" Vehicle=\"{7}\" />", TeleportHome.IsEnabled, TeleportHome.Set_Home2_Enabled, TeleportHome.Set_Home2_Donor_Only, TeleportHome.Delay_Between_Uses, TeleportHome.Command_Cost, TeleportHome.PvP_Check, TeleportHome.Zombie_Check, TeleportHome.Vehicle));
                sw.WriteLine(string.Format("        <Tool Name=\"Shop\" Enable=\"{0}\" Inside_Market=\"{1}\" Inside_Traders=\"{2}\" />", Shop.IsEnabled, Shop.Inside_Market, Shop.Inside_Traders));
                sw.WriteLine(string.Format("        <Tool Name=\"Starting_Items\" Enable=\"{0}\" />", StartingItems.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Stopserver\" Ten_Second_Countdown=\"{0}\" Kick_30_Seconds=\"{1}\" Alert_Count=\"{2}\" />", StopServer.Ten_Second_Countdown, StopServer.Kick_30_Seconds, StopServer.Alert_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Stuck\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", Stuck.IsEnabled, Stuck.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Suicide\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" PvP_Check=\"{2}\" Zombie_Check=\"{3}\" />", Suicide.IsEnabled, Suicide.Delay_Between_Uses, Suicide.PvP_Check, Suicide.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Travel\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Command_Cost=\"{2}\" PvP_Check=\"{3}\" Zombie_Check=\"{4}\" />", Travel.IsEnabled, Travel.Delay_Between_Uses, Travel.Command_Cost, Travel.PvP_Check, Travel.Zombie_Check));
                sw.WriteLine(string.Format("        <Tool Name=\"Underground_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Max_Ping=\"{2}\" Kill_Enabled=\"{3}\" Announce=\"{4}\" Jail_Enabled=\"{5}\" Kick_Enabled=\"{6}\" Ban_Enabled=\"{7}\" />", UndergroundCheck.IsEnabled, UndergroundCheck.Admin_Level, UndergroundCheck.Max_Ping, UndergroundCheck.Kill_Player, UndergroundCheck.Announce, UndergroundCheck.Jail_Enabled, UndergroundCheck.Kick_Enabled, UndergroundCheck.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Vehicle_Teleport\" Enable=\"{0}\" Bike=\"{1}\" Mini_Bike=\"{2}\" Motor_Bike=\"{3}\" Jeep=\"{4}\" Gyro=\"{5}\" Inside_Claim=\"{6}\" Delay_Between_Uses=\"{7}\" Command_Cost=\"{8}\" />", VehicleTeleport.IsEnabled, VehicleTeleport.Bike, VehicleTeleport.Mini_Bike, VehicleTeleport.Motor_Bike, VehicleTeleport.Jeep, VehicleTeleport.Gyro, VehicleTeleport.Inside_Claim, VehicleTeleport.Delay_Between_Uses, VehicleTeleport.Command_Cost));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting\" Enable=\"{0}\" Your_Voting_Site=\"{1}\" API_Key=\"{2}\" Delay_Between_Uses=\"{3}\" Reward_Count=\"{4}\" />", VoteReward.IsEnabled, VoteReward.Your_Voting_Site, VoteReward.API_Key, VoteReward.Delay_Between_Uses, VoteReward.Reward_Count));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting_Extended\" Reward_Entity=\"{0}\" Entity_Id=\"{1}\" Weekly_Votes=\"{2}\" />", VoteReward.Reward_Entity, VoteReward.Entity_Id, VoteReward.Weekly_Votes));
                sw.WriteLine(string.Format("        <Tool Name=\"Wallet\" Enable=\"{0}\" Coin_Name=\"{1}\" Zombie_Kill_Value=\"{2}\" Player_Kill_Value=\"{3}\" Death_Penalty_Value=\"{4}\" Lose_On_Death=\"{5}\" />", Wallet.IsEnabled, Wallet.Coin_Name, Wallet.Zombie_Kills, Wallet.Player_Kills, Wallet.Deaths, Wallet.Lose_On_Death));
                sw.WriteLine(string.Format("        <Tool Name=\"Watchlist\" Enable=\"{0}\" Admin_Level=\"{1}\" Alert_Delay=\"{2}\" />", Watchlist.IsEnabled, Watchlist.Admin_Level, Timers.Alert_Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Waypoints\" Enable=\"{0}\" Max_Waypoints =\"{1}\" Donator_Max_Waypoints=\"{2}\" Command_Cost =\"{3}\" Delay_Between_Uses=\"{4}\" PvP_Check =\"{5}\" Zombie_Check=\"{6}\" Vehicle=\"{7}\" />", Waypoint.IsEnabled, Waypoint.Max_Waypoints, Waypoint.Donator_Max_Waypoints, Waypoint.Command_Cost, Waypoint.Delay_Between_Uses, Waypoint.PvP_Check, Waypoint.Zombie_Check, Waypoint.Vehicle));
                sw.WriteLine(string.Format("        <Tool Name=\"Weather_Vote\" Enable=\"{0}\" Players_Online=\"{1}\" Votes_Needed=\"{2}\" />", WeatherVote.IsEnabled, WeatherVote.Players_Online, WeatherVote.Votes_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"World_Radius\" Enable=\"{0}\" Normal_Player=\"{1}\" Donator=\"{2}\" Admin_Level=\"{3}\" />", WorldRadius.IsEnabled, WorldRadius.Normal_Player, WorldRadius.Donator, WorldRadius.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Zone\" Enable=\"{0}\" Kill_Enabled=\"{1}\" Jail_Enabled=\"{2}\" Kick_Enabled=\"{3}\" Ban_Enabled=\"{4}\" Zone_Message=\"{5}\" Reminder_Notice_Delay=\"{6}\" Set_Home=\"{7}\"/>", Zones.IsEnabled, Zones.Kill_Enabled, Zones.Jail_Enabled, Zones.Kick_Enabled, Zones.Ban_Enabled, Zones.Zone_Message, Zones.Reminder_Delay, Zones.Set_Home));
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
            if (!Utils.FileExists(configFilePath))
            {
                WriteXml();
            }
            LoadXml();
            Mods.Load();
        }
    }
}