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
        public static string Command_commands = "commands";
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
                    sw.WriteLine("        <Command Trigger=\"help\" Response=\"global Type " + ChatHook.Chat_Command_Prefix1 + Command_commands + " for a list of chat commands.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
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
                if (FriendTeleport.Command_friend != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                }
                if (FriendTeleport.Command_accept != "***" && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_accept);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Wallet.IsEnabled)
            {
                if (Wallet.Command_wallet != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Wallet.Command_wallet);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shop.IsEnabled)
            {
                if (Shop.Command_shop != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop);
                }
                if (Shop.Command_shop_buy != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop_buy);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Gimme.IsEnabled)
            {
                if (Gimme.Command_gimme != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Gimme.Command_gimme);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Homes.IsEnabled)
            {
                if (Homes.Command_home != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home);
                }
                if (Homes.Command_fhome != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_fhome);
                }
                if (Homes.Command_save != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_save);
                }
                if (Homes.Command_delete != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_delete);
                }
                if (Homes.Command_go != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_go);
                }
                if (Homes.Command_set != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_set);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                if (ClanManager.Command_chat != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_chat);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled)
            {
                if (ClanManager.Command_clan_list != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_clan_list);
                }

                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ClanManager.IsEnabled && !ClanManager.ClanMember.Contains(_cInfo.playerId))
            {
                if (ClanManager.Command_request != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_request);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (FirstClaimBlock.IsEnabled)
            {
                if (FirstClaimBlock.Command_claim != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command_claim);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (RestartVote.IsEnabled)
            {
                if (RestartVote.Command_restartvote != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command_restartvote);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Animals.IsEnabled)
            {
                if (Animals.Command_trackanimal != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command_trackanimal);
                }
                if (Animals.Command_track != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command_track);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (VoteReward.IsEnabled)
            {
                if (VoteReward.Command_reward != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VoteReward.Command_reward);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Shutdown.IsEnabled)
            {
                if (Shutdown.Command_shutdown != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command_shutdown);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (AdminList.IsEnabled)
            {
                if (AdminList.Command_adminlist != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, AdminList.Command_adminlist);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Travel.IsEnabled)
            {
                if (Travel.Command_travel != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Travel.Command_travel);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (WeatherVote.IsEnabled)
            {
                if (WeatherVote.Command_weathervote != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_weathervote);
                }
                if (WeatherVote.VoteOpen)
                {
                    if (WeatherVote.Command_sun != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_sun);
                    }
                    if (WeatherVote.Command_rain != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_rain);
                    }
                    if (WeatherVote.Command_snow != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_snow);
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
                if (Auction.Command_auction != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction);
                }
                if (Auction.Command_auction_cancel != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_cancel);
                }
                if (Auction.Command_auction_buy != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_buy);
                }
                if (Auction.Command_auction_sell != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_sell);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Died.IsEnabled)
            {
                if (Died.Command_died != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Died.Command_died);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Fps.IsEnabled)
            {
                if (Fps.Command_fps != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Fps.Command_fps);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Loc.IsEnabled)
            {
                if (Loc.Command_loc != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Loc.Command_loc);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (MuteVote.IsEnabled && Mute.IsEnabled)
            {
                if (MuteVote.Command_mutevote != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command_mutevote);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (KickVote.IsEnabled)
            {
                if (KickVote.Command_kickvote != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, KickVote.Command_kickvote);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Suicide.IsEnabled)
            {
                if (Suicide.Command_killme != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_killme);
                }
                if (Suicide.Command_suicide != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_suicide);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lobby.IsEnabled)
            {
                if (Lobby.Command_lobby != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobby);
                }
                if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                {
                    if (Lobby.Command_lobbyback != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobbyback);
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
                if (Bounties.Command_bounty != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                }
                if (Bounties.Command_bounty != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Lottery.IsEnabled)
            {
                if (Lottery.Command_lottery != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                }
                if (Lottery.Command_lottery != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                }
                if (Lottery.Command_lottery_enter != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery_enter);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Report.IsEnabled)
            {
                if (Report.Command_report != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Report.Command_report);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Stuck.IsEnabled)
            {
                if (Stuck.Command_stuck != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Stuck.Command_stuck);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bank.IsEnabled)
            {
                if (Bank.Command_bank != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_bank);
                }
                if (Bank.Command_deposit != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                }
                if (Bank.Command_withdraw != "***")
                {
                    _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                }
                if (Wallet.IsEnabled)
                {
                    if (Bank.Command_wallet_deposit != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_wallet_deposit);
                    }
                    if (Bank.Command_wallet_withdraw != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_wallet_withdraw);
                    }
                }
                if (Bank.Player_Transfers)
                {
                    if (Bank.Command_transfer != "***")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_transfer);
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
                if (Market.Command_market != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command_market);
                }
                if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                {
                    if (Market.Command_marketback != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command_marketback);
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
                if (InfoTicker.Command_infoticker != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command_infoticker);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Waypoints.IsEnabled)
            {
                if (Waypoints.Command_waypoint != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                }
                if (Waypoints.Command_waypoint != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                }
                if (Waypoints.Command_waypoint_save != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_save);
                }
                if (Waypoints.Command_waypoint_del != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_del);
                }
                if (Waypoints.Command_fwaypoint != "***")
                {
                    _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_fwaypoint);
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
                    if (VehicleTeleport.Command_bike != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_bike);
                    }
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    if (VehicleTeleport.Command_minibike != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_minibike);
                    }
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    if (VehicleTeleport.Command_motorbike != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_motorbike);
                    }
                }
                if (VehicleTeleport.Jeep)
                {
                    if (VehicleTeleport.Command_jeep != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_jeep);
                    }
                }
                if (VehicleTeleport.Gyro)
                {
                    if (VehicleTeleport.Command_gyro != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_gyro);
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
                if (PlayerList.Command_playerlist != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command_playerlist);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Bloodmoon.IsEnabled)
            {
                if (Bloodmoon.Command_bloodmoon != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command_bloodmoon);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Whisper.IsEnabled)
            {
                if (Whisper.Command_rmessage != "***")
                {
                    _commands = string.Format("{0} {1}{2} {3}{4}", _commands, ChatHook.Chat_Command_Prefix1, Whisper.Command_pmessage, ChatHook.Chat_Command_Prefix1, Whisper.Command_rmessage);
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
                    if (Hardcore.Command_top3 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                    }
                    if (Hardcore.Command_score != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                    }
                    if (Hardcore.Command_hardcore != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                    }
                    if (Hardcore.Max_Extra_Lives > 0)
                    {
                        if (Hardcore.Command_buy_life != "***")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                        }
                    }
                }
                else
                {
                    if (Hardcore.Command_top3 != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                    }
                    if (Hardcore.Command_score != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                    {
                        if (Hardcore.Command_hardcore != "***")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                        }
                        if (Hardcore.Max_Extra_Lives > 0)
                        {
                            if (Hardcore.Command_buy_life != "***")
                            {
                                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                            }
                        }
                    }
                    else
                    {
                        if (Hardcore.Command_hardcore_on != "***")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore_on);
                        }
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
                if (ReservedSlots.Command_reserved != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ReservedSlots.Command_reserved);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Day7.IsEnabled)
            {
                if (Day7.Command_day7 != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Day7.Command_day7);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Mute.IsEnabled)
            {
                if (Mute.Command_mute != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mute);
                }
                if (Mute.Command_unmute != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_unmute);
                }
                if (Mute.Command_mutelist != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mutelist);
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (Prayer.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Prayer.Command_pray);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ExitCommand.IsEnabled)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ExitCommand.Command_exit);
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
            }
            if (ChatColorPrefix.IsEnabled && ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
            {
                if (ChatColorPrefix.Command_ccp != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColorPrefix.Command_ccp);
                    if (_commands.Length >= 100)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        _commands = "";
                    }
                }
                if (ChatColorPrefix.Rotate)
                {
                    if (ChatColorPrefix.Command_ccpr != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColorPrefix.Command_ccpr);
                        if (_commands.Length >= 100)
                        {
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            _commands = "";
                        }
                    }
                    if (ChatColorPrefix.Command_ccnr != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColorPrefix.Command_ccnr);
                        if (_commands.Length >= 100)
                        {
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            _commands = "";
                        }
                    }
                }
                if (ChatColorPrefix.Custom_Color)
                {
                    if (ChatColorPrefix.Command_ccpr != "***")
                    {
                        _commands = string.Format("{0} {1}{2} [******]", _commands, ChatHook.Chat_Command_Prefix1, ChatColorPrefix.Command_ccpr);
                        if (_commands.Length >= 100)
                        {
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            _commands = "";
                        }
                    }
                    if (ChatColorPrefix.Command_ccnr != "***")
                    {
                        _commands = string.Format("{0} {1}{2} [******]", _commands, ChatHook.Chat_Command_Prefix1, ChatColorPrefix.Command_ccnr);
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
                _commands = string.Format("{0} @" + AdminChat.Command_admin, _commands);
            }
            if (Jail.IsEnabled)
            {
                if (Jail.Command_jail != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Jail.Command_jail);
                }
                if (Jail.Command_unjail != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Jail.Command_unjail);
                }
            }
            if (NewSpawnTele.IsEnabled)
            {
                if (NewSpawnTele.Command_setspawn != "***")
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, NewSpawnTele.Command_setspawn);
                }
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
                _phrase331 = _phrase331.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
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
                        if (Zones.IsEnabled && Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
                        {
                            Zones.ZonePlayer.Remove(_cInfo.entityId);
                        }
                        SdtdConsole.Instance.ExecuteSync(_command, null);
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