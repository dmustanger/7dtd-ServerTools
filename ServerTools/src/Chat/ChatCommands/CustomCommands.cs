using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static string Command15 = "commands";
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        public static Dictionary<string, int[]> Dict1 = new Dictionary<string, int[]>();
        public static List<int> TeleportCheckProtection = new List<int>();
        private const string file = "CustomChatCommands.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            if (IsRunning && !IsEnabled)
            {
                Dict.Clear();
                Dict1.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
            }
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
                if (childNode.Name == "Commands")
                {
                    Dict.Clear();
                    Dict1.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Custom Commands' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Trigger"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Trigger attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Hidden"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Custom Commands entry because of missing a Hidden attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _delay = 0;
                        if (_line.HasAttribute("DelayBetweenUses"))
                        {
                            if (!int.TryParse(_line.GetAttribute("DelayBetweenUses"), out _delay))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses of Custom Commands entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        int _number = 0;
                        if (_line.HasAttribute("Number"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Number"), out _number))
                            {
                                _number = Dict.Count + 1;
                            }
                        }
                        int _cost = 0;
                        if (_line.HasAttribute("Cost"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Cost"), out _cost))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for Cost of Custom Commands entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        string _trigger = _line.GetAttribute("Trigger");
                        if (!Dict.ContainsKey(_trigger))
                        {
                            string _response1 = _line.GetAttribute("Response");
                            string _hidden = _line.GetAttribute("Hidden");
                            string[] _s = { _response1, _hidden };
                            Dict.Add(_trigger, _s);
                        }
                        if (!Dict1.ContainsKey(_trigger))
                        {
                            int[] _c = { _number, _delay, _cost };
                            Dict1.Add(_trigger, _c);
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
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        int[] _value;
                        if (Dict1.TryGetValue(kvp.Key, out _value))
                        {
                            sw.WriteLine(string.Format("        <Command Number=\"{0}\" Trigger=\"{1}\" Response=\"{2}\" DelayBetweenUses=\"{3}\" Hidden=\"{4}\" Cost=\"{5}\" />", _value[0], kvp.Key, kvp.Value[0], _value[1], kvp.Value[1], _value[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Number=\"1\" Trigger=\"help\" Response=\"global Type " + ChatHook.Command_Private + Command15 + " for a list of chat commands.\" DelayBetweenUses=\"0\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"2\" Trigger=\"info\" Response=\"global Server Info: \" DelayBetweenUses=\"0\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"3\" Trigger=\"rules\" Response=\"whisper Visit YourSiteHere to see the rules.\" DelayBetweenUses=\"0\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"4\" Trigger=\"website\" Response =\"whisper Visit YourSiteHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"5\" Trigger=\"teamspeak\" Response=\"whisper The Teamspeak3 info is YourInfoHere.\" DelayBetweenUses=\"0\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"6\" Trigger=\"spawnz\" Response=\"ser {EntityId} 40 @ 4 11 14 ^ whisper Zombies have spawn around you.\" DelayBetweenUses=\"60\" Hidden=\"false\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"7\" Trigger=\"discord\" Response=\"whisper The discord channel is .\" DelayBetweenUses=\"20\" Hidden=\"true\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"8\" Trigger=\"test1\" Response=\"Your command here\" DelayBetweenUses=\"30\" Hidden=\"true\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"9\" Trigger=\"test2\" Response=\"Your command here\" DelayBetweenUses=\"40\" Hidden=\"true\" Cost=\"0\" />");
                    sw.WriteLine("        <Command Number=\"10\" Trigger=\"test3\" Response=\"Your command here\" DelayBetweenUses=\"40\" Hidden=\"true\" Cost=\"0\" />");
                }
                sw.WriteLine("    </Commands>");
                sw.WriteLine("</CustomCommands>");
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
        }

        public static string GetChatCommands1(ClientInfo _cInfo)
        {
            string _commands_1 = string.Format("{0}Commands are:", LoadConfig.Chat_Response_Color);
            if (FriendTeleport.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, FriendTeleport.Command59);
                _commands_1 = string.Format("{0} {1}{2} #", _commands_1, ChatHook.Command_Private, FriendTeleport.Command59);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, FriendTeleport.Command60);
            }
            if (Shop.IsEnabled && Wallet.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Wallet.Command56);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Shop.Command57);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Shop.Command58);
            }
            if (Gimme.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Gimme.Command24);
            }
            if (TeleportHome.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, TeleportHome.Command1);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, TeleportHome.Command2);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, TeleportHome.Command3);
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, TeleportHome.Command4);
            }
            if (Day7.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Day7.Command16);
            }
            if (Bloodmoon.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, Bloodmoon.Command18);
            }
            if (Whisper.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2} {3}{4}", _commands_1, ChatHook.Command_Private, Whisper.Command120, ChatHook.Command_Private, Whisper.Command122);
            }
            if (ClanManager.IsEnabled)
            {
                _commands_1 = string.Format("{0} {1}{2}", _commands_1, ChatHook.Command_Private, ClanManager.Command42);
            }
            return _commands_1;
        }

        public static string GetChatCommands2(ClientInfo _cInfo)
        {
            string _commands_2 = string.Format("{0}More Commands:", LoadConfig.Chat_Response_Color);
            if (FirstClaimBlock.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, FirstClaimBlock.Command32);
            }
            if (RestartVote.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, RestartVote.Command66);
            }
            if (Animals.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, Animals.Command30);
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, Animals.Command31);
            }
            if (VoteReward.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, VoteReward.Command46);
            }
            if (ReservedSlots.Reserved_Check)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, ReservedSlots.Command45);
            }
            if (AutoShutdown.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, AutoShutdown.Command47);
            }
            if (AdminList.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, AdminList.Command48);
            }
            if (Travel.IsEnabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, Travel.Command49);
            }
            if (TeleportHome.IsEnabled & TeleportHome.Set_Home2_Enabled)
            {
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, TeleportHome.Command5);
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, TeleportHome.Command6);
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, TeleportHome.Command7);
                _commands_2 = string.Format("{0} {1}{2}", _commands_2, ChatHook.Command_Private, TeleportHome.Command8);
            }
            return _commands_2;
        }

        public static string GetChatCommands3(ClientInfo _cInfo)
        {
            string _commands_3 = string.Format("{0}More Commands:", LoadConfig.Chat_Response_Color);
            if (WeatherVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, WeatherVote.Command62);
                if (WeatherVote.VoteOpen)
                {
                    _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, WeatherVote.Command63);
                    _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, WeatherVote.Command64);
                    _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, WeatherVote.Command65);
                }
            }
            if (AuctionBox.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, AuctionBox.Command71);
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, AuctionBox.Command72);
                _commands_3 = string.Format("{0} {1}{2} #", _commands_3, ChatHook.Command_Private, AuctionBox.Command73);
                _commands_3 = string.Format("{0} {1}{2} #", _commands_3, ChatHook.Command_Private, AuctionBox.Command74);
            }
            if (DeathSpot.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, DeathSpot.Command61);
            }
            if (Fps.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, Fps.Command75);
            }
            if (Loc.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, Loc.Command76);
            }
            if (MuteVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, MuteVote.Command67);
            }
            if (KickVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, KickVote.Command68);
            }
            if (Suicide.IsEnabled)
            {
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, Suicide.Command20);
                _commands_3 = string.Format("{0} {1}{2}", _commands_3, ChatHook.Command_Private, Suicide.Command23);
            }
            return _commands_3;
        }

        public static string GetChatCommands4(ClientInfo _cInfo)
        {
            string _commands_4 = string.Format("{0}More Commands:", LoadConfig.Chat_Response_Color);
            if (LobbyChat.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, LobbyChat.Command88);
                if (LobbyChat.Return)
                {
                    _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, LobbyChat.Command53);
                }
            }
            if (Bounties.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Bounties.Command83);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Bounties.Command83);
            }
            if (Lottery.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Lottery.Command84);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Lottery.Command84);
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Lottery.Command85);
            }
            if (Report.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Report.Command82);
            }
            if (Stuck.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Stuck.Command90);
            }
            if (Bank.IsEnabled)
            {
                _commands_4 = string.Format("{0} {1}{2}", _commands_4, ChatHook.Command_Private, Bank.Command94);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Bank.Command95);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Bank.Command96);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Bank.Command97);
                _commands_4 = string.Format("{0} {1}{2} #", _commands_4, ChatHook.Command_Private, Bank.Command98);
            }
            return _commands_4;
        }

        public static string GetChatCommands5(ClientInfo _cInfo)
        {
            string _commands_5 = string.Format("{0}More Commands:", LoadConfig.Chat_Response_Color);
            if (MarketChat.IsEnabled)
            {
                _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, MarketChat.Command103);
                if (MarketChat.Return)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, MarketChat.Command51);
                }
            }
            if (InfoTicker.IsEnabled)
            {
                _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, InfoTicker.Command104);
            }
            if (Waypoint.IsEnabled)
            {
                _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, Waypoint.Command106);
                _commands_5 = string.Format("{0} {1}{2} 'name'", _commands_5, ChatHook.Command_Private, Waypoint.Command106);
                _commands_5 = string.Format("{0} {1}{2} 'name'", _commands_5, ChatHook.Command_Private, Waypoint.Command112);
                _commands_5 = string.Format("{0} {1}{2} 'name'", _commands_5, ChatHook.Command_Private, Waypoint.Command115);
                _commands_5 = string.Format("{0} {1}{2} 'name'", _commands_5, ChatHook.Command_Private, Waypoint.Command109);
            }
            if (VehicleTeleport.IsEnabled)
            {
                if (VehicleTeleport.Bike)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, VehicleTeleport.Command77);
                }
                if (VehicleTeleport.Mini_Bike)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, VehicleTeleport.Command78);
                }
                if (VehicleTeleport.Motor_Bike)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, VehicleTeleport.Command79);
                }
                if (VehicleTeleport.Jeep)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, VehicleTeleport.Command80);
                }
                if (VehicleTeleport.Gyro)
                {
                    _commands_5 = string.Format("{0} {1}{2}", _commands_5, ChatHook.Command_Private, VehicleTeleport.Command81);
                }
            }
            return _commands_5;
        }

        public static string GetChatCommands6(ClientInfo _cInfo)
        {
            string _commands_6 = string.Format("{0}More Commands:", LoadConfig.Chat_Response_Color);
            if (PlayerList.IsEnabled)
            {
                _commands_6 = string.Format("{0} {1}{2}", _commands_6, ChatHook.Command_Private, PlayerList.Command89);
            }
            if (ClanManager.IsEnabled)
            {
                _commands_6 = string.Format("{0} {1}{2}", _commands_6, ChatHook.Command_Private, ClanManager.Command125);
            }
            if (Hardcore.IsEnabled && Hardcore.Max_Extra_Lives > 0)
            {
                _commands_6 = string.Format("{0} {1}{2}", _commands_6, ChatHook.Command_Private, Hardcore.Command126);
            }
            return _commands_6;
        }

        public static string GetChatCommandsCustom(ClientInfo _cInfo)
        {
            string _commandsCustom = string.Format("{0}Custom commands are:", LoadConfig.Chat_Response_Color);
            if (Dict.Count > 0)
            {
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    string _h = kvp.Value[1];
                    bool _result;
                    if (bool.TryParse(_h, out _result))
                    {
                        if (!_result)
                        {
                            string _c = kvp.Key;
                            _commandsCustom = string.Format("{0} /{1}", _commandsCustom, _c);
                        }
                    }
                }
            }
            return _commandsCustom;
        }

        public static string GetChatCommandsAdmin(ClientInfo _cInfo)
        {
            string _commandsAdmin = string.Format("{0}Admin commands are:", LoadConfig.Chat_Response_Color);
            if (AdminChat.IsEnabled && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel <= ChatHook.Mod_Level)
                {
                    if (AdminChat.IsEnabled)
                    {
                        _commandsAdmin = string.Format("{0} @" + AdminChat.Command118, _commandsAdmin);
                        _commandsAdmin = string.Format("{0} @" + AdminChat.Command119, _commandsAdmin);
                    }
                    if (Jail.IsEnabled)
                    {
                        _commandsAdmin = string.Format("{0} {1}{2}", _commandsAdmin, ChatHook.Command_Private, Jail.Command27);
                    }
                    if (MutePlayer.IsEnabled)
                    {
                        _commandsAdmin = string.Format("{0} {1}{2}", _commandsAdmin, ChatHook.Command_Private, MutePlayer.Command13);
                        _commandsAdmin = string.Format("{0} {1}{2}", _commandsAdmin, ChatHook.Command_Private, MutePlayer.Command14);
                    }
                }
            }
            return _commandsAdmin;
        }

        public static void CheckCustomDelay(ClientInfo _cInfo, string _message, string _playerName, bool _announce)
        {
            int _timepassed = 0;
            int[] _c;
            if (Dict1.TryGetValue(_message, out _c))
            {
                if (_c[1] == 0)
                {
                    Permission(_cInfo, _message, _playerName, _announce, _c);
                }
                else
                {
                    bool _donator = false;
                    if (_c[0] == 1)
                    {
                        string _sql = string.Format("SELECT customCommand1 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand1;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand1);
                        _result.Dispose();
                        if (_customCommand1.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand1;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 2)
                    {
                        string _sql = string.Format("SELECT customCommand2 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand2;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand2);
                        _result.Dispose();
                        if (_customCommand2.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand2;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 3)
                    {
                        string _sql = string.Format("SELECT customCommand3 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand3;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand3);
                        _result.Dispose();
                        if (_customCommand3.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand3;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 4)
                    {
                        string _sql = string.Format("SELECT customCommand4 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand4;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand4);
                        _result.Dispose();
                        if (_customCommand4.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand4;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 5)
                    {
                        string _sql = string.Format("SELECT customCommand5 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand5;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand5);
                        _result.Dispose();
                        if (_customCommand5.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand5;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 6)
                    {
                        string _sql = string.Format("SELECT customCommand6 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand6;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand6);
                        _result.Dispose();
                        if (_customCommand6.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand6;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 7)
                    {
                        string _sql = string.Format("SELECT customCommand7 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand7;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand7);
                        _result.Dispose();
                        if (_customCommand7.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand7;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 8)
                    {
                        string _sql = string.Format("SELECT customCommand8 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand8;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand8);
                        _result.Dispose();
                        if (_customCommand8.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand8;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 9)
                    {
                        string _sql = string.Format("SELECT customCommand9 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand9;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand9);
                        _result.Dispose();
                        if (_customCommand9.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand9;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 10)
                    {
                        string _sql = string.Format("SELECT customCommand10 FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        DateTime _customCommand10;
                        DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _customCommand10);
                        _result.Dispose();
                        if (_customCommand10.ToString() != "10/29/2000 7:30:00 AM")
                        {
                            TimeSpan varTime = DateTime.Now - _customCommand10;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            Permission(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft1 = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft1, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= _c[1] || _timepassed == -1)
                        {
                            Permission(_cInfo, _message, _playerName, _announce, _c);
                        }
                        else
                        {
                            int _timeleft = _c[1] - _timepassed;
                            if (_announce)
                            {
                                DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _c[1]);
                            }
                            else
                            {
                                DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _c[1]);
                            }
                        }
                    }
                }
            }
        }

        public static void DelayResponse(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int _timeleft, int _newDelay)
        {
            string _phrase616;
            if (!Phrases.Dict.TryGetValue(616, out _phrase616))
            {
                _phrase616 = " you can only use {CommandPrivate}{Command15} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
            }
            _phrase616 = _phrase616.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase616 = _phrase616.Replace("{Command15}", Command15);
            _phrase616 = _phrase616.Replace("{DelayBetweenUses}", _newDelay.ToString());
            _phrase616 = _phrase616.Replace("{TimeRemaining}", _timeleft.ToString());
            if (_announce)
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase616 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase616 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Permission(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int[] _c)
        {
            string[] _command = { _message };
            if (GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo.playerId))
            {
                CommandCost(_cInfo, _message, _playerName, _announce, _c);
            }
            else
            {
                string _phrase827;
                if (!Phrases.Dict.TryGetValue(827, out _phrase827))
                {
                    _phrase827 = " you do not have permission to use {Command}.";
                }
                _phrase827 = _phrase827.Replace("{Command}", _message);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase827 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int[] _c)
        {
            if (_c[2] > 0)
            {
                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                if (_currentCoins >= _c[2])
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _c[2]);
                    CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                CommandResponse(_cInfo, _message, _playerName, _announce, _c);
            }
        }

        public static void CommandResponse(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int[] _c)
        {
            if (_c[0] == 1)
            {
                string _sql = string.Format("UPDATE Players SET customCommand1 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 2)
            {
                string _sql = string.Format("UPDATE Players SET customCommand2 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 3)
            {
                string _sql = string.Format("UPDATE Players SET customCommand3 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 4)
            {
                string _sql = string.Format("UPDATE Players SET customCommand4 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 5)
            {
                string _sql = string.Format("UPDATE Players SET customCommand5 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 6)
            {
                string _sql = string.Format("UPDATE Players SET customCommand6 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 7)
            {
                string _sql = string.Format("UPDATE Players SET customCommand7 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 8)
            {
                string _sql = string.Format("UPDATE Players SET customCommand8 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 9)
            {
                string _sql = string.Format("UPDATE Players SET customCommand9 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            if (_c[0] == 10)
            {
                string _sql = string.Format("UPDATE Players SET customCommand10 = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                SQL.FastQuery(_sql, "CustomCommands");
            }
            string[] _r;
            if (Dict.TryGetValue(_message, out _r))
            {
                string[] _responseSplit = _r[0].Split('^');
                foreach (string _response in _responseSplit)
                {
                    string _responseAdj = _response;
                    _responseAdj = _responseAdj.Replace("{EntityId}", _cInfo.entityId.ToString());
                    _responseAdj = _responseAdj.Replace("{SteamId}", _cInfo.playerId);
                    _responseAdj = _responseAdj.Replace("{PlayerName}", _playerName);
                    if (_responseAdj.StartsWith("global "))
                    {
                        _responseAdj = _responseAdj.Replace("global ", "");
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _responseAdj + "[-]", _cInfo.entityId, _playerName, EChatType.Global, null);
                    }
                    else if (_responseAdj.StartsWith("whisper ") || _responseAdj.StartsWith("whisper "))
                    {
                        _responseAdj = _responseAdj.Replace("whisper ", "");
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _responseAdj + "[-]", _cInfo.entityId, _playerName, EChatType.Whisper, null);
                    }
                    else if (_responseAdj.StartsWith("tele ") || _responseAdj.StartsWith("tp ") || _responseAdj.StartsWith("teleportplayer "))
                    {
                        try
                        {
                            SdtdConsole.Instance.ExecuteSync(_responseAdj, _cInfo);
                            if (Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                            {
                                Zones.ZoneExit.Remove(_cInfo.entityId);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.Run: {0}.", e));
                        }
                    }
                    else
                    {
                        try
                        {
                            SdtdConsole.Instance.ExecuteSync(_responseAdj, _cInfo);
                        }
                        catch (Exception e)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.Run: {0}.", e));
                        }
                    }
                }
                
            }
        }
    }
}