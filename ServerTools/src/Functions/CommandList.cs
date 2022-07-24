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
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
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
                            File.Delete(FilePath);
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
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing CommandList.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in CommandList.LoadXml: {0}", e.Message));
                }
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
                sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
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
                    if (Commands.Count > 0)
                    {
                        for (int i = 0; i < Commands.Count; i++)
                        {
                            sw.WriteLine(Commands[i]);
                        }
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
            if (!File.Exists(FilePath))
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
                    switch (kvp.Key)
                    {
                        case "home":
                            Homes.Command_home = kvp.Value;
                            continue;
                        case "fhome":
                            Homes.Command_fhome = kvp.Value;
                            continue;
                        case "home save":
                            Homes.Command_save = kvp.Value;
                            continue;
                        case "home del":
                            Homes.Command_delete = kvp.Value;
                            continue;
                        case "go home":
                            Homes.Command_go = kvp.Value;
                            continue;
                        case "sethome":
                            Homes.Command_set = kvp.Value;
                            continue;
                        case "go way":
                            Waypoints.Command_go_way = kvp.Value;
                            continue;
                        case "top3":
                            Hardcore.Command_top3 = kvp.Value;
                            continue;
                        case "score":
                            Hardcore.Command_score = kvp.Value;
                            continue;
                        case "buy life":
                            Hardcore.Command_buy_life = kvp.Value;
                            continue;
                        case "hardcore":
                            Hardcore.Command_hardcore = kvp.Value;
                            continue;
                        case "hardcore on":
                            Hardcore.Command_hardcore_on = kvp.Value;
                            continue;
                        case "mute":
                            Mute.Command_mute = kvp.Value;
                            continue;
                        case "unmute":
                            Mute.Command_unmute = kvp.Value;
                            continue;
                        case "mutelist":
                            Mute.Command_mutelist = kvp.Value;
                            continue;
                        case "commands":
                            PersistentOperations.Command_commands = kvp.Value;
                            continue;
                        case "day7":
                            Day7.Command_day7 = kvp.Value;
                            continue;
                        case "day":
                            Day7.Command_day = kvp.Value;
                            continue;
                        case "bloodmoon":
                            Bloodmoon.Command_bloodmoon = kvp.Value;
                            continue;
                        case "bm":
                            Bloodmoon.Command_bm = kvp.Value;
                            continue;
                        case "killme":
                            Suicide.Command_killme = kvp.Value;
                            continue;
                        case "wrist":
                            Suicide.Command_wrist = kvp.Value;
                            continue;
                        case "hang":
                            Suicide.Command_hang = kvp.Value;
                            continue;
                        case "suicide":
                            Suicide.Command_suicide = kvp.Value;
                            continue;
                        case "gimme":
                            Gimme.Command_gimme = kvp.Value;
                            continue;
                        case "gimmie":
                            Gimme.Command_gimmie = kvp.Value;
                            continue;
                        case "setjail":
                            Jail.Command_set = kvp.Value;
                            continue;
                        case "forgive":
                            Jail.Command_forgive = kvp.Value;
                            continue;
                        case "setspawn":
                            NewSpawnTele.Command_setspawn = kvp.Value;
                            continue;
                        case "ready":
                            NewSpawnTele.Command_ready = kvp.Value;
                            continue;
                        case "trackanimal":
                            AnimalTracking.Command_trackanimal = kvp.Value;
                            continue;
                        case "track":
                            AnimalTracking.Command_track = kvp.Value;
                            continue;
                        case "claim":
                            FirstClaimBlock.Command_claim = kvp.Value;
                            continue;
                        case "clan add":
                            ClanManager.Command_add = kvp.Value;
                            continue;
                        case "clan del":
                            ClanManager.Command_delete = kvp.Value;
                            continue;
                        case "clan invite":
                            ClanManager.Command_invite = kvp.Value;
                            continue;
                        case "clan accept":
                            ClanManager.Command_accept = kvp.Value;
                            continue;
                        case "clan decline":
                            ClanManager.Command_decline = kvp.Value;
                            continue;
                        case "clan remove":
                            ClanManager.Command_remove = kvp.Value;
                            continue;
                        case "clan promote":
                            ClanManager.Command_promote = kvp.Value;
                            continue;
                        case "clan demote":
                            ClanManager.Command_demote = kvp.Value;
                            continue;
                        case "clan leave":
                            ClanManager.Command_leave = kvp.Value;
                            continue;
                        case "clan commands":
                            ClanManager.Command_commands = kvp.Value;
                            continue;
                        case "clan chat":
                            ClanManager.Command_chat = kvp.Value;
                            continue;
                        case "clan rename":
                            ClanManager.Command_rename = kvp.Value;
                            continue;
                        case "clan request":
                            ClanManager.Command_request = kvp.Value;
                            continue;
                        case "clan cc":
                            ClanManager.Command_cc = kvp.Value;
                            continue;
                        case "clan list":
                            ClanManager.Command_clan_list = kvp.Value;
                            continue;
                        case "reward":
                            Voting.Command_reward = kvp.Value;
                            continue;
                        case "vote":
                            Voting.Command_vote = kvp.Value;
                            continue;
                        case "shutdown":
                            Shutdown.Command_shutdown = kvp.Value;
                            continue;
                        case "adminlist":
                            AdminList.Command_adminlist = kvp.Value;
                            continue;
                        case "travel":
                            Travel.Command_travel = kvp.Value;
                            continue;
                        case "marketback":
                            Market.Command_marketback = kvp.Value;
                            continue;
                        case "mback":
                            Market.Command_mback = kvp.Value;
                            continue;
                        case "setmarket":
                            Market.Command_set = kvp.Value;
                            continue;
                        case "market":
                            Market.Command_market = kvp.Value;
                            continue;
                        case "lobbyback":
                            Lobby.Command_lobbyback = kvp.Value;
                            continue;
                        case "lback":
                            Lobby.Command_lobbyback = kvp.Value;
                            continue;
                        case "setlobby":
                            Lobby.Command_set = kvp.Value;
                            continue;
                        case "lobby":
                            Lobby.Command_lobby = kvp.Value;
                            continue;
                        case "shop":
                            Shop.Command_shop = kvp.Value;
                            continue;
                        case "shop buy":
                            Shop.Command_shop_buy = kvp.Value;
                            continue;
                        case "friend":
                            FriendTeleport.Command_friend = kvp.Value;
                            continue;
                        case "accept":
                            FriendTeleport.Command_accept = kvp.Value;
                            continue;
                        case "died":
                            Died.Command_died = kvp.Value;
                            continue;
                        case "restartvote":
                            RestartVote.Command_restartvote = kvp.Value;
                            continue;
                        case "mutevote":
                            MuteVote.Command_mutevote = kvp.Value;
                            continue;
                        case "kickvote":
                            KickVote.Command_kickvote = kvp.Value;
                            continue;
                        case "yes":
                            RestartVote.Command_yes = kvp.Value;
                            continue;
                        case "auction":
                            Auction.Command_auction = kvp.Value;
                            continue;
                        case "auction cancel":
                            Auction.Command_auction_cancel = kvp.Value;
                            continue;
                        case "auction buy":
                            Auction.Command_auction_buy = kvp.Value;
                            continue;
                        case "auction sell":
                            Auction.Command_auction_sell = kvp.Value;
                            continue;
                        case "fps":
                            Fps.Command_fps = kvp.Value;
                            continue;
                        case "loc":
                            Loc.Command_loc = kvp.Value;
                            continue;
                        case "vehicle":
                            VehicleRecall.Command_vehicle = kvp.Value;
                            continue;
                        case "vehicle save":
                            VehicleRecall.Command_vehicle_save = kvp.Value;
                            continue;
                        case "vehicle remove":
                            VehicleRecall.Command_vehicle_remove = kvp.Value;
                            continue;
                        case "report":
                            Report.Command_report = kvp.Value;
                            continue;
                        case "bounty":
                            Bounties.Command_bounty = kvp.Value;
                            continue;
                        case "lottery":
                            Lottery.Command_lottery = kvp.Value;
                            continue;
                        case "lottery enter":
                            Lottery.Command_lottery_enter = kvp.Value;
                            continue;
                        case "playerlist":
                            PlayerList.Command_playerlist = kvp.Value;
                            continue;
                        case "plist":
                            PlayerList.Command_plist = kvp.Value;
                            continue;
                        case "stuck":
                            Stuck.Command_stuck = kvp.Value;
                            continue;
                        case "poll yes":
                            Poll.Command_poll_yes = kvp.Value;
                            continue;
                        case "poll no":
                            Poll.Command_poll_no = kvp.Value;
                            continue;
                        case "poll":
                            Poll.Command_poll = kvp.Value;
                            continue;
                        case "bank":
                            Bank.Command_bank = kvp.Value;
                            continue;
                        case "deposit":
                            Bank.Command_deposit = kvp.Value;
                            continue;
                        case "withdraw":
                            Bank.Command_withdraw = kvp.Value;
                            continue;
                        case "transfer":
                            Bank.Command_transfer = kvp.Value;
                            continue;
                        case "join":
                            Event.Command_join = kvp.Value;
                            continue;
                        case "infoticker":
                            InfoTicker.Command_infoticker = kvp.Value;
                            continue;
                        case "session":
                            Session.Command_session = kvp.Value;
                            continue;
                        case "waypoint":
                            Waypoints.Command_waypoint = kvp.Value;
                            continue;
                        case "way":
                            Waypoints.Command_way = kvp.Value;
                            continue;
                        case "wp":
                            Waypoints.Command_wp = kvp.Value;
                            continue;
                        case "fwaypoint":
                            Waypoints.Command_fwaypoint = kvp.Value;
                            continue;
                        case "fway":
                            Waypoints.Command_fway = kvp.Value;
                            continue;
                        case "fwp":
                            Waypoints.Command_fwp = kvp.Value;
                            continue;
                        case "waypoint save":
                            Waypoints.Command_waypoint_save = kvp.Value;
                            continue;
                        case "way save":
                            Waypoints.Command_way_save = kvp.Value;
                            continue;
                        case "ws":
                            Waypoints.Command_ws = kvp.Value;
                            continue;
                        case "waypoint del":
                            Waypoints.Command_waypoint_del = kvp.Value;
                            continue;
                        case "way del":
                            Waypoints.Command_way_del = kvp.Value;
                            continue;
                        case "wd":
                            Waypoints.Command_wd = kvp.Value;
                            continue;
                        case "admin":
                            AdminChat.Command_admin = kvp.Value;
                            continue;
                        case "pmessage":
                            Whisper.Command_pmessage = kvp.Value;
                            continue;
                        case "pm":
                            Whisper.Command_pm = kvp.Value;
                            continue;
                        case "rmessage":
                            Whisper.Command_rmessage = kvp.Value;
                            continue;
                        case "rm":
                            Whisper.Command_rm = kvp.Value;
                            continue;
                        case "pray":
                            Prayer.Command_pray = kvp.Value;
                            continue;
                        case "scoutplayer":
                            ScoutPlayer.Command_scoutplayer = kvp.Value;
                            continue;
                        case "scout":
                            ScoutPlayer.Command_scout = kvp.Value;
                            continue;
                        case "exit":
                            ExitCommand.Command_exit = kvp.Value;
                            continue;
                        case "quit":
                            ExitCommand.Command_quit = kvp.Value;
                            continue;
                        case "ccc":
                            ChatColor.Command_ccc = kvp.Value;
                            continue;
                        case "ccpr":
                            ChatColor.Command_ccpr = kvp.Value;
                            continue;
                        case "ccnr":
                            ChatColor.Command_ccnr = kvp.Value;
                            continue;
                        case "gamble":
                            Gamble.Command_gamble = kvp.Value;
                            continue;
                        case "gamble bet":
                            Gamble.Command_gamble_bet = kvp.Value;
                            continue;
                        case "gamble payout":
                            Gamble.Command_gamble_payout = kvp.Value;
                            continue;
                        case "party":
                            AutoPartyInvite.Command_party = kvp.Value;
                            continue;
                        case "party add":
                            AutoPartyInvite.Command_party_add = kvp.Value;
                            continue;
                        case "party remove":
                            AutoPartyInvite.Command_party_remove = kvp.Value;
                            continue;
                        case "expire":
                            PersistentOperations.Command_expire = kvp.Value;
                            continue;
                        case "pickup":
                            BlockPickup.Command_pickup = kvp.Value;
                            continue;
                        case "wall":
                            Wall.Command_wall = kvp.Value;
                            continue;
                        case "bed":
                            Bed.Command_bed = kvp.Value;
                            continue;
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
            Commands.Add("    <Command Default=\"setjail\" Replacement=\"setjail\" />");
            Commands.Add("    <Command Default=\"forgive\" Replacement=\"forgive\" />");
            Commands.Add("    <Command Default=\"setspawn\" Replacement=\"setspawn\" />");
            Commands.Add("    <Command Default=\"ready\" Replacement=\"ready\" />");
            Commands.Add("    <Command Default=\"vote\" Replacement=\"vote\" />");
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
            Commands.Add("    <Command Default=\"shop\" Replacement=\"shop\" />");
            Commands.Add("    <Command Default=\"shop buy\" Replacement=\"shop buy\" />");
            Commands.Add("    <Command Default=\"friend\" Replacement=\"friend\" />");
            Commands.Add("    <Command Default=\"accept\" Replacement=\"accept\" />");
            Commands.Add("    <Command Default=\"died\" Replacement=\"died\" />");
            Commands.Add("    <Command Default=\"restartvote\" Replacement=\"restartvote\" />");
            Commands.Add("    <Command Default=\"mutevote\" Replacement=\"mutevote\" />");
            Commands.Add("    <Command Default=\"kickvote\" Replacement=\"kickvote\" />");
            Commands.Add("    <Command Default=\"yes\" Replacement=\"yes\" />");
            Commands.Add("    <Command Default=\"auction\" Replacement=\"auction\" />");
            Commands.Add("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" />");
            Commands.Add("    <Command Default=\"auction buy\" Replacement=\"auction buy\" />");
            Commands.Add("    <Command Default=\"auction sell\" Replacement=\"auction sell\" />");
            Commands.Add("    <Command Default=\"fps\" Replacement=\"fps\" />");
            Commands.Add("    <Command Default=\"loc\" Replacement=\"loc\" />");
            Commands.Add("    <Command Default=\"vehicle\" Replacement=\"vehicle\" />");
            Commands.Add("    <Command Default=\"vehicle save\" Replacement=\"vehicle save\" />");
            Commands.Add("    <Command Default=\"vehicle remove\" Replacement=\"vehicle remove\" />");
            Commands.Add("    <Command Default=\"report\" Replacement=\"report\" />");
            Commands.Add("    <Command Default=\"bounty\" Replacement=\"bounty\" />");
            Commands.Add("    <Command Default=\"lottery\" Replacement=\"lottery\" />");
            Commands.Add("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" />");
            Commands.Add("    <Command Default=\"playerlist\" Replacement=\"playerlist\" />");
            Commands.Add("    <Command Default=\"plist\" Replacement=\"plist\" />");
            Commands.Add("    <Command Default=\"stuck\" Replacement=\"stuck\" />");
            Commands.Add("    <Command Default=\"poll yes\" Replacement=\"poll yes\" />");
            Commands.Add("    <Command Default=\"poll no\" Replacement=\"poll no\" />");
            Commands.Add("    <Command Default=\"poll\" Replacement=\"poll\" />");
            Commands.Add("    <Command Default=\"bank\" Replacement=\"bank\" />");
            Commands.Add("    <Command Default=\"deposit\" Replacement=\"deposit\" />");
            Commands.Add("    <Command Default=\"withdraw\" Replacement=\"withdraw\" />");
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
            Commands.Add("    <Command Default=\"expire\" Replacement=\"expire\" />");
            Commands.Add("    <Command Default=\"pickup\" Replacement=\"pickup\" />");
            Commands.Add("    <Command Default=\"wall\" Replacement=\"wall\" />");
            Commands.Add("    <Command Default=\"bed\" Replacement=\"bed\" />");
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    List<string> commandList = Commands;
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CommandList>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                    sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
                    if (OldNodeList != null)
                    {
                        for (int i = 0; i < OldNodeList.Count; i++)
                        {
                            if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- All capital letters in commands") &&
                                !OldNodeList[i].OuterXml.Contains("<!-- Leave the default alone."))
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
                                if (line.HasAttributes && line.Name == "Command")
                                {
                                    string defaultCommand = "", replacement = "";
                                    if (line.HasAttribute("Default") && line.HasAttribute("Replacement"))
                                    {
                                        defaultCommand = line.GetAttribute("Default");
                                        replacement = line.GetAttribute("Replacement");
                                        if (commandList.Count > 0)
                                        {
                                            for (int j = 0; j < commandList.Count; j++)
                                            {
                                                if (commandList[j].Contains(defaultCommand))
                                                {
                                                    commandList.RemoveAt(j);
                                                    sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" />", defaultCommand, replacement));
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (commandList.Count > 0)
                    {
                        for (int i = 0; i < commandList.Count; i++)
                        {
                            sw.WriteLine(commandList[i]);
                        }
                    }
                    sw.WriteLine("</CommandList>");
                    sw.Flush();
                    sw.Close();
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
