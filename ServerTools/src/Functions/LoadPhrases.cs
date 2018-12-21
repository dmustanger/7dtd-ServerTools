﻿using System.Collections.Generic;
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of missing 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Phrase"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of missing 'Phrase' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _phrase = _line.GetAttribute("Phrase");
                        int _id;
                        if (!int.TryParse(_line.GetAttribute("id"), out _id))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because of invalid (non-numeric) value for 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_id) && !Dict.ContainsValue(_phrase))
                        {
                            Dict.Add(_id, _phrase);
                        }
                        else if (Dict.ContainsKey(_id) && !Dict.ContainsValue(_phrase))
                        {
                            string _value;
                            Dict.TryGetValue(_id, out _value);
                            Log.Warning(string.Format("[SERVERTOOLS] Replaced Phrases entry {0}: {1}. New Phrase is {2}", _id, _value, _phrase));
                            Dict.Remove(_id);
                            Dict.Add(_id, _phrase);
                        }
                        else
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Phrases entry because it already exists in the list. id {0}: phrase: {1}", _id, _phrase));
                            continue;
                        }
                    }
                }
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
                sw.WriteLine(string.Format("        <!-- *********************** V.{0} ************************* -->", LoadConfig.version));
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
                    _phrase2 = "Auto Kicked: Ping too high. ({PlayerPing}) Max ping is {MaxPing}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"2\" Phrase=\"{0}\" />", _phrase2));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ***************** Invalid_Item_Phrases ***************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase3;
                if (!Dict.TryGetValue(3, out _phrase3))
                {
                    _phrase3 = " you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
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
                    _phrase6 = " you can only use /gimme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"6\" Phrase=\"{0}\" />", _phrase6));
                string _phrase7;
                if (!Dict.TryGetValue(7, out _phrase7))
                {
                    _phrase7 = " received {ItemCount} {ItemName} from gimme.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"7\" Phrase=\"{0}\" />", _phrase7));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Killme ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase8;
                if (!Dict.TryGetValue(8, out _phrase8))
                {
                    _phrase8 = " you can only use /killme, /wrist, /hang, or /suicide once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"8\" Phrase=\"{0}\" />", _phrase8));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Set_Home ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase9;
                if (!Dict.TryGetValue(9, out _phrase9))
                {
                    _phrase9 = " you already have a home set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"9\" Phrase=\"{0}\" />", _phrase9));
                string _phrase10;
                if (!Dict.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = " your home has been saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"10\" Phrase=\"{0}\" />", _phrase10));
                string _phrase11;
                if (!Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = " you do not have a home saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"11\" Phrase=\"{0}\" />", _phrase11));
                string _phrase12;
                if (!Dict.TryGetValue(12, out _phrase12))
                {
                    _phrase12 = " your home has been removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"12\" Phrase=\"{0}\" />", _phrase12));
                string _phrase13;
                if (!Dict.TryGetValue(13, out _phrase13))
                {
                    _phrase13 = " you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"13\" Phrase=\"{0}\" />", _phrase13));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Whisper ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase14;
                if (!Dict.TryGetValue(14, out _phrase14))
                {
                    _phrase14 = " player {TargetName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"14\" Phrase=\"{0}\" />", _phrase14));
                string _phrase15;
                if (!Dict.TryGetValue(15, out _phrase15))
                {
                    _phrase15 = " no one has pm'd you.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"15\" Phrase=\"{0}\" />", _phrase15));
                string _phrase16;
                if (!Dict.TryGetValue(16, out _phrase16))
                {
                    _phrase16 = " the player is not online.";
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
                    _phrase22 = "Sorry {PlayerName} you have reached the max session time. Please wait {TimeRemaining} minutes before rejoining.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"22\" Phrase=\"{0}\" />", _phrase22));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Clans ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase101;
                if (!Dict.TryGetValue(101, out _phrase101))
                {
                    _phrase101 = " you have already created the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"101\" Phrase=\"{0}\" />", _phrase101));
                string _phrase102;
                if (!Dict.TryGetValue(102, out _phrase102))
                {
                    _phrase102 = " can not add the clan {ClanName} because it already exist.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"102\" Phrase=\"{0}\" />", _phrase102));
                string _phrase103;
                if (!Dict.TryGetValue(103, out _phrase103))
                {
                    _phrase103 = " you are currently a member of the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"103\" Phrase=\"{0}\" />", _phrase103));
                string _phrase104;
                if (!Dict.TryGetValue(104, out _phrase104))
                {
                    _phrase104 = " you have added the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"104\" Phrase=\"{0}\" />", _phrase104));
                string _phrase105;
                if (!Dict.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = " you are not the owner of any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"105\" Phrase=\"{0}\" />", _phrase105));
                string _phrase106;
                if (!Dict.TryGetValue(106, out _phrase106))
                {
                    _phrase106 = " you have removed the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"106\" Phrase=\"{0}\" />", _phrase106));
                string _phrase107;
                if (!Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"107\" Phrase=\"{0}\" />", _phrase107));
                string _phrase108;
                if (!Dict.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = " the name {TargetPlayerName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"108\" Phrase=\"{0}\" />", _phrase108));
                string _phrase109;
                if (!Dict.TryGetValue(109, out _phrase109))
                {
                    _phrase109 = " is already a member of a clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"109\" Phrase=\"{0}\" />", _phrase109));
                string _phrase110;
                if (!Dict.TryGetValue(110, out _phrase110))
                {
                    _phrase110 = " already has pending clan invites.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"110\" Phrase=\"{0}\" />", _phrase110));
                string _phrase111;
                if (!Dict.TryGetValue(111, out _phrase111))
                {
                    _phrase111 = " you have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"111\" Phrase=\"{0}\" />", _phrase111));
                string _phrase112;
                if (!Dict.TryGetValue(112, out _phrase112))
                {
                    _phrase112 = " you have invited {InvitedPlayerName} to the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"112\" Phrase=\"{0}\" />", _phrase112));
                string _phrase113;
                if (!Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = " you have not been invited to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"113\" Phrase=\"{0}\" />", _phrase113));
                string _phrase114;
                if (!Dict.TryGetValue(114, out _phrase114))
                {
                    _phrase114 = " the clan could not be found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"114\" Phrase=\"{0}\" />", _phrase114));
                string _phrase115;
                if (!Dict.TryGetValue(115, out _phrase115))
                {
                    _phrase115 = " has joined the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"115\" Phrase=\"{0}\" />", _phrase115));
                string _phrase116;
                if (!Dict.TryGetValue(116, out _phrase116))
                {
                    _phrase116 = " you have declined the invite to the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"116\" Phrase=\"{0}\" />", _phrase116));
                string _phrase117;
                if (!Dict.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = " is not a member of your clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"117\" Phrase=\"{0}\" />", _phrase117));
                string _phrase118;
                if (!Dict.TryGetValue(118, out _phrase118))
                {
                    _phrase118 = " only the clan owner can remove officers.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"118\" Phrase=\"{0}\" />", _phrase118));
                string _phrase119;
                if (!Dict.TryGetValue(119, out _phrase119))
                {
                    _phrase119 = " clan owners can not be removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"119\" Phrase=\"{0}\" />", _phrase119));
                string _phrase120;
                if (!Dict.TryGetValue(120, out _phrase120))
                {
                    _phrase120 = " you have removed {PlayertoRemove} from clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"120\" Phrase=\"{0}\" />", _phrase120));
                string _phrase121;
                if (!Dict.TryGetValue(121, out _phrase121))
                {
                    _phrase121 = " you have been removed from the clan {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"121\" Phrase=\"{0}\" />", _phrase121));
                string _phrase122;
                if (!Dict.TryGetValue(122, out _phrase122))
                {
                    _phrase122 = " is already a officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"122\" Phrase=\"{0}\" />", _phrase122));
                string _phrase123;
                if (!Dict.TryGetValue(123, out _phrase123))
                {
                    _phrase123 = " has been promoted to an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"123\" Phrase=\"{0}\" />", _phrase123));
                string _phrase124;
                if (!Dict.TryGetValue(124, out _phrase124))
                {
                    _phrase124 = " is not an officer.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"124\" Phrase=\"{0}\" />", _phrase124));
                string _phrase125;
                if (!Dict.TryGetValue(125, out _phrase125))
                {
                    _phrase125 = " has been demoted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"125\" Phrase=\"{0}\" />", _phrase125));
                string _phrase126;
                if (!Dict.TryGetValue(126, out _phrase126))
                {
                    _phrase126 = " you can not leave the clan because you are the owner. You can only delete the clan.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"126\" Phrase=\"{0}\" />", _phrase126));
                string _phrase127;
                if (!Dict.TryGetValue(127, out _phrase127))
                {
                    _phrase127 = " you do not belong to any clans.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"127\" Phrase=\"{0}\" />", _phrase127));
                string _phrase128;
                if (!Dict.TryGetValue(128, out _phrase128))
                {
                    _phrase128 = " the clan {ClanName} was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"128\" Phrase=\"{0}\" />", _phrase128));
                string _phrase129;
                if (!Dict.TryGetValue(129, out _phrase129))
                {
                    _phrase129 = " the clanName must be longer than 2 characters.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"129\" Phrase=\"{0}\" />", _phrase129));
                string _phrase130;
                if (!Dict.TryGetValue(130, out _phrase130))
                {
                    _phrase130 = " you have changed your clan name to {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"130\" Phrase=\"{0}\" />", _phrase130));
                string _phrase131;
                if (!Dict.TryGetValue(131, out _phrase131))
                {
                    _phrase131 = " your clan name has been changed by the owner to {ClanName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"131\" Phrase=\"{0}\" />", _phrase131));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ****************** Admins_Chat_Commands **************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase201;
                if (!Dict.TryGetValue(201, out _phrase201))
                {
                    _phrase201 = " player {PlayerName} was not found online.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"201\" Phrase=\"{0}\" />", _phrase201));
                string _phrase202;
                if (!Dict.TryGetValue(202, out _phrase202))
                {
                    _phrase202 = " player {PlayerName} is already muted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"202\" Phrase=\"{0}\" />", _phrase202));
                string _phrase203;
                if (!Dict.TryGetValue(203, out _phrase203))
                {
                    _phrase203 = " you have muted {PlayerName} for 60 minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"203\" Phrase=\"{0}\" />", _phrase203));
                string _phrase204;
                if (!Dict.TryGetValue(204, out _phrase204))
                {
                    _phrase204 = " player {PlayerName} is not muted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"204\" Phrase=\"{0}\" />", _phrase204));
                string _phrase205;
                if (!Dict.TryGetValue(205, out _phrase205))
                {
                    _phrase205 = " you have unmuted {PlayerName}.";
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
                    _phrase303 = "Bicycles:{Bicycles} Minibikes:{Minibikes} Motorcycles:{Motorcycles} 4x4:{4x4} Gyros:{Gyros}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"303\" Phrase=\"{0}\" />", _phrase303));
                string _phrase304;
                if (!Dict.TryGetValue(304, out _phrase304))
                {
                    _phrase304 = "Total Supply Crates:{SupplyCrates}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"304\" Phrase=\"{0}\" />", _phrase304));
                string _phrase305;
                if (!Dict.TryGetValue(305, out _phrase305))
                {
                    _phrase305 = "";
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
                    _phrase500 = " you have been sent to jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"500\" Phrase=\"{0}\" />", _phrase500));
                string _phrase501;
                if (!Dict.TryGetValue(501, out _phrase501))
                {
                    _phrase501 = " you have been released from jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"501\" Phrase=\"{0}\" />", _phrase501));
                string _phrase502;
                if (!Dict.TryGetValue(502, out _phrase502))
                {
                    _phrase502 = " you have set the jail position as {JailPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"502\" Phrase=\"{0}\" />", _phrase502));
                string _phrase503;
                if (!Dict.TryGetValue(503, out _phrase503))
                {
                    _phrase503 = " the jail position has not been set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"503\" Phrase=\"{0}\" />", _phrase503));
                string _phrase504;
                if (!Dict.TryGetValue(504, out _phrase504))
                {
                    _phrase504 = " player {PlayerName} is already in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"504\" Phrase=\"{0}\" />", _phrase504));
                string _phrase505;
                if (!Dict.TryGetValue(505, out _phrase505))
                {
                    _phrase505 = " you have put {PlayerName} in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"505\" Phrase=\"{0}\" />", _phrase505));
                string _phrase506;
                if (!Dict.TryGetValue(506, out _phrase506))
                {
                    _phrase506 = " player {PlayerName} is not in jail.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"506\" Phrase=\"{0}\" />", _phrase506));
                string _phrase507;
                if (!Dict.TryGetValue(507, out _phrase507))
                {
                    _phrase507 = " the jail is electrified. Do not try to leave it.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"507\" Phrase=\"{0}\" />", _phrase507));
                string _phrase508;
                if (!Dict.TryGetValue(508, out _phrase508))
                {
                    _phrase508 = " do not pee on the electric fence.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"508\" Phrase=\"{0}\" />", _phrase508));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** New_Spawn_Tele ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase525;
                if (!Dict.TryGetValue(525, out _phrase525))
                {
                    _phrase525 = " you have set the New Spawn position as {NewSpawnTelePosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"525\" Phrase=\"{0}\" />", _phrase525));
                string _phrase526;
                if (!Dict.TryGetValue(526, out _phrase526))
                {
                    _phrase526 = " you have been teleported to the new spawn location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"526\" Phrase=\"{0}\" />", _phrase526));
                string _phrase527;
                if (!Dict.TryGetValue(527, out _phrase527))
                {
                    _phrase527 = " type /ready when you are prepared to leave. You will teleport back to your spawn location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"527\" Phrase=\"{0}\" />", _phrase527));
                string _phrase528;
                if (!Dict.TryGetValue(528, out _phrase528))
                {
                    _phrase528 = " you have no saved return point or you have used it.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"528\" Phrase=\"{0}\" />", _phrase528));
                string _phrase529;
                if (!Dict.TryGetValue(529, out _phrase529))
                {
                    _phrase529 = " you have left the new player area. Return to it before using /ready.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"529\" Phrase=\"{0}\" />", _phrase529));
                string _phrase530;
                if (!Dict.TryGetValue(530, out _phrase530))
                {
                    _phrase530 = " you have been sent back to your original spawn location. Good luck.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"530\" Phrase=\"{0}\" />", _phrase530));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Lottery ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase535;
                if (!Dict.TryGetValue(535, out _phrase535))
                {
                    _phrase535 = " there is no open lottery. Type /lottery # to open a new lottery at that buy in price. You must have enough in your wallet.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"535\" Phrase=\"{0}\" />", _phrase535));
                string _phrase536;
                if (!Dict.TryGetValue(536, out _phrase536))
                {
                    _phrase536 = " a lottery is open for {Value} {CoinName}. Minimum buy in is {BuyIn}. Enter it by typing /lottery enter.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"536\" Phrase=\"{0}\" />", _phrase536));
                string _phrase537;
                if (!Dict.TryGetValue(537, out _phrase537))
                {
                    _phrase537 = " you must type a valid integer above zero for the lottery #.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"537\" Phrase=\"{0}\" />", _phrase537));
                string _phrase538;
                if (!Dict.TryGetValue(538, out _phrase538))
                {
                    _phrase538 = " you have opened a new lottery for {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"538\" Phrase=\"{0}\" />", _phrase538));
                string _phrase539;
                if (!Dict.TryGetValue(539, out _phrase539))
                {
                    _phrase539 = "A lottery has opened for {Value} {CoinName} and will draw soon. Type /lottery enter to join.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"539\" Phrase=\"{0}\" />", _phrase539));
                string _phrase540;
                if (!Dict.TryGetValue(540, out _phrase540))
                {
                    _phrase540 = " you do not have enough {CoinName}. Earn some more and enter the lottery before it ends.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"540\" Phrase=\"{0}\" />", _phrase540));
                string _phrase541;
                if (!Dict.TryGetValue(541, out _phrase541))
                {
                    _phrase541 = " you have entered the lottery, good luck in the draw.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"541\" Phrase=\"{0}\" />", _phrase541));
                string _phrase542;
                if (!Dict.TryGetValue(542, out _phrase542))
                {
                    _phrase542 = " you are already in the lottery, good luck in the draw.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"542\" Phrase=\"{0}\" />", _phrase542));
                string _phrase543;
                if (!Dict.TryGetValue(543, out _phrase543))
                {
                    _phrase543 = "A lottery draw will begin in five minutes. Get your entries in before it starts. Type /lottery enter.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"543\" Phrase=\"{0}\" />", _phrase543));
                string _phrase544;
                if (!Dict.TryGetValue(544, out _phrase544))
                {
                    _phrase544 = "Winner! {PlayerName} has won the lottery and received {Value} {CoinName}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"544\" Phrase=\"{0}\" />", _phrase544));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Lobby ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase550;
                if (!Dict.TryGetValue(550, out _phrase550))
                {
                    _phrase550 = " you can only use /lobby once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"550\" Phrase=\"{0}\" />", _phrase550));
                string _phrase551;
                if (!Dict.TryGetValue(551, out _phrase551))
                {
                    _phrase551 = " you have set the lobby position as {LobbyPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"551\" Phrase=\"{0}\" />", _phrase551));
                string _phrase552;
                if (!Dict.TryGetValue(552, out _phrase552))
                {
                    _phrase552 = " you can go back by typing /lobbyback when you are ready to leave the lobby.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"552\" Phrase=\"{0}\" />", _phrase552));
                string _phrase553;
                if (!Dict.TryGetValue(553, out _phrase553))
                {
                    _phrase553 = " you have been sent to the lobby.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"553\" Phrase=\"{0}\" />", _phrase553));
                string _phrase554;
                if (!Dict.TryGetValue(554, out _phrase554))
                {
                    _phrase554 = " the lobby position is not set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"554\" Phrase=\"{0}\" />", _phrase554));
                string _phrase555;
                if (!Dict.TryGetValue(555, out _phrase555))
                {
                    _phrase555 = " sent you back to your saved location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"555\" Phrase=\"{0}\" />", _phrase555));
                string _phrase556;
                if (!Dict.TryGetValue(556, out _phrase556))
                {
                    _phrase556 = " you are outside the lobby. Get inside it and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"556\" Phrase=\"{0}\" />", _phrase556));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Market ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase560;
                if (!Dict.TryGetValue(560, out _phrase560))
                {
                    _phrase560 = " you can only use /market once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"560\" Phrase=\"{0}\" />", _phrase560));
                string _phrase561;
                if (!Dict.TryGetValue(561, out _phrase561))
                {
                    _phrase561 = " you can go back by typing /marketback when you are ready to leave the market.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"561\" Phrase=\"{0}\" />", _phrase561));
                string _phrase562;
                if (!Dict.TryGetValue(562, out _phrase562))
                {
                    _phrase562 = " sent you to the market.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"562\" Phrase=\"{0}\" />", _phrase562));
                string _phrase563;
                if (!Dict.TryGetValue(563, out _phrase563))
                {
                    _phrase563 = " the market position is not set.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"563\" Phrase=\"{0}\" />", _phrase563));
                string _phrase564;
                if (!Dict.TryGetValue(564, out _phrase564))
                {
                    _phrase564 = " you are outside the market. Get inside it and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"564\" Phrase=\"{0}\" />", _phrase564));
                string _phrase565;
                if (!Dict.TryGetValue(565, out _phrase565))
                {
                    _phrase565 = " you have set the market position as {MarketPosition}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"565\" Phrase=\"{0}\" />", _phrase565));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Session ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase570;
                if (!Dict.TryGetValue(570, out _phrase570))
                {
                    _phrase570 = " your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"570\" Phrase=\"{0}\" />", _phrase570));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Waypoints *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase575;
                if (!Dict.TryGetValue(575, out _phrase575))
                {
                    _phrase575 = " you can only use waypoints once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"575\" Phrase=\"{0}\" />", _phrase575));
                string _phrase576;
                if (!Dict.TryGetValue(576, out _phrase576))
                {
                    _phrase576 = " you can only use a waypoint that is outside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"576\" Phrase=\"{0}\" />", _phrase576));
                string _phrase577;
                if (!Dict.TryGetValue(577, out _phrase577))
                {
                    _phrase577 = "Traveling to waypoint {Waypoint}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"577\" Phrase=\"{0}\" />", _phrase577));
                string _phrase578;
                if (!Dict.TryGetValue(578, out _phrase578))
                {
                    _phrase578 = " you have not saved this waypoint.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"578\" Phrase=\"{0}\" />", _phrase578));
                string _phrase579;
                if (!Dict.TryGetValue(579, out _phrase579))
                {
                    _phrase579 = " you have a maximum {Count} waypoints.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"579\" Phrase=\"{0}\" />", _phrase579));
                string _phrase580;
                if (!Dict.TryGetValue(580, out _phrase580))
                {
                    _phrase580 = " this is not a valid waypoint.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"580\" Phrase=\"{0}\" />", _phrase580));
                string _phrase581;
                if (!Dict.TryGetValue(581, out _phrase581))
                {
                    _phrase581 = " you have a maximum {DonatorCount} waypoints.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"581\" Phrase=\"{0}\" />", _phrase581));
                string _phrase582;
                if (!Dict.TryGetValue(582, out _phrase582))
                {
                    _phrase582 = " you have a maximum {NormalCount} waypoints.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"582\" Phrase=\"{0}\" />", _phrase582));
                string _phrase583;
                if (!Dict.TryGetValue(583, out _phrase583))
                {
                    _phrase583 = " waypoint {Waypoint} has been deleted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"583\" Phrase=\"{0}\" />", _phrase583));
                string _phrase584;
                if (!Dict.TryGetValue(584, out _phrase584))
                {
                    _phrase584 = " waypoint name set to: {Name}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"584\" Phrase=\"{0}\" />", _phrase584));
                string _phrase585;
                if (!Dict.TryGetValue(585, out _phrase585))
                {
                    _phrase585 = " you have no waypoints saved to list.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"585\" Phrase=\"{0}\" />", _phrase585));
                string _phrase586;
                if (!Dict.TryGetValue(586, out _phrase586))
                {
                    _phrase586 = " you can only save a waypoint that is outside of a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"586\" Phrase=\"{0}\" />", _phrase586));
                string _phrase587;
                if (!Dict.TryGetValue(587, out _phrase587))
                {
                    _phrase587 = " you can not teleport to a waypoint with a vehicle.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"587\" Phrase=\"{0}\" />", _phrase587));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase601;
                if (!Dict.TryGetValue(601, out _phrase601))
                {
                    _phrase601 = " you have taxed your tracking ability. Wait {TimeRemaining} minutes and try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"601\" Phrase=\"{0}\" />", _phrase601));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Vote_Reward ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase602;
                if (!Dict.TryGetValue(602, out _phrase602))
                {
                    _phrase602 = " you can only use /reward once every {DelayBetweenRewards} hours. Time remaining: {TimeRemaining} hour(s).";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"602\" Phrase=\"{0}\" />", _phrase602)); 
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Travel_Locations ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase603;
                if (!Dict.TryGetValue(603, out _phrase603))
                {
                    _phrase603 = " you have traveled to {Destination}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"603\" Phrase=\"{0}\" />", _phrase603));
                string _phrase604;
                if (!Dict.TryGetValue(604, out _phrase604))
                {
                    _phrase604 = " you are not in a travel location.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"604\" Phrase=\"{0}\" />", _phrase604));
                string _phrase605;
                if (!Dict.TryGetValue(605, out _phrase605))
                {
                    _phrase605 = " you can only use /travel once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"605\" Phrase=\"{0}\" />", _phrase605));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Zone_Protection ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase606;
                if (!Dict.TryGetValue(606, out _phrase606))
                {
                    _phrase606 = " you can only use /return for two minutes after respawn. Time has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"606\" Phrase=\"{0}\" />", _phrase606));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Set_Home2 ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase607;
                if (!Dict.TryGetValue(607, out _phrase607))
                {
                    _phrase607 = " your home2 has been saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"607\" Phrase=\"{0}\" />", _phrase607));
                string _phrase608;
                if (!Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = " you do not have a home2 saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"608\" Phrase=\"{0}\" />", _phrase608));
                string _phrase609;
                if (!Dict.TryGetValue(609, out _phrase609))
                {
                    _phrase609 = " your home2 has been removed.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"609\" Phrase=\"{0}\" />", _phrase609));
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
                    _phrase613 = "Weather vote complete. Most votes went to {Weather}.";
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
                    _phrase616 = " you can only use {Command} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"616\" Phrase=\"{0}\" />", _phrase616));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Coin_Market ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase617;
                if (!Dict.TryGetValue(617, out _phrase617))
                {
                    _phrase617 = "The shop categories are:";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"617\" Phrase=\"{0}\" />", _phrase617));
                string _phrase618;
                if (!Dict.TryGetValue(618, out _phrase618))
                {
                    _phrase618 = "Type /shop 'category' to view that list";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"618\" Phrase=\"{0}\" />", _phrase618));
                string _phrase619;
                if (!Dict.TryGetValue(619, out _phrase619))
                {
                    _phrase619 = " you are not inside a trader area. Find a trader and use this command again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"619\" Phrase=\"{0}\" />", _phrase619));
                string _phrase620;
                if (!Dict.TryGetValue(620, out _phrase620))
                {
                    _phrase620 = " the item or amount # you are trying to buy is not an integer. Please input /buy 1 2 for example.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"620\" Phrase=\"{0}\" />", _phrase620));
                string _phrase621;
                if (!Dict.TryGetValue(621, out _phrase621))
                {
                    _phrase621 = " you do not have enough {CoinName}. Your wallet balance is {WalletBalance}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"621\" Phrase=\"{0}\" />", _phrase621));
                string _phrase622;
                if (!Dict.TryGetValue(622, out _phrase622))
                {
                    _phrase622 = " there was no item # matching the shop. Check the shop category again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"622\" Phrase=\"{0}\" />", _phrase622));
                string _phrase623;
                if (!Dict.TryGetValue(623, out _phrase623))
                {
                    _phrase623 = " there was an error in the shop list. Unable to buy this item. Please alert an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"623\" Phrase=\"{0}\" />", _phrase623));
                string _phrase624;
                if (!Dict.TryGetValue(624, out _phrase624))
                {
                    _phrase624 = " the shop does not contain any items. Contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"624\" Phrase=\"{0}\" />", _phrase624));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Friend_Teleport ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase625;
                if (!Dict.TryGetValue(625, out _phrase625))
                {
                    _phrase625 = " your friend {FriendName} with Id # {EntityId} is online.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"625\" Phrase=\"{0}\" />", _phrase625));
                string _phrase626;
                if (!Dict.TryGetValue(626, out _phrase626))
                {
                    _phrase626 = " this {EntityId} is not valid. Only integers accepted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"626\" Phrase=\"{0}\" />", _phrase626));
                string _phrase627;
                if (!Dict.TryGetValue(627, out _phrase627))
                {
                    _phrase627 = " sent your friend {PlayerName} a teleport request.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"627\" Phrase=\"{0}\" />", _phrase627));
                string _phrase628;
                if (!Dict.TryGetValue(628, out _phrase628))
                {
                    _phrase628 = " would like to teleport to you. Type /accept in chat to accept the request.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"628\" Phrase=\"{0}\" />", _phrase628));
                string _phrase629;
                if (!Dict.TryGetValue(629, out _phrase629))
                {
                    _phrase629 = " did not find EntityId {EntityId}. No teleport request sent.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"629\" Phrase=\"{0}\" />", _phrase629));
                string _phrase630;
                if (!Dict.TryGetValue(630, out _phrase630))
                {
                    _phrase630 = " you can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"630\" Phrase=\"{0}\" />", _phrase630));
                string _phrase631;
                if (!Dict.TryGetValue(631, out _phrase631))
                {
                    _phrase631 = " your request was accepted. Teleporting you to your friend.";
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
                    _phrase701 = "Thank you for your vote {PlayerName}. You can vote and receive another reward in {VoteDelay} hours.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"701\" Phrase=\"{0}\" />", _phrase701));
                string _phrase702;
                if (!Dict.TryGetValue(702, out _phrase702))
                {
                    _phrase702 = "Unable to get a result from the website, {PlayerName}. Please try again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"702\" Phrase=\"{0}\" />", _phrase702));
                string _phrase703;
                if (!Dict.TryGetValue(703, out _phrase703))
                {
                    _phrase703 = " reward items were sent to your inventory. If it is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"703\" Phrase=\"{0}\" />", _phrase703));
                string _phrase704;
                if (!Dict.TryGetValue(704, out _phrase704))
                {
                    _phrase704 = " you have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"704\" Phrase=\"{0}\" />", _phrase704));
                string _phrase705;
                if (!Dict.TryGetValue(705, out _phrase705))
                {
                    _phrase705 = " you have voted {Votes} time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"705\" Phrase=\"{0}\" />", _phrase705));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Flight_Check ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase706;
                if (!Dict.TryGetValue(706, out _phrase706))
                {
                    _phrase706 = "Detected {PlayerName} flying @ {X} {Y} {Z}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"706\" Phrase=\"{0}\" />", _phrase706));
                string _phrase707;
                if (!Dict.TryGetValue(707, out _phrase707))
                {
                    _phrase707 = " has been detected flying.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"707\" Phrase=\"{0}\" />", _phrase707));
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
                    _phrase711 = " has been detected flying underground.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"711\" Phrase=\"{0}\" />", _phrase711));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Animal_Tracking ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase715;
                if (!Dict.TryGetValue(715, out _phrase715))
                {
                    _phrase715 = " tracked down an animal to within {Radius} metres.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"715\" Phrase=\"{0}\" />", _phrase715));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************** Hatch_Elevator ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase720;
                if (!Dict.TryGetValue(720, out _phrase720))
                {
                    _phrase720 = " you are stunned and have broken your leg while smashing yourself through hatches. Ouch!";
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
                    _phrase735 = " you can only use /died once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"735\" Phrase=\"{0}\" />", _phrase735));
                string _phrase736;
                if (!Dict.TryGetValue(736, out _phrase736))
                {
                    _phrase736 = " teleporting you to your last death position. You can use this again in {DelayBetweenUses} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"736\" Phrase=\"{0}\" />", _phrase736));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Restart_Vote ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase740;
                if (!Dict.TryGetValue(740, out _phrase740))
                {
                    _phrase740 = "A vote to restart the server has opened and will close in 30 seconds. Type /yes to cast your vote.";
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
                    _phrase743 = "Players voted yes but not enough votes were cast to restart.";
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
                    _phrase747 = " you started the last restart vote. Another player must initiate the next vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"747\" Phrase=\"{0}\" />", _phrase747));
                string _phrase748;
                if (!Dict.TryGetValue(748, out _phrase748))
                {
                    _phrase748 = "{Player} has requested a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"748\" Phrase=\"{0}\" />", _phrase748));
                string _phrase749;
                if (!Dict.TryGetValue(749, out _phrase749))
                {
                    _phrase749 = " a administrator is currently online. They have been alerted.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"749\" Phrase=\"{0}\" />", _phrase749));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Location *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase760;
                if (!Dict.TryGetValue(760, out _phrase760))
                {
                    _phrase760 = "Your current position is X {X}, Y {Y}, Z {Z}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"760\" Phrase=\"{0}\" />", _phrase760));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Real_World_Time ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase765;
                if (!Dict.TryGetValue(765, out _phrase765))
                {
                    _phrase765 = "The real world time is {Time} {TimeZone}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"765\" Phrase=\"{0}\" />", _phrase765));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************* Bag ************************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase770;
                if (!Dict.TryGetValue(770, out _phrase770))
                {
                    _phrase770 = " your dropped bag was sent to you.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"770\" Phrase=\"{0}\" />", _phrase770));
                string _phrase771;
                if (!Dict.TryGetValue(771, out _phrase771))
                {
                    _phrase771 = " you can only use /bag once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"771\" Phrase=\"{0}\" />", _phrase771));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Mute_Player ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase775;
                if (!Dict.TryGetValue(775, out _phrase775))
                {
                    _phrase775 = "A vote to mute {PlayerName} in chat has begun and will close in 60 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"775\" Phrase=\"{0}\" />", _phrase775));
                string _phrase776;
                if (!Dict.TryGetValue(776, out _phrase776))
                {
                    _phrase776 = "Type /yes to cast your vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"776\" Phrase=\"{0}\" />", _phrase776));
                string _phrase777;
                if (!Dict.TryGetValue(777, out _phrase777))
                {
                    _phrase777 = " has been muted for 60 minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"777\" Phrase=\"{0}\" />", _phrase777));
                string _phrase778;
                if (!Dict.TryGetValue(778, out _phrase778))
                {
                    _phrase778 = " type /mute # to start a vote to mute that player from chat.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"778\" Phrase=\"{0}\" />", _phrase778));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Vehicle Teleport ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase780;
                if (!Dict.TryGetValue(780, out _phrase780))
                {
                    _phrase780 = " you have not claimed this space or a friend. You can only save your vehicle inside a claimed space.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"780\" Phrase=\"{0}\" />", _phrase780));
                string _phrase781;
                if (!Dict.TryGetValue(781, out _phrase781))
                {
                    _phrase781 = " saved your current vehicle for retrieval.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"781\" Phrase=\"{0}\" />", _phrase781));
                string _phrase782;
                if (!Dict.TryGetValue(782, out _phrase782))
                {
                    _phrase782 = " found your vehicle and sent it to you.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"782\" Phrase=\"{0}\" />", _phrase782));
                string _phrase783;
                if (!Dict.TryGetValue(783, out _phrase783))
                {
                    _phrase783 = " you do not have this vehicle type saved.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"783\" Phrase=\"{0}\" />", _phrase783));
                string _phrase784;
                if (!Dict.TryGetValue(784, out _phrase784))
                {
                    _phrase784 = " could not find your vehicle near by.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"784\" Phrase=\"{0}\" />", _phrase784));
                string _phrase785;
                if (!Dict.TryGetValue(785, out _phrase785))
                {
                    _phrase785 = " found your vehicle but someone else is on it.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"785\" Phrase=\"{0}\" />", _phrase785));
                string _phrase786;
                if (!Dict.TryGetValue(786, out _phrase786))
                {
                    _phrase786 = " you can only use vehicle teleport once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"786\" Phrase=\"{0}\" />", _phrase786));
                string _phrase787;
                if (!Dict.TryGetValue(787, out _phrase787))
                {
                    _phrase787 = " you are on the wrong vehicle to save it with this command. You are using a {Vehicle}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"787\" Phrase=\"{0}\" />", _phrase787));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* World_Radius ********************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase790;
                if (!Dict.TryGetValue(790, out _phrase790))
                {
                    _phrase790 = " you have reached the world border.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"790\" Phrase=\"{0}\" />", _phrase790));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Report ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase795;
                if (!Dict.TryGetValue(795, out _phrase795))
                {
                    _phrase795 = " you can only use /report once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"795\" Phrase=\"{0}\" />", _phrase795));
                string _phrase796;
                if (!Dict.TryGetValue(796, out _phrase796))
                {
                    _phrase796 = "Report from {PlayerName}: {Message}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"796\" Phrase=\"{0}\" />", _phrase796));
                string _phrase797;
                if (!Dict.TryGetValue(797, out _phrase797))
                {
                    _phrase797 = " your report has been sent to online administrators and logged.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"797\" Phrase=\"{0}\" />", _phrase797));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Unsorted_Phrases ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase799;
                if (!Dict.TryGetValue(799, out _phrase799))
                {
                    _phrase799 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"799\" Phrase=\"{0}\" />", _phrase799));
                string _phrase800;
                if (!Dict.TryGetValue(800, out _phrase800))
                {
                    _phrase800 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"800\" Phrase=\"{0}\" />", _phrase800));
                string _phrase801;
                if (!Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{PlayerName} has murdered you while you were in a protected zone.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"801\" Phrase=\"{0}\" />", _phrase801));
                string _phrase802;
                if (!Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"802\" Phrase=\"{0}\" />", _phrase802));
                string _phrase804;
                if (!Dict.TryGetValue(804, out _phrase804))
                {
                    _phrase804 = "{Count} {ItemName} was sent to your inventory. If your bag is full, check the ground.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"804\" Phrase=\"{0}\" />", _phrase804));
                string _phrase805;
                if (!Dict.TryGetValue(805, out _phrase805))
                {
                    _phrase805 = "Not enough votes were cast in the weather vote. No changes were made.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"805\" Phrase=\"{0}\" />", _phrase805));
                string _phrase806;
                if (!Dict.TryGetValue(806, out _phrase806))
                {
                    _phrase806 = " you have received the starting items. Check your inventory. If full, check the ground.";
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
                    _phrase808 = " no spawn points were found near you. Move locations and try /reward again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"808\" Phrase=\"{0}\" />", _phrase808));
                string _phrase809;
                if (!Dict.TryGetValue(809, out _phrase809))
                {
                    _phrase809 = " you have been sent to jail for life.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"809\" Phrase=\"{0}\" />", _phrase809));
                string _phrase811;
                if (!Dict.TryGetValue(811, out _phrase811))
                {
                    _phrase811 = " you can only use /return for four minutes after respawn. Time has expired.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"811\" Phrase=\"{0}\" />", _phrase811));
                string _phrase812;
                if (!Dict.TryGetValue(812, out _phrase812))
                {
                    _phrase812 = " you have already voted on the poll.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"812\" Phrase=\"{0}\" />", _phrase812));
                string _phrase813;
                if (!Dict.TryGetValue(813, out _phrase813))
                {
                    _phrase813 = "Currently, the poll is yes {YesCount} / no {NoCount}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"813\" Phrase=\"{0}\" />", _phrase813));
                string _phrase814;
                if (!Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"814\" Phrase=\"{0}\" />", _phrase814));
                string _phrase815;
                if (!Dict.TryGetValue(815, out _phrase815))
                {
                    _phrase815 = " you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"815\" Phrase=\"{0}\" />", _phrase815));
                string _phrase816;
                if (!Dict.TryGetValue(816, out _phrase816))
                {
                    _phrase816 = " you must wait thirty minutes after the server starts before opening a restart vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"816\" Phrase=\"{0}\" />", _phrase816));
                string _phrase817;
                if (!Dict.TryGetValue(817, out _phrase817))
                {
                    _phrase817 = " you are not inside your own or a friend's claimed space. You can not save this as your home.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"817\" Phrase=\"{0}\" />", _phrase817));
                string _phrase818;
                if (!Dict.TryGetValue(818, out _phrase818))
                {
                    _phrase818 = " you are traveling home.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"818\" Phrase=\"{0}\" />", _phrase818));
                string _phrase819;
                if (!Dict.TryGetValue(819, out _phrase819))
                {
                    _phrase819 = " you are too close to a player that is not a friend. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"819\" Phrase=\"{0}\" />", _phrase819));
                string _phrase820;
                if (!Dict.TryGetValue(820, out _phrase820))
                {
                    _phrase820 = " you are too close to a zombie. Command unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"820\" Phrase=\"{0}\" />", _phrase820));
                string _phrase821;
                if (!Dict.TryGetValue(821, out _phrase821))
                {
                    _phrase821 = " you are not inside a market or trader area. Find one and use this command again.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"821\" Phrase=\"{0}\" />", _phrase821));
                string _phrase822;
                if (!Dict.TryGetValue(822, out _phrase822))
                {
                    _phrase822 = "This category is missing. Check /shop.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"822\" Phrase=\"{0}\" />", _phrase822));
                string _phrase823;
                if (!Dict.TryGetValue(823, out _phrase823))
                {
                    _phrase823 = "Type /buy # to purchase the shop item. You can add how many times you want to buy it. /buy # #";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"823\" Phrase=\"{0}\" />", _phrase823));
                string _phrase824;
                if (!Dict.TryGetValue(824, out _phrase824))
                {
                    _phrase824 = " there is a vote already open.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"824\" Phrase=\"{0}\" />", _phrase824));
                string _phrase825;
                if (!Dict.TryGetValue(825, out _phrase825))
                {
                    _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"825\" Phrase=\"{0}\" />", _phrase825));
                string _phrase826;
                if (!Dict.TryGetValue(826, out _phrase826))
                {
                    _phrase826 = " you can not teleport home with a vehicle.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"826\" Phrase=\"{0}\" />", _phrase826));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- *********************** Auction ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase900;
                if (!Dict.TryGetValue(900, out _phrase900))
                {
                    _phrase900 = " you can only use /auction sell {DelayBetweenUses} hours after a sale. Time remaining: {TimeRemaining} hours.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"900\" Phrase=\"{0}\" />", _phrase900));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Bounty ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase910;
                if (!Dict.TryGetValue(910, out _phrase910))
                {
                    _phrase910 = " type /bounty Id# Value or /bounty Id# for the minimum bounty against this player.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"910\" Phrase=\"{0}\" />", _phrase910));
                string _phrase911;
                if (!Dict.TryGetValue(911, out _phrase911))
                {
                    _phrase911 = "{PlayerName}, # {EntityId}. Current bounty: {CurrentBounty}. Minimum bounty is {Minimum} {CoinName}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"911\" Phrase=\"{0}\" />", _phrase911));
                string _phrase912;
                if (!Dict.TryGetValue(912, out _phrase912))
                {
                    _phrase912 = "{PlayerName} is a bounty hunter! {Victim} was snuffed out.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"912\" Phrase=\"{0}\" />", _phrase912));
                string _phrase913;
                if (!Dict.TryGetValue(913, out _phrase913))
                {
                    _phrase913 = "{PlayerName} is on a kill streak! Their bounty has increased.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"913\" Phrase=\"{0}\" />", _phrase913));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Kill_Notice ********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase915;
                if (!Dict.TryGetValue(915, out _phrase915))
                {
                    _phrase915 = "{PlayerName} has killed {Victim} with {Item}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"915\" Phrase=\"{0}\" />", _phrase915));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Stuck ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase920;
                if (!Dict.TryGetValue(920, out _phrase920))
                {
                    _phrase920 = " you can only use /stuck once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"920\" Phrase=\"{0}\" />", _phrase920));
                string _phrase921;
                if (!Dict.TryGetValue(921, out _phrase921))
                {
                    _phrase921 = " you are outside of your claimed space or a friends. Command is unavailable.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"921\" Phrase=\"{0}\" />", _phrase921));
                string _phrase922;
                if (!Dict.TryGetValue(922, out _phrase922))
                {
                    _phrase922 = " sending you to the world surface. If you are still stuck, contact an administrator.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"922\" Phrase=\"{0}\" />", _phrase922));
                string _phrase923;
                if (!Dict.TryGetValue(923, out _phrase923))
                {
                    _phrase923 = " you do not seem to be stuck.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"923\" Phrase=\"{0}\" />", _phrase923));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Poll ************************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase925;
                if (!Dict.TryGetValue(925, out _phrase925))
                {
                    _phrase925 = "Poll results: Yes {YesVote} / No {NoVote}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"925\" Phrase=\"{0}\" />", _phrase925));
                string _phrase926;
                if (!Dict.TryGetValue(926, out _phrase926))
                {
                    _phrase926 = "Poll: {Message}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"926\" Phrase=\"{0}\" />", _phrase926));
                string _phrase927;
                if (!Dict.TryGetValue(927, out _phrase927))
                {
                    _phrase927 = "Type /pollyes or /pollno to vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"927\" Phrase=\"{0}\" />", _phrase927));
                string _phrase928;
                if (!Dict.TryGetValue(928, out _phrase928))
                {
                    _phrase928 = " you have cast a vote for yes. Currently, the pole is yes {Yes} / no {No}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"928\" Phrase=\"{0}\" />", _phrase928));
                string _phrase929;
                if (!Dict.TryGetValue(929, out _phrase929))
                {
                    _phrase929 = " you have cast a vote for no. Currently, the pole is yes {Yes} / no {No}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"929\" Phrase=\"{0}\" />", _phrase929));

                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Night_Vote *********************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase930;
                if (!Dict.TryGetValue(930, out _phrase930))
                {
                    _phrase930 = " you can not start a vote during a bloodmoon.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"930\" Phrase=\"{0}\" />", _phrase930));
                string _phrase931;
                if (!Dict.TryGetValue(931, out _phrase931))
                {
                    _phrase931 = " you can not start a vote during the day.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"931\" Phrase=\"{0}\" />", _phrase931));
                string _phrase932;
                if (!Dict.TryGetValue(932, out _phrase932))
                {
                    _phrase932 = "A vote to skip the night has begun. You have 30 seconds to type /yes.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"932\" Phrase=\"{0}\" />", _phrase932));
                string _phrase933;
                if (!Dict.TryGetValue(933, out _phrase933))
                {
                    _phrase933 = " not enough players are online to start a weather vote.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"933\" Phrase=\"{0}\" />", _phrase933));
                string _phrase934;
                if (!Dict.TryGetValue(934, out _phrase934))
                {
                    _phrase934 = "Players voted yes to skip this night. Good morning.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"934\" Phrase=\"{0}\" />", _phrase934));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Night_Time_Alert ******************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase940;
                if (!Dict.TryGetValue(940, out _phrase940))
                {
                    _phrase940 = "{Time} hours until night time.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"940\" Phrase=\"{0}\" />", _phrase940));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************** Hardcore ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase945;
                if (!Dict.TryGetValue(945, out _phrase945))
                {
                    _phrase945 = "Hardcore Top Players";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"945\" Phrase=\"{0}\" />", _phrase945));
                string _phrase946;
                if (!Dict.TryGetValue(946, out _phrase946))
                {
                    _phrase946 = "Playtime 1 {Name1}, {Session1}. Playtime 2 {Name2}, {Session3}. Playtime 3 {Name3}, {Session3}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"946\" Phrase=\"{0}\" />", _phrase946));
                string _phrase947;
                if (!Dict.TryGetValue(947, out _phrase947))
                {
                    _phrase947 = "Score 1 {Name1}, {Score1}. Score 2 {Name2}, {Score2}. Score 3 {Name3}, {Score3}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"947\" Phrase=\"{0}\" />", _phrase947));
                string _phrase948;
                if (!Dict.TryGetValue(948, out _phrase948))
                {
                    _phrase948 = " your last hardcore stats: Name {LastName} Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Deaths {Deaths}, Score {Score}, Playtime {Playtime} Minutes";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"948\" Phrase=\"{0}\" />", _phrase948));
                string _phrase949;
                if (!Dict.TryGetValue(949, out _phrase949))
                {
                    _phrase949 = " hardcore mode is enabled! You have {Lives} lives remaining...";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"949\" Phrase=\"{0}\" />", _phrase949));
                string _phrase950;
                if (!Dict.TryGetValue(950, out _phrase950))
                {
                    _phrase950 = "Hardcore: Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Score {Score}, Lives remaining {Lives}...";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"950\" Phrase=\"{0}\" />", _phrase950));
                string _phrase951;
                if (!Dict.TryGetValue(951, out _phrase951))
                {
                    _phrase951 = "Hardcore Game Over: Player {PlayerName}, Zombie Kills {ZombieKills}, Player Kills {PlayerKills}, Deaths {Deaths}, Score {Score}, Playtime {Playtime}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"951\" Phrase=\"{0}\" />", _phrase951));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ********************* Kick_Vote ************************ -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase955;
                if (!Dict.TryGetValue(955, out _phrase955))
                {
                    _phrase955 = "A vote to kick {PlayerName} has begun and will close in 30 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"955\" Phrase=\"{0}\" />", _phrase955));
                string _phrase956;
                if (!Dict.TryGetValue(956, out _phrase956))
                {
                    _phrase956 = " this player id was not found.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"956\" Phrase=\"{0}\" />", _phrase956));
                string _phrase957;
                if (!Dict.TryGetValue(957, out _phrase957))
                {
                    _phrase957 = " not enough players are online to start a vote to kick.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"957\" Phrase=\"{0}\" />", _phrase957));
                string _phrase958;
                if (!Dict.TryGetValue(958, out _phrase958))
                {
                    _phrase958 = "PlayerName = {PlayerName}, # = {Id}.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"958\" Phrase=\"{0}\" />", _phrase958));
                string _phrase959;
                if (!Dict.TryGetValue(959, out _phrase959))
                {
                    _phrase959 = " type /kickvote # to start a vote to kick that player.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"959\" Phrase=\"{0}\" />", _phrase959));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ************************ Mogul ************************* -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase965;
                if (!Dict.TryGetValue(965, out _phrase965))
                {
                    _phrase965 = "Richest players:";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"965\" Phrase=\"{0}\" />", _phrase965));
                string _phrase966;
                if (!Dict.TryGetValue(966, out _phrase966))
                {
                    _phrase966 = "{Name1}, valued at {Top1}. {Name2}, valued at {Top2}. {Name3}, valued at {Top3}";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"966\" Phrase=\"{0}\" />", _phrase966));
                sw.WriteLine("        <!-- ******************************************************** -->");
                sw.WriteLine("        <!-- ******************* Chat Protection ******************** -->");
                sw.WriteLine("        <!-- ******************************************************** -->");
                string _phrase970;
                if (!Dict.TryGetValue(970, out _phrase970))
                {
                    _phrase970 = " you have sent too many messages in one minute. Wait 60 seconds.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"970\" Phrase=\"{0}\" />", _phrase970));
                string _phrase971;
                if (!Dict.TryGetValue(971, out _phrase971))
                {
                    _phrase971 = " message is too long.";
                }
                sw.WriteLine(string.Format("        <Phrase id=\"971\" Phrase=\"{0}\" />", _phrase971));
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