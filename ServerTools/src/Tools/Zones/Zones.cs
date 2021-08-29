using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Zones
    {
        public static bool IsEnabled = false, IsRunning = false, Zone_Message = false, Set_Home = false;
        public static int Reminder_Delay = 20;
        public static Dictionary<int, DateTime> Reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string[]> ZonePlayer = new Dictionary<int, string[]>();
        public static List<string[]> ZoneList = new List<string[]>();
        public static Dictionary<int, string[]> ZoneSetup = new Dictionary<int, string[]>();

        private const string file = "Zones.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            ZoneList.Clear();
            Reminder.Clear();
            ZonePlayer.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
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
                    ZoneList.Clear();
                    Reminder.Clear();
                    ZonePlayer.Clear();
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") != Config.Version)
                                {
                                    UpgradeXml(_childNodes);
                                    return;
                                }
                                else if (_line.HasAttribute("Name") && _line.HasAttribute("Corner1") && _line.HasAttribute("Corner2") && _line.HasAttribute("Circle") &&
                                    _line.HasAttribute("EntryMessage") && _line.HasAttribute("ExitMessage") && _line.HasAttribute("EntryCommand") && _line.HasAttribute("ExitCommand") &&
                                    _line.HasAttribute("ReminderNotice") && _line.HasAttribute("PvPvE") && _line.HasAttribute("NoZombie"))
                                {
                                    string[] _zone = { _line.GetAttribute("Name"), _line.GetAttribute("Corner1"), _line.GetAttribute("Corner2"), _line.GetAttribute("Circle"),
                                _line.GetAttribute("EntryMessage"), _line.GetAttribute("ExitMessage"), _line.GetAttribute("EntryCommand"),_line.GetAttribute("ExitCommand"),
                                _line.GetAttribute("ReminderNotice"), _line.GetAttribute("PvPvE"), _line.GetAttribute("NoZombie") };
                                    if (_zone[6] == "")
                                    {
                                        _zone[6] = "";
                                    }
                                    if (_zone[7] == "")
                                    {
                                        _zone[7] = "";
                                    }
                                    if (_zone[3].ToLower() == "false")
                                    {
                                        if (_zone[1].Contains(",") && _zone[2].Contains(","))
                                        {
                                            string[] _corner1 = _zone[1].Split(',');
                                            string[] _corner2 = _zone[2].Split(',');
                                            int.TryParse(_corner1[0], out int x1);
                                            int.TryParse(_corner1[1], out int y1);
                                            int.TryParse(_corner1[2], out int z1);
                                            int.TryParse(_corner2[0], out int x2);
                                            int.TryParse(_corner2[1], out int y2);
                                            int.TryParse(_corner2[2], out int z2);
                                            int _switch;
                                            if (x1 > x2)
                                            {
                                                _switch = x2;
                                                x2 = x1;
                                                x1 = _switch;
                                            }
                                            if (y1 > y2)
                                            {
                                                _switch = y2;
                                                y2 = y1;
                                                y1 = _switch;
                                            }
                                            else if (y1 == y2)
                                            {
                                                y2++;
                                            }
                                            if (z1 > z2)
                                            {
                                                _switch = z2;
                                                z2 = z1;
                                                z1 = _switch;
                                            }
                                            _zone[1] = x1 + "," + y1 + "," + z1;
                                            _zone[2] = x2 + "," + y2 + "," + z2;
                                        }
                                        else
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Zones.xml entry. Improper format in corner1 or corner2 attribute: {0}", _line.OuterXml));
                                            continue;
                                        }
                                    }
                                    if (!ZoneList.Contains(_zone))
                                    {
                                        ZoneList.Add(_zone);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Zones>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Do not use decimals in the corner positions -->");
                    sw.WriteLine("<!-- Overlapping zones: the first zone listed that is overlapping will take priority -->");
                    sw.WriteLine("<!-- PvPvE: 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone -->");
                    sw.WriteLine("<!-- EntryCommand and ExitCommand trigger console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("<!-- Possible variables for commands include {PlayerName}, {EntityId}, {PlayerId}, {Delay}, whisper, global -->");
                    sw.WriteLine("<!-- <Zone Name=\"Example\" Corner1=\"1,2,3\" Corner2=\"-3,4,-5\" Circle=\"false\" EntryMessage=\"You have entered example\" ExitMessage=\"You have exited example\" EntryCommand=\"whisper This is a pve space\" ExitCommand=\"\" ReminderNotice=\"You are still in example\" PvPvE=\"0\" NoZombie=\"True\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (ZoneList.Count > 0)
                    {
                        for (int i = 0; i < ZoneList.Count; i++)
                        {
                            string[] _zone = ZoneList[i];
                            sw.WriteLine(string.Format("    <Zone Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Circle=\"{3}\" EntryMessage=\"{4}\" ExitMessage=\"{5}\" EntryCommand=\"{6}\" ExitCommand=\"{7}\" ReminderNotice=\"{8}\" PvPvE=\"{9}\" NoZombie=\"{10}\" />", _zone[0], _zone[1], _zone[2], _zone[3], _zone[4], _zone[5], _zone[6], _zone[7], _zone[8], _zone[9], _zone[10]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Zone Name=\"\" Corner1=\"\" Corner2=\"\" Circle=\"\" EntryMessage=\"\" ExitMessage=\"\" EntryCommand=\"\" ExitCommand=\"\" ReminderNotice=\"\" PvPvE=\"\" NoZombie=\"\" /> -->");
                    }
                    sw.WriteLine("</Zones>");
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.UpdateXml: {0}", e.Message));
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

        public static void ZoneCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            try
            {
                if (ZoneList.Count > 0)
                {
                    for (int i = 0; i < ZoneList.Count; i++)
                    {
                        string[] _zone = ZoneList[i];
                        if (InsideZone(_zone, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z))
                        {
                            if (ZonePlayer.ContainsKey(_player.entityId))
                            {
                                ZonePlayer.TryGetValue(_player.entityId, out string[] _info);
                                if (_info != _zone)
                                {
                                    if (Zone_Message)
                                    {
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[4] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    if (_zone[6] != "")
                                    {
                                        ProcessCommand(_cInfo, _zone[6]);
                                    }
                                    ZonePlayer[_player.entityId] = _zone;
                                    Reminder[_player.entityId] = DateTime.Now;
                                    if (_zone[9] == "0")
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 0", true));
                                    }
                                    else if (_zone[9] == "1")
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 1", true));
                                    }
                                    else if (_zone[9] == "2")
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 2", true));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 3", true));
                                    }
                                }
                            }
                            else
                            {
                                if (Zone_Message)
                                {
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[4] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (_zone[6] != "")
                                {
                                    ProcessCommand(_cInfo, _zone[6]);
                                }
                                ZonePlayer.Add(_player.entityId, _zone);
                                Reminder.Add(_player.entityId, DateTime.Now);
                                if (_zone[9] == "0")
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 0", true));
                                }
                                else if (_zone[9] == "1")
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 1", true));
                                }
                                else if (_zone[9] == "2")
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 2", true));
                                }
                                else
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 3", true));
                                }
                            }
                            return;
                        }
                    }
                }
                if (ZonePlayer.ContainsKey(_player.entityId))
                {
                    ZonePlayer.TryGetValue(_player.entityId, out string[] _zone);
                    if (Zone_Message && _zone[5] != "")
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[5] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (_zone[7] != "")
                    {
                        ProcessCommand(_cInfo, _zone[7]);
                    }
                    if (_zone[9] != PersistentOperations.Player_Killing_Mode.ToString())
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sgs PlayerKillingMode {0}", PersistentOperations.Player_Killing_Mode), true));
                    }
                    ZonePlayer.Remove(_player.entityId);
                    Reminder.Remove(_player.entityId);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ZoneCheck: {0}", e.Message));
            }
        }

        private static void ProcessCommand(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (_command.Contains("^"))
                {
                    List<string> _commands = _command.Split('^').ToList();
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string _commandTrimmed = _commands[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] _commandSplit = _commandTrimmed.Split(' ');
                            if (int.TryParse(_commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Zone_SingleUseTimer(_time, _cInfo.playerId, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Zone command error. Unable to commit delay with improper integer: {0}", _command));
                            }
                        }
                        else
                        {
                            Command(_cInfo, _commandTrimmed);
                        }
                    }
                }
                else
                {
                    Command(_cInfo, _command);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ProcessCommand: {0}", e.Message));
            }
        }

        public static void ZoneCommandDelayed(string _playerId, List<string> _commands)
        {
            try
            {
                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_playerId);
                if (_cInfo != null)
                {
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string _commandTrimmed = _commands[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] _commandSplit = _commandTrimmed.Split(' ');
                            if (int.TryParse(_commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Zone_SingleUseTimer(_time, _cInfo.playerId, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Zone command error. Unable to commit delay with improper integer: {0}", _commands));
                            }
                        }
                        else
                        {
                            Command(_cInfo, _commandTrimmed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ZoneCommandDelayed: {0}", e.Message));
            }
        }

        public static void Command(ClientInfo _cInfo, string _command)
        {
            try
            {
                _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                _command = _command.Replace("{SteamId}", _cInfo.playerId);
                _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                if (_command.ToLower().StartsWith("global "))
                {
                    _command = _command.Replace("Global ", "");
                    _command = _command.Replace("global ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (_command.ToLower().StartsWith("whisper "))
                {
                    _command = _command.Replace("Whisper ", "");
                    _command = _command.Replace("whisper ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (_command.StartsWith("tele ") || _command.StartsWith("tp ") || _command.StartsWith("teleportplayer "))
                {
                    if (Zones.IsEnabled && Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
                    {
                        Zones.ZonePlayer.Remove(_cInfo.entityId);
                    }
                    SdtdConsole.Instance.ExecuteSync(_command, null);
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_command, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.Command: {0}", e.Message));
            }
        }

        public static bool InsideZone(string[] _zone, float _X, float _Y, float _Z)
        {
            string[] _corner1 = _zone[1].Split(',');
            float.TryParse(_corner1[0], out float xMin);
            float.TryParse(_corner1[1], out float yMin);
            float.TryParse(_corner1[2], out float zMin);
            if (_zone[3].ToLower() == "true")
            {
                if (int.TryParse(_zone[2], out int _radius))
                {
                    if (VectorCircle(xMin, zMin, _X, _Z, _radius))
                    {
                        return true;
                    }
                }
            }
            else
            {
                string[] _corner2 = _zone[2].Split(',');
                float.TryParse(_corner2[0], out float xMax);
                float.TryParse(_corner2[1], out float yMax);
                float.TryParse(_corner2[2], out float zMax);
                if (VectorBox(xMin, yMin, zMin, xMax, yMax, zMax, _X, _Y, _Z))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool VectorCircle(float xMin, float zMin, float _X, float _Z, int _radius)
        {
            if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
            {
                return true;
            }
            return false;
        }

        public static bool VectorBox(float xMin, float yMin, float zMin, float xMax, float yMax, float zMax, float _X, float _Y, float _Z)
        {
            if (_X >= xMin && _X <= xMax && _Y >= yMin && _Y <= yMax && _Z >= zMin && _Z <= zMax)
            {
                return true;
            }
            return false;
        }

        public static void HostileCheck()
        {
            try
            {
                if (ZoneList.Count > 0)
                {
                    for (int i = 0; i < ZoneList.Count; i++)
                    {
                        string[] _zone = ZoneList[i];
                        if (_zone[10].ToLower() == "true")
                        {
                            List<Entity> Entities = GameManager.Instance.World.Entities.list;
                            for (int j = 0; j < Entities.Count; j++)
                            {
                                Entity _entity = Entities[j];
                                if (_entity != null && !_entity.IsClientControlled() && !_entity.IsDead())
                                {
                                    string _tags = _entity.EntityClass.Tags.ToString();
                                    if (_tags.Contains("zombie") || _tags.Contains("hostile"))
                                    {
                                        Vector3 _vec = _entity.position;
                                        int _X = (int)_entity.position.x;
                                        int _Y = (int)_entity.position.y;
                                        int _Z = (int)_entity.position.z;
                                        string[] _corner1 = _zone[1].Split(',');
                                        int.TryParse(_corner1[0], out int _xMin);
                                        int.TryParse(_corner1[1], out int _yMin);
                                        int.TryParse(_corner1[2], out int _zMin);
                                        if (_zone[3].ToLower() == "true")
                                        {
                                            if (int.TryParse(_zone[2], out int _radius))
                                            {
                                                if (VectorCircle(_xMin, _zMin, _X, _Z, _radius))
                                                {
                                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from zone {1} @ {2} {3} {4}",
                                                        EntityClass.list[_entity.entityClass].entityClassName, _zone[0], _X, _Y, _Z));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string[] _corner2 = _zone[2].Split(',');
                                            int.TryParse(_corner2[0], out int _xMax);
                                            int.TryParse(_corner2[1], out int _yMax);
                                            int.TryParse(_corner2[2], out int _zMax);
                                            if (VectorBox(_xMin, _yMin, _zMin, _xMax, _yMax, _zMax, _X, _Y, _Z))
                                            {
                                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from zone {1} @ {2} {3} {4}",
                                                    EntityClass.list[_entity.entityClass].entityClassName, _zone[0], _X, _Y, _Z));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.HostileCheck: {0}", e.Message));
            }
        }

        public static void ReminderExec()
        {
            try
            {
                foreach (KeyValuePair<int, DateTime> time in Reminder.ToArray())
                {
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(time.Key);
                    if (_cInfo != null)
                    {
                        DateTime _dt = time.Value;
                        TimeSpan varTime = DateTime.Now - _dt;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= Reminder_Delay)
                        {
                            ZonePlayer.TryGetValue(_cInfo.entityId, out string[] _zone);
                            if (_zone != null && _zone[8] != "")
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[8] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Reminder[_cInfo.entityId] = DateTime.Now;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ReminderExec: {0}", e.Message));
            }
        }

        public static bool IsValid(EntityPlayer _player1, ClientInfo _cInfo2, EntityPlayer _player2)
        {
            try
            {
                if (ZonePlayer.ContainsKey(_player1.entityId) && ZonePlayer.ContainsKey(_player2.entityId))
                {
                    ZonePlayer.TryGetValue(_player1.entityId, out string[] _zone1);
                    ZonePlayer.TryGetValue(_player2.entityId, out string[] _zone2);
                    if (_zone1[9] != _zone2[9])
                    {
                        Phrases.Dict.TryGetValue("Zones3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                else if (ZonePlayer.ContainsKey(_player1.entityId) && !ZonePlayer.ContainsKey(_player2.entityId))
                {
                    ZonePlayer.TryGetValue(_player1.entityId, out string[] _zone);
                    if (_zone[9] != PersistentOperations.Player_Killing_Mode.ToString())
                    {
                        Phrases.Dict.TryGetValue("Zones3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                else if (!ZonePlayer.ContainsKey(_player1.entityId) && ZonePlayer.ContainsKey(_player2.entityId))
                {
                    ZonePlayer.TryGetValue(_player2.entityId, out string[] _zone);
                    if (_zone[9] != PersistentOperations.Player_Killing_Mode.ToString())
                    {
                        Phrases.Dict.TryGetValue("Zones3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.IsValid: {0}", e.Message));
            }
            return true;
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Zones>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Do not use decimals in the corner positions -->");
                    sw.WriteLine("<!-- Overlapping zones: the first zone listed that is overlapping will take priority -->");
                    sw.WriteLine("<!-- PvPvE: 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone -->");
                    sw.WriteLine("<!-- EntryCommand and ExitCommand trigger console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("<!-- Possible variables for commands include {PlayerName}, {EntityId}, {PlayerId}, {Delay}, whisper, global -->");
                    sw.WriteLine("<!-- <Zone Name=\"Example\" Corner1=\"1,2,3\" Corner2=\"-3,4,-5\" Circle=\"false\" EntryMessage=\"You have entered example\" ExitMessage=\"You have exited example\" EntryCommand=\"whisper This is a pve space\" ExitCommand=\"\" ReminderNotice=\"You are still in example\" PvPvE=\"0\" NoZombie=\"True\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- Do not use decimals") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- Overlapping zones:") && !_oldChildNodes[i].OuterXml.StartsWith("<!-- PvPvE: 0 = No Killing") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- EntryCommand and ExitCommand") && !_oldChildNodes[i].OuterXml.StartsWith("<!-- Possible variables for commands") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Zone Name=\"Example\"") && !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Zone Name=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Zone")
                            {
                                _blank = false;
                                string _name = "", _corner1 = "", _corner2 = "", _circle = "", _entryMessage = "", _exitMessage = "", _entryCommand = "",
                                    _exitCommand = "", _reminder = "", _pvpve = "", _noZ = "";
                                if (_line.HasAttribute("Name"))
                                {
                                    _name = _line.GetAttribute("Name");
                                }
                                if (_line.HasAttribute("Corner1"))
                                {
                                    _corner1 = _line.GetAttribute("Corner1");
                                }
                                if (_line.HasAttribute("Corner2"))
                                {
                                    _corner2 = _line.GetAttribute("Corner2");
                                }
                                if (_line.HasAttribute("Circle"))
                                {
                                    _circle = _line.GetAttribute("Circle");
                                }
                                if (_line.HasAttribute("EntryMessage"))
                                {
                                    _entryMessage = _line.GetAttribute("EntryMessage");
                                }
                                if (_line.HasAttribute("ExitMessage"))
                                {
                                    _exitMessage = _line.GetAttribute("ExitMessage");
                                }
                                if (_line.HasAttribute("EntryCommand"))
                                {
                                    _entryCommand = _line.GetAttribute("EntryCommand");
                                }
                                if (_line.HasAttribute("ExitCommand"))
                                {
                                    _exitCommand = _line.GetAttribute("ExitCommand");
                                }
                                if (_line.HasAttribute("ReminderNotice"))
                                {
                                    _reminder = _line.GetAttribute("ReminderNotice");
                                }
                                if (_line.HasAttribute("PvPvE"))
                                {
                                    _pvpve = _line.GetAttribute("PvPvE");
                                }
                                if (_line.HasAttribute("NoZombie"))
                                {
                                    _noZ = _line.GetAttribute("NoZombie");
                                }
                                sw.WriteLine(string.Format("    <Zone Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Circle=\"{3}\" EntryMessage=\"{4}\" ExitMessage=\"{5}\" EntryCommand=\"{6}\" ExitCommand=\"{7}\" ReminderNotice=\"{8}\" PvPvE=\"{9}\" NoZombie=\"{10}\" />", _name, _corner1, _corner2, _circle, _entryMessage, _exitMessage, _entryCommand, _exitCommand, _reminder, _pvpve, _noZ));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Zone Name=\"\" Corner1=\"\" Corner2=\"\" Circle=\"\" EntryMessage=\"\" ExitMessage=\"\" EntryCommand=\"\" ExitCommand=\"\" ReminderNotice=\"\" PvPvE=\"\" NoZombie=\"\" /> -->");
                    }
                    sw.WriteLine("</Zones>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
