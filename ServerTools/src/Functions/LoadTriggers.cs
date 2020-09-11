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
                if (childNode.Name == "Triggers")
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
                        if (!_line.HasAttribute("Number"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing Number attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Number"), out int _Number))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Event Triggers because of invalid (non-numeric) value for 'Number' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Default"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing Default attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Replacement"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Event Triggers entry because of missing Replacement attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string[] _c = { _line.GetAttribute("Default"), _line.GetAttribute("Replacement") };
                        if (!Dict.ContainsKey(_Number))
                        {
                            Dict.Add(_Number, _c);
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
                sw.WriteLine("    <Triggers>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("        <Trigger Number=\"{0}\" Default=\"{1}\" Replacement=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Trigger Number=\"1\" Default=\"sethome\" Replacement=\"sethome\" />");
                    sw.WriteLine("        <Trigger Number=\"2\" Default=\"home\" Replacement=\"home\" />");
                    sw.WriteLine("        <Trigger Number=\"3\" Default=\"fhome\" Replacement=\"fhome\" />");
                    sw.WriteLine("        <Trigger Number=\"4\" Default=\"delhome\" Replacement=\"delhome\" />");
                    sw.WriteLine("        <Trigger Number=\"5\" Default=\"sethome2\" Replacement=\"sethome2\" />");
                    sw.WriteLine("        <Trigger Number=\"6\" Default=\"home2\" Replacement=\"home2\" />");
                    sw.WriteLine("        <Trigger Number=\"7\" Default=\"fhome2\" Replacement=\"fhome2\" />");
                    sw.WriteLine("        <Trigger Number=\"8\" Default=\"delhome2\" Replacement=\"delhome2\" />");
                    sw.WriteLine("        <Trigger Number=\"9\" Default=\"go\" Replacement=\"go\" />");
                    sw.WriteLine("        <Trigger Number=\"10\" Default=\"go way\" Replacement=\"go way\" />");
                    sw.WriteLine("        <Trigger Number=\"11\" Default=\"top3\" Replacement=\"top3\" />");
                    sw.WriteLine("        <Trigger Number=\"12\" Default=\"score\" Replacement=\"score\" />");
                    sw.WriteLine("        <Trigger Number=\"13\" Default=\"mute\" Replacement=\"mute\" />");
                    sw.WriteLine("        <Trigger Number=\"14\" Default=\"unmute\" Replacement=\"unmute\" />");
                    sw.WriteLine("        <Trigger Number=\"15\" Default=\"commands\" Replacement=\"commands\" />");
                    sw.WriteLine("        <Trigger Number=\"16\" Default=\"day7\" Replacement=\"day7\" />");
                    sw.WriteLine("        <Trigger Number=\"17\" Default=\"day\" Replacement=\"day\" />");
                    sw.WriteLine("        <Trigger Number=\"18\" Default=\"bloodmoon\" Replacement=\"bloodmoon\" />");
                    sw.WriteLine("        <Trigger Number=\"19\" Default=\"bm\" Replacement=\"bm\" />");
                    sw.WriteLine("        <Trigger Number=\"20\" Default=\"killme\" Replacement=\"killme\" />");
                    sw.WriteLine("        <Trigger Number=\"21\" Default=\"wrist\" Replacement=\"wrist\" />");
                    sw.WriteLine("        <Trigger Number=\"22\" Default=\"hang\" Replacement=\"hang\" />");
                    sw.WriteLine("        <Trigger Number=\"23\" Default=\"suicide\" Replacement=\"suicide\" />");
                    sw.WriteLine("        <Trigger Number=\"24\" Default=\"gimme\" Replacement=\"gimme\" />");
                    sw.WriteLine("        <Trigger Number=\"25\" Default=\"gimmie\" Replacement=\"gimmie\" />");
                    sw.WriteLine("        <Trigger Number=\"26\" Default=\"set jail\" Replacement=\"set jail\" />");
                    sw.WriteLine("        <Trigger Number=\"27\" Default=\"jail\" Replacement=\"jail\" />");
                    sw.WriteLine("        <Trigger Number=\"28\" Default=\"unjail\" Replacement=\"unjail\" />");
                    sw.WriteLine("        <Trigger Number=\"29\" Default=\"setspawn\" Replacement=\"setspawn\" />");
                    sw.WriteLine("        <Trigger Number=\"30\" Default=\"trackanimal\" Replacement=\"trackanimal\" />");
                    sw.WriteLine("        <Trigger Number=\"31\" Default=\"track\" Replacement=\"track\" />");
                    sw.WriteLine("        <Trigger Number=\"32\" Default=\"claim\" Replacement=\"claim\" />");
                    sw.WriteLine("        <Trigger Number=\"33\" Default=\"clan add\" Replacement=\"clan add\" />");
                    sw.WriteLine("        <Trigger Number=\"34\" Default=\"clan del\" Replacement=\"clan del\" />");
                    sw.WriteLine("        <Trigger Number=\"35\" Default=\"clan invite\" Replacement=\"clan invite\" />");
                    sw.WriteLine("        <Trigger Number=\"36\" Default=\"clan accept\" Replacement=\"clan accept\" />");
                    sw.WriteLine("        <Trigger Number=\"37\" Default=\"clan decline\" Replacement=\"clan decline\" />");
                    sw.WriteLine("        <Trigger Number=\"38\" Default=\"clan remove\" Replacement=\"clan remove\" />");
                    sw.WriteLine("        <Trigger Number=\"39\" Default=\"clan promote\" Replacement=\"clan promote\" />");
                    sw.WriteLine("        <Trigger Number=\"40\" Default=\"clan demote\" Replacement=\"clan demote\" />");
                    sw.WriteLine("        <Trigger Number=\"41\" Default=\"clan leave\" Replacement=\"clan leave\" />");
                    sw.WriteLine("        <Trigger Number=\"42\" Default=\"clan commands\" Replacement=\"clan commands\" />");
                    sw.WriteLine("        <Trigger Number=\"43\" Default=\"clan chat\" Replacement=\"clan chat\" />");
                    sw.WriteLine("        <Trigger Number=\"44\" Default=\"clan rename\" Replacement=\"clan rename\" />");
                    sw.WriteLine("        <Trigger Number=\"45\" Default=\"clan request\" Replacement=\"clan request\" />");
                    sw.WriteLine("        <Trigger Number=\"46\" Default=\"reward\" Replacement=\"reward\" />");
                    sw.WriteLine("        <Trigger Number=\"47\" Default=\"shutdown\" Replacement=\"shutdown\" />");
                    sw.WriteLine("        <Trigger Number=\"48\" Default=\"adminlist\" Replacement=\"adminlist\" />");
                    sw.WriteLine("        <Trigger Number=\"49\" Default=\"travel\" Replacement=\"travel\" />");
                    sw.WriteLine("        <Trigger Number=\"50\" Default=\"return\" Replacement=\"return\" />");
                    sw.WriteLine("        <Trigger Number=\"51\" Default=\"marketback\" Replacement=\"marketback\" />");
                    sw.WriteLine("        <Trigger Number=\"52\" Default=\"mback\" Replacement=\"mback\" />");
                    sw.WriteLine("        <Trigger Number=\"53\" Default=\"lobbyback\" Replacement=\"lobbyback\" />");
                    sw.WriteLine("        <Trigger Number=\"54\" Default=\"lback\" Replacement=\"lback\" />");
                    sw.WriteLine("        <Trigger Number=\"55\" Default=\"forgive\" Replacement=\"forgive\" />");
                    sw.WriteLine("        <Trigger Number=\"56\" Default=\"wallet\" Replacement=\"wallet\" />");
                    sw.WriteLine("        <Trigger Number=\"57\" Default=\"shop\" Replacement=\"shop\" />");
                    sw.WriteLine("        <Trigger Number=\"58\" Default=\"shop buy\" Replacement=\"shop buy\" />");
                    sw.WriteLine("        <Trigger Number=\"59\" Default=\"friend\" Replacement=\"friend\" />");
                    sw.WriteLine("        <Trigger Number=\"60\" Default=\"accept\" Replacement=\"accept\" />");
                    sw.WriteLine("        <Trigger Number=\"61\" Default=\"died\" Replacement=\"died\" />");
                    sw.WriteLine("        <Trigger Number=\"62\" Default=\"weathervote\" Replacement=\"weathervote\" />");
                    sw.WriteLine("        <Trigger Number=\"63\" Default=\"sun\" Replacement=\"sun\" />");
                    sw.WriteLine("        <Trigger Number=\"64\" Default=\"rain\" Replacement=\"rain\" />");
                    sw.WriteLine("        <Trigger Number=\"65\" Default=\"snow\" Replacement=\"snow\" />");
                    sw.WriteLine("        <Trigger Number=\"66\" Default=\"restartvote\" Replacement=\"restartvote\" />");
                    sw.WriteLine("        <Trigger Number=\"67\" Default=\"mutevote\" Replacement=\"mutevote\" />");
                    sw.WriteLine("        <Trigger Number=\"68\" Default=\"kickvote\" Replacement=\"kickvote\" />");
                    sw.WriteLine("        <Trigger Number=\"69\" Default=\"reserved\" Replacement=\"reserved\" />");
                    sw.WriteLine("        <Trigger Number=\"70\" Default=\"yes\" Replacement=\"yes\" />");
                    sw.WriteLine("        <Trigger Number=\"71\" Default=\"auction\" Replacement=\"auction\" />");
                    sw.WriteLine("        <Trigger Number=\"72\" Default=\"auction cancel\" Replacement=\"auction cancel\" />");
                    sw.WriteLine("        <Trigger Number=\"73\" Default=\"auction buy\" Replacement=\"auction buy\" />");
                    sw.WriteLine("        <Trigger Number=\"74\" Default=\"auction sell\" Replacement=\"auction sell\" />");
                    sw.WriteLine("        <Trigger Number=\"75\" Default=\"fps\" Replacement=\"fps\" />");
                    sw.WriteLine("        <Trigger Number=\"76\" Default=\"loc\" Replacement=\"loc\" />");
                    sw.WriteLine("        <Trigger Number=\"77\" Default=\"bike\" Replacement=\"bike\" />");
                    sw.WriteLine("        <Trigger Number=\"78\" Default=\"minibike\" Replacement=\"minibike\" />");
                    sw.WriteLine("        <Trigger Number=\"79\" Default=\"motorbike\" Replacement=\"motorbike\" />");
                    sw.WriteLine("        <Trigger Number=\"80\" Default=\"jeep\" Replacement=\"jeep\" />");
                    sw.WriteLine("        <Trigger Number=\"81\" Default=\"gyro\" Replacement=\"gyro\" />");
                    sw.WriteLine("        <Trigger Number=\"82\" Default=\"report\" Replacement=\"report\" />");
                    sw.WriteLine("        <Trigger Number=\"83\" Default=\"bounty\" Replacement=\"bounty\" />");
                    sw.WriteLine("        <Trigger Number=\"84\" Default=\"lottery\" Replacement=\"lottery\" />");
                    sw.WriteLine("        <Trigger Number=\"85\" Default=\"lottery enter\" Replacement=\"lottery enter\" />");
                    sw.WriteLine("        <Trigger Number=\"86\" Default=\"ready\" Replacement=\"ready\" />");
                    sw.WriteLine("        <Trigger Number=\"87\" Default=\"setlobby\" Replacement=\"setlobby\" />");
                    sw.WriteLine("        <Trigger Number=\"88\" Default=\"lobby\" Replacement=\"lobby\" />");
                    sw.WriteLine("        <Trigger Number=\"89\" Default=\"playerlist\" Replacement=\"playerlist\" />");
                    sw.WriteLine("        <Trigger Number=\"90\" Default=\"stuck\" Replacement=\"stuck\" />");
                    sw.WriteLine("        <Trigger Number=\"91\" Default=\"pollyes\" Replacement=\"pollyes\" />");
                    sw.WriteLine("        <Trigger Number=\"92\" Default=\"pollno\" Replacement=\"pollno\" />");
                    sw.WriteLine("        <Trigger Number=\"93\" Default=\"poll\" Replacement=\"poll\" />");
                    sw.WriteLine("        <Trigger Number=\"94\" Default=\"bank\" Replacement=\"bank\" />");
                    sw.WriteLine("        <Trigger Number=\"95\" Default=\"deposit\" Replacement=\"deposit\" />");
                    sw.WriteLine("        <Trigger Number=\"96\" Default=\"withdraw\" Replacement=\"withdraw\" />");
                    sw.WriteLine("        <Trigger Number=\"97\" Default=\"wallet deposit\" Replacement=\"wallet deposit\" />");
                    sw.WriteLine("        <Trigger Number=\"98\" Default=\"wallet withdraw\" Replacement=\"wallet withdraw\" />");
                    sw.WriteLine("        <Trigger Number=\"99\" Default=\"transfer\" Replacement=\"transfer\" />");
                    sw.WriteLine("        <Trigger Number=\"100\" Default=\"join\" Replacement=\"event\" />");
                    sw.WriteLine("        <Trigger Number=\"101\" Default=\"buy life\" Replacement=\"buy life\" />");
                    sw.WriteLine("        <Trigger Number=\"102\" Default=\"setmarket\" Replacement=\"setmarket\" />");
                    sw.WriteLine("        <Trigger Number=\"103\" Default=\"market\" Replacement=\"market\" />");
                    sw.WriteLine("        <Trigger Number=\"104\" Default=\"infoticker\" Replacement=\"infoticker\" />");
                    sw.WriteLine("        <Trigger Number=\"105\" Default=\"session\" Replacement=\"session\" />");
                    sw.WriteLine("        <Trigger Number=\"106\" Default=\"waypoint\" Replacement=\"waypoint\" />");
                    sw.WriteLine("        <Trigger Number=\"107\" Default=\"way\" Replacement=\"way\" />");
                    sw.WriteLine("        <Trigger Number=\"108\" Default=\"wp\" Replacement=\"wp\" />");
                    sw.WriteLine("        <Trigger Number=\"109\" Default=\"fwaypoint\" Replacement=\"fwaypoint\" />");
                    sw.WriteLine("        <Trigger Number=\"110\" Default=\"fway\" Replacement=\"fway\" />");
                    sw.WriteLine("        <Trigger Number=\"111\" Default=\"fwp\" Replacement=\"fwp\" />");
                    sw.WriteLine("        <Trigger Number=\"112\" Default=\"waypoint save\" Replacement=\"waypoint save\" />");
                    sw.WriteLine("        <Trigger Number=\"113\" Default=\"way save\" Replacement=\"way save\" />");
                    sw.WriteLine("        <Trigger Number=\"114\" Default=\"ws\" Replacement=\"ws\" />");
                    sw.WriteLine("        <Trigger Number=\"115\" Default=\"waypoint del\" Replacement=\"waypoint del\" />");
                    sw.WriteLine("        <Trigger Number=\"116\" Default=\"way del\" Replacement=\"way del\" />");
                    sw.WriteLine("        <Trigger Number=\"117\" Default=\"wd\" Replacement=\"wd\" />");
                    sw.WriteLine("        <Trigger Number=\"118\" Default=\"admin\" Replacement=\"admin\" />");
                    sw.WriteLine("        <Trigger Number=\"119\" Default=\"mutelist\" Replacement=\"mutelist\" />");
                    sw.WriteLine("        <Trigger Number=\"120\" Default=\"pmessage\" Replacement=\"pmessage\" />");
                    sw.WriteLine("        <Trigger Number=\"121\" Default=\"pm\" Replacement=\"pm\" />");
                    sw.WriteLine("        <Trigger Number=\"122\" Default=\"rmessage\" Replacement=\"rmessage\" />");
                    sw.WriteLine("        <Trigger Number=\"123\" Default=\"rm\" Replacement=\"rm\" />");
                    sw.WriteLine("        <Trigger Number=\"124\" Default=\"cc\" Replacement=\"cc\" />");
                    sw.WriteLine("        <Trigger Number=\"125\" Default=\"clanlist\" Replacement=\"clanlist\" />");
                    sw.WriteLine("        <Trigger Number=\"126\" Default=\"pray\" Replacement=\"pray\" />");
                    sw.WriteLine("        <Trigger Number=\"127\" Default=\"hardcore\" Replacement=\"hardcore\" />");
                    sw.WriteLine("        <Trigger Number=\"128\" Default=\"hardcore on\" Replacement=\"hardcore on\" />");
                    sw.WriteLine("        <Trigger Number=\"129\" Default=\"scoutplayer\" Replacement=\"scoutplayer\" />");
                    sw.WriteLine("        <Trigger Number=\"130\" Default=\"scout\" Replacement=\"scout\" />");
                    sw.WriteLine("        <Trigger Number=\"131\" Default=\"exit\" Replacement=\"exit\" />");
                    sw.WriteLine("        <Trigger Number=\"132\" Default=\"quit\" Replacement=\"quit\" />");
                }
                sw.WriteLine("    </Triggers>");
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
                        Home.Command1 = kvp.Value[1];
                    }
                    else if (kvp.Key == 2)
                    {
                        Home.Command2 = kvp.Value[1];
                    }
                    else if (kvp.Key == 3)
                    {
                        Home.Command3 = kvp.Value[1];
                    }
                    else if (kvp.Key == 4)
                    {
                        Home.Command4 = kvp.Value[1];
                    }
                    else if (kvp.Key == 5)
                    {
                        Home.Command5 = kvp.Value[1];
                    }
                    else if (kvp.Key == 6)
                    {
                        Home.Command6 = kvp.Value[1];
                    }
                    else if (kvp.Key == 7)
                    {
                        Home.Command7 = kvp.Value[1];
                    }
                    else if (kvp.Key == 8)
                    {
                        Home.Command8 = kvp.Value[1];
                    }
                    else if (kvp.Key == 9)
                    {
                        Home.Command9 = kvp.Value[1];
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
                        Auction.Command71 = kvp.Value[1];
                    }
                    else if (kvp.Key == 72)
                    {
                        Auction.Command72 = kvp.Value[1];
                    }
                    else if (kvp.Key == 73)
                    {
                        Auction.Command73 = kvp.Value[1];
                    }
                    else if (kvp.Key == 74)
                    {
                        Auction.Command74 = kvp.Value[1];
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
