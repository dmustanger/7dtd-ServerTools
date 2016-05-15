using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Phrases
    {
        public static SortedDictionary<int, string> Dict = new SortedDictionary<int, string>();
        private const string file = "ServerToolsPhrases.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadPhrases();
            InitFileWatcher();
        }

        private static void LoadPhrases()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdatePhrases();
                return;
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
            XmlNode _configXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _configXml.ChildNodes)
            {
                if (childNode.Name == "Phrases")
                {
                    Dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Phrases' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("id"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of missing 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Phrase"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of missing 'Phrase' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _id;
                        if (!int.TryParse(_line.GetAttribute("id"), out _id))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrase entry because of invalid (non-numeric) value for 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_id))
                        {
                            Dict.Add(_id, _line.GetAttribute("Phrase"));
                        }
                    }
                }
            }
            if (Config.UpdateConfigs)
            {
                UpdatePhrases();
            }
        }

        public static void UpdatePhrases()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Phrases>");
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *************** High Ping Kicker Phrases *************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase1;
                if (!Dict.TryGetValue(1, out _phrase1))
                {
                    _phrase1 = "Auto Kicking {PlayerName} for high ping. ({PlayerPing}) Maxping is {MaxPing}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"1\" Phrase=\"{0}\" />", _phrase1));
                string _phrase2;
                if (!Dict.TryGetValue(2, out _phrase2))
                {
                    _phrase2 = "Auto Kicked: Ping To High. ({PlayerPing}) Max Ping is {MaxPing}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"2\" Phrase=\"{0}\" />", _phrase2));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ***************** Invalid Item Phrases ***************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase3;
                if (!Dict.TryGetValue(3, out _phrase3))
                {
                    _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"3\" Phrase=\"{0}\" />", _phrase3));
                string _phrase4;
                if (!Dict.TryGetValue(4, out _phrase4))
                {
                    _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"4\" Phrase=\"{0}\" />", _phrase4));
                string _phrase5;
                if (!Dict.TryGetValue(5, out _phrase5))
                {
                    _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"5\" Phrase=\"{0}\" />", _phrase5));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Gimme ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase6;
                if (!Dict.TryGetValue(6, out _phrase6))
                {
                    _phrase6 = "{PlayerName} you can only use /gimme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"6\" Phrase=\"{0}\" />", _phrase6));
                string _phrase7;
                if (!Dict.TryGetValue(7, out _phrase7))
                {
                    _phrase7 = "{PlayerName} has received {ItemCount} {ItemName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"7\" Phrase=\"{0}\" />", _phrase7));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Killme ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase8;
                if (!Dict.TryGetValue(8, out _phrase8))
                {
                    _phrase8 = "{PlayerName} you can only use /killme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"8\" Phrase=\"{0}\" />", _phrase8));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** SetHome ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase9;
                if (!Dict.TryGetValue(9, out _phrase9))
                {
                    _phrase9 = "{PlayerName} you already have a home set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"9\" Phrase=\"{0}\" />", _phrase9));
                string _phrase10;
                if (!Dict.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = "{PlayerName} your home has been saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"10\" Phrase=\"{0}\" />", _phrase10));
                string _phrase11;
                if (!Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = "{PlayerName} you do not have a home saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"11\" Phrase=\"{0}\" />", _phrase11));
                string _phrase12;
                if (!Dict.TryGetValue(12, out _phrase12))
                {
                    _phrase12 = "{PlayerName} your home has been removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"12\" Phrase=\"{0}\" />", _phrase12));
                string _phrase13;
                if (!Dict.TryGetValue(13, out _phrase13))
                {
                    _phrase13 = "{PlayerName} you can only use /home once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"13\" Phrase=\"{0}\" />", _phrase13));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Whisper ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase14;
                if (!Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = "{SenderName} player {TargetName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"14\" Phrase=\"{0}\" />", _phrase14));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** ReservedSlots ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase20;
                if (!Dict.TryGetValue(20, out _phrase20))
                {
                    _phrase20 = "{PlayerName} this slot is reserved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"20\" Phrase=\"{0}\" />", _phrase20));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Clan Tag Protection ***************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase100;
                if (!Dict.TryGetValue(100, out _phrase100))
                {
                    _phrase100 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"100\" Phrase=\"{0}\" />", _phrase100));
                string _phrase101;
                if (!Dict.TryGetValue(101, out _phrase101))
                {
                    _phrase101 = "{PlayerName} you have already created the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"101\" Phrase=\"{0}\" />", _phrase101));
                string _phrase102;
                if (!Dict.TryGetValue(102, out _phrase102))
                {
                    _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"102\" Phrase=\"{0}\" />", _phrase102));
                string _phrase103;
                if (!Dict.TryGetValue(103, out _phrase103))
                {
                    _phrase103 = "{PlayerName} you are currently a member of the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"103\" Phrase=\"{0}\" />", _phrase103));
                string _phrase104;
                if (!Dict.TryGetValue(104, out _phrase104))
                {
                    _phrase104 = "{PlayerName} you have add the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"104\" Phrase=\"{0}\" />", _phrase104));
                string _phrase105;
                if (!Dict.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = "{PlayerName} you are not the owner of any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"105\" Phrase=\"{0}\" />", _phrase105));
                string _phrase106;
                if (!Dict.TryGetValue(106, out _phrase106))
                {
                    _phrase106 = "{PlayerName} you have removed the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"106\" Phrase=\"{0}\" />", _phrase106));
                string _phrase107;
                if (!Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"107\" Phrase=\"{0}\" />", _phrase107));
                string _phrase108;
                if (!Dict.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"108\" Phrase=\"{0}\" />", _phrase108));
                string _phrase109;
                if (!Dict.TryGetValue(109, out _phrase109))
                {
                    _phrase109 = "{PlayerName} is already a member of a clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"109\" Phrase=\"{0}\" />", _phrase109));
                string _phrase110;
                if (!Dict.TryGetValue(110, out _phrase110))
                {
                    _phrase110 = "{PlayerName} already has pending clan invites.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"110\" Phrase=\"{0}\" />", _phrase110));
                string _phrase111;
                if (!Dict.TryGetValue(111, out _phrase111))
                {
                    _phrase111 = "{PlayerName} you have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"111\" Phrase=\"{0}\" />", _phrase111));
                string _phrase112;
                if (!Dict.TryGetValue(112, out _phrase112))
                {
                    _phrase112 = "{PlayerName} you have invited {InvitedPlayerName} to the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"112\" Phrase=\"{0}\" />", _phrase112));
                string _phrase113;
                if (!Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = "{PlayerName} you have not been invited to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"113\" Phrase=\"{0}\" />", _phrase113));
                string _phrase114;
                if (!Dict.TryGetValue(114, out _phrase114))
                {
                    _phrase114 = "{PlayerName} the clan could not be found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"114\" Phrase=\"{0}\" />", _phrase114));
                string _phrase115;
                if (!Dict.TryGetValue(115, out _phrase115))
                {
                    _phrase115 = "{PlayerName} has joined the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"115\" Phrase=\"{0}\" />", _phrase115));
                string _phrase116;
                if (!Dict.TryGetValue(116, out _phrase116))
                {
                    _phrase116 = "{PlayerName} you have declined the invite to the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"116\" Phrase=\"{0}\" />", _phrase116));
                string _phrase117;
                if (!Dict.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = "{PlayerName} is not a member of your clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"117\" Phrase=\"{0}\" />", _phrase117));
                string _phrase118;
                if (!Dict.TryGetValue(118, out _phrase118))
                {
                    _phrase118 = "{PlayerName} only the clan owner can remove officers.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"118\" Phrase=\"{0}\" />", _phrase118));
                string _phrase119;
                if (!Dict.TryGetValue(119, out _phrase119))
                {
                    _phrase119 = "{PlayerName} clan owners can not be removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"119\" Phrase=\"{0}\" />", _phrase119));
                string _phrase120;
                if (!Dict.TryGetValue(120, out _phrase120))
                {
                    _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"120\" Phrase=\"{0}\" />", _phrase120));
                string _phrase121;
                if (!Dict.TryGetValue(121, out _phrase121))
                {
                    _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"121\" Phrase=\"{0}\" />", _phrase121));
                string _phrase122;
                if (!Dict.TryGetValue(122, out _phrase122))
                {
                    _phrase122 = "{PlayerName} is already a officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"122\" Phrase=\"{0}\" />", _phrase122));
                string _phrase123;
                if (!Dict.TryGetValue(123, out _phrase123))
                {
                    _phrase123 = "{PlayerName} has been promoted to an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"123\" Phrase=\"{0}\" />", _phrase123));
                string _phrase124;
                if (!Dict.TryGetValue(124, out _phrase124))
                {
                    _phrase124 = "{PlayerName} is not an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"124\" Phrase=\"{0}\" />", _phrase124));
                string _phrase125;
                if (!Dict.TryGetValue(125, out _phrase125))
                {
                    _phrase125 = "{PlayerName} has been demoted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"125\" Phrase=\"{0}\" />", _phrase125));
                string _phrase126;
                if (!Dict.TryGetValue(126, out _phrase126))
                {
                    _phrase126 = "{PlayerName} you can not leave the clan because you are the owner. You can only delete the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"126\" Phrase=\"{0}\" />", _phrase126));
                string _phrase127;
                if (!Dict.TryGetValue(127, out _phrase127))
                {
                    _phrase127 = "{PlayerName} you do not belong to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"127\" Phrase=\"{0}\" />", _phrase127));
                string _phrase128;
                if (!Dict.TryGetValue(128, out _phrase128))
                {
                    _phrase128 = "{PlayerName} the clan {ClanName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"128\" Phrase=\"{0}\" />", _phrase128));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Admins Chat Commands **************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase200;
                if (!Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"200\" Phrase=\"{0}\" />", _phrase200));
                string _phrase201;
                if (!Dict.TryGetValue(201, out _phrase201))
                {
                    _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"201\" Phrase=\"{0}\" />", _phrase201));
                string _phrase202;
                if (!Dict.TryGetValue(202, out _phrase202))
                {
                    _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"202\" Phrase=\"{0}\" />", _phrase202));
                string _phrase203;
                if (!Dict.TryGetValue(203, out _phrase203))
                {
                    _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"203\" Phrase=\"{0}\" />", _phrase203));
                string _phrase204;
                if (!Dict.TryGetValue(204, out _phrase204))
                {
                    _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"204\" Phrase=\"{0}\" />", _phrase204));
                string _phrase205;
                if (!Dict.TryGetValue(205, out _phrase205))
                {
                    _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"205\" Phrase=\"{0}\" />", _phrase205));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************** Day7 ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase300;
                if (!Dict.TryGetValue(300, out _phrase300))
                {
                    _phrase300 = "Server FPS: {Fps}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"300\" Phrase=\"{0}\" />", _phrase300));
                string _phrase301;
                if (!Dict.TryGetValue(301, out _phrase301))
                {
                    _phrase301 = "Next 7th day is in {DaysUntil7} days";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"301\" Phrase=\"{0}\" />", _phrase301));
                string _phrase302;
                if (!Dict.TryGetValue(302, out _phrase302))
                {
                    _phrase302 = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"302\" Phrase=\"{0}\" />", _phrase302));
                string _phrase303;
                if (!Dict.TryGetValue(303, out _phrase303))
                {
                    _phrase303 = "Feral Zombies:{Ferals} Cops:{Cops} Dogs:{Dogs} Bees:{Bees} Screamers:{Screamers}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"303\" Phrase=\"{0}\" />", _phrase303));
                string _phrase304;
                if (!Dict.TryGetValue(304, out _phrase304))
                {
                    _phrase304 = "Bears:{Bears} Stags:{Stags} Pigs:{Pigs} Rabbits:{Rabbits} Chickens:{Chickens}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"304\" Phrase=\"{0}\" />", _phrase304));
                string _phrase305;
                if (!Dict.TryGetValue(305, out _phrase305))
                {
                    _phrase305 = "Total Supply Crates:{SupplyCrates} Total Mini Bikes:{MiniBikes}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"305\" Phrase=\"{0}\" />", _phrase305));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Watchlist ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase350;
                if (!Dict.TryGetValue(350, out _phrase350))
                {
                    _phrase350 = "Player {PlayerName} is on the watchlist for {Reason}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"350\" Phrase=\"{0}\" />", _phrase350));
                sw.WriteLine("    </Phrases>");
                sw.WriteLine("</ServerTools>");
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
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdatePhrases();
            }
            LoadPhrases();
        }
    }
}