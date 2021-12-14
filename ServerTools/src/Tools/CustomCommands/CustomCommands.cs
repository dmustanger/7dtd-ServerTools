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
        public static List<int> TeleportCheckProtection = new List<int>();

        private const string file = "CustomCommands.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
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
                                else if (line.HasAttribute("Trigger") && line.HasAttribute("Command") && line.HasAttribute("DelayBetweenUses") && line.HasAttribute("Hidden") &&
                                    line.HasAttribute("Reserved") && line.HasAttribute("Permission") && line.HasAttribute("Cost"))
                                {
                                    string trigger = line.GetAttribute("Trigger").ToLower();
                                    string command = line.GetAttribute("Command");
                                    string delay = line.GetAttribute("DelayBetweenUses");
                                    string hidden = line.GetAttribute("Hidden");
                                    if (!bool.TryParse(line.GetAttribute("Reserved").ToLower(), out bool reserved))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (true/false) value for 'Reserved' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!bool.TryParse(line.GetAttribute("Permission").ToLower(), out bool permission))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (true/false) value for 'Permission' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("Cost"), out int cost))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (non-numeric) value for 'Cost' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    string[] c = { command, delay, hidden, reserved.ToString(), permission.ToString(), cost.ToString() };
                                    if (!Dict.ContainsKey(trigger))
                                    {
                                        Dict.Add(trigger, c);
                                        if (!GameManager.Instance.adminTools.GetCommands().ContainsKey(trigger))
                                        {
                                            GameManager.Instance.adminTools.AddCommandPermission(trigger, 0, true);
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
                            Utils.FileDelete(FilePath);
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
                                    Utils.FileDelete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            Utils.FileDelete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing CustomCommands.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    Utils.FileDelete(FilePath);
                    UpdateXml();
                    Log.Out(string.Format("[SERVERTOOLS] CustomCommands.xml has been created for version {0}", Config.Version));
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<CustomCommands>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {SteamId}, {PlayerName}, {Delay}, {RandomPlayerId}, whisper, global -->");
                    sw.WriteLine("    <!-- <Custom Trigger=\"Example\" Command=\"whisper Server Info... ^ whisper You have triggered the example\" DelayBetweenUses=\"0\" Hidden=\"false\" Reserved=\"false\" Permission=\"false\" Cost=\"0\" /> -->");
                    sw.WriteLine("    <!-- <Custom Trigger=\"\" Command=\"\" DelayBetweenUses=\"\" Hidden=\"\" Reserved=\"\" Permission=\"\" Cost=\"\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Custom Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Reserved=\"{4}\" Permission=\"{5}\" Cost=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], kvp.Value[5]));
                        }
                    }
                    sw.WriteLine("</CustomCommands>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void CommandList(ClientInfo _cInfo)
        {
            try
            {
                string _commands = "";
                if (FriendTeleport.IsEnabled)
                {
                    if (FriendTeleport.Command_friend != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_friend);
                    }
                    if (FriendTeleport.Command_accept != "" && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FriendTeleport.Command_accept);
                    }
                }
                if (Shop.IsEnabled)
                {
                    if (Shop.Command_shop != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop);
                    }
                    if (Shop.Command_shop_buy != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shop.Command_shop_buy);
                    }
                }
                if (Gimme.IsEnabled)
                {
                    if (Gimme.Command_gimme != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Gimme.Command_gimme);
                    }
                }
                if (Homes.IsEnabled)
                {
                    if (Homes.Command_home != "***")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_home);
                    }
                    if (Homes.Command_fhome != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_fhome);
                    }
                    if (Homes.Command_save != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_save);
                    }
                    if (Homes.Command_delete != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_delete);
                    }
                    if (Homes.Command_go != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_go);
                    }
                    if (Homes.Command_set != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Homes.Command_set);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.Command_chat != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_chat);
                    }
                }
                if (ClanManager.IsEnabled)
                {
                    if (ClanManager.Command_clan_list != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_clan_list);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (ClanManager.IsEnabled && !ClanManager.ClanMember.Contains(_cInfo.playerId))
                {
                    if (ClanManager.Command_request != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ClanManager.Command_request);
                    }
                }
                if (FirstClaimBlock.IsEnabled)
                {
                    if (FirstClaimBlock.Command_claim != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, FirstClaimBlock.Command_claim);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (RestartVote.IsEnabled)
                {
                    if (RestartVote.Command_restartvote != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, RestartVote.Command_restartvote);
                    }
                }
                if (AnimalTracking.IsEnabled)
                {
                    if (AnimalTracking.Command_trackanimal != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_trackanimal);
                    }
                    if (AnimalTracking.Command_track != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, AnimalTracking.Command_track);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (VoteReward.IsEnabled)
                {
                    if (VoteReward.Command_reward != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VoteReward.Command_reward);
                    }
                }
                if (Shutdown.IsEnabled)
                {
                    if (Shutdown.Command_shutdown != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Shutdown.Command_shutdown);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (AdminList.IsEnabled)
                {
                    if (AdminList.Command_adminlist != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, AdminList.Command_adminlist);
                    }
                }
                if (Travel.IsEnabled)
                {
                    if (Travel.Command_travel != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Travel.Command_travel);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Auction.IsEnabled)
                {
                    if (Auction.Command_auction != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction);
                    }
                    if (Auction.Command_auction_cancel != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_cancel);
                    }
                    if (Auction.Command_auction_buy != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_buy);
                    }
                    if (Auction.Command_auction_sell != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Auction.Command_auction_sell);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Died.IsEnabled)
                {
                    if (Died.Command_died != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Died.Command_died);
                    }
                }
                if (Fps.IsEnabled)
                {
                    if (Fps.Command_fps != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Fps.Command_fps);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Loc.IsEnabled)
                {
                    if (Loc.Command_loc != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Loc.Command_loc);
                    }
                }
                if (MuteVote.IsEnabled && Mute.IsEnabled)
                {
                    if (MuteVote.Command_mutevote != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, MuteVote.Command_mutevote);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (KickVote.IsEnabled)
                {
                    if (KickVote.Command_kickvote != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, KickVote.Command_kickvote);
                    }
                }
                if (Suicide.IsEnabled)
                {
                    if (Suicide.Command_killme != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_killme);
                    }
                    if (Suicide.Command_suicide != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Suicide.Command_suicide);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Lobby.IsEnabled)
                {
                    if (Lobby.Command_lobby != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobby);
                    }
                    if (Lobby.Return && Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                    {
                        if (Lobby.Command_lobbyback != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lobby.Command_lobbyback);
                        }
                    }
                }
                if (Bounties.IsEnabled)
                {
                    if (Bounties.Command_bounty != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                    }
                    if (Bounties.Command_bounty != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bounties.Command_bounty);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Lottery.IsEnabled)
                {
                    if (Lottery.Command_lottery != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                    }
                    if (Lottery.Command_lottery != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery);
                    }
                    if (Lottery.Command_lottery_enter != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Lottery.Command_lottery_enter);
                    }
                }
                if (Report.IsEnabled)
                {
                    if (Report.Command_report != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Report.Command_report);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Stuck.IsEnabled)
                {
                    if (Stuck.Command_stuck != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Stuck.Command_stuck);
                    }
                }
                if (Bank.IsEnabled)
                {
                    if (Bank.Command_bank != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_bank);
                    }
                    if (Wallet.IsEnabled && Bank.Command_deposit != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                    }
                    if (Wallet.IsEnabled && Bank.Command_withdraw != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                    }
                    if (Bank.Player_Transfers)
                    {
                        if (Bank.Command_transfer != "")
                        {
                            _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_transfer);
                        }
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Market.IsEnabled)
                {
                    if (Market.Command_market != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command_market);
                    }
                    if (Market.Return && Market.MarketPlayers.Contains(_cInfo.entityId))
                    {
                        if (Market.Command_marketback != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Market.Command_marketback);
                        }
                    }
                }
                if (InfoTicker.IsEnabled)
                {
                    if (InfoTicker.Command_infoticker != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, InfoTicker.Command_infoticker);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Waypoints.IsEnabled)
                {
                    if (Waypoints.Command_waypoint != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                    }
                    if (Waypoints.Command_waypoint != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint);
                    }
                    if (Waypoints.Command_waypoint_save != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_save);
                    }
                    if (Waypoints.Command_waypoint_del != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_waypoint_del);
                    }
                    if (Waypoints.Command_fwaypoint != "")
                    {
                        _commands = string.Format("{0} {1}{2} 'name'", _commands, ChatHook.Chat_Command_Prefix1, Waypoints.Command_fwaypoint);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (VehicleRecall.IsEnabled)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleRecall.Command_recall);
                }
                if (PlayerList.IsEnabled)
                {
                    if (PlayerList.Command_playerlist != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, PlayerList.Command_playerlist);
                    }
                }
                if (Bloodmoon.IsEnabled)
                {
                    if (Bloodmoon.Command_bloodmoon != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Bloodmoon.Command_bloodmoon);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Whisper.IsEnabled)
                {
                    if (Whisper.Command_rmessage != "")
                    {
                        _commands = string.Format("{0} {1}{2} {3}{4}", _commands, ChatHook.Chat_Command_Prefix1, Whisper.Command_pmessage, ChatHook.Chat_Command_Prefix1, Whisper.Command_rmessage);
                    }
                }
                if (Hardcore.IsEnabled)
                {
                    if (!Hardcore.Optional)
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                        }
                        if (Hardcore.Command_score != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                        }
                        if (Hardcore.Command_hardcore != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                        }
                        if (Hardcore.Max_Extra_Lives > 0)
                        {
                            if (Hardcore.Command_buy_life != "")
                            {
                                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                            }
                        }
                    }
                    else
                    {
                        if (Hardcore.Command_top3 != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_top3);
                        }
                        if (Hardcore.Command_score != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_score);
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                        {
                            if (Hardcore.Command_hardcore != "")
                            {
                                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore);
                            }
                            if (Hardcore.Max_Extra_Lives > 0)
                            {
                                if (Hardcore.Command_buy_life != "")
                                {
                                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_buy_life);
                                }
                            }
                        }
                        else
                        {
                            if (Hardcore.Command_hardcore_on != "")
                            {
                                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Hardcore.Command_hardcore_on);
                            }
                        }
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    if (ReservedSlots.Command_reserved != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ReservedSlots.Command_reserved);
                    }
                }
                if (Day7.IsEnabled)
                {
                    if (Day7.Command_day7 != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Day7.Command_day7);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (Mute.IsEnabled)
                {
                    if (Mute.Command_mute != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mute);
                    }
                    if (Mute.Command_unmute != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_unmute);
                    }
                    if (Mute.Command_mutelist != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Mute.Command_mutelist);
                    }
                }
                if (Prayer.IsEnabled)
                {
                    if (Prayer.Command_pray != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Prayer.Command_pray);
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
                }
                if (ExitCommand.IsEnabled)
                {
                    _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ExitCommand.Command_exit);
                }
                if (ChatColor.IsEnabled && ChatColor.Players.ContainsKey(_cInfo.playerId))
                {
                    if (ChatColor.Command_ccc != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccc);
                    }
                    if (ChatColor.Rotate)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                        }
                    }
                    if (ChatColor.Custom_Color)
                    {
                        if (ChatColor.Command_ccpr != "")
                        {
                            _commands = string.Format("{0} {1}{2} [******]", _commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccpr);
                        }
                        if (ChatColor.Command_ccnr != "")
                        {
                            _commands = string.Format("{0} {1}{2} [******]", _commands, ChatHook.Chat_Command_Prefix1, ChatColor.Command_ccnr);
                        }
                    }
                }
                if (_commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message));
            }
        }

        public static void CustomCommandList(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    string commands = "";
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        if (bool.TryParse(kvp.Value[2], out bool result))
                        {
                            if (!result)
                            {
                                string c = kvp.Key;
                                commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, c);
                                if (commands.Length >= 100)
                                {
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    commands = "";
                                }
                            }
                        }
                    }
                    if (commands.Length > 0)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message));
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            try
            {
                string commands = "";
                if (AdminChat.IsEnabled)
                {
                    commands = string.Format("{0} @" + AdminChat.Command_admin, commands);
                }
                if (Jail.IsEnabled)
                {
                    if (Jail.Command_jail != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Jail.Command_jail);
                    }
                    if (Jail.Command_unjail != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, Jail.Command_unjail);
                    }
                }
                if (NewSpawnTele.IsEnabled)
                {
                    if (NewSpawnTele.Command_setspawn != "")
                    {
                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, NewSpawnTele.Command_setspawn);
                    }
                }
                if (commands.Length > 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CommandList: {0}", e.Message));
            }
        }

        public static void InitiateCommand(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (Dict.TryGetValue(_command, out string[] c))
                {
                    int.TryParse(c[1], out int delay);
                    int.TryParse(c[4], out int cost);
                    bool permission = bool.Parse(c[3]);
                    if (permission && !Permission(_cInfo, _command))
                    {
                        return; 
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_command))
                    {
                        DateTime lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_command];
                        Delay(_cInfo, _command, delay, cost, lastUse);
                    }
                    else
                    {
                        Delay(_cInfo, _command, delay, cost, DateTime.MinValue);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.ProcessCommand: {0}", e.Message));
            }
        }

        private static bool Permission(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (!GameManager.Instance.adminTools.GetCommands().ContainsKey(_command))
                {
                    GameManager.Instance.adminTools.AddCommandPermission(_command, 0, true);
                }
                string[] commands = { _command };
                if (GameManager.Instance.adminTools.CommandAllowedFor(commands, _cInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.Permission: {0}", e.Message));
            }
            Phrases.Dict.TryGetValue("CustomCommands2", out string phrase);
            phrase = phrase.Replace("{Command}", _command);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            return false;
        }

        private static void Delay(ClientInfo _cInfo, string _message, int _delay, int _cost, DateTime _lastUse)
        {
            TimeSpan varTime = DateTime.Now - _lastUse;
            double fractionalMinutes = varTime.TotalMinutes;
            int timePassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
                    {
                        int newDelay = _delay / 2;
                        TimePass(_cInfo, _message, timePassed, newDelay, _cost);
                        return;
                    }
                }
            }
            TimePass(_cInfo, _message, timePassed, _delay, _cost);
        }

        private static void TimePass(ClientInfo _cInfo, string _message, int _timePassed, int _delay, int _cost)
        {
            if (_timePassed >= _delay)
            {
                CommandCost(_cInfo, _message, _cost);
            }
            else
            {
                int _timeleft = _delay - _timePassed;
                Delayed(_cInfo, _message, _timeleft, _delay);
            }
        }

        private static void Delayed(ClientInfo _cInfo, string _message, int _timeLeft, int _delay)
        {
            try
            {
                Phrases.Dict.TryGetValue("CustomCommands1", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{CommandCustom}", _message);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", _timeLeft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.Delayed: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _command, int _cost)
        {
            try
            {
                if (Wallet.IsEnabled && _cost > 0)
                {
                    int currentCoins = Wallet.GetCurrency(_cInfo.playerId);
                    if (currentCoins >= _cost)
                    {
                        Wallet.RemoveCurrency(_cInfo.playerId, _cost);
                        ProcessCommand(_cInfo, _command);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("CustomCommands3", out string phrase);
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ProcessCommand(_cInfo, _command);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandCost: {0}", e.Message));
            }
        }

        private static void ProcessCommand(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_command))
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_command] = DateTime.Now;
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.Add(_command, DateTime.Now);
                    }
                }
                else
                {
                    Dictionary<string, DateTime> delays = new Dictionary<string, DateTime>
                    {
                        { _command, DateTime.Now }
                    };
                    PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays = delays;
                }
                PersistentContainer.DataChange = true;
                if (Dict.TryGetValue(_command, out string[] commandData))
                {
                    if (commandData[0].Contains("^"))
                    {
                        List<string> commands = commandData[0].Split('^').ToList();
                        for (int i = 0; i < commands.Count; i++)
                        {
                            string commandTrimmed = commands[i].Trim();
                            if (commandTrimmed.StartsWith("{Delay}"))
                            {
                                string[] commandSplit = commandTrimmed.Split(' ');
                                if (int.TryParse(commandSplit[1], out int time))
                                {
                                    commands.RemoveRange(0, i + 1);
                                    Timers.Custom_SingleUseTimer(time, _cInfo.playerId, commands);
                                    return;
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _command));
                                }
                            }
                            else
                            {
                                CommandExec(_cInfo, commandTrimmed);
                            }
                        }
                    }
                    else
                    {
                        CommandExec(_cInfo, commandData[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandDelay: {0}", e.Message));
            }
        }

        public static void CustomCommandDelayed(string _playerId, List<string> _commands)
        {
            try
            {
                ClientInfo cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
                if (cInfo != null)
                {
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string commandTrimmed = _commands[i].Trim();
                        if (commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] commandSplit = commandTrimmed.Split(' ');
                            if (int.TryParse(commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Custom_SingleUseTimer(_time, cInfo.playerId, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _commands));
                            }
                        }
                        else
                        {
                            CommandExec(cInfo, commandTrimmed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CustomCommandDelayed: {0}", e.Message));
            }
        }

        private static void CommandExec(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (_cInfo != null)
                {
                    if (_command.Contains("{EntityId}"))
                    {
                        _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                    }
                    if (_command.Contains("{SteamId}"))
                    {
                        _command = _command.Replace("{SteamId}", _cInfo.playerId);
                    }
                    if (_command.Contains("{PlayerName}"))
                    {
                        _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                    }
                    if (_command.Contains("{RandomPlayerId}"))
                    {
                        List<ClientInfo> clientList = PersistentOperations.ClientList();
                        if (clientList != null)
                        {
                            ClientInfo cInfo2 = clientList.ElementAt(Random.Next(clientList.Count));
                            if (cInfo2 != null)
                            {
                                _command = _command.Replace("{RandomPlayerId}", cInfo2.playerId);
                            }
                        }
                    }
                    string commandLower = _command.ToLower();
                    if (commandLower.StartsWith("global "))
                    {
                        _command = commandLower.Replace("global ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (commandLower.StartsWith("whisper "))
                    {
                        _command = commandLower.Replace("whisper ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else if (commandLower.StartsWith("tele ") || commandLower.StartsWith("teleportplayer ") || commandLower.StartsWith("tp "))
                    {
                        _command = commandLower.Replace("tele ", "teleportplayer ");
                        _command = commandLower.Replace("tp ", "teleportplayer ");
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

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CustomCommands>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {SteamId}, {PlayerName}, {Delay}, {RandomPlayerId}, whisper, global -->");
                    sw.WriteLine("    <!-- <Custom Trigger=\"Example\" Command=\"whisper Server Info... ^ whisper You have triggered the example\" DelayBetweenUses=\"0\" Hidden=\"false\" Reserved=\"false\" Permission=\"false\" Cost=\"0\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Possible variables") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Custom Trigger="))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && (line.Name == "Custom" || line.Name == "Command"))
                            {
                                string trigger = "", command = "", delay = "", hidden = "", reserved = "", permission = "", cost = "";
                                if (line.HasAttribute("Trigger"))
                                {
                                    trigger = line.GetAttribute("Trigger");
                                }
                                if (line.HasAttribute("Command"))
                                {
                                    command = line.GetAttribute("Command");
                                }
                                if (line.HasAttribute("DelayBetweenUses"))
                                {
                                    delay = line.GetAttribute("DelayBetweenUses");
                                }
                                if (line.HasAttribute("Hidden"))
                                {
                                    hidden = line.GetAttribute("Hidden");
                                }
                                if (line.HasAttribute("Reserved"))
                                {
                                    reserved = line.GetAttribute("Reserved");
                                }
                                if (line.HasAttribute("Permission"))
                                {
                                    permission = line.GetAttribute("Permission");
                                }
                                if (line.HasAttribute("Cost"))
                                {
                                    cost = line.GetAttribute("Cost");
                                }
                                sw.WriteLine(string.Format("    <Custom Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Reserved=\"{4}\" Permission=\"{5}\" Cost=\"{6}\" />", trigger, command, delay, hidden, reserved, permission, cost));
                            }
                        }
                    }
                    sw.WriteLine("</CustomCommands>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}