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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                            }
                            else if (_line.HasAttribute("Trigger") && _line.HasAttribute("Command") && _line.HasAttribute("DelayBetweenUses") && _line.HasAttribute("Hidden") &&
                                _line.HasAttribute("Permission") && _line.HasAttribute("Cost"))
                            {
                                string _trigger = _line.GetAttribute("Trigger");
                                string _command = _line.GetAttribute("Command");
                                string _stringDelay = _line.GetAttribute("DelayBetweenUses");
                                string _hidden = _line.GetAttribute("Hidden");
                                string _permission = _line.GetAttribute("Permission");
                                string _stringCost = _line.GetAttribute("Cost");
                                string[] _c = { _command, _stringDelay, _hidden, _permission, _stringCost };
                                if (!Dict.ContainsKey(_trigger))
                                {
                                    Dict.Add(_trigger, _c);
                                }
                                if (_permission.ToLower() == "true" && !GameManager.Instance.adminTools.GetCommands().ContainsKey(_trigger))
                                {
                                    GameManager.Instance.adminTools.AddCommandPermission(_trigger, 0);
                                }
                            }
                        }
                    }
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<!-- possible variables {EntityId} {SteamId} {PlayerName} {Delay} -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Custom Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Permission=\"{4}\" Cost=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Custom Trigger=\"help\" Command=\"global Type " + ChatHook.Chat_Command_Prefix1 + Command_commands + " for a list of chat commands.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"info\" Command=\"global Server Info: \" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"rules\" Command=\"whisper Visit YourSiteHere to see the rules.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"website\" Command =\"whisper Visit YourSiteHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"teamspeak\" Command=\"whisper The Teamspeak3 info is YourInfoHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"spawnz\" Command=\"st-ser {EntityId} r.40 4 11 17 ^ whisper Zombies have spawn around you.\" DelayBetweenUses=\"60\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"discord\" Command=\"whisper The discord channel is ...\" DelayBetweenUses=\"20\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc8\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc9\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc10\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc11\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc12\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc13\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc14\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc15\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc16\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc17\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc18\" Command=\"First command ^ Second command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc19\" Command=\"First command ^ {Delay} 30 ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
                        sw.WriteLine("    <Custom Trigger=\"cc20\" Command=\"First command ^ Second command ^ Third Command\" DelayBetweenUses=\"0\" Hidden=\"false\" Permission=\"false\" Cost=\"0\" />");
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
                if (Wallet.IsEnabled)
                {
                    if (Wallet.Command_wallet != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Wallet.Command_wallet);
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
                if (Animals.IsEnabled)
                {
                    if (Animals.Command_trackanimal != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command_trackanimal);
                    }
                    if (Animals.Command_track != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Animals.Command_track);
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
                if (WeatherVote.IsEnabled)
                {
                    if (WeatherVote.Command_weathervote != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_weathervote);
                    }
                    if (WeatherVote.VoteOpen)
                    {
                        if (WeatherVote.Command_sun != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_sun);
                        }
                        if (WeatherVote.Command_rain != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_rain);
                        }
                        if (WeatherVote.Command_snow != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, WeatherVote.Command_snow);
                        }
                    }
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
                    if (Bank.Command_deposit != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_deposit);
                    }
                    if (Bank.Command_withdraw != "")
                    {
                        _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_withdraw);
                    }
                    if (Wallet.IsEnabled)
                    {
                        if (Bank.Command_wallet_deposit != "")
                        {
                            _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_wallet_deposit);
                        }
                        if (Bank.Command_wallet_withdraw != "")
                        {
                            _commands = string.Format("{0} {1}{2} #", _commands, ChatHook.Chat_Command_Prefix1, Bank.Command_wallet_withdraw);
                        }
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
                if (VehicleTeleport.IsEnabled)
                {
                    if (VehicleTeleport.Bike)
                    {
                        if (VehicleTeleport.Command_bike != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_bike);
                        }
                    }
                    if (VehicleTeleport.Mini_Bike)
                    {
                        if (VehicleTeleport.Command_minibike != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_minibike);
                        }
                    }
                    if (VehicleTeleport.Motor_Bike)
                    {
                        if (VehicleTeleport.Command_motorbike != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_motorbike);
                        }
                    }
                    if (VehicleTeleport.Jeep)
                    {
                        if (VehicleTeleport.Command_jeep != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_jeep);
                        }
                    }
                    if (VehicleTeleport.Gyro)
                    {
                        if (VehicleTeleport.Command_gyro != "")
                        {
                            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, VehicleTeleport.Command_gyro);
                        }
                    }
                }
                if (_commands.Length >= 100)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    _commands = "";
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
                if (_commands.Length >= 0)
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
                    string _commands = "";
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        if (bool.TryParse(kvp.Value[1], out bool _result))
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message));
            }
        }

        public static void AdminCommandList(ClientInfo _cInfo)
        {
            try
            {
                string _commands = "";
                if (AdminChat.IsEnabled)
                {
                    _commands = string.Format("{0} @" + AdminChat.Command_admin, _commands);
                }
                if (Jail.IsEnabled)
                {
                    if (Jail.Command_jail != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Jail.Command_jail);
                    }
                    if (Jail.Command_unjail != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Jail.Command_unjail);
                    }
                }
                if (NewSpawnTele.IsEnabled)
                {
                    if (NewSpawnTele.Command_setspawn != "")
                    {
                        _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, NewSpawnTele.Command_setspawn);
                    }
                }
                if (_commands.Length >= 0)
                {
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                if (Dict.TryGetValue(_command, out string[] _c))
                {
                    int.TryParse(_c[1], out int _delay);
                    int.TryParse(_c[4], out int _cost);
                    bool _permission = bool.Parse(_c[2]);
                    if (_permission && !Permission(_cInfo, _command))
                    {
                        return;
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays.ContainsKey(_command))
                    {
                        DateTime _lastUse = PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays[_command];
                        Delay(_cInfo, _command, _delay, _cost, _lastUse);
                    }
                    else
                    {
                        Delay(_cInfo, _command, _delay, _cost, DateTime.MinValue);
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
                string[] _commands = { _command };
                if (GameManager.Instance.adminTools.CommandAllowedFor(_commands, _cInfo))
                {
                    return true;
                }
                else
                {
                    Phrases.Dict.TryGetValue("CustomCommands2", out string _phrase);
                    _phrase = _phrase.Replace("{Command}", _command);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.Permission: {0}", e.Message));
            }
            return false;
        }

        private static void Delay(ClientInfo _cInfo, string _message, int _delay, int _cost, DateTime _lastUse)
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
                        int _newDelay = _delay / 2;
                        TimePass(_cInfo, _message, _timePassed, _newDelay, _cost);
                        return;
                    }
                }
            }
            TimePass(_cInfo, _message, _timePassed, _delay, _cost);
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
                Phrases.Dict.TryGetValue("CustomCommands1", out string _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{CommandCustom}", _message);
                _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase = _phrase.Replace("{TimeRemaining}", _timeLeft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                    if (_currentCoins >= _cost)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                        ProcessCommand(_cInfo, _command);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("CustomCommands3", out string _phrase);
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Dictionary<string, DateTime> _delays = new Dictionary<string, DateTime>
                    {
                        { _command, DateTime.Now }
                    };
                    PersistentContainer.Instance.Players[_cInfo.playerId].CustomCommandDelays = _delays;
                }
                PersistentContainer.DataChange = true;
                if (Dict.TryGetValue(_command, out string[] _commandData))
                {
                    if (_commandData[1].Contains("^"))
                    {
                        List<string> _commands = _commandData[1].Split('^').ToList();
                        for (int i = 0; i < _commands.Count; i++)
                        {
                            string _commandTrimmed = _commands[i].Trim();
                            if (_commandTrimmed.StartsWith("{Delay}"))
                            {
                                string[] _commandSplit = _commandTrimmed.Split(' ');
                                if (int.TryParse(_commandSplit[1], out int _time))
                                {
                                    _commands.RemoveRange(0, i + 1);
                                    Timers.Custom_SingleUseTimer(_time, _cInfo.playerId, _commands);
                                    return;
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _command));
                                }
                            }
                            else
                            {
                                CommandExec(_cInfo, _commandTrimmed);
                            }
                        }
                    }
                    else
                    {
                        CommandExec(_cInfo, _commandData[1]);
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
                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_playerId);
                if (_cInfo != null)
                {
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string _commandTrimmed = _commands[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] _commandSplit = _commandTrimmed.Split(' ');
                            if (int.TryParse(_commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Custom_SingleUseTimer(_time, _cInfo.playerId, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _commands));
                            }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CustomCommandDelayed: {0}", e.Message));
            }
        }

        private static void CommandExec(ClientInfo _cInfo, string _command)
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

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CustomCommands>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- possible variables {EntityId} {SteamId} {PlayerName} {Delay} -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Command")
                        {
                            string _trigger = "", _command = "", _delay = "", _hidden = "", _permission = "", _cost = "";
                            if (_line.HasAttribute("Trigger"))
                            {
                                _trigger = _line.GetAttribute("Trigger");
                            }
                            if (_line.HasAttribute("Command"))
                            {
                                _command = _line.GetAttribute("Command");
                            }
                            if (_line.HasAttribute("DelayBetweenUses"))
                            {
                                _delay = _line.GetAttribute("DelayBetweenUses");
                            }
                            if (_line.HasAttribute("Hidden"))
                            {
                                _hidden = _line.GetAttribute("Hidden");
                            }
                            if (_line.HasAttribute("Permission"))
                            {
                                _permission = _line.GetAttribute("Permission");
                            }
                            if (_line.HasAttribute("Cost"))
                            {
                                _cost = _line.GetAttribute("Cost");
                            }
                            sw.WriteLine(string.Format("    <Command Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Permission=\"{4}\" Cost=\"{5}\" />", _trigger, _command, _delay, _hidden, _permission, _cost));
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
            LoadXml();
        }
    }
}