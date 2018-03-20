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
        private const double version = Config.version;

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
                sw.WriteLine(string.Format("                <!-- This was generated for version \"{0}\" -->", version));
                sw.WriteLine("        <!-- ******* If your version is incorrect, shutdown, ******** -->");
                sw.WriteLine("        <!-- ************* delete this file, restart **************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine();
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** High_Ping_Kicker ****************** -->");
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
                sw.WriteLine("        <!-- ***************** Invalid_Item_Phrases ***************** -->");
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
                sw.WriteLine("        <!-- ********************** Set_Home ************************ -->");
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
                    _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
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
                sw.WriteLine("        <!-- ******************** Reserved_Slots ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase20;
                if (!Dict.TryGetValue(20, out _phrase20))
                {
                    _phrase20 = "Sorry {PlayerName} server is at max capacity and this slot is reserved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"20\" Phrase=\"{0}\" />", _phrase20));
                string _phrase21;
                if (!Dict.TryGetValue(21, out _phrase21))
                {
                    _phrase21 = "Sorry {PlayerName} server is at max capacity and your reserved status has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"21\" Phrase=\"{0}\" />", _phrase21));
                string _phrase22;
                if (!Dict.TryGetValue(22, out _phrase22))
                {
                    _phrase22 = "Sorry, server is at max capacity and this slot is reserved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"22\" Phrase=\"{0}\" />", _phrase22));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Clan_Tag_Protection ***************** -->");
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
                sw.WriteLine("        <!-- ****************** Admins_Chat_Commands **************** -->");
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
                    _phrase301 = "Next horde night is in {DaysUntilHorde} days";
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
                    _phrase306 = "Next horde night is today";
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
                sw.WriteLine("        <!-- ********************** Reset_Player ******************** -->");
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
                sw.WriteLine("        <!-- ********************** Stop_Server ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase450;
                if (!Dict.TryGetValue(450, out _phrase450))
                {
                    _phrase450 = "Server Shutdown In {Minutes} Minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"450\" Phrase=\"{0}\" />", _phrase450));
                string _phrase451;
                if (!Dict.TryGetValue(451, out _phrase451))
                {
                    _phrase451 = "Saving World Now. Do not exchange items from inventory or build.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"451\" Phrase=\"{0}\" />", _phrase451));
                string _phrase452;
                if (!Dict.TryGetValue(452, out _phrase452))
                {
                    _phrase452 = "Shutdown is in {Minutes} minutes {Seconds} seconds. Please come back after the server restarts.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"452\" Phrase=\"{0}\" />", _phrase452));
                string _phrase453;
                if (!Dict.TryGetValue(453, out _phrase453))
                {
                    _phrase453 = "Shutdown is in 30 seconds. Please come back after the server restarts.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"453\" Phrase=\"{0}\" />", _phrase453));
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
                sw.WriteLine("        <!-- ******************** New_Spawn_Tele ******************** -->");
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
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase601;
                if (!Dict.TryGetValue(601, out _phrase601))
                {
                    _phrase601 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"601\" Phrase=\"{0}\" />", _phrase601));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Vote_Reward ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase602;
                if (!Dict.TryGetValue(602, out _phrase602))
                {
                    _phrase602 = "{PlayerName} you can only use /reward once every {DelayBetweenRewards} hours. Time remaining: {TimeRemaining} hour(s).";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"602\" Phrase=\"{0}\" />", _phrase602)); 
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Travel_Locations ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase603;
                if (!Dict.TryGetValue(603, out _phrase603))
                {
                    _phrase603 = "You have traveled to {Destination}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"603\" Phrase=\"{0}\" />", _phrase603));
                string _phrase604;
                if (!Dict.TryGetValue(604, out _phrase604))
                {
                    _phrase604 = "You are not in a travel location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"604\" Phrase=\"{0}\" />", _phrase604));
                string _phrase605;
                if (!Dict.TryGetValue(605, out _phrase605))
                {
                    _phrase605 = "{PlayerName} you can only use /travel once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"605\" Phrase=\"{0}\" />", _phrase605));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Zone_Protection ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase606;
                if (!Dict.TryGetValue(606, out _phrase606))
                {
                    _phrase606 = "{PlayerName} you can only use /return for two minutes after respawn. Time has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"606\" Phrase=\"{0}\" />", _phrase606));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Set_Home2 ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase607;
                if (!Dict.TryGetValue(607, out _phrase607))
                {
                    _phrase607 = "{PlayerName} your home2 has been saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"607\" Phrase=\"{0}\" />", _phrase607));
                string _phrase608;
                if (!Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = "{PlayerName} you do not have a home2 saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"608\" Phrase=\"{0}\" />", _phrase608));
                string _phrase609;
                if (!Dict.TryGetValue(609, out _phrase609))
                {
                    _phrase609 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"609\" Phrase=\"{0}\" />", _phrase609));
                string _phrase610;
                if (!Dict.TryGetValue(610, out _phrase610))
                {
                    _phrase610 = "{PlayerName} your home2 has been removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"610\" Phrase=\"{0}\" />", _phrase610));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Weather_Vote ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase611;
                if (!Dict.TryGetValue(611, out _phrase611))
                {
                    _phrase611 = "A vote to change the weather has begun and will close in 30 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"611\" Phrase=\"{0}\" />", _phrase611));
                string _phrase612;
                if (!Dict.TryGetValue(612, out _phrase612))
                {
                    _phrase612 = "Weather vote complete but no votes were cast. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"612\" Phrase=\"{0}\" />", _phrase612));
                string _phrase613;
                if (!Dict.TryGetValue(613, out _phrase613))
                {
                    _phrase613 = "Weather vote complete. Most votes went to {weather}. The next weather vote can be started in {VoteDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"613\" Phrase=\"{0}\" />", _phrase613));
                string _phrase614;
                if (!Dict.TryGetValue(614, out _phrase614))
                {
                    _phrase614 = "Weather vote was a tie. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"614\" Phrase=\"{0}\" />", _phrase614));
                string _phrase615;
                if (!Dict.TryGetValue(615, out _phrase615))
                {
                    _phrase615 = "Type /clear, /rain or /snow to cast your vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"615\" Phrase=\"{0}\" />", _phrase615));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Custom_Commands ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase616;
                if (!Dict.TryGetValue(616, out _phrase616))
                {
                    _phrase616 = "{PlayerName} you can only use {Command} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"616\" Phrase=\"{0}\" />", _phrase616));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Coin_Market ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase617;
                if (!Dict.TryGetValue(617, out _phrase617))
                {
                    _phrase617 = "The shop contains the following:";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"617\" Phrase=\"{0}\" />", _phrase617));
                string _phrase618;
                if (!Dict.TryGetValue(618, out _phrase618))
                {
                    _phrase618 = "Type /buy # to purchase the corresponding value from the shop list.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"618\" Phrase=\"{0}\" />", _phrase618));
                string _phrase619;
                if (!Dict.TryGetValue(619, out _phrase619))
                {
                    _phrase619 = "{PlayerName} you are not inside a trade area. Find a trader and use /shop again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"619\" Phrase=\"{0}\" />", _phrase619));
                string _phrase620;
                if (!Dict.TryGetValue(620, out _phrase620))
                {
                    _phrase620 = "{PlayerName} the item # you are trying to buy is not an interger. Please input /buy 1 for example.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"620\" Phrase=\"{0}\" />", _phrase620));
                string _phrase621;
                if (!Dict.TryGetValue(621, out _phrase621))
                {
                    _phrase621 = "{PlayerName} you do not have enough {CoinName}. Your wallet balance is {WalletBalance}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"621\" Phrase=\"{0}\" />", _phrase621));
                string _phrase622;
                if (!Dict.TryGetValue(622, out _phrase622))
                {
                    _phrase622 = "{PlayerName} there was no item # matching the shop goods. Type /shop to review the list.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"622\" Phrase=\"{0}\" />", _phrase622));
                string _phrase623;
                if (!Dict.TryGetValue(623, out _phrase623))
                {
                    _phrase623 = "{PlayerName} there was an error in the shop list. Unable to buy this item. Please alert an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"623\" Phrase=\"{0}\" />", _phrase623));
                string _phrase624;
                if (!Dict.TryGetValue(624, out _phrase624))
                {
                    _phrase624 = "{PlayerName} you are not inside a trade area. Find a trader and use /buy again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"624\" Phrase=\"{0}\" />", _phrase624));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Friend_Teleport ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase625;
                if (!Dict.TryGetValue(625, out _phrase625))
                {
                    _phrase625 = "Your friend {FriendName} with Id # {EntityId} is online.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"625\" Phrase=\"{0}\" />", _phrase625));
                string _phrase626;
                if (!Dict.TryGetValue(626, out _phrase626))
                {
                    _phrase626 = "This {EntityId} is not valid. Only intergers accepted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"626\" Phrase=\"{0}\" />", _phrase626));
                string _phrase627;
                if (!Dict.TryGetValue(627, out _phrase627))
                {
                    _phrase627 = "Sent your friend {FriendsName} a teleport request.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"627\" Phrase=\"{0}\" />", _phrase627));
                string _phrase628;
                if (!Dict.TryGetValue(628, out _phrase628))
                {
                    _phrase628 = "{SenderName} would like to teleport to you. Type /accept in chat in the next 30 seconds to accept the request.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"628\" Phrase=\"{0}\" />", _phrase628));
                string _phrase629;
                if (!Dict.TryGetValue(629, out _phrase629))
                {
                    _phrase629 = "Did not find EntityId {EntityId}. No teleport request sent.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"629\" Phrase=\"{0}\" />", _phrase629));
                string _phrase630;
                if (!Dict.TryGetValue(630, out _phrase630))
                {
                    _phrase630 = "{PlayerName} you can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"630\" Phrase=\"{0}\" />", _phrase630));
                string _phrase631;
                if (!Dict.TryGetValue(631, out _phrase631))
                {
                    _phrase631 = "Your request was accepted. Teleported you to your friend.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"631\" Phrase=\"{0}\" />", _phrase631));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Voting ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase700;
                if (!Dict.TryGetValue(700, out _phrase700))
                {
                    _phrase700 = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"700\" Phrase=\"{0}\" />", _phrase700));
                string _phrase701;
                if (!Dict.TryGetValue(701, out _phrase701))
                {
                    _phrase701 = "Thank you for your vote { PlayerName}. You can vote and receive another reward in {VoteDelay} hours.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"701\" Phrase=\"{0}\" />", _phrase701));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Flight_Check ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase705;
                if (!Dict.TryGetValue(705, out _phrase705))
                {
                    _phrase705 = "Detected {PlayerName} flying @ {X} {Y} {Z}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"705\" Phrase=\"{0}\" />", _phrase705));
                string _phrase706;
                if (!Dict.TryGetValue(706, out _phrase706))
                {
                    _phrase706 = "{PlayerName} has been detected flying.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"706\" Phrase=\"{0}\" />", _phrase706));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Underground_Check ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase710;
                if (!Dict.TryGetValue(710, out _phrase710))
                {
                    _phrase710 = "Detected {PlayerName} flying underground @ {X} {Y} {Z}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"710\" Phrase=\"{0}\" />", _phrase710));
                string _phrase711;
                if (!Dict.TryGetValue(711, out _phrase711))
                {
                    _phrase711 = "{PlayerName} has been detected flying underground.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"711\" Phrase=\"{0}\" />", _phrase711));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase715;
                if (!Dict.TryGetValue(715, out _phrase715))
                {
                    _phrase715 = "{PlayerName} has tracked down an animal to within {Radius} metres.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"715\" Phrase=\"{0}\" />", _phrase715));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Hatch_Elevator ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase720;
                if (!Dict.TryGetValue(720, out _phrase720))
                {
                    _phrase720 = "You are stunned and have broken your leg while smashing yourself through hatches. Ouch!";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"720\" Phrase=\"{0}\" />", _phrase720));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Admin_List *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase725;
                if (!Dict.TryGetValue(725, out _phrase725))
                {
                    _phrase725 = "Server admins in game: [FF8000]";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"725\" Phrase=\"{0}\" />", _phrase725));
                string _phrase726;
                if (!Dict.TryGetValue(726, out _phrase726))
                {
                    _phrase726 = "Server moderators in game: [FF8000]";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"726\" Phrase=\"{0}\" />", _phrase726));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Auto_Shutdown ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase730;
                if (!Dict.TryGetValue(730, out _phrase730))
                {
                    _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"730\" Phrase=\"{0}\" />", _phrase730));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Death_Spot ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase735;
                if (!Dict.TryGetValue(735, out _phrase735))
                {
                    _phrase735 = "{PlayerName} you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"735\" Phrase=\"{0}\" />", _phrase735));
                string _phrase736;
                if (!Dict.TryGetValue(736, out _phrase736))
                {
                    _phrase736 = "Teleported you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"736\" Phrase=\"{0}\" />", _phrase736));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Restart_Vote ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase740;
                if (!Dict.TryGetValue(740, out _phrase740))
                {
                    _phrase740 = "A vote to restart the server has begun and will close in 30 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"740\" Phrase=\"{0}\" />", _phrase740));
                string _phrase741;
                if (!Dict.TryGetValue(741, out _phrase741))
                {
                    _phrase741 = "There are not enough players online to start a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"741\" Phrase=\"{0}\" />", _phrase741));
                string _phrase742;
                if (!Dict.TryGetValue(742, out _phrase742))
                {
                    _phrase742 = "Players voted yes to a server restart. Shutdown has been initiated.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"742\" Phrase=\"{0}\" />", _phrase742));
                string _phrase743;
                if (!Dict.TryGetValue(743, out _phrase743))
                {
                    _phrase743 = "Players voted yes but not enough votes cast to restart. A new vote can open in {RestartDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"743\" Phrase=\"{0}\" />", _phrase743));
                string _phrase744;
                if (!Dict.TryGetValue(744, out _phrase744))
                {
                    _phrase744 = "Players voted no to a server restart. A new vote can open in {RestartDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"744\" Phrase=\"{0}\" />", _phrase744));
                string _phrase745;
                if (!Dict.TryGetValue(745, out _phrase745))
                {
                    _phrase745 = "The restart vote was a tie. A new vote can open in {RestartDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"745\" Phrase=\"{0}\" />", _phrase745));
                string _phrase746;
                if (!Dict.TryGetValue(746, out _phrase746))
                {
                    _phrase746 = "No votes were cast to restart the server.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"746\" Phrase=\"{0}\" />", _phrase746));
                string _phrase747;
                if (!Dict.TryGetValue(747, out _phrase747))
                {
                    _phrase747 = "You started the last restart vote. Another player must initiate the next vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"747\" Phrase=\"{0}\" />", _phrase747));
                string _phrase748;
                if (!Dict.TryGetValue(748, out _phrase748))
                {
                    _phrase748 = "{Player} has requested a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"748\" Phrase=\"{0}\" />", _phrase748));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Item_Cleanup ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase750;
                if (!Dict.TryGetValue(750, out _phrase750))
                {
                    _phrase750 = "Too many items detected on the ground in a pile. Cleared them away.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"750\" Phrase=\"{0}\" />", _phrase750));
                string _phrase751;
                if (!Dict.TryGetValue(751, out _phrase751))
                {
                    _phrase751 = "Detected items in a pile and cleaned it up. Last removed item was @";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"751\" Phrase=\"{0}\" />", _phrase751));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* More_Phrases ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase800;
                if (!Dict.TryGetValue(800, out _phrase800))
                {
                    _phrase800 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"800\" Phrase=\"{0}\" />", _phrase800));
                string _phrase801;
                if (!Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{Killer} has murdered you while you were in a protected zone.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"801\" Phrase=\"{0}\" />", _phrase801));
                string _phrase802;
                if (!Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"802\" Phrase=\"{0}\" />", _phrase802));
                string _phrase803;
                if (!Dict.TryGetValue(803, out _phrase803))
                {
                    _phrase803 = "{Killer} has murdered you while they were in a protected zone.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"803\" Phrase=\"{0}\" />", _phrase803));
                string _phrase804;
                if (!Dict.TryGetValue(804, out _phrase804))
                {
                    _phrase804 = "{Count} {ItemName} was sent to your inventory by an admin. If your bag is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"804\" Phrase=\"{0}\" />", _phrase804));
                string _phrase805;
                if (!Dict.TryGetValue(805, out _phrase805))
                {
                    _phrase805 = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"805\" Phrase=\"{0}\" />", _phrase805));
                string _phrase806;
                if (!Dict.TryGetValue(806, out _phrase806))
                {
                    _phrase806 = "You have received your starting items.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"806\" Phrase=\"{0}\" />", _phrase806));
                string _phrase807;
                if (!Dict.TryGetValue(807, out _phrase807))
                {
                    _phrase807 = "OH NO! How did that get in there? You have received a zombie.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"807\" Phrase=\"{0}\" />", _phrase807));
                string _phrase808;
                if (!Dict.TryGetValue(808, out _phrase808))
                {
                    _phrase808 = "No spawn points were found near you. Move locations and try /reward again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"808\" Phrase=\"{0}\" />", _phrase808));
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