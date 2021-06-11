using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Phrases
    {
        public static SortedDictionary<int, string> Dict = new SortedDictionary<int, string>();
        private const string file = "Phrases.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                        if (!_line.HasAttribute("Id"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of missing 'Id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Phrase"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of missing 'Phrase' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _phrase = _line.GetAttribute("Phrase");
                        if (!int.TryParse(_line.GetAttribute("Id"), out int _id))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of invalid (non-numeric) value for 'Id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_id))
                        {
                            Dict.Add(_id, _phrase);
                        }
                        else
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because the id already exists in the list. Id: {0}", _id));
                            continue;
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ServerTools>");
                sw.WriteLine("    <Phrases>");
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine(string.Format("        <!-- *********************** V.{0} ************************* -->", Config.version));
                sw.WriteLine("        <!-- ******* If your version is incorrect, shutdown, ******** -->");
                sw.WriteLine("        <!-- ************* delete this file, restart **************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine();
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** High_Ping_Kicker ****************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(1, out string _phrase1))
                {
                    _phrase1 = "Auto Kicking {PlayerName} for high ping of {PlayerPing}. Maxping is {MaxPing}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1\" Phrase=\"{0}\" />", _phrase1));
                if (!Dict.TryGetValue(2, out string _phrase2))
                {
                    _phrase2 = "Auto Kicked: Ping is too high at {PlayerPing}. Max ping is {MaxPing}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"2\" Phrase=\"{0}\" />", _phrase2));
                if (!Dict.TryGetValue(3, out string _phrase3))
                {
                    _phrase3 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"3\" Phrase=\"{0}\" />", _phrase3));
                if (!Dict.TryGetValue(4, out string _phrase4))
                {
                    _phrase4 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"4\" Phrase=\"{0}\" />", _phrase4));
                if (!Dict.TryGetValue(5, out string _phrase5))
                {
                    _phrase5 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"5\" Phrase=\"{0}\" />", _phrase5));
                if (!Dict.TryGetValue(6, out string _phrase6))
                {
                    _phrase6 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"6\" Phrase=\"{0}\" />", _phrase6));
                if (!Dict.TryGetValue(7, out string _phrase7))
                {
                    _phrase7 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"7\" Phrase=\"{0}\" />", _phrase7));
                if (!Dict.TryGetValue(8, out string _phrase8))
                {
                    _phrase8 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"8\" Phrase=\"{0}\" />", _phrase8));
                if (!Dict.TryGetValue(9, out string _phrase9))
                {
                    _phrase9 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"9\" Phrase=\"{0}\" />", _phrase9));
                if (!Dict.TryGetValue(10, out string _phrase10))
                {
                    _phrase10 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"10\" Phrase=\"{0}\" />", _phrase10));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Invalid_Items ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(11, out string _phrase11))
                {
                    _phrase11 = "You have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"11\" Phrase=\"{0}\" />", _phrase11));
                if (!Dict.TryGetValue(12, out string _phrase12))
                {
                    _phrase12 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"12\" Phrase=\"{0}\" />", _phrase12));
                if (!Dict.TryGetValue(13, out string _phrase13))
                {
                    _phrase13 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"13\" Phrase=\"{0}\" />", _phrase13));
                if (!Dict.TryGetValue(14, out string _phrase14))
                {
                    _phrase14 = "Automatic detection: Invalid item {1}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"14\" Phrase=\"{0}\" />", _phrase14));
                if (!Dict.TryGetValue(15, out string _phrase15))
                {
                    _phrase15 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"15\" Phrase=\"{0}\" />", _phrase15));
                if (!Dict.TryGetValue(16, out string _phrase16))
                {
                    _phrase16 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"16\" Phrase=\"{0}\" />", _phrase16));
                if (!Dict.TryGetValue(17, out string _phrase17))
                {
                    _phrase17 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"17\" Phrase=\"{0}\" />", _phrase17));
                if (!Dict.TryGetValue(18, out string _phrase18))
                {
                    _phrase18 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"18\" Phrase=\"{0}\" />", _phrase18));
                if (!Dict.TryGetValue(19, out string _phrase19))
                {
                    _phrase19 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"19\" Phrase=\"{0}\" />", _phrase19));
                if (!Dict.TryGetValue(20, out string _phrase20))
                {
                    _phrase20 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"20\" Phrase=\"{0}\" />", _phrase20));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Gimme ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(21, out string _phrase21))
                {
                    _phrase21 = "You can only use {CommandPrivate}{Command24} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"21\" Phrase=\"{0}\" />", _phrase21));
                if (!Dict.TryGetValue(22, out string _phrase22))
                {
                    _phrase22 = "Received {ItemCount} {ItemName} from gimme.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"22\" Phrase=\"{0}\" />", _phrase22));
                if (!Dict.TryGetValue(23, out string _phrase23))
                {
                    _phrase23 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"23\" Phrase=\"{0}\" />", _phrase23));
                if (!Dict.TryGetValue(24, out string _phrase24))
                {
                    _phrase24 = "OH NO! How did that get in there? You have received a zombie.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"24\" Phrase=\"{0}\" />", _phrase24));
                if (!Dict.TryGetValue(25, out string _phrase25))
                {
                    _phrase25 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"25\" Phrase=\"{0}\" />", _phrase25));
                if (!Dict.TryGetValue(26, out string _phrase26))
                {
                    _phrase26 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"26\" Phrase=\"{0}\" />", _phrase26));
                if (!Dict.TryGetValue(27, out string _phrase27))
                {
                    _phrase27 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"27\" Phrase=\"{0}\" />", _phrase27));
                if (!Dict.TryGetValue(28, out string _phrase28))
                {
                    _phrase28 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"28\" Phrase=\"{0}\" />", _phrase28));
                if (!Dict.TryGetValue(29, out string _phrase29))
                {
                    _phrase29 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"29\" Phrase=\"{0}\" />", _phrase29));
                if (!Dict.TryGetValue(30, out string _phrase30))
                {
                    _phrase30 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"30\" Phrase=\"{0}\" />", _phrase30));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Suicide ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(31, out string _phrase31))
                {
                    _phrase31 = "You can only use {CommandPrivate}{Command20}, {CommandPrivate}{Command21}, {CommandPrivate}{Command22}, or {CommandPrivate}{Command23} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"31\" Phrase=\"{0}\" />", _phrase31));
                if (!Dict.TryGetValue(32, out string _phrase32))
                {
                    _phrase32 = "You are too close to a player that is not a friend. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"32\" Phrase=\"{0}\" />", _phrase32));
                if (!Dict.TryGetValue(33, out string _phrase33))
                {
                    _phrase33 = "You are too close to a zombie. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"33\" Phrase=\"{0}\" />", _phrase33));
                if (!Dict.TryGetValue(34, out string _phrase34))
                {
                    _phrase34 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"34\" Phrase=\"{0}\" />", _phrase34));
                if (!Dict.TryGetValue(35, out string _phrase35))
                {
                    _phrase35 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"35\" Phrase=\"{0}\" />", _phrase35));
                if (!Dict.TryGetValue(36, out string _phrase36))
                {
                    _phrase36 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"36\" Phrase=\"{0}\" />", _phrase36));
                if (!Dict.TryGetValue(37, out string _phrase37))
                {
                    _phrase37 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"37\" Phrase=\"{0}\" />", _phrase37));
                if (!Dict.TryGetValue(38, out string _phrase38))
                {
                    _phrase38 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"38\" Phrase=\"{0}\" />", _phrase38));
                if (!Dict.TryGetValue(39, out string _phrase39))
                {
                    _phrase39 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"39\" Phrase=\"{0}\" />", _phrase39));
                if (!Dict.TryGetValue(40, out string _phrase40))
                {
                    _phrase40 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"40\" Phrase=\"{0}\" />", _phrase40));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************** Fps ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(41, out string _phrase41))
                {
                    _phrase41 = "Server FPS: {Fps}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"41\" Phrase=\"{0}\" />", _phrase41));
                if (!Dict.TryGetValue(42, out string _phrase42))
                {
                    _phrase42 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"42\" Phrase=\"{0}\" />", _phrase42));
                if (!Dict.TryGetValue(43, out string _phrase43))
                {
                    _phrase43 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"43\" Phrase=\"{0}\" />", _phrase43));
                if (!Dict.TryGetValue(44, out string _phrase44))
                {
                    _phrase44 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"44\" Phrase=\"{0}\" />", _phrase44));
                if (!Dict.TryGetValue(45, out string _phrase45))
                {
                    _phrase45 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"45\" Phrase=\"{0}\" />", _phrase45));
                if (!Dict.TryGetValue(46, out string _phrase46))
                {
                    _phrase46 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"46\" Phrase=\"{0}\" />", _phrase46));
                if (!Dict.TryGetValue(47, out string _phrase47))
                {
                    _phrase47 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"47\" Phrase=\"{0}\" />", _phrase47));
                if (!Dict.TryGetValue(48, out string _phrase48))
                {
                    _phrase48 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"48\" Phrase=\"{0}\" />", _phrase48));
                if (!Dict.TryGetValue(49, out string _phrase49))
                {
                    _phrase49 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"49\" Phrase=\"{0}\" />", _phrase49));
                if (!Dict.TryGetValue(50, out string _phrase50))
                {
                    _phrase50 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"50\" Phrase=\"{0}\" />", _phrase50));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Whisper ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(51, out string _phrase51))
                {
                    _phrase51 = "{PlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"51\" Phrase=\"{0}\" />", _phrase51));
                if (!Dict.TryGetValue(52, out string _phrase52))
                {
                    _phrase52 = "No one has pm'd you.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"52\" Phrase=\"{0}\" />", _phrase52));
                if (!Dict.TryGetValue(53, out string _phrase53))
                {
                    _phrase53 = "Invalid message used to whisper.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"53\" Phrase=\"{0}\" />", _phrase53));
                if (!Dict.TryGetValue(54, out string _phrase54))
                {
                    _phrase54 = "The player is not online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"54\" Phrase=\"{0}\" />", _phrase54));
                if (!Dict.TryGetValue(55, out string _phrase55))
                {
                    _phrase55 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"55\" Phrase=\"{0}\" />", _phrase55));
                if (!Dict.TryGetValue(56, out string _phrase56))
                {
                    _phrase56 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"56\" Phrase=\"{0}\" />", _phrase56));
                if (!Dict.TryGetValue(57, out string _phrase57))
                {
                    _phrase57 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"57\" Phrase=\"{0}\" />", _phrase57));
                if (!Dict.TryGetValue(58, out string _phrase58))
                {
                    _phrase58 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"58\" Phrase=\"{0}\" />", _phrase58));
                if (!Dict.TryGetValue(59, out string _phrase59))
                {
                    _phrase59 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"59\" Phrase=\"{0}\" />", _phrase59));
                if (!Dict.TryGetValue(60, out string _phrase60))
                {
                    _phrase60 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"60\" Phrase=\"{0}\" />", _phrase60));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Reserved_Slots ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(61, out string _phrase61))
                {
                    _phrase61 = "{ServerResponseName} - The server is full. You were kicked by the reservation system to open a slot.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"61\" Phrase=\"{0}\" />", _phrase61));
                if (!Dict.TryGetValue(62, out string _phrase62))
                {
                    _phrase62 = "Sorry {PlayerName} you have been kicked with the longest session time. Please wait {TimeRemaining} minutes before rejoining.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"62\" Phrase=\"{0}\" />", _phrase62));
                if (!Dict.TryGetValue(63, out string _phrase63))
                {
                    _phrase63 = "The player is not online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"63\" Phrase=\"{0}\" />", _phrase63));
                if (!Dict.TryGetValue(64, out string _phrase64))
                {
                    _phrase64 = "Your reserved status expires on {DateTime}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"64\" Phrase=\"{0}\" />", _phrase64));
                if (!Dict.TryGetValue(65, out string _phrase65))
                {
                    _phrase65 = "Your reserved status has expired on {DateTime}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"65\" Phrase=\"{0}\" />", _phrase65));
                if (!Dict.TryGetValue(66, out string _phrase66))
                {
                    _phrase66 = "You are not on the reservation list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"66\" Phrase=\"{0}\" />", _phrase66));
                if (!Dict.TryGetValue(67, out string _phrase67))
                {
                    _phrase67 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"67\" Phrase=\"{0}\" />", _phrase67));
                if (!Dict.TryGetValue(68, out string _phrase68))
                {
                    _phrase68 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"68\" Phrase=\"{0}\" />", _phrase68));
                if (!Dict.TryGetValue(69, out string _phrase69))
                {
                    _phrase69 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"69\" Phrase=\"{0}\" />", _phrase69));
                if (!Dict.TryGetValue(70, out string _phrase70))
                {
                    _phrase70 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"70\" Phrase=\"{0}\" />", _phrase70));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Clan_Manager ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(71, out string _phrase71))
                {
                    _phrase71 = "You have already created the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"71\" Phrase=\"{0}\" />", _phrase71));
                if (!Dict.TryGetValue(72, out string _phrase72))
                {
                    _phrase72 = "Can not add the clan {ClanName} because it already exists.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"72\" Phrase=\"{0}\" />", _phrase72));
                if (!Dict.TryGetValue(73, out string _phrase73))
                {
                    _phrase73 = "You are currently a member of the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"73\" Phrase=\"{0}\" />", _phrase73));
                if (!Dict.TryGetValue(74, out string _phrase74))
                {
                    _phrase74 = "You have added the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"74\" Phrase=\"{0}\" />", _phrase74));
                if (!Dict.TryGetValue(75, out string _phrase75))
                {
                    _phrase75 = "You are not the owner of any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"75\" Phrase=\"{0}\" />", _phrase75));
                if (!Dict.TryGetValue(76, out string _phrase76))
                {
                    _phrase76 = "You have removed the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"76\" Phrase=\"{0}\" />", _phrase76));
                if (!Dict.TryGetValue(77, out string _phrase77))
                {
                    _phrase77 = "You do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"77\" Phrase=\"{0}\" />", _phrase77));
                if (!Dict.TryGetValue(78, out string _phrase78))
                {
                    _phrase78 = "The player {PlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"78\" Phrase=\"{0}\" />", _phrase78));
                if (!Dict.TryGetValue(79, out string _phrase79))
                {
                    _phrase79 = "{PlayerName} is already a member of a clan named {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"79\" Phrase=\"{0}\" />", _phrase79));
                if (!Dict.TryGetValue(80, out string _phrase80))
                {
                    _phrase80 = "{PlayerName} already has a clan invitation.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"80\" Phrase=\"{0}\" />", _phrase80));
                if (!Dict.TryGetValue(81, out string _phrase81))
                {
                    _phrase81 = "You have been invited to join the clan {ClanName}. Type {CommandPrivate}{Command36} to join or {CommandPrivate}{Command37} to decline the offer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"81\" Phrase=\"{0}\" />", _phrase81));
                if (!Dict.TryGetValue(82, out string _phrase82))
                {
                    _phrase82 = "You have invited {PlayerName} to the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"82\" Phrase=\"{0}\" />", _phrase82));
                if (!Dict.TryGetValue(83, out string _phrase83))
                {
                    _phrase83 = "You have not been invited to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"83\" Phrase=\"{0}\" />", _phrase83));
                if (!Dict.TryGetValue(84, out string _phrase84))
                {
                    _phrase84 = "The clan could not be found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"84\" Phrase=\"{0}\" />", _phrase84));
                if (!Dict.TryGetValue(85, out string _phrase85))
                {
                    _phrase85 = "{PlayerName} has declined the invite to the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"85\" Phrase=\"{0}\" />", _phrase85));
                if (!Dict.TryGetValue(86, out string _phrase86))
                {
                    _phrase86 = "You have declined the invite to the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"86\" Phrase=\"{0}\" />", _phrase86));
                if (!Dict.TryGetValue(87, out string _phrase87))
                {
                    _phrase87 = "{PlayerName} is not a member of your clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"87\" Phrase=\"{0}\" />", _phrase87));
                if (!Dict.TryGetValue(88, out string _phrase88))
                {
                    _phrase88 = "Only the clan owner can remove officers.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"88\" Phrase=\"{0}\" />", _phrase88));
                if (!Dict.TryGetValue(89, out string _phrase89))
                {
                    _phrase89 = "Clan owners can not be removed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"89\" Phrase=\"{0}\" />", _phrase89));
                if (!Dict.TryGetValue(90, out string _phrase90))
                {
                    _phrase90 = "You have removed {PlayerName} from clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"90\" Phrase=\"{0}\" />", _phrase90));
                if (!Dict.TryGetValue(91, out string _phrase91))
                {
                    _phrase91 = "You have been removed from the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"91\" Phrase=\"{0}\" />", _phrase91));
                if (!Dict.TryGetValue(92, out string _phrase92))
                {
                    _phrase92 = "{PlayerName} is already a officer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"92\" Phrase=\"{0}\" />", _phrase92));
                if (!Dict.TryGetValue(93, out string _phrase93))
                {
                    _phrase93 = "{PlayerName} has been promoted to an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"93\" Phrase=\"{0}\" />", _phrase93));
                if (!Dict.TryGetValue(94, out string _phrase94))
                {
                    _phrase94 = "{PlayerName} is not an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"94\" Phrase=\"{0}\" />", _phrase94));
                if (!Dict.TryGetValue(95, out string _phrase95))
                {
                    _phrase95 = "{PlayerName} has been demoted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"95\" Phrase=\"{0}\" />", _phrase95));
                if (!Dict.TryGetValue(96, out string _phrase96))
                {
                    _phrase96 = "You can not leave the clan because you are the owner. You can only delete the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"96\" Phrase=\"{0}\" />", _phrase96));
                if (!Dict.TryGetValue(97, out string _phrase97))
                {
                    _phrase97 = "You do not belong to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"97\" Phrase=\"{0}\" />", _phrase97));
                if (!Dict.TryGetValue(98, out string _phrase98))
                {
                    _phrase98 = "The clan {ClanName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"98\" Phrase=\"{0}\" />", _phrase98));
                if (!Dict.TryGetValue(99, out string _phrase99))
                {
                    _phrase99 = "{PlayerName} has joined the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"99\" Phrase=\"{0}\" />", _phrase99));
                if (!Dict.TryGetValue(100, out string _phrase100))
                {
                    _phrase100 = "You have changed your clan name to {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"100\" Phrase=\"{0}\" />", _phrase100));
                if (!Dict.TryGetValue(101, out string _phrase101))
                {
                    _phrase101 = "Your clan name has been changed by the owner to {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"101\" Phrase=\"{0}\" />", _phrase101));
                if (!Dict.TryGetValue(102, out string _phrase102))
                {
                    _phrase102 = "{PlayerName} has left the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"102\" Phrase=\"{0}\" />", _phrase102));
                if (!Dict.TryGetValue(103, out string _phrase103))
                {
                    _phrase103 = "The player is not online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"103\" Phrase=\"{0}\" />", _phrase103));
                if (!Dict.TryGetValue(104, out string _phrase104))
                {
                    _phrase104 = "Clan names are:";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"104\" Phrase=\"{0}\" />", _phrase104));
                if (!Dict.TryGetValue(105, out string _phrase105))
                {
                    _phrase105 = "No clans were found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"105\" Phrase=\"{0}\" />", _phrase105));
                if (!Dict.TryGetValue(106, out string _phrase106))
                {
                    _phrase106 = "The clan name is too short or too long. It must be 2 to {MaxLength} characters.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"106\" Phrase=\"{0}\" />", _phrase106));
                if (!Dict.TryGetValue(107, out string _phrase107))
                {
                    _phrase107 = "{PlayerName} has joined another clan. Request removed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"107\" Phrase=\"{0}\" />", _phrase107));
                if (!Dict.TryGetValue(108, out string _phrase108))
                {
                    _phrase108 = "There is a request to join the clan from {PlayerName}. Type {CommandPrivate}{Command36} to join or {CommandPrivate}{Command37} to decline the offer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"108\" Phrase=\"{0}\" />", _phrase108));
                if (!Dict.TryGetValue(109, out string _phrase109))
                {
                    _phrase109 = "There are no requests to join the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"109\" Phrase=\"{0}\" />", _phrase109));
                if (!Dict.TryGetValue(110, out string _phrase110))
                {
                    _phrase110 = "You have sent a request to join the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"110\" Phrase=\"{0}\" />", _phrase110));
                if (!Dict.TryGetValue(111, out string _phrase111))
                {
                    _phrase111 = "You have already sent a request to join this clan.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"111\" Phrase=\"{0}\" />", _phrase111));
                if (!Dict.TryGetValue(112, out string _phrase112))
                {
                    _phrase112 = "That clan name was not found on the list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"112\" Phrase=\"{0}\" />", _phrase112));
                if (!Dict.TryGetValue(113, out string _phrase113))
                {
                    _phrase113 = "Available clan commands are:";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"113\" Phrase=\"{0}\" />", _phrase113));
                if (!Dict.TryGetValue(114, out string _phrase114))
                {
                    _phrase114 = "Usage: {CommandPrivate}{Command33} playerName.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"114\" Phrase=\"{0}\" />", _phrase114));
                if (!Dict.TryGetValue(115, out string _phrase115))
                {
                    _phrase115 = "Usage: {CommandPrivate}{Command35} playerName.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"115\" Phrase=\"{0}\" />", _phrase115));
                if (!Dict.TryGetValue(116, out string _phrase116))
                {
                    _phrase116 = "Usage: {CommandPrivate}{Command38} playerName.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"116\" Phrase=\"{0}\" />", _phrase116));
                if (!Dict.TryGetValue(117, out string _phrase117))
                {
                    _phrase117 = "Usage: {CommandPrivate}{Command39} playerName.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"117\" Phrase=\"{0}\" />", _phrase117));
                if (!Dict.TryGetValue(118, out string _phrase118))
                {
                    _phrase118 = "Usage: {CommandPrivate}{Command40} playerName.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"118\" Phrase=\"{0}\" />", _phrase118));
                if (!Dict.TryGetValue(119, out string _phrase119))
                {
                    _phrase119 = "Usage: {CommandPrivate}{Command43} message or {CommandPrivate}{Command124}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"119\" Phrase=\"{0}\" />", _phrase119));
                if (!Dict.TryGetValue(120, out string _phrase120))
                {
                    _phrase120 = "Usage: {CommandPrivate}{Command44} Name.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"120\" Phrase=\"{0}\" />", _phrase120));
                if (!Dict.TryGetValue(121, out string _phrase121))
                {
                    _phrase121 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"121\" Phrase=\"{0}\" />", _phrase121));
                if (!Dict.TryGetValue(122, out string _phrase122))
                {
                    _phrase122 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"122\" Phrase=\"{0}\" />", _phrase122));
                if (!Dict.TryGetValue(123, out string _phrase123))
                {
                    _phrase123 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"123\" Phrase=\"{0}\" />", _phrase123));
                if (!Dict.TryGetValue(124, out string _phrase124))
                {
                    _phrase124 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"124\" Phrase=\"{0}\" />", _phrase124));
                if (!Dict.TryGetValue(125, out string _phrase125))
                {
                    _phrase125 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"125\" Phrase=\"{0}\" />", _phrase125));
                if (!Dict.TryGetValue(126, out string _phrase126))
                {
                    _phrase126 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"126\" Phrase=\"{0}\" />", _phrase126));
                if (!Dict.TryGetValue(127, out string _phrase127))
                {
                    _phrase127 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"127\" Phrase=\"{0}\" />", _phrase127));
                if (!Dict.TryGetValue(128, out string _phrase128))
                {
                    _phrase128 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"128\" Phrase=\"{0}\" />", _phrase128));
                if (!Dict.TryGetValue(129, out string _phrase129))
                {
                    _phrase129 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"129\" Phrase=\"{0}\" />", _phrase129));
                if (!Dict.TryGetValue(130, out string _phrase130))
                {
                    _phrase130 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"130\" Phrase=\"{0}\" />", _phrase130));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Watchlist ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(140, out string _phrase140))
                {
                    _phrase140 = "{PlayerName} is on the watchlist for {Reason}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"140\" Phrase=\"{0}\" />", _phrase140));
                if (!Dict.TryGetValue(141, out string _phrase141))
                {
                    _phrase141 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"141\" Phrase=\"{0}\" />", _phrase141));
                if (!Dict.TryGetValue(142, out string _phrase142))
                {
                    _phrase142 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"142\" Phrase=\"{0}\" />", _phrase142));
                if (!Dict.TryGetValue(143, out string _phrase143))
                {
                    _phrase143 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"143\" Phrase=\"{0}\" />", _phrase143));
                if (!Dict.TryGetValue(144, out string _phrase144))
                {
                    _phrase144 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"144\" Phrase=\"{0}\" />", _phrase144));
                if (!Dict.TryGetValue(145, out string _phrase145))
                {
                    _phrase145 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"145\" Phrase=\"{0}\" />", _phrase145));
                if (!Dict.TryGetValue(146, out string _phrase146))
                {
                    _phrase146 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"146\" Phrase=\"{0}\" />", _phrase146));
                if (!Dict.TryGetValue(147, out string _phrase147))
                {
                    _phrase147 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"147\" Phrase=\"{0}\" />", _phrase147));
                if (!Dict.TryGetValue(148, out string _phrase148))
                {
                    _phrase148 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"148\" Phrase=\"{0}\" />", _phrase148));
                if (!Dict.TryGetValue(149, out string _phrase149))
                {
                    _phrase149 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"149\" Phrase=\"{0}\" />", _phrase149));
                if (!Dict.TryGetValue(150, out string _phrase150))
                {
                    _phrase150 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"150\" Phrase=\"{0}\" />", _phrase150));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Reset_Player ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(151, out string _phrase151))
                {
                    _phrase151 = "Reseting your player profile.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"151\" Phrase=\"{0}\" />", _phrase151));
                if (!Dict.TryGetValue(152, out string _phrase152))
                {
                    _phrase152 = "You have reset the profile for player {Steamid}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"152\" Phrase=\"{0}\" />", _phrase152));
                if (!Dict.TryGetValue(153, out string _phrase153))
                {
                    _phrase153 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"153\" Phrase=\"{0}\" />", _phrase153));
                if (!Dict.TryGetValue(154, out string _phrase154))
                {
                    _phrase154 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"154\" Phrase=\"{0}\" />", _phrase154));
                if (!Dict.TryGetValue(155, out string _phrase155))
                {
                    _phrase155 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"155\" Phrase=\"{0}\" />", _phrase155));
                if (!Dict.TryGetValue(156, out string _phrase156))
                {
                    _phrase156 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"156\" Phrase=\"{0}\" />", _phrase156));
                if (!Dict.TryGetValue(157, out string _phrase157))
                {
                    _phrase157 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"157\" Phrase=\"{0}\" />", _phrase157));
                if (!Dict.TryGetValue(158, out string _phrase158))
                {
                    _phrase158 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"158\" Phrase=\"{0}\" />", _phrase158));
                if (!Dict.TryGetValue(159, out string _phrase159))
                {
                    _phrase159 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"159\" Phrase=\"{0}\" />", _phrase159));
                if (!Dict.TryGetValue(160, out string _phrase160))
                {
                    _phrase160 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"160\" Phrase=\"{0}\" />", _phrase160));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Stop_Server ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(170, out string _phrase170))
                {
                    _phrase170 = "Server Shutdown In {Value} Minute.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"170\" Phrase=\"{0}\" />", _phrase170));
                if (!Dict.TryGetValue(171, out string _phrase171))
                {
                    _phrase171 = "Saving World Now. Do not exchange items from inventory or build.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"171\" Phrase=\"{0}\" />", _phrase171));
                if (!Dict.TryGetValue(172, out string _phrase172))
                {
                    _phrase172 = "Shutdown is in 30 seconds. Please come back after the server restarts.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"172\" Phrase=\"{0}\" />", _phrase172));
                if (!Dict.TryGetValue(173, out string _phrase173))
                {
                    _phrase173 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"173\" Phrase=\"{0}\" />", _phrase173));
                if (!Dict.TryGetValue(174, out string _phrase174))
                {
                    _phrase174 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"174\" Phrase=\"{0}\" />", _phrase174));
                if (!Dict.TryGetValue(175, out string _phrase175))
                {
                    _phrase175 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"175\" Phrase=\"{0}\" />", _phrase175));
                if (!Dict.TryGetValue(176, out string _phrase176))
                {
                    _phrase176 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"176\" Phrase=\"{0}\" />", _phrase176));
                if (!Dict.TryGetValue(177, out string _phrase177))
                {
                    _phrase177 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"177\" Phrase=\"{0}\" />", _phrase177));
                if (!Dict.TryGetValue(178, out string _phrase178))
                {
                    _phrase178 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"178\" Phrase=\"{0}\" />", _phrase178));
                if (!Dict.TryGetValue(179, out string _phrase179))
                {
                    _phrase179 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"179\" Phrase=\"{0}\" />", _phrase179));
                if (!Dict.TryGetValue(180, out string _phrase180))
                {
                    _phrase180 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"180\" Phrase=\"{0}\" />", _phrase180));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Jail ************************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(190, out string _phrase190))
                {
                    _phrase190 = "You have been sent to jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"190\" Phrase=\"{0}\" />", _phrase190));
                if (!Dict.TryGetValue(191, out string _phrase191))
                {
                    _phrase191 = "You have been released from jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"191\" Phrase=\"{0}\" />", _phrase191));
                if (!Dict.TryGetValue(192, out string _phrase192))
                {
                    _phrase192 = "You have set the jail position as {JailPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"192\" Phrase=\"{0}\" />", _phrase192));
                if (!Dict.TryGetValue(193, out string _phrase193))
                {
                    _phrase193 = "The jail position has not been set.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"193\" Phrase=\"{0}\" />", _phrase193));
                if (!Dict.TryGetValue(194, out string _phrase194))
                {
                    _phrase194 = "Player {PlayerName} is already in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"194\" Phrase=\"{0}\" />", _phrase194));
                if (!Dict.TryGetValue(195, out string _phrase195))
                {
                    _phrase195 = "You have put {PlayerName} in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"195\" Phrase=\"{0}\" />", _phrase195));
                if (!Dict.TryGetValue(196, out string _phrase196))
                {
                    _phrase196 = "Player {PlayerName} is not in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"196\" Phrase=\"{0}\" />", _phrase196));
                if (!Dict.TryGetValue(197, out string _phrase197))
                {
                    _phrase197 = "The jail is electrified. Do not try to leave it.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"197\" Phrase=\"{0}\" />", _phrase197));
                if (!Dict.TryGetValue(198, out string _phrase198))
                {
                    _phrase198 = "Do not pee on the electric fence.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"198\" Phrase=\"{0}\" />", _phrase198));
                if (!Dict.TryGetValue(199, out string _phrase199))
                {
                    _phrase199 = "You do not have permission to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"199\" Phrase=\"{0}\" />", _phrase199));
                if (!Dict.TryGetValue(200, out string _phrase200))
                {
                    _phrase200 = "Player {PlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"200\" Phrase=\"{0}\" />", _phrase200));
                if (!Dict.TryGetValue(201, out string _phrase201))
                {
                    _phrase201 = "You have forgiven {PlayerName} and released them from jail.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"201\" Phrase=\"{0}\" />", _phrase201));
                if (!Dict.TryGetValue(202, out string _phrase202))
                {
                    _phrase202 = "You have been forgiven and released from jail by {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"202\" Phrase=\"{0}\" />", _phrase202));
                if (!Dict.TryGetValue(203, out string _phrase203))
                {
                    _phrase203 = "Player {PlayerName} has not spawned. Try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"203\" Phrase=\"{0}\" />", _phrase203));
                if (!Dict.TryGetValue(204, out string _phrase204))
                {
                    _phrase204 = "[FF0000]{PlayerName} has been jailed for attempted murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"204\" Phrase=\"{0}\" />", _phrase204));
                if (!Dict.TryGetValue(205, out string _phrase205))
                {
                    _phrase205 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"205\" Phrase=\"{0}\" />", _phrase205));
                if (!Dict.TryGetValue(206, out string _phrase206))
                {
                    _phrase206 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"206\" Phrase=\"{0}\" />", _phrase206));
                if (!Dict.TryGetValue(207, out string _phrase207))
                {
                    _phrase207 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"207\" Phrase=\"{0}\" />", _phrase207));
                if (!Dict.TryGetValue(208, out string _phrase208))
                {
                    _phrase208 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"208\" Phrase=\"{0}\" />", _phrase208));
                if (!Dict.TryGetValue(209, out string _phrase209))
                {
                    _phrase209 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"209\" Phrase=\"{0}\" />", _phrase209));
                if (!Dict.TryGetValue(210, out string _phrase210))
                {
                    _phrase210 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"210\" Phrase=\"{0}\" />", _phrase210));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** New_Spawn_Tele ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(211, out string _phrase211))
                {
                    _phrase211 = "You have set the new spawn position as {Position}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"211\" Phrase=\"{0}\" />", _phrase211));
                if (!Dict.TryGetValue(212, out string _phrase212))
                {
                    _phrase212 = "You have been teleported to the new spawn location.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"212\" Phrase=\"{0}\" />", _phrase212));
                if (!Dict.TryGetValue(213, out string _phrase213))
                {
                    _phrase213 = "Type {CommandPrivate}{Command86} when you are prepared to leave. You will teleport back to your spawn location.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"213\" Phrase=\"{0}\" />", _phrase213));
                if (!Dict.TryGetValue(214, out string _phrase214))
                {
                    _phrase214 = "You have no saved return point or you have used it.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"214\" Phrase=\"{0}\" />", _phrase214));
                if (!Dict.TryGetValue(215, out string _phrase215))
                {
                    _phrase215 = "You have left the new player area. Return to it before using {CommandPrivate}{Command86}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"215\" Phrase=\"{0}\" />", _phrase215));
                if (!Dict.TryGetValue(216, out string _phrase216))
                {
                    _phrase216 = "You have been sent back to your original spawn location. Good luck.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"216\" Phrase=\"{0}\" />", _phrase216));
                if (!Dict.TryGetValue(217, out string _phrase217))
                {
                    _phrase217 = "You do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"217\" Phrase=\"{0}\" />", _phrase217));
                if (!Dict.TryGetValue(218, out string _phrase218))
                {
                    _phrase218 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"218\" Phrase=\"{0}\" />", _phrase218));
                if (!Dict.TryGetValue(219, out string _phrase219))
                {
                    _phrase219 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"219\" Phrase=\"{0}\" />", _phrase219));
                if (!Dict.TryGetValue(220, out string _phrase220))
                {
                    _phrase220 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"220\" Phrase=\"{0}\" />", _phrase220));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Lottery ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(221, out string _phrase221))
                {
                    _phrase221 = "There is no open lottery. Type {CommandPrivate}{Command84} # to open a new lottery at that buy in price. You must have enough in your wallet.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"221\" Phrase=\"{0}\" />", _phrase221));
                if (!Dict.TryGetValue(222, out string _phrase222))
                {
                    _phrase222 = "A lottery is open for {Value} {CoinName}. Minimum buy in is {BuyIn}. Enter it by typing {CommandPrivate}{Command85}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"222\" Phrase=\"{0}\" />", _phrase222));
                if (!Dict.TryGetValue(223, out string _phrase223))
                {
                    _phrase223 = "You must type a valid integer above zero for the lottery #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"223\" Phrase=\"{0}\" />", _phrase223));
                if (!Dict.TryGetValue(224, out string _phrase224))
                {
                    _phrase224 = "You have opened a new lottery for {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"224\" Phrase=\"{0}\" />", _phrase224));
                if (!Dict.TryGetValue(225, out string _phrase225))
                {
                    _phrase225 = "A lottery has opened for {Value} {CoinName} and will draw soon. Type {CommandPrivate}{Command85} to join.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"225\" Phrase=\"{0}\" />", _phrase225));
                if (!Dict.TryGetValue(226, out string _phrase226))
                {
                    _phrase226 = "You do not have enough {CoinName}. Earn some more and enter the lottery before it ends.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"226\" Phrase=\"{0}\" />", _phrase226));
                if (!Dict.TryGetValue(227, out string _phrase227))
                {
                    _phrase227 = "You have entered the lottery, good luck in the draw.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"227\" Phrase=\"{0}\" />", _phrase227));
                if (!Dict.TryGetValue(228, out string _phrase228))
                {
                    _phrase228 = "You are already in the lottery, good luck in the draw.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"228\" Phrase=\"{0}\" />", _phrase228));
                if (!Dict.TryGetValue(229, out string _phrase229))
                {
                    _phrase229 = "A lottery draw will begin in five minutes. Type {CommandPrivate}{Command85} to join the draw.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"229\" Phrase=\"{0}\" />", _phrase229));
                if (!Dict.TryGetValue(230, out string _phrase230))
                {
                    _phrase230 = "Winner! {PlayerName} has won the lottery and received {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"230\" Phrase=\"{0}\" />", _phrase230));
                if (!Dict.TryGetValue(231, out string _phrase231))
                {
                    _phrase231 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"231\" Phrase=\"{0}\" />", _phrase231));
                if (!Dict.TryGetValue(232, out string _phrase232))
                {
                    _phrase232 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"232\" Phrase=\"{0}\" />", _phrase232));
                if (!Dict.TryGetValue(233, out string _phrase233))
                {
                    _phrase233 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"233\" Phrase=\"{0}\" />", _phrase233));
                if (!Dict.TryGetValue(234, out string _phrase234))
                {
                    _phrase234 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"234\" Phrase=\"{0}\" />", _phrase234));
                if (!Dict.TryGetValue(235, out string _phrase235))
                {
                    _phrase235 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"235\" Phrase=\"{0}\" />", _phrase235));
                if (!Dict.TryGetValue(236, out string _phrase236))
                {
                    _phrase236 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"236\" Phrase=\"{0}\" />", _phrase236));
                if (!Dict.TryGetValue(237, out string _phrase237))
                {
                    _phrase237 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"237\" Phrase=\"{0}\" />", _phrase237));
                if (!Dict.TryGetValue(238, out string _phrase238))
                {
                    _phrase238 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"238\" Phrase=\"{0}\" />", _phrase238));
                if (!Dict.TryGetValue(239, out string _phrase239))
                {
                    _phrase239 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"239\" Phrase=\"{0}\" />", _phrase239));
                if (!Dict.TryGetValue(240, out string _phrase240))
                {
                    _phrase240 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"240\" Phrase=\"{0}\" />", _phrase240));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Lobby ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(241, out string _phrase241))
                {
                    _phrase241 = "You can only use {CommandPrivate}{Command88} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"241\" Phrase=\"{0}\" />", _phrase241));
                if (!Dict.TryGetValue(242, out string _phrase242))
                {
                    _phrase242 = "You have set the lobby position as {LobbyPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"242\" Phrase=\"{0}\" />", _phrase242));
                if (!Dict.TryGetValue(243, out string _phrase243))
                {
                    _phrase243 = "You can go back by typing {CommandPrivate}{Command53} when you are ready to leave the lobby.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"243\" Phrase=\"{0}\" />", _phrase243));
                if (!Dict.TryGetValue(244, out string _phrase244))
                {
                    _phrase244 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"244\" Phrase=\"{0}\" />", _phrase244));
                if (!Dict.TryGetValue(245, out string _phrase245))
                {
                    _phrase245 = "The lobby position is not set.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"245\" Phrase=\"{0}\" />", _phrase245));
                if (!Dict.TryGetValue(246, out string _phrase246))
                {
                    _phrase246 = "You have no return point saved.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"246\" Phrase=\"{0}\" />", _phrase246));
                if (!Dict.TryGetValue(247, out string _phrase247))
                {
                    _phrase247 = "You have left the lobby";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"247\" Phrase=\"{0}\" />", _phrase247));
                if (!Dict.TryGetValue(248, out string _phrase248))
                {
                    _phrase248 = "You do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"248\" Phrase=\"{0}\" />", _phrase248));
                if (!Dict.TryGetValue(249, out string _phrase249))
                {
                    _phrase249 = "This command is locked to donors only";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"249\" Phrase=\"{0}\" />", _phrase249));
                if (!Dict.TryGetValue(250, out string _phrase250))
                {
                    _phrase250 = "You are inside the lobby. Unable to run command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"250\" Phrase=\"{0}\" />", _phrase250));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Market ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(251, out string _phrase251))
                {
                    _phrase251 = "You can only use {CommandPrivate}{Command103} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"251\" Phrase=\"{0}\" />", _phrase251));
                if (!Dict.TryGetValue(252, out string _phrase252))
                {
                    _phrase252 = "You can go back by typing {CommandPrivate}{Command51} when you are ready to leave the market.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"252\" Phrase=\"{0}\" />", _phrase252));
                if (!Dict.TryGetValue(253, out string _phrase253))
                {
                    _phrase253 = "You have no saved return point.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"253\" Phrase=\"{0}\" />", _phrase253));
                if (!Dict.TryGetValue(254, out string _phrase254))
                {
                    _phrase254 = "The market position is not set.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"254\" Phrase=\"{0}\" />", _phrase254));
                if (!Dict.TryGetValue(255, out string _phrase255))
                {
                    _phrase255 = "You have left the market space. {CommandPrivate}{Command51} command is no longer available.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"255\" Phrase=\"{0}\" />", _phrase255));
                if (!Dict.TryGetValue(256, out string _phrase256))
                {
                    _phrase256 = "You have set the market position as {MarketPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"256\" Phrase=\"{0}\" />", _phrase256));
                if (!Dict.TryGetValue(257, out string _phrase257))
                {
                    _phrase257 = "You do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"257\" Phrase=\"{0}\" />", _phrase257));
                if (!Dict.TryGetValue(258, out string _phrase258))
                {
                    _phrase258 = "This command is locked to donors only.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"258\" Phrase=\"{0}\" />", _phrase258));
                if (!Dict.TryGetValue(259, out string _phrase259))
                {
                    _phrase259 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"259\" Phrase=\"{0}\" />", _phrase259));
                if (!Dict.TryGetValue(260, out string _phrase260))
                {
                    _phrase260 = "Do not attack players inside the lobby or market!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"260\" Phrase=\"{0}\" />", _phrase260));
                if (!Dict.TryGetValue(261, out string _phrase261))
                {
                    _phrase261 = "You are inside the market. Unable to run command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"261\" Phrase=\"{0}\" />", _phrase261));
                if (!Dict.TryGetValue(262, out string _phrase262))
                {
                    _phrase262 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"262\" Phrase=\"{0}\" />", _phrase262));
                if (!Dict.TryGetValue(263, out string _phrase263))
                {
                    _phrase263 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"263\" Phrase=\"{0}\" />", _phrase263));
                if (!Dict.TryGetValue(264, out string _phrase264))
                {
                    _phrase264 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"264\" Phrase=\"{0}\" />", _phrase264));
                if (!Dict.TryGetValue(265, out string _phrase265))
                {
                    _phrase265 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"265\" Phrase=\"{0}\" />", _phrase265));
                if (!Dict.TryGetValue(266, out string _phrase266))
                {
                    _phrase266 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"266\" Phrase=\"{0}\" />", _phrase266));
                if (!Dict.TryGetValue(267, out string _phrase267))
                {
                    _phrase267 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"267\" Phrase=\"{0}\" />", _phrase267));
                if (!Dict.TryGetValue(268, out string _phrase268))
                {
                    _phrase268 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"268\" Phrase=\"{0}\" />", _phrase268));
                if (!Dict.TryGetValue(269, out string _phrase269))
                {
                    _phrase269 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"269\" Phrase=\"{0}\" />", _phrase269));
                if (!Dict.TryGetValue(270, out string _phrase270))
                {
                    _phrase270 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"270\" Phrase=\"{0}\" />", _phrase270));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Waypoints *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(271, out string _phrase271))
                {
                    _phrase271 = "You can only use {CommandPrivate}{Command106} once every {DelayBetweenUses} minutes. Time remaining: {Value} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"271\" Phrase=\"{0}\" />", _phrase271));
                if (!Dict.TryGetValue(272, out string _phrase272))
                {
                    _phrase272 = "You can only use a waypoint that is outside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"272\" Phrase=\"{0}\" />", _phrase272));
                if (!Dict.TryGetValue(273, out string _phrase273))
                {
                    _phrase273 = "Traveling to waypoint {Waypoint}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"273\" Phrase=\"{0}\" />", _phrase273));
                if (!Dict.TryGetValue(274, out string _phrase274))
                {
                    _phrase274 = "This waypoint was not found on your list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"274\" Phrase=\"{0}\" />", _phrase274));
                if (!Dict.TryGetValue(275, out string _phrase275))
                {
                    _phrase275 = "You have a maximum {Value} waypoints.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"275\" Phrase=\"{0}\" />", _phrase275));
                if (!Dict.TryGetValue(276, out string _phrase276))
                {
                    _phrase276 = "This is not a valid waypoint.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"276\" Phrase=\"{0}\" />", _phrase276));
                if (!Dict.TryGetValue(277, out string _phrase277))
                {
                    _phrase277 = "Waypoint {Name} has been deleted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"277\" Phrase=\"{0}\" />", _phrase277));
                if (!Dict.TryGetValue(278, out string _phrase278))
                {
                    _phrase278 = "Saved waypoint as {Name} at position {Position}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"278\" Phrase=\"{0}\" />", _phrase278));
                if (!Dict.TryGetValue(279, out string _phrase279))
                {
                    _phrase279 = "You have no waypoints saved.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"279\" Phrase=\"{0}\" />", _phrase279));
                if (!Dict.TryGetValue(280, out string _phrase280))
                {
                    _phrase280 = "You can only save a waypoint that is outside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"280\" Phrase=\"{0}\" />", _phrase280));
                if (!Dict.TryGetValue(281, out string _phrase281))
                {
                    _phrase281 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"281\" Phrase=\"{0}\" />", _phrase281));
                if (!Dict.TryGetValue(282, out string _phrase282))
                {
                    _phrase282 = "Waypoint {Name} @ {Value} {Value2} {Value3}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"282\" Phrase=\"{0}\" />", _phrase282));
                if (!Dict.TryGetValue(283, out string _phrase283))
                {
                    _phrase283 = "You can not use waypoint commands while in a event.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"283\" Phrase=\"{0}\" />", _phrase283));
                if (!Dict.TryGetValue(284, out string _phrase284))
                {
                    _phrase284 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"284\" Phrase=\"{0}\" />", _phrase284));
                if (!Dict.TryGetValue(285, out string _phrase285))
                {
                    _phrase285 = "This waypoint already exists. Choose another name.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"285\" Phrase=\"{0}\" />", _phrase285));
                if (!Dict.TryGetValue(286, out string _phrase286))
                {
                    _phrase286 = "Your friend {PlayerName} has invited you to their saved waypoint. Type {CommandPrivate}{Command10} to accept the request.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"286\" Phrase=\"{0}\" />", _phrase286));
                if (!Dict.TryGetValue(287, out string _phrase287))
                {
                    _phrase287 = "Invited your friend {PlayerName} to your saved waypoint.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"287\" Phrase=\"{0}\" />", _phrase287));
                if (!Dict.TryGetValue(288, out string _phrase288))
                {
                    _phrase288 = "You have run out of time to accept your friend's waypoint invitation.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"288\" Phrase=\"{0}\" />", _phrase288));
                if (!Dict.TryGetValue(289, out string _phrase289))
                {
                    _phrase289 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"289\" Phrase=\"{0}\" />", _phrase289));
                if (!Dict.TryGetValue(290, out string _phrase290))
                {
                    _phrase290 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"290\" Phrase=\"{0}\" />", _phrase290));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(291, out string _phrase291))
                {
                    _phrase291 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"291\" Phrase=\"{0}\" />", _phrase291));
                if (!Dict.TryGetValue(292, out string _phrase292))
                {
                    _phrase292 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"292\" Phrase=\"{0}\" />", _phrase292));
                if (!Dict.TryGetValue(293, out string _phrase293))
                {
                    _phrase293 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"293\" Phrase=\"{0}\" />", _phrase293));
                if (!Dict.TryGetValue(294, out string _phrase294))
                {
                    _phrase294 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"294\" Phrase=\"{0}\" />", _phrase294));
                if (!Dict.TryGetValue(295, out string _phrase295))
                {
                    _phrase295 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"295\" Phrase=\"{0}\" />", _phrase295));
                if (!Dict.TryGetValue(296, out string _phrase296))
                {
                    _phrase296 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"296\" Phrase=\"{0}\" />", _phrase296));
                if (!Dict.TryGetValue(297, out string _phrase297))
                {
                    _phrase297 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"297\" Phrase=\"{0}\" />", _phrase297));
                if (!Dict.TryGetValue(298, out string _phrase298))
                {
                    _phrase298 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"298\" Phrase=\"{0}\" />", _phrase298));
                if (!Dict.TryGetValue(299, out string _phrase299))
                {
                    _phrase299 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"299\" Phrase=\"{0}\" />", _phrase299));
                if (!Dict.TryGetValue(300, out string _phrase300))
                {
                    _phrase300 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"300\" Phrase=\"{0}\" />", _phrase300));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Vote_Reward ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(301, out string _phrase301))
                {
                    _phrase301 = "You can only use {CommandPrivate}{Command46} once every {DelayBetweenRewards} hours. Time remaining: {TimeRemaining} hour(s).";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"301\" Phrase=\"{0}\" />", _phrase301));
                if (!Dict.TryGetValue(302, out string _phrase302))
                {
                    _phrase302 = "No items found on the vote reward list. Contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"302\" Phrase=\"{0}\" />", _phrase302));
                if (!Dict.TryGetValue(303, out string _phrase303))
                {
                    _phrase303 = "Unable to get a result from the website, {PlayerName}. Please try {CommandPrivate}{Command46} again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"303\" Phrase=\"{0}\" />", _phrase303));
                if (!Dict.TryGetValue(304, out string _phrase304))
                {
                    _phrase304 = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"304\" Phrase=\"{0}\" />", _phrase304));
                if (!Dict.TryGetValue(305, out string _phrase305))
                {
                    _phrase305 = "You have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"305\" Phrase=\"{0}\" />", _phrase305));
                if (!Dict.TryGetValue(306, out string _phrase306))
                {
                    _phrase306 = "You have voted {Value} time since {Date}. You need {Value2} more votes before {Date2} to reach the bonus.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"306\" Phrase=\"{0}\" />", _phrase306));
                if (!Dict.TryGetValue(307, out string _phrase307))
                {
                    _phrase307 = "Thank you for your vote. You can vote and receive another reward in {Value} hours.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"307\" Phrase=\"{0}\" />", _phrase307));
                if (!Dict.TryGetValue(308, out string _phrase308))
                {
                    _phrase308 = "Reward items were sent to your inventory. If it is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"308\" Phrase=\"{0}\" />", _phrase308));
                if (!Dict.TryGetValue(309, out string _phrase309))
                {
                    _phrase309 = "Spawned a {EntityName} near you.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"309\" Phrase=\"{0}\" />", _phrase309));
                if (!Dict.TryGetValue(310, out string _phrase310))
                {
                    _phrase310 = "No spawn point was found near you. Please move locations and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"310\" Phrase=\"{0}\" />", _phrase310));
                if (!Dict.TryGetValue(311, out string _phrase311))
                {
                    _phrase311 = "{PlayerName} received an amazing reward because they are also amazing and voted for the server. Yippy!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"311\" Phrase=\"{0}\" />", _phrase311));
                if (!Dict.TryGetValue(312, out string _phrase312))
                {
                    _phrase312 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"312\" Phrase=\"{0}\" />", _phrase312));
                if (!Dict.TryGetValue(313, out string _phrase313))
                {
                    _phrase313 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"313\" Phrase=\"{0}\" />", _phrase313));
                if (!Dict.TryGetValue(314, out string _phrase314))
                {
                    _phrase314 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"314\" Phrase=\"{0}\" />", _phrase314));
                if (!Dict.TryGetValue(315, out string _phrase315))
                {
                    _phrase315 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"315\" Phrase=\"{0}\" />", _phrase315));
                if (!Dict.TryGetValue(316, out string _phrase316))
                {
                    _phrase316 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"316\" Phrase=\"{0}\" />", _phrase316));
                if (!Dict.TryGetValue(317, out string _phrase317))
                {
                    _phrase317 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"317\" Phrase=\"{0}\" />", _phrase317));
                if (!Dict.TryGetValue(318, out string _phrase318))
                {
                    _phrase318 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"318\" Phrase=\"{0}\" />", _phrase318));
                if (!Dict.TryGetValue(319, out string _phrase319))
                {
                    _phrase319 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"319\" Phrase=\"{0}\" />", _phrase319));
                if (!Dict.TryGetValue(320, out string _phrase320))
                {
                    _phrase320 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"320\" Phrase=\"{0}\" />", _phrase320));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Zones ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(321, out string _phrase321))
                {
                    _phrase321 = "You can only use {CommandPrivate}{Command50} for {Time} minutes after being killed in a pve zone. Time has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"321\" Phrase=\"{0}\" />", _phrase321));
                if (!Dict.TryGetValue(322, out string _phrase322))
                {
                    _phrase322 = "You can not set your home inside a zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"322\" Phrase=\"{0}\" />", _phrase322));
                if (!Dict.TryGetValue(323, out string _phrase323))
                {
                    _phrase323 = "Do not attack players inside a pve zone or while standing in one!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"323\" Phrase=\"{0}\" />", _phrase323));
                if (!Dict.TryGetValue(324, out string _phrase324))
                {
                    _phrase324 = "[FF0000]{PlayerName} has been executed for attempted murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"324\" Phrase=\"{0}\" />", _phrase324));
                if (!Dict.TryGetValue(325, out string _phrase325))
                {
                    _phrase325 = "[FF0000]{PlayerName} has been kicked for attempted murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"325\" Phrase=\"{0}\" />", _phrase325));
                if (!Dict.TryGetValue(326, out string _phrase326))
                {
                    _phrase326 = "Auto detection has kicked you for attempted murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"326\" Phrase=\"{0}\" />", _phrase326));
                if (!Dict.TryGetValue(327, out string _phrase327))
                {
                    _phrase327 = "[FF0000]{PlayerName} has been banned for attempted murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"327\" Phrase=\"{0}\" />", _phrase327));
                if (!Dict.TryGetValue(328, out string _phrase328))
                {
                    _phrase328 = "Auto detection has banned you for murder in a pve zone.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"328\" Phrase=\"{0}\" />", _phrase328));
                if (!Dict.TryGetValue(329, out string _phrase329))
                {
                    _phrase329 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"329\" Phrase=\"{0}\" />", _phrase329));
                if (!Dict.TryGetValue(330, out string _phrase330))
                {
                    _phrase330 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"330\" Phrase=\"{0}\" />", _phrase330));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Custom_Commands ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(331, out string _phrase331))
                {
                    _phrase331 = "You can only use {CommandPrivate}{CommandCustom} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"331\" Phrase=\"{0}\" />", _phrase331));
                if (!Dict.TryGetValue(332, out string _phrase332))
                {
                    _phrase332 = "You do not have permission to use {Command}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"332\" Phrase=\"{0}\" />", _phrase332));
                if (!Dict.TryGetValue(333, out string _phrase333))
                {
                    _phrase333 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"333\" Phrase=\"{0}\" />", _phrase333));
                if (!Dict.TryGetValue(334, out string _phrase334))
                {
                    _phrase334 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"334\" Phrase=\"{0}\" />", _phrase334));
                if (!Dict.TryGetValue(335, out string _phrase335))
                {
                    _phrase335 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"335\" Phrase=\"{0}\" />", _phrase335));
                if (!Dict.TryGetValue(336, out string _phrase336))
                {
                    _phrase336 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"336\" Phrase=\"{0}\" />", _phrase336));
                if (!Dict.TryGetValue(337, out string _phrase337))
                {
                    _phrase337 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"337\" Phrase=\"{0}\" />", _phrase337));
                if (!Dict.TryGetValue(338, out string _phrase338))
                {
                    _phrase338 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"338\" Phrase=\"{0}\" />", _phrase338));
                if (!Dict.TryGetValue(339, out string _phrase339))
                {
                    _phrase339 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"339\" Phrase=\"{0}\" />", _phrase339));
                if (!Dict.TryGetValue(340, out string _phrase340))
                {
                    _phrase340 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"340\" Phrase=\"{0}\" />", _phrase340));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Shop ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(341, out string _phrase341))
                {
                    _phrase341 = "The shop categories are:";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"341\" Phrase=\"{0}\" />", _phrase341));
                if (!Dict.TryGetValue(342, out string _phrase342))
                {
                    _phrase342 = "Type {CommandPrivate}{Command57} 'category' to view that list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"342\" Phrase=\"{0}\" />", _phrase342));
                if (!Dict.TryGetValue(343, out string _phrase343))
                {
                    _phrase343 = "You are not inside a trader area. Find a trader and use this command again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"343\" Phrase=\"{0}\" />", _phrase343));
                if (!Dict.TryGetValue(344, out string _phrase344))
                {
                    _phrase344 = "The item amount you input is invalid. Example: {CommandPrivate}{Command58} 1 2.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"344\" Phrase=\"{0}\" />", _phrase344));
                if (!Dict.TryGetValue(345, out string _phrase345))
                {
                    _phrase345 = "You do not have enough {CoinName}. Your wallet balance is {Value}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"345\" Phrase=\"{0}\" />", _phrase345));
                if (!Dict.TryGetValue(346, out string _phrase346))
                {
                    _phrase346 = "There was no item # matching the shop. Check the shop category again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"346\" Phrase=\"{0}\" />", _phrase346));
                if (!Dict.TryGetValue(347, out string _phrase347))
                {
                    _phrase347 = "There was an error in the shop list. Unable to buy this item. Please alert an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"347\" Phrase=\"{0}\" />", _phrase347));
                if (!Dict.TryGetValue(348, out string _phrase348))
                {
                    _phrase348 = "The shop does not contain any items. Contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"348\" Phrase=\"{0}\" />", _phrase348));
                if (!Dict.TryGetValue(349, out string _phrase349))
                {
                    _phrase349 = "You are not inside a market and trader area. Find one and use this command again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"349\" Phrase=\"{0}\" />", _phrase349));
                if (!Dict.TryGetValue(350, out string _phrase350))
                {
                    _phrase350 = "You are outside the market. Get inside it and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"350\" Phrase=\"{0}\" />", _phrase350));
                if (!Dict.TryGetValue(351, out string _phrase351))
                {
                    _phrase351 = "# {Id}: {Count} {Item} {Quality} quality for {Price} {Name}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"351\" Phrase=\"{0}\" />", _phrase351));
                if (!Dict.TryGetValue(352, out string _phrase352))
                {
                    _phrase352 = "# {Id}: {Count} {Item} for {Price} {Name}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"352\" Phrase=\"{0}\" />", _phrase352));
                if (!Dict.TryGetValue(353, out string _phrase353))
                {
                    _phrase353 = "Type {CommandPrivate}{Command58} # to purchase the shop item. You can add how many times you want to buy it with {CommandPrivate}{Command58} # #";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"353\" Phrase=\"{0}\" />", _phrase353));
                if (!Dict.TryGetValue(354, out string _phrase354))
                {
                    _phrase354 = "This category is missing. Check {CommandPrivate}{Command57}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"354\" Phrase=\"{0}\" />", _phrase354));
                if (!Dict.TryGetValue(355, out string _phrase355))
                {
                    _phrase355 = "Item not found for shop purchase.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"355\" Phrase=\"{0}\" />", _phrase355));
                if (!Dict.TryGetValue(356, out string _phrase356))
                {
                    _phrase356 = "{Count} {Item} was purchased through the shop. If your bag is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"356\" Phrase=\"{0}\" />", _phrase356));
                if (!Dict.TryGetValue(357, out string _phrase357))
                {
                    _phrase357 = "You can only purchase a full stack worth at a time. The maximum stack size for this is {Value}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"357\" Phrase=\"{0}\" />", _phrase357));
                if (!Dict.TryGetValue(358, out string _phrase358))
                {
                    _phrase358 = "Could not find shop category.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"358\" Phrase=\"{0}\" />", _phrase358));
                if (!Dict.TryGetValue(359, out string _phrase359))
                {
                    _phrase359 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"359\" Phrase=\"{0}\" />", _phrase359));
                if (!Dict.TryGetValue(360, out string _phrase360))
                {
                    _phrase360 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"360\" Phrase=\"{0}\" />", _phrase360));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Friend_Teleport ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(361, out string _phrase361))
                {
                    _phrase361 = "Friend = {FriendName}, Id = {EntityId}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"361\" Phrase=\"{0}\" />", _phrase361));
                if (!Dict.TryGetValue(362, out string _phrase362))
                {
                    _phrase362 = "This {EntityId} is not valid. Only integers accepted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"362\" Phrase=\"{0}\" />", _phrase362));
                if (!Dict.TryGetValue(363, out string _phrase363))
                {
                    _phrase363 = "Sent your friend {PlayerName} a teleport request.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"363\" Phrase=\"{0}\" />", _phrase363));
                if (!Dict.TryGetValue(364, out string _phrase364))
                {
                    _phrase364 = "{PlayerName} would like to teleport to you. Type {CommandPrivate}{Command60} in chat to accept the request.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"364\" Phrase=\"{0}\" />", _phrase364));
                if (!Dict.TryGetValue(365, out string _phrase365))
                {
                    _phrase365 = "Did not find EntityId {EntityId}. No teleport request sent.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"365\" Phrase=\"{0}\" />", _phrase365));
                if (!Dict.TryGetValue(366, out string _phrase366))
                {
                    _phrase366 = "You can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"366\" Phrase=\"{0}\" />", _phrase366));
                if (!Dict.TryGetValue(367, out string _phrase367))
                {
                    _phrase367 = "Your request was accepted. Teleporting you to your friend.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"367\" Phrase=\"{0}\" />", _phrase367));
                if (!Dict.TryGetValue(368, out string _phrase368))
                {
                    _phrase368 = "No friends found online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"368\" Phrase=\"{0}\" />", _phrase368));
                if (!Dict.TryGetValue(369, out string _phrase369))
                {
                    _phrase369 = "This player is not your friend. You can not request teleport to them.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"369\" Phrase=\"{0}\" />", _phrase369));
                if (!Dict.TryGetValue(370, out string _phrase370))
                {
                    _phrase370 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"370\" Phrase=\"{0}\" />", _phrase370));
                if (!Dict.TryGetValue(371, out string _phrase371))
                {
                    _phrase371 = "Unable to complete request. Friend was not found online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"371\" Phrase=\"{0}\" />", _phrase371));
                if (!Dict.TryGetValue(372, out string _phrase372))
                {
                    _phrase372 = "Your friend's teleport request has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"372\" Phrase=\"{0}\" />", _phrase372));
                if (!Dict.TryGetValue(373, out string _phrase373))
                {
                    _phrase373 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"373\" Phrase=\"{0}\" />", _phrase373));
                if (!Dict.TryGetValue(374, out string _phrase374))
                {
                    _phrase374 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"374\" Phrase=\"{0}\" />", _phrase374));
                if (!Dict.TryGetValue(375, out string _phrase375))
                {
                    _phrase375 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"375\" Phrase=\"{0}\" />", _phrase375));
                if (!Dict.TryGetValue(376, out string _phrase376))
                {
                    _phrase376 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"376\" Phrase=\"{0}\" />", _phrase376));
                if (!Dict.TryGetValue(377, out string _phrase377))
                {
                    _phrase377 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"377\" Phrase=\"{0}\" />", _phrase377));
                if (!Dict.TryGetValue(378, out string _phrase378))
                {
                    _phrase378 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"378\" Phrase=\"{0}\" />", _phrase378));
                if (!Dict.TryGetValue(379, out string _phrase379))
                {
                    _phrase379 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"379\" Phrase=\"{0}\" />", _phrase379));
                if (!Dict.TryGetValue(380, out string _phrase380))
                {
                    _phrase380 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"380\" Phrase=\"{0}\" />", _phrase380));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Voting ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(381, out string _phrase381))
                {
                    _phrase381 = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"381\" Phrase=\"{0}\" />", _phrase381));
                if (!Dict.TryGetValue(382, out string _phrase382))
                {
                    _phrase382 = "Thank you for your vote. You can vote and receive another reward in {VoteDelay} hours.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"382\" Phrase=\"{0}\" />", _phrase382));
                if (!Dict.TryGetValue(383, out string _phrase383))
                {
                    _phrase383 = "Unable to get a result from the website, {PlayerName}. Please try {CommandPrivate}{Command46} again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"383\" Phrase=\"{0}\" />", _phrase383));
                if (!Dict.TryGetValue(384, out string _phrase384))
                {
                    _phrase384 = "Reward items were sent to your inventory. If it is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"384\" Phrase=\"{0}\" />", _phrase384));
                if (!Dict.TryGetValue(385, out string _phrase385))
                {
                    _phrase385 = "You have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"385\" Phrase=\"{0}\" />", _phrase385));
                if (!Dict.TryGetValue(386, out string _phrase386))
                {
                    _phrase386 = "You have voted {Votes} time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"386\" Phrase=\"{0}\" />", _phrase386));
                if (!Dict.TryGetValue(387, out string _phrase387))
                {
                    _phrase387 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"387\" Phrase=\"{0}\" />", _phrase387));
                if (!Dict.TryGetValue(388, out string _phrase388))
                {
                    _phrase388 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"388\" Phrase=\"{0}\" />", _phrase388));
                if (!Dict.TryGetValue(389, out string _phrase389))
                {
                    _phrase389 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"389\" Phrase=\"{0}\" />", _phrase389));
                if (!Dict.TryGetValue(390, out string _phrase390))
                {
                    _phrase390 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"390\" Phrase=\"{0}\" />", _phrase390));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(391, out string _phrase391))
                {
                    _phrase391 = "You have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"391\" Phrase=\"{0}\" />", _phrase391));
                if (!Dict.TryGetValue(392, out string _phrase392))
                {
                    _phrase392 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"392\" Phrase=\"{0}\" />", _phrase392));
                if (!Dict.TryGetValue(393, out string _phrase393))
                {
                    _phrase393 = "You have tracked down an animal to within {Radius} metres.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"393\" Phrase=\"{0}\" />", _phrase393));
                if (!Dict.TryGetValue(394, out string _phrase394))
                {
                    _phrase394 = "Animal list is empty. Contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"394\" Phrase=\"{0}\" />", _phrase394));
                if (!Dict.TryGetValue(395, out string _phrase395))
                {
                    _phrase395 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"395\" Phrase=\"{0}\" />", _phrase395));
                if (!Dict.TryGetValue(396, out string _phrase396))
                {
                    _phrase396 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"396\" Phrase=\"{0}\" />", _phrase396));
                if (!Dict.TryGetValue(397, out string _phrase397))
                {
                    _phrase397 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"397\" Phrase=\"{0}\" />", _phrase397));
                if (!Dict.TryGetValue(398, out string _phrase398))
                {
                    _phrase398 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"398\" Phrase=\"{0}\" />", _phrase398));
                if (!Dict.TryGetValue(399, out string _phrase399))
                {
                    _phrase399 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"399\" Phrase=\"{0}\" />", _phrase399));
                if (!Dict.TryGetValue(400, out string _phrase400))
                {
                    _phrase400 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"400\" Phrase=\"{0}\" />", _phrase400));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Hatch_Elevator ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(401, out string _phrase401))
                {
                    _phrase401 = "You are stunned and have broken your leg while smashing yourself through hatches. Ouch!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"401\" Phrase=\"{0}\" />", _phrase401));
                if (!Dict.TryGetValue(402, out string _phrase402))
                {
                    _phrase402 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"402\" Phrase=\"{0}\" />", _phrase402));
                if (!Dict.TryGetValue(403, out string _phrase403))
                {
                    _phrase403 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"403\" Phrase=\"{0}\" />", _phrase403));
                if (!Dict.TryGetValue(404, out string _phrase404))
                {
                    _phrase404 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"404\" Phrase=\"{0}\" />", _phrase404));
                if (!Dict.TryGetValue(405, out string _phrase405))
                {
                    _phrase405 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"405\" Phrase=\"{0}\" />", _phrase405));
                if (!Dict.TryGetValue(406, out string _phrase406))
                {
                    _phrase406 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"406\" Phrase=\"{0}\" />", _phrase406));
                if (!Dict.TryGetValue(407, out string _phrase407))
                {
                    _phrase407 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"407\" Phrase=\"{0}\" />", _phrase407));
                if (!Dict.TryGetValue(408, out string _phrase408))
                {
                    _phrase408 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"408\" Phrase=\"{0}\" />", _phrase408));
                if (!Dict.TryGetValue(409, out string _phrase409))
                {
                    _phrase409 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"409\" Phrase=\"{0}\" />", _phrase409));
                if (!Dict.TryGetValue(410, out string _phrase410))
                {
                    _phrase410 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"410\" Phrase=\"{0}\" />", _phrase410));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Admin_List *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(411, out string _phrase411))
                {
                    _phrase411 = "Server admins in game: [FF8000]";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"411\" Phrase=\"{0}\" />", _phrase411));
                if (!Dict.TryGetValue(412, out string _phrase412))
                {
                    _phrase412 = "Server moderators in game: [FF8000]";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"412\" Phrase=\"{0}\" />", _phrase412));
                if (!Dict.TryGetValue(413, out string _phrase413))
                {
                    _phrase413 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"413\" Phrase=\"{0}\" />", _phrase413));
                if (!Dict.TryGetValue(414, out string _phrase414))
                {
                    _phrase414 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"414\" Phrase=\"{0}\" />", _phrase414));
                if (!Dict.TryGetValue(415, out string _phrase415))
                {
                    _phrase415 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"415\" Phrase=\"{0}\" />", _phrase415));
                if (!Dict.TryGetValue(416, out string _phrase416))
                {
                    _phrase416 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"416\" Phrase=\"{0}\" />", _phrase416));
                if (!Dict.TryGetValue(417, out string _phrase417))
                {
                    _phrase417 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"417\" Phrase=\"{0}\" />", _phrase417));
                if (!Dict.TryGetValue(418, out string _phrase418))
                {
                    _phrase418 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"418\" Phrase=\"{0}\" />", _phrase418));
                if (!Dict.TryGetValue(419, out string _phrase419))
                {
                    _phrase419 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"419\" Phrase=\"{0}\" />", _phrase419));
                if (!Dict.TryGetValue(420, out string _phrase420))
                {
                    _phrase420 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"420\" Phrase=\"{0}\" />", _phrase420));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Shutdown ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(421, out string _phrase421))
                {
                    _phrase421 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"421\" Phrase=\"{0}\" />", _phrase421));
                if (!Dict.TryGetValue(422, out string _phrase422))
                {
                    _phrase422 = "A event is currently active. The server can not shutdown until it completes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"422\" Phrase=\"{0}\" />", _phrase422));
                if (!Dict.TryGetValue(423, out string _phrase423))
                {
                    _phrase423 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"423\" Phrase=\"{0}\" />", _phrase423));
                if (!Dict.TryGetValue(424, out string _phrase424))
                {
                    _phrase424 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"424\" Phrase=\"{0}\" />", _phrase424));
                if (!Dict.TryGetValue(425, out string _phrase425))
                {
                    _phrase425 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"425\" Phrase=\"{0}\" />", _phrase425));
                if (!Dict.TryGetValue(426, out string _phrase426))
                {
                    _phrase426 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"426\" Phrase=\"{0}\" />", _phrase426));
                if (!Dict.TryGetValue(427, out string _phrase427))
                {
                    _phrase427 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"427\" Phrase=\"{0}\" />", _phrase427));
                if (!Dict.TryGetValue(428, out string _phrase428))
                {
                    _phrase428 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"428\" Phrase=\"{0}\" />", _phrase428));
                if (!Dict.TryGetValue(429, out string _phrase429))
                {
                    _phrase429 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"429\" Phrase=\"{0}\" />", _phrase429));
                if (!Dict.TryGetValue(430, out string _phrase430))
                {
                    _phrase430 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"430\" Phrase=\"{0}\" />", _phrase430));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Death_Spot ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(431, out string _phrase431))
                {
                    _phrase431 = "You can only use {CommandPrivate}{Command61} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"431\" Phrase=\"{0}\" />", _phrase431));
                if (!Dict.TryGetValue(432, out string _phrase432))
                {
                    _phrase432 = "Your last death occurred too long ago. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"432\" Phrase=\"{0}\" />", _phrase432));
                if (!Dict.TryGetValue(433, out string _phrase433))
                {
                    _phrase433 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"433\" Phrase=\"{0}\" />", _phrase433));
                if (!Dict.TryGetValue(434, out string _phrase434))
                {
                    _phrase434 = "You have no saved death position.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"434\" Phrase=\"{0}\" />", _phrase434));
                if (!Dict.TryGetValue(435, out string _phrase435))
                {
                    _phrase435 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"435\" Phrase=\"{0}\" />", _phrase435));
                if (!Dict.TryGetValue(436, out string _phrase436))
                {
                    _phrase436 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"436\" Phrase=\"{0}\" />", _phrase436));
                if (!Dict.TryGetValue(437, out string _phrase437))
                {
                    _phrase437 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"437\" Phrase=\"{0}\" />", _phrase437));
                if (!Dict.TryGetValue(438, out string _phrase438))
                {
                    _phrase438 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"438\" Phrase=\"{0}\" />", _phrase438));
                if (!Dict.TryGetValue(439, out string _phrase439))
                {
                    _phrase439 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"439\" Phrase=\"{0}\" />", _phrase439));
                if (!Dict.TryGetValue(440, out string _phrase440))
                {
                    _phrase440 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"440\" Phrase=\"{0}\" />", _phrase440));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Restart_Vote ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(441, out string _phrase441))
                {
                    _phrase441 = "A vote to restart the server has opened and will close in 60 seconds. Type {CommandPrivate}{Command70} to cast your vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"441\" Phrase=\"{0}\" />", _phrase441));
                if (!Dict.TryGetValue(442, out string _phrase442))
                {
                    _phrase442 = "There are not enough players online to start a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"442\" Phrase=\"{0}\" />", _phrase442));
                if (!Dict.TryGetValue(443, out string _phrase443))
                {
                    _phrase443 = "Players voted yes to a server restart. Shutdown has been initiated.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"443\" Phrase=\"{0}\" />", _phrase443));
                if (!Dict.TryGetValue(444, out string _phrase444))
                {
                    _phrase444 = "Players voted yes but not enough votes were cast to restart.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"444\" Phrase=\"{0}\" />", _phrase444));
                if (!Dict.TryGetValue(445, out string _phrase445))
                {
                    _phrase445 = "Players voted no to a server restart. A new vote can open in {RestartDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"445\" Phrase=\"{0}\" />", _phrase445));
                if (!Dict.TryGetValue(446, out string _phrase446))
                {
                    _phrase446 = "The restart vote was a tie. A new vote can open in {RestartDelay} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"446\" Phrase=\"{0}\" />", _phrase446));
                if (!Dict.TryGetValue(447, out string _phrase447))
                {
                    _phrase447 = "No votes were cast to restart the server.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"447\" Phrase=\"{0}\" />", _phrase447));
                if (!Dict.TryGetValue(448, out string _phrase448))
                {
                    _phrase448 = "You started the last restart vote. Another player must initiate the next vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"448\" Phrase=\"{0}\" />", _phrase448));
                if (!Dict.TryGetValue(449, out string _phrase449))
                {
                    _phrase449 = "{PlayerName} has requested a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"449\" Phrase=\"{0}\" />", _phrase449));
                if (!Dict.TryGetValue(450, out string _phrase450))
                {
                    _phrase450 = "A administrator is currently online. They have been alerted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"450\" Phrase=\"{0}\" />", _phrase450));
                if (!Dict.TryGetValue(451, out string _phrase451))
                {
                    _phrase451 = "There is a vote already open.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"451\" Phrase=\"{0}\" />", _phrase451));
                if (!Dict.TryGetValue(452, out string _phrase452))
                {
                    _phrase452 = "You must wait thirty minutes after the server starts before opening a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"452\" Phrase=\"{0}\" />", _phrase452));
                if (!Dict.TryGetValue(453, out string _phrase453))
                {
                    _phrase453 = "There are now {Value} of {VotesNeeded} votes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"453\" Phrase=\"{0}\" />", _phrase453));
                if (!Dict.TryGetValue(454, out string _phrase454))
                {
                    _phrase454 = "You have already voted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"454\" Phrase=\"{0}\" />", _phrase454));
                if (!Dict.TryGetValue(455, out string _phrase455))
                {
                    _phrase455 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"455\" Phrase=\"{0}\" />", _phrase455));
                if (!Dict.TryGetValue(456, out string _phrase456))
                {
                    _phrase456 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"456\" Phrase=\"{0}\" />", _phrase456));
                if (!Dict.TryGetValue(457, out string _phrase457))
                {
                    _phrase457 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"457\" Phrase=\"{0}\" />", _phrase457));
                if (!Dict.TryGetValue(458, out string _phrase458))
                {
                    _phrase458 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"458\" Phrase=\"{0}\" />", _phrase458));
                if (!Dict.TryGetValue(459, out string _phrase459))
                {
                    _phrase459 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"459\" Phrase=\"{0}\" />", _phrase459));
                if (!Dict.TryGetValue(460, out string _phrase460))
                {
                    _phrase460 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"460\" Phrase=\"{0}\" />", _phrase460));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Location *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(461, out string _phrase461))
                {
                    _phrase461 = "Your current position is X  {X}, Y  {Y}, Z  {Z}. Zone: {Name}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"461\" Phrase=\"{0}\" />", _phrase461));
                if (!Dict.TryGetValue(462, out string _phrase462))
                {
                    _phrase462 = "Your current position is X  {X}, Y  {Y}, Z  {Z}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"462\" Phrase=\"{0}\" />", _phrase462));
                if (!Dict.TryGetValue(463, out string _phrase463))
                {
                    _phrase463 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"463\" Phrase=\"{0}\" />", _phrase463));
                if (!Dict.TryGetValue(464, out string _phrase464))
                {
                    _phrase464 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"464\" Phrase=\"{0}\" />", _phrase464));
                if (!Dict.TryGetValue(465, out string _phrase465))
                {
                    _phrase465 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"465\" Phrase=\"{0}\" />", _phrase465));
                if (!Dict.TryGetValue(466, out string _phrase466))
                {
                    _phrase466 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"466\" Phrase=\"{0}\" />", _phrase466));
                if (!Dict.TryGetValue(467, out string _phrase467))
                {
                    _phrase467 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"467\" Phrase=\"{0}\" />", _phrase467));
                if (!Dict.TryGetValue(468, out string _phrase468))
                {
                    _phrase468 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"468\" Phrase=\"{0}\" />", _phrase468));
                if (!Dict.TryGetValue(469, out string _phrase469))
                {
                    _phrase469 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"469\" Phrase=\"{0}\" />", _phrase469));
                if (!Dict.TryGetValue(470, out string _phrase470))
                {
                    _phrase470 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"470\" Phrase=\"{0}\" />", _phrase470));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Real_World_Time ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(471, out string _phrase471))
                {
                    _phrase471 = "The real world time is {Time} {TimeZone}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"471\" Phrase=\"{0}\" />", _phrase471));
                if (!Dict.TryGetValue(472, out string _phrase472))
                {
                    _phrase472 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"472\" Phrase=\"{0}\" />", _phrase472));
                if (!Dict.TryGetValue(473, out string _phrase473))
                {
                    _phrase473 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"473\" Phrase=\"{0}\" />", _phrase473));
                if (!Dict.TryGetValue(474, out string _phrase474))
                {
                    _phrase474 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"474\" Phrase=\"{0}\" />", _phrase474));
                if (!Dict.TryGetValue(475, out string _phrase475))
                {
                    _phrase475 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"475\" Phrase=\"{0}\" />", _phrase475));
                if (!Dict.TryGetValue(476, out string _phrase476))
                {
                    _phrase476 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"476\" Phrase=\"{0}\" />", _phrase476));
                if (!Dict.TryGetValue(477, out string _phrase477))
                {
                    _phrase477 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"477\" Phrase=\"{0}\" />", _phrase477));
                if (!Dict.TryGetValue(478, out string _phrase478))
                {
                    _phrase478 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"478\" Phrase=\"{0}\" />", _phrase478));
                if (!Dict.TryGetValue(479, out string _phrase479))
                {
                    _phrase479 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"479\" Phrase=\"{0}\" />", _phrase479));
                if (!Dict.TryGetValue(480, out string _phrase480))
                {
                    _phrase480 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"480\" Phrase=\"{0}\" />", _phrase480));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************** Day7 ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(481, out string _phrase481))
                {
                    _phrase481 = "Server FPS:{Value}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"481\" Phrase=\"{0}\" />", _phrase481));
                if (!Dict.TryGetValue(482, out string _phrase482))
                {
                    _phrase482 = "Next horde night: {Value} days";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"482\" Phrase=\"{0}\" />", _phrase482));
                if (!Dict.TryGetValue(483, out string _phrase483))
                {
                    _phrase483 = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"483\" Phrase=\"{0}\" />", _phrase483));
                if (!Dict.TryGetValue(484, out string _phrase484))
                {
                    _phrase484 = "Bicycles:{Bicycles} Minibikes:{Minibikes} Motorcycles:{Motorcycles} 4x4:{4x4} Gyros:{Gyros}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"484\" Phrase=\"{0}\" />", _phrase484));
                if (!Dict.TryGetValue(485, out string _phrase485))
                {
                    _phrase485 = "Supply Crates:{Value}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"485\" Phrase=\"{0}\" />", _phrase485));
                if (!Dict.TryGetValue(486, out string _phrase486))
                {
                    _phrase486 = "The horde is here!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"486\" Phrase=\"{0}\" />", _phrase486));
                if (!Dict.TryGetValue(487, out string _phrase487))
                {
                    _phrase487 = "Next horde night is today";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"487\" Phrase=\"{0}\" />", _phrase487));
                if (!Dict.TryGetValue(488, out string _phrase488))
                {
                    _phrase488 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"488\" Phrase=\"{0}\" />", _phrase488));
                if (!Dict.TryGetValue(489, out string _phrase489))
                {
                    _phrase489 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"489\" Phrase=\"{0}\" />", _phrase489));
                if (!Dict.TryGetValue(490, out string _phrase490))
                {
                    _phrase490 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"490\" Phrase=\"{0}\" />", _phrase490));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Vehicle_Teleport ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(491, out string _phrase491))
                {
                    _phrase491 = "You or a friend have not claimed this space. You can only save your vehicle inside a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"491\" Phrase=\"{0}\" />", _phrase491));
                if (!Dict.TryGetValue(492, out string _phrase492))
                {
                    _phrase492 = "Saved your current {Vehicle} for retrieval.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"492\" Phrase=\"{0}\" />", _phrase492));
                if (!Dict.TryGetValue(493, out string _phrase493))
                {
                    _phrase493 = "Found your vehicle and sent it to you.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"493\" Phrase=\"{0}\" />", _phrase493));
                if (!Dict.TryGetValue(494, out string _phrase494))
                {
                    _phrase494 = "You do not have this vehicle type saved.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"494\" Phrase=\"{0}\" />", _phrase494));
                if (!Dict.TryGetValue(495, out string _phrase495))
                {
                    _phrase495 = "Could not find your vehicle near by.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"495\" Phrase=\"{0}\" />", _phrase495));
                if (!Dict.TryGetValue(496, out string _phrase496))
                {
                    _phrase496 = "Found your vehicle but someone else is on it.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"496\" Phrase=\"{0}\" />", _phrase496));
                if (!Dict.TryGetValue(497, out string _phrase497))
                {
                    _phrase497 = "You can only use vehicle teleport once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"497\" Phrase=\"{0}\" />", _phrase497));
                if (!Dict.TryGetValue(498, out string _phrase498))
                {
                    _phrase498 = "You are on the wrong vehicle to save it with this command. You are using a {Vehicle}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"498\" Phrase=\"{0}\" />", _phrase498));
                if (!Dict.TryGetValue(499, out string _phrase499))
                {
                    _phrase499 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"499\" Phrase=\"{0}\" />", _phrase499));
                if (!Dict.TryGetValue(500, out string _phrase500))
                {
                    _phrase500 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"500\" Phrase=\"{0}\" />", _phrase500));
                if (!Dict.TryGetValue(501, out string _phrase501))
                {
                    _phrase501 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"501\" Phrase=\"{0}\" />", _phrase501));
                if (!Dict.TryGetValue(502, out string _phrase502))
                {
                    _phrase502 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"502\" Phrase=\"{0}\" />", _phrase502));
                if (!Dict.TryGetValue(503, out string _phrase503))
                {
                    _phrase503 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"503\" Phrase=\"{0}\" />", _phrase503));
                if (!Dict.TryGetValue(504, out string _phrase504))
                {
                    _phrase504 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"504\" Phrase=\"{0}\" />", _phrase504));
                if (!Dict.TryGetValue(505, out string _phrase505))
                {
                    _phrase505 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"505\" Phrase=\"{0}\" />", _phrase505));
                if (!Dict.TryGetValue(506, out string _phrase506))
                {
                    _phrase506 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"506\" Phrase=\"{0}\" />", _phrase506));
                if (!Dict.TryGetValue(507, out string _phrase507))
                {
                    _phrase507 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"507\" Phrase=\"{0}\" />", _phrase507));
                if (!Dict.TryGetValue(508, out string _phrase508))
                {
                    _phrase508 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"508\" Phrase=\"{0}\" />", _phrase508));
                if (!Dict.TryGetValue(509, out string _phrase509))
                {
                    _phrase509 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"509\" Phrase=\"{0}\" />", _phrase509));
                if (!Dict.TryGetValue(510, out string _phrase510))
                {
                    _phrase510 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"510\" Phrase=\"{0}\" />", _phrase510));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* World_Radius ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(511, out string _phrase511))
                {
                    _phrase511 = "You have reached the world border.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"511\" Phrase=\"{0}\" />", _phrase511));
                if (!Dict.TryGetValue(512, out string _phrase512))
                {
                    _phrase512 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"512\" Phrase=\"{0}\" />", _phrase512));
                if (!Dict.TryGetValue(513, out string _phrase513))
                {
                    _phrase513 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"513\" Phrase=\"{0}\" />", _phrase513));
                if (!Dict.TryGetValue(514, out string _phrase514))
                {
                    _phrase514 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"514\" Phrase=\"{0}\" />", _phrase514));
                if (!Dict.TryGetValue(515, out string _phrase515))
                {
                    _phrase515 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"515\" Phrase=\"{0}\" />", _phrase515));
                if (!Dict.TryGetValue(516, out string _phrase516))
                {
                    _phrase516 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"516\" Phrase=\"{0}\" />", _phrase516));
                if (!Dict.TryGetValue(517, out string _phrase517))
                {
                    _phrase517 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"517\" Phrase=\"{0}\" />", _phrase517));
                if (!Dict.TryGetValue(518, out string _phrase518))
                {
                    _phrase518 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"518\" Phrase=\"{0}\" />", _phrase518));
                if (!Dict.TryGetValue(519, out string _phrase519))
                {
                    _phrase519 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"519\" Phrase=\"{0}\" />", _phrase519));
                if (!Dict.TryGetValue(520, out string _phrase520))
                {
                    _phrase520 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"520\" Phrase=\"{0}\" />", _phrase520));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Report ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(521, out string _phrase521))
                {
                    _phrase521 = "You can only make a report once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"521\" Phrase=\"{0}\" />", _phrase521));
                if (!Dict.TryGetValue(522, out string _phrase522))
                {
                    _phrase522 = "Report @ position {Position} from {PlayerName}: {Message}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"522\" Phrase=\"{0}\" />", _phrase522));
                if (!Dict.TryGetValue(523, out string _phrase523))
                {
                    _phrase523 = "Your report has been sent to online administrators and logged.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"523\" Phrase=\"{0}\" />", _phrase523));
                if (!Dict.TryGetValue(524, out string _phrase524))
                {
                    _phrase524 = "Your report is too long. Please reduce it to {Length} characters.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"524\" Phrase=\"{0}\" />", _phrase524));
                if (!Dict.TryGetValue(525, out string _phrase525))
                {
                    _phrase525 = "Your report has been sent to online administrators and logged.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"525\" Phrase=\"{0}\" />", _phrase525));
                if (!Dict.TryGetValue(526, out string _phrase526))
                {
                    _phrase526 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"526\" Phrase=\"{0}\" />", _phrase526));
                if (!Dict.TryGetValue(527, out string _phrase527))
                {
                    _phrase527 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"527\" Phrase=\"{0}\" />", _phrase527));
                if (!Dict.TryGetValue(528, out string _phrase528))
                {
                    _phrase528 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"528\" Phrase=\"{0}\" />", _phrase528));
                if (!Dict.TryGetValue(529, out string _phrase529))
                {
                    _phrase529 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"529\" Phrase=\"{0}\" />", _phrase529));
                if (!Dict.TryGetValue(530, out string _phrase530))
                {
                    _phrase530 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"530\" Phrase=\"{0}\" />", _phrase530));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Bounties *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(531, out string _phrase531))
                {
                    _phrase531 = "{PlayerName} has collected {Value} bounties without dying! Their bounty has increased.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"531\" Phrase=\"{0}\" />", _phrase531));
                if (!Dict.TryGetValue(532, out string _phrase532))
                {
                    _phrase532 = "Player {Victim}' kill streak has come to an end by {Killer}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"532\" Phrase=\"{0}\" />", _phrase532));
                if (!Dict.TryGetValue(533, out string _phrase533))
                {
                    _phrase533 = "Player {Killer}' has collected the bounty of {Victim}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"533\" Phrase=\"{0}\" />", _phrase533));
                if (!Dict.TryGetValue(534, out string _phrase534))
                {
                    _phrase534 = "You do not have enough in your wallet for this bounty: {Value}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"534\" Phrase=\"{0}\" />", _phrase534));
                if (!Dict.TryGetValue(535, out string _phrase535))
                {
                    _phrase535 = "A bounty of {Value} has been added to {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"535\" Phrase=\"{0}\" />", _phrase535));
                if (!Dict.TryGetValue(536, out string _phrase536))
                {
                    _phrase536 = "You input an invalid integer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"536\" Phrase=\"{0}\" />", _phrase536));
                if (!Dict.TryGetValue(537, out string _phrase537))
                {
                    _phrase537 = "Type {CommandPrivate}{Command83} Id# Value or {CommandPrivate}{Command83} Id# for the minimum bounty against this player.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"537\" Phrase=\"{0}\" />", _phrase537));
                if (!Dict.TryGetValue(538, out string _phrase538))
                {
                    _phrase538 = "{PlayerName}. Id: {EntityId}. Bounty: {CurrentBounty}. Minimum: {Minimum} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"538\" Phrase=\"{0}\" />", _phrase538));
                if (!Dict.TryGetValue(539, out string _phrase539))
                {
                    _phrase539 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"539\" Phrase=\"{0}\" />", _phrase539));
                if (!Dict.TryGetValue(540, out string _phrase540))
                {
                    _phrase540 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"540\" Phrase=\"{0}\" />", _phrase540));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Kill_Notice ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(541, out string _phrase541))
                {
                    _phrase541 = "{Name1} has killed {Name2} with {Item} for {Damage}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"541\" Phrase=\"{0}\" />", _phrase541));
                if (!Dict.TryGetValue(542, out string _phrase542))
                {
                    _phrase542 = "{Name1} ({Level1}) has killed {Name2} ({Level2}) with {Item}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"542\" Phrase=\"{0}\" />", _phrase542));
                if (!Dict.TryGetValue(543, out string _phrase543))
                {
                    _phrase543 = "{Name1} ({Level1}) has killed {Name2} ({Level2}) with {Item} for {Damage}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"543\" Phrase=\"{0}\" />", _phrase543));
                if (!Dict.TryGetValue(544, out string _phrase544))
                {
                    _phrase544 = "{Name1} has killed {Name2} with {Item}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"544\" Phrase=\"{0}\" />", _phrase544));
                if (!Dict.TryGetValue(545, out string _phrase545))
                {
                    _phrase545 = "{PlayerName}{Level} has been ate by {ZombieName}. Nom nom nom";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"545\" Phrase=\"{0}\" />", _phrase545));
                if (!Dict.TryGetValue(546, out string _phrase546))
                {
                    _phrase546 = "{PlayerName} has been ate by {ZombieName}. Nom nom nom";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"546\" Phrase=\"{0}\" />", _phrase546));
                if (!Dict.TryGetValue(547, out string _phrase547))
                {
                    _phrase547 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"547\" Phrase=\"{0}\" />", _phrase547));
                if (!Dict.TryGetValue(548, out string _phrase548))
                {
                    _phrase548 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"548\" Phrase=\"{0}\" />", _phrase548));
                if (!Dict.TryGetValue(549, out string _phrase549))
                {
                    _phrase549 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"549\" Phrase=\"{0}\" />", _phrase549));
                if (!Dict.TryGetValue(550, out string _phrase550))
                {
                    _phrase550 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"550\" Phrase=\"{0}\" />", _phrase550));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Stuck ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(551, out string _phrase551))
                {
                    _phrase551 = "You can only use {CommandPrivate}{Command90} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"551\" Phrase=\"{0}\" />", _phrase551));
                if (!Dict.TryGetValue(552, out string _phrase552))
                {
                    _phrase552 = "You are outside of your claimed space or a friends. Command is unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"552\" Phrase=\"{0}\" />", _phrase552));
                if (!Dict.TryGetValue(553, out string _phrase553))
                {
                    _phrase553 = "Sent you to the world surface. If you are still stuck, contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"553\" Phrase=\"{0}\" />", _phrase553));
                if (!Dict.TryGetValue(554, out string _phrase554))
                {
                    _phrase554 = "You do not seem to be stuck.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"554\" Phrase=\"{0}\" />", _phrase554));
                if (!Dict.TryGetValue(555, out string _phrase555))
                {
                    _phrase555 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"555\" Phrase=\"{0}\" />", _phrase555));
                if (!Dict.TryGetValue(556, out string _phrase556))
                {
                    _phrase556 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"556\" Phrase=\"{0}\" />", _phrase556));
                if (!Dict.TryGetValue(557, out string _phrase557))
                {
                    _phrase557 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"557\" Phrase=\"{0}\" />", _phrase557));
                if (!Dict.TryGetValue(558, out string _phrase558))
                {
                    _phrase558 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"558\" Phrase=\"{0}\" />", _phrase558));
                if (!Dict.TryGetValue(559, out string _phrase559))
                {
                    _phrase559 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"559\" Phrase=\"{0}\" />", _phrase559));
                if (!Dict.TryGetValue(560, out string _phrase560))
                {
                    _phrase560 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"560\" Phrase=\"{0}\" />", _phrase560));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Poll ************************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(561, out string _phrase561))
                {
                    _phrase561 = "Poll results: Yes {YesVote} / No {NoVote}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"561\" Phrase=\"{0}\" />", _phrase561));
                if (!Dict.TryGetValue(562, out string _phrase562))
                {
                    _phrase562 = "Poll: {Message}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"562\" Phrase=\"{0}\" />", _phrase562));
                if (!Dict.TryGetValue(563, out string _phrase563))
                {
                    _phrase563 = "Type {CommandPrivate}{Command91} or {CommandPrivate}{Command92} to vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"563\" Phrase=\"{0}\" />", _phrase563));
                if (!Dict.TryGetValue(564, out string _phrase564))
                {
                    _phrase564 = "You have cast a vote for yes in the poll.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"564\" Phrase=\"{0}\" />", _phrase564));
                if (!Dict.TryGetValue(565, out string _phrase565))
                {
                    _phrase565 = "You have cast a vote for no in the poll.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"565\" Phrase=\"{0}\" />", _phrase565));
                if (!Dict.TryGetValue(566, out string _phrase566))
                {
                    _phrase566 = "You have already voted on the poll.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"566\" Phrase=\"{0}\" />", _phrase566));
                if (!Dict.TryGetValue(567, out string _phrase567))
                {
                    _phrase567 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"567\" Phrase=\"{0}\" />", _phrase567));
                if (!Dict.TryGetValue(568, out string _phrase568))
                {
                    _phrase568 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"568\" Phrase=\"{0}\" />", _phrase568));
                if (!Dict.TryGetValue(569, out string _phrase569))
                {
                    _phrase569 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"569\" Phrase=\"{0}\" />", _phrase569));
                if (!Dict.TryGetValue(570, out string _phrase570))
                {
                    _phrase570 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"570\" Phrase=\"{0}\" />", _phrase570));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Night_Vote *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(571, out string _phrase571))
                {
                    _phrase571 = "You can not start a vote during a bloodmoon.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"571\" Phrase=\"{0}\" />", _phrase571));
                if (!Dict.TryGetValue(572, out string _phrase572))
                {
                    _phrase572 = "You can not start a vote during the day.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"572\" Phrase=\"{0}\" />", _phrase572));
                if (!Dict.TryGetValue(573, out string _phrase573))
                {
                    _phrase573 = "A vote to skip the night has begun. You have 60 seconds to type {CommandPrivate}{Command70}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"573\" Phrase=\"{0}\" />", _phrase573));
                if (!Dict.TryGetValue(574, out string _phrase574))
                {
                    _phrase574 = "You can only start this vote if at least {Count} players are online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"574\" Phrase=\"{0}\" />", _phrase574));
                if (!Dict.TryGetValue(575, out string _phrase575))
                {
                    _phrase575 = "Players voted yes to skip this night. Good morning.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"575\" Phrase=\"{0}\" />", _phrase575));
                if (!Dict.TryGetValue(576, out string _phrase576))
                {
                    _phrase576 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"576\" Phrase=\"{0}\" />", _phrase576));
                if (!Dict.TryGetValue(577, out string _phrase577))
                {
                    _phrase577 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"577\" Phrase=\"{0}\" />", _phrase577));
                if (!Dict.TryGetValue(578, out string _phrase578))
                {
                    _phrase578 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"578\" Phrase=\"{0}\" />", _phrase578));
                if (!Dict.TryGetValue(579, out string _phrase579))
                {
                    _phrase579 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"579\" Phrase=\"{0}\" />", _phrase579));
                if (!Dict.TryGetValue(580, out string _phrase580))
                {
                    _phrase580 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"580\" Phrase=\"{0}\" />", _phrase580));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Night_Alert ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(581, out string _phrase581))
                {
                    _phrase581 = "{Time} hours until night time.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"581\" Phrase=\"{0}\" />", _phrase581));
                if (!Dict.TryGetValue(582, out string _phrase582))
                {
                    _phrase582 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"582\" Phrase=\"{0}\" />", _phrase582));
                if (!Dict.TryGetValue(583, out string _phrase583))
                {
                    _phrase583 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"583\" Phrase=\"{0}\" />", _phrase583));
                if (!Dict.TryGetValue(584, out string _phrase584))
                {
                    _phrase584 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"584\" Phrase=\"{0}\" />", _phrase584));
                if (!Dict.TryGetValue(585, out string _phrase585))
                {
                    _phrase585 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"585\" Phrase=\"{0}\" />", _phrase585));
                if (!Dict.TryGetValue(586, out string _phrase586))
                {
                    _phrase586 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"586\" Phrase=\"{0}\" />", _phrase586));
                if (!Dict.TryGetValue(587, out string _phrase587))
                {
                    _phrase587 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"587\" Phrase=\"{0}\" />", _phrase587));
                if (!Dict.TryGetValue(588, out string _phrase588))
                {
                    _phrase588 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"588\" Phrase=\"{0}\" />", _phrase588));
                if (!Dict.TryGetValue(589, out string _phrase589))
                {
                    _phrase589 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"589\" Phrase=\"{0}\" />", _phrase589));
                if (!Dict.TryGetValue(590, out string _phrase590))
                {
                    _phrase590 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"590\" Phrase=\"{0}\" />", _phrase590));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Hardcore ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(591, out string _phrase591))
                {
                    _phrase591 = "Hardcore Top Players";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"591\" Phrase=\"{0}\" />", _phrase591));
                if (!Dict.TryGetValue(592, out string _phrase592))
                {
                    _phrase592 = "Playtime 1 {Name1}, {Session1}. Playtime 2 {Name2}, {Session3}. Playtime 3 {Name3}, {Session3}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"592\" Phrase=\"{0}\" />", _phrase592));
                if (!Dict.TryGetValue(593, out string _phrase593))
                {
                    _phrase593 = "Score 1 {Name1}, {Score1}. Score 2 {Name2}, {Score2}. Score 3 {Name3}, {Score3}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"593\" Phrase=\"{0}\" />", _phrase593));
                if (!Dict.TryGetValue(594, out string _phrase594))
                {
                    _phrase594 = "Hardcore: Name {PlayerName}, Playtime {PlayTime}, Player Kills {PlayerKills}, Zombie Kills {ZombieKills}, Deaths {Deaths}, Score {Score}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"594\" Phrase=\"{0}\" />", _phrase594));
                if (!Dict.TryGetValue(595, out string _phrase595))
                {
                    _phrase595 = "Hardcore mode is enabled! You have {Value} lives remaining...";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"595\" Phrase=\"{0}\" />", _phrase595));
                if (!Dict.TryGetValue(596, out string _phrase596))
                {
                    _phrase596 = "There are no hardcore records";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"596\" Phrase=\"{0}\" />", _phrase596));
                if (!Dict.TryGetValue(597, out string _phrase597))
                {
                    _phrase597 = "You have bought one extra life.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"597\" Phrase=\"{0}\" />", _phrase597));
                if (!Dict.TryGetValue(598, out string _phrase598))
                {
                    _phrase598 = "You do not have enough to buy a life.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"598\" Phrase=\"{0}\" />", _phrase598));
                if (!Dict.TryGetValue(599, out string _phrase599))
                {
                    _phrase599 = "You are at the maximum extra lives.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"599\" Phrase=\"{0}\" />", _phrase599));
                if (!Dict.TryGetValue(600, out string _phrase600))
                {
                    _phrase600 = "You have not turned on hardcore mode.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"600\" Phrase=\"{0}\" />", _phrase600));
                if (!Dict.TryGetValue(601, out string _phrase601))
                {
                    _phrase601 = "You are now in hardcore mode with limited lives remaining. Type {CommandPrivate}{Command127} to check how many lives remain.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"601\" Phrase=\"{0}\" />", _phrase601));
                if (!Dict.TryGetValue(602, out string _phrase602))
                {
                    _phrase602 = "You are already signed up for hardcore mode. Type {CommandPrivate}{Command127} to check how many lives remain.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"602\" Phrase=\"{0}\" />", _phrase602));
                if (!Dict.TryGetValue(603, out string _phrase603))
                {
                    _phrase603 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"603\" Phrase=\"{0}\" />", _phrase603));
                if (!Dict.TryGetValue(604, out string _phrase604))
                {
                    _phrase604 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"604\" Phrase=\"{0}\" />", _phrase604));
                if (!Dict.TryGetValue(605, out string _phrase605))
                {
                    _phrase605 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"605\" Phrase=\"{0}\" />", _phrase605));
                if (!Dict.TryGetValue(606, out string _phrase606))
                {
                    _phrase606 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"606\" Phrase=\"{0}\" />", _phrase606));
                if (!Dict.TryGetValue(607, out string _phrase607))
                {
                    _phrase607 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"607\" Phrase=\"{0}\" />", _phrase607));
                if (!Dict.TryGetValue(608, out string _phrase608))
                {
                    _phrase608 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"608\" Phrase=\"{0}\" />", _phrase608));
                if (!Dict.TryGetValue(609, out string _phrase609))
                {
                    _phrase609 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"609\" Phrase=\"{0}\" />", _phrase609));
                if (!Dict.TryGetValue(610, out string _phrase610))
                {
                    _phrase610 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"610\" Phrase=\"{0}\" />", _phrase610));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- **************** Chat_Flood_Protection ***************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(611, out string _phrase611))
                {
                    _phrase611 = "You have sent too many messages in one minute. Your chat function is locked temporarily.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"611\" Phrase=\"{0}\" />", _phrase611));
                if (!Dict.TryGetValue(612, out string _phrase612))
                {
                    _phrase612 = "Message is too long.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"612\" Phrase=\"{0}\" />", _phrase612));
                if (!Dict.TryGetValue(613, out string _phrase613))
                {
                    _phrase613 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"613\" Phrase=\"{0}\" />", _phrase613));
                if (!Dict.TryGetValue(614, out string _phrase614))
                {
                    _phrase614 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"614\" Phrase=\"{0}\" />", _phrase614));
                if (!Dict.TryGetValue(615, out string _phrase615))
                {
                    _phrase615 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"615\" Phrase=\"{0}\" />", _phrase615));
                if (!Dict.TryGetValue(616, out string _phrase616))
                {
                    _phrase616 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"616\" Phrase=\"{0}\" />", _phrase616));
                if (!Dict.TryGetValue(617, out string _phrase617))
                {
                    _phrase617 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"617\" Phrase=\"{0}\" />", _phrase617));
                if (!Dict.TryGetValue(618, out string _phrase618))
                {
                    _phrase618 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"618\" Phrase=\"{0}\" />", _phrase618));
                if (!Dict.TryGetValue(619, out string _phrase619))
                {
                    _phrase619 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"619\" Phrase=\"{0}\" />", _phrase619));
                if (!Dict.TryGetValue(620, out string _phrase620))
                {
                    _phrase620 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"620\" Phrase=\"{0}\" />", _phrase620));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Auction ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(621, out string _phrase621))
                {
                    _phrase621 = "Your auction item {Name} has been removed from the secure loot and added to the auction.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"621\" Phrase=\"{0}\" />", _phrase621));
                if (!Dict.TryGetValue(622, out string _phrase622))
                {
                    _phrase622 = "You have the max auction items already listed. Wait for one to sell or cancel it with {CommandPrivate}{Command72} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"622\" Phrase=\"{0}\" />", _phrase622));
                if (!Dict.TryGetValue(623, out string _phrase623))
                {
                    _phrase623 = "You need to input a price greater than zero. This is not a transfer system.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"623\" Phrase=\"{0}\" />", _phrase623));
                if (!Dict.TryGetValue(624, out string _phrase624))
                {
                    _phrase624 = "Your sell price must be an integer and greater than zero.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"624\" Phrase=\"{0}\" />", _phrase624));
                if (!Dict.TryGetValue(625, out string _phrase625))
                {
                    _phrase625 = "# {Id}: {Count} {Item} at {Quality} quality, {Durability} percent durability for {Price} {Coin}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"625\" Phrase=\"{0}\" />", _phrase625));
                if (!Dict.TryGetValue(626, out string _phrase626))
                {
                    _phrase626 = "No items are currently for sale.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"626\" Phrase=\"{0}\" />", _phrase626));
                if (!Dict.TryGetValue(627, out string _phrase627))
                {
                    _phrase627 = "You can not make this purchase. You need {Value} more {Name}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"627\" Phrase=\"{0}\" />", _phrase627));
                if (!Dict.TryGetValue(628, out string _phrase628))
                {
                    _phrase628 = "This # could not be found. Please check the auction list by typing {CommandPrivate}{Command71}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"628\" Phrase=\"{0}\" />", _phrase628));
                if (!Dict.TryGetValue(629, out string _phrase629))
                {
                    _phrase629 = "You have purchased {Count} {ItemName} from the auction for {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"629\" Phrase=\"{0}\" />", _phrase629));
                if (!Dict.TryGetValue(630, out string _phrase630))
                {
                    _phrase630 = "Your auction item was purchased and the value placed in your wallet.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"630\" Phrase=\"{0}\" />", _phrase630));
                if (!Dict.TryGetValue(631, out string _phrase631))
                {
                    _phrase631 = "Your auction item has returned to you.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"631\" Phrase=\"{0}\" />", _phrase631));
                if (!Dict.TryGetValue(632, out string _phrase632))
                {
                    _phrase632 = "Could not find this id listed in the auction. Unable to cancel.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"632\" Phrase=\"{0}\" />", _phrase632));
                if (!Dict.TryGetValue(633, out string _phrase633))
                {
                    _phrase633 = "The auction is disabled for your admin tier.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"633\" Phrase=\"{0}\" />", _phrase633));
                if (!Dict.TryGetValue(634, out string _phrase634))
                {
                    _phrase634 = "You have used an auction item # that does not exist or has sold.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"634\" Phrase=\"{0}\" />", _phrase634));
                if (!Dict.TryGetValue(635, out string _phrase635))
                {
                    _phrase635 = "Command is disabled. Wallet is not enabled.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"635\" Phrase=\"{0}\" />", _phrase635));
                if (!Dict.TryGetValue(636, out string _phrase636))
                {
                    _phrase636 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"636\" Phrase=\"{0}\" />", _phrase636));
                if (!Dict.TryGetValue(637, out string _phrase637))
                {
                    _phrase637 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"637\" Phrase=\"{0}\" />", _phrase637));
                if (!Dict.TryGetValue(638, out string _phrase638))
                {
                    _phrase638 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"638\" Phrase=\"{0}\" />", _phrase638));
                if (!Dict.TryGetValue(639, out string _phrase639))
                {
                    _phrase639 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"639\" Phrase=\"{0}\" />", _phrase639));
                if (!Dict.TryGetValue(640, out string _phrase640))
                {
                    _phrase640 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"640\" Phrase=\"{0}\" />", _phrase640));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Bank ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(641, out string _phrase641))
                {
                    _phrase641 = "Your bank account is worth {Value}. Transfer Id is {Id}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"641\" Phrase=\"{0}\" />", _phrase641));
                if (!Dict.TryGetValue(642, out string _phrase642))
                {
                    _phrase642 = "You can not use this command here. Stand in your own or a friend's claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"642\" Phrase=\"{0}\" />", _phrase642));
                if (!Dict.TryGetValue(643, out string _phrase643))
                {
                    _phrase643 = "Deposited {Value} {Name} from the secure loot to your bank account. " + "{Percent}" + "% deposit fee was applied.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"643\" Phrase=\"{0}\" />", _phrase643));
                if (!Dict.TryGetValue(644, out string _phrase644))
                {
                    _phrase644 = "Deposited {Value} {Name} from the secure loot to your bank account.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"644\" Phrase=\"{0}\" />", _phrase644));
                if (!Dict.TryGetValue(645, out string _phrase645))
                {
                    _phrase645 = "Could not find any {CoinName} in a near by secure loot.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"645\" Phrase=\"{0}\" />", _phrase645));
                if (!Dict.TryGetValue(646, out string _phrase646))
                {
                    _phrase646 = "You input an invalid integer. Type {CommandPrivate}{Command95} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"646\" Phrase=\"{0}\" />", _phrase646));
                if (!Dict.TryGetValue(647, out string _phrase647))
                {
                    _phrase647 = "The bank coin is not setup correctly, contact a server Admin.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"647\" Phrase=\"{0}\" />", _phrase647));
                if (!Dict.TryGetValue(648, out string _phrase648))
                {
                    _phrase648 = "Your bag can not take all of the {CoinName}. Check on the ground by your feet.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"648\" Phrase=\"{0}\" />", _phrase648));
                if (!Dict.TryGetValue(649, out string _phrase649))
                {
                    _phrase649 = "You have received the {CoinName} in your bag.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"649\" Phrase=\"{0}\" />", _phrase649));
                if (!Dict.TryGetValue(650, out string _phrase650))
                {
                    _phrase650 = "You can only withdraw a full stack at a time. The maximum stack size is {Max}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"650\" Phrase=\"{0}\" />", _phrase650));
                if (!Dict.TryGetValue(651, out string _phrase651))
                {
                    _phrase651 = "Your bank account does not have enough to withdraw that value. Bank account is currently {Total}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"651\" Phrase=\"{0}\" />", _phrase651));
                if (!Dict.TryGetValue(652, out string _phrase652))
                {
                    _phrase652 = "You input an invalid integer. Type {CommandPrivate}{Command96} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"652\" Phrase=\"{0}\" />", _phrase652));
                if (!Dict.TryGetValue(653, out string _phrase653))
                {
                    _phrase653 = "Deposited {Value} {CoinName} from your wallet to your bank account." + " {Percent}" + "% fee was applied.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"653\" Phrase=\"{0}\" />", _phrase653));
                if (!Dict.TryGetValue(654, out string _phrase654))
                {
                    _phrase654 = "Deposited {Value} {CoinName} from your wallet to your bank account.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"654\" Phrase=\"{0}\" />", _phrase654));
                if (!Dict.TryGetValue(655, out string _phrase655))
                {
                    _phrase655 = "Your wallet does not have enough to deposit that value.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"655\" Phrase=\"{0}\" />", _phrase655));
                if (!Dict.TryGetValue(656, out string _phrase656))
                {
                    _phrase656 = "You input an invalid integer. Type {CommandPrivate}{Command97} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"656\" Phrase=\"{0}\" />", _phrase656));
                if (!Dict.TryGetValue(657, out string _phrase657))
                {
                    _phrase657 = "You have received the {Name} from your bank. It has gone to your wallet.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"657\" Phrase=\"{0}\" />", _phrase657));
                if (!Dict.TryGetValue(658, out string _phrase658))
                {
                    _phrase658 = "Your bank account does not have enough to withdraw that value.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"658\" Phrase=\"{0}\" />", _phrase658));
                if (!Dict.TryGetValue(659, out string _phrase659))
                {
                    _phrase659 = "You input an invalid integer. Type {CommandPrivate}{Command98} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"659\" Phrase=\"{0}\" />", _phrase659));
                if (!Dict.TryGetValue(660, out string _phrase660))
                {
                    _phrase660 = "You have made a bank transfer of {Value} to player {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"660\" Phrase=\"{0}\" />", _phrase660));
                if (!Dict.TryGetValue(661, out string _phrase661))
                {
                    _phrase661 = "You have received a bank transfer of {Value} from player {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"661\" Phrase=\"{0}\" />", _phrase661));
                if (!Dict.TryGetValue(662, out string _phrase662))
                {
                    _phrase662 = "The player is offline. They must be online to transfer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"662\" Phrase=\"{0}\" />", _phrase662));
                if (!Dict.TryGetValue(663, out string _phrase663))
                {
                    _phrase663 = "Id not found. Ask for the transfer Id from the player you want to transfer to.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"663\" Phrase=\"{0}\" />", _phrase663));
                if (!Dict.TryGetValue(664, out string _phrase664))
                {
                    _phrase664 = "You input an invalid integer. Type {CommandPrivate}{Command99} #.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"664\" Phrase=\"{0}\" />", _phrase664));
                if (!Dict.TryGetValue(665, out string _phrase665))
                {
                    _phrase665 = "You input an invalid integer.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"665\" Phrase=\"{0}\" />", _phrase665));
                if (!Dict.TryGetValue(666, out string _phrase666))
                {
                    _phrase666 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"666\" Phrase=\"{0}\" />", _phrase666));
                if (!Dict.TryGetValue(667, out string _phrase667))
                {
                    _phrase667 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"667\" Phrase=\"{0}\" />", _phrase667));
                if (!Dict.TryGetValue(668, out string _phrase668))
                {
                    _phrase668 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"668\" Phrase=\"{0}\" />", _phrase668));
                if (!Dict.TryGetValue(669, out string _phrase669))
                {
                    _phrase669 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"669\" Phrase=\"{0}\" />", _phrase669));
                if (!Dict.TryGetValue(670, out string _phrase670))
                {
                    _phrase670 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"670\" Phrase=\"{0}\" />", _phrase670));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Battle_Logger ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(671, out string _phrase671))
                {
                    _phrase671 = "You moved and need to restart your countdown.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"671\" Phrase=\"{0}\" />", _phrase671));
                if (!Dict.TryGetValue(672, out string _phrase672))
                {
                    _phrase672 = "Type {CommandPrivate}{Command131} to quit the game or you may drop items.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"672\" Phrase=\"{0}\" />", _phrase672));
                if (!Dict.TryGetValue(673, out string _phrase673))
                {
                    _phrase673 = "You have disconnected. Thank you for playing with us. Come back soon";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"673\" Phrase=\"{0}\" />", _phrase673));
                if (!Dict.TryGetValue(674, out string _phrase674))
                {
                    _phrase674 = "Please wait {Time} seconds for disconnection and do not move";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"674\" Phrase=\"{0}\" />", _phrase674));
                if (!Dict.TryGetValue(675, out string _phrase675))
                {
                    _phrase675 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"675\" Phrase=\"{0}\" />", _phrase675));
                if (!Dict.TryGetValue(676, out string _phrase676))
                {
                    _phrase676 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"676\" Phrase=\"{0}\" />", _phrase676));
                if (!Dict.TryGetValue(677, out string _phrase677))
                {
                    _phrase677 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"677\" Phrase=\"{0}\" />", _phrase677));
                if (!Dict.TryGetValue(678, out string _phrase678))
                {
                    _phrase678 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"678\" Phrase=\"{0}\" />", _phrase678));
                if (!Dict.TryGetValue(679, out string _phrase679))
                {
                    _phrase679 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"679\" Phrase=\"{0}\" />", _phrase679));
                if (!Dict.TryGetValue(680, out string _phrase680))
                {
                    _phrase680 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"680\" Phrase=\"{0}\" />", _phrase680));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Bloodmoon ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(681, out string _phrase681))
                {
                    _phrase681 = "Next horde night is in {Value} days.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"681\" Phrase=\"{0}\" />", _phrase681));
                if (!Dict.TryGetValue(682, out string _phrase682))
                {
                    _phrase682 = "Next horde night is today";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"682\" Phrase=\"{0}\" />", _phrase682));
                if (!Dict.TryGetValue(683, out string _phrase683))
                {
                    _phrase683 = "The horde is here!";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"683\" Phrase=\"{0}\" />", _phrase683));
                if (!Dict.TryGetValue(684, out string _phrase684))
                {
                    _phrase684 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"684\" Phrase=\"{0}\" />", _phrase684));
                if (!Dict.TryGetValue(685, out string _phrase685))
                {
                    _phrase685 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"685\" Phrase=\"{0}\" />", _phrase685));
                if (!Dict.TryGetValue(686, out string _phrase686))
                {
                    _phrase686 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"686\" Phrase=\"{0}\" />", _phrase686));
                if (!Dict.TryGetValue(687, out string _phrase687))
                {
                    _phrase687 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"687\" Phrase=\"{0}\" />", _phrase687));
                if (!Dict.TryGetValue(688, out string _phrase688))
                {
                    _phrase688 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"688\" Phrase=\"{0}\" />", _phrase688));
                if (!Dict.TryGetValue(689, out string _phrase689))
                {
                    _phrase689 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"689\" Phrase=\"{0}\" />", _phrase689));
                if (!Dict.TryGetValue(690, out string _phrase690))
                {
                    _phrase690 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"690\" Phrase=\"{0}\" />", _phrase690));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Bloodmoon_Warrior ****************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(691, out string _phrase691))
                {
                    _phrase691 = "Hades has called upon you. Survive this night and kill {Count} zombies to be rewarded by the king of the underworld.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"691\" Phrase=\"{0}\" />", _phrase691));
                if (!Dict.TryGetValue(692, out string _phrase692))
                {
                    _phrase692 = "You have survived and been rewarded by hades himself. Your death count was reduced by one.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"692\" Phrase=\"{0}\" />", _phrase692));
                if (!Dict.TryGetValue(693, out string _phrase693))
                {
                    _phrase693 = "You have survived and been rewarded by hades himself.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"693\" Phrase=\"{0}\" />", _phrase693));
                if (!Dict.TryGetValue(694, out string _phrase694))
                {
                    _phrase694 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"694\" Phrase=\"{0}\" />", _phrase694));
                if (!Dict.TryGetValue(695, out string _phrase695))
                {
                    _phrase695 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"695\" Phrase=\"{0}\" />", _phrase695));
                if (!Dict.TryGetValue(696, out string _phrase696))
                {
                    _phrase696 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"696\" Phrase=\"{0}\" />", _phrase696));
                if (!Dict.TryGetValue(697, out string _phrase697))
                {
                    _phrase697 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"697\" Phrase=\"{0}\" />", _phrase697));
                if (!Dict.TryGetValue(698, out string _phrase698))
                {
                    _phrase698 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"698\" Phrase=\"{0}\" />", _phrase698));
                if (!Dict.TryGetValue(699, out string _phrase699))
                {
                    _phrase699 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"699\" Phrase=\"{0}\" />", _phrase699));
                if (!Dict.TryGetValue(700, out string _phrase700))
                {
                    _phrase700 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"700\" Phrase=\"{0}\" />", _phrase700));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* First_Claim_Block ****************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(701, out string _phrase701))
                {
                    _phrase701 = "Claim block has been added to your inventory or if inventory is full, it dropped at your feet.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"701\" Phrase=\"{0}\" />", _phrase701));
                if (!Dict.TryGetValue(702, out string _phrase702))
                {
                    _phrase702 = "You have already received your first claim block. Contact an administrator if you require help.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"702\" Phrase=\"{0}\" />", _phrase702));
                if (!Dict.TryGetValue(703, out string _phrase703))
                {
                    _phrase703 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"703\" Phrase=\"{0}\" />", _phrase703));
                if (!Dict.TryGetValue(704, out string _phrase704))
                {
                    _phrase704 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"704\" Phrase=\"{0}\" />", _phrase704));
                if (!Dict.TryGetValue(705, out string _phrase705))
                {
                    _phrase705 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"705\" Phrase=\"{0}\" />", _phrase705));
                if (!Dict.TryGetValue(706, out string _phrase706))
                {
                    _phrase706 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"706\" Phrase=\"{0}\" />", _phrase706));
                if (!Dict.TryGetValue(707, out string _phrase707))
                {
                    _phrase707 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"707\" Phrase=\"{0}\" />", _phrase707));
                if (!Dict.TryGetValue(708, out string _phrase708))
                {
                    _phrase708 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"708\" Phrase=\"{0}\" />", _phrase708));
                if (!Dict.TryGetValue(709, out string _phrase709))
                {
                    _phrase709 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"709\" Phrase=\"{0}\" />", _phrase709));
                if (!Dict.TryGetValue(710, out string _phrase710))
                {
                    _phrase710 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"710\" Phrase=\"{0}\" />", _phrase710));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Kick_Vote ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(711, out string _phrase711))
                {
                    _phrase711 = "A vote to kick {PlayerName} has begun and will close in 30 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"711\" Phrase=\"{0}\" />", _phrase711));
                if (!Dict.TryGetValue(712, out string _phrase712))
                {
                    _phrase712 = "This player id was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"712\" Phrase=\"{0}\" />", _phrase712));
                if (!Dict.TryGetValue(713, out string _phrase713))
                {
                    _phrase713 = "Not enough players are online to start a vote to kick.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"713\" Phrase=\"{0}\" />", _phrase713));
                if (!Dict.TryGetValue(714, out string _phrase714))
                {
                    _phrase714 = "PlayerName = {PlayerName}, # = {Id}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"714\" Phrase=\"{0}\" />", _phrase714));
                if (!Dict.TryGetValue(715, out string _phrase715))
                {
                    _phrase715 = "Type {CommandPrivate}{Command68} to vote kick the player.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"715\" Phrase=\"{0}\" />", _phrase715));
                if (!Dict.TryGetValue(716, out string _phrase716))
                {
                    _phrase716 = "Can not start a vote for yourself.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"716\" Phrase=\"{0}\" />", _phrase716));
                if (!Dict.TryGetValue(717, out string _phrase717))
                {
                    _phrase717 = "Players voted to kick but not enough votes were cast.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"717\" Phrase=\"{0}\" />", _phrase717));
                if (!Dict.TryGetValue(718, out string _phrase718))
                {
                    _phrase718 = "No votes were cast to kick the player.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"718\" Phrase=\"{0}\" />", _phrase718));
                if (!Dict.TryGetValue(719, out string _phrase719))
                {
                    _phrase719 = "Type {CommandPrivate}{Command68} # to start a vote to kick that player.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"719\" Phrase=\"{0}\" />", _phrase719));
                if (!Dict.TryGetValue(720, out string _phrase720))
                {
                    _phrase720 = "The players voted to kick you from the game";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"720\" Phrase=\"{0}\" />", _phrase720));
                if (!Dict.TryGetValue(721, out string _phrase721))
                {
                    _phrase721 = "No other users were found online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"721\" Phrase=\"{0}\" />", _phrase721));
                if (!Dict.TryGetValue(722, out string _phrase722))
                {
                    _phrase722 = "There is a vote already open.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"722\" Phrase=\"{0}\" />", _phrase722));
                if (!Dict.TryGetValue(723, out string _phrase723))
                {
                    _phrase723 = "There are now {Value} of {VotesNeeded} votes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"723\" Phrase=\"{0}\" />", _phrase723));
                if (!Dict.TryGetValue(724, out string _phrase724))
                {
                    _phrase724 = "You have already voted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"724\" Phrase=\"{0}\" />", _phrase724));
                if (!Dict.TryGetValue(725, out string _phrase725))
                {
                    _phrase725 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"725\" Phrase=\"{0}\" />", _phrase725));
                if (!Dict.TryGetValue(726, out string _phrase726))
                {
                    _phrase726 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"726\" Phrase=\"{0}\" />", _phrase726));
                if (!Dict.TryGetValue(727, out string _phrase727))
                {
                    _phrase727 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"727\" Phrase=\"{0}\" />", _phrase727));
                if (!Dict.TryGetValue(728, out string _phrase728))
                {
                    _phrase728 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"728\" Phrase=\"{0}\" />", _phrase728));
                if (!Dict.TryGetValue(729, out string _phrase729))
                {
                    _phrase729 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"729\" Phrase=\"{0}\" />", _phrase729));
                if (!Dict.TryGetValue(730, out string _phrase730))
                {
                    _phrase730 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"730\" Phrase=\"{0}\" />", _phrase730));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Home ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(731, out string _phrase731))
                {
                    _phrase731 = "You have no homes saved.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"731\" Phrase=\"{0}\" />", _phrase731));
                if (!Dict.TryGetValue(732, out string _phrase732))
                {
                    _phrase732 = "Home {Name} @ {Value} {Value2} {Value3}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"732\" Phrase=\"{0}\" />", _phrase732));
                if (!Dict.TryGetValue(733, out string _phrase733))
                {
                    _phrase733 = "You can not use home commands while in a event.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"733\" Phrase=\"{0}\" />", _phrase733));
                if (!Dict.TryGetValue(734, out string _phrase734))
                {
                    _phrase734 = "You can only use {CommandPrivate}{Command1} once every {DelayBetweenUses} minutes. Time remaining: {Value} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"734\" Phrase=\"{0}\" />", _phrase734));
                if (!Dict.TryGetValue(735, out string _phrase735))
                {
                    _phrase735 = "You can only teleport to a home that is inside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"735\" Phrase=\"{0}\" />", _phrase735));
                if (!Dict.TryGetValue(736, out string _phrase736))
                {
                    _phrase736 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"736\" Phrase=\"{0}\" />", _phrase736));
                if (!Dict.TryGetValue(737, out string _phrase737))
                {
                    _phrase737 = "This home was not found on your list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"737\" Phrase=\"{0}\" />", _phrase737));
                if (!Dict.TryGetValue(738, out string _phrase738))
                {
                    _phrase738 = "You can only save a home that is inside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"738\" Phrase=\"{0}\" />", _phrase738));
                if (!Dict.TryGetValue(739, out string _phrase739))
                {
                    _phrase739 = "Saved home as {Name} at position {Position}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"739\" Phrase=\"{0}\" />", _phrase739));
                if (!Dict.TryGetValue(740, out string _phrase740))
                {
                    _phrase740 = "This home already exists. Choose another name.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"740\" Phrase=\"{0}\" />", _phrase740));
                if (!Dict.TryGetValue(741, out string _phrase741))
                {
                    _phrase741 = "You have a maximum {Value} homes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"741\" Phrase=\"{0}\" />", _phrase741));
                if (!Dict.TryGetValue(742, out string _phrase742))
                {
                    _phrase742 = "Saved home as {Name} at position {Position}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"742\" Phrase=\"{0}\" />", _phrase742));
                if (!Dict.TryGetValue(743, out string _phrase743))
                {
                    _phrase743 = "Home {Name} has been deleted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"743\" Phrase=\"{0}\" />", _phrase743));
                if (!Dict.TryGetValue(744, out string _phrase744))
                {
                    _phrase744 = "Your friend {PlayerName} has invited you to their saved home. Type {CommandPrivate}{Command5} to accept the request.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"744\" Phrase=\"{0}\" />", _phrase744));
                if (!Dict.TryGetValue(745, out string _phrase745))
                {
                    _phrase745 = "Invited your friend {PlayerName} to your saved home.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"745\" Phrase=\"{0}\" />", _phrase745));
                if (!Dict.TryGetValue(746, out string _phrase746))
                {
                    _phrase746 = "You have run out of time to accept your friend's home invitation.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"746\" Phrase=\"{0}\" />", _phrase746));
                if (!Dict.TryGetValue(747, out string _phrase747))
                {
                    _phrase747 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"747\" Phrase=\"{0}\" />", _phrase747));
                if (!Dict.TryGetValue(748, out string _phrase748))
                {
                    _phrase748 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"748\" Phrase=\"{0}\" />", _phrase748));
                if (!Dict.TryGetValue(749, out string _phrase749))
                {
                    _phrase749 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"749\" Phrase=\"{0}\" />", _phrase749));
                if (!Dict.TryGetValue(750, out string _phrase750))
                {
                    _phrase750 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"750\" Phrase=\"{0}\" />", _phrase750));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Mute ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(751, out string _phrase751))
                {
                    _phrase751 = "Could not mute. {PlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"751\" Phrase=\"{0}\" />", _phrase751));
                if (!Dict.TryGetValue(752, out string _phrase752))
                {
                    _phrase752 = "You have muted player {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"752\" Phrase=\"{0}\" />", _phrase752));
                if (!Dict.TryGetValue(753, out string _phrase753))
                {
                    _phrase753 = "You have already muted player {PlayerName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"753\" Phrase=\"{0}\" />", _phrase753));
                if (!Dict.TryGetValue(754, out string _phrase754))
                {
                    _phrase754 = "You have removed {PlayerName} with id {EntityId} from your mute list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"754\" Phrase=\"{0}\" />", _phrase754));
                if (!Dict.TryGetValue(755, out string _phrase755))
                {
                    _phrase755 = "You have removed player with id {EntityId} from your mute list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"755\" Phrase=\"{0}\" />", _phrase755));
                if (!Dict.TryGetValue(756, out string _phrase756))
                {
                    _phrase756 = "This player is not on your mute list.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"756\" Phrase=\"{0}\" />", _phrase756));
                if (!Dict.TryGetValue(757, out string _phrase757))
                {
                    _phrase757 = "Muted: Unknown name with id {EntityId}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"757\" Phrase=\"{0}\" />", _phrase757));
                if (!Dict.TryGetValue(758, out string _phrase758))
                {
                    _phrase758 = "Invalid id: {EntityId}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"758\" Phrase=\"{0}\" />", _phrase758));
                if (!Dict.TryGetValue(759, out string _phrase759))
                {
                    _phrase759 = "Muted: {PlayerName} with id {EntityId}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"759\" Phrase=\"{0}\" />", _phrase759));
                if (!Dict.TryGetValue(760, out string _phrase760))
                {
                    _phrase760 = "You are muted and blocked from commands.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"760\" Phrase=\"{0}\" />", _phrase760));
                if (!Dict.TryGetValue(761, out string _phrase761))
                {
                    _phrase761 = "You are muted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"761\" Phrase=\"{0}\" />", _phrase761));
                if (!Dict.TryGetValue(762, out string _phrase762))
                {
                    _phrase762 = "You have no muted players.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"762\" Phrase=\"{0}\" />", _phrase762));
                if (!Dict.TryGetValue(763, out string _phrase763))
                {
                    _phrase763 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"763\" Phrase=\"{0}\" />", _phrase763));
                if (!Dict.TryGetValue(764, out string _phrase764))
                {
                    _phrase764 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"764\" Phrase=\"{0}\" />", _phrase764));
                if (!Dict.TryGetValue(765, out string _phrase765))
                {
                    _phrase765 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"765\" Phrase=\"{0}\" />", _phrase765));
                if (!Dict.TryGetValue(766, out string _phrase766))
                {
                    _phrase766 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"766\" Phrase=\"{0}\" />", _phrase766));
                if (!Dict.TryGetValue(767, out string _phrase767))
                {
                    _phrase767 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"767\" Phrase=\"{0}\" />", _phrase767));
                if (!Dict.TryGetValue(768, out string _phrase768))
                {
                    _phrase768 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"768\" Phrase=\"{0}\" />", _phrase768));
                if (!Dict.TryGetValue(769, out string _phrase769))
                {
                    _phrase769 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"769\" Phrase=\"{0}\" />", _phrase769));
                if (!Dict.TryGetValue(770, out string _phrase770))
                {
                    _phrase770 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"770\" Phrase=\"{0}\" />", _phrase770));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Event ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(771, out string _phrase771))
                {
                    _phrase771 = "Not enough players signed up for the event. The invitation and setup has been cleared.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"771\" Phrase=\"{0}\" />", _phrase771));
                if (!Dict.TryGetValue(772, out string _phrase772))
                {
                    _phrase772 = "The event did not have enough players join. It has been cancelled.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"772\" Phrase=\"{0}\" />", _phrase772));
                if (!Dict.TryGetValue(773, out string _phrase773))
                {
                    _phrase773 = "The event is full. Unable to join.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"773\" Phrase=\"{0}\" />", _phrase773));
                if (!Dict.TryGetValue(774, out string _phrase774))
                {
                    _phrase774 = "You have signed up for the event and your current location has been saved for return.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"774\" Phrase=\"{0}\" />", _phrase774));
                if (!Dict.TryGetValue(775, out string _phrase775))
                {
                    _phrase775 = "You are on team {Value}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"775\" Phrase=\"{0}\" />", _phrase775));
                if (!Dict.TryGetValue(776, out string _phrase776))
                {
                    _phrase776 = "{EventName} has now started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"776\" Phrase=\"{0}\" />", _phrase776));
                if (!Dict.TryGetValue(777, out string _phrase777))
                {
                    _phrase777 = "{EventName} still has space for more players. Type {CommandPrivate}{Command100} to join";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"777\" Phrase=\"{0}\" />", _phrase777));
                if (!Dict.TryGetValue(778, out string _phrase778))
                {
                    _phrase778 = "{Value} of {PlayerTotal} have joined the event.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"778\" Phrase=\"{0}\" />", _phrase778));
                if (!Dict.TryGetValue(779, out string _phrase779))
                {
                    _phrase779 = "You have already joined this event. It will start when enough players sign up.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"779\" Phrase=\"{0}\" />", _phrase779));
                if (!Dict.TryGetValue(780, out string _phrase780))
                {
                    _phrase780 = "The event is at half time.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"780\" Phrase=\"{0}\" />", _phrase780));
                if (!Dict.TryGetValue(781, out string _phrase781))
                {
                    _phrase781 = "The event has five minutes remaining.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"781\" Phrase=\"{0}\" />", _phrase781));
                if (!Dict.TryGetValue(782, out string _phrase782))
                {
                    _phrase782 = "If you need to extend the time remaining, use the console to type event extend #. The time is in minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"782\" Phrase=\"{0}\" />", _phrase782));
                if (!Dict.TryGetValue(783, out string _phrase783))
                {
                    _phrase783 = "The event has ended and all players have been sent to their return positions.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"783\" Phrase=\"{0}\" />", _phrase783));
                if (!Dict.TryGetValue(784, out string _phrase784))
                {
                    _phrase784 = "The event ended while you were offline or not spawned. You have been sent to your return position.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"784\" Phrase=\"{0}\" />", _phrase784));
                if (!Dict.TryGetValue(785, out string _phrase785))
                {
                    _phrase785 = "You have been sent to your event spawn point.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"785\" Phrase=\"{0}\" />", _phrase785));
                if (!Dict.TryGetValue(786, out string _phrase786))
                {
                    _phrase786 = "The event ended while you were offline or not spawned.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"786\" Phrase=\"{0}\" />", _phrase786));
                if (!Dict.TryGetValue(787, out string _phrase787))
                {
                    _phrase787 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"787\" Phrase=\"{0}\" />", _phrase787));
                if (!Dict.TryGetValue(788, out string _phrase788))
                {
                    _phrase788 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"788\" Phrase=\"{0}\" />", _phrase788));
                if (!Dict.TryGetValue(789, out string _phrase789))
                {
                    _phrase789 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"789\" Phrase=\"{0}\" />", _phrase789));
                if (!Dict.TryGetValue(790, out string _phrase790))
                {
                    _phrase790 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"790\" Phrase=\"{0}\" />", _phrase790));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Session *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(791, out string _phrase791))
                {
                    _phrase791 = "Your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"791\" Phrase=\"{0}\" />", _phrase791));
                if (!Dict.TryGetValue(792, out string _phrase792))
                {
                    _phrase792 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"792\" Phrase=\"{0}\" />", _phrase792));
                if (!Dict.TryGetValue(793, out string _phrase793))
                {
                    _phrase793 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"793\" Phrase=\"{0}\" />", _phrase793));
                if (!Dict.TryGetValue(794, out string _phrase794))
                {
                    _phrase794 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"794\" Phrase=\"{0}\" />", _phrase794));
                if (!Dict.TryGetValue(795, out string _phrase795))
                {
                    _phrase795 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"795\" Phrase=\"{0}\" />", _phrase795));
                if (!Dict.TryGetValue(796, out string _phrase796))
                {
                    _phrase796 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"796\" Phrase=\"{0}\" />", _phrase796));
                if (!Dict.TryGetValue(797, out string _phrase797))
                {
                    _phrase797 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"797\" Phrase=\"{0}\" />", _phrase797));
                if (!Dict.TryGetValue(798, out string _phrase798))
                {
                    _phrase798 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"798\" Phrase=\"{0}\" />", _phrase798));
                if (!Dict.TryGetValue(799, out string _phrase799))
                {
                    _phrase799 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"799\" Phrase=\"{0}\" />", _phrase799));
                if (!Dict.TryGetValue(800, out string _phrase800))
                {
                    _phrase800 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"800\" Phrase=\"{0}\" />", _phrase800));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Night_Alert ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(801, out string _phrase801))
                {
                    _phrase801 = "{Value} hours until night time.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"801\" Phrase=\"{0}\" />", _phrase801));
                if (!Dict.TryGetValue(802, out string _phrase802))
                {
                    _phrase802 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"802\" Phrase=\"{0}\" />", _phrase802));
                if (!Dict.TryGetValue(803, out string _phrase803))
                {
                    _phrase803 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"803\" Phrase=\"{0}\" />", _phrase803));
                if (!Dict.TryGetValue(804, out string _phrase804))
                {
                    _phrase804 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"804\" Phrase=\"{0}\" />", _phrase804));
                if (!Dict.TryGetValue(805, out string _phrase805))
                {
                    _phrase805 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"805\" Phrase=\"{0}\" />", _phrase805));
                if (!Dict.TryGetValue(806, out string _phrase806))
                {
                    _phrase806 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"806\" Phrase=\"{0}\" />", _phrase806));
                if (!Dict.TryGetValue(807, out string _phrase807))
                {
                    _phrase807 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"807\" Phrase=\"{0}\" />", _phrase807));
                if (!Dict.TryGetValue(808, out string _phrase808))
                {
                    _phrase808 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"808\" Phrase=\"{0}\" />", _phrase808));
                if (!Dict.TryGetValue(809, out string _phrase809))
                {
                    _phrase809 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"809\" Phrase=\"{0}\" />", _phrase809));
                if (!Dict.TryGetValue(810, out string _phrase810))
                {
                    _phrase810 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"810\" Phrase=\"{0}\" />", _phrase810));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Player_List ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(811, out string _phrase811))
                {
                    _phrase811 = "Player = {PlayerName}, Id = {EntityId}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"811\" Phrase=\"{0}\" />", _phrase811));
                if (!Dict.TryGetValue(812, out string _phrase812))
                {
                    _phrase812 = "No other players found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"812\" Phrase=\"{0}\" />", _phrase812));
                if (!Dict.TryGetValue(813, out string _phrase813))
                {
                    _phrase813 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"813\" Phrase=\"{0}\" />", _phrase813));
                if (!Dict.TryGetValue(814, out string _phrase814))
                {
                    _phrase814 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"814\" Phrase=\"{0}\" />", _phrase814));
                if (!Dict.TryGetValue(815, out string _phrase815))
                {
                    _phrase815 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"815\" Phrase=\"{0}\" />", _phrase815));
                if (!Dict.TryGetValue(816, out string _phrase816))
                {
                    _phrase816 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"816\" Phrase=\"{0}\" />", _phrase816));
                if (!Dict.TryGetValue(817, out string _phrase817))
                {
                    _phrase817 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"817\" Phrase=\"{0}\" />", _phrase817));
                if (!Dict.TryGetValue(818, out string _phrase818))
                {
                    _phrase818 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"818\" Phrase=\"{0}\" />", _phrase818));
                if (!Dict.TryGetValue(819, out string _phrase819))
                {
                    _phrase819 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"819\" Phrase=\"{0}\" />", _phrase819));
                if (!Dict.TryGetValue(820, out string _phrase820))
                {
                    _phrase820 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"820\" Phrase=\"{0}\" />", _phrase820));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Prayer ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(821, out string _phrase821))
                {
                    _phrase821 = "You can only use {CommandPrivate}{Command126} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"821\" Phrase=\"{0}\" />", _phrase821));
                if (!Dict.TryGetValue(822, out string _phrase822))
                {
                    _phrase822 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"822\" Phrase=\"{0}\" />", _phrase822));
                if (!Dict.TryGetValue(823, out string _phrase823))
                {
                    _phrase823 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"823\" Phrase=\"{0}\" />", _phrase823));
                if (!Dict.TryGetValue(824, out string _phrase824))
                {
                    _phrase824 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"824\" Phrase=\"{0}\" />", _phrase824));
                if (!Dict.TryGetValue(825, out string _phrase825))
                {
                    _phrase825 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"825\" Phrase=\"{0}\" />", _phrase825));
                if (!Dict.TryGetValue(826, out string _phrase826))
                {
                    _phrase826 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"826\" Phrase=\"{0}\" />", _phrase826));
                if (!Dict.TryGetValue(827, out string _phrase827))
                {
                    _phrase827 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"827\" Phrase=\"{0}\" />", _phrase827));
                if (!Dict.TryGetValue(828, out string _phrase828))
                {
                    _phrase828 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"828\" Phrase=\"{0}\" />", _phrase828));
                if (!Dict.TryGetValue(829, out string _phrase829))
                {
                    _phrase829 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"829\" Phrase=\"{0}\" />", _phrase829));
                if (!Dict.TryGetValue(830, out string _phrase830))
                {
                    _phrase830 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"830\" Phrase=\"{0}\" />", _phrase830));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Scout_Player ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(831, out string _phrase831))
                {
                    _phrase831 = "You can only use {CommandPrivate}{Command129} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"831\" Phrase=\"{0}\" />", _phrase831));
                if (!Dict.TryGetValue(832, out string _phrase832))
                {
                    _phrase832 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"832\" Phrase=\"{0}\" />", _phrase832));
                if (!Dict.TryGetValue(833, out string _phrase833))
                {
                    _phrase833 = "You have scouted a trail to a hostile player in the area with in {Value} blocks";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"833\" Phrase=\"{0}\" />", _phrase833));
                if (!Dict.TryGetValue(834, out string _phrase834))
                {
                    _phrase834 = "No trails or tracks were detected to a nearby hostile player.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"834\" Phrase=\"{0}\" />", _phrase834));
                if (!Dict.TryGetValue(835, out string _phrase835))
                {
                    _phrase835 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"835\" Phrase=\"{0}\" />", _phrase835));
                if (!Dict.TryGetValue(836, out string _phrase836))
                {
                    _phrase836 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"836\" Phrase=\"{0}\" />", _phrase836));
                if (!Dict.TryGetValue(837, out string _phrase837))
                {
                    _phrase837 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"837\" Phrase=\"{0}\" />", _phrase837));
                if (!Dict.TryGetValue(838, out string _phrase838))
                {
                    _phrase838 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"838\" Phrase=\"{0}\" />", _phrase838));
                if (!Dict.TryGetValue(839, out string _phrase839))
                {
                    _phrase839 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"839\" Phrase=\"{0}\" />", _phrase839));
                if (!Dict.TryGetValue(840, out string _phrase840))
                {
                    _phrase840 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"840\" Phrase=\"{0}\" />", _phrase840));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Starting_Items ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(841, out string _phrase841))
                {
                    _phrase841 = "You have received the starting items. Check your inventory. If full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"841\" Phrase=\"{0}\" />", _phrase841));
                if (!Dict.TryGetValue(842, out string _phrase842))
                {
                    _phrase842 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"842\" Phrase=\"{0}\" />", _phrase842));
                if (!Dict.TryGetValue(843, out string _phrase843))
                {
                    _phrase843 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"843\" Phrase=\"{0}\" />", _phrase843));
                if (!Dict.TryGetValue(844, out string _phrase844))
                {
                    _phrase844 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"844\" Phrase=\"{0}\" />", _phrase844));
                if (!Dict.TryGetValue(845, out string _phrase845))
                {
                    _phrase845 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"845\" Phrase=\"{0}\" />", _phrase845));
                if (!Dict.TryGetValue(846, out string _phrase846))
                {
                    _phrase846 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"846\" Phrase=\"{0}\" />", _phrase846));
                if (!Dict.TryGetValue(847, out string _phrase847))
                {
                    _phrase847 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"847\" Phrase=\"{0}\" />", _phrase847));
                if (!Dict.TryGetValue(848, out string _phrase848))
                {
                    _phrase848 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"848\" Phrase=\"{0}\" />", _phrase848));
                if (!Dict.TryGetValue(849, out string _phrase849))
                {
                    _phrase849 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"849\" Phrase=\"{0}\" />", _phrase849));
                if (!Dict.TryGetValue(850, out string _phrase850))
                {
                    _phrase850 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"850\" Phrase=\"{0}\" />", _phrase850));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Teleport *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(851, out string _phrase851))
                {
                    _phrase851 = "You are too close to a hostile zombie or animal. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"851\" Phrase=\"{0}\" />", _phrase851));
                if (!Dict.TryGetValue(852, out string _phrase852))
                {
                    _phrase852 = "You are too close to a hostile player. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"852\" Phrase=\"{0}\" />", _phrase852));
                if (!Dict.TryGetValue(853, out string _phrase853))
                {
                    _phrase853 = "You can not teleport with a vehicle.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"853\" Phrase=\"{0}\" />", _phrase853));
                if (!Dict.TryGetValue(854, out string _phrase854))
                {
                    _phrase854 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"854\" Phrase=\"{0}\" />", _phrase854));
                if (!Dict.TryGetValue(855, out string _phrase855))
                {
                    _phrase855 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"855\" Phrase=\"{0}\" />", _phrase855));
                if (!Dict.TryGetValue(856, out string _phrase856))
                {
                    _phrase856 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"856\" Phrase=\"{0}\" />", _phrase856));
                if (!Dict.TryGetValue(857, out string _phrase857))
                {
                    _phrase857 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"857\" Phrase=\"{0}\" />", _phrase857));
                if (!Dict.TryGetValue(858, out string _phrase858))
                {
                    _phrase858 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"858\" Phrase=\"{0}\" />", _phrase858));
                if (!Dict.TryGetValue(859, out string _phrase859))
                {
                    _phrase859 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"859\" Phrase=\"{0}\" />", _phrase859));
                if (!Dict.TryGetValue(860, out string _phrase860))
                {
                    _phrase860 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"860\" Phrase=\"{0}\" />", _phrase860));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Wallet ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(861, out string _phrase861))
                {
                    _phrase861 = "Your wallet contains: {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"861\" Phrase=\"{0}\" />", _phrase861));
                if (!Dict.TryGetValue(862, out string _phrase862))
                {
                    _phrase862 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"862\" Phrase=\"{0}\" />", _phrase862));
                if (!Dict.TryGetValue(863, out string _phrase863))
                {
                    _phrase863 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"863\" Phrase=\"{0}\" />", _phrase863));
                if (!Dict.TryGetValue(864, out string _phrase864))
                {
                    _phrase864 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"864\" Phrase=\"{0}\" />", _phrase864));
                if (!Dict.TryGetValue(865, out string _phrase865))
                {
                    _phrase865 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"865\" Phrase=\"{0}\" />", _phrase865));
                if (!Dict.TryGetValue(866, out string _phrase866))
                {
                    _phrase866 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"866\" Phrase=\"{0}\" />", _phrase866));
                if (!Dict.TryGetValue(867, out string _phrase867))
                {
                    _phrase867 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"867\" Phrase=\"{0}\" />", _phrase867));
                if (!Dict.TryGetValue(868, out string _phrase868))
                {
                    _phrase868 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"868\" Phrase=\"{0}\" />", _phrase868));
                if (!Dict.TryGetValue(869, out string _phrase869))
                {
                    _phrase869 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"869\" Phrase=\"{0}\" />", _phrase869));
                if (!Dict.TryGetValue(870, out string _phrase870))
                {
                    _phrase870 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"870\" Phrase=\"{0}\" />", _phrase870));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Weather_Vote ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(871, out string _phrase871))
                {
                    _phrase871 = "A vote to change the weather has begun and will close in 60 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"871\" Phrase=\"{0}\" />", _phrase871));
                if (!Dict.TryGetValue(872, out string _phrase872))
                {
                    _phrase872 = "Weather vote complete but no votes were cast. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"872\" Phrase=\"{0}\" />", _phrase872));
                if (!Dict.TryGetValue(873, out string _phrase873))
                {
                    _phrase873 = "Weather vote complete. Most votes went to {Weather}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"873\" Phrase=\"{0}\" />", _phrase873));
                if (!Dict.TryGetValue(874, out string _phrase874))
                {
                    _phrase874 = "Weather vote was a tie. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"874\" Phrase=\"{0}\" />", _phrase874));
                if (!Dict.TryGetValue(875, out string _phrase875))
                {
                    _phrase875 = "Type {CommandPrivate}{Command63}, {CommandPrivate}{Command64} or {CommandPrivate}{Command65} to cast your vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"875\" Phrase=\"{0}\" />", _phrase875));
                if (!Dict.TryGetValue(876, out string _phrase876))
                {
                    _phrase876 = "Not enough players are online to start a weather vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"876\" Phrase=\"{0}\" />", _phrase876));
                if (!Dict.TryGetValue(877, out string _phrase877))
                {
                    _phrase877 = "Wait sixty minutes before starting a new vote to change the weather. Time remaining {Value}";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"877\" Phrase=\"{0}\" />", _phrase877));
                if (!Dict.TryGetValue(878, out string _phrase878))
                {
                    _phrase878 = "There is a vote already open.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"878\" Phrase=\"{0}\" />", _phrase878));
                if (!Dict.TryGetValue(879, out string _phrase879))
                {
                    _phrase879 = "Clear skies ahead.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"879\" Phrase=\"{0}\" />", _phrase879));
                if (!Dict.TryGetValue(880, out string _phrase880))
                {
                    _phrase880 = "Light rain has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"880\" Phrase=\"{0}\" />", _phrase880));
                if (!Dict.TryGetValue(881, out string _phrase881))
                {
                    _phrase881 = "A rain storm has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"881\" Phrase=\"{0}\" />", _phrase881));
                if (!Dict.TryGetValue(882, out string _phrase882))
                {
                    _phrase882 = "A heavy rain storm has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"882\" Phrase=\"{0}\" />", _phrase882));
                if (!Dict.TryGetValue(883, out string _phrase883))
                {
                    _phrase883 = "Light snow has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"883\" Phrase=\"{0}\" />", _phrase883));
                if (!Dict.TryGetValue(884, out string _phrase884))
                {
                    _phrase884 = "A snow storm has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"884\" Phrase=\"{0}\" />", _phrase884));
                if (!Dict.TryGetValue(885, out string _phrase885))
                {
                    _phrase885 = "A heavy snow storm has started.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"885\" Phrase=\"{0}\" />", _phrase885));
                if (!Dict.TryGetValue(886, out string _phrase886))
                {
                    _phrase886 = "Not enough votes were cast in the weather vote. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"886\" Phrase=\"{0}\" />", _phrase886));
                if (!Dict.TryGetValue(887, out string _phrase887))
                {
                    _phrase887 = "Vote cast for clear.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"887\" Phrase=\"{0}\" />", _phrase887));
                if (!Dict.TryGetValue(888, out string _phrase888))
                {
                    _phrase888 = "You have already voted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"888\" Phrase=\"{0}\" />", _phrase888));
                if (!Dict.TryGetValue(889, out string _phrase889))
                {
                    _phrase889 = "There is no active weather vote. Type {CommandPrivate}{Command62} to open a new vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"889\" Phrase=\"{0}\" />", _phrase889));
                if (!Dict.TryGetValue(890, out string _phrase890))
                {
                    _phrase890 = "Vote cast for rain.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"890\" Phrase=\"{0}\" />", _phrase890));
                if (!Dict.TryGetValue(891, out string _phrase891))
                {
                    _phrase891 = "Vote cast for snow.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"891\" Phrase=\"{0}\" />", _phrase891));
                if (!Dict.TryGetValue(892, out string _phrase892))
                {
                    _phrase892 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"892\" Phrase=\"{0}\" />", _phrase892));
                if (!Dict.TryGetValue(893, out string _phrase893))
                {
                    _phrase893 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"893\" Phrase=\"{0}\" />", _phrase893));
                if (!Dict.TryGetValue(894, out string _phrase894))
                {
                    _phrase894 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"894\" Phrase=\"{0}\" />", _phrase894));
                if (!Dict.TryGetValue(895, out string _phrase895))
                {
                    _phrase895 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"895\" Phrase=\"{0}\" />", _phrase895));
                if (!Dict.TryGetValue(896, out string _phrase896))
                {
                    _phrase896 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"896\" Phrase=\"{0}\" />", _phrase896));
                if (!Dict.TryGetValue(897, out string _phrase897))
                {
                    _phrase897 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"897\" Phrase=\"{0}\" />", _phrase897));
                if (!Dict.TryGetValue(898, out string _phrase898))
                {
                    _phrase898 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"898\" Phrase=\"{0}\" />", _phrase898));
                if (!Dict.TryGetValue(899, out string _phrase899))
                {
                    _phrase899 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"899\" Phrase=\"{0}\" />", _phrase899));
                if (!Dict.TryGetValue(900, out string _phrase900))
                {
                    _phrase900 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"900\" Phrase=\"{0}\" />", _phrase900));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Give_Item *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(901, out string _phrase901))
                {
                    _phrase901 = "{Value} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"901\" Phrase=\"{0}\" />", _phrase901));
                if (!Dict.TryGetValue(902, out string _phrase902))
                {
                    _phrase902 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"902\" Phrase=\"{0}\" />", _phrase902));
                if (!Dict.TryGetValue(903, out string _phrase903))
                {
                    _phrase903 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"903\" Phrase=\"{0}\" />", _phrase903));
                if (!Dict.TryGetValue(904, out string _phrase904))
                {
                    _phrase904 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"904\" Phrase=\"{0}\" />", _phrase904));
                if (!Dict.TryGetValue(905, out string _phrase905))
                {
                    _phrase905 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"905\" Phrase=\"{0}\" />", _phrase905));
                if (!Dict.TryGetValue(906, out string _phrase906))
                {
                    _phrase906 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"906\" Phrase=\"{0}\" />", _phrase906));
                if (!Dict.TryGetValue(907, out string _phrase907))
                {
                    _phrase907 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"907\" Phrase=\"{0}\" />", _phrase907));
                if (!Dict.TryGetValue(908, out string _phrase908))
                {
                    _phrase908 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"908\" Phrase=\"{0}\" />", _phrase908));
                if (!Dict.TryGetValue(909, out string _phrase909))
                {
                    _phrase909 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"909\" Phrase=\"{0}\" />", _phrase909));
                if (!Dict.TryGetValue(910, out string _phrase910))
                {
                    _phrase910 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"910\" Phrase=\"{0}\" />", _phrase910));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Mute_Vote ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(911, out string _phrase911))
                {
                    _phrase911 = "A vote to mute {PlayerName} in chat has begun and will close in 60 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"911\" Phrase=\"{0}\" />", _phrase911));
                if (!Dict.TryGetValue(912, out string _phrase912))
                {
                    _phrase912 = "Type {CommandPrivate}{Command70} to cast your vote.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"912\" Phrase=\"{0}\" />", _phrase912));
                if (!Dict.TryGetValue(913, out string _phrase913))
                {
                    _phrase913 = "{PlayerName} has been muted for 60 minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"913\" Phrase=\"{0}\" />", _phrase913));
                if (!Dict.TryGetValue(914, out string _phrase914))
                {
                    _phrase914 = "Type {CommandPrivate}{Command67} # to start a vote to mute that player from chat.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"914\" Phrase=\"{0}\" />", _phrase914));
                if (!Dict.TryGetValue(915, out string _phrase915))
                {
                    _phrase915 = "This player is already muted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"915\" Phrase=\"{0}\" />", _phrase915));
                if (!Dict.TryGetValue(916, out string _phrase916))
                {
                    _phrase916 = "Player id was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"916\" Phrase=\"{0}\" />", _phrase916));
                if (!Dict.TryGetValue(917, out string _phrase917))
                {
                    _phrase917 = "Not enough players are online to start a vote to mute.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"917\" Phrase=\"{0}\" />", _phrase917));
                if (!Dict.TryGetValue(918, out string _phrase918))
                {
                    _phrase918 = "Player = {PlayerName}, # = {Id}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"918\" Phrase=\"{0}\" />", _phrase918));
                if (!Dict.TryGetValue(919, out string _phrase919))
                {
                    _phrase919 = "No other users were found online.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"919\" Phrase=\"{0}\" />", _phrase919));
                if (!Dict.TryGetValue(920, out string _phrase920))
                {
                    _phrase920 = "There is a vote already open.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"920\" Phrase=\"{0}\" />", _phrase920));
                if (!Dict.TryGetValue(921, out string _phrase921))
                {
                    _phrase921 = "There are now {Value} of {VotesNeeded} votes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"921\" Phrase=\"{0}\" />", _phrase921));
                if (!Dict.TryGetValue(922, out string _phrase922))
                {
                    _phrase922 = "You have already voted.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"922\" Phrase=\"{0}\" />", _phrase922));
                if (!Dict.TryGetValue(923, out string _phrase923))
                {
                    _phrase923 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"923\" Phrase=\"{0}\" />", _phrase923));
                if (!Dict.TryGetValue(924, out string _phrase924))
                {
                    _phrase924 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"924\" Phrase=\"{0}\" />", _phrase924));
                if (!Dict.TryGetValue(925, out string _phrase925))
                {
                    _phrase925 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"925\" Phrase=\"{0}\" />", _phrase925));
                if (!Dict.TryGetValue(926, out string _phrase926))
                {
                    _phrase926 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"926\" Phrase=\"{0}\" />", _phrase926));
                if (!Dict.TryGetValue(927, out string _phrase927))
                {
                    _phrase927 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"927\" Phrase=\"{0}\" />", _phrase927));
                if (!Dict.TryGetValue(928, out string _phrase928))
                {
                    _phrase928 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"928\" Phrase=\"{0}\" />", _phrase928));
                if (!Dict.TryGetValue(929, out string _phrase929))
                {
                    _phrase929 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"929\" Phrase=\"{0}\" />", _phrase929));
                if (!Dict.TryGetValue(930, out string _phrase930))
                {
                    _phrase930 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"930\" Phrase=\"{0}\" />", _phrase930));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Chat_Color_Prefix ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(931, out string _phrase931))
                {
                    _phrase931 = "Your chat color prefix time has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"931\" Phrase=\"{0}\" />", _phrase931));
                if (!Dict.TryGetValue(932, out string _phrase932))
                {
                    _phrase932 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"932\" Phrase=\"{0}\" />", _phrase932));
                if (!Dict.TryGetValue(933, out string _phrase933))
                {
                    _phrase933 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"933\" Phrase=\"{0}\" />", _phrase933));
                if (!Dict.TryGetValue(934, out string _phrase934))
                {
                    _phrase934 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"934\" Phrase=\"{0}\" />", _phrase934));
                if (!Dict.TryGetValue(935, out string _phrase935))
                {
                    _phrase935 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"935\" Phrase=\"{0}\" />", _phrase935));
                if (!Dict.TryGetValue(936, out string _phrase936))
                {
                    _phrase936 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"936\" Phrase=\"{0}\" />", _phrase936));
                if (!Dict.TryGetValue(937, out string _phrase937))
                {
                    _phrase937 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"937\" Phrase=\"{0}\" />", _phrase937));
                if (!Dict.TryGetValue(938, out string _phrase938))
                {
                    _phrase938 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"938\" Phrase=\"{0}\" />", _phrase938));
                if (!Dict.TryGetValue(939, out string _phrase939))
                {
                    _phrase939 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"939\" Phrase=\"{0}\" />", _phrase939));
                if (!Dict.TryGetValue(940, out string _phrase940))
                {
                    _phrase940 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"940\" Phrase=\"{0}\" />", _phrase940));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Info_Ticker ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(941, out string _phrase941))
                {
                    _phrase941 = "You have turned off info ticker messages until the server restarts.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"941\" Phrase=\"{0}\" />", _phrase941));
                if (!Dict.TryGetValue(942, out string _phrase942))
                {
                    _phrase942 = "You have turned on info ticker messages.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"942\" Phrase=\"{0}\" />", _phrase942));
                if (!Dict.TryGetValue(943, out string _phrase943))
                {
                    _phrase943 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"943\" Phrase=\"{0}\" />", _phrase943));
                if (!Dict.TryGetValue(944, out string _phrase944))
                {
                    _phrase944 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"944\" Phrase=\"{0}\" />", _phrase944));
                if (!Dict.TryGetValue(945, out string _phrase945))
                {
                    _phrase945 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"945\" Phrase=\"{0}\" />", _phrase945));
                if (!Dict.TryGetValue(946, out string _phrase946))
                {
                    _phrase946 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"946\" Phrase=\"{0}\" />", _phrase946));
                if (!Dict.TryGetValue(947, out string _phrase947))
                {
                    _phrase947 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"947\" Phrase=\"{0}\" />", _phrase947));
                if (!Dict.TryGetValue(948, out string _phrase948))
                {
                    _phrase948 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"948\" Phrase=\"{0}\" />", _phrase948));
                if (!Dict.TryGetValue(949, out string _phrase949))
                {
                    _phrase949 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"949\" Phrase=\"{0}\" />", _phrase949));
                if (!Dict.TryGetValue(950, out string _phrase950))
                {
                    _phrase950 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"950\" Phrase=\"{0}\" />", _phrase950));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Damage_Detector ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(951, out string _phrase951))
                {
                    _phrase951 = "[FF0000]{PlayerName} has been banned for exceeding the damage limit.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"951\" Phrase=\"{0}\" />", _phrase951));
                if (!Dict.TryGetValue(952, out string _phrase952))
                {
                    _phrase952 = "Auto detection has banned you for exceeding the damage limit. Damage:";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"952\" Phrase=\"{0}\" />", _phrase952));
                if (!Dict.TryGetValue(953, out string _phrase953))
                {
                    _phrase953 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"953\" Phrase=\"{0}\" />", _phrase953));
                if (!Dict.TryGetValue(954, out string _phrase954))
                {
                    _phrase954 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"954\" Phrase=\"{0}\" />", _phrase954));
                if (!Dict.TryGetValue(955, out string _phrase955))
                {
                    _phrase955 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"955\" Phrase=\"{0}\" />", _phrase955));
                if (!Dict.TryGetValue(956, out string _phrase956))
                {
                    _phrase956 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"956\" Phrase=\"{0}\" />", _phrase956));
                if (!Dict.TryGetValue(957, out string _phrase957))
                {
                    _phrase957 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"957\" Phrase=\"{0}\" />", _phrase957));
                if (!Dict.TryGetValue(958, out string _phrase958))
                {
                    _phrase958 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"958\" Phrase=\"{0}\" />", _phrase958));
                if (!Dict.TryGetValue(959, out string _phrase959))
                {
                    _phrase959 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"959\" Phrase=\"{0}\" />", _phrase959));
                if (!Dict.TryGetValue(960, out string _phrase960))
                {
                    _phrase960 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"960\" Phrase=\"{0}\" />", _phrase960));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Spectator *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(961, out string _phrase961))
                {
                    _phrase961 = "[FF0000]{PlayerName} has been banned for spectator mode.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"961\" Phrase=\"{0}\" />", _phrase961));
                if (!Dict.TryGetValue(962, out string _phrase962))
                {
                    _phrase962 = "Auto detection has banned you for spectator mode.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"962\" Phrase=\"{0}\" />", _phrase962));
                if (!Dict.TryGetValue(963, out string _phrase963))
                {
                    _phrase963 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"963\" Phrase=\"{0}\" />", _phrase963));
                if (!Dict.TryGetValue(964, out string _phrase964))
                {
                    _phrase964 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"964\" Phrase=\"{0}\" />", _phrase964));
                if (!Dict.TryGetValue(965, out string _phrase965))
                {
                    _phrase965 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"965\" Phrase=\"{0}\" />", _phrase965));
                if (!Dict.TryGetValue(966, out string _phrase966))
                {
                    _phrase966 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"966\" Phrase=\"{0}\" />", _phrase966));
                if (!Dict.TryGetValue(967, out string _phrase967))
                {
                    _phrase967 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"967\" Phrase=\"{0}\" />", _phrase967));
                if (!Dict.TryGetValue(968, out string _phrase968))
                {
                    _phrase968 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"968\" Phrase=\"{0}\" />", _phrase968));
                if (!Dict.TryGetValue(969, out string _phrase969))
                {
                    _phrase969 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"969\" Phrase=\"{0}\" />", _phrase969));
                if (!Dict.TryGetValue(970, out string _phrase970))
                {
                    _phrase970 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"970\" Phrase=\"{0}\" />", _phrase970));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Godmode_Detector ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(971, out string _phrase971))
                {
                    _phrase971 = "[FF0000]{PlayerName} has been banned for godmode.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"971\" Phrase=\"{0}\" />", _phrase971));
                if (!Dict.TryGetValue(972, out string _phrase972))
                {
                    _phrase972 = "Auto detection has banned you for godmode.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"972\" Phrase=\"{0}\" />", _phrase972));
                if (!Dict.TryGetValue(973, out string _phrase973))
                {
                    _phrase973 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"973\" Phrase=\"{0}\" />", _phrase973));
                if (!Dict.TryGetValue(974, out string _phrase974))
                {
                    _phrase974 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"974\" Phrase=\"{0}\" />", _phrase974));
                if (!Dict.TryGetValue(975, out string _phrase975))
                {
                    _phrase975 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"975\" Phrase=\"{0}\" />", _phrase975));
                if (!Dict.TryGetValue(976, out string _phrase976))
                {
                    _phrase976 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"976\" Phrase=\"{0}\" />", _phrase976));
                if (!Dict.TryGetValue(977, out string _phrase977))
                {
                    _phrase977 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"977\" Phrase=\"{0}\" />", _phrase977));
                if (!Dict.TryGetValue(978, out string _phrase978))
                {
                    _phrase978 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"978\" Phrase=\"{0}\" />", _phrase978));
                if (!Dict.TryGetValue(979, out string _phrase979))
                {
                    _phrase979 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"979\" Phrase=\"{0}\" />", _phrase979));
                if (!Dict.TryGetValue(980, out string _phrase980))
                {
                    _phrase980 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"980\" Phrase=\"{0}\" />", _phrase980));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Flying_Detector ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(981, out string _phrase981))
                {
                    _phrase981 = "[FF0000]{PlayerName} has been banned for flying.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"981\" Phrase=\"{0}\" />", _phrase981));
                if (!Dict.TryGetValue(982, out string _phrase982))
                {
                    _phrase982 = "Auto detection has banned you for flying.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"982\" Phrase=\"{0}\" />", _phrase982));
                if (!Dict.TryGetValue(983, out string _phrase983))
                {
                    _phrase983 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"983\" Phrase=\"{0}\" />", _phrase983));
                if (!Dict.TryGetValue(984, out string _phrase984))
                {
                    _phrase984 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"984\" Phrase=\"{0}\" />", _phrase984));
                if (!Dict.TryGetValue(985, out string _phrase985))
                {
                    _phrase985 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"985\" Phrase=\"{0}\" />", _phrase985));
                if (!Dict.TryGetValue(986, out string _phrase986))
                {
                    _phrase986 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"986\" Phrase=\"{0}\" />", _phrase986));
                if (!Dict.TryGetValue(987, out string _phrase987))
                {
                    _phrase987 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"987\" Phrase=\"{0}\" />", _phrase987));
                if (!Dict.TryGetValue(988, out string _phrase988))
                {
                    _phrase988 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"988\" Phrase=\"{0}\" />", _phrase988));
                if (!Dict.TryGetValue(989, out string _phrase989))
                {
                    _phrase989 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"989\" Phrase=\"{0}\" />", _phrase989));
                if (!Dict.TryGetValue(990, out string _phrase990))
                {
                    _phrase990 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"990\" Phrase=\"{0}\" />", _phrase990));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Player_Stats ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(991, out string _phrase991))
                {
                    _phrase991 = "Auto detection has kicked you for illegal player stat health.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"991\" Phrase=\"{0}\" />", _phrase991));
                if (!Dict.TryGetValue(992, out string _phrase992))
                {
                    _phrase992 = "[FF0000]{PlayerName} was detected and kicked for illegal player stat health.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"992\" Phrase=\"{0}\" />", _phrase992));
                if (!Dict.TryGetValue(993, out string _phrase993))
                {
                    _phrase993 = "Auto detection has banned you for illegal player stat health.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"993\" Phrase=\"{0}\" />", _phrase993));
                if (!Dict.TryGetValue(994, out string _phrase994))
                {
                    _phrase994 = "[FF0000]{PlayerName} was detected and banned for illegal player stat health.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"994\" Phrase=\"{0}\" />", _phrase994));
                if (!Dict.TryGetValue(995, out string _phrase995))
                {
                    _phrase995 = "Auto detection has kicked you for illegal player stat stamina.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"995\" Phrase=\"{0}\" />", _phrase995));
                if (!Dict.TryGetValue(996, out string _phrase996))
                {
                    _phrase996 = "[FF0000]{PlayerName} was detected and kicked for illegal player stat stamina.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"996\" Phrase=\"{0}\" />", _phrase996));
                if (!Dict.TryGetValue(997, out string _phrase997))
                {
                    _phrase997 = "Auto detection has banned you for illegal player stat stamina.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"997\" Phrase=\"{0}\" />", _phrase997));
                if (!Dict.TryGetValue(998, out string _phrase998))
                {
                    _phrase998 = "[FF0000]{PlayerName} was detected and banned for illegal player stat stamina.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"998\" Phrase=\"{0}\" />", _phrase998));
                if (!Dict.TryGetValue(999, out string _phrase999))
                {
                    _phrase999 = "Auto detection has kicked you for illegal player stat jump strength.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"999\" Phrase=\"{0}\" />", _phrase999));
                if (!Dict.TryGetValue(1000, out string _phrase1000))
                {
                    _phrase1000 = "[FF0000]{PlayerName} was detected and kicked for illegal player stat jump strength.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1000\" Phrase=\"{0}\" />", _phrase1000));
                if (!Dict.TryGetValue(1001, out string _phrase1001))
                {
                    _phrase1001 = "Auto detection has banned you for illegal player stat jump strength.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1001\" Phrase=\"{0}\" />", _phrase1001));
                if (!Dict.TryGetValue(1002, out string _phrase1002))
                {
                    _phrase1002 = "[FF0000]{PlayerName} was detected and banned for illegal player stat jump strength.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1002\" Phrase=\"{0}\" />", _phrase1002));
                if (!Dict.TryGetValue(1003, out string _phrase1003))
                {
                    _phrase1003 = "Auto detection has kicked you for illegal player height.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1003\" Phrase=\"{0}\" />", _phrase1003));
                if (!Dict.TryGetValue(1004, out string _phrase1004))
                {
                    _phrase1004 = "[FF0000]{PlayerName} was detected and kicked for illegal player height.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1004\" Phrase=\"{0}\" />", _phrase1004));
                if (!Dict.TryGetValue(1005, out string _phrase1005))
                {
                    _phrase1005 = "Auto detection has banned you for illegal player height.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1005\" Phrase=\"{0}\" />", _phrase1005));
                if (!Dict.TryGetValue(1006, out string _phrase1006))
                {
                    _phrase1006 = "[FF0000]{PlayerName} was detected and banned for illegal player height.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1006\" Phrase=\"{0}\" />", _phrase1006));
                if (!Dict.TryGetValue(1007, out string _phrase1007))
                {
                    _phrase1007 = "Auto detection has kicked you for illegal player stat run speed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1007\" Phrase=\"{0}\" />", _phrase1007));
                if (!Dict.TryGetValue(1008, out string _phrase1008))
                {
                    _phrase1008 = "[FF0000]{PlayerName} was detected and kicked for illegal player stat run speed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1008\" Phrase=\"{0}\" />", _phrase1008));
                if (!Dict.TryGetValue(1009, out string _phrase1009))
                {
                    _phrase1009 = "Auto detection has banned you for illegal player stat run speed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1009\" Phrase=\"{0}\" />", _phrase1009));
                if (!Dict.TryGetValue(1010, out string _phrase1010))
                {
                    _phrase1010 = "[FF0000]{PlayerName} was detected and banned for illegal player stat run speed.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1010\" Phrase=\"{0}\" />", _phrase1010));
                if (!Dict.TryGetValue(1011, out string _phrase1011))
                {
                    _phrase1011 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1011\" Phrase=\"{0}\" />", _phrase1011));
                if (!Dict.TryGetValue(1012, out string _phrase1012))
                {
                    _phrase1012 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1012\" Phrase=\"{0}\" />", _phrase1012));
                if (!Dict.TryGetValue(1013, out string _phrase1013))
                {
                    _phrase1013 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1013\" Phrase=\"{0}\" />", _phrase1013));
                if (!Dict.TryGetValue(1014, out string _phrase1014))
                {
                    _phrase1014 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1014\" Phrase=\"{0}\" />", _phrase1014));
                if (!Dict.TryGetValue(1015, out string _phrase1015))
                {
                    _phrase1015 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1015\" Phrase=\"{0}\" />", _phrase1015));
                if (!Dict.TryGetValue(1016, out string _phrase1016))
                {
                    _phrase1016 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1016\" Phrase=\"{0}\" />", _phrase1016));
                if (!Dict.TryGetValue(1017, out string _phrase1017))
                {
                    _phrase1017 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1017\" Phrase=\"{0}\" />", _phrase1017));
                if (!Dict.TryGetValue(1018, out string _phrase1018))
                {
                    _phrase1018 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1018\" Phrase=\"{0}\" />", _phrase1018));
                if (!Dict.TryGetValue(1019, out string _phrase1019))
                {
                    _phrase1019 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1019\" Phrase=\"{0}\" />", _phrase1019));
                if (!Dict.TryGetValue(1020, out string _phrase1020))
                {
                    _phrase1020 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1020\" Phrase=\"{0}\" />", _phrase1020));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* World_Radius ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(1021, out string _phrase1021))
                {
                    _phrase1021 = "You have reached the world border.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1021\" Phrase=\"{0}\" />", _phrase1021));
                if (!Dict.TryGetValue(1022, out string _phrase1022))
                {
                    _phrase1022 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1022\" Phrase=\"{0}\" />", _phrase1022));
                if (!Dict.TryGetValue(1023, out string _phrase1023))
                {
                    _phrase1023 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1023\" Phrase=\"{0}\" />", _phrase1023));
                if (!Dict.TryGetValue(1024, out string _phrase1024))
                {
                    _phrase1024 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1024\" Phrase=\"{0}\" />", _phrase1024));
                if (!Dict.TryGetValue(1025, out string _phrase1025))
                {
                    _phrase1025 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1025\" Phrase=\"{0}\" />", _phrase1025));
                if (!Dict.TryGetValue(1026, out string _phrase1026))
                {
                    _phrase1026 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1026\" Phrase=\"{0}\" />", _phrase1026));
                if (!Dict.TryGetValue(1027, out string _phrase1027))
                {
                    _phrase1027 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1027\" Phrase=\"{0}\" />", _phrase1027));
                if (!Dict.TryGetValue(1028, out string _phrase1028))
                {
                    _phrase1028 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1028\" Phrase=\"{0}\" />", _phrase1028));
                if (!Dict.TryGetValue(1029, out string _phrase1029))
                {
                    _phrase1029 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1029\" Phrase=\"{0}\" />", _phrase1029));
                if (!Dict.TryGetValue(1030, out string _phrase1030))
                {
                    _phrase1030 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1030\" Phrase=\"{0}\" />", _phrase1030));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* POI_Protection ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(1031, out string _phrase1031))
                {
                    _phrase1031 = "You can not place a bed in a POI";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1031\" Phrase=\"{0}\" />", _phrase1031));
                if (!Dict.TryGetValue(1032, out string _phrase1032))
                {
                    _phrase1032 = "You can not place a claim in a POI";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1032\" Phrase=\"{0}\" />", _phrase1032));
                if (!Dict.TryGetValue(1033, out string _phrase1033))
                {
                    _phrase1033 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1033\" Phrase=\"{0}\" />", _phrase1033));
                if (!Dict.TryGetValue(1034, out string _phrase1034))
                {
                    _phrase1034 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1034\" Phrase=\"{0}\" />", _phrase1034));
                if (!Dict.TryGetValue(1035, out string _phrase1035))
                {
                    _phrase1035 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1035\" Phrase=\"{0}\" />", _phrase1035));
                if (!Dict.TryGetValue(1036, out string _phrase1036))
                {
                    _phrase1036 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1036\" Phrase=\"{0}\" />", _phrase1036));
                if (!Dict.TryGetValue(1037, out string _phrase1037))
                {
                    _phrase1037 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1037\" Phrase=\"{0}\" />", _phrase1037));
                if (!Dict.TryGetValue(1038, out string _phrase1038))
                {
                    _phrase1038 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1038\" Phrase=\"{0}\" />", _phrase1038));
                if (!Dict.TryGetValue(1039, out string _phrase1039))
                {
                    _phrase1039 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1039\" Phrase=\"{0}\" />", _phrase1039));
                if (!Dict.TryGetValue(1040, out string _phrase1040))
                {
                    _phrase1040 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1040\" Phrase=\"{0}\" />", _phrase1040));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Travel *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                if (!Dict.TryGetValue(1041, out string _phrase1041))
                {
                    _phrase1041 = "You have traveled to {Destination}.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1041\" Phrase=\"{0}\" />", _phrase1041));
                if (!Dict.TryGetValue(1042, out string _phrase1042))
                {
                    _phrase1042 = "You are not in a travel location.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1042\" Phrase=\"{0}\" />", _phrase1042));
                if (!Dict.TryGetValue(1043, out string _phrase1043))
                {
                    _phrase1043 = "You can only use {CommandPrivate}{Command49} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1043\" Phrase=\"{0}\" />", _phrase1043));
                if (!Dict.TryGetValue(1044, out string _phrase1044))
                {
                    _phrase1044 = "You do not have enough {CoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1044\" Phrase=\"{0}\" />", _phrase1044));
                if (!Dict.TryGetValue(1045, out string _phrase1045))
                {
                    _phrase1045 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1045\" Phrase=\"{0}\" />", _phrase1045));
                if (!Dict.TryGetValue(1046, out string _phrase1046))
                {
                    _phrase1046 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1046\" Phrase=\"{0}\" />", _phrase1046));
                if (!Dict.TryGetValue(1047, out string _phrase1047))
                {
                    _phrase1047 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1047\" Phrase=\"{0}\" />", _phrase1047));
                if (!Dict.TryGetValue(1048, out string _phrase1048))
                {
                    _phrase1048 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1048\" Phrase=\"{0}\" />", _phrase1048));


                if (!Dict.TryGetValue(1049, out string _phrase1049))
                {
                    _phrase1049 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1049\" Phrase=\"{0}\" />", _phrase1049));
                if (!Dict.TryGetValue(1050, out string _phrase1050))
                {
                    _phrase1050 = "";
                }
                sw.WriteLine(string.Format("        <Phrase Id=\"1050\" Phrase=\"{0}\" />", _phrase1050));

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