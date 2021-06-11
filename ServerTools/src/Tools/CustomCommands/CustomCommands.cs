using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static string Command15 = "commands";
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        public static Dictionary<string, int[]> Dict1 = new Dictionary<string, int[]>();
        public static List<int> TeleportCheckProtection = new List<int>();
        private const string file = "CustomChatCommands.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            if (IsRunning && !IsEnabled)
            {
                Dict.Clear();
                Dict1.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
            }
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Commands")
                {
                    Dict.Clear();
                    Dict1.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Custom Commands' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Trigger"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Trigger attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Hidden"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Hidden attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Permission"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing (True/False) value for 'Permission' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _delay = 0;
                        if (_line.HasAttribute("DelayBetweenUses"))
                        {
                            if (!int.TryParse(_line.GetAttribute("DelayBetweenUses"), out _delay))
                            {
                                string _stringDelay = _line.GetAttribute("DelayBetweenUses");
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses of custom command because of invalid (non-numeric) value {0}", _stringDelay));
                            }
                        }
                        int _cost = 0;
                        if (_line.HasAttribute("Cost"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Cost"), out _cost))
                            {
                                string _stringCost = _line.GetAttribute("Cost");
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for Cost of custom command because of invalid (non-numeric) value {0}", _stringCost));
                            }
                        }
                        string _trigger = _line.GetAttribute("Trigger");
                        string _response = _line.GetAttribute("Response");
                        string _hidden = _line.GetAttribute("Hidden");
                        string _permission = _line.GetAttribute("Permission");
                        string[] _s = { _response, _hidden, _permission };
                        int[] _c = { _delay, _cost };
                        if (!Dict.ContainsKey(_trigger))
                        {
                            Dict.Add(_trigger, _s);
                        }
                        else
                        {
                            Dict[_trigger] = _s;
                        }
                        if (!Dict1.ContainsKey(_trigger))
                        {
                            Dict1.Add(_trigger, _c);
                        }
                        else
                        {
                            Dict1[_trigger] = _c;
                        }
                        if (_permission.ToLower() == "true" && !GameManager.Instance.adminTools.GetCommands().ContainsKey(_trigger))
                        {
                            GameManager.Instance.adminTools.AddCommandPermission(_trigger, 0);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        if (Dict1.TryGetValue(kvp.Key, out int[] _value))
                        {
                            sw.WriteLine(string.Format("        <Command Trigger=\"{0}\" Response=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Permission=\"{4}\" Cost=\"{5}\" />", kvp.Key, kvp.Value[0], _value[0], kvp.Value[1], kvp.Value[2], _value[1]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Trigger=\"help\" Response=\"global Type " + ChatHook.Chat_Command_Prefix1 + Command15 + " for a list of chat commands.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"info\" Response=\"global Server Info: \" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"rules\" Response=\"whisper Visit YourSiteHere to see the rules.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"website\" Response =\"whisper Visit YourSiteHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"teamspeak\" Response=\"whisper The Teamspeak3 info is YourInfoHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"spawnz\" Response=\"st-ser {EntityId} r.40 4 11 17 ^ whisper Zombies have spawn around you.\" DelayBetweenUses=\"60\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"discord\" Response=\"whisper The discord channel is ...\" DelayBetweenUses=\"20\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc8\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc9\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc10\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc11\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc12\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc13\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc14\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc15\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc16\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc17\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc18\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc19\" Response=\"First command ^ {Delay} 30 ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Trigger=\"cc20\" Response=\"First command ^ Second command ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                }
                sw.WriteLine("    </Commands>");
                sw.WriteLine("</CustomCommands>");
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
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void CommandList(ClientInfo _cInfo)
        {
            string _commands = "";
            if (FriendTeleport.IsEnabled)
            {
                if (FriendTeleport.Command59 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command59);
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command59);
                }
                if (FriendTeleport.Command60 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command60);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Wallet.IsEnabled)
            {
                if (Wallet.Command56 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Wallet.Command56);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shop.IsEnabled)
            {
                if (Shop.Command57 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command57);
                }
                if (Shop.Command58 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command58);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Gimme.IsEnabled)
            {
                if (Gimme.Command24 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Gimme.Command24);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Homes.IsEnabled)
            {
                if (Homes.Command1 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command1);
                }
                if (Homes.Command2 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command1);
                }
                if (Homes.Command3 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command2);
                }
                if (Homes.Command4 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command4);
                }
                if (Homes.Command5 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command5);
                }
                if (Homes.Command6 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command6);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                if (ClanManager.Command42 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command42);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                if (ClanManager.Command125 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command125);
                }

                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled && !ClanManager.ClanMember.Contains(_cInfo.playerId))
            {
                if (ClanManager.Command45 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command45);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (FirstClaimBlock.IsEnabled)
            {
                if (FirstClaimBlock.Command32 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command32);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (RestartVote.IsEnabled)
            {
                if (RestartVote.Command66 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command66);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Animals.IsEnabled)
            {
                if (Animals.Command30 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command30);
                }
                if (Animals.Command31 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command31);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (VoteReward.IsEnabled)
            {
                if (VoteReward.Command46 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VoteReward.Command46);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shutdown.IsEnabled)
            {
                if (Shutdown.Command47 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command47);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (AdminList.IsEnabled)
            {
                if (AdminList.Command48 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, AdminList.Command48);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Travel.IsEnabled)
            {
                if (Travel.Command49 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Travel.Command49);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (WeatherVote.IsEnabled)
            {
                if (WeatherVote.Command62 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command62);
                }
                if (WeatherVote.VoteOpen)
                {
                    if (WeatherVote.Command63 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command63);
                    }
                    if (WeatherVote.Command64 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command64);
                    }
                    if (WeatherVote.Command65 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command65);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Auction.IsEnabled)
            {
                if (Auction.Command71 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command71);
                }
                if (Auction.Command72 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command72);
                }
                if (Auction.Command73 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command73);
                }
                if (Auction.Command74 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command74);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Died.IsEnabled)
            {
                if (Died.Command61 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Died.Command61);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Fps.IsEnabled)
            {
                if (Fps.Command75 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Fps.Command75);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Loc.IsEnabled)
            {
                if (Loc.Command76 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Loc.Command76);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (MuteVote.IsEnabled && Mute.IsEnabled)
            {
                if (MuteVote.Command67 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command67);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (KickVote.IsEnabled)
            {
                if (KickVote.Command68 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, KickVote.Command68);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Suicide.IsEnabled)
            {
                if (Suicide.Command20 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command20);
                }
                if (Suicide.Command23 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command23);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lobby.IsEnabled)
            {
                if (Lobby.Command88 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command88);
                }
                if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                {
                    if (Lobby.Command53 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command53);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bounties.IsEnabled)
            {
                if (Bounties.Command83 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command83);
                }
                if (Bounties.Command83 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command83);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lottery.IsEnabled)
            {
                if (Lottery.Command84 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command84);
                }
                if (Lottery.Command84 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command84);
                }
                if (Lottery.Command85 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command85);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Report.IsEnabled)
            {
                if (Report.Command82 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Report.Command82);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Stuck.IsEnabled)
            {
                if (Stuck.Command90 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Stuck.Command90);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bank.IsEnabled)
            {
                if (Bank.Command94 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command94);
                }
                if (Bank.Command95 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command95);
                }
                if (Bank.Command96 != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command96);
                }
                if (Wallet.IsEnabled)
                {
                    if (Bank.Command97 != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command97);
                    }
                    if (Bank.Command98 != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command98);
                    }
                }
                if (Bank.Player_Transfers)
                {
                    if (Bank.Command99 != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command99);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Market.IsEnabled)
            {
                if (Market.Command103 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command103);
                }
                if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                {
                    if (Market.Command51 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command51);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (InfoTicker.IsEnabled)
            {
                if (InfoTicker.Command104 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command104);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Waypoints.IsEnabled)
            {
                if (Waypoints.Command106 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command106);
                }
                if (Waypoints.Command106 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command106);
                }
                if (Waypoints.Command112 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command112);
                }
                if (Waypoints.Command115 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command115);
                }
                if (Waypoints.Command109 != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command109);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (VehicleTeleport.IsEnabled)
            {
                if (VehicleTeleport.Bike)
                {
                    if (VehicleTeleport.Command77 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command77);
                    }
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    if (VehicleTeleport.Command78 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command78);
                    }
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    if (VehicleTeleport.Command79 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command79);
                    }
                }
                if (VehicleTeleport.Jeep)
                {
                    if (VehicleTeleport.Command80 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command80);
                    }
                }
                if (VehicleTeleport.Gyro)
                {
                    if (VehicleTeleport.Command81 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command81);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (PlayerList.IsEnabled)
            {
                if (PlayerList.Command89 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command89);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bloodmoon.IsEnabled)
            {
                if (Bloodmoon.Command18 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command18);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Whisper.IsEnabled)
            {
                if (Whisper.Command122 != "***")
                {
                    _commands = string.Format("{0} {1}{2} {3}{4}", _commands, ChatHook.Chat_Command_Prefix1, Whisper.Command120, ChatHook.Chat_Command_Prefix1, Whisper.Command122);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Hardcore.IsEnabled)
            {
                if (!Hardcore.Optional)
                {
                    if (Hardcore.Command11 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command11);
                    }
                    if (Hardcore.Command12 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command12);
                    }
                    if (Hardcore.Command127 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command127);
                    }
                    if (Hardcore.Max_Extra_Lives > 0)
                    {
                        if (Hardcore.Command101 != "***")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command101);
                        }
                    }
                }
                else if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                {
                    if (Hardcore.Command11 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command11);
                    }
                    if (Hardcore.Command12 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command12);
                    }
                    if (Hardcore.Command127 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command127);
                    }
                    if (Hardcore.Max_Extra_Lives > 0)
                    {
                        if (Hardcore.Command101 != "***")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command101);
                        }
                    }
                }
                else
                {
                    if (Hardcore.Command11 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command11);
                    }
                    if (Hardcore.Command12 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command12);
                    }
                    if (Hardcore.Command128 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command128);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
            {
                if (ReservedSlots.Command69 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ReservedSlots.Command69);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Day7.IsEnabled)
            {
                if (Day7.Command16 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Day7.Command16);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Mute.IsEnabled)
            {
                if (Mute.Command13 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command13);
                }
                if (Mute.Command14 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command14);
                }
                if (Mute.Command119 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command119);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (_commands.Length >= 0)
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CustomCommandList(ClientInfo _cInfo)
        {
            if (Dict.Count > 0)
            {
                string _commands = "";
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    string _h = kvp.Value[1];
                    if (bool.TryParse(_h, out bool _result))
                    {
                        if (!_result)
                        {
                            string _c = kvp.Key;
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, _c);
                            if (_commands.Length >= 100)
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                _commands = "";
                            }
                        }
                    }
                }
                if (_commands.Length >= 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            string _commands = "";
            if (AdminChat.IsEnabled)
            {
                _commands = string.Format("{0} @" + AdminChat.Command118, _commands);
            }
            if (Jail.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Jail.Command27);
            }
            if (_commands.Length >= 0)
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            if (Dict.TryGetValue(_message, out string[] _c))
            {
                if (Dict1.TryGetValue(_message, out int[] _c1))
                {
                    if (bool.TryParse(_c[2], out bool _permission))
                    {
                        if (_permission)
                        {
                            if (Permission(_cInfo, _message))
                            {
                                if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_message))
                                {
                                    DateTime _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_message];
                                    Delay(_cInfo, _message, _c1, _lastUse);
                                }
                                else
                                {
                                    Delay(_cInfo, _message, _c1, DateTime.MinValue);
                                }
                            }
                        }
                        else
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_message))
                            {
                                DateTime _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_message];
                                Delay(_cInfo, _message, _c1, _lastUse);
                            }
                            else
                            {
                                Delay(_cInfo, _message, _c1, DateTime.MinValue);
                            }
                        }
                    }
                }
            }
        }

        private static bool Permission(ClientInfo _cInfo, string _message)
        {
            try
            {
                string[] _command = { _message };
                if (GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo))
                {
                    return true;
                }
                else
                {
                    Phrases.Dict.TryGetValue(332, out string _phrase332);
                    _phrase332 = _phrase332.Replace("{Command}", _message);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase332 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.Permission: {0}", e.Message));
            }
            return false;
        }

        private static void Delay(ClientInfo _cInfo, string _message, int[] _c1, DateTime _lastUse)
        {
            TimeSpan varTime = DateTime.Now - _lastUse;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timePassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
                    {
                        int _newDelay = _c1[0] / 2;
                        TimePass(_cInfo, _message, _timePassed, _newDelay, _c1);
                        return;
                    }
                }
            }
            TimePass(_cInfo, _message, _timePassed, _c1[0], _c1);
        }

        private static void TimePass(ClientInfo _cInfo, string _message, int _timePassed, int _delay, int[] _c1)
        {
            if (_timePassed >= _delay)
            {
                CommandCost(_cInfo, _message, _c1);
            }
            else
            {
                int _timeleft = _delay - _timePassed;
                Response(_cInfo, _message, _timeleft, _delay);
            }
        }

        private static void Response(ClientInfo _cInfo, string _message, int _timeLeft, int _newDelay)
        {
            try
            {
                Phrases.Dict.TryGetValue(331, out string _phrase331);
                _phrase331 = _phrase331.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase331 = _phrase331.Replace("{CommandCustom}", _message);
                _phrase331 = _phrase331.Replace("{DelayBetweenUses}", _newDelay.ToString());
                _phrase331 = _phrase331.Replace("{TimeRemaining}", _timeLeft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase331 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.Response: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _message, int[] _c1)
        {
            try
            {
                if (Wallet.IsEnabled && _c1[1] > 0)
                {
                    int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                    if (_currentCoins >= _c1[1])
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _c1[1]);
                        CommandDelay(_cInfo, _message);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(333, out string _phrase333);
                        _phrase333 = _phrase333.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase333 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    CommandDelay(_cInfo, _message);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandCost: {0}", e.Message));
            }
        }

        private static void CommandDelay(ClientInfo _cInfo, string _trigger)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_trigger))
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_trigger] = DateTime.Now;
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.Add(_trigger, DateTime.Now);
                    }
                }
                else
                {
                    Dictionary<string, DateTime> _delays = new Dictionary<string, DateTime>
                    {
                        { _trigger, DateTime.Now }
                    };
                    PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays = _delays;
                }
                PersistentContainer.DataChange = true;
                if (Dict.TryGetValue(_trigger, out string[] _commandResponse))
                {
                    string[] _commandsSplit = _commandResponse[0].Split('^');
                    for (int i = 0; i < _commandsSplit.Length; i++)
                    {
                        string _commandTrimmed = _commandsSplit[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            if (i < _commandsSplit.Length)
                            {
                                string _commandsRebuilt = "";
                                for (int j = i; j < _commandsSplit.Length; j++)
                                {
                                    string _delayedCommandTrimmed = _commandsSplit[j].Trim();
                                    if (j == i + 1)
                                    {
                                        _commandsRebuilt = _delayedCommandTrimmed;
                                    }
                                    else
                                    {
                                        _commandsRebuilt = _commandsRebuilt + " ^ " + _delayedCommandTrimmed;
                                    }
                                }
                                string _timeString = _commandTrimmed.Split(' ').Skip(1).First();
                                int.TryParse(_timeString, out int _time);
                                Timers.SingleUseTimer(_time, _cInfo.playerId, _commandsRebuilt);
                            }
                            return;
                        }
                        else
                        {
                            CommandExec(_cInfo, _commandTrimmed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandDelay: {0}", e.Message));
            }
        }

        public static void DelayedCommand(string _playerId, string _commands)
        {
            try
            {
                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
                if (_cInfo != null)
                {
                    string[] _commandsSplit = _commands.Split('^');
                    for (int i = 0; i < _commandsSplit.Length; i++)
                    {
                        string _commandTrimmed = _commandsSplit[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            if (i < _commandsSplit.Length)
                            {
                                string _commandsRebuilt = "";
                                for (int j = i + 1; j < _commandsSplit.Length; j++)
                                {
                                    string _delayedCommandTrimmed = _commandsSplit[j].Trim();
                                    if (j == i + 1)
                                    {
                                        _commandsRebuilt = _delayedCommandTrimmed;
                                    }
                                    else
                                    {
                                        _commandsRebuilt = _commandsRebuilt + " ^ " + _delayedCommandTrimmed;
                                    }
                                }
                                string _timeString = _commandTrimmed.Split(' ').Skip(1).First();
                                int.TryParse(_timeString, out int _time);
                                Timers.SingleUseTimer(_time, _cInfo.playerId, _commandsRebuilt);
                            }
                            return;
                        }
                        else
                        {
                            CommandExec(_cInfo, _commandTrimmed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.DelayedCommand: {0}", e.Message));
            }
        }

        public static void CommandExec(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (_cInfo != null)
                {
                    _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                    _command = _command.Replace("{SteamId}", _cInfo.playerId);
                    _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                    if (_command.ToLower().StartsWith("global "))
                    {
                        _command = _command.Replace("Global ", "");
                        _command = _command.Replace("global ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (_command.ToLower().StartsWith("whisper "))
                    {
                        _command = _command.Replace("Whisper ", "");
                        _command = _command.Replace("whisper ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else if (_command.StartsWith("tele ") || _command.StartsWith("tp ") || _command.StartsWith("teleportplayer "))
                    {
                        SdtdConsole.Instance.ExecuteSync(_command, null);
                        if (Zones.IsEnabled && Zones.ZoneInfo.ContainsKey(_cInfo.entityId))
                        {
                            Zones.ZoneInfo.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.ExecuteSync(_command, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandExec: {0}", e.Message));
            }
        }
    }
}