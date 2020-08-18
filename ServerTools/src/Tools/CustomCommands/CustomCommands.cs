using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of invalid (True/False) value for 'Permission' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _delay = 0;
                        if (_line.HasAttribute("DelayBetweenUses"))
                        {
                            if (!int.TryParse(_line.GetAttribute("DelayBetweenUses"), out _delay))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses of Custom Commands entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        int _number = 0;
                        if (_line.HasAttribute("Number"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Number"), out _number))
                            {
                                _number = Dict.Count + 1;
                            }
                        }
                        int _cost = 0;
                        if (_line.HasAttribute("Cost"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Cost"), out _cost))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for cost of custom commands entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        string _trigger = _line.GetAttribute("Trigger");
                        string _response = _line.GetAttribute("Response");
                        string _hidden = _line.GetAttribute("Hidden");
                        string _permission = _line.GetAttribute("Permission");
                        string[] _s = { _response, _hidden, _permission };
                        int[] _c = { _number, _delay, _cost };
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
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        int[] _value;
                        if (Dict1.TryGetValue(kvp.Key, out _value))
                        {
                            sw.WriteLine(string.Format("        <Command Number=\"{0}\" Trigger=\"{1}\" Response=\"{2}\" DelayBetweenUses=\"{3}\" Hidden=\"{4}\" Permission=\"{5}\" Cost=\"{6}\" />", _value[0], kvp.Key, kvp.Value[0], _value[1], kvp.Value[1], kvp.Value[2], _value[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Number=\"1\" Trigger=\"help\" Response=\"global Type " + ChatHook.Command_Private + Command15 + " for a list of chat commands.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"2\" Trigger=\"info\" Response=\"global Server Info: \" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"3\" Trigger=\"rules\" Response=\"whisper Visit YourSiteHere to see the rules.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"4\" Trigger=\"website\" Response =\"whisper Visit YourSiteHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"5\" Trigger=\"teamspeak\" Response=\"whisper The Teamspeak3 info is YourInfoHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"6\" Trigger=\"spawnz\" Response=\"ser {EntityId} 40 @ 4 11 14 ^ whisper Zombies have spawn around you.\" DelayBetweenUses=\"60\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"7\" Trigger=\"discord\" Response=\"whisper The discord channel is ...\" DelayBetweenUses=\"20\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"8\" Trigger=\"cc8\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"9\" Trigger=\"cc9\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"10\" Trigger=\"cc10\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"11\" Trigger=\"cc11\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"12\" Trigger=\"cc12\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"13\" Trigger=\"cc13\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"14\" Trigger=\"cc14\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"15\" Trigger=\"cc15\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"16\" Trigger=\"cc16\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"17\" Trigger=\"cc17\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"18\" Trigger=\"cc18\" Response=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"19\" Trigger=\"cc19\" Response=\"First command ^ {Delay} 30 ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"20\" Trigger=\"cc20\" Response=\"First command ^ Second command ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
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
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, FriendTeleport.Command59);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, FriendTeleport.Command59);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, FriendTeleport.Command60);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Wallet.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Wallet.Command56);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shop.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Shop.Command57);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Shop.Command58);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Gimme.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Gimme.Command24);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (TeleportHome.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command1);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command2);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command3);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command4);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command42);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command125);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled && !ClanManager.ClanMember.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command45);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (FirstClaimBlock.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, FirstClaimBlock.Command32);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (RestartVote.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, RestartVote.Command66);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Animals.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Animals.Command30);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Animals.Command31);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (VoteReward.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VoteReward.Command46);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shutdown.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Shutdown.Command47);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (AdminList.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, AdminList.Command48);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Travel.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Travel.Command49);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (TeleportHome.IsEnabled & TeleportHome.Set_Home2_Enabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command5);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command6);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command7);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, TeleportHome.Command8);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (WeatherVote.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, WeatherVote.Command62);
                if (WeatherVote.VoteOpen)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, WeatherVote.Command63);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, WeatherVote.Command64);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, WeatherVote.Command65);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (AuctionBox.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, AuctionBox.Command71);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, AuctionBox.Command72);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, AuctionBox.Command73);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, AuctionBox.Command74);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (DeathSpot.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, DeathSpot.Command61);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Fps.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Fps.Command75);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Loc.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Loc.Command76);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (MuteVote.IsEnabled && Mute.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, MuteVote.Command67);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (KickVote.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, KickVote.Command68);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Suicide.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Suicide.Command20);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Suicide.Command23);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lobby.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Lobby.Command88);
                if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Lobby.Command53);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bounties.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Bounties.Command83);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bounties.Command83);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lottery.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Lottery.Command84);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Lottery.Command84);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Lottery.Command85);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Report.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Report.Command82);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Stuck.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Stuck.Command90);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bank.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Bank.Command94);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bank.Command95);
                _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bank.Command96);
                if (Wallet.IsEnabled)
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bank.Command97);
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bank.Command98);
                }
                if (Bank.Player_Transfers)
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Command_Private, Bank.Command99);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Market.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Market.Command103);
                if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Market.Command51);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (InfoTicker.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, InfoTicker.Command104);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Waypoints.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Waypoints.Command106);
                _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Command_Private, Waypoints.Command106);
                _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Command_Private, Waypoints.Command112);
                _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Command_Private, Waypoints.Command115);
                _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Command_Private, Waypoints.Command109);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (VehicleTeleport.IsEnabled)
            {
                if (VehicleTeleport.Bike)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VehicleTeleport.Command77);
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VehicleTeleport.Command78);
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VehicleTeleport.Command79);
                }
                if (VehicleTeleport.Jeep)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VehicleTeleport.Command80);
                }
                if (VehicleTeleport.Gyro)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, VehicleTeleport.Command81);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (PlayerList.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, PlayerList.Command89);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bloodmoon.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Bloodmoon.Command18);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Whisper.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2} {3}{4}", _commands, ChatHook.Command_Private, Whisper.Command120, ChatHook.Command_Private, Whisper.Command122);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Hardcore.IsEnabled)
            {
                if (!Hardcore.Optional)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command11);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command12);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command127);
                    if (Hardcore.Max_Extra_Lives > 0)
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command101);
                    }
                }
                else if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command11);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command12);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command127);
                    if (Hardcore.Max_Extra_Lives > 0)
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command101);
                    }
                }
                else
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command11);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command12);
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Hardcore.Command128);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ReservedSlots.Command69);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Day7.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Day7.Command16);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Mute.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Mute.Command13);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Mute.Command14);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Mute.Command119);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (_commands.Length >= 0)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CustomCommandList(ClientInfo _cInfo)
        {
            if (Dict.Count > 0)
            {
                string _commands = "Custom commands:";
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    string _h = kvp.Value[1];
                    bool _result;
                    if (bool.TryParse(_h, out _result))
                    {
                        if (!_result)
                        {
                            string _c = kvp.Key;
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, _c);
                            if (_commands.Length >= 100)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                _commands = "Custom commands:";
                            }
                        }
                    }
                }
                if (_commands != "Custom commands:")
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            string _commands = "Admin commands:";
            if (AdminChat.IsEnabled)
            {
                _commands = string.Format("{0} @" + AdminChat.Command118, _commands);
            }
            if (Jail.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, Jail.Command27);
            }
            if (_commands != "Admin commands:")
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static string GetChatCommandsAdmin(ClientInfo _cInfo)
        {
            string _commandsAdmin = string.Format("{0}Admin commands are:", LoadConfig.Chat_Response_Color);
            if (AdminChat.IsEnabled && GameManager.Instance.adminTools.IsAdmin(_cInfo))
            {
                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= ChatHook.Mod_Level)
                {
                    if (AdminChat.IsEnabled)
                    {
                        _commandsAdmin = string.Format("{0} @" + AdminChat.Command118, _commandsAdmin);
                    }
                    if (Jail.IsEnabled)
                    {
                        _commandsAdmin = string.Format("{0} {1}{2}", _commandsAdmin, ChatHook.Command_Private, Jail.Command27);
                    }
                }
            }
            return _commandsAdmin;
        }

        public static void Exec(ClientInfo _cInfo, string _message)
        {
            string[] _c;
            if (Dict.TryGetValue(_message, out _c))
            {
                int[] _c1;
                if (Dict1.TryGetValue(_message, out _c1))
                {
                    if (_c1[0] > 20 || _c1[1] <= 0)
                    {
                        Permission(_cInfo, _message, _c, _c1);
                    }
                    else
                    {
                        DateTime _lastUse = DateTime.Now;
                        if (_c1[0] == 1)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand1;
                        }
                        else if (_c1[0] == 2)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand2;
                        }
                        else if (_c1[0] == 3)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand3;
                        }
                        else if (_c1[0] == 4)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand4;
                        }
                        else if (_c1[0] == 5)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand5;
                        }
                        else if (_c1[0] == 6)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand6;
                        }
                        else if (_c1[0] == 7)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand7;
                        }
                        else if (_c1[0] == 8)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand8;
                        }
                        else if (_c1[0] == 9)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand9;
                        }
                        else if (_c1[0] == 10)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand10;
                        }
                        else if (_c1[0] == 11)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand11;
                        }
                        else if (_c1[0] == 12)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand12;
                        }
                        else if (_c1[0] == 13)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand13;
                        }
                        else if (_c1[0] == 14)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand14;
                        }
                        else if (_c1[0] == 15)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand15;
                        }
                        else if (_c1[0] == 16)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand16;
                        }
                        else if (_c1[0] == 17)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand17;
                        }
                        else if (_c1[0] == 18)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand18;
                        }
                        else if (_c1[0] == 19)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand19;
                        }
                        else if (_c1[0] == 20)
                        {
                            _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand20;
                        }
                        Delay(_cInfo, _message, _c, _c1, _lastUse);
                    }
                }
            }
        }

        public static void Delay(ClientInfo _cInfo, string _message, string[] _c, int[] _c1, DateTime _lastUse)
        {
            TimeSpan varTime = DateTime.Now - _lastUse;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timePassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        int _newDelay = _c1[1] / 2;
                        TimePass(_cInfo, _message, _timePassed, _newDelay, _c, _c1);
                        return;
                    }
                }
            }
            TimePass(_cInfo, _message, _timePassed, _c1[1], _c, _c1);
        }

        public static void TimePass(ClientInfo _cInfo, string _message, int _timePassed, int _delay, string[] _c, int[] _c1)
        {
            if (_timePassed >= _delay)
            {
                Permission(_cInfo, _message, _c, _c1);
            }
            else
            {
                int _timeleft = _delay - _timePassed;
                Response(_cInfo, _message, _timeleft, _delay);
            }
        }


        public static void Response(ClientInfo _cInfo, string _message, int _timeLeft, int _newDelay)
        {
            string _phrase616;
            if (!Phrases.Dict.TryGetValue(616, out _phrase616))
            {
                _phrase616 = "You can only use {CommandPrivate}{Command15} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
            }
            _phrase616 = _phrase616.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase616 = _phrase616.Replace("{Command15}", Command15);
            _phrase616 = _phrase616.Replace("{DelayBetweenUses}", _newDelay.ToString());
            _phrase616 = _phrase616.Replace("{TimeRemaining}", _timeLeft.ToString());
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase616 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void Permission(ClientInfo _cInfo, string _message, string[] _c, int[] _c1)
        {
            if (_c[2].ToLower() == "true")
            {
                string[] _command = { _message };
                if (GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo))
                {
                    CommandCost(_cInfo, _message, _c1);
                }
                else
                {
                    string _phrase827;
                    if (!Phrases.Dict.TryGetValue(827, out _phrase827))
                    {
                        _phrase827 = "You do not have permission to use {Command}.";
                    }
                    _phrase827 = _phrase827.Replace("{Command}", _message);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase827 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                CommandCost(_cInfo, _message, _c1);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _message, int[] _c1)
        {
            if (Wallet.IsEnabled && _c1[2] > 0)
            {
                int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                if (_currentCoins >= _c1[2])
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _c1[2]);
                    CommandDelay(_cInfo, _message, _c1);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "You do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                CommandDelay(_cInfo, _message, _c1);
            }
        }

        public static void CommandDelay(ClientInfo _cInfo, string _message, int[] _c1)
        {
            try
            {
                if (_c1[0] < 21 && _c1[1] > 0)
                {
                    if (_c1[0] == 1)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand1 = DateTime.Now;
                    }
                    else if (_c1[0] == 2)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand2 = DateTime.Now;
                    }
                    else if (_c1[0] == 3)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand3 = DateTime.Now;
                    }
                    else if (_c1[0] == 4)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand4 = DateTime.Now;
                    }
                    else if (_c1[0] == 5)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand5 = DateTime.Now;
                    }
                    else if (_c1[0] == 6)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand6 = DateTime.Now;
                    }
                    else if (_c1[0] == 7)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand7 = DateTime.Now;
                    }
                    else if (_c1[0] == 8)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand8 = DateTime.Now;
                    }
                    else if (_c1[0] == 9)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand9 = DateTime.Now;
                    }
                    else if (_c1[0] == 10)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand10 = DateTime.Now;
                    }
                    else if (_c1[0] == 11)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand11 = DateTime.Now;
                    }
                    else if (_c1[0] == 12)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand12 = DateTime.Now;
                    }
                    else if (_c1[0] == 13)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand13 = DateTime.Now;
                    }
                    else if (_c1[0] == 14)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand14 = DateTime.Now;
                    }
                    else if (_c1[0] == 15)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand15 = DateTime.Now;
                    }
                    else if (_c1[0] == 16)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand16 = DateTime.Now;
                    }
                    else if (_c1[0] == 17)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand17 = DateTime.Now;
                    }
                    else if (_c1[0] == 18)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand18 = DateTime.Now;
                    }
                    else if (_c1[0] == 19)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand19 = DateTime.Now;
                    }
                    else if (_c1[0] == 20)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommand20 = DateTime.Now;
                    }
                    PersistentContainer.Instance.Save();
                }
                string[] _r;
                if (Dict.TryGetValue(_message, out _r))
                {
                    string[] _commandsSplit = _r[0].Split('^');
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
                                int _time = 5;
                                int.TryParse(_timeString, out _time);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandDelay: {0}.", e));
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
                                int _time = 5;
                                int.TryParse(_timeString, out _time);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.DelayedCommand: {0}.", e));
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
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _command + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (_command.ToLower().StartsWith("whisper "))
                    {
                        _command = _command.Replace("Whisper ", "");
                        _command = _command.Replace("whisper ", "");
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _command + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else if (_command.StartsWith("tele ") || _command.StartsWith("tp ") || _command.StartsWith("teleportplayer "))
                    {
                        SdtdConsole.Instance.ExecuteSync(_command, null);
                        if (Zones.IsEnabled && Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                        {
                            Zones.ZoneExit.Remove(_cInfo.entityId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandExec: {0}.", e));
            }
        }
    }
}