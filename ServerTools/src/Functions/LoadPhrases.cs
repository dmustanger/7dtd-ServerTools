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
            LoadXml();
            InitFileWatcher();
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
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
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
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
                UpdateXml();
            }
        }

        public static void UpdateXml()
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
                string _phrase15;
                if (!Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = "{SenderName} no one has pm'd you.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"15\" Phrase=\"{0}\" />", _phrase15));
                string _phrase16;
                if (!Dict.TryGetValue(16, out _phrase16))
                {
                    _phrase16 = "{SenderName} the player is not online.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"16\" Phrase=\"{0}\" />", _phrase16));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** ReservedSlots ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase20;
                if (!Dict.TryGetValue(20, out _phrase20))
                {
                    _phrase20 = "Sorry {PlayerName} this slot is reserved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"20\" Phrase=\"{0}\" />", _phrase20));
                string _phrase21;
                if (!Dict.TryGetValue(21, out _phrase21))
                {
                    _phrase21 = "Sorry {PlayerName} your reserved slot has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"21\" Phrase=\"{0}\" />", _phrase21));
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
                    _phrase104 = "{PlayerName} you have added the clan {ClanName}.";
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
                string _phrase129;
                if (!Dict.TryGetValue(129, out _phrase129))
                {
                    _phrase129 = "{PlayerName} the clanName must be longer the 3 characters.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"129\" Phrase=\"{0}\" />", _phrase129));
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
                    _phrase303 = "Feral Zombies:{Ferals} Radiated Zombies:{Radiated} Dogs:{Dogs} Vultures:{Vultures} Screamers:{Screamers}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"303\" Phrase=\"{0}\" />", _phrase303));
                string _phrase304;
                if (!Dict.TryGetValue(304, out _phrase304))
                {
                    _phrase304 = "Bears:{Bears} Stags:{Stags} Boars:{Boars} Rabbits:{Rabbits} Chickens:{Chickens} Snakes:{Snakes} Wolves:{Wolves}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"304\" Phrase=\"{0}\" />", _phrase304));
                string _phrase305;
                if (!Dict.TryGetValue(305, out _phrase305))
                {
                    _phrase305 = "Total Supply Crates:{SupplyCrates} Total Mini Bikes:{MiniBikes}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"305\" Phrase=\"{0}\" />", _phrase305));
                string _phrase306;
                if (!Dict.TryGetValue(306, out _phrase306))
                {
                    _phrase306 = "Next 7th day is today";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"306\" Phrase=\"{0}\" />", _phrase306));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Watchlist ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase350;
                if (!Dict.TryGetValue(350, out _phrase350))
                {
                    _phrase350 = "Player {PlayerName} is on the watchlist for {Reason}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"350\" Phrase=\"{0}\" />", _phrase350));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Reset Player ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase400;
                if (!Dict.TryGetValue(400, out _phrase400))
                {
                    _phrase400 = "Reseting your player profile.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"400\" Phrase=\"{0}\" />", _phrase400));
                string _phrase401;
                if (!Dict.TryGetValue(401, out _phrase401))
                {
                    _phrase401 = "You have reset the profile for player {SteamId}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"401\" Phrase=\"{0}\" />", _phrase401));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Stop Server ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase450;
                if (!Dict.TryGetValue(450, out _phrase450))
                {
                    _phrase450 = "Server Restarting In {Minutes} Minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"450\" Phrase=\"{0}\" />", _phrase450));
                string _phrase451;
                if (!Dict.TryGetValue(451, out _phrase451))
                {
                    _phrase451 = "Saving World Now. Do not exchange items from inventory or build until after restart.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"451\" Phrase=\"{0}\" />", _phrase451));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Jail ************************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase500;
                if (!Dict.TryGetValue(500, out _phrase500))
                {
                    _phrase500 = "{PlayerName} you have been sent to jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"500\" Phrase=\"{0}\" />", _phrase500));
                string _phrase501;
                if (!Dict.TryGetValue(501, out _phrase501))
                {
                    _phrase501 = "{PlayerName} you have been released from jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"501\" Phrase=\"{0}\" />", _phrase501));
                string _phrase502;
                if (!Dict.TryGetValue(502, out _phrase502))
                {
                    _phrase502 = "{PlayerName} you have set the jail position as {JailPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"502\" Phrase=\"{0}\" />", _phrase502));
                string _phrase503;
                if (!Dict.TryGetValue(503, out _phrase503))
                {
                    _phrase503 = "{PlayerName} the jail position jas not been set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"503\" Phrase=\"{0}\" />", _phrase503));
                string _phrase504;
                if (!Dict.TryGetValue(504, out _phrase504))
                {
                    _phrase504 = "{AdminPlayerName} player {PlayerName} is already in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"504\" Phrase=\"{0}\" />", _phrase504));
                string _phrase505;
                if (!Dict.TryGetValue(505, out _phrase505))
                {
                    _phrase505 = "{AdminPlayerName} you have put {PlayerName} in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"505\" Phrase=\"{0}\" />", _phrase505));
                string _phrase506;
                if (!Dict.TryGetValue(506, out _phrase506))
                {
                    _phrase506 = "{AdminPlayerName} player {PlayerName} is not in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"506\" Phrase=\"{0}\" />", _phrase506));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** New Spawn Tele ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase525;
                if (!Dict.TryGetValue(525, out _phrase525))
                {
                    _phrase525 = "{PlayerName} you have set the New Spawn position as {NewSpawnTelePosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"525\" Phrase=\"{0}\" />", _phrase525));
                string _phrase526;
                if (!Dict.TryGetValue(526, out _phrase526))
                {
                    _phrase526 = "{PlayerName} you have been teleported to the new spawn location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"526\" Phrase=\"{0}\" />", _phrase526));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** AnimalTracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase601;
                if (!Dict.TryGetValue(601, out _phrase601))
                {
                    _phrase601 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"601\" Phrase=\"{0}\" />", _phrase601));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** VoteReward ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase602;
                if (!Dict.TryGetValue(602, out _phrase602))
                {
                    _phrase602 = "{PlayerName} you can only use /reward once every 24 hours. Time remaining: {TimeRemaining} hour(s).";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"602\" Phrase=\"{0}\" />", _phrase602)); 
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
                UpdateXml();
            }
            LoadXml();
        }
    }
}