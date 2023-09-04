using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Phrases
    {
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();

        public static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);

        private const string file = "Phrases.xml";
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            FileWatcher.Dispose();
        }

        private static void LoadXml()
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
                    if (!line.HasAttributes)
                    {
                        continue;
                    }
                    if (line.HasAttribute("Name") && line.HasAttribute("Message"))
                    {
                        string name = line.GetAttribute("Name");
                        if (name == "")
                        {
                            continue;
                        }
                        string message = line.GetAttribute("Message");
                        if (!Dict.ContainsKey(name))
                        {
                            Dict.Add(name, message);
                        }
                    }
                }
            }
            else
            {
                UpgradeXml(childNodes);
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                string phrase = "";
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Phrases>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** High_Ping_Kicker ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("HighPing1", out phrase))
                    {
                        phrase = "Auto Kicked: {PlayerName} for high ping of {Value}. Maxping is {MaxPing}";
                    }
                    sw.WriteLine("    <Phrase Name=\"HighPing1\" Message=\"{0}\" />", phrase);
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Invalid_Items ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("InvalidItem1", out phrase))
                    {
                        phrase = "You have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InvalidItem2", out phrase))
                    {
                        phrase = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InvalidItem3", out phrase))
                    {
                        phrase = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InvalidItem4", out phrase))
                    {
                        phrase = "Automatic detection: Invalid item {ItemName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InvalidItem5", out phrase))
                    {
                        phrase = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InvalidItem6", out phrase))
                    {
                        phrase = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InvalidItem6\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Gimme ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Gimme1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_gimme} once every {DelayBetweenUses} minute. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gimme1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gimme2", out phrase))
                    {
                        phrase = "Received {ItemCount} {ItemName} from gimme";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gimme2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gimme3", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gimme3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gimme4", out phrase))
                    {
                        phrase = "OH NO! How did that get in there? You have received a zombie";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gimme4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gimme5", out phrase))
                    {
                        phrase = "Unable to give currency for Gimme reward because Wallet is not enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gimme5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Suicide ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Suicide1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_killme}, {Command_Prefix1}{Command_wrist}, {Command_Prefix1}{Command_hang}, or {Command_Prefix1}{Command_suicide} once every {DelayBetweenUses} minute. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Suicide1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Suicide2", out phrase))
                    {
                        phrase = "You are too close to a player that is not a friend. Command unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Suicide2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Suicide3", out phrase))
                    {
                        phrase = "You are too close to a zombie. Command unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Suicide3\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************** Fps ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Fps1", out phrase))
                    {
                        phrase = "Server FPS: {Fps}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Fps1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Whisper ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->")
                        ;
                    if (!Dict.TryGetValue("Whisper1", out phrase))
                    {
                        phrase = "{PlayerName} was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Whisper1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Whisper2", out phrase))
                    {
                        phrase = "No one has pm'd you";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Whisper2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Whisper3", out phrase))
                    {
                        phrase = "Invalid message used to whisper";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Whisper3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Whisper4", out phrase))
                    {
                        phrase = "The player is not online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Whisper4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Reserved_Slots ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Reserved1", out phrase))
                    {
                        phrase = "{ServerResponseName} - The server is full. You were kicked by the reservation system to open a slot";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved2", out phrase))
                    {
                        phrase = "Sorry {PlayerName} you have been kicked with the longest session time. Please wait {TimeRemaining} minute before rejoining";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved3", out phrase))
                    {
                        phrase = "The player is not online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved4", out phrase))
                    {
                        phrase = "Your reserved status expires at {DateTime}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved5", out phrase))
                    {
                        phrase = "Your reserved status expired at {DateTime}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved6", out phrase))
                    {
                        phrase = "You are not on the reservation list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Reserved7", out phrase))
                    {
                        phrase = "A slot has been opened for you. Please retry";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Reserved7\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Clan_Manager ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Clan1", out phrase))
                    {
                        phrase = "You have already created the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan2", out phrase))
                    {
                        phrase = "Can not add the clan {ClanName} because it already exists";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan3", out phrase))
                    {
                        phrase = "You are currently a member of the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan4", out phrase))
                    {
                        phrase = "You have added the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan5", out phrase))
                    {
                        phrase = "You are not the owner of any clans";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan6", out phrase))
                    {
                        phrase = "You have removed the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan6\" Message=\"{0}\" />", phrase));

                    if (!Dict.TryGetValue("Clan7", out phrase))
                    {
                        phrase = "You do not have permissions to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan8", out phrase))
                    {
                        phrase = "The player {PlayerName} was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan9", out phrase))
                    {
                        phrase = "{PlayerName} is already a member of a clan named {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan10", out phrase))
                    {
                        phrase = "{PlayerName} already has a clan invitation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan11", out phrase))
                    {
                        phrase = "You have been invited to join the clan {ClanName}. Type {Command_Prefix1}{Command_accept} to join or {Command_Prefix1}{Command_decline} to decline the offer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan12", out phrase))
                    {
                        phrase = "You have invited {PlayerName} to the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan13", out phrase))
                    {
                        phrase = "You have not been invited to any clans";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan14", out phrase))
                    {
                        phrase = "The clan could not be found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan15", out phrase))
                    {
                        phrase = "{PlayerName} has declined the invite to the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan16", out phrase))
                    {
                        phrase = "You have declined the invite to the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan17", out phrase))
                    {
                        phrase = "{PlayerName} is not a member of your clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan18", out phrase))
                    {
                        phrase = "Only the clan owner can remove officers";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan19", out phrase))
                    {
                        phrase = "Clan owners can not be removed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan19\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan20", out phrase))
                    {
                        phrase = "You have removed {PlayerName} from clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan20\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan21", out phrase))
                    {
                        phrase = "You have been removed from the clan {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan21\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan22", out phrase))
                    {
                        phrase = "{PlayerName} is already a officer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan22\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan23", out phrase))
                    {
                        phrase = "{PlayerName} has been promoted to an officer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan23\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan24", out phrase))
                    {
                        phrase = "{PlayerName} is not an officer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan24\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan25", out phrase))
                    {
                        phrase = "{PlayerName} has been demoted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan25\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan26", out phrase))
                    {
                        phrase = "You can not leave the clan because you are the owner. You can only delete the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan26\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan27", out phrase))
                    {
                        phrase = "You do not belong to any clans";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan27\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan28", out phrase))
                    {
                        phrase = "The clan {ClanName} was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan28\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan29", out phrase))
                    {
                        phrase = "{PlayerName} has joined the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan29\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan30", out phrase))
                    {
                        phrase = "You have changed your clan name to {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan30\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan31", out phrase))
                    {
                        phrase = "Your clan name has been changed by the owner to {ClanName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan31\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan32", out phrase))
                    {
                        phrase = "{PlayerName} has left the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan32\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan33", out phrase))
                    {
                        phrase = "The player is not online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan33\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan34", out phrase))
                    {
                        phrase = "Clan names are:";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan34\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan35", out phrase))
                    {
                        phrase = "No clans were found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan35\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan36", out phrase))
                    {
                        phrase = "The clan name is too short or too long. It must be 2 to {MaxLength} characters";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan36\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan37", out phrase))
                    {
                        phrase = "{PlayerName} has joined another clan. Request removed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan37\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan38", out phrase))
                    {
                        phrase = "There is a request to join the clan from {PlayerName}. Type {Command_Prefix1}{Command_accept} to join or {Command_Prefix1}{Command_decline} to decline the offer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan38\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan39", out phrase))
                    {
                        phrase = "There are no requests to join the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan39\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan40", out phrase))
                    {
                        phrase = "You have sent a request to join the clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan40\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan41", out phrase))
                    {
                        phrase = "You have already sent a request to join this clan";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan41\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan42", out phrase))
                    {
                        phrase = "That clan name was not found on the list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan42\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan43", out phrase))
                    {
                        phrase = "Available clan commands are:";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan43\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan44", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_add} playerName";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan44\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan45", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_invite} playerName";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan45\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan46", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_remove} playerName";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan46\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan47", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_promote} playerName";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan47\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan48", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_demote} playerName";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan48\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan49", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_chat} message or {Command_Prefix1}{Command_cc}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan49\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Clan50", out phrase))
                    {
                        phrase = "Usage: {Command_Prefix1}{Command_rename} Name";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Clan50\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Watchlist ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Watchlist1", out phrase))
                    {
                        phrase = "{PlayerName} is on the watchlist for {Reason}.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Watchlist1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Reset_Player ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ResetPlayer1", out phrase))
                    {
                        phrase = "Reseting your player profile";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ResetPlayer1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ResetPlayer2", out phrase))
                    {
                        phrase = "You have reset the profile for player {Id}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ResetPlayer2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Stop_Server ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("StopServer1", out phrase))
                    {
                        phrase = "Server Shutdown In {Value} Minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StopServer1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("StopServer2", out phrase))
                    {
                        phrase = "Saving World. Exchanging items and new construction risks being lost from this point";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StopServer2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("StopServer3", out phrase))
                    {
                        phrase = "Shutdown is in 30 seconds. Please come back after the server restarts";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StopServer3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("StopServer4", out phrase))
                    {
                        phrase = "Loot, storage, trader, vendor and workstations will close and lock in 15 seconds";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StopServer4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("StopServer5", out phrase))
                    {
                        phrase = "Loot, storage, trader, vendor and workstations are now locked";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StopServer5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Jail ************************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Jail1", out phrase))
                    {
                        phrase = "You have been sent to jail";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail2", out phrase))
                    {
                        phrase = "You have been released from jail";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail3", out phrase))
                    {
                        phrase = "You have set the jail position as {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail4", out phrase))
                    {
                        phrase = "The jail position has not been set";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail5", out phrase))
                    {
                        phrase = "Player {PlayerName} is already in jail";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail6", out phrase))
                    {
                        phrase = "You have put {PlayerName} in jail.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail7", out phrase))
                    {
                        phrase = "Player {PlayerName} is not in jail";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail8", out phrase))
                    {
                        phrase = "The jail is electrified. Do not try to leave it";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail9", out phrase))
                    {
                        phrase = "Do not pee on the electric fence";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail10", out phrase))
                    {
                        phrase = "You do not have permission to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail11", out phrase))
                    {
                        phrase = "Player {PlayerName} was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Jail12", out phrase))
                    {
                        phrase = "You have been released from jail but failed to locate a suitable nearby location. Please rejoin or contact an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Jail12\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** New_Spawn_Tele ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("NewSpawnTele1", out phrase))
                    {
                        phrase = "You have set the new spawn position as {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele2", out phrase))
                    {
                        phrase = "You have been teleported to the new spawn location";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele3", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_ready} when you are prepared to leave. You will teleport back to your spawn location";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele4", out phrase))
                    {
                        phrase = "You have no saved return point or you have used it";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele5", out phrase))
                    {
                        phrase = "You have left the new player area. Return to it before using {Command_Prefix1}{Command_ready}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele6", out phrase))
                    {
                        phrase = "You have been sent back to your original spawn location. Good luck";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewSpawnTele7", out phrase))
                    {
                        phrase = "You do not have permissions to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewSpawnTele7\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Lottery ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Lottery1", out phrase))
                    {
                        phrase = "The lottery is now at {Value1}. Type {Command_Prefix1}{Command_lottery_enter} to buy a ticket for {Value2} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery2", out phrase))
                    {
                        phrase = "You have purchased lottery ticket number {Value}. The draw is in {Time} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery3", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to buy a ticket.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery4", out phrase))
                    {
                        phrase = "The lottery numbers are";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery5", out phrase))
                    {
                        phrase = "We have a winner!!! Thank you for playing everyone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery6", out phrase))
                    {
                        phrase = "You won the lottery. Congratulations!";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery7", out phrase))
                    {
                        phrase = "No winner this time. Thanks for playing everyone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lottery8", out phrase))
                    {
                        phrase = "The lottery is sold out";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lottery8\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Lobby ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Lobby1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_lobby} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby2", out phrase))
                    {
                        phrase = "You have set the lobby position as {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby3", out phrase))
                    {
                        phrase = "You can go back by typing {Command_Prefix1}{Command_lback} when you are ready to leave the lobby";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby4", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby5", out phrase))
                    {
                        phrase = "The lobby position is not set";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby6", out phrase))
                    {
                        phrase = "You have no return point saved";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby7", out phrase))
                    {
                        phrase = "You have left the lobby";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby8", out phrase))
                    {
                        phrase = "You do not have permissions to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby9", out phrase))
                    {
                        phrase = "This command is locked to donors only";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby10", out phrase))
                    {
                        phrase = "You are already inside the lobby. Unable to run command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby11", out phrase))
                    {
                        phrase = "Do not attack this player. Your PvP mode does not match";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby12", out phrase))
                    {
                        phrase = "You can not damage zombies while inside the lobby on bloodmoon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby13", out phrase))
                    {
                        phrase = "You can not use this command during the bloodmoon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby14", out phrase))
                    {
                        phrase = "You have been executed for attacking another player inside a PvE area";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Lobby15", out phrase))
                    {
                        phrase = "You have been kicked for attacking another player inside a PvE area";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Lobby15\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Market ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Market1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_market} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market2", out phrase))
                    {
                        phrase = "You can go back by typing {Command_Prefix1}{Command_mback} when you are ready to leave the market";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market3", out phrase))
                    {
                        phrase = "You have no saved return point";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market4", out phrase))
                    {
                        phrase = "The market position is not set";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market5", out phrase))
                    {
                        phrase = "You have left the market";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market6", out phrase))
                    {
                        phrase = "You have set the market position as {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market7", out phrase))
                    {
                        phrase = "You do not have permissions to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market8", out phrase))
                    {
                        phrase = "This command is locked to donors only";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market9", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market10", out phrase))
                    {
                        phrase = "Do not attack this player. Your PvP mode does not match";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market11", out phrase))
                    {
                        phrase = "You are already inside the market. Unable to run command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market12", out phrase))
                    {
                        phrase = "You can not damage zombies while inside the market on bloodmoon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market13", out phrase))
                    {
                        phrase = "You can not use this command during the bloodmoon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market14", out phrase))
                    {
                        phrase = "You have been executed for killing another player inside a PvE area";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Market15", out phrase))
                    {
                        phrase = "You have been kicked for attacking another player inside a PvE area";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Market15\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Waypoints *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Waypoints1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_waypoint} once every {DelayBetweenUses} minutes. Time remaining: {Value} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints2", out phrase))
                    {
                        phrase = "You can only use a waypoint that is outside of a claimed space";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints3", out phrase))
                    {
                        phrase = "Traveling to waypoint {Waypoint}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints4", out phrase))
                    {
                        phrase = "That waypoint name was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints5", out phrase))
                    {
                        phrase = "You have a maximum {Value} waypoints";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints6", out phrase))
                    {
                        phrase = "You can not save a waypoint in a POI area";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints7", out phrase))
                    {
                        phrase = "Waypoint {Name} has been deleted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints8", out phrase))
                    {
                        phrase = "Saved waypoint as {Name} at position {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints9", out phrase))
                    {
                        phrase = "Could not find a waypoint with this name on the list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints10", out phrase))
                    {
                        phrase = "You can only save a waypoint that is outside of a claimed space";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints11", out phrase))
                    {
                        phrase = "The waypoint name you are saving is blank. Unable to save";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints12", out phrase))
                    {
                        phrase = "Waypoint {Name} @ {Position} for {Cost} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints13", out phrase))
                    {
                        phrase = "You can not use waypoint commands while in a event";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints14", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints15", out phrase))
                    {
                        phrase = "This waypoint already exists. Choose another name";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints16", out phrase))
                    {
                        phrase = "Your friend {PlayerName} has invited you to their saved waypoint. Type {Command_Prefix1}{Command_go_way} to accept the request";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints17", out phrase))
                    {
                        phrase = "Invited your friend {PlayerName} to your saved waypoint";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints18", out phrase))
                    {
                        phrase = "You have run out of time to accept your friend's waypoint invitation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Waypoints19", out phrase))
                    {
                        phrase = "You have no waypoints saved";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Waypoints19\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Animal_Tracking ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("AnimalTracking1", out phrase))
                    {
                        phrase = "You have taxed your tracking ability. Wait {TimeRemaining} minute and try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AnimalTracking1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AnimalTracking2", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AnimalTracking2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AnimalTracking3", out phrase))
                    {
                        phrase = "You have tracked down an animal to within {Radius} blocks";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AnimalTracking3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AnimalTracking4", out phrase))
                    {
                        phrase = "Animal list is empty. Contact an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AnimalTracking4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AnimalTracking5", out phrase))
                    {
                        phrase = "Could not locate a suitable spawn location near by";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AnimalTracking5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Vote_Reward ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("VoteReward1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_reward} once every {DelayBetweenRewards} hour. Time remaining: {TimeRemaining} hour";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward2", out phrase))
                    {
                        phrase = "No items found on the vote reward list. Contact an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward3", out phrase))
                    {
                        phrase = "Unable to get a result from the website, {PlayerName}. Please try {Command_Prefix1}{Command_reward} again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward4", out phrase))
                    {
                        phrase = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward5", out phrase))
                    {
                        phrase = "You have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward6", out phrase))
                    {
                        phrase = "You have voted {Value} time since {Date}. You need {Value2} more votes before {Date2} to reach the bonus";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward7", out phrase))
                    {
                        phrase = "Thank you for your vote. You can vote and receive another reward in {Value} hours";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward8", out phrase))
                    {
                        phrase = "Reward items were sent to your inventory. If it is full, check the ground";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward9", out phrase))
                    {
                        phrase = "Spawned a {EntityName} near you";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward10", out phrase))
                    {
                        phrase = "No spawn point was found near you. Please move locations and try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward11", out phrase))
                    {
                        phrase = "{PlayerName} received an amazing reward because they are also amazing and voted for the server. Yippy!";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VoteReward12", out phrase))
                    {
                        phrase = "Unable to give WalletCoin for Vote reward because Wallet is not enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VoteReward12\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Zones ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Zones1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_return} for {Time} minutes after being killed in a pve zone. Time has expired";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones2", out phrase))
                    {
                        phrase = "You can not set your home inside a zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones3", out phrase))
                    {
                        phrase = "No damage. This player is in an area that does not match your player killing mode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones4", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been executed for attempted murder in a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones5", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been kicked for attempted murder in a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones6", out phrase))
                    {
                        phrase = "Auto detection has kicked you for attempted murder in a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones7", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been banned for attempted murder in a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones8", out phrase))
                    {
                        phrase = "Auto detection has banned you for murder in a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones9", out phrase))
                    {
                        phrase = "No damage. The player is inside a pve zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones10", out phrase))
                    {
                        phrase = "No damage. The player is inside a ally damage zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Zones11", out phrase))
                    {
                        phrase = "No damage. The player is inside a stranger damage zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Zones11\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Custom_Commands ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("CustomCommands1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{CommandCustom} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CustomCommands1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("CustomCommands2", out phrase))
                    {
                        phrase = "You do not have permission to use {Command}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CustomCommands2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("CustomCommands3", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CustomCommands3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("CustomCommands4", out phrase))
                    {
                        phrase = "You are not on the reserved list. Unable to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CustomCommands4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("CustomCommands5", out phrase))
                    {
                        phrase = "It is currently a bloodmoon. Unable to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CustomCommands5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Shop ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Shop1", out phrase))
                    {
                        phrase = "The shop categories are:";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop2", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_shop} 'category' to view that list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop3", out phrase))
                    {
                        phrase = "You are not inside a trader area. Find a trader and use this command again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop4", out phrase))
                    {
                        phrase = "Shop security code set to {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop5", out phrase))
                    {
                        phrase = "You do not have enough {CoinName}. Your wallet balance is {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop6", out phrase))
                    {
                        phrase = "There was no item # matching the shop. Check the shop category again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop7", out phrase))
                    {
                        phrase = "There was an error in the shop list. Unable to buy this item. Please alert an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop8", out phrase))
                    {
                        phrase = "The shop does not contain any items. Contact an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop9", out phrase))
                    {
                        phrase = "You are not inside a market and trader area. Find one and use this command again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop10", out phrase))
                    {
                        phrase = "You are outside the market. Get inside it and try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop11", out phrase))
                    {
                        phrase = "# {Id}: {Count} {Item} {Quality} quality for {Price} {Name}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop12", out phrase))
                    {
                        phrase = "# {Id}: {Count} {Item} for {Price} {Name}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop13", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_shop_buy} # to purchase the shop item. You can add how many times you want to buy it with {Command_Prefix1}{Command_shop_buy} # #";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop14", out phrase))
                    {
                        phrase = "This category is missing. Check {Command_Prefix1}{Command_shop}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop15", out phrase))
                    {
                        phrase = "Item not found for shop purchase";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shop16", out phrase))
                    {
                        phrase = "{Count} {Item} was purchased through the shop. If your bag is full, check the ground";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shop16\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Friend_Teleport ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("FriendTeleport1", out phrase))
                    {
                        phrase = "Friend = {FriendName}, Id = {EntityId}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport2", out phrase))
                    {
                        phrase = "This {EntityId} is not valid. Only integers accepted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport3", out phrase))
                    {
                        phrase = "Sent your friend {PlayerName} a teleport request";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport4", out phrase))
                    {
                        phrase = "{PlayerName} would like to teleport to you. Type {Command_Prefix1}{Command_accept} in chat to accept the request";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport5", out phrase))
                    {
                        phrase = "Did not find EntityId {EntityId}. No teleport request sent";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport6", out phrase))
                    {
                        phrase = "You can only teleport to a friend once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport7", out phrase))
                    {
                        phrase = "Your request was accepted. Teleporting you to your friend";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport8", out phrase))
                    {
                        phrase = "No friends found online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport9", out phrase))
                    {
                        phrase = "This player is not your friend. You can not request teleport to them";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport10", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport11", out phrase))
                    {
                        phrase = "Unable to complete request. Friend was not found online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FriendTeleport12", out phrase))
                    {
                        phrase = "Your friend's teleport request has expired";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FriendTeleport12\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Admin_List *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("AdminList1", out phrase))
                    {
                        phrase = "Server admins in game: [FF8000]";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AdminList1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AdminList2", out phrase))
                    {
                        phrase = "Server moderators in game: [FF8000]";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AdminList2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AdminList3", out phrase))
                    {
                        phrase = "No admins or moderators are online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AdminList3\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Shutdown ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Shutdown1", out phrase))
                    {
                        phrase = "The next auto shutdown is in [FF8000]{TimeLeft}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shutdown1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shutdown2", out phrase))
                    {
                        phrase = "A event is currently active. The server can not shutdown until it completes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shutdown2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shutdown3", out phrase))
                    {
                        phrase = "Loot, storage, vehicles, traders and workstations are locked until shutdown completes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shutdown3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Shutdown4", out phrase))
                    {
                        phrase = "Server is shutting down. Rejoin when it restarts";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Shutdown4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Died ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Died1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_died} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Died2", out phrase))
                    {
                        phrase = "Your last death occurred too long ago. Command unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Died3", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Died4", out phrase))
                    {
                        phrase = "You have no saved death position";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Died5", out phrase))
                    {
                        phrase = "You are too low a level to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Died6", out phrase))
                    {
                        phrase = "You are too high a level to use this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Died6\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Restart_Vote ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("RestartVote1", out phrase))
                    {
                        phrase = "A vote to restart the server has opened and will close in 60 seconds. Type {Command_Prefix1}{Command_yes} to cast your vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote2", out phrase))
                    {
                        phrase = "There are not enough players online to start a restart vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote3", out phrase))
                    {
                        phrase = "Players voted yes to a server restart. Shutdown has been initiated";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote4", out phrase))
                    {
                        phrase = "Players voted yes but not enough votes were cast to restart";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote5", out phrase))
                    {
                        phrase = "Players voted no to a server restart. A new vote can open in {RestartDelay} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote6", out phrase))
                    {
                        phrase = "The restart vote was a tie. A new vote can open in {RestartDelay} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote7", out phrase))
                    {
                        phrase = "No votes were cast to restart the server";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote8", out phrase))
                    {
                        phrase = "You started the last restart vote. Another player must initiate the next vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote9", out phrase))
                    {
                        phrase = "{PlayerName} has requested a restart vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote10", out phrase))
                    {
                        phrase = "A administrator is currently online. They have been alerted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote11", out phrase))
                    {
                        phrase = "There is a vote already open";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote12", out phrase))
                    {
                        phrase = "You must wait thirty minutes after the server starts before opening a restart vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote13", out phrase))
                    {
                        phrase = "There are now {Value} of {VotesNeeded} votes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote14", out phrase))
                    {
                        phrase = "You have already voted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("RestartVote15", out phrase))
                    {
                        phrase = "Unable to start a restart vote during the bloodmoon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RestartVote15\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Location *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Location1", out phrase))
                    {
                        phrase = "Your current position is {Position}. Zone: {Name}, set to {Mode}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Location1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Location2", out phrase))
                    {
                        phrase = "Your current position is {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Location2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Real_World_Time ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("RealWorldTime1", out phrase))
                    {
                        phrase = "The real world time is {Time} {TimeZone}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RealWorldTime1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************** Day7 ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Day7_1", out phrase))
                    {
                        phrase = "Server FPS:{Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_2", out phrase))
                    {
                        phrase = "Next horde night: {Value} days";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_3", out phrase))
                    {
                        phrase = "Total Players:{Players} Total Zombies:{Zombies} Total Animals:{Animals}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_4", out phrase))
                    {
                        phrase = "Bicycles:{Bicycles} Minibikes:{Minibikes} Motorcycles:{Motorcycles} 4x4:{4x4} Gyros:{Gyros}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_5", out phrase))
                    {
                        phrase = "Supply Crates:{Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_6", out phrase))
                    {
                        phrase = "The horde is here!";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Day7_7", out phrase))
                    {
                        phrase = "Next horde night is today";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Day7_7\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Vehicle_Recall ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("VehicleRecall1", out phrase))
                    {
                        phrase = "Vehicle {Id}, {Type}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall2", out phrase))
                    {
                        phrase = "You do not have any vehicles in your list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall3", out phrase))
                    {
                        phrase = "Invalid vehicle Id provided. Requires a valid number";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall4", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_recall} once every {DelayBetweenUses} minutes for each vehicle you have saved. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall5", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall6", out phrase))
                    {
                        phrase = "You do not own this vehicle";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall7", out phrase))
                    {
                        phrase = "Someone is using this vehicle. Unable to recall it";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall8", out phrase))
                    {
                        phrase = "Vehicle has been recalled to your location";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("VehicleRecall9", out phrase))
                    {
                        phrase = "This vehicle is out of range";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"VehicleRecall9\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* World_Radius ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("WorldRadius1", out phrase))
                    {
                        phrase = "You have reached the world border";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WorldRadius1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Report ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Report1", out phrase))
                    {
                        phrase = "You can only make a report once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Report1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Report2", out phrase))
                    {
                        phrase = "Report @ position {Position} from {PlayerName}: {Message}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Report2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Report3", out phrase))
                    {
                        phrase = "Your report has been sent to online administrators and logged";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Report3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Report4", out phrase))
                    {
                        phrase = "Your report is too long. Please reduce it to {Length} characters";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Report4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Report5", out phrase))
                    {
                        phrase = "Your report has been sent to online administrators and logged";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Report5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Bounties *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Bounties1", out phrase))
                    {
                        phrase = "{PlayerName} has collected {Value} bounties without dying! Their bounty has increased";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties2", out phrase))
                    {
                        phrase = "Player {Victim}' kill streak has come to an end by {Killer}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties3", out phrase))
                    {
                        phrase = "Player {Killer}' has collected the bounty of {Victim}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties4", out phrase))
                    {
                        phrase = "You do not have enough in your wallet for this bounty: {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties5", out phrase))
                    {
                        phrase = "A bounty of {Value} has been added to {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties6", out phrase))
                    {
                        phrase = "You input an invalid integer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties7", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_bounty} Id Value or with no Value for the minimum bounty against this player id";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties8", out phrase))
                    {
                        phrase = "{PlayerName} with id {EntityId}. Bounty of {Value} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bounties9", out phrase))
                    {
                        phrase = "No players have a bounty on them";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bounties9\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Kill_Notice ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("KillNotice1", out phrase))
                    {
                        phrase = "{Name1} has killed {Name2} using {Item}. {Value} damage";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice2", out phrase))
                    {
                        phrase = "{Name1} lvl ({Level1}) has killed {Name2} lvl ({Level2}) using {Item}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice3", out phrase))
                    {
                        phrase = "{Name1} lvl ({Level1}) has killed {Name2} lvl ({Level2}) using {Item}. {Value} damage";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice4", out phrase))
                    {
                        phrase = "{Name1} has killed {Name2} using {Item}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice5", out phrase))
                    {
                        phrase = "{Name1} lvl ({Level}) has been ate by {Name2}. {Value} damage. Nom nom nom";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice6", out phrase))
                    {
                        phrase = "{Name1} lvl ({Level}) has been ate by {Name2}. Nom nom nom";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice7", out phrase))
                    {
                        phrase = "{Name1} has been ate by {Name2}. {Value} damage. Nom nom nom";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice8", out phrase))
                    {
                        phrase = "{Name1} has been ate by {Name2}. Nom nom nom";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KillNotice9", out phrase))
                    {
                        phrase = "{Name} has died from {DamageType}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KillNotice9\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Stuck ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Stuck1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_stuck} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Stuck1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Stuck2", out phrase))
                    {
                        phrase = "You are outside of your claimed space or a friends. Command is unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Stuck2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Stuck3", out phrase))
                    {
                        phrase = "Sent you to the world surface. If you are still stuck, contact an administrator";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Stuck3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Stuck4", out phrase))
                    {
                        phrase = "You do not seem to be stuck";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Stuck4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Poll ************************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Poll1", out phrase))
                    {
                        phrase = "Poll results: Yes {YesVote} / No {NoVote}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Poll2", out phrase))
                    {
                        phrase = "Poll: {Message}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Poll3", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_poll_yes} or {Command_Prefix1}{Command_poll_no} to vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Poll4", out phrase))
                    {
                        phrase = "You have cast a vote for yes in the poll";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Poll5", out phrase))
                    {
                        phrase = "You have cast a vote for no in the poll";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Poll6", out phrase))
                    {
                        phrase = "You have already voted on the poll";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Poll6\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Hardcore ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Hardcore1", out phrase))
                    {
                        phrase = "Hardcore Top Players";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore2", out phrase))
                    {
                        phrase = "Playtime 1 {Name1}, {Session1}. Playtime 2 {Name2}, {Session3}. Playtime 3 {Name3}, {Session3}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore3", out phrase))
                    {
                        phrase = "Score 1 {Name1}, {Score1}. Score 2 {Name2}, {Score2}. Score 3 {Name3}, {Score3}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore4", out phrase))
                    {
                        phrase = "Hardcore: Name {PlayerName}, Playtime {PlayTime}, Player Kills {PlayerKills}, Zombie Kills {ZombieKills}, Deaths {Deaths}, Score {Score}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore5", out phrase))
                    {
                        phrase = "Hardcore mode is enabled! You have {Value} lives remaining...";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore6", out phrase))
                    {
                        phrase = "There are no hardcore records";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore7", out phrase))
                    {
                        phrase = "You have bought one extra life";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore8", out phrase))
                    {
                        phrase = "You do not have enough to buy a life";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore9", out phrase))
                    {
                        phrase = "You are at the maximum extra lives";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore10", out phrase))
                    {
                        phrase = "You have not turned on hardcore mode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore11", out phrase))
                    {
                        phrase = "You are now in hardcore mode with limited lives remaining. Type {Command_Prefix1}{Command_hardcore} to check how many lives remain";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore12", out phrase))
                    {
                        phrase = "You are already signed up for hardcore mode. Type {Command_Prefix1}{Command_hardcore} to check how many lives remain";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore13", out phrase))
                    {
                        phrase = "The server is resetting your data profile. Unable to join";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Hardcore14", out phrase))
                    {
                        phrase = "Auto kicked at end of hardcore session. Please rejoin in 10 seconds";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hardcore14\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- **************** Chat_Flood_Protection ***************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ChatFloodProtection1", out phrase))
                    {
                        phrase = "You have sent too many messages in one minute. Your chat function is locked temporarily";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatFloodProtection1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatFloodProtection2", out phrase))
                    {
                        phrase = "Message is too long";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatFloodProtection2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Auction ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Auction1", out phrase))
                    {
                        phrase = "The auction item {Name} has been removed from your inventory and added as id {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction2", out phrase))
                    {
                        phrase = "You have the max auction items already listed. Wait for one to sell or cancel it with {Command_Prefix1}{Command_auction_cancel} #";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction3", out phrase))
                    {
                        phrase = "You need to input a price greater than zero. This is not a transfer system";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction4", out phrase))
                    {
                        phrase = "Your sell price must be an integer and greater than zero";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction5", out phrase))
                    {
                        phrase = "# {Id}: {Count} {Item} at {Quality} quality, {Durability} percent durability for {Price} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction6", out phrase))
                    {
                        phrase = "No items are currently for sale";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction7", out phrase))
                    {
                        phrase = "You can not make this purchase. You need {Value} more {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction8", out phrase))
                    {
                        phrase = "This # could not be found. Please check the auction list by typing {Command_Prefix1}{Command_auction}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction9", out phrase))
                    {
                        phrase = "You have purchased {Count} {ItemName} from the auction for {Value} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction10", out phrase))
                    {
                        phrase = "Your auction item was purchased";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction11", out phrase))
                    {
                        phrase = "Your auction item has returned to you";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction12", out phrase))
                    {
                        phrase = "Could not find this id listed in the auction. Unable to cancel";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction13", out phrase))
                    {
                        phrase = "The auction is disabled for your admin tier";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction14", out phrase))
                    {
                        phrase = "You have used an auction item # that does not exist or has sold";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction15", out phrase))
                    {
                        phrase = "Command is disabled. Wallet is not enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction16", out phrase))
                    {
                        phrase = "Unable to set a unique id for your auction item. Try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction17", out phrase))
                    {
                        phrase = "# {Id}: {Count} {Item} for {Price} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction18", out phrase))
                    {
                        phrase = "You must remove modifications and cosmetics from the item you wish to enter the auction";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction19", out phrase))
                    {
                        phrase = "The following {Value} items are yours";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction19\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction20", out phrase))
                    {
                        phrase = "Auction security code set to {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction20\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Auction21", out phrase))
                    {
                        phrase = "Auction failed. No item found in the first slot of near by chest";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Auction21\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Bank ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Bank1", out phrase))
                    {
                        phrase = "Your bank account holds {Value} {CoinName}. Transfer Id is {Id}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank2", out phrase))
                    {
                        phrase = "You can not use this command here. Stand in your own or a friend's claimed space";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank3", out phrase))
                    {
                        phrase = "Deposited {Value} {Name} from your bag to your bank account. {Percent}% deposit fee was applied";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank4", out phrase))
                    {
                        phrase = "Deposited {Value} {Name} from your bag to your bank account";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank5", out phrase))
                    {
                        phrase = "You do not have enough {Name} to transfer to your bank";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank6", out phrase))
                    {
                        phrase = "You input an invalid integer";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank7", out phrase))
                    {
                        phrase = "The bank coin is not setup correctly, contact a server Admin";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank8", out phrase))
                    {
                        phrase = "You have received the {CoinName} from your bank. If your bag is full, check the ground at your feet";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank9", out phrase))
                    {
                        phrase = "You can only withdraw a full stack at a time. The maximum stack size is {Max}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank10", out phrase))
                    {
                        phrase = "You do not have this much in your bank";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank11", out phrase))
                    {
                        phrase = "Id not found. Ask for the transfer Id from the player you want to transfer to";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank11\" Message=\"{0}\" />", phrase));

                    if (!Dict.TryGetValue("Bank12", out phrase))
                    {
                        phrase = "Your command format was invalid. Type {Command_Prefix1}{Command_transfer} # #";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank13", out phrase))
                    {
                        phrase = "Transfer id not found. Request the id from the player you intend to transfer to";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank14", out phrase))
                    {
                        phrase = "Could not find this player online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank15", out phrase))
                    {
                        phrase = "You have made a bank transfer of {Value} to player {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank16", out phrase))
                    {
                        phrase = "You have received a bank transfer of {Value} from player {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank17", out phrase))
                    {
                        phrase = "{Value} {CoinName} added to your bank";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank18", out phrase))
                    {
                        phrase = "Your bank account holds {Value} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bank19", out phrase))
                    {
                        phrase = "Bank reduced {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bank19\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Exit_Command ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ExitCommand1", out phrase))
                    {
                        phrase = "You moved and need to restart your countdown";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ExitCommand1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ExitCommand2", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_exit} to quit the game or you may drop items";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ExitCommand2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ExitCommand3", out phrase))
                    {
                        phrase = "You have disconnected. Thank you for playing with us. Come back soon";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ExitCommand3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ExitCommand4", out phrase))
                    {
                        phrase = "Please wait {Time} seconds for disconnection and do not move";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ExitCommand4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Bloodmoon ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Bloodmoon1", out phrase))
                    {
                        phrase = "Next horde night is in {Value} days";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bloodmoon1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bloodmoon2", out phrase))
                    {
                        phrase = "Next horde night is today. It will start at {Time}:00";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bloodmoon2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bloodmoon3", out phrase))
                    {
                        phrase = "The horde is here! Hold out until {Time}:00";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bloodmoon3\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Bloodmoon_Warrior ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("BloodmoonWarrior1", out phrase))
                    {
                        phrase = "Hades has called upon you. Survive this night and kill {Count} zombies to be rewarded by the king of the underworld.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"BloodmoonWarrior1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("BloodmoonWarrior2", out phrase))
                    {
                        phrase = "You have survived and been rewarded by hades himself. Your death count was reduced by one.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"BloodmoonWarrior2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("BloodmoonWarrior3", out phrase))
                    {
                        phrase = "You have survived and been rewarded by hades himself.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"BloodmoonWarrior3\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* First_Claim_Block ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("FirstClaimBlock1", out phrase))
                    {
                        phrase = "Claim block has been added to your inventory or if inventory is full, it dropped at your feet";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FirstClaimBlock1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("FirstClaimBlock2", out phrase))
                    {
                        phrase = "You have already received your first claim block. Contact an administrator if you require help";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FirstClaimBlock2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Kick_Vote ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("KickVote1", out phrase))
                    {
                        phrase = "A vote to kick {PlayerName} has begun and will close in 30 seconds";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote2", out phrase))
                    {
                        phrase = "This player id was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote3", out phrase))
                    {
                        phrase = "Not enough players are online to start a vote to kick";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote4", out phrase))
                    {
                        phrase = "PlayerName = {PlayerName}, # = {Id}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote5", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_yes} to vote kick the player";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote6", out phrase))
                    {
                        phrase = "Can not start a vote for yourself";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote7", out phrase))
                    {
                        phrase = "Players voted to kick but not enough votes were cast";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote8", out phrase))
                    {
                        phrase = "No votes were cast to kick the player";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote9", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_kickvote} # to start a vote to kick that player";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote10", out phrase))
                    {
                        phrase = "The players voted to kick you from the game";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote11", out phrase))
                    {
                        phrase = "No other users were found online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote12", out phrase))
                    {
                        phrase = "There is a vote already open";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote13", out phrase))
                    {
                        phrase = "There are now {Value} of {VotesNeeded} votes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("KickVote14", out phrase))
                    {
                        phrase = "You have already voted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"KickVote14\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Homes ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Homes1", out phrase))
                    {
                        phrase = "You have no homes saved";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes2", out phrase))
                    {
                        phrase = "Home {Name} @ {Position} for {Cost} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes3", out phrase))
                    {
                        phrase = "You can not use home commands while in a event";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes4", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_home} once every {DelayBetweenUses} minutes. Time remaining: {Value} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes5", out phrase))
                    {
                        phrase = "This saved home is no longer inside a claimed space. Home point removed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes6", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes7", out phrase))
                    {
                        phrase = "This home was not found on your list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes8", out phrase))
                    {
                        phrase = "You can only save a home that is inside of a claimed space";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes9", out phrase))
                    {
                        phrase = "Saved home as {Name} at position {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes10", out phrase))
                    {
                        phrase = "This home already exists. Choose another name";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes11", out phrase))
                    {
                        phrase = "You have a maximum {Value} homes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes12", out phrase))
                    {
                        phrase = "Saved home as {Name} at position {Position}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes13", out phrase))
                    {
                        phrase = "Home {Name} has been deleted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes14", out phrase))
                    {
                        phrase = "Your friend {PlayerName} has invited you to their saved home. Type {Command_Prefix1}{Command_go_home} to accept the request";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes15", out phrase))
                    {
                        phrase = "Invited your friend {PlayerName} to your saved home";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes16", out phrase))
                    {
                        phrase = "You have run out of time to accept your friend's home invitation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Homes17", out phrase))
                    {
                        phrase = "The home name you are saving is blank. Unable to save";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Homes17\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Mute ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Mute1", out phrase))
                    {
                        phrase = "Could not mute. {PlayerName} was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute2", out phrase))
                    {
                        phrase = "You have muted player {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute3", out phrase))
                    {
                        phrase = "You have already muted player {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute4", out phrase))
                    {
                        phrase = "You have removed {PlayerName} with id {EntityId} from your mute list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute5", out phrase))
                    {
                        phrase = "You have removed player with id {EntityId} from your mute list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute6", out phrase))
                    {
                        phrase = "This player is not on your mute list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute7", out phrase))
                    {
                        phrase = "Muted: Unknown name with id {EntityId}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute8", out phrase))
                    {
                        phrase = "Invalid id: {EntityId}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute9", out phrase))
                    {
                        phrase = "Muted: {PlayerName} with id {EntityId}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute10", out phrase))
                    {
                        phrase = "You are muted and blocked from commands";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute11", out phrase))
                    {
                        phrase = "You are muted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Mute12", out phrase))
                    {
                        phrase = "You have no muted players";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Mute12\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Event ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Event1", out phrase))
                    {
                        phrase = "Not enough players signed up for the event. The invitation and setup has been cleared";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event2", out phrase))
                    {
                        phrase = "The event did not have enough players join. It has been cancelled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event3", out phrase))
                    {
                        phrase = "The event is full. Unable to join";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event4", out phrase))
                    {
                        phrase = "You have signed up for the event and your current location has been saved for return";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event5", out phrase))
                    {
                        phrase = "You are on team {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event6", out phrase))
                    {
                        phrase = "{EventName} has now started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event7", out phrase))
                    {
                        phrase = "{EventName} still has space for more players. Type {Command_Prefix1}{Command_join} to join";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event8", out phrase))
                    {
                        phrase = "{Value} of {PlayerTotal} have joined the event";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event9", out phrase))
                    {
                        phrase = "You have already joined this event. It will start when enough players sign up";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event10", out phrase))
                    {
                        phrase = "The event is at half time";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event11", out phrase))
                    {
                        phrase = "The event has five minutes remaining";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event12", out phrase))
                    {
                        phrase = "If you need to extend the time remaining, use the console to type event extend #. The time is in minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event13", out phrase))
                    {
                        phrase = "The event has ended and all players have been sent to their return positions";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event14", out phrase))
                    {
                        phrase = "The event ended while you were offline or not spawned. You have been sent to your return position";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event15", out phrase))
                    {
                        phrase = "You have been sent to your event spawn point";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Event16", out phrase))
                    {
                        phrase = "The event ended while you were offline or not spawned";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Event16\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Session *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Session1", out phrase))
                    {
                        phrase = "Your current session is at {TimePassed} minutes. Your total session time is at {TotalTimePassed} minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Session1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Night_Alert ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("NightAlert1", out phrase))
                    {
                        phrase = "{Value} hours until night time";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NightAlert1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Player_List ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("PlayerList1", out phrase))
                    {
                        phrase = "Player = {PlayerName}, Id = {EntityId}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerList1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerList2", out phrase))
                    {
                        phrase = "No other players found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerList2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Prayer ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Prayer1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_pray} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Prayer1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Prayer2", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Prayer2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Scout_Player ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ScoutPlayer1", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_scoutplayer} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ScoutPlayer1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ScoutPlayer2", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ScoutPlayer2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ScoutPlayer3", out phrase))
                    {
                        phrase = "You have scouted a trail to a hostile player in the area with in {Value} blocks";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ScoutPlayer3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ScoutPlayer4", out phrase))
                    {
                        phrase = "No trails or tracks were detected to a nearby hostile player";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ScoutPlayer4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Starting_Items ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("StartingItems1", out phrase))
                    {
                        phrase = "You have received the starting items. Check your inventory. If full, check the ground";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StartingItems1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("StartingItems2", out phrase))
                    {
                        phrase = "Unable to give WalletCoin for Starting items because Wallet is not enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"StartingItems2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Teleport *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Teleport1", out phrase))
                    {
                        phrase = "You are too close to a hostile zombie or animal. Command unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport2", out phrase))
                    {
                        phrase = "You are too close to a hostile player. Command unavailable";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport3", out phrase))
                    {
                        phrase = "You can not teleport with a vehicle";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport4", out phrase))
                    {
                        phrase = "You have been jailed for illegal teleportation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport5", out phrase))
                    {
                        phrase = "You have been killed for illegal teleportation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport6", out phrase))
                    {
                        phrase = "You have been kicked for illegal teleportation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Teleport7", out phrase))
                    {
                        phrase = "You have been banned for illegal teleportation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Teleport7\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Weather_Vote ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("WeatherVote1", out phrase))
                    {
                        phrase = "A vote to change the weather has begun and will close in 60 seconds";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote2", out phrase))
                    {
                        phrase = "Weather vote complete but no votes were cast. No changes were made";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote3", out phrase))
                    {
                        phrase = "Weather vote complete. Most votes went to {Weather}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote4", out phrase))
                    {
                        phrase = "Weather vote was a tie. No changes were made";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote5", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_sun}, {Command_Prefix1}{Command_rain} or {Command_Prefix1}{Command_snow} to cast your vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote6", out phrase))
                    {
                        phrase = "Not enough players are online to start a weather vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote7", out phrase))
                    {
                        phrase = "Wait sixty minutes before starting a new vote to change the weather. Time remaining {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote8", out phrase))
                    {
                        phrase = "There is a vote already open";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote9", out phrase))
                    {
                        phrase = "Clear skies ahead";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote10", out phrase))
                    {
                        phrase = "Light rain has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote11", out phrase))
                    {
                        phrase = "A rain storm has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote12", out phrase))
                    {
                        phrase = "A heavy rain storm has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote13", out phrase))
                    {
                        phrase = "Light snow has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote14", out phrase))
                    {
                        phrase = "A snow storm has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote15", out phrase))
                    {
                        phrase = "A heavy snow storm has started";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote16", out phrase))
                    {
                        phrase = "Not enough votes were cast in the weather vote. No changes were made";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote17", out phrase))
                    {
                        phrase = "Vote cast for clear";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote18", out phrase))
                    {
                        phrase = "You have already voted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote19", out phrase))
                    {
                        phrase = "There is no active weather vote. Type {Command_Prefix1}{Command_weathervote} to open a new vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote19\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote20", out phrase))
                    {
                        phrase = "Vote cast for rain";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote20\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("WeatherVote21", out phrase))
                    {
                        phrase = "Vote cast for snow";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WeatherVote21\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Give_Item *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("GiveItem1", out phrase))
                    {
                        phrase = "{Value} {ItemName} was sent to your inventory. If your bag is full, check the ground";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"GiveItem1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Mute_Vote ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("MuteVote1", out phrase))
                    {
                        phrase = "A vote to mute {PlayerName} in chat has begun and will close in 60 seconds";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote2", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_yes} to cast your vote";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote3", out phrase))
                    {
                        phrase = "{PlayerName} has been muted for 60 minutes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote4", out phrase))
                    {
                        phrase = "Type {Command_Prefix1}{Command_mutevote} # to start a vote to mute that player from chat.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote5", out phrase))
                    {
                        phrase = "This player is already muted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote6", out phrase))
                    {
                        phrase = "Player id was not found";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote7", out phrase))
                    {
                        phrase = "Not enough players are online to start a vote to mute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote8", out phrase))
                    {
                        phrase = "Player = {PlayerName}, # = {Id}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote9", out phrase))
                    {
                        phrase = "No other users were found online";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote10", out phrase))
                    {
                        phrase = "There is a vote already open";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote11", out phrase))
                    {
                        phrase = "There are now {Value} of {VotesNeeded} votes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("MuteVote12", out phrase))
                    {
                        phrase = "You have already voted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MuteVote12\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Chat_Color ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ChatColor1", out phrase))
                    {
                        phrase = "Your chat color prefix time has expired";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatColor2", out phrase))
                    {
                        phrase = "Your chat color tags are: {PrefixTags} and {NameTags}. They expire at {DateTime}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatColor3", out phrase))
                    {
                        phrase = "Improper format given for new color. Use HTML color tags. Example: [FF0000] or [FF0000],[FFCC00]";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatColor4", out phrase))
                    {
                        phrase = "Your chat prefix color has been set to {Tags}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatColor5", out phrase))
                    {
                        phrase = "Your chat name color has been set to {Tags}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("ChatColor6", out phrase))
                    {
                        phrase = "The chat color list is empty. Unable to rotate to a new color";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ChatColor6\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Info_Ticker ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("InfoTicker1", out phrase))
                    {
                        phrase = "You have turned off info ticker messages until the server restarts";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InfoTicker1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InfoTicker2", out phrase))
                    {
                        phrase = "You have turned on info ticker messages";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InfoTicker2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Damage_Detector ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("DamageDetector1", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been banned for damage manipulation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"DamageDetector1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("DamageDetector2", out phrase))
                    {
                        phrase = "Auto detection has banned you for damage manipulation";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"DamageDetector2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Spectator *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Spectator1", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been banned for spectator mode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Spectator1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Spectator2", out phrase))
                    {
                        phrase = "Auto detection has banned you for spectator mode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Spectator2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Godmode_Detector ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("GodMode1", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been banned for godmode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"GodMode1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("GodMode2", out phrase))
                    {
                        phrase = "Auto detection has banned you for godmode";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"GodMode2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Flying_Detector ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Flying1", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} has been banned for flying";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Flying1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Flying2", out phrase))
                    {
                        phrase = "Auto detection has banned you for flying";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Flying2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Player_Stats ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("PlayerStats1", out phrase))
                    {
                        phrase = "Auto detection has kicked you for illegal player stat health";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats2", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and kicked for illegal player stat health";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats3", out phrase))
                    {
                        phrase = "Auto detection has banned you for illegal player stat health";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats4", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and banned for illegal player stat health";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats5", out phrase))
                    {
                        phrase = "Auto detection has kicked you for illegal player stat stamina";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats6", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and kicked for illegal player stat stamina";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats7", out phrase))
                    {
                        phrase = "Auto detection has banned you for illegal player stat stamina";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats8", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and banned for illegal player stat stamina";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats9", out phrase))
                    {
                        phrase = "Auto detection has kicked you for illegal player stat jump strength";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats9\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats10", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and kicked for illegal player stat jump strength";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats10\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats11", out phrase))
                    {
                        phrase = "Auto detection has banned you for illegal player stat jump strength";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats11\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats12", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and banned for illegal player stat jump strength";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats12\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats13", out phrase))
                    {
                        phrase = "Auto detection has kicked you for illegal player height";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats13\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats14", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and kicked for illegal player height";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats14\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats15", out phrase))
                    {
                        phrase = "Auto detection has banned you for illegal player height";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats15\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats16", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and banned for illegal player height";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats16\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats17", out phrase))
                    {
                        phrase = "Auto detection has kicked you for illegal player stat run speed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats17\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats18", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and kicked for illegal player stat run speed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats18\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats19", out phrase))
                    {
                        phrase = "Auto detection has banned you for illegal player stat run speed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats19\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("PlayerStats20", out phrase))
                    {
                        phrase = "[FF0000]{PlayerName} was detected and banned for illegal player stat run speed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"PlayerStats20\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* POI_Protection ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("POI1", out phrase))
                    {
                        phrase = "You can not place a bed in a POI";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"POI1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("POI2", out phrase))
                    {
                        phrase = "You can not place a claim in a POI";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"POI2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Travel *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Travel1", out phrase))
                    {
                        phrase = "You have traveled to {Destination}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Travel1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Travel2", out phrase))
                    {
                        phrase = "You are not in a travel location";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Travel2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Travel3", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_travel} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Travel3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Travel4", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Travel4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Auto_Backup ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("AutoBackup1", out phrase))
                    {
                        phrase = "Starting auto backup. You might experience periods of lag and slow down until complete";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoBackup1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoBackup2", out phrase))
                    {
                        phrase = "Auto backup completed successfully";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoBackup2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ****************** New_Player_Protection *************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("NewPlayerProtection1", out phrase))
                    {
                        phrase = "This player is under the level required for PvP. No damage inflicted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewPlayerProtection1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("NewPlayerProtection2", out phrase))
                    {
                        phrase = "You are under the level required for PvP. No damage inflicted";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewPlayerProtection2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Level_Up ********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("LevelUp1", out phrase))
                    {
                        phrase = "Hooray! {PlayerName} has reached level {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"LevelUp1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Gamble *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Gamble1", out phrase))
                    {
                        phrase = "You are on round {Value1} with a total of {Value2}. Type {Command_Prefix1}{Command_gamble_payout} to collect it or {Command_Prefix1}{Command_gamble_bet} to gamble";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble2", out phrase))
                    {
                        phrase = "You have not started to gamble. Type {Command_Prefix1}{Command_gamble_bet} to gamble {Value} {CoinName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble3", out phrase))
                    {
                        phrase = "You do not have enough {Value} {CoinName} in your wallet to gamble";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble4", out phrase))
                    {
                        phrase = "Added {Value} {CoinName} to your wallet from gambling. Congratulations";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble5", out phrase))
                    {
                        phrase = "Round {Number} won! Collect {Value} using {Command_Prefix1}{Command_gamble_payout} or increase your bet using {Command_Prefix1}{Command_gamble_bet}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble6", out phrase))
                    {
                        phrase = "Round {Number} lost! You can start a new gamble with {Command_Prefix1}{Command_gamble_bet}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Gamble7", out phrase))
                    {
                        phrase = "You can only use the gamble system once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Gamble7\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Auto_Party_Invite ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("AutoPartyInvite1", out phrase))
                    {
                        phrase = "Player id {Value} named {Name}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite2", out phrase))
                    {
                        phrase = "You do not have any players on your auto party list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite3", out phrase))
                    {
                        phrase = "This player id is already on your list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite4", out phrase))
                    {
                        phrase = "Added player id {Value} named {Name} to your auto party list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite5", out phrase))
                    {
                        phrase = "Removed player id {Value} named {Name} from your party list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite6", out phrase))
                    {
                        phrase = "Player id {Value} was not found on your auto party list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite7", out phrase))
                    {
                        phrase = "Received auto party invitation from {PlayerName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AutoPartyInvite8", out phrase))
                    {
                        phrase = "Player id {Value} named {Name} can be added to your auto party list";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AutoPartyInvite8\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ***************** Dropped_Bag_Protection *************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("DroppedBagProtection1", out phrase))
                    {
                        phrase = "This bag is protected. Unable to access";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"DroppedBagProtection1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Anti-Cheat ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("AntiCheat1", out phrase))
                    {
                        phrase = "Banned for using a speed hack";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AntiCheat1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AntiCheat2", out phrase))
                    {
                        phrase = "Auto detection has banned you for using infinite ammo hack";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AntiCheat2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AntiCheat3", out phrase))
                    {
                        phrase = "Banned {PlayerName} for using a speed hack";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AntiCheat3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("AntiCheat4", out phrase))
                    {
                        phrase = "Bonk! Hitting your head on blocks will lose your vision";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"AntiCheat4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Workstation_Lock ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("WorkstationLock1", out phrase))
                    {
                        phrase = "Workstation is locked";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"WorkstationLock1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** New_Player ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("NewPlayer1", out phrase))
                    {
                        phrase = "Currently in bloodmoon. Please join when it finishes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NewPlayer1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Country_Ban ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("CountryBan1", out phrase))
                    {
                        phrase = "Auto detection has banned you for country IP region";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CountryBan1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Hordes *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Hordes1", out phrase))
                    {
                        phrase = "A horde has been spotted wandering the wastes";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Hordes1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Magic_Bullet ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("MagicBullet1", out phrase))
                    {
                        phrase = "Auto detection has banned you for using magic bullet hack";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"MagicBullet1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************ Proxy_Ban ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ProxyBan1", out phrase))
                    {
                        phrase = "Auto detection has banned you for proxy use";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ProxyBan1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Family_Share ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("FamilyShare1", out phrase))
                    {
                        phrase = "Family share accounts are forbidden without permission";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"FamilyShare1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Login_Notice ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("LoginNotice1", out phrase))
                    {
                        phrase = "Your login notice expires at {DateTime}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"LoginNotice1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Pickup *********************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Pickup1", out phrase))
                    {
                        phrase = "Pickup mode enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup2", out phrase))
                    {
                        phrase = "Pickup mode disabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup3", out phrase))
                    {
                        phrase = "Unable to pickup block. It is outside of your claimed space";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup4", out phrase))
                    {
                        phrase = "Unable to pickup block. Repair it first";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup5", out phrase))
                    {
                        phrase = "Picked up block {BlockName}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup5\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup6", out phrase))
                    {
                        phrase = "Pickup is blocked in this area until {DateTime}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup6\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup7", out phrase))
                    {
                        phrase = "Invalid block type. Unable to pickup";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup7\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup8", out phrase))
                    {
                        phrase = "Command restricted to reserved only";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup8\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Pickup9", out phrase))
                    {
                        phrase = "Unable to pickup blocks inside of a POI";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Pickup9\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************** Wall ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Wall1", out phrase))
                    {
                        phrase = "Wall mode enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Wall1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Wall2", out phrase))
                    {
                        phrase = "Wall mode disabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Wall2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Wall3", out phrase))
                    {
                        phrase = "Walls can only be built inside your claimed space.";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Wall3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Wall4", out phrase))
                    {
                        phrase = "Command restricted to reserved only";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Wall4\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Wall5", out phrase))
                    {
                        phrase = "Wall Completed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Wall5\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************** Bed ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Bed1", out phrase))
                    {
                        phrase = "You do not have any bedroll positions";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bed1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bed2", out phrase))
                    {
                        phrase = "You can only use {Command_Prefix1}{Command_bed} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minute";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bed2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bed3", out phrase))
                    {
                        phrase = "You do not have enough {CoinName} to run this command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bed3\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Bed4", out phrase))
                    {
                        phrase = "You have gone to bed. zzz";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Bed4\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************** Roll_It_Out ********************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Rio1", out phrase))
                    {
                        phrase = "Roll it out security code set to {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Rio1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Steam_Overlay ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Overlay1", out phrase))
                    {
                        phrase = "Overlay is set to false. Steam browser is disabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Overlay1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Overlay2", out phrase))
                    {
                        phrase = "Overlay has been set to false";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Overlay2\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Overlay3", out phrase))
                    {
                        phrase = "Overlay has been set to true";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Overlay3\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************** Interactive_Map ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("IMap1", out phrase))
                    {
                        phrase = "Interactive map security code set to {Value}";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"IMap1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Land_Claim_Count ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("LandClaim1", out phrase))
                    {
                        phrase = "You have {Value1} of {Value2} active land claims";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"LandClaim1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* No_Vehicle_Drone ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("NoVehicleDrone1", out phrase))
                    {
                        phrase = "Pick up your drone before entering a vehicle";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"NoVehicleDrone1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ****************** Command_Permission ****************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("CommandPermission1", out phrase))
                    {
                        phrase = "You do not have permission to use that command";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"CommandPermission1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ************************* Vault ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Vault1", out phrase))
                    {
                        phrase = "The vault can only be accessed inside of a claimed spaced";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Vault1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("Vault2", out phrase))
                    {
                        phrase = "The vault is currently disabled. Your stored items are safe and will show when the vault is enabled";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Vault2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ********************* Infinite_Ammo ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("InfiniteAmmo1", out phrase))
                    {
                        phrase = "You have been detected and banned for using infinite ammo";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InfiniteAmmo1\" Message=\"{0}\" />", phrase));
                    if (!Dict.TryGetValue("InfiniteAmmo2", out phrase))
                    {
                        phrase = "Detected {PlayerName} using infinite ammo. They have been banned";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"InfiniteAmmo2\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Overlay ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Overlay1", out phrase))
                    {
                        phrase = "Your overlay is set off. Unable to open link in steam browser";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Overlay1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ***************** Region_Chunk_Reset ******************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("RegionChunkReset1", out phrase))
                    {
                        phrase = "The server is busy. Please try again";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"RegionChunkReset1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Sorter ************************* -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Sorter1", out phrase))
                    {
                        phrase = "Sort Completed";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Sorter1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************* Protected_Zones ******************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("ProtectedZones1", out phrase))
                    {
                        phrase = "Protected Zone";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"ProtectedZones1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- *********************** Harvest ************************ -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    if (!Dict.TryGetValue("Harvest1", out phrase))
                    {
                        phrase = "Harvest Complete";
                    }
                    sw.WriteLine(string.Format("    <Phrase Name=\"Harvest1\" Message=\"{0}\" />", phrase));
                    sw.WriteLine();
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("    <!-- ******************************************************** -->");
                    sw.WriteLine("</Phrases>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Phrases.LoadXml: {0}", e.Message));
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

        public static void ResetXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                UpdateXml();
                FileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Phrases.UpdateXml: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                UpdateXml();
                FileWatcher.EnableRaisingEvents = false;
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
                if (childNodes != null)
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            if (childNodes[i].Name == "Phrase" && childNodes[i].Attributes.Count > 0)
                            {
                                for (int j = 0; j < _oldChildNodes.Count; j++)
                                {
                                    if (_oldChildNodes[j].NodeType == XmlNodeType.Comment)
                                    {
                                        continue;
                                    }
                                    if (_oldChildNodes[j].Name == "Phrase" && _oldChildNodes[j].Attributes.Count > 0 && _oldChildNodes[j].Attributes[0].Value == childNodes[i].Attributes[0].Value)
                                    {
                                        childNodes[i].Attributes[1].Value = _oldChildNodes[j].Attributes[1].Value;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    xmlDoc.Save(FilePath);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Phrases.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}