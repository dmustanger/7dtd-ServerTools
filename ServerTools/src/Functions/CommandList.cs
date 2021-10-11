using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class CommandList
    {
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private static List<string> Commands = new List<string>();
        private const string file = "CommandList.xml";
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
        }

        public static void LoadXml()
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
            if (childNodes != null && childNodes.Count > 0)
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
                            else if (line.HasAttribute("Default") && line.HasAttribute("Replacement"))
                            {
                                string _default = line.GetAttribute("Default");
                                string replacement = line.GetAttribute("Replacement").ToLower();
                                if (!Dict.ContainsKey(_default))
                                {
                                    Dict.Add(_default, replacement);
                                }
                            }
                        }
                    }
                }
                if (Dict.Count > 0)
                {
                    Exec();
                }
            }
            if (childNodes != null && upgrade)
            {
                UpgradeXml(childNodes);
                return;
            }
        }

        private static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<CommandList>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("<!-- Leave the default alone. Only edit the replacement to your desired command -->");
                sw.WriteLine("<!-- All capital letters in commands will be reduced to lowercase -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    for (int i = 0; i < Commands.Count; i++)
                    {
                        sw.WriteLine(Commands[i]);
                    }
                }
                sw.WriteLine("</CommandList>");
                sw.Flush();
                sw.Close();
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec()
        {
            try
            {
                foreach (KeyValuePair<string, string> kvp in Dict)
                {
                    if (kvp.Key == "home")
                    {
                        Homes.Command_home = kvp.Value;
                    }
                    else if (kvp.Key == "fhome")
                    {
                        Homes.Command_fhome = kvp.Value;
                    }
                    else if (kvp.Key == "home save")
                    {
                        Homes.Command_save = kvp.Value;
                    }
                    else if (kvp.Key == "home del")
                    {
                        Homes.Command_delete = kvp.Value;
                    }
                    else if (kvp.Key == "go home")
                    {
                        Homes.Command_go = kvp.Value;
                    }
                    else if (kvp.Key == "sethome")
                    {
                        Homes.Command_set = kvp.Value;
                    }
                    else if (kvp.Key == "go way")
                    {
                        Waypoints.Command_go_way = kvp.Value;
                    }
                    else if (kvp.Key == "top3")
                    {
                        Hardcore.Command_top3 = kvp.Value;
                    }
                    else if (kvp.Key == "score")
                    {
                        Hardcore.Command_score = kvp.Value;
                    }
                    else if (kvp.Key == "buy life")
                    {
                        Hardcore.Command_buy_life = kvp.Value;
                    }
                    else if (kvp.Key == "hardcore")
                    {
                        Hardcore.Command_hardcore = kvp.Value;
                    }
                    else if (kvp.Key == "hardcore on")
                    {
                        Hardcore.Command_hardcore_on = kvp.Value;
                    }
                    else if (kvp.Key == "mute")
                    {
                        Mute.Command_mute = kvp.Value;
                    }
                    else if (kvp.Key == "unmute")
                    {
                        Mute.Command_unmute = kvp.Value;
                    }
                    else if (kvp.Key == "mutelist")
                    {
                        Mute.Command_mutelist = kvp.Value;
                    }
                    else if (kvp.Key == "commands")
                    {
                        CustomCommands.Command_commands = kvp.Value;
                    }
                    else if (kvp.Key == "day7")
                    {
                        Day7.Command_day7 = kvp.Value;
                    }
                    else if (kvp.Key == "day")
                    {
                        Day7.Command_day = kvp.Value;
                    }
                    else if (kvp.Key == "bloodmoon")
                    {
                        Bloodmoon.Command_bloodmoon = kvp.Value;
                    }
                    else if (kvp.Key == "bm")
                    {
                        Bloodmoon.Command_bm = kvp.Value;
                    }
                    else if (kvp.Key == "killme")
                    {
                        Suicide.Command_killme = kvp.Value;
                    }
                    else if (kvp.Key == "wrist")
                    {
                        Suicide.Command_wrist = kvp.Value;
                    }
                    else if (kvp.Key == "hang")
                    {
                        Suicide.Command_hang = kvp.Value;
                    }
                    else if (kvp.Key == "suicide")
                    {
                        Suicide.Command_suicide = kvp.Value;
                    }
                    else if (kvp.Key == "gimme")
                    {
                        Gimme.Command_gimme = kvp.Value;
                    }
                    else if (kvp.Key == "gimmie")
                    {
                        Gimme.Command_gimmie = kvp.Value;
                    }
                    else if (kvp.Key == "set jail")
                    {
                        Jail.Command_set_jail = kvp.Value;
                    }
                    else if (kvp.Key == "jail")
                    {
                        Jail.Command_jail = kvp.Value;
                    }
                    else if (kvp.Key == "unjail")
                    {
                        Jail.Command_unjail = kvp.Value;
                    }
                    else if (kvp.Key == "forgive")
                    {
                        Jail.Command_forgive = kvp.Value;
                    }
                    else if (kvp.Key == "setspawn")
                    {
                        NewSpawnTele.Command_setspawn = kvp.Value;
                    }
                    else if (kvp.Key == "ready")
                    {
                        NewSpawnTele.Command_ready = kvp.Value;
                    }
                    else if (kvp.Key == "trackanimal")
                    {
                        AnimalTracking.Command_trackanimal = kvp.Value;
                    }
                    else if (kvp.Key == "track")
                    {
                        AnimalTracking.Command_track = kvp.Value;
                    }
                    else if (kvp.Key == "claim")
                    {
                        FirstClaimBlock.Command_claim = kvp.Value;
                    }
                    else if (kvp.Key == "clan add")
                    {
                        ClanManager.Command_add = kvp.Value;
                    }
                    else if (kvp.Key == "clan del")
                    {
                        ClanManager.Command_delete = kvp.Value;
                    }
                    else if (kvp.Key == "clan invite")
                    {
                        ClanManager.Command_invite = kvp.Value;
                    }
                    else if (kvp.Key == "clan accept")
                    {
                        ClanManager.Command_accept = kvp.Value;
                    }
                    else if (kvp.Key == "clan decline")
                    {
                        ClanManager.Command_decline = kvp.Value;
                    }
                    else if (kvp.Key == "clan remove")
                    {
                        ClanManager.Command_remove = kvp.Value;
                    }
                    else if (kvp.Key == "clan promote")
                    {
                        ClanManager.Command_promote = kvp.Value;
                    }
                    else if (kvp.Key == "clan demote")
                    {
                        ClanManager.Command_demote = kvp.Value;
                    }
                    else if (kvp.Key == "clan leave")
                    {
                        ClanManager.Command_leave = kvp.Value;
                    }
                    else if (kvp.Key == "clan commands")
                    {
                        ClanManager.Command_commands = kvp.Value;
                    }
                    else if (kvp.Key == "clan chat")
                    {
                        ClanManager.Command_chat = kvp.Value;
                    }
                    else if (kvp.Key == "clan rename")
                    {
                        ClanManager.Command_rename = kvp.Value;
                    }
                    else if (kvp.Key == "clan request")
                    {
                        ClanManager.Command_request = kvp.Value;
                    }
                    else if (kvp.Key == "cc")
                    {
                        ClanManager.Command_chat = kvp.Value;
                    }
                    else if (kvp.Key == "clan list")
                    {
                        ClanManager.Command_clan_list = kvp.Value;
                    }
                    else if (kvp.Key == "reward")
                    {
                        VoteReward.Command_reward = kvp.Value;
                    }
                    else if (kvp.Key == "shutdown")
                    {
                        Shutdown.Command_shutdown = kvp.Value;
                    }
                    else if (kvp.Key == "adminlist")
                    {
                        AdminList.Command_adminlist = kvp.Value;
                    }
                    else if (kvp.Key == "travel")
                    {
                        Travel.Command_travel = kvp.Value;
                    }
                    else if (kvp.Key == "marketback")
                    {
                        Market.Command_marketback = kvp.Value;
                    }
                    else if (kvp.Key == "mback")
                    {
                        Market.Command_mback = kvp.Value;
                    }
                    else if (kvp.Key == "setmarket")
                    {
                        Market.Command_set = kvp.Value;
                    }
                    else if (kvp.Key == "market")
                    {
                        Market.Command_market = kvp.Value;
                    }
                    else if (kvp.Key == "lobbyback")
                    {
                        Lobby.Command_lobbyback = kvp.Value;
                    }
                    else if (kvp.Key == "lback")
                    {
                        Lobby.Command_lback = kvp.Value;
                    }
                    else if (kvp.Key == "setlobby")
                    {
                        Lobby.Command_set = kvp.Value;
                    }
                    else if (kvp.Key == "lobby")
                    {
                        Lobby.Command_lobby = kvp.Value;
                    }
                    else if (kvp.Key == "wallet")
                    {
                        Wallet.Command_wallet = kvp.Value;
                    }
                    else if (kvp.Key == "shop")
                    {
                        Shop.Command_shop = kvp.Value;
                    }
                    else if (kvp.Key == "shop buy")
                    {
                        Shop.Command_shop_buy = kvp.Value;
                    }
                    else if (kvp.Key == "friend")
                    {
                        FriendTeleport.Command_friend = kvp.Value;
                    }
                    else if (kvp.Key == "accept")
                    {
                        FriendTeleport.Command_accept = kvp.Value;
                    }
                    else if (kvp.Key == "died")
                    {
                        Died.Command_died = kvp.Value;
                    }
                    else if (kvp.Key == "weathervote")
                    {
                        WeatherVote.Command_weathervote = kvp.Value;
                    }
                    else if (kvp.Key == "sun")
                    {
                        WeatherVote.Command_sun = kvp.Value;
                    }
                    else if (kvp.Key == "rain")
                    {
                        WeatherVote.Command_rain = kvp.Value;
                    }
                    else if (kvp.Key == "snow")
                    {
                        WeatherVote.Command_snow = kvp.Value;
                    }
                    else if (kvp.Key == "restartvote")
                    {
                        RestartVote.Command_restartvote = kvp.Value;
                    }
                    else if (kvp.Key == "mutevote")
                    {
                        MuteVote.Command_mutevote = kvp.Value;
                    }
                    else if (kvp.Key == "kickvote")
                    {
                        KickVote.Command_kickvote = kvp.Value;
                    }
                    else if (kvp.Key == "yes")
                    {
                        RestartVote.Command_yes = kvp.Value;
                    }
                    else if (kvp.Key == "reserved")
                    {
                        ReservedSlots.Command_reserved = kvp.Value;
                    }
                    else if (kvp.Key == "auction")
                    {
                        Auction.Command_auction = kvp.Value;
                    }
                    else if (kvp.Key == "auction cancel")
                    {
                        Auction.Command_auction_cancel = kvp.Value;
                    }
                    else if (kvp.Key == "auction buy")
                    {
                        Auction.Command_auction_buy = kvp.Value;
                    }
                    else if (kvp.Key == "auction sell")
                    {
                        Auction.Command_auction_sell = kvp.Value;
                    }
                    else if (kvp.Key == "fps")
                    {
                        Fps.Command_fps = kvp.Value;
                    }
                    else if (kvp.Key == "loc")
                    {
                        Loc.Command_loc = kvp.Value;
                    }
                    else if (kvp.Key == "recall")
                    {
                        VehicleRecall.Command_recall = kvp.Value;
                    }
                    else if (kvp.Key == "report")
                    {
                        Report.Command_report = kvp.Value;
                    }
                    else if (kvp.Key == "bounty")
                    {
                        Bounties.Command_bounty = kvp.Value;
                    }
                    else if (kvp.Key == "lottery")
                    {
                        Lottery.Command_lottery = kvp.Value;
                    }
                    else if (kvp.Key == "lottery enter")
                    {
                        Lottery.Command_lottery_enter = kvp.Value;
                    }
                    else if (kvp.Key == "playerlist")
                    {
                        PlayerList.Command_playerlist = kvp.Value;
                    }
                    else if (kvp.Key == "stuck")
                    {
                        Stuck.Command_stuck = kvp.Value;
                    }
                    else if (kvp.Key == "poll yes")
                    {
                        Poll.Command_poll_yes = kvp.Value;
                    }
                    else if (kvp.Key == "poll no")
                    {
                        Poll.Command_poll_no = kvp.Value;
                    }
                    else if (kvp.Key == "poll")
                    {
                        Poll.Command_poll = kvp.Value;
                    }
                    else if (kvp.Key == "bank")
                    {
                        Bank.Command_bank = kvp.Value;
                    }
                    else if (kvp.Key == "deposit")
                    {
                        Bank.Command_deposit = kvp.Value;
                    }
                    else if (kvp.Key == "withdraw")
                    {
                        Bank.Command_withdraw = kvp.Value;
                    }
                    else if (kvp.Key == "wallet deposit")
                    {
                        Bank.Command_wallet_deposit = kvp.Value;
                    }
                    else if (kvp.Key == "wallet withdraw")
                    {
                        Bank.Command_wallet_withdraw = kvp.Value;
                    }
                    else if (kvp.Key == "transfer")
                    {
                        Bank.Command_transfer = kvp.Value;
                    }
                    else if (kvp.Key == "join")
                    {
                        Event.Command_join = kvp.Value;
                    }
                    else if (kvp.Key == "infoticker")
                    {
                        InfoTicker.Command_infoticker = kvp.Value;
                    }
                    else if (kvp.Key == "session")
                    {
                        Session.Command_session = kvp.Value;
                    }
                    else if (kvp.Key == "waypoint")
                    {
                        Waypoints.Command_waypoint = kvp.Value;
                    }
                    else if (kvp.Key == "way")
                    {
                        Waypoints.Command_way = kvp.Value;
                    }
                    else if (kvp.Key == "wp")
                    {
                        Waypoints.Command_wp = kvp.Value;
                    }
                    else if (kvp.Key == "fwaypoint")
                    {
                        Waypoints.Command_fwaypoint = kvp.Value;
                    }
                    else if (kvp.Key == "fway")
                    {
                        Waypoints.Command_fway = kvp.Value;
                    }
                    else if (kvp.Key == "fwp")
                    {
                        Waypoints.Command_fwp = kvp.Value;
                    }
                    else if (kvp.Key == "waypoint save")
                    {
                        Waypoints.Command_waypoint_save = kvp.Value;
                    }
                    else if (kvp.Key == "way save")
                    {
                        Waypoints.Command_way_save = kvp.Value;
                    }
                    else if (kvp.Key == "ws")
                    {
                        Waypoints.Command_ws = kvp.Value;
                    }
                    else if (kvp.Key == "waypoint del")
                    {
                        Waypoints.Command_waypoint_del = kvp.Value;
                    }
                    else if (kvp.Key == "way del")
                    {
                        Waypoints.Command_way_del = kvp.Value;
                    }
                    else if (kvp.Key == "wd")
                    {
                        Waypoints.Command_wd = kvp.Value;
                    }
                    else if (kvp.Key == "admin")
                    {
                        AdminChat.Command_admin = kvp.Value;
                    }
                    else if (kvp.Key == "pmessage")
                    {
                        Whisper.Command_pmessage = kvp.Value;
                    }
                    else if (kvp.Key == "pm")
                    {
                        Whisper.Command_pm = kvp.Value;
                    }
                    else if (kvp.Key == "rmessage")
                    {
                        Whisper.Command_rmessage = kvp.Value;
                    }
                    else if (kvp.Key == "rm")
                    {
                        Whisper.Command_rm = kvp.Value;
                    }
                    else if (kvp.Key == "pray")
                    {
                        Prayer.Command_pray = kvp.Value;
                    }
                    else if (kvp.Key == "scoutplayer")
                    {
                        ScoutPlayer.Command_scoutplayer = kvp.Value;
                    }
                    else if (kvp.Key == "scout")
                    {
                        ScoutPlayer.Command_scout = kvp.Value;
                    }
                    else if (kvp.Key == "exit")
                    {
                        ExitCommand.Command_exit = kvp.Value;
                    }
                    else if (kvp.Key == "quit")
                    {
                        ExitCommand.Command_quit = kvp.Value;
                    }
                    else if (kvp.Key == "ccc")
                    {
                        ChatColor.Command_ccc = kvp.Value;
                    }
                    else if (kvp.Key == "ccpr")
                    {
                        ChatColor.Command_ccpr = kvp.Value;
                    }
                    else if (kvp.Key == "ccnr")
                    {
                        ChatColor.Command_ccnr = kvp.Value;
                    }
                    else if (kvp.Key == "gamble")
                    {
                        Gamble.Command_gamble = kvp.Value;
                    }
                    else if (kvp.Key == "gamble bet")
                    {
                        Gamble.Command_gamble_bet = kvp.Value;
                    }
                    else if (kvp.Key == "gamble payout")
                    {
                        Gamble.Command_gamble_payout = kvp.Value;
                    }
                    else if (kvp.Key == "party")
                    {
                        AutoPartyInvite.Command_party = kvp.Value;
                    }
                    else if (kvp.Key == "party add")
                    {
                        AutoPartyInvite.Command_party_add = kvp.Value;
                    }
                    else if (kvp.Key == "party remove")
                    {
                        AutoPartyInvite.Command_party_remove = kvp.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandList.Exec: {0}", e.Message));
            }
        }

        public static void BuildList()
        {
            Commands.Add("    <Command Default=\"home\" Replacement=\"home\" />");
            Commands.Add("    <Command Default=\"fhome\" Replacement=\"fhome\" />");
            Commands.Add("    <Command Default=\"home save\" Replacement=\"home save\" />");
            Commands.Add("    <Command Default=\"home del\" Replacement=\"home del\" />");
            Commands.Add("    <Command Default=\"go home\" Replacement=\"go home\" />");
            Commands.Add("    <Command Default=\"sethome\" Replacement=\"sethome\" />");
            Commands.Add("    <Command Default=\"go way\" Replacement=\"go way\" />");
            Commands.Add("    <Command Default=\"top3\" Replacement=\"top3\" />");
            Commands.Add("    <Command Default=\"score\" Replacement=\"score\" />");
            Commands.Add("    <Command Default=\"buy life\" Replacement=\"buy life\" />");
            Commands.Add("    <Command Default=\"hardcore\" Replacement=\"hardcore\" />");
            Commands.Add("    <Command Default=\"hardcore on\" Replacement=\"hardcore on\" />");
            Commands.Add("    <Command Default=\"mute\" Replacement=\"mute\" />");
            Commands.Add("    <Command Default=\"unmute\" Replacement=\"unmute\" />");
            Commands.Add("    <Command Default=\"mutelist\" Replacement=\"mutelist\" />");
            Commands.Add("    <Command Default=\"commands\" Replacement=\"commands\" />");
            Commands.Add("    <Command Default=\"day7\" Replacement=\"day7\" />");
            Commands.Add("    <Command Default=\"day\" Replacement=\"day\" />");
            Commands.Add("    <Command Default=\"bloodmoon\" Replacement=\"bloodmoon\" />");
            Commands.Add("    <Command Default=\"bm\" Replacement=\"bm\" />");
            Commands.Add("    <Command Default=\"killme\" Replacement=\"killme\" />");
            Commands.Add("    <Command Default=\"wrist\" Replacement=\"wrist\" />");
            Commands.Add("    <Command Default=\"hang\" Replacement=\"hang\" />");
            Commands.Add("    <Command Default=\"suicide\" Replacement=\"suicide\" />");
            Commands.Add("    <Command Default=\"gimme\" Replacement=\"gimme\" />");
            Commands.Add("    <Command Default=\"gimmie\" Replacement=\"gimmie\" />");
            Commands.Add("    <Command Default=\"set jail\" Replacement=\"set jail\" />");
            Commands.Add("    <Command Default=\"jail\" Replacement=\"jail\" />");
            Commands.Add("    <Command Default=\"unjail\" Replacement=\"unjail\" />");
            Commands.Add("    <Command Default=\"forgive\" Replacement=\"forgive\" />");
            Commands.Add("    <Command Default=\"setspawn\" Replacement=\"setspawn\" />");
            Commands.Add("    <Command Default=\"ready\" Replacement=\"ready\" />");
            Commands.Add("    <Command Default=\"trackanimal\" Replacement=\"trackanimal\" />");
            Commands.Add("    <Command Default=\"track\" Replacement=\"track\" />");
            Commands.Add("    <Command Default=\"claim\" Replacement=\"claim\" />");
            Commands.Add("    <Command Default=\"clan add\" Replacement=\"clan add\" />");
            Commands.Add("    <Command Default=\"clan del\" Replacement=\"clan del\" />");
            Commands.Add("    <Command Default=\"clan invite\" Replacement=\"clan invite\" />");
            Commands.Add("    <Command Default=\"clan accept\" Replacement=\"clan accept\" />");
            Commands.Add("    <Command Default=\"clan decline\" Replacement=\"clan decline\" />");
            Commands.Add("    <Command Default=\"clan remove\" Replacement=\"clan remove\" />");
            Commands.Add("    <Command Default=\"clan promote\" Replacement=\"clan promote\" />");
            Commands.Add("    <Command Default=\"clan demote\" Replacement=\"clan demote\" />");
            Commands.Add("    <Command Default=\"clan leave\" Replacement=\"clan leave\" />");
            Commands.Add("    <Command Default=\"clan commands\" Replacement=\"clan commands\" />");
            Commands.Add("    <Command Default=\"clan chat\" Replacement=\"clan chat\" />");
            Commands.Add("    <Command Default=\"clan rename\" Replacement=\"clan rename\" />");
            Commands.Add("    <Command Default=\"clan request\" Replacement=\"clan request\" />");
            Commands.Add("    <Command Default=\"cc\" Replacement=\"cc\" />");
            Commands.Add("    <Command Default=\"clanlist\" Replacement=\"clanlist\" />");
            Commands.Add("    <Command Default=\"reward\" Replacement=\"reward\" />");
            Commands.Add("    <Command Default=\"shutdown\" Replacement=\"shutdown\" />");
            Commands.Add("    <Command Default=\"adminlist\" Replacement=\"adminlist\" />");
            Commands.Add("    <Command Default=\"travel\" Replacement=\"travel\" />");
            Commands.Add("    <Command Default=\"marketback\" Replacement=\"marketback\" />");
            Commands.Add("    <Command Default=\"mback\" Replacement=\"mback\" />");
            Commands.Add("    <Command Default=\"setmarket\" Replacement=\"setmarket\" />");
            Commands.Add("    <Command Default=\"market\" Replacement=\"market\" />");
            Commands.Add("    <Command Default=\"lobbyback\" Replacement=\"lobbyback\" />");
            Commands.Add("    <Command Default=\"lback\" Replacement=\"lback\" />");
            Commands.Add("    <Command Default=\"setlobby\" Replacement=\"setlobby\" />");
            Commands.Add("    <Command Default=\"lobby\" Replacement=\"lobby\" />");
            Commands.Add("    <Command Default=\"wallet\" Replacement=\"wallet\" />");
            Commands.Add("    <Command Default=\"shop\" Replacement=\"shop\" />");
            Commands.Add("    <Command Default=\"shop buy\" Replacement=\"shop buy\" />");
            Commands.Add("    <Command Default=\"friend\" Replacement=\"friend\" />");
            Commands.Add("    <Command Default=\"accept\" Replacement=\"accept\" />");
            Commands.Add("    <Command Default=\"died\" Replacement=\"died\" />");
            Commands.Add("    <Command Default=\"weathervote\" Replacement=\"weathervote\" />");
            Commands.Add("    <Command Default=\"sun\" Replacement=\"sun\" />");
            Commands.Add("    <Command Default=\"rain\" Replacement=\"rain\" />");
            Commands.Add("    <Command Default=\"snow\" Replacement=\"snow\" />");
            Commands.Add("    <Command Default=\"restartvote\" Replacement=\"restartvote\" />");
            Commands.Add("    <Command Default=\"mutevote\" Replacement=\"mutevote\" />");
            Commands.Add("    <Command Default=\"kickvote\" Replacement=\"kickvote\" />");
            Commands.Add("    <Command Default=\"yes\" Replacement=\"yes\" />");
            Commands.Add("    <Command Default=\"reserved\" Replacement=\"reserved\" />");
            Commands.Add("    <Command Default=\"auction\" Replacement=\"auction\" />");
            Commands.Add("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" />");
            Commands.Add("    <Command Default=\"auction buy\" Replacement=\"auction buy\" />");
            Commands.Add("    <Command Default=\"auction sell\" Replacement=\"auction sell\" />");
            Commands.Add("    <Command Default=\"fps\" Replacement=\"fps\" />");
            Commands.Add("    <Command Default=\"loc\" Replacement=\"loc\" />");
            Commands.Add("    <Command Default=\"recall\" Replacement=\"recall\" />");
            Commands.Add("    <Command Default=\"report\" Replacement=\"report\" />");
            Commands.Add("    <Command Default=\"bounty\" Replacement=\"bounty\" />");
            Commands.Add("    <Command Default=\"lottery\" Replacement=\"lottery\" />");
            Commands.Add("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" />");
            Commands.Add("    <Command Default=\"playerlist\" Replacement=\"playerlist\" />");
            Commands.Add("    <Command Default=\"stuck\" Replacement=\"stuck\" />");
            Commands.Add("    <Command Default=\"poll yes\" Replacement=\"poll yes\" />");
            Commands.Add("    <Command Default=\"poll no\" Replacement=\"poll no\" />");
            Commands.Add("    <Command Default=\"poll\" Replacement=\"poll\" />");
            Commands.Add("    <Command Default=\"bank\" Replacement=\"bank\" />");
            Commands.Add("    <Command Default=\"deposit\" Replacement=\"deposit\" />");
            Commands.Add("    <Command Default=\"withdraw\" Replacement=\"withdraw\" />");
            Commands.Add("    <Command Default=\"wallet deposit\" Replacement=\"wallet deposit\" />");
            Commands.Add("    <Command Default=\"wallet withdraw\" Replacement=\"wallet withdraw\" />");
            Commands.Add("    <Command Default=\"transfer\" Replacement=\"transfer\" />");
            Commands.Add("    <Command Default=\"join\" Replacement=\"event\" />");
            Commands.Add("    <Command Default=\"infoticker\" Replacement=\"infoticker\" />");
            Commands.Add("    <Command Default=\"session\" Replacement=\"session\" />");
            Commands.Add("    <Command Default=\"waypoint\" Replacement=\"waypoint\" />");
            Commands.Add("    <Command Default=\"way\" Replacement=\"way\" />");
            Commands.Add("    <Command Default=\"wp\" Replacement=\"wp\" />");
            Commands.Add("    <Command Default=\"fwaypoint\" Replacement=\"fwaypoint\" />");
            Commands.Add("    <Command Default=\"fway\" Replacement=\"fway\" />");
            Commands.Add("    <Command Default=\"fwp\" Replacement=\"fwp\" />");
            Commands.Add("    <Command Default=\"waypoint save\" Replacement=\"waypoint save\" />");
            Commands.Add("    <Command Default=\"way save\" Replacement=\"way save\" />");
            Commands.Add("    <Command Default=\"ws\" Replacement=\"ws\" />");
            Commands.Add("    <Command Default=\"waypoint del\" Replacement=\"waypoint del\" />");
            Commands.Add("    <Command Default=\"way del\" Replacement=\"way del\" />");
            Commands.Add("    <Command Default=\"wd\" Replacement=\"wd\" />");
            Commands.Add("    <Command Default=\"admin\" Replacement=\"admin\" />");
            Commands.Add("    <Command Default=\"pmessage\" Replacement=\"pmessage\" />");
            Commands.Add("    <Command Default=\"pm\" Replacement=\"pm\" />");
            Commands.Add("    <Command Default=\"rmessage\" Replacement=\"rmessage\" />");
            Commands.Add("    <Command Default=\"rm\" Replacement=\"rm\" />");
            Commands.Add("    <Command Default=\"pray\" Replacement=\"pray\" />");
            Commands.Add("    <Command Default=\"scoutplayer\" Replacement=\"scoutplayer\" />");
            Commands.Add("    <Command Default=\"scout\" Replacement=\"scout\" />");
            Commands.Add("    <Command Default=\"exit\" Replacement=\"exit\" />");
            Commands.Add("    <Command Default=\"quit\" Replacement=\"quit\" />");
            Commands.Add("    <Command Default=\"ccc\" Replacement=\"ccc\" />");
            Commands.Add("    <Command Default=\"ccpr\" Replacement=\"ccpr\" />");
            Commands.Add("    <Command Default=\"ccnr\" Replacement=\"ccnr\" />");
            Commands.Add("    <Command Default=\"gamble\" Replacement=\"gamble\" />");
            Commands.Add("    <Command Default=\"gamble bet\" Replacement=\"gamble bet\" />");
            Commands.Add("    <Command Default=\"gamble payout\" Replacement=\"gamble payout\" />");
            Commands.Add("    <Command Default=\"party\" Replacement=\"party\" />");
            Commands.Add("    <Command Default=\"party add\" Replacement=\"party add\" />");
            Commands.Add("    <Command Default=\"party remove\" Replacement=\"party remove\" />");
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CommandList>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Leave the default alone. Only edit the replacement to your desired command -->");
                    sw.WriteLine("<!-- All capital letters in commands will be reduced to lowercase -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- All capital letters in commands") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- Leave the default alone."))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)_oldChildNodes[i];
                            if (line.HasAttributes && line.OuterXml.Contains("Command"))
                            {
                                string defaultCommand = "", replacement = "";
                                if (line.HasAttribute("Default") && line.HasAttribute("Replacement"))
                                {
                                    defaultCommand = line.GetAttribute("Default");
                                    replacement = line.GetAttribute("Replacement");
                                    if (Commands.Count > 0)
                                    {
                                        for (int j = 0; j < Commands.Count; j++)
                                        {
                                            if (Commands[j].Contains(defaultCommand))
                                            {
                                                Commands.RemoveAt(j);
                                                sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" />", defaultCommand, replacement));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Commands.Count > 0)
                    {
                        for (int i = 0; i < Commands.Count; i++)
                        {
                            sw.WriteLine(Commands);
                        }
                        Commands.Clear();
                    }
                    sw.WriteLine("</CommandList>");
                    sw.Flush();
                    sw.Close();
                    BuildList();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandList.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
