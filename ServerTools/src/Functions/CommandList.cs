using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class CommandList
    {
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Default") || !line.HasAttribute("Replacement") || !line.HasAttribute("Hidden"))
                        {
                            continue;
                        }
                        string defaultCommand = line.GetAttribute("Default");
                        string replacement = line.GetAttribute("Replacement").ToLower();
                        if (defaultCommand == "" || replacement == "")
                        {
                            continue;
                        }
                        if (bool.TryParse(line.GetAttribute("Hidden"), out bool hidden))
                        {
                            if (defaultCommand != "" && replacement != "" && !Dict.ContainsKey(defaultCommand))
                            {
                                Dict.Add(defaultCommand, new string[] { replacement, hidden.ToString() });
                            }
                        }
                    }
                    if (Dict.Count > 0)
                    {
                        Exec();
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
                sw.WriteLine("    <!-- If hidden is set to true, the command will not show in response to using /commands -->");
                sw.WriteLine("    <!-- <Command Default=\"gimme\" Replacement=\"give\" Hidden=\"false\" /> -->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" Hidden=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
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
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    switch (kvp.Key)
                    {
                        case "home":
                            Homes.Command_home = kvp.Value[0];
                            continue;
                        case "fhome":
                            Homes.Command_fhome = kvp.Value[0];
                            continue;
                        case "home save":
                            Homes.Command_save = kvp.Value[0];
                            continue;
                        case "home delete":
                            Homes.Command_delete = kvp.Value[0];
                            continue;
                        case "go home":
                            Homes.Command_go = kvp.Value[0];
                            continue;
                        case "sethome":
                            Homes.Command_set = kvp.Value[0];
                            continue;
                        case "go way":
                            Waypoints.Command_go_way = kvp.Value[0];
                            continue;
                        case "top3":
                            Hardcore.Command_top3 = kvp.Value[0];
                            continue;
                        case "score":
                            Hardcore.Command_score = kvp.Value[0];
                            continue;
                        case "buy life":
                            Hardcore.Command_buy_life = kvp.Value[0];
                            continue;
                        case "hardcore":
                            Hardcore.Command_hardcore = kvp.Value[0];
                            continue;
                        case "hardcore on":
                            Hardcore.Command_hardcore_on = kvp.Value[0];
                            continue;
                        case "mute":
                            Mute.Command_mute = kvp.Value[0];
                            continue;
                        case "unmute":
                            Mute.Command_unmute = kvp.Value[0];
                            continue;
                        case "mutelist":
                            Mute.Command_mutelist = kvp.Value[0];
                            continue;
                        case "commands":
                            GeneralOperations.Command_commands = kvp.Value[0];
                            continue;
                        case "day7":
                            Day7.Command_day7 = kvp.Value[0];
                            continue;
                        case "day":
                            Day7.Command_day = kvp.Value[0];
                            continue;
                        case "bloodmoon":
                            Bloodmoon.Command_bloodmoon = kvp.Value[0];
                            continue;
                        case "bm":
                            Bloodmoon.Command_bm = kvp.Value[0];
                            continue;
                        case "killme":
                            Suicide.Command_killme = kvp.Value[0];
                            continue;
                        case "wrist":
                            Suicide.Command_wrist = kvp.Value[0];
                            continue;
                        case "hang":
                            Suicide.Command_hang = kvp.Value[0];
                            continue;
                        case "suicide":
                            Suicide.Command_suicide = kvp.Value[0];
                            continue;
                        case "gimme":
                            Gimme.Command_gimme = kvp.Value[0];
                            continue;
                        case "gimmie":
                            Gimme.Command_gimmie = kvp.Value[0];
                            continue;
                        case "forgive":
                            Jail.Command_forgive = kvp.Value[0];
                            continue;
                        case "ready":
                            NewSpawnTele.Command_ready = kvp.Value[0];
                            continue;
                        case "trackanimal":
                            AnimalTracking.Command_trackanimal = kvp.Value[0];
                            continue;
                        case "track":
                            AnimalTracking.Command_track = kvp.Value[0];
                            continue;
                        case "claim":
                            FirstClaimBlock.Command_claim = kvp.Value[0];
                            continue;
                        case "clan add":
                            ClanManager.Command_add = kvp.Value[0];
                            continue;
                        case "clan del":
                            ClanManager.Command_delete = kvp.Value[0];
                            continue;
                        case "clan invite":
                            ClanManager.Command_invite = kvp.Value[0];
                            continue;
                        case "clan accept":
                            ClanManager.Command_accept = kvp.Value[0];
                            continue;
                        case "clan decline":
                            ClanManager.Command_decline = kvp.Value[0];
                            continue;
                        case "clan remove":
                            ClanManager.Command_remove = kvp.Value[0];
                            continue;
                        case "clan promote":
                            ClanManager.Command_promote = kvp.Value[0];
                            continue;
                        case "clan demote":
                            ClanManager.Command_demote = kvp.Value[0];
                            continue;
                        case "clan leave":
                            ClanManager.Command_leave = kvp.Value[0];
                            continue;
                        case "clan commands":
                            ClanManager.Command_commands = kvp.Value[0];
                            continue;
                        case "clan chat":
                            ClanManager.Command_chat = kvp.Value[0];
                            continue;
                        case "clan rename":
                            ClanManager.Command_rename = kvp.Value[0];
                            continue;
                        case "clan request":
                            ClanManager.Command_request = kvp.Value[0];
                            continue;
                        case "clan cc":
                            ClanManager.Command_cc = kvp.Value[0];
                            continue;
                        case "clan list":
                            ClanManager.Command_clan_list = kvp.Value[0];
                            continue;
                        case "reward":
                            Voting.Command_reward = kvp.Value[0];
                            continue;
                        case "vote":
                            Voting.Command_vote = kvp.Value[0];
                            continue;
                        case "shutdown":
                            Shutdown.Command_shutdown = kvp.Value[0];
                            continue;
                        case "adminlist":
                            AdminList.Command_adminlist = kvp.Value[0];
                            continue;
                        case "travel":
                            Travel.Command_travel = kvp.Value[0];
                            continue;
                        case "marketback":
                            Market.Command_marketback = kvp.Value[0];
                            continue;
                        case "mback":
                            Market.Command_mback = kvp.Value[0];
                            continue;
                        case "market":
                            Market.Command_market = kvp.Value[0];
                            continue;
                        case "lobbyback":
                            Lobby.Command_lobbyback = kvp.Value[0];
                            continue;
                        case "lback":
                            Lobby.Command_lobbyback = kvp.Value[0];
                            continue;
                        case "lobby":
                            Lobby.Command_lobby = kvp.Value[0];
                            continue;
                        case "shop":
                            Shop.Command_shop = kvp.Value[0];
                            continue;
                        case "shop buy":
                            Shop.Command_shop_buy = kvp.Value[0];
                            continue;
                        case "friend":
                            FriendTeleport.Command_friend = kvp.Value[0];
                            continue;
                        case "accept":
                            FriendTeleport.Command_accept = kvp.Value[0];
                            continue;
                        case "died":
                            Died.Command_died = kvp.Value[0];
                            continue;
                        case "restartvote":
                            RestartVote.Command_restartvote = kvp.Value[0];
                            continue;
                        case "mutevote":
                            MuteVote.Command_mutevote = kvp.Value[0];
                            continue;
                        case "kickvote":
                            KickVote.Command_kickvote = kvp.Value[0];
                            continue;
                        case "yes":
                            RestartVote.Command_yes = kvp.Value[0];
                            continue;
                        case "auction":
                            Auction.Command_auction = kvp.Value[0];
                            continue;
                        case "auction cancel":
                            Auction.Command_auction_cancel = kvp.Value[0];
                            continue;
                        case "auction buy":
                            Auction.Command_auction_buy = kvp.Value[0];
                            continue;
                        case "auction sell":
                            Auction.Command_auction_sell = kvp.Value[0];
                            continue;
                        case "fps":
                            Fps.Command_fps = kvp.Value[0];
                            continue;
                        case "loc":
                            Loc.Command_loc = kvp.Value[0];
                            continue;
                        case "vehicle":
                            VehicleRecall.Command_vehicle = kvp.Value[0];
                            continue;
                        case "report":
                            Report.Command_report = kvp.Value[0];
                            continue;
                        case "bounty":
                            Bounties.Command_bounty = kvp.Value[0];
                            continue;
                        case "lottery":
                            Lottery.Command_lottery = kvp.Value[0];
                            continue;
                        case "lottery enter":
                            Lottery.Command_lottery_enter = kvp.Value[0];
                            continue;
                        case "playerlist":
                            PlayerList.Command_playerlist = kvp.Value[0];
                            continue;
                        case "plist":
                            PlayerList.Command_plist = kvp.Value[0];
                            continue;
                        case "stuck":
                            Stuck.Command_stuck = kvp.Value[0];
                            continue;
                        case "poll yes":
                            Poll.Command_poll_yes = kvp.Value[0];
                            continue;
                        case "poll no":
                            Poll.Command_poll_no = kvp.Value[0];
                            continue;
                        case "poll":
                            Poll.Command_poll = kvp.Value[0];
                            continue;
                        case "bank":
                            Bank.Command_bank = kvp.Value[0];
                            continue;
                        case "deposit":
                            Bank.Command_deposit = kvp.Value[0];
                            continue;
                        case "withdraw":
                            Bank.Command_withdraw = kvp.Value[0];
                            continue;
                        case "transfer":
                            Bank.Command_transfer = kvp.Value[0];
                            continue;
                        case "join":
                            Event.Command_join = kvp.Value[0];
                            continue;
                        case "infoticker":
                            InfoTicker.Command_infoticker = kvp.Value[0];
                            continue;
                        case "session":
                            Session.Command_session = kvp.Value[0];
                            continue;
                        case "waypoint":
                            Waypoints.Command_waypoint = kvp.Value[0];
                            continue;
                        case "way":
                            Waypoints.Command_way = kvp.Value[0];
                            continue;
                        case "wp":
                            Waypoints.Command_wp = kvp.Value[0];
                            continue;
                        case "fwaypoint":
                            Waypoints.Command_fwaypoint = kvp.Value[0];
                            continue;
                        case "fway":
                            Waypoints.Command_fway = kvp.Value[0];
                            continue;
                        case "fwp":
                            Waypoints.Command_fwp = kvp.Value[0];
                            continue;
                        case "waypoint save":
                            Waypoints.Command_waypoint_save = kvp.Value[0];
                            continue;
                        case "way save":
                            Waypoints.Command_way_save = kvp.Value[0];
                            continue;
                        case "ws":
                            Waypoints.Command_ws = kvp.Value[0];
                            continue;
                        case "waypoint delete":
                            Waypoints.Command_waypoint_delete = kvp.Value[0];
                            continue;
                        case "way delete":
                            Waypoints.Command_way_delete = kvp.Value[0];
                            continue;
                        case "wd":
                            Waypoints.Command_wd = kvp.Value[0];
                            continue;
                        case "admin":
                            AdminChat.Command_admin = kvp.Value[0];
                            continue;
                        case "pmessage":
                            Whisper.Command_pmessage = kvp.Value[0];
                            continue;
                        case "pm":
                            Whisper.Command_pm = kvp.Value[0];
                            continue;
                        case "rmessage":
                            Whisper.Command_rmessage = kvp.Value[0];
                            continue;
                        case "rm":
                            Whisper.Command_rm = kvp.Value[0];
                            continue;
                        case "pray":
                            Prayer.Command_pray = kvp.Value[0];
                            continue;
                        case "scoutplayer":
                            ScoutPlayer.Command_scoutplayer = kvp.Value[0];
                            continue;
                        case "scout":
                            ScoutPlayer.Command_scout = kvp.Value[0];
                            continue;
                        case "exit":
                            ExitCommand.Command_exit = kvp.Value[0];
                            continue;
                        case "quit":
                            ExitCommand.Command_quit = kvp.Value[0];
                            continue;
                        case "ccc":
                            ChatColor.Command_ccc = kvp.Value[0];
                            continue;
                        case "ccpr":
                            ChatColor.Command_ccpr = kvp.Value[0];
                            continue;
                        case "ccnr":
                            ChatColor.Command_ccnr = kvp.Value[0];
                            continue;
                        case "gamble":
                            Gamble.Command_gamble = kvp.Value[0];
                            continue;
                        case "gamble bet":
                            Gamble.Command_gamble_bet = kvp.Value[0];
                            continue;
                        case "gamble payout":
                            Gamble.Command_gamble_payout = kvp.Value[0];
                            continue;
                        case "party":
                            AutoPartyInvite.Command_party = kvp.Value[0];
                            continue;
                        case "party add":
                            AutoPartyInvite.Command_party_add = kvp.Value[0];
                            continue;
                        case "party remove":
                            AutoPartyInvite.Command_party_remove = kvp.Value[0];
                            continue;
                        case "expire":
                            GeneralOperations.Command_expire = kvp.Value[0];
                            continue;
                        case "pickup":
                            BlockPickup.Command_pickup = kvp.Value[0];
                            continue;
                        case "wall":
                            Wall.Command_wall = kvp.Value[0];
                            continue;
                        case "bed":
                            Bed.Command_bed = kvp.Value[0];
                            continue;
                        case "rio":
                            RIO.Command_rio = kvp.Value[0];
                            continue;
                        case "overlay":
                            GeneralOperations.Command_overlay = kvp.Value[0];
                            continue;
                        case "imap":
                            InteractiveMap.Command_imap = kvp.Value[0];
                            continue;
                        case "map":
                            AllocsMap.Command_map = kvp.Value[0];
                            continue;
                        case "discord":
                            DiscordLink.Command_discord = kvp.Value[0];
                            continue;
                        case "claims":
                            LandClaimCount.Command_claims = kvp.Value[0];
                            continue;
                        case "donate":
                            DonationLink.Command_donate = kvp.Value[0];
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
            Commands.Add("    <Command Default=\"home\" Replacement=\"home\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"fhome\" Replacement=\"fhome\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"home save\" Replacement=\"home save\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"home delete\" Replacement=\"home delete\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"go home\" Replacement=\"go home\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"sethome\" Replacement=\"sethome\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"go way\" Replacement=\"go way\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"top3\" Replacement=\"top3\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"score\" Replacement=\"score\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"buy life\" Replacement=\"buy life\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"hardcore\" Replacement=\"hardcore\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"hardcore on\" Replacement=\"hardcore on\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"mute\" Replacement=\"mute\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"unmute\" Replacement=\"unmute\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"mutelist\" Replacement=\"mutelist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"commands\" Replacement=\"commands\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"day7\" Replacement=\"day7\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"day\" Replacement=\"day\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"bloodmoon\" Replacement=\"bloodmoon\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"bm\" Replacement=\"bm\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"killme\" Replacement=\"killme\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"wrist\" Replacement=\"wrist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"hang\" Replacement=\"hang\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"suicide\" Replacement=\"suicide\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"gimme\" Replacement=\"gimme\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"gimmie\" Replacement=\"gimmie\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"forgive\" Replacement=\"forgive\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"ready\" Replacement=\"ready\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"vote\" Replacement=\"vote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"trackanimal\" Replacement=\"trackanimal\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"track\" Replacement=\"track\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"claim\" Replacement=\"claim\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan add\" Replacement=\"clan add\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan del\" Replacement=\"clan del\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan invite\" Replacement=\"clan invite\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan accept\" Replacement=\"clan accept\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan decline\" Replacement=\"clan decline\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan remove\" Replacement=\"clan remove\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan promote\" Replacement=\"clan promote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan demote\" Replacement=\"clan demote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan leave\" Replacement=\"clan leave\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan commands\" Replacement=\"clan commands\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan chat\" Replacement=\"clan chat\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan rename\" Replacement=\"clan rename\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clan request\" Replacement=\"clan request\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"cc\" Replacement=\"cc\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"clanlist\" Replacement=\"clanlist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"reward\" Replacement=\"reward\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"shutdown\" Replacement=\"shutdown\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"adminlist\" Replacement=\"adminlist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"travel\" Replacement=\"travel\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"marketback\" Replacement=\"marketback\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"mback\" Replacement=\"mback\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"market\" Replacement=\"market\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"lobbyback\" Replacement=\"lobbyback\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"lback\" Replacement=\"lback\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"lobby\" Replacement=\"lobby\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"shop\" Replacement=\"shop\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"shop buy\" Replacement=\"shop buy\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"friend\" Replacement=\"friend\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"accept\" Replacement=\"accept\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"died\" Replacement=\"died\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"restartvote\" Replacement=\"restartvote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"mutevote\" Replacement=\"mutevote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"kickvote\" Replacement=\"kickvote\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"yes\" Replacement=\"yes\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"auction\" Replacement=\"auction\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"auction buy\" Replacement=\"auction buy\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"auction sell\" Replacement=\"auction sell\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"fps\" Replacement=\"fps\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"loc\" Replacement=\"loc\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"vehicle\" Replacement=\"vehicle\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"report\" Replacement=\"report\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"bounty\" Replacement=\"bounty\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"lottery\" Replacement=\"lottery\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"playerlist\" Replacement=\"playerlist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"plist\" Replacement=\"plist\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"stuck\" Replacement=\"stuck\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"poll yes\" Replacement=\"poll yes\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"poll no\" Replacement=\"poll no\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"poll\" Replacement=\"poll\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"bank\" Replacement=\"bank\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"deposit\" Replacement=\"deposit\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"withdraw\" Replacement=\"withdraw\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"transfer\" Replacement=\"transfer\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"join\" Replacement=\"event\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"infoticker\" Replacement=\"infoticker\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"session\" Replacement=\"session\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"waypoint\" Replacement=\"waypoint\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"way\" Replacement=\"way\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"wp\" Replacement=\"wp\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"fwaypoint\" Replacement=\"fwaypoint\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"fway\" Replacement=\"fway\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"fwp\" Replacement=\"fwp\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"waypoint save\" Replacement=\"waypoint save\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"way save\" Replacement=\"way save\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"ws\" Replacement=\"ws\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"waypoint delete\" Replacement=\"waypoint delete\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"way delete\" Replacement=\"way delete\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"wd\" Replacement=\"wd\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"admin\" Replacement=\"admin\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"pmessage\" Replacement=\"pmessage\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"pm\" Replacement=\"pm\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"rmessage\" Replacement=\"rmessage\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"rm\" Replacement=\"rm\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"pray\" Replacement=\"pray\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"scoutplayer\" Replacement=\"scoutplayer\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"scout\" Replacement=\"scout\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"exit\" Replacement=\"exit\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"quit\" Replacement=\"quit\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"ccc\" Replacement=\"ccc\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"ccpr\" Replacement=\"ccpr\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"ccnr\" Replacement=\"ccnr\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"gamble\" Replacement=\"gamble\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"gamble bet\" Replacement=\"gamble bet\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"gamble payout\" Replacement=\"gamble payout\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"party\" Replacement=\"party\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"party add\" Replacement=\"party add\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"party remove\" Replacement=\"party remove\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"expire\" Replacement=\"expire\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"pickup\" Replacement=\"pickup\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"wall\" Replacement=\"wall\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"bed\" Replacement=\"bed\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"rio\" Replacement=\"rio\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"overlay\" Replacement=\"overlay\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"imap\" Replacement=\"imap\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"map\" Replacement=\"map\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"discord\" Replacement=\"discord\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"claims\" Replacement=\"claims\" Hidden=\"false\" />");
            Commands.Add("    <Command Default=\"donate\" Replacement=\"donate\" Hidden=\"false\" />");
        }

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    List<string> commandList = Commands;
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CommandList>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                    sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
                    sw.WriteLine("    <!-- If hidden is set to true, the command will not show in response to using /commands -->");
                    sw.WriteLine("    <!-- <Command Default=\"gimme\" Replacement=\"give\" Hidden=\"false\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- All capital letters in commands") &&
                            !nodeList[i].OuterXml.Contains("<!-- Leave the default alone.") && !nodeList[i].OuterXml.Contains("<!-- If hidden is set to true") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Command Default=\"gimme") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Command")
                            {
                                string defaultCommand = "", replacement = "", hidden = "";
                                if (line.HasAttribute("Default") && line.HasAttribute("Replacement") && line.HasAttribute("Hidden"))
                                {
                                    defaultCommand = line.GetAttribute("Default");
                                    replacement = line.GetAttribute("Replacement");
                                    hidden = line.GetAttribute("Hidden");
                                    if (commandList.Count > 0)
                                    {
                                        for (int j = 0; j < commandList.Count; j++)
                                        {
                                            if (commandList[j].Contains(defaultCommand))
                                            {
                                                commandList.RemoveAt(j);
                                                sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" Hidden=\"{2}\" />", defaultCommand, replacement, hidden));
                                                break;
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
