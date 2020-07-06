using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoadTriggers
    {
        public static bool IsRunning = false;
        private const string file = "EventTriggers.xml";
        public static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        public static Dictionary<int, string[]> Dict = new Dictionary<int, string[]>();
        public static List<string> TriggerList = new List<string>();

        public static void Load()
        {
            if (!IsRunning)
            {
                LoadXml();
                Exec();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            if (IsRunning)
            {
                Dict.Clear();
                TriggerList.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
            }
        }

        public static void LoadXml()
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
                if (childNode.Name == "triggers")
                {
                    Dict.Clear();
                    TriggerList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Event Triggers' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("number"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing number attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("number"), out int _number))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Event Triggers because of invalid (non-numeric) value for 'number' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("default"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing default attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("replacement"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing replacement attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string[] _c = { _line.GetAttribute("default"), _line.GetAttribute("replacement") };
                        if (!Dict.ContainsKey(_number))
                        {
                            Dict.Add(_number, _c);
                            TriggerList.Add(_c[1]);
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
                sw.WriteLine("<Event>");
                sw.WriteLine("    <triggers>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("        <trigger number=\"{0}\" default=\"{1}\" replacement=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <trigger number=\"1\" default=\"sethome\" replacement=\"sethome\" />");
                    sw.WriteLine("        <trigger number=\"2\" default=\"home\" replacement=\"home\" />");
                    sw.WriteLine("        <trigger number=\"3\" default=\"fhome\" replacement=\"fhome\" />");
                    sw.WriteLine("        <trigger number=\"4\" default=\"delhome\" replacement=\"delhome\" />");
                    sw.WriteLine("        <trigger number=\"5\" default=\"sethome2\" replacement=\"sethome2\" />");
                    sw.WriteLine("        <trigger number=\"6\" default=\"home2\" replacement=\"home2\" />");
                    sw.WriteLine("        <trigger number=\"7\" default=\"fhome2\" replacement=\"fhome2\" />");
                    sw.WriteLine("        <trigger number=\"8\" default=\"delhome2\" replacement=\"delhome2\" />");
                    sw.WriteLine("        <trigger number=\"9\" default=\"go\" replacement=\"go\" />");
                    sw.WriteLine("        <trigger number=\"10\" default=\"go way\" replacement=\"go way\" />");
                    sw.WriteLine("        <trigger number=\"11\" default=\"top3\" replacement=\"top3\" />");
                    sw.WriteLine("        <trigger number=\"12\" default=\"score\" replacement=\"score\" />");
                    sw.WriteLine("        <trigger number=\"13\" default=\"mute\" replacement=\"mute\" />");
                    sw.WriteLine("        <trigger number=\"14\" default=\"unmute\" replacement=\"unmute\" />");
                    sw.WriteLine("        <trigger number=\"15\" default=\"commands\" replacement=\"commands\" />");
                    sw.WriteLine("        <trigger number=\"16\" default=\"day7\" replacement=\"day7\" />");
                    sw.WriteLine("        <trigger number=\"17\" default=\"day\" replacement=\"day\" />");
                    sw.WriteLine("        <trigger number=\"18\" default=\"bloodmoon\" replacement=\"bloodmoon\" />");
                    sw.WriteLine("        <trigger number=\"19\" default=\"bm\" replacement=\"bm\" />");
                    sw.WriteLine("        <trigger number=\"20\" default=\"killme\" replacement=\"killme\" />");
                    sw.WriteLine("        <trigger number=\"21\" default=\"wrist\" replacement=\"wrist\" />");
                    sw.WriteLine("        <trigger number=\"22\" default=\"hang\" replacement=\"hang\" />");
                    sw.WriteLine("        <trigger number=\"23\" default=\"suicide\" replacement=\"suicide\" />");
                    sw.WriteLine("        <trigger number=\"24\" default=\"gimme\" replacement=\"gimme\" />");
                    sw.WriteLine("        <trigger number=\"25\" default=\"gimmie\" replacement=\"gimmie\" />");
                    sw.WriteLine("        <trigger number=\"26\" default=\"set jail\" replacement=\"set jail\" />");
                    sw.WriteLine("        <trigger number=\"27\" default=\"jail\" replacement=\"jail\" />");
                    sw.WriteLine("        <trigger number=\"28\" default=\"unjail\" replacement=\"unjail\" />");
                    sw.WriteLine("        <trigger number=\"29\" default=\"setspawn\" replacement=\"setspawn\" />");
                    sw.WriteLine("        <trigger number=\"30\" default=\"trackanimal\" replacement=\"trackanimal\" />");
                    sw.WriteLine("        <trigger number=\"31\" default=\"track\" replacement=\"track\" />");
                    sw.WriteLine("        <trigger number=\"32\" default=\"claim\" replacement=\"claim\" />");
                    sw.WriteLine("        <trigger number=\"33\" default=\"clan add\" replacement=\"clan add\" />");
                    sw.WriteLine("        <trigger number=\"34\" default=\"clan del\" replacement=\"clan del\" />");
                    sw.WriteLine("        <trigger number=\"35\" default=\"clan invite\" replacement=\"clan invite\" />");
                    sw.WriteLine("        <trigger number=\"36\" default=\"clan accept\" replacement=\"clan accept\" />");
                    sw.WriteLine("        <trigger number=\"37\" default=\"clan decline\" replacement=\"clan decline\" />");
                    sw.WriteLine("        <trigger number=\"38\" default=\"clan remove\" replacement=\"clan remove\" />");
                    sw.WriteLine("        <trigger number=\"39\" default=\"clan promote\" replacement=\"clan promote\" />");
                    sw.WriteLine("        <trigger number=\"40\" default=\"clan demote\" replacement=\"clan demote\" />");
                    sw.WriteLine("        <trigger number=\"41\" default=\"clan leave\" replacement=\"clan leave\" />");
                    sw.WriteLine("        <trigger number=\"42\" default=\"clan commands\" replacement=\"clan commands\" />");
                    sw.WriteLine("        <trigger number=\"43\" default=\"clan chat\" replacement=\"clan chat\" />");
                    sw.WriteLine("        <trigger number=\"44\" default=\"clan rename\" replacement=\"clan rename\" />");
                    sw.WriteLine("        <trigger number=\"45\" default=\"clan request\" replacement=\"clan request\" />");
                    sw.WriteLine("        <trigger number=\"46\" default=\"reward\" replacement=\"reward\" />");
                    sw.WriteLine("        <trigger number=\"47\" default=\"shutdown\" replacement=\"shutdown\" />");
                    sw.WriteLine("        <trigger number=\"48\" default=\"adminlist\" replacement=\"adminlist\" />");
                    sw.WriteLine("        <trigger number=\"49\" default=\"travel\" replacement=\"travel\" />");
                    sw.WriteLine("        <trigger number=\"50\" default=\"return\" replacement=\"return\" />");
                    sw.WriteLine("        <trigger number=\"51\" default=\"marketback\" replacement=\"marketback\" />");
                    sw.WriteLine("        <trigger number=\"52\" default=\"mback\" replacement=\"mback\" />");
                    sw.WriteLine("        <trigger number=\"53\" default=\"lobbyback\" replacement=\"lobbyback\" />");
                    sw.WriteLine("        <trigger number=\"54\" default=\"lback\" replacement=\"lback\" />");
                    sw.WriteLine("        <trigger number=\"55\" default=\"forgive\" replacement=\"forgive\" />");
                    sw.WriteLine("        <trigger number=\"56\" default=\"wallet\" replacement=\"wallet\" />");
                    sw.WriteLine("        <trigger number=\"57\" default=\"shop\" replacement=\"shop\" />");
                    sw.WriteLine("        <trigger number=\"58\" default=\"shop buy\" replacement=\"shop buy\" />");
                    sw.WriteLine("        <trigger number=\"59\" default=\"friend\" replacement=\"friend\" />");
                    sw.WriteLine("        <trigger number=\"60\" default=\"accept\" replacement=\"accept\" />");
                    sw.WriteLine("        <trigger number=\"61\" default=\"died\" replacement=\"died\" />");
                    sw.WriteLine("        <trigger number=\"62\" default=\"weathervote\" replacement=\"weathervote\" />");
                    sw.WriteLine("        <trigger number=\"63\" default=\"sun\" replacement=\"sun\" />");
                    sw.WriteLine("        <trigger number=\"64\" default=\"rain\" replacement=\"rain\" />");
                    sw.WriteLine("        <trigger number=\"65\" default=\"snow\" replacement=\"snow\" />");
                    sw.WriteLine("        <trigger number=\"66\" default=\"restartvote\" replacement=\"restartvote\" />");
                    sw.WriteLine("        <trigger number=\"67\" default=\"mutevote\" replacement=\"mutevote\" />");
                    sw.WriteLine("        <trigger number=\"68\" default=\"kickvote\" replacement=\"kickvote\" />");
                    sw.WriteLine("        <trigger number=\"69\" default=\"reserved\" replacement=\"reserved\" />");
                    sw.WriteLine("        <trigger number=\"70\" default=\"yes\" replacement=\"yes\" />");
                    sw.WriteLine("        <trigger number=\"71\" default=\"auction\" replacement=\"auction\" />");
                    sw.WriteLine("        <trigger number=\"72\" default=\"auction cancel\" replacement=\"auction cancel\" />");
                    sw.WriteLine("        <trigger number=\"73\" default=\"auction buy\" replacement=\"auction buy\" />");
                    sw.WriteLine("        <trigger number=\"74\" default=\"auction sell\" replacement=\"auction sell\" />");
                    sw.WriteLine("        <trigger number=\"75\" default=\"fps\" replacement=\"fps\" />");
                    sw.WriteLine("        <trigger number=\"76\" default=\"loc\" replacement=\"loc\" />");
                    sw.WriteLine("        <trigger number=\"77\" default=\"bike\" replacement=\"bike\" />");
                    sw.WriteLine("        <trigger number=\"78\" default=\"minibike\" replacement=\"minibike\" />");
                    sw.WriteLine("        <trigger number=\"79\" default=\"motorbike\" replacement=\"motorbike\" />");
                    sw.WriteLine("        <trigger number=\"80\" default=\"jeep\" replacement=\"jeep\" />");
                    sw.WriteLine("        <trigger number=\"81\" default=\"gyro\" replacement=\"gyro\" />");
                    sw.WriteLine("        <trigger number=\"82\" default=\"report\" replacement=\"report\" />");
                    sw.WriteLine("        <trigger number=\"83\" default=\"bounty\" replacement=\"bounty\" />");
                    sw.WriteLine("        <trigger number=\"84\" default=\"lottery\" replacement=\"lottery\" />");
                    sw.WriteLine("        <trigger number=\"85\" default=\"lottery enter\" replacement=\"lottery enter\" />");
                    sw.WriteLine("        <trigger number=\"86\" default=\"ready\" replacement=\"ready\" />");
                    sw.WriteLine("        <trigger number=\"87\" default=\"setlobby\" replacement=\"setlobby\" />");
                    sw.WriteLine("        <trigger number=\"88\" default=\"lobby\" replacement=\"lobby\" />");
                    sw.WriteLine("        <trigger number=\"89\" default=\"playerlist\" replacement=\"playerlist\" />");
                    sw.WriteLine("        <trigger number=\"90\" default=\"stuck\" replacement=\"stuck\" />");
                    sw.WriteLine("        <trigger number=\"91\" default=\"pollyes\" replacement=\"pollyes\" />");
                    sw.WriteLine("        <trigger number=\"92\" default=\"pollno\" replacement=\"pollno\" />");
                    sw.WriteLine("        <trigger number=\"93\" default=\"poll\" replacement=\"poll\" />");
                    sw.WriteLine("        <trigger number=\"94\" default=\"bank\" replacement=\"bank\" />");
                    sw.WriteLine("        <trigger number=\"95\" default=\"deposit\" replacement=\"deposit\" />");
                    sw.WriteLine("        <trigger number=\"96\" default=\"withdraw\" replacement=\"withdraw\" />");
                    sw.WriteLine("        <trigger number=\"97\" default=\"wallet deposit\" replacement=\"wallet deposit\" />");
                    sw.WriteLine("        <trigger number=\"98\" default=\"wallet withdraw\" replacement=\"wallet withdraw\" />");
                    sw.WriteLine("        <trigger number=\"99\" default=\"transfer\" replacement=\"transfer\" />");
                    sw.WriteLine("        <trigger number=\"100\" default=\"join\" replacement=\"event\" />");
                    sw.WriteLine("        <trigger number=\"101\" default=\"buy life\" replacement=\"buy life\" />");
                    sw.WriteLine("        <trigger number=\"102\" default=\"setmarket\" replacement=\"setmarket\" />");
                    sw.WriteLine("        <trigger number=\"103\" default=\"market\" replacement=\"market\" />");
                    sw.WriteLine("        <trigger number=\"104\" default=\"infoticker\" replacement=\"infoticker\" />");
                    sw.WriteLine("        <trigger number=\"105\" default=\"session\" replacement=\"session\" />");
                    sw.WriteLine("        <trigger number=\"106\" default=\"waypoint\" replacement=\"waypoint\" />");
                    sw.WriteLine("        <trigger number=\"107\" default=\"way\" replacement=\"way\" />");
                    sw.WriteLine("        <trigger number=\"108\" default=\"wp\" replacement=\"wp\" />");
                    sw.WriteLine("        <trigger number=\"109\" default=\"fwaypoint\" replacement=\"fwaypoint\" />");
                    sw.WriteLine("        <trigger number=\"110\" default=\"fway\" replacement=\"fway\" />");
                    sw.WriteLine("        <trigger number=\"111\" default=\"fwp\" replacement=\"fwp\" />");
                    sw.WriteLine("        <trigger number=\"112\" default=\"waypoint save\" replacement=\"waypoint save\" />");
                    sw.WriteLine("        <trigger number=\"113\" default=\"way save\" replacement=\"way save\" />");
                    sw.WriteLine("        <trigger number=\"114\" default=\"ws\" replacement=\"ws\" />");
                    sw.WriteLine("        <trigger number=\"115\" default=\"waypoint del\" replacement=\"waypoint del\" />");
                    sw.WriteLine("        <trigger number=\"116\" default=\"way del\" replacement=\"way del\" />");
                    sw.WriteLine("        <trigger number=\"117\" default=\"wd\" replacement=\"wd\" />");
                    sw.WriteLine("        <trigger number=\"118\" default=\"admin\" replacement=\"admin\" />");
                    sw.WriteLine("        <trigger number=\"119\" default=\"mutelist\" replacement=\"mutelist\" />");
                    sw.WriteLine("        <trigger number=\"120\" default=\"pmessage\" replacement=\"pmessage\" />");
                    sw.WriteLine("        <trigger number=\"121\" default=\"pm\" replacement=\"pm\" />");
                    sw.WriteLine("        <trigger number=\"122\" default=\"rmessage\" replacement=\"rmessage\" />");
                    sw.WriteLine("        <trigger number=\"123\" default=\"rm\" replacement=\"rm\" />");
                    sw.WriteLine("        <trigger number=\"124\" default=\"cc\" replacement=\"cc\" />");
                    sw.WriteLine("        <trigger number=\"125\" default=\"clanlist\" replacement=\"clanlist\" />");
                    sw.WriteLine("        <trigger number=\"126\" default=\"pray\" replacement=\"pray\" />");
                    sw.WriteLine("        <trigger number=\"127\" default=\"hardcore\" replacement=\"hardcore\" />");
                    sw.WriteLine("        <trigger number=\"128\" default=\"hardcore on\" replacement=\"hardcore on\" />");
                    sw.WriteLine("        <trigger number=\"129\" default=\"scoutplayer\" replacement=\"scoutplayer\" />");
                    sw.WriteLine("        <trigger number=\"130\" default=\"scout\" replacement=\"scout\" />");
                    sw.WriteLine("        <trigger number=\"131\" default=\"exit\" replacement=\"exit\" />");
                    sw.WriteLine("        <trigger number=\"132\" default=\"quit\" replacement=\"quit\" />");
                }
                sw.WriteLine("    </triggers>");
                sw.WriteLine("</Event>");
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
            Exec();
        }

        public static void Exec()
        {
            if (Dict.Count > 0)
            {
                foreach (KeyValuePair<int, string[]> kvp in Dict)
                {
                    if (kvp.Key == 1)
                    {
                        TeleportHome.Command1 = kvp.Value[1];
                    }
                    else if (kvp.Key == 2)
                    {
                        TeleportHome.Command2 = kvp.Value[1];
                    }
                    else if (kvp.Key == 3)
                    {
                        TeleportHome.Command3 = kvp.Value[1];
                    }
                    else if (kvp.Key == 4)
                    {
                        TeleportHome.Command4 = kvp.Value[1];
                    }
                    else if (kvp.Key == 5)
                    {
                        TeleportHome.Command5 = kvp.Value[1];
                    }
                    else if (kvp.Key == 6)
                    {
                        TeleportHome.Command6 = kvp.Value[1];
                    }
                    else if (kvp.Key == 7)
                    {
                        TeleportHome.Command7 = kvp.Value[1];
                    }
                    else if (kvp.Key == 8)
                    {
                        TeleportHome.Command8 = kvp.Value[1];
                    }
                    else if (kvp.Key == 9)
                    {
                        TeleportHome.Command9 = kvp.Value[1];
                    }
                    else if (kvp.Key == 10)
                    {
                        Waypoints.Command10 = kvp.Value[1];
                    }
                    else if (kvp.Key == 11)
                    {
                        Hardcore.Command11 = kvp.Value[1];
                    }
                    else if (kvp.Key == 12)
                    {
                        Hardcore.Command12 = kvp.Value[1];
                    }
                    else if (kvp.Key == 13)
                    {
                        Mute.Command13 = kvp.Value[1];
                    }
                    else if (kvp.Key == 14)
                    {
                        Mute.Command14 = kvp.Value[1];
                    }
                    else if (kvp.Key == 15)
                    {
                        CustomCommands.Command15 = kvp.Value[1];
                    }
                    else if (kvp.Key == 16)
                    {
                        Day7.Command16 = kvp.Value[1];
                    }
                    else if (kvp.Key == 17)
                    {
                        Day7.Command17 = kvp.Value[1];
                    }
                    else if (kvp.Key == 18)
                    {
                        Bloodmoon.Command18 = kvp.Value[1];
                    }
                    else if (kvp.Key == 19)
                    {
                        Bloodmoon.Command19 = kvp.Value[1];
                    }
                    else if (kvp.Key == 20)
                    {
                        Suicide.Command20 = kvp.Value[1];
                    }
                    else if (kvp.Key == 21)
                    {
                        Suicide.Command21 = kvp.Value[1];
                    }
                    else if (kvp.Key == 22)
                    {
                        Suicide.Command22 = kvp.Value[1];
                    }
                    else if (kvp.Key == 23)
                    {
                        Suicide.Command23 = kvp.Value[1];
                    }
                    else if (kvp.Key == 24)
                    {
                        Gimme.Command24 = kvp.Value[1];
                    }
                    else if (kvp.Key == 25)
                    {
                        Gimme.Command25 = kvp.Value[1];
                    }
                    else if (kvp.Key == 26)
                    {
                        Jail.Command26 = kvp.Value[1];
                    }
                    else if (kvp.Key == 27)
                    {
                        Jail.Command27 = kvp.Value[1];
                    }
                    else if (kvp.Key == 28)
                    {
                        Jail.Command28 = kvp.Value[1];
                    }
                    else if (kvp.Key == 29)
                    {
                        NewSpawnTele.Command29 = kvp.Value[1];
                    }
                    else if (kvp.Key == 30)
                    {
                        Animals.Command30 = kvp.Value[1];
                    }
                    else if (kvp.Key == 31)
                    {
                        Animals.Command31 = kvp.Value[1];
                    }
                    else if (kvp.Key == 32)
                    {
                        FirstClaimBlock.Command32 = kvp.Value[1];
                    }
                    else if (kvp.Key == 33)
                    {
                        ClanManager.Command33 = kvp.Value[1];
                    }
                    else if (kvp.Key == 34)
                    {
                        ClanManager.Command34 = kvp.Value[1];
                    }
                    else if (kvp.Key == 35)
                    {
                        ClanManager.Command35 = kvp.Value[1];
                    }
                    else if (kvp.Key == 36)
                    {
                        ClanManager.Command36 = kvp.Value[1];
                    }
                    else if (kvp.Key == 37)
                    {
                        ClanManager.Command37 = kvp.Value[1];
                    }
                    else if (kvp.Key == 38)
                    {
                        ClanManager.Command38 = kvp.Value[1];
                    }
                    else if (kvp.Key == 39)
                    {
                        ClanManager.Command39 = kvp.Value[1];
                    }
                    else if (kvp.Key == 40)
                    {
                        ClanManager.Command40 = kvp.Value[1];
                    }
                    else if (kvp.Key == 41)
                    {
                        ClanManager.Command41 = kvp.Value[1];
                    }
                    else if (kvp.Key == 42)
                    {
                        ClanManager.Command42 = kvp.Value[1];
                    }
                    else if (kvp.Key == 43)
                    {
                        ClanManager.Command43 = kvp.Value[1];
                    }
                    else if (kvp.Key == 44)
                    {
                        ClanManager.Command44 = kvp.Value[1];
                    }
                    else if (kvp.Key == 45)
                    {
                        ClanManager.Command45 = kvp.Value[1];
                    }
                    else if (kvp.Key == 46)
                    {
                        VoteReward.Command46 = kvp.Value[1];
                    }
                    else if (kvp.Key == 47)
                    {
                        Shutdown.Command47 = kvp.Value[1];
                    }
                    else if (kvp.Key == 48)
                    {
                        AdminList.Command48 = kvp.Value[1];
                    }
                    else if (kvp.Key == 49)
                    {
                        Travel.Command49 = kvp.Value[1];
                    }
                    else if (kvp.Key == 50)
                    {
                        Zones.Command50 = kvp.Value[1];
                    }
                    else if (kvp.Key == 51)
                    {
                        Market.Command51 = kvp.Value[1];
                    }
                    else if (kvp.Key == 52)
                    {
                        Market.Command52 = kvp.Value[1];
                    }
                    else if (kvp.Key == 53)
                    {
                        Lobby.Command53 = kvp.Value[1];
                    }
                    else if (kvp.Key == 54)
                    {
                        Lobby.Command54 = kvp.Value[1];
                    }
                    else if (kvp.Key == 55)
                    {
                        Jail.Command55 = kvp.Value[1];
                    }
                    else if (kvp.Key == 56)
                    {
                        Wallet.Command56 = kvp.Value[1];
                    }
                    else if (kvp.Key == 57)
                    {
                        Shop.Command57 = kvp.Value[1];
                    }
                    else if (kvp.Key == 58)
                    {
                        Shop.Command58 = kvp.Value[1];
                    }
                    else if (kvp.Key == 59)
                    {
                        FriendTeleport.Command59 = kvp.Value[1];
                    }
                    else if (kvp.Key == 60)
                    {
                        FriendTeleport.Command60 = kvp.Value[1];
                    }
                    else if (kvp.Key == 61)
                    {
                        DeathSpot.Command61 = kvp.Value[1];
                    }
                    else if (kvp.Key == 62)
                    {
                        WeatherVote.Command62 = kvp.Value[1];
                    }
                    else if (kvp.Key == 63)
                    {
                        WeatherVote.Command63 = kvp.Value[1];
                    }
                    else if (kvp.Key == 64)
                    {
                        WeatherVote.Command64 = kvp.Value[1];
                    }
                    else if (kvp.Key == 65)
                    {
                        WeatherVote.Command65 = kvp.Value[1];
                    }
                    else if (kvp.Key == 66)
                    {
                        RestartVote.Command66 = kvp.Value[1];
                    }
                    else if (kvp.Key == 67)
                    {
                        MuteVote.Command67 = kvp.Value[1];
                    }
                    else if (kvp.Key == 68)
                    {
                        KickVote.Command68 = kvp.Value[1];
                    }
                    else if (kvp.Key == 69)
                    {
                        ReservedSlots.Command69 = kvp.Value[1];
                    }
                    else if (kvp.Key == 70)
                    {
                        RestartVote.Command70 = kvp.Value[1];
                    }
                    else if (kvp.Key == 71)
                    {
                        AuctionBox.Command71 = kvp.Value[1];
                    }
                    else if (kvp.Key == 72)
                    {
                        AuctionBox.Command72 = kvp.Value[1];
                    }
                    else if (kvp.Key == 73)
                    {
                        AuctionBox.Command73 = kvp.Value[1];
                    }
                    else if (kvp.Key == 74)
                    {
                        AuctionBox.Command74 = kvp.Value[1];
                    }
                    else if (kvp.Key == 75)
                    {
                        Fps.Command75 = kvp.Value[1];
                    }
                    else if (kvp.Key == 76)
                    {
                        Loc.Command76 = kvp.Value[1];
                    }
                    else if (kvp.Key == 77)
                    {
                        VehicleTeleport.Command77 = kvp.Value[1];
                    }
                    else if (kvp.Key == 78)
                    {
                        VehicleTeleport.Command78 = kvp.Value[1];
                    }
                    else if (kvp.Key == 79)
                    {
                        VehicleTeleport.Command79 = kvp.Value[1];
                    }
                    else if (kvp.Key == 80)
                    {
                        VehicleTeleport.Command80 = kvp.Value[1];
                    }
                    else if (kvp.Key == 81)
                    {
                        VehicleTeleport.Command81 = kvp.Value[1];
                    }
                    else if (kvp.Key == 82)
                    {
                        Report.Command82 = kvp.Value[1];
                    }
                    else if (kvp.Key == 83)
                    {
                        Bounties.Command83 = kvp.Value[1];
                    }
                    else if (kvp.Key == 84)
                    {
                        Lottery.Command84 = kvp.Value[1];
                    }
                    else if (kvp.Key == 85)
                    {
                        Lottery.Command85 = kvp.Value[1];
                    }
                    else if (kvp.Key == 86)
                    {
                        NewSpawnTele.Command86 = kvp.Value[1];
                    }
                    else if (kvp.Key == 87)
                    {
                        Lobby.Command87 = kvp.Value[1];
                    }
                    else if (kvp.Key == 88)
                    {
                        Lobby.Command88 = kvp.Value[1];
                    }
                    else if (kvp.Key == 89)
                    {
                        PlayerList.Command89 = kvp.Value[1];
                    }
                    else if (kvp.Key == 90)
                    {
                        Stuck.Command90 = kvp.Value[1];
                    }
                    else if (kvp.Key == 91)
                    {
                        Poll.Command91 = kvp.Value[1];
                    }
                    else if (kvp.Key == 92)
                    {
                        Poll.Command92 = kvp.Value[1];
                    }
                    else if (kvp.Key == 93)
                    {
                        Poll.Command93 = kvp.Value[1];
                    }
                    else if (kvp.Key == 94)
                    {
                        Bank.Command94 = kvp.Value[1];
                    }
                    else if (kvp.Key == 95)
                    {
                        Bank.Command95 = kvp.Value[1];
                    }
                    else if (kvp.Key == 96)
                    {
                        Bank.Command96 = kvp.Value[1];
                    }
                    else if (kvp.Key == 97)
                    {
                        Bank.Command97 = kvp.Value[1];
                    }
                    else if (kvp.Key == 98)
                    {
                        Bank.Command98 = kvp.Value[1];
                    }
                    else if (kvp.Key == 99)
                    {
                        Bank.Command99 = kvp.Value[1];
                    }
                    else if (kvp.Key == 100)
                    {
                        Event.Command100 = kvp.Value[1];
                    }
                    else if (kvp.Key == 101)
                    {
                        Hardcore.Command101 = kvp.Value[1];
                    }
                    else if (kvp.Key == 102)
                    {
                        Market.Command102 = kvp.Value[1];
                    }
                    else if (kvp.Key == 103)
                    {
                        Market.Command103 = kvp.Value[1];
                    }
                    else if (kvp.Key == 104)
                    {
                        InfoTicker.Command104 = kvp.Value[1];
                    }
                    else if (kvp.Key == 105)
                    {
                        Session.Command105 = kvp.Value[1];
                    }
                    else if (kvp.Key == 106)
                    {
                        Waypoints.Command106 = kvp.Value[1];
                    }
                    else if (kvp.Key == 107)
                    {
                        Waypoints.Command107 = kvp.Value[1];
                    }
                    else if (kvp.Key == 108)
                    {
                        Waypoints.Command108 = kvp.Value[1];
                    }
                    else if (kvp.Key == 109)
                    {
                        Waypoints.Command109 = kvp.Value[1];
                    }
                    else if (kvp.Key == 110)
                    {
                        Waypoints.Command110 = kvp.Value[1];
                    }
                    else if (kvp.Key == 111)
                    {
                        Waypoints.Command111 = kvp.Value[1];
                    }
                    else if (kvp.Key == 112)
                    {
                        Waypoints.Command112 = kvp.Value[1];
                    }
                    else if (kvp.Key == 113)
                    {
                        Waypoints.Command113 = kvp.Value[1];
                    }
                    else if (kvp.Key == 114)
                    {
                        Waypoints.Command114 = kvp.Value[1];
                    }
                    else if (kvp.Key == 115)
                    {
                        Waypoints.Command115 = kvp.Value[1];
                    }
                    else if (kvp.Key == 116)
                    {
                        Waypoints.Command116 = kvp.Value[1];
                    }
                    else if (kvp.Key == 117)
                    {
                        Waypoints.Command117 = kvp.Value[1];
                    }
                    else if (kvp.Key == 118)
                    {
                        AdminChat.Command118 = kvp.Value[1];
                    }
                    else if (kvp.Key == 119)
                    {
                        Mute.Command119 = kvp.Value[1];
                    }
                    else if (kvp.Key == 120)
                    {
                        Whisper.Command120 = kvp.Value[1];
                    }
                    else if (kvp.Key == 121)
                    {
                        Whisper.Command121 = kvp.Value[1];
                    }
                    else if (kvp.Key == 122)
                    {
                        Whisper.Command122 = kvp.Value[1];
                    }
                    else if (kvp.Key == 123)
                    {
                        Whisper.Command123 = kvp.Value[1];
                    }
                    else if (kvp.Key == 124)
                    {
                        ClanManager.Command124 = kvp.Value[1];
                    }
                    else if (kvp.Key == 125)
                    {
                        ClanManager.Command125 = kvp.Value[1];
                    }
                    else if (kvp.Key == 126)
                    {
                        Prayer.Command126 = kvp.Value[1];
                    }
                    else if (kvp.Key == 127)
                    {
                        Hardcore.Command127 = kvp.Value[1];
                    }
                    else if (kvp.Key == 128)
                    {
                        Hardcore.Command128 = kvp.Value[1];
                    }
                    else if (kvp.Key == 129)
                    {
                        ScoutPlayer.Command129 = kvp.Value[1];
                    }
                    else if (kvp.Key == 130)
                    {
                        ScoutPlayer.Command130 = kvp.Value[1];
                    }
                    else if (kvp.Key == 131)
                    {
                        BattleLogger.Command131 = kvp.Value[1];
                    }
                    else if (kvp.Key == 132)
                    {
                        BattleLogger.Command132 = kvp.Value[1];
                    }
                }
            }
            else
            {
                UpdateXml();
            }
        }
    }
}
