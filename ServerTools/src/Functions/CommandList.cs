using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class CommandList
    {
        public static bool IsRunning = false;
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

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
            IsRunning = false;
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
                        else if (_line.HasAttribute("Default") && _line.HasAttribute("Replacement"))
                        {
                            string _default = _line.GetAttribute("Default");
                            string _replacement = _line.GetAttribute("Replacement").ToLower();
                            if (!Dict.ContainsKey(_default))
                            {
                                Dict.Add(_default, _replacement);
                            }
                        }
                    }
                }
                if (Dict.Count > 0)
                {
                    Exec();
                }
                if (upgrade)
                {
                    UpgradeXml(_childNodes);
                    return;
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
                    sw.WriteLine("    <Command Default=\"home\" Replacement=\"home\" />");
                    sw.WriteLine("    <Command Default=\"fhome\" Replacement=\"fhome\" />");
                    sw.WriteLine("    <Command Default=\"home save\" Replacement=\"home save\" />");
                    sw.WriteLine("    <Command Default=\"home del\" Replacement=\"home del\" />");
                    sw.WriteLine("    <Command Default=\"go home\" Replacement=\"go home\" />");
                    sw.WriteLine("    <Command Default=\"sethome\" Replacement=\"sethome\" />");
                    sw.WriteLine("    <Command Default=\"go way\" Replacement=\"go way\" />");
                    sw.WriteLine("    <Command Default=\"top3\" Replacement=\"top3\" />");
                    sw.WriteLine("    <Command Default=\"score\" Replacement=\"score\" />");
                    sw.WriteLine("    <Command Default=\"buy life\" Replacement=\"buy life\" />");
                    sw.WriteLine("    <Command Default=\"hardcore\" Replacement=\"hardcore\" />");
                    sw.WriteLine("    <Command Default=\"hardcore on\" Replacement=\"hardcore on\" />");
                    sw.WriteLine("    <Command Default=\"mute\" Replacement=\"mute\" />");
                    sw.WriteLine("    <Command Default=\"unmute\" Replacement=\"unmute\" />");
                    sw.WriteLine("    <Command Default=\"mutelist\" Replacement=\"mutelist\" />");
                    sw.WriteLine("    <Command Default=\"commands\" Replacement=\"commands\" />");
                    sw.WriteLine("    <Command Default=\"day7\" Replacement=\"day7\" />");
                    sw.WriteLine("    <Command Default=\"day\" Replacement=\"day\" />");
                    sw.WriteLine("    <Command Default=\"bloodmoon\" Replacement=\"bloodmoon\" />");
                    sw.WriteLine("    <Command Default=\"bm\" Replacement=\"bm\" />");
                    sw.WriteLine("    <Command Default=\"killme\" Replacement=\"killme\" />");
                    sw.WriteLine("    <Command Default=\"wrist\" Replacement=\"wrist\" />");
                    sw.WriteLine("    <Command Default=\"hang\" Replacement=\"hang\" />");
                    sw.WriteLine("    <Command Default=\"suicide\" Replacement=\"suicide\" />");
                    sw.WriteLine("    <Command Default=\"gimme\" Replacement=\"gimme\" />");
                    sw.WriteLine("    <Command Default=\"gimmie\" Replacement=\"gimmie\" />");
                    sw.WriteLine("    <Command Default=\"set jail\" Replacement=\"set jail\" />");
                    sw.WriteLine("    <Command Default=\"jail\" Replacement=\"jail\" />");
                    sw.WriteLine("    <Command Default=\"unjail\" Replacement=\"unjail\" />");
                    sw.WriteLine("    <Command Default=\"forgive\" Replacement=\"forgive\" />");
                    sw.WriteLine("    <Command Default=\"setspawn\" Replacement=\"setspawn\" />");
                    sw.WriteLine("    <Command Default=\"ready\" Replacement=\"ready\" />");
                    sw.WriteLine("    <Command Default=\"trackanimal\" Replacement=\"trackanimal\" />");
                    sw.WriteLine("    <Command Default=\"track\" Replacement=\"track\" />");
                    sw.WriteLine("    <Command Default=\"claim\" Replacement=\"claim\" />");
                    sw.WriteLine("    <Command Default=\"clan add\" Replacement=\"clan add\" />");
                    sw.WriteLine("    <Command Default=\"clan del\" Replacement=\"clan del\" />");
                    sw.WriteLine("    <Command Default=\"clan invite\" Replacement=\"clan invite\" />");
                    sw.WriteLine("    <Command Default=\"clan accept\" Replacement=\"clan accept\" />");
                    sw.WriteLine("    <Command Default=\"clan decline\" Replacement=\"clan decline\" />");
                    sw.WriteLine("    <Command Default=\"clan remove\" Replacement=\"clan remove\" />");
                    sw.WriteLine("    <Command Default=\"clan promote\" Replacement=\"clan promote\" />");
                    sw.WriteLine("    <Command Default=\"clan demote\" Replacement=\"clan demote\" />");
                    sw.WriteLine("    <Command Default=\"clan leave\" Replacement=\"clan leave\" />");
                    sw.WriteLine("    <Command Default=\"clan commands\" Replacement=\"clan commands\" />");
                    sw.WriteLine("    <Command Default=\"clan chat\" Replacement=\"clan chat\" />");
                    sw.WriteLine("    <Command Default=\"clan rename\" Replacement=\"clan rename\" />");
                    sw.WriteLine("    <Command Default=\"clan request\" Replacement=\"clan request\" />");
                    sw.WriteLine("    <Command Default=\"cc\" Replacement=\"cc\" />");
                    sw.WriteLine("    <Command Default=\"clanlist\" Replacement=\"clanlist\" />");
                    sw.WriteLine("    <Command Default=\"reward\" Replacement=\"reward\" />");
                    sw.WriteLine("    <Command Default=\"shutdown\" Replacement=\"shutdown\" />");
                    sw.WriteLine("    <Command Default=\"adminlist\" Replacement=\"adminlist\" />");
                    sw.WriteLine("    <Command Default=\"travel\" Replacement=\"travel\" />");
                    sw.WriteLine("    <Command Default=\"marketback\" Replacement=\"marketback\" />");
                    sw.WriteLine("    <Command Default=\"mback\" Replacement=\"mback\" />");
                    sw.WriteLine("    <Command Default=\"setmarket\" Replacement=\"setmarket\" />");
                    sw.WriteLine("    <Command Default=\"market\" Replacement=\"market\" />");
                    sw.WriteLine("    <Command Default=\"lobbyback\" Replacement=\"lobbyback\" />");
                    sw.WriteLine("    <Command Default=\"lback\" Replacement=\"lback\" />");
                    sw.WriteLine("    <Command Default=\"setlobby\" Replacement=\"setlobby\" />");
                    sw.WriteLine("    <Command Default=\"lobby\" Replacement=\"lobby\" />");
                    sw.WriteLine("    <Command Default=\"wallet\" Replacement=\"wallet\" />");
                    sw.WriteLine("    <Command Default=\"shop\" Replacement=\"shop\" />");
                    sw.WriteLine("    <Command Default=\"shop buy\" Replacement=\"shop buy\" />");
                    sw.WriteLine("    <Command Default=\"friend\" Replacement=\"friend\" />");
                    sw.WriteLine("    <Command Default=\"accept\" Replacement=\"accept\" />");
                    sw.WriteLine("    <Command Default=\"died\" Replacement=\"died\" />");
                    sw.WriteLine("    <Command Default=\"weathervote\" Replacement=\"weathervote\" />");
                    sw.WriteLine("    <Command Default=\"sun\" Replacement=\"sun\" />");
                    sw.WriteLine("    <Command Default=\"rain\" Replacement=\"rain\" />");
                    sw.WriteLine("    <Command Default=\"snow\" Replacement=\"snow\" />");
                    sw.WriteLine("    <Command Default=\"restartvote\" Replacement=\"restartvote\" />");
                    sw.WriteLine("    <Command Default=\"mutevote\" Replacement=\"mutevote\" />");
                    sw.WriteLine("    <Command Default=\"kickvote\" Replacement=\"kickvote\" />");
                    sw.WriteLine("    <Command Default=\"yes\" Replacement=\"yes\" />");
                    sw.WriteLine("    <Command Default=\"reserved\" Replacement=\"reserved\" />");
                    sw.WriteLine("    <Command Default=\"auction\" Replacement=\"auction\" />");
                    sw.WriteLine("    <Command Default=\"auction cancel\" Replacement=\"auction cancel\" />");
                    sw.WriteLine("    <Command Default=\"auction buy\" Replacement=\"auction buy\" />");
                    sw.WriteLine("    <Command Default=\"auction sell\" Replacement=\"auction sell\" />");
                    sw.WriteLine("    <Command Default=\"fps\" Replacement=\"fps\" />");
                    sw.WriteLine("    <Command Default=\"loc\" Replacement=\"loc\" />");
                    sw.WriteLine("    <Command Default=\"bike\" Replacement=\"bike\" />");
                    sw.WriteLine("    <Command Default=\"minibike\" Replacement=\"minibike\" />");
                    sw.WriteLine("    <Command Default=\"motorbike\" Replacement=\"motorbike\" />");
                    sw.WriteLine("    <Command Default=\"jeep\" Replacement=\"jeep\" />");
                    sw.WriteLine("    <Command Default=\"gyro\" Replacement=\"gyro\" />");
                    sw.WriteLine("    <Command Default=\"report\" Replacement=\"report\" />");
                    sw.WriteLine("    <Command Default=\"bounty\" Replacement=\"bounty\" />");
                    sw.WriteLine("    <Command Default=\"lottery\" Replacement=\"lottery\" />");
                    sw.WriteLine("    <Command Default=\"lottery enter\" Replacement=\"lottery enter\" />");
                    sw.WriteLine("    <Command Default=\"playerlist\" Replacement=\"playerlist\" />");
                    sw.WriteLine("    <Command Default=\"stuck\" Replacement=\"stuck\" />");
                    sw.WriteLine("    <Command Default=\"poll yes\" Replacement=\"poll yes\" />");
                    sw.WriteLine("    <Command Default=\"poll no\" Replacement=\"poll no\" />");
                    sw.WriteLine("    <Command Default=\"poll\" Replacement=\"poll\" />");
                    sw.WriteLine("    <Command Default=\"bank\" Replacement=\"bank\" />");
                    sw.WriteLine("    <Command Default=\"deposit\" Replacement=\"deposit\" />");
                    sw.WriteLine("    <Command Default=\"withdraw\" Replacement=\"withdraw\" />");
                    sw.WriteLine("    <Command Default=\"wallet deposit\" Replacement=\"wallet deposit\" />");
                    sw.WriteLine("    <Command Default=\"wallet withdraw\" Replacement=\"wallet withdraw\" />");
                    sw.WriteLine("    <Command Default=\"transfer\" Replacement=\"transfer\" />");
                    sw.WriteLine("    <Command Default=\"join\" Replacement=\"event\" />");
                    sw.WriteLine("    <Command Default=\"infoticker\" Replacement=\"infoticker\" />");
                    sw.WriteLine("    <Command Default=\"session\" Replacement=\"session\" />");
                    sw.WriteLine("    <Command Default=\"waypoint\" Replacement=\"waypoint\" />");
                    sw.WriteLine("    <Command Default=\"way\" Replacement=\"way\" />");
                    sw.WriteLine("    <Command Default=\"wp\" Replacement=\"wp\" />");
                    sw.WriteLine("    <Command Default=\"fwaypoint\" Replacement=\"fwaypoint\" />");
                    sw.WriteLine("    <Command Default=\"fway\" Replacement=\"fway\" />");
                    sw.WriteLine("    <Command Default=\"fwp\" Replacement=\"fwp\" />");
                    sw.WriteLine("    <Command Default=\"waypoint save\" Replacement=\"waypoint save\" />");
                    sw.WriteLine("    <Command Default=\"way save\" Replacement=\"way save\" />");
                    sw.WriteLine("    <Command Default=\"ws\" Replacement=\"ws\" />");
                    sw.WriteLine("    <Command Default=\"waypoint del\" Replacement=\"waypoint del\" />");
                    sw.WriteLine("    <Command Default=\"way del\" Replacement=\"way del\" />");
                    sw.WriteLine("    <Command Default=\"wd\" Replacement=\"wd\" />");
                    sw.WriteLine("    <Command Default=\"admin\" Replacement=\"admin\" />");
                    sw.WriteLine("    <Command Default=\"pmessage\" Replacement=\"pmessage\" />");
                    sw.WriteLine("    <Command Default=\"pm\" Replacement=\"pm\" />");
                    sw.WriteLine("    <Command Default=\"rmessage\" Replacement=\"rmessage\" />");
                    sw.WriteLine("    <Command Default=\"rm\" Replacement=\"rm\" />");
                    sw.WriteLine("    <Command Default=\"pray\" Replacement=\"pray\" />");
                    sw.WriteLine("    <Command Default=\"scoutplayer\" Replacement=\"scoutplayer\" />");
                    sw.WriteLine("    <Command Default=\"scout\" Replacement=\"scout\" />");
                    sw.WriteLine("    <Command Default=\"exit\" Replacement=\"exit\" />");
                    sw.WriteLine("    <Command Default=\"quit\" Replacement=\"quit\" />");
                    sw.WriteLine("    <Command Default=\"ccc\" Replacement=\"ccc\" />");
                    sw.WriteLine("    <Command Default=\"ccpr\" Replacement=\"ccpr\" />");
                    sw.WriteLine("    <Command Default=\"ccnr\" Replacement=\"ccnr\" />");
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
                        Animals.Command_trackanimal = kvp.Value;
                    }
                    else if (kvp.Key == "track")
                    {
                        Animals.Command_track = kvp.Value;
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
                    else if (kvp.Key == "bike")
                    {
                        VehicleTeleport.Command_bike = kvp.Value;
                    }
                    else if (kvp.Key == "minibike")
                    {
                        VehicleTeleport.Command_minibike = kvp.Value;
                    }
                    else if (kvp.Key == "motorbike")
                    {
                        VehicleTeleport.Command_motorbike = kvp.Value;
                    }
                    else if (kvp.Key == "jeep")
                    {
                        VehicleTeleport.Command_jeep = kvp.Value;
                    }
                    else if (kvp.Key == "gyro")
                    {
                        VehicleTeleport.Command_gyro = kvp.Value;
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
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandList.Exec: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<CommandList>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- All capital letters in commands will be reduced to lowercase -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.OuterXml.Contains("Command"))
                        {
                            string _default = "", _replacement = "";
                            DateTime _dateTime = DateTime.Now;
                            if (_line.HasAttribute("Default"))
                            {
                                _default = _line.GetAttribute("Default");
                            }
                            if (_line.HasAttribute("Replacement"))
                            {
                                _replacement = _line.GetAttribute("Replacement");
                            }
                            sw.WriteLine(string.Format("    <Command Default=\"{0}\" Replacement=\"{1}\" />", _default, _replacement));
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
