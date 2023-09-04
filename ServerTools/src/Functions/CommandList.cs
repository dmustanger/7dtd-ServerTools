using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class CommandList
    {
        public static Dictionary<string, bool> Dict = new Dictionary<string, bool>();

        private static Dictionary<string, string> Replacements = new Dictionary<string, string>();

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
                    Log.Error("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message);
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                Replacements.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    string[] values = new string[2];
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
                            if (!Dict.ContainsKey(defaultCommand))
                            {
                                Dict.Add(defaultCommand, hidden);
                                Replacements.Add(defaultCommand, replacement);
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
                        Timers.UpgradeCommandListXml(nodeList);
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
                    Log.Out("[SERVERTOOLS] Error in CommandList.LoadXml: {0}", e.Message);
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
                sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
                sw.WriteLine("    <!-- If hidden is set to true, the command will not show in response to using /commands -->");
                sw.WriteLine("    <!-- <Command Default=\"gimme\" Replacement=\"give\" Hidden=\"false\" /> -->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> kvp in Dict)
                    {
                        Replacements.TryGetValue(kvp.Key, out string replacement);
                        sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" Hidden=\"{2}\" />", kvp.Key, replacement, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine("    <Command Default=\"go home\" Replacement=\"go home\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"home\" Replacement=\"home\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ho\" Replacement=\"ho\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fhome\" Replacement=\"fhome\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fho\" Replacement=\"fho\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"sethome\" Replacement=\"sethome\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"home save\" Replacement=\"home save\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"hs\" Replacement=\"hs\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"home delete\" Replacement=\"home delete\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"hd\" Replacement=\"hd\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"top3\" Replacement=\"top3\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"score\" Replacement=\"score\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"buy life\" Replacement=\"buy life\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"hardcore\" Replacement=\"hardcore\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"hardcore on\" Replacement=\"hardcore on\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"mute\" Replacement=\"mute\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"unmute\" Replacement=\"unmute\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"mutelist\" Replacement=\"mutelist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"commands\" Replacement=\"commands\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"day7\" Replacement=\"day7\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"day\" Replacement=\"day\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"bloodmoon\" Replacement=\"bloodmoon\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"bm\" Replacement=\"bm\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"killme\" Replacement=\"killme\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"wrist\" Replacement=\"wrist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"hang\" Replacement=\"hang\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"suicide\" Replacement=\"suicide\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"gimme\" Replacement=\"gimme\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"gimmie\" Replacement=\"gimmie\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"forgive\" Replacement=\"forgive\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ready\" Replacement=\"ready\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"vote\" Replacement=\"vote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"animal\" Replacement=\"animal\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"track\" Replacement=\"track\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"claim\" Replacement=\"claim\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan add\" Replacement=\"clan add\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan del\" Replacement=\"clan del\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan invite\" Replacement=\"clan invite\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan accept\" Replacement=\"clan accept\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan decline\" Replacement=\"clan decline\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan remove\" Replacement=\"clan remove\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan promote\" Replacement=\"clan promote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan demote\" Replacement=\"clan demote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan leave\" Replacement=\"clan leave\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan commands\" Replacement=\"clan commands\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan chat\" Replacement=\"clan chat\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan rename\" Replacement=\"clan rename\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clan request\" Replacement=\"clan request\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"cc\" Replacement=\"cc\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"clanlist\" Replacement=\"clanlist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"reward\" Replacement=\"reward\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"shutdown\" Replacement=\"shutdown\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"adminlist\" Replacement=\"adminlist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"travel\" Replacement=\"travel\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"marketback\" Replacement=\"marketback\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"mback\" Replacement=\"mback\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"market\" Replacement=\"market\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"lobbyback\" Replacement=\"lobbyback\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"lback\" Replacement=\"lback\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"lobby\" Replacement=\"lobby\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"shop\" Replacement=\"shop\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"shop buy\" Replacement=\"shop buy\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"friend\" Replacement=\"friend\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"accept\" Replacement=\"accept\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"died\" Replacement=\"died\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"restartvote\" Replacement=\"restartvote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"mutevote\" Replacement=\"mutevote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"kickvote\" Replacement=\"kickvote\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"yes\" Replacement=\"yes\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"auction\" Replacement=\"auction\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"auction buy\" Replacement=\"auction buy\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"auction sell\" Replacement=\"auction sell\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fps\" Replacement=\"fps\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"loc\" Replacement=\"loc\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"vehicle\" Replacement=\"vehicle\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"report\" Replacement=\"report\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"bounty\" Replacement=\"bounty\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"lottery\" Replacement=\"lottery\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"playerlist\" Replacement=\"playerlist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"plist\" Replacement=\"plist\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"stuck\" Replacement=\"stuck\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"poll yes\" Replacement=\"poll yes\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"poll no\" Replacement=\"poll no\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"poll\" Replacement=\"poll\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"bank\" Replacement=\"bank\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"deposit\" Replacement=\"deposit\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"withdraw\" Replacement=\"withdraw\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"transfer\" Replacement=\"transfer\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"join\" Replacement=\"event\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"infoticker\" Replacement=\"infoticker\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"session\" Replacement=\"session\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"go way\" Replacement=\"go way\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"waypoint\" Replacement=\"waypoint\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"way\" Replacement=\"way\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"wp\" Replacement=\"wp\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fwaypoint\" Replacement=\"fwaypoint\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fway\" Replacement=\"fway\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"fwp\" Replacement=\"fwp\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"setwaypoint\" Replacement=\"setwaypoint\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"waypoint save\" Replacement=\"waypoint save\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"way save\" Replacement=\"way save\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ws\" Replacement=\"ws\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"waypoint delete\" Replacement=\"waypoint delete\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"way delete\" Replacement=\"way delete\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"wd\" Replacement=\"wd\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"admin\" Replacement=\"admin\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"pmessage\" Replacement=\"pmessage\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"pm\" Replacement=\"pm\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"rmessage\" Replacement=\"rmessage\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"rm\" Replacement=\"rm\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"pray\" Replacement=\"pray\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"scoutplayer\" Replacement=\"scoutplayer\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"scout\" Replacement=\"scout\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"exit\" Replacement=\"exit\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"quit\" Replacement=\"quit\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ccc\" Replacement=\"ccc\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ccpr\" Replacement=\"ccpr\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"ccnr\" Replacement=\"ccnr\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"gamble\" Replacement=\"gamble\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"gamble bet\" Replacement=\"gamble bet\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"gamble payout\" Replacement=\"gamble payout\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"party\" Replacement=\"party\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"party add\" Replacement=\"party add\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"party remove\" Replacement=\"party remove\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"expire\" Replacement=\"expire\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"pickup\" Replacement=\"pickup\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"wall\" Replacement=\"wall\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"bed\" Replacement=\"bed\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"rio\" Replacement=\"rio\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"overlay\" Replacement=\"overlay\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"discord\" Replacement=\"discord\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"claims\" Replacement=\"claims\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"donate\" Replacement=\"donate\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"sort\" Replacement=\"sort\" Hidden=\"false\" />");
                    sw.WriteLine("    <Command Default=\"harvest\" Replacement=\"harvest\" Hidden=\"false\" />");
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
                foreach (KeyValuePair<string, bool> kvp in Dict)
                {
                    Replacements.TryGetValue(kvp.Key, out string replacement);
                    switch (kvp.Key)
                    {
                        case "go home":
                            Homes.Command_go_home = replacement;
                            continue;
                        case "home":
                            Homes.Command_home = replacement;
                            continue;
                        case "ho":
                            Homes.Command_ho = replacement;
                            continue;
                        case "fhome":
                            Homes.Command_fhome = replacement;
                            continue;
                        case "fho":
                            Homes.Command_fho = replacement;
                            continue;
                        case "sethome":
                            Homes.Command_sethome = replacement;
                            continue;
                        case "home save":
                            Homes.Command_home_save = replacement;
                            continue;
                        case "hs":
                            Homes.Command_hs = replacement;
                            continue;
                        case "home delete":
                            Homes.Command_home_delete = replacement;
                            continue;
                        case "hd":
                            Homes.Command_hd = replacement;
                            continue;
                        case "top3":
                            Hardcore.Command_top3 = replacement;
                            continue;
                        case "score":
                            Hardcore.Command_score = replacement;
                            continue;
                        case "buy life":
                            Hardcore.Command_buy_life = replacement;
                            continue;
                        case "hardcore":
                            Hardcore.Command_hardcore = replacement;
                            continue;
                        case "hardcore on":
                            Hardcore.Command_hardcore_on = replacement;
                            continue;
                        case "mute":
                            Mute.Command_mute = replacement;
                            continue;
                        case "unmute":
                            Mute.Command_unmute = replacement;
                            continue;
                        case "mutelist":
                            Mute.Command_mutelist = replacement;
                            continue;
                        case "commands":
                            GeneralOperations.Command_commands = replacement;
                            continue;
                        case "day7":
                            Day7.Command_day7 = replacement;
                            continue;
                        case "day":
                            Day7.Command_day = replacement;
                            continue;
                        case "bloodmoon":
                            Bloodmoon.Command_bloodmoon = replacement;
                            continue;
                        case "bm":
                            Bloodmoon.Command_bm = replacement;
                            continue;
                        case "killme":
                            Suicide.Command_killme = replacement;
                            continue;
                        case "wrist":
                            Suicide.Command_wrist = replacement;
                            continue;
                        case "hang":
                            Suicide.Command_hang = replacement;
                            continue;
                        case "suicide":
                            Suicide.Command_suicide = replacement;
                            continue;
                        case "gimme":
                            Gimme.Command_gimme = replacement;
                            continue;
                        case "gimmie":
                            Gimme.Command_gimmie = replacement;
                            continue;
                        case "forgive":
                            Jail.Command_forgive = replacement;
                            continue;
                        case "ready":
                            NewSpawnTele.Command_ready = replacement;
                            continue;
                        case "animal":
                            AnimalTracking.Command_animal = replacement;
                            continue;
                        case "track":
                            AnimalTracking.Command_track = replacement;
                            continue;
                        case "claim":
                            FirstClaimBlock.Command_claim = replacement;
                            continue;
                        case "clan add":
                            ClanManager.Command_add = replacement;
                            continue;
                        case "clan del":
                            ClanManager.Command_delete = replacement;
                            continue;
                        case "clan invite":
                            ClanManager.Command_invite = replacement;
                            continue;
                        case "clan accept":
                            ClanManager.Command_accept = replacement;
                            continue;
                        case "clan decline":
                            ClanManager.Command_decline = replacement;
                            continue;
                        case "clan remove":
                            ClanManager.Command_remove = replacement;
                            continue;
                        case "clan promote":
                            ClanManager.Command_promote = replacement;
                            continue;
                        case "clan demote":
                            ClanManager.Command_demote = replacement;
                            continue;
                        case "clan leave":
                            ClanManager.Command_leave = replacement;
                            continue;
                        case "clan commands":
                            ClanManager.Command_commands = replacement;
                            continue;
                        case "clan chat":
                            ClanManager.Command_chat = replacement;
                            continue;
                        case "clan rename":
                            ClanManager.Command_rename = replacement;
                            continue;
                        case "clan request":
                            ClanManager.Command_request = replacement;
                            continue;
                        case "clan cc":
                            ClanManager.Command_cc = replacement;
                            continue;
                        case "clan list":
                            ClanManager.Command_clan_list = replacement;
                            continue;
                        case "reward":
                            Voting.Command_reward = replacement;
                            continue;
                        case "vote":
                            Voting.Command_vote = replacement;
                            continue;
                        case "shutdown":
                            Shutdown.Command_shutdown = replacement;
                            continue;
                        case "adminlist":
                            AdminList.Command_adminlist = replacement;
                            continue;
                        case "travel":
                            Travel.Command_travel = replacement;
                            continue;
                        case "marketback":
                            Market.Command_marketback = replacement;
                            continue;
                        case "mback":
                            Market.Command_mback = replacement;
                            continue;
                        case "market":
                            Market.Command_market = replacement;
                            continue;
                        case "lobbyback":
                            Lobby.Command_lobbyback = replacement;
                            continue;
                        case "lback":
                            Lobby.Command_lobbyback = replacement;
                            continue;
                        case "lobby":
                            Lobby.Command_lobby = replacement;
                            continue;
                        case "shop":
                            Shop.Command_shop = replacement;
                            continue;
                        case "shop buy":
                            Shop.Command_shop_buy = replacement;
                            continue;
                        case "friend":
                            FriendTeleport.Command_friend = replacement;
                            continue;
                        case "accept":
                            FriendTeleport.Command_accept = replacement;
                            continue;
                        case "died":
                            Died.Command_died = replacement;
                            continue;
                        case "restartvote":
                            RestartVote.Command_restartvote = replacement;
                            continue;
                        case "mutevote":
                            MuteVote.Command_mutevote = replacement;
                            continue;
                        case "kickvote":
                            KickVote.Command_kickvote = replacement;
                            continue;
                        case "yes":
                            RestartVote.Command_yes = replacement;
                            continue;
                        case "auction":
                            Auction.Command_auction = replacement;
                            continue;
                        case "auction cancel":
                            Auction.Command_auction_cancel = replacement;
                            continue;
                        case "auction buy":
                            Auction.Command_auction_buy = replacement;
                            continue;
                        case "auction sell":
                            Auction.Command_auction_sell = replacement;
                            continue;
                        case "fps":
                            Fps.Command_fps = replacement;
                            continue;
                        case "loc":
                            Loc.Command_loc = replacement;
                            continue;
                        case "vehicle":
                            VehicleRecall.Command_vehicle = replacement;
                            continue;
                        case "report":
                            Report.Command_report = replacement;
                            continue;
                        case "bounty":
                            Bounties.Command_bounty = replacement;
                            continue;
                        case "lottery":
                            Lottery.Command_lottery = replacement;
                            continue;
                        case "lottery enter":
                            Lottery.Command_lottery_enter = replacement;
                            continue;
                        case "playerlist":
                            PlayerList.Command_playerlist = replacement;
                            continue;
                        case "plist":
                            PlayerList.Command_plist = replacement;
                            continue;
                        case "stuck":
                            Stuck.Command_stuck = replacement;
                            continue;
                        case "poll yes":
                            Poll.Command_poll_yes = replacement;
                            continue;
                        case "poll no":
                            Poll.Command_poll_no = replacement;
                            continue;
                        case "poll":
                            Poll.Command_poll = replacement;
                            continue;
                        case "bank":
                            Bank.Command_bank = replacement;
                            continue;
                        case "deposit":
                            Bank.Command_deposit = replacement;
                            continue;
                        case "withdraw":
                            Bank.Command_withdraw = replacement;
                            continue;
                        case "transfer":
                            Bank.Command_transfer = replacement;
                            continue;
                        case "join":
                            Event.Command_join = replacement;
                            continue;
                        case "infoticker":
                            InfoTicker.Command_infoticker = replacement;
                            continue;
                        case "session":
                            Session.Command_session = replacement;
                            continue;
                        case "go way":
                            Waypoints.Command_go_way = replacement;
                            continue;
                        case "waypoint":
                            Waypoints.Command_waypoint = replacement;
                            continue;
                        case "way":
                            Waypoints.Command_way = replacement;
                            continue;
                        case "wp":
                            Waypoints.Command_wp = replacement;
                            continue;
                        case "fwaypoint":
                            Waypoints.Command_fwaypoint = replacement;
                            continue;
                        case "fway":
                            Waypoints.Command_fway = replacement;
                            continue;
                        case "fwp":
                            Waypoints.Command_fwp = replacement;
                            continue;
                        case "setwaypoint":
                            Waypoints.Command_setwaypoint = replacement;
                            continue;
                        case "waypoint save":
                            Waypoints.Command_waypoint_save = replacement;
                            continue;
                        case "way save":
                            Waypoints.Command_way_save = replacement;
                            continue;
                        case "ws":
                            Waypoints.Command_ws = replacement;
                            continue;
                        case "waypoint delete":
                            Waypoints.Command_waypoint_delete = replacement;
                            continue;
                        case "way delete":
                            Waypoints.Command_way_delete = replacement;
                            continue;
                        case "wd":
                            Waypoints.Command_wd = replacement;
                            continue;
                        case "admin":
                            AdminChat.Command_admin = replacement;
                            continue;
                        case "pmessage":
                            Whisper.Command_pmessage = replacement;
                            continue;
                        case "pm":
                            Whisper.Command_pm = replacement;
                            continue;
                        case "rmessage":
                            Whisper.Command_rmessage = replacement;
                            continue;
                        case "rm":
                            Whisper.Command_rm = replacement;
                            continue;
                        case "pray":
                            Prayer.Command_pray = replacement;
                            continue;
                        case "scoutplayer":
                            ScoutPlayer.Command_scoutplayer = replacement;
                            continue;
                        case "scout":
                            ScoutPlayer.Command_scout = replacement;
                            continue;
                        case "exit":
                            ExitCommand.Command_exit = replacement;
                            continue;
                        case "quit":
                            ExitCommand.Command_quit = replacement;
                            continue;
                        case "ccc":
                            ChatColor.Command_ccc = replacement;
                            continue;
                        case "ccpr":
                            ChatColor.Command_ccpr = replacement;
                            continue;
                        case "ccnr":
                            ChatColor.Command_ccnr = replacement;
                            continue;
                        case "gamble":
                            Gamble.Command_gamble = replacement;
                            continue;
                        case "gamble bet":
                            Gamble.Command_gamble_bet = replacement;
                            continue;
                        case "gamble payout":
                            Gamble.Command_gamble_payout = replacement;
                            continue;
                        case "party":
                            AutoPartyInvite.Command_party = replacement;
                            continue;
                        case "party add":
                            AutoPartyInvite.Command_party_add = replacement;
                            continue;
                        case "party remove":
                            AutoPartyInvite.Command_party_remove = replacement;
                            continue;
                        case "expire":
                            GeneralOperations.Command_expire = replacement;
                            continue;
                        case "pickup":
                            BlockPickup.Command_pickup = replacement;
                            continue;
                        case "wall":
                            Wall.Command_wall = replacement;
                            continue;
                        case "bed":
                            Bed.Command_bed = replacement;
                            continue;
                        case "rio":
                            RIO.Command_rio = replacement;
                            continue;
                        case "overlay":
                            GeneralOperations.Command_overlay = replacement;
                            continue;
                        case "discord":
                            DiscordLink.Command_discord = replacement;
                            continue;
                        case "claims":
                            LandClaimCount.Command_claims = replacement;
                            continue;
                        case "donate":
                            DonationLink.Command_donate = replacement;
                            continue;
                        case "sort":
                            Sorter.Command_sort = replacement;
                            continue;
                        case "harvest":
                            Harvest.Command_harvest = replacement;
                            continue;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in CommandList.Exec: {0}", e.Message);
            }
        }

        public static void UpgradeXml(XmlNodeList upgradeNodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CommandList>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Leave the default alone. Only edit the replacement to your desired command -->");
                    sw.WriteLine("    <!-- All capital letters in commands will be reduced to lowercase -->");
                    sw.WriteLine("    <!-- If hidden is set to true, the command will not show in response to using /commands -->");
                    sw.WriteLine("    <!-- <Command Default=\"gimme\" Replacement=\"give\" Hidden=\"false\" /> -->");
                    for (int i = 0; i < upgradeNodeList.Count; i++)
                    {
                        if (upgradeNodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!upgradeNodeList[i].OuterXml.Contains("<!-- All capital letters in commands") &&
                            !upgradeNodeList[i].OuterXml.Contains("<!-- Leave the default alone.") && !upgradeNodeList[i].OuterXml.Contains("<!-- If hidden is set to true") &&
                            !upgradeNodeList[i].OuterXml.Contains("<!-- <Command Default=\"gimme") && !upgradeNodeList[i].OuterXml.Contains("<Command Default=\"\"") &&
                            !upgradeNodeList[i].OuterXml.Contains("<!-- <Version") && !upgradeNodeList[i].OuterXml.Contains("<!-- Do not forget"))
                            {
                                sw.WriteLine(upgradeNodeList[i].OuterXml);
                            }
                        }
                    }
                        sw.WriteLine("    <Command Default=\"go home\" Replacement=\"go home\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"home\" Replacement=\"home\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ho\" Replacement=\"ho\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fhome\" Replacement=\"fhome\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fho\" Replacement=\"fho\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"sethome\" Replacement=\"sethome\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"home save\" Replacement=\"home save\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"hs\" Replacement=\"hs\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"home delete\" Replacement=\"home delete\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"hd\" Replacement=\"hd\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"top3\" Replacement=\"top3\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"score\" Replacement=\"score\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"buy life\" Replacement=\"buy life\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"hardcore\" Replacement=\"hardcore\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"hardcore on\" Replacement=\"hardcore on\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"mute\" Replacement=\"mute\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"unmute\" Replacement=\"unmute\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"mutelist\" Replacement=\"mutelist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"commands\" Replacement=\"commands\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"day7\" Replacement=\"day7\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"day\" Replacement=\"day\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"bloodmoon\" Replacement=\"bloodmoon\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"bm\" Replacement=\"bm\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"killme\" Replacement=\"killme\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"wrist\" Replacement=\"wrist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"hang\" Replacement=\"hang\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"suicide\" Replacement=\"suicide\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"gimme\" Replacement=\"gimme\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"gimmie\" Replacement=\"gimmie\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"forgive\" Replacement=\"forgive\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ready\" Replacement=\"ready\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"vote\" Replacement=\"vote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"animal\" Replacement=\"animal\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"track\" Replacement=\"track\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"claim\" Replacement=\"claim\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan add\" Replacement=\"clan add\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan del\" Replacement=\"clan del\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan invite\" Replacement=\"clan invite\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan accept\" Replacement=\"clan accept\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan decline\" Replacement=\"clan decline\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan remove\" Replacement=\"clan remove\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan promote\" Replacement=\"clan promote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan demote\" Replacement=\"clan demote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan leave\" Replacement=\"clan leave\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan commands\" Replacement=\"clan commands\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan chat\" Replacement=\"clan chat\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan rename\" Replacement=\"clan rename\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clan request\" Replacement=\"clan request\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"cc\" Replacement=\"cc\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"clanlist\" Replacement=\"clanlist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"reward\" Replacement=\"reward\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"shutdown\" Replacement=\"shutdown\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"adminlist\" Replacement=\"adminlist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"travel\" Replacement=\"travel\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"marketback\" Replacement=\"marketback\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"mback\" Replacement=\"mback\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"market\" Replacement=\"market\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"lobbyback\" Replacement=\"lobbyback\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"lback\" Replacement=\"lback\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"lobby\" Replacement=\"lobby\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"shop\" Replacement=\"shop\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"shop buy\" Replacement=\"shop buy\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"friend\" Replacement=\"friend\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"accept\" Replacement=\"accept\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"died\" Replacement=\"died\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"restartvote\" Replacement=\"restartvote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"mutevote\" Replacement=\"mutevote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"kickvote\" Replacement=\"kickvote\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"yes\" Replacement=\"yes\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"auction\" Replacement=\"auction\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"auction buy\" Replacement=\"auction buy\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"auction sell\" Replacement=\"auction sell\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fps\" Replacement=\"fps\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"loc\" Replacement=\"loc\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"vehicle\" Replacement=\"vehicle\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"report\" Replacement=\"report\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"bounty\" Replacement=\"bounty\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"lottery\" Replacement=\"lottery\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"playerlist\" Replacement=\"playerlist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"plist\" Replacement=\"plist\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"stuck\" Replacement=\"stuck\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"poll yes\" Replacement=\"poll yes\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"poll no\" Replacement=\"poll no\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"poll\" Replacement=\"poll\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"bank\" Replacement=\"bank\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"deposit\" Replacement=\"deposit\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"withdraw\" Replacement=\"withdraw\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"transfer\" Replacement=\"transfer\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"join\" Replacement=\"event\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"infoticker\" Replacement=\"infoticker\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"session\" Replacement=\"session\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"go way\" Replacement=\"go way\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"waypoint\" Replacement=\"waypoint\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"way\" Replacement=\"way\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"wp\" Replacement=\"wp\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fwaypoint\" Replacement=\"fwaypoint\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fway\" Replacement=\"fway\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"fwp\" Replacement=\"fwp\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"setwaypoint\" Replacement=\"setwaypoint\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"waypoint save\" Replacement=\"waypoint save\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"way save\" Replacement=\"way save\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ws\" Replacement=\"ws\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"waypoint delete\" Replacement=\"waypoint delete\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"way delete\" Replacement=\"way delete\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"wd\" Replacement=\"wd\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"admin\" Replacement=\"admin\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"pmessage\" Replacement=\"pmessage\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"pm\" Replacement=\"pm\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"rmessage\" Replacement=\"rmessage\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"rm\" Replacement=\"rm\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"pray\" Replacement=\"pray\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"scoutplayer\" Replacement=\"scoutplayer\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"scout\" Replacement=\"scout\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"exit\" Replacement=\"exit\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"quit\" Replacement=\"quit\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ccc\" Replacement=\"ccc\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ccpr\" Replacement=\"ccpr\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"ccnr\" Replacement=\"ccnr\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"gamble\" Replacement=\"gamble\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"gamble bet\" Replacement=\"gamble bet\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"gamble payout\" Replacement=\"gamble payout\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"party\" Replacement=\"party\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"party add\" Replacement=\"party add\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"party remove\" Replacement=\"party remove\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"expire\" Replacement=\"expire\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"pickup\" Replacement=\"pickup\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"wall\" Replacement=\"wall\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"bed\" Replacement=\"bed\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"rio\" Replacement=\"rio\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"overlay\" Replacement=\"overlay\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"discord\" Replacement=\"discord\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"claims\" Replacement=\"claims\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"donate\" Replacement=\"donate\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"sort\" Replacement=\"sort\" Hidden=\"false\" />");
                        sw.WriteLine("    <Command Default=\"harvest\" Replacement=\"harvest\" Hidden=\"false\" />");
                        sw.WriteLine("</CommandList>");
                        sw.Flush();
                        sw.Close();

                    if (File.Exists(FilePath))
                    {
                        XmlDocument xml = new XmlDocument();
                        try
                        {
                            xml.Load(FilePath);
                        }
                        catch (XmlException e)
                        {
                            Log.Error("[SERVERTOOLS] Failed loading {0}: {1}", FilePath, e.Message);
                            return;
                        }
                        XmlNodeList nodeList = xml.DocumentElement.ChildNodes;
                        if (nodeList == null)
                        {
                            return;
                        }
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            if (nodeList[i].HasChildNodes)
                            {
                                XmlNodeList childNodeList = nodeList[i].ChildNodes;
                                if (childNodeList == null)
                                {
                                    continue;
                                }
                                for (int j = 0; j < childNodeList.Count; j++)
                                {
                                    if (childNodeList[j].NodeType == XmlNodeType.Comment)
                                    {
                                        continue;
                                    }
                                    XmlElement newLine = (XmlElement)childNodeList[j];
                                    if (newLine.HasAttributes && newLine.Name == "Command")
                                    {
                                        XmlAttributeCollection newAttributes = newLine.Attributes;
                                        for (int k = 0; k < upgradeNodeList.Count; k++)
                                        {
                                            if (upgradeNodeList[k].HasChildNodes)
                                            {
                                                XmlNodeList oldChildNodeList = upgradeNodeList[k].ChildNodes;
                                                if (oldChildNodeList == null)
                                                {
                                                    continue;
                                                }
                                                for (int l = 0; l < oldChildNodeList.Count; l++)
                                                {
                                                    if (oldChildNodeList[l].NodeType == XmlNodeType.Comment)
                                                    {
                                                        continue;
                                                    }
                                                    XmlElement oldLine = (XmlElement)oldChildNodeList[l];
                                                    if (oldLine.HasAttributes && oldLine.Name == "Command" && newLine.Attributes[0].Value == oldLine.Attributes[0].Value)
                                                    {
                                                        XmlAttributeCollection oldAttributes = oldLine.Attributes;
                                                        for (int m = 1; m < newAttributes.Count; m++)
                                                        {
                                                            for (int n = 1; n < oldAttributes.Count; n++)
                                                            {
                                                                if (newAttributes[m] != null && oldAttributes[n] != null && newAttributes[m].Name == oldAttributes[n].Name && newAttributes[m].Value != oldAttributes[n].Value)
                                                                {
                                                                    newAttributes[m].Value = oldAttributes[n].Value;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in CommandList.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
