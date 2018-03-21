using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Config
    {
        private const string configFile = "ServerToolsConfig.xml";
        private static string configFilePath = string.Format("{0}/{1}", API.ConfigPath, configFile);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, configFile);
        public const double version = 8.1;
        public static bool UpdateConfigs = false;
        public static string Chat_Response_Color = "[00FF00]";
        public static string Clan_Response_Color = "[00FF00]";

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(configFilePath))
            {
                UpdateXml();
                Phrases.Load();
                return;
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
                            UpdateConfigs = true;
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
                            case "Admin_Name_Coloring":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Admin_Name_Coloring))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out ChatHook.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Admin_Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Admin_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Moderator_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Moderator_Level"), out ChatHook.Mod_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of invalid (non-numeric) value for 'Moderator_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Moderator_Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Moderator_Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Moderator_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Admin_Name_Coloring entry because of missing 'Moderator_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }                              
                                ChatHook.Admin_Prefix = _line.GetAttribute("Admin_Prefix");
                                ChatHook.Admin_Color = _line.GetAttribute("Admin_Color");
                                ChatHook.Mod_Prefix = _line.GetAttribute("Moderator_Prefix");
                                ChatHook.Mod_Color = _line.GetAttribute("Moderator_Color");
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
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
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
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out Animals.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Minimum_Spawn_Radius"), out Animals.Minimum_Spawn_Radius))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Animal_Tracking entry because of invalid (non-numeric) value for 'Minimum_Spawn_Radius' attribute: {0}", subChild.OuterXml));
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
                                Animals.Animal_List = _line.GetAttribute("Entity_Id");
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
                                if (!_line.HasAttribute("Auto_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Auto_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Auto_Enabled"), out Bloodmoon.Auto_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (true/false) value for 'Auto_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Auto_Show_Bloodmoon_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of missing 'Auto_Show_Bloodmoon_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Auto_Show_Bloodmoon_Delay"), out Timers.Auto_Show_Bloodmoon_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bloodmoon entry because of invalid (non-numeric) value for 'Auto_Show_Bloodmoon_Delay' attribute: {0}", subChild.OuterXml));
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
                            case "Chat_Command_Response":
                                if (!_line.HasAttribute("Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                Config.Chat_Response_Color = _line.GetAttribute("Color");
                                if (!_line.HasAttribute("Chat_Command_Private"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Chat_Command_Private' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Command_Private = _line.GetAttribute("Chat_Command_Private");
                                if (!_line.HasAttribute("Chat_Command_Public"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Chat_Command_Response entry because of missing 'Chat_Command_Public' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Command_Public = _line.GetAttribute("Chat_Command_Public");
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
                                if (!_line.HasAttribute("Chat_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan_Manager entry because of missing 'Chat_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                Config.Clan_Response_Color = _line.GetAttribute("Chat_Color");
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
                                if (!_line.HasAttribute("Bad_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of missing 'Bad_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Bad_Id"), out CredentialCheck.Bad_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Credentials entry because of invalid (true/false) value for 'Bad_Id' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of missing 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay"), out DeathSpot.Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Death_Spot entry because of invalid (non-numeric) value for 'Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Donator_Name_Coloring":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Donator_Name_Coloring))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ReservedSlots.Donator_Name_Coloring))
                                {
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator_Level1"), out ChatHook.Don_Level1))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of invalid (non-numeric) value for 'Donator_Level1' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Prefix1"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Prefix1' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Color1"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Color1' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator_Level2"), out ChatHook.Don_Level2))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of invalid (non-numeric) value for 'Donator_Level2' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Prefix2"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Prefix2' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Color2"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Color2' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Donator_Level3"), out ChatHook.Don_Level3))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of invalid (non-numeric) value for 'Donator_Level3' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Prefix3"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Prefix3' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Donator_Color3"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Donator_Name_Coloring entry because of missing 'Donator_Color3' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Don_Prefix1 = _line.GetAttribute("Donator_Prefix1");
                                ChatHook.Don_Prefix2 = _line.GetAttribute("Donator_Prefix2");
                                ChatHook.Don_Prefix3 = _line.GetAttribute("Donator_Prefix3");
                                ChatHook.Don_Color1 = _line.GetAttribute("Donator_Color1");
                                ChatHook.Don_Color2 = _line.GetAttribute("Donator_Color2");
                                ChatHook.Don_Color3 = _line.GetAttribute("Donator_Color3");
                                break;
                            case "Entity_Underground_Check":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out EntityUnderground.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Alert_Admin"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of missing 'Alert_Admin' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Alert_Admin"), out EntityUnderground.Alert_Admin))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of invalid (true/false) value for 'Alert_Admin' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Admin_Level"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of missing 'Admin_Level' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Admin_Level"), out EntityUnderground.Admin_Level))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Entity_Underground_Check entry because of invalid (non-numeric) value for 'Admin_Level' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Days_Before_Log_Delete"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of missing 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Before_Log_Delete"), out FlightCheck.Days_Before_Log_Delete))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Flight_Check entry because of invalid (non-numeric) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
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
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Messages"), out InfoTicker.Delay_Between_Messages))
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
                            case "Item_Cleanup":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out EntityCleanup.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("World_Max"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of missing 'World_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("World_Max"), out EntityCleanup.World_Max))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of invalid (non-numeric) value for 'World_Max' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Max_Pile"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of missing 'Max_Pile' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Max_Pile"), out EntityCleanup.Max_Pile))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item_Cleanup entry because of invalid (non-numeric) value for 'Max_Pile' attribute: {0}", subChild.OuterXml));
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
                                Jail.Jail_Position = _line.GetAttribute("Jail_Position");
                                break;
                            case "Killme":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out KillMe.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Delay_Between_Uses"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of missing 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Delay_Between_Uses"), out KillMe.Delay_Between_Uses))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Killme entry because of invalid (non-numeric) value for 'Delay_Between_Uses' attribute: {0}", subChild.OuterXml));
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
                                NewSpawnTele.New_Spawn_Tele_Position = _line.GetAttribute("New_Spawn_Tele_Position");
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
                                ChatHook.Normal_Player_Prefix = _line.GetAttribute("Normal_Player_Prefix");
                                ChatHook.Normal_Player_Color = _line.GetAttribute("Normal_Player_Color");
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
                                if (!_line.HasAttribute("Days_Before_Log_Delete"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of missing 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Before_Log_Delete"), out PlayerLogs.Days_Before_Log_Delete))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player_Logs entry because of invalid (non-numeric) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
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
                                if (!bool.TryParse(_line.GetAttribute("Reserved_Check"), out ChatHook.Reserved_Check))
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
                                if (!_line.HasAttribute("Restart_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Restart_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Restart_Delay"), out Timers.Restart_Vote_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (non-numeric) value for 'Restart_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Minimum_Players"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of missing 'Minimum_Players' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Minimum_Players"), out RestartVote.Minimum_Players))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Restart_Vote entry because of invalid (non-numeric) value for 'Minimum_Players' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Negative_Wallet"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Negative_Wallet' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out Wallet.Negative_Wallet))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (true/false) value for 'Negative_Wallet' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Coin_Name"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Coin_Name' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                Wallet.Coin_Name = _line.GetAttribute("Coin_Name");
                                if (!_line.HasAttribute("Zombie_Kill_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Zombie_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Zombie_Kill_Value"), out Wallet.Zombie_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Zombie_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Player_Kill_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Player_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Player_Kill_Value"), out Wallet.Player_Kills))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Player_Kill_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Death_Penalty_Value"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Death_Penalty_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Death_Penalty_Value"), out Wallet.Deaths))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Death_Penalty_Value' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Shop_Anywhere"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing 'Shop_Anywhere' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Shop_Anywhere"), out Shop.Anywhere))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (true/false) value for 'Shop_Anywhere' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Special_Player_Name_Coloring":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Special_Player_Name_Coloring entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ChatHook.Special_Player_Name_Coloring))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Special_Player_Name_Coloring entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Special_Player_Prefix"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Special_Player_Name_Coloring entry because of missing 'Special_Player_Prefix' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Special_Player_Prefix = _line.GetAttribute("Special_Player_Prefix");
                                if (!_line.HasAttribute("Special_Player_Color"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Special_Player_Name_Coloring entry because of missing 'Special_Player_Color' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Special_Player_Color = _line.GetAttribute("Special_Player_Color");
                                if (!_line.HasAttribute("Special_Player_Steam_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Special_Player_Name_Coloring entry because of missing 'Special_Player_SteamId' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                ChatHook.Special_Players_List = _line.GetAttribute("Special_Player_Steam_Id");
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
                                if (!_line.HasAttribute("Kick_Login"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of missing 'Kick_Login' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Login"), out StopServer.Kick_Login))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Stopserver entry because of invalid (true/false) value for 'Kick_Login' attribute: {0}", subChild.OuterXml));
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
                                VoteReward.Your_Voting_Site = _line.GetAttribute("Your_Voting_Site");
                                if (!_line.HasAttribute("API_Key"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'API_Key' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                VoteReward.API_Key = _line.GetAttribute("API_Key");
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
                                if (!_line.HasAttribute("Reward_Entity"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Reward_Entity"), out VoteReward.Reward_Entity))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (true/false) value for 'Reward_Entity' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Entity_Id"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of missing 'Entity_Id' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Entity_Id"), out VoteReward.Entity_Id))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Voting entry because of invalid (non-numeric) value for 'Entity_Id' attribute: {0}", subChild.OuterXml));
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
                                if (!int.TryParse(_line.GetAttribute("Alert_Delay"), out Watchlist.Alert_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Watchlist entry because of invalid (non-numeric) value for 'Alert_Delay' attribute: {0}", subChild.OuterXml));
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
                                if (!_line.HasAttribute("Vote_Delay"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Weather_Vote entry because of missing 'Vote_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Vote_Delay"), out Timers.Weather_Vote_Delay))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Weather_Vote entry because of invalid (non-numeric) value for 'Vote_Delay' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                            case "Zone_Protection":
                                if (!_line.HasAttribute("Enable"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Enable"), out ZoneProtection.IsEnabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Enable' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kill_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kill_Enabled"), out ZoneProtection.Kill_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Kill_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Jail_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Jail_Enabled"), out ZoneProtection.Jail_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Jail_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Kick_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Kick_Enabled"), out ZoneProtection.Kick_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Kick_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Ban_Enabled"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Ban_Enabled"), out ZoneProtection.Ban_Enabled))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Ban_Enabled' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Zone_Message"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Zone_Message"), out ZoneProtection.Zone_Message))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Zone_Message' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Set_Home"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!bool.TryParse(_line.GetAttribute("Set_Home"), out ZoneProtection.Set_Home))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (true/false) value for 'Set_Home' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!_line.HasAttribute("Days_Before_Log_Delete"))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of missing 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Days_Before_Log_Delete"), out ZoneProtection.Days_Before_Log_Delete))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone_Protection entry because of invalid (non-numeric) value for 'Days_Before_Log_Delete' attribute: {0}", subChild.OuterXml));
                                    continue;
                                }
                                break;
                        }
                    }
                }
            }
            if (UpdateConfigs)
            {
                UpdateXml();
            }
            Phrases.Load();
            Mods.Load();
            UpdateConfigs = false;
        }

        public static void UpdateXml()
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
                sw.WriteLine(string.Format("        <Tool Name=\"Admin_Name_Coloring\" Enable=\"{0}\" Admin_Level=\"{1}\" Admin_Prefix=\"{2}\" Admin_Color=\"{3}\" Moderator_Level=\"{4}\" Moderator_Prefix=\"{5}\" Moderator_Color=\"{6}\" />", ChatHook.Admin_Name_Coloring, ChatHook.Admin_Level, ChatHook.Admin_Prefix, ChatHook.Admin_Color, ChatHook.Mod_Level, ChatHook.Mod_Prefix, ChatHook.Mod_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Animal_Tracking\" Enable=\"{0}\" Always_Show_Response=\"{1}\" Delay_Between_Uses=\"{2}\" Minimum_Spawn_Radius=\"{3}\" Maximum_Spawn_Radius=\"{4}\" Entity_Id=\"{5}\" />", Animals.IsEnabled, Animals.Always_Show_Response, Animals.Delay_Between_Uses, Animals.Minimum_Spawn_Radius, Animals.Maximum_Spawn_Radius, Animals.Animal_List));
                sw.WriteLine(string.Format("        <Tool Name=\"Announce_Invalid_Item_Stack\" Enable=\"{0}\" />", InventoryCheck.Anounce_Invalid_Stack));
                sw.WriteLine(string.Format("        <Tool Name=\"Auction\" Enable=\"{0}\" />", AuctionBox.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Save_World\" Enable=\"{0}\" Delay_Between_World_Saves=\"{1}\" />", AutoSaveWorld.IsEnabled, Timers.Delay_Between_World_Saves));
                sw.WriteLine(string.Format("        <Tool Name=\"Auto_Shutdown\" Enable=\"{0}\" Countdown_Timer=\"{1}\" Time_Before_Shutdown=\"{2}\" Alert_On_Login=\"{3}\" />", AutoShutdown.IsEnabled, AutoShutdown.Countdown_Timer, Timers.Shutdown_Delay, AutoShutdown.Alert_On_Login));
                sw.WriteLine(string.Format("        <Tool Name=\"Bad_Word_Filter\" Enable=\"{0}\" Invalid_Name=\"{1}\" />", Badwords.IsEnabled, Badwords.Invalid_Name));
                sw.WriteLine(string.Format("        <Tool Name=\"Bloodmoon\" Enable=\"{0}\" Show_On_Login=\"{1}\" Show_On_Respawn=\"{2}\" Auto_Enabled=\"{3}\" Auto_Show_Bloodmoon_Delay=\"{4}\" Days_Until_Horde=\"{5}\" />", Bloodmoon.IsEnabled, Bloodmoon.Show_On_Login, Bloodmoon.Show_On_Respawn, Bloodmoon.Auto_Enabled, Timers.Auto_Show_Bloodmoon_Delay, Bloodmoon.Days_Until_Horde));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Command_Response\" Color=\"{0}\" Chat_Command_Private=\"{1}\" Chat_Command_Public=\"{2}\" />", Chat_Response_Color, ChatHook.Command_Private, ChatHook.Command_Public));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Flood_Protection\" Enable=\"{0}\" />", ChatHook.ChatFlood));
                sw.WriteLine(string.Format("        <Tool Name=\"Chat_Logger\" Enable=\"{0}\" />", ChatLog.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Clan_Manager\" Enable=\"{0}\" Chat_Color=\"{1}\" />", ClanManager.IsEnabled, Clan_Response_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Credentials\" Enable=\"{0}\" No_Family_Share=\"{1}\" Bad_Id=\"{2}\" No_Internal=\"{3}\" Admin_Level=\"{4}\" />", CredentialCheck.IsEnabled, CredentialCheck.Family_Share, CredentialCheck.Bad_Id, CredentialCheck.No_Internal, CredentialCheck.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Custom_Commands\" Enable=\"{0}\" />", CustomCommands.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Day7\" Enable=\"{0}\" Days_Until_Horde=\"{1}\" />", Day7.IsEnabled, Day7.Days_Until_Horde));
                sw.WriteLine(string.Format("        <Tool Name=\"Death_Spot\" Enable=\"{0}\" Delay=\"{1}\" />", DeathSpot.IsEnabled, DeathSpot.Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Donator_Name_Coloring\" Enable=\"{0}\" Donator_Level1=\"{1}\" Donator_Level2=\"{2}\" Donator_Level3=\"{3}\" Donator_Prefix1=\"{4}\" Donator_Prefix2=\"{5}\" Donator_Prefix3=\"{6}\" Donator_Color1=\"{7}\" Donator_Color2=\"{8}\" Donator_Color3=\"{9}\" />", ChatHook.Donator_Name_Coloring && ReservedSlots.Donator_Name_Coloring, ChatHook.Don_Level1, ChatHook.Don_Level2, ChatHook.Don_Level3, ChatHook.Don_Prefix1, ChatHook.Don_Prefix2, ChatHook.Don_Prefix3, ChatHook.Don_Color1, ChatHook.Don_Color2, ChatHook.Don_Color3));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Cleanup\" Enable=\"{0}\" World_Max=\"{1}\" Max_Pile=\"{2}\" />", EntityCleanup.IsEnabled, EntityCleanup.World_Max, EntityCleanup.Max_Pile));
                sw.WriteLine(string.Format("        <Tool Name=\"Entity_Underground_Check\" Enable=\"{0}\" Alert_Admin=\"{1}\" Admin_Level=\"{2}\" />", EntityUnderground.IsEnabled, EntityUnderground.Alert_Admin, EntityUnderground.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"First_Claim_Block\" Enable=\"{0}\" />", FirstClaimBlock.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Flight_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Max_Ping=\"{2}\" Max_Height=\"{3}\" Kill_Enabled=\"{4}\" Announce=\"{5}\" Jail_Enabled=\"{6}\" Kick_Enabled=\"{7}\" Ban_Enabled=\"{8}\" Days_Before_Log_Delete=\"{9}\" />", FlightCheck.IsEnabled, FlightCheck.Admin_Level, FlightCheck.Max_Ping, FlightCheck.Max_Height, FlightCheck.Kill_Player, FlightCheck.Announce, FlightCheck.Jail_Enabled, FlightCheck.Kick_Enabled, FlightCheck.Ban_Enabled, FlightCheck.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Friend_Teleport\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", FriendTeleport.IsEnabled, FriendTeleport.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Gimme\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" Always_Show_Response=\"{2}\" Zombies=\"{3}\" />", Gimme.IsEnabled, Gimme.Delay_Between_Uses, Gimme.Always_Show_Response, Gimme.Zombies));
                sw.WriteLine(string.Format("        <Tool Name=\"Hatch_Elevator_Detector\" Enable=\"{0}\" />", HatchElevator.IsEnabled));
                sw.WriteLine(string.Format("        <Tool Name=\"High_Ping_Kicker\" Enable=\"{0}\" Max_Ping=\"{1}\" Samples_Needed=\"{2}\" />", HighPingKicker.IsEnabled, HighPingKicker.Max_Ping, HighPingKicker.Samples_Needed));
                sw.WriteLine(string.Format("        <Tool Name=\"Info_Ticker\" Enable=\"{0}\" Delay_Between_Messages=\"{1}\" Random=\"{2}\" />", InfoTicker.IsEnabled, InfoTicker.Delay_Between_Messages, InfoTicker.Random));
                sw.WriteLine(string.Format("        <Tool Name=\"Invalid_Item_Kicker\" Enable=\"{0}\" Ban=\"{1}\" Admin_Level=\"{2}\" />", InventoryCheck.IsEnabled, InventoryCheck.Ban_Player, InventoryCheck.Admin_Level));               
                sw.WriteLine(string.Format("        <Tool Name=\"Jail\" Enable=\"{0}\" Jail_Size=\"{1}\" Jail_Position=\"{2}\" />", Jail.IsEnabled, Jail.Jail_Size, Jail.Jail_Position));
                sw.WriteLine(string.Format("        <Tool Name=\"Killme\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", KillMe.IsEnabled, KillMe.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Motd\" Enable=\"{0}\" Show_On_Respawn=\"{1}\" />", Motd.IsEnabled, Motd.Show_On_Respawn));
                sw.WriteLine(string.Format("        <Tool Name=\"New_Spawn_Tele\" Enable=\"{0}\" New_Spawn_Tele_Position=\"{1}\" />", NewSpawnTele.IsEnabled, NewSpawnTele.New_Spawn_Tele_Position));
                sw.WriteLine(string.Format("        <Tool Name=\"Normal_Player_Name_Coloring\" Enable=\"{0}\" Normal_Player_Prefix=\"{1}\" Normal_Player_Color=\"{2}\" />", ChatHook.Normal_Player_Name_Coloring, ChatHook.Normal_Player_Prefix, ChatHook.Normal_Player_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Logs\" Enable=\"{0}\" Interval=\"{1}\" Position=\"{2}\" Inventory=\"{3}\" Extra=\"{4}\" Days_Before_Log_Delete=\"{5}\" />", PlayerLogs.IsEnabled, Timers.Player_Log_Interval, PlayerLogs.Position, PlayerLogs.Inventory, PlayerLogs.P_Data, PlayerLogs.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Player_Stat_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Kick_Enabled=\"{2}\" Ban_Enabled=\"{3}\" />", PlayerStatCheck.IsEnabled, PlayerStatCheck.Admin_Level, PlayerStatCheck.Kick_Enabled, PlayerStatCheck.Ban_Enabled));
                sw.WriteLine(string.Format("        <Tool Name=\"Reserved_Slots\" Enable=\"{0}\" Reserved_Check=\"{1}\" Session_Time=\"{2}\" Admin_Level=\"{3}\" />", ReservedSlots.IsEnabled, ChatHook.Reserved_Check, ReservedSlots.Session_Time, ReservedSlots.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Restart_Vote\" Enable=\"{0}\" Restart_Delay=\"{1}\" Minimum_Players=\"{2}\" Admin_Level=\"{3}\" />", RestartVote.IsEnabled, Timers.Restart_Vote_Delay, RestartVote.Minimum_Players, RestartVote.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Set_Home\" Enable=\"{0}\" Set_Home2_Enabled=\"{1}\" Set_Home2_Donor_Only=\"{2}\" Delay_Between_Uses=\"{3}\" />", TeleportHome.IsEnabled, TeleportHome.Set_Home2_Enabled, TeleportHome.Set_Home2_Donor_Only, TeleportHome.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Shop\" Enable=\"{0}\" Negative_Wallet=\"{1}\" Coin_Name=\"{2}\" Zombie_Kill_Value=\"{3}\" Player_Kill_Value=\"{4}\" Death_Penalty_Value=\"{5}\" Shop_Anywhere=\"{6}\" />", Shop.IsEnabled, Wallet.Negative_Wallet, Wallet.Coin_Name, Wallet.Zombie_Kills, Wallet.Player_Kills, Wallet.Deaths, Shop.Anywhere));
                sw.WriteLine(string.Format("        <Tool Name=\"Special_Player_Name_Coloring\" Enable=\"{0}\" Special_Player_Steam_Id=\"{1}\" Special_Player_Prefix=\"{2}\" Special_Player_Color=\"{3}\" />", ChatHook.Special_Player_Name_Coloring, ChatHook.Special_Players_List, ChatHook.Special_Player_Prefix, ChatHook.Special_Player_Color));
                sw.WriteLine(string.Format("        <Tool Name=\"Starting_Items\" Enable=\"{0}\" />", StartingItems.IsEnabled));                
                sw.WriteLine(string.Format("        <Tool Name=\"Stopserver\" Ten_Second_Countdown=\"{0}\" Kick_30_Seconds=\"{1}\" Kick_Login=\"{2}\" />", StopServer.Ten_Second_Countdown, StopServer.Kick_30_Seconds, StopServer.Kick_Login));
                sw.WriteLine(string.Format("        <Tool Name=\"Travel\" Enable=\"{0}\" Delay_Between_Uses=\"{1}\" />", Travel.IsEnabled, Travel.Delay_Between_Uses));
                sw.WriteLine(string.Format("        <Tool Name=\"Underground_Check\" Enable=\"{0}\" Admin_Level=\"{1}\" Max_Ping=\"{2}\" Kill_Enabled=\"{3}\" Announce=\"{4}\" Jail_Enabled=\"{5}\" Kick_Enabled=\"{6}\" Ban_Enabled=\"{7}\" Days_Before_Log_Delete=\"{8}\" />", UndergroundCheck.IsEnabled, UndergroundCheck.Admin_Level, UndergroundCheck.Max_Ping, UndergroundCheck.Kill_Player, UndergroundCheck.Announce, UndergroundCheck.Jail_Enabled, UndergroundCheck.Kick_Enabled, UndergroundCheck.Ban_Enabled, UndergroundCheck.Days_Before_Log_Delete));
                sw.WriteLine(string.Format("        <Tool Name=\"Voting\" Enable=\"{0}\" Your_Voting_Site=\"{1}\" API_Key=\"{2}\" Delay_Between_Uses=\"{3}\" Reward_Count=\"{4}\" />", VoteReward.IsEnabled, VoteReward.Your_Voting_Site, VoteReward.API_Key, VoteReward.Delay_Between_Uses, VoteReward.Reward_Count));               
                sw.WriteLine(string.Format("        <Tool Name=\"Watchlist\" Enable=\"{0}\" Admin_Level=\"{1}\" />", Watchlist.IsEnabled, Watchlist.Admin_Level));
                sw.WriteLine(string.Format("        <Tool Name=\"Weather_Vote\" Enable=\"{0}\" Vote_Delay=\"{1}\" />", WeatherVote.IsEnabled, Timers.Weather_Vote_Delay));
                sw.WriteLine(string.Format("        <Tool Name=\"Zone_Protection\" Enable=\"{0}\" Kill_Enabled=\"{1}\" Jail_Enabled=\"{2}\" Kick_Enabled=\"{3}\" Ban_Enabled=\"{4}\" Zone_Message=\"{5}\" Set_Home=\"{6}\" Days_Before_Log_Delete=\"{7}\" />", ZoneProtection.IsEnabled, ZoneProtection.Kill_Enabled, ZoneProtection.Jail_Enabled, ZoneProtection.Kick_Enabled, ZoneProtection.Ban_Enabled, ZoneProtection.Zone_Message, ZoneProtection.Set_Home, ZoneProtection.Days_Before_Log_Delete));
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
                UpdateXml();
            }
            LoadXml();
        }
    }
}