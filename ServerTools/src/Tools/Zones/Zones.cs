using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Zones
    {
        public static bool IsEnabled = false, IsRunning = false, Zone_Message = false, Set_Home = false, BuffProtection = false;
        public static string Reminder_Delay = "20";

        public static Dictionary<int, DateTime> Reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string[]> ZonePlayer = new Dictionary<int, string[]>();
        public static List<string[]> ZoneList = new List<string[]>();
        public static List<Bounds> ZoneBounds = new List<Bounds>();
        public static Dictionary<int, string[]> ZoneSetup = new Dictionary<int, string[]>();

        private static string EventDelay = "";
        private const string file = "Zones.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static readonly System.Random Random = new System.Random();

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            ZoneList.Clear();
            ZoneBounds.Clear();
            Reminder.Clear();
            ZonePlayer.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (BuffManager.Buffs.ContainsKey("pve_zone") && BuffManager.Buffs.ContainsKey("pvp_damage"))
                {
                    BuffProtection = true;
                }
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
                ZoneList.Clear();
                ZoneBounds.Clear();
                Reminder.Clear();
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
                        if (line.HasAttribute("Name") && line.HasAttribute("Corner1") && line.HasAttribute("Corner2") && line.HasAttribute("Circle") &&
                            line.HasAttribute("EntryMessage") && line.HasAttribute("ExitMessage") && line.HasAttribute("EntryCommand") && line.HasAttribute("ExitCommand") &&
                            line.HasAttribute("ReminderNotice") && line.HasAttribute("PvPvE") && line.HasAttribute("NoZombie"))
                        {
                            string name = line.GetAttribute("Name");
                            if (name == "")
                            {
                                continue;
                            }
                            string[] zone = { name, line.GetAttribute("Corner1"), line.GetAttribute("Corner2"), line.GetAttribute("Circle"),
                                line.GetAttribute("EntryMessage"), line.GetAttribute("ExitMessage"), line.GetAttribute("EntryCommand"), line.GetAttribute("ExitCommand"),
                                line.GetAttribute("ReminderNotice"), line.GetAttribute("PvPvE"), line.GetAttribute("NoZombie") };
                            if (zone[3].ToLower() == "false")
                            {
                                if (!zone[1].Contains(",") || !zone[2].Contains(","))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Zones.xml entry. Improper format in corner1 or corner2 attribute: {0}", line.OuterXml));
                                    continue;
                                }
                                string[] corner1 = zone[1].Split(',');
                                string[] corner2 = zone[2].Split(',');
                                int.TryParse(corner1[0], out int x1);
                                int.TryParse(corner1[1], out int y1);
                                int.TryParse(corner1[2], out int z1);
                                int.TryParse(corner2[0], out int x2);
                                int.TryParse(corner2[1], out int y2);
                                int.TryParse(corner2[2], out int z2);
                                Vector3 vector1 = new Vector3((x1 <= x2) ? x1 : x2, (y1 <= y2) ? y1 : y2, (z1 <= z2) ? z1 : z2);
                                Vector3 vector2 = new Vector3((x1 <= x2) ? x2 : x1, (y1 <= y2) ? y2 : y1, (z1 <= z2) ? z2 : z1);
                                zone[1] = vector1.x + "," + vector1.y + "," + vector1.z;
                                zone[2] = vector2.x + "," + vector2.y + "," + vector2.z;
                                Bounds bounds = new Bounds();
                                bounds.SetMinMax(vector1, vector2);
                                if (!ZoneList.Contains(zone))
                                {
                                    ZoneList.Add(zone);
                                    if (!ZoneBounds.Contains(bounds))
                                    {
                                        ZoneBounds.Add(bounds);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Zones.LoadXml: {0}", e.Message));
                }
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
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not use decimals in the corner positions -->");
                    sw.WriteLine("    <!-- Overlapping zones: the first zone listed that is overlapping will take priority -->");
                    sw.WriteLine("    <!-- PvPvE: 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone -->");
                    sw.WriteLine("    <!-- EntryCommand and ExitCommand trigger console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("    <!-- Possible variables for commands include {PlayerName}, {EntityId}, {Id}, {EOS}, {Delay}, whisper, global -->");
                    sw.WriteLine("    <!-- <Zone Name=\"Example\" Corner1=\"1,2,3\" Corner2=\"-3,4,-5\" Circle=\"false\" EntryMessage=\"You have entered example\" ExitMessage=\"You have exited example\" EntryCommand=\"whisper This is a pve space\" ExitCommand=\"\" ReminderNotice=\"You are still in example\" PvPvE=\"0\" NoZombie=\"True\" /> -->");
                    sw.WriteLine("    <Zone Name=\"\" Corner1=\"\" Corner2=\"\" Circle=\"\" EntryMessage=\"\" ExitMessage=\"\" EntryCommand=\"\" ExitCommand=\"\" ReminderNotice=\"\" PvPvE=\"\" NoZombie=\"\" />");
                    if (ZoneList.Count > 0)
                    {
                        for (int i = 0; i < ZoneList.Count; i++)
                        {
                            string[] zone = ZoneList[i];
                            sw.WriteLine("    <Zone Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Circle=\"{3}\" EntryMessage=\"{4}\" ExitMessage=\"{5}\" EntryCommand=\"{6}\" ExitCommand=\"{7}\" ReminderNotice=\"{8}\" PvPvE=\"{9}\" NoZombie=\"{10}\" />", zone[0], zone[1], zone[2], zone[3], zone[4], zone[5], zone[6], zone[7], zone[8], zone[9], zone[10]);
                        }
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Reminder_Delay || _reset)
            {
                EventDelay = Reminder_Delay;
                EventSchedule.Clear("Zones_");
                if (int.TryParse(Reminder_Delay, out int delay))
                {
                    DateTime time = DateTime.Now.AddMinutes(delay);
                    EventSchedule.Schedule.Add("Zones_" + time, time);
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Invalid Zones Reminder_Delay detected. Use a single integer"));
                    Log.Out(string.Format("[SERVERTOOLS] Example: 20 or 40"));
                }
            }
        }

        public static void ZoneCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            try
            {
                for (int i = 0; i < ZoneList.Count; i++)
                {
                    string[] zone = ZoneList[i];
                    if (InsideZone(ZoneBounds[i], zone, _player.position.x, _player.position.y, _player.position.z))
                    {
                        if (ZonePlayer.ContainsKey(_player.entityId))
                        {
                            ZonePlayer.TryGetValue(_player.entityId, out string[] info);
                            if (info != zone)
                            {
                                ZonePlayer[_player.entityId] = zone;
                                Reminder[_player.entityId] = DateTime.Now;
                                NewZone(_cInfo, zone);
                            }
                        }
                        else
                        {
                            ZonePlayer.Add(_player.entityId, zone);
                            Reminder.Add(_player.entityId, DateTime.Now);
                            NewZone(_cInfo, zone);
                        }
                        return;
                    }
                }
                if (ZonePlayer.ContainsKey(_player.entityId))
                {
                    ZonePlayer.TryGetValue(_player.entityId, out string[] zone);
                    ZonePlayer.Remove(_player.entityId);
                    Reminder.Remove(_player.entityId);
                    RemoveZone(_cInfo, zone);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ZoneCheck: {0}", e.Message));
            }
        }

        public static void NewZone(ClientInfo _cInfo, string[] _zone)
        {
            if (BuffProtection)
            {
                switch (_zone[9])
                {
                    case "0":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pve_zone"), null);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 0", true));
                        break;
                    case "1":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_ally_zone"), null);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 1", true));
                        break;
                    case "2":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_stranger_zone"), null);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 2", true));
                        break;
                    case "3":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_zone"), null);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 3", true));
                        break;
                }
            }
            if (Zone_Message && _zone[4] != "")
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[4] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            if (_zone[6] != "")
            {
                ProcessCommand(_cInfo, _zone[6]);
            }
        }

        public static void RemoveZone(ClientInfo _cInfo, string[] _zone)
        {
            if (BuffProtection)
            {
                switch (_zone[9])
                {
                    case "0":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pve_zone"), null);
                        break;
                    case "1":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_ally_zone"), null);
                        break;
                    case "2":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_stranger_zone"), null);
                        break;
                    case "3":
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "pvp_zone"), null);
                        break;
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sgs PlayerKillingMode {0}", GeneralOperations.Player_Killing_Mode), true));
            }
            if (Zone_Message && _zone[5] != "")
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _zone[5] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            if (_zone[7] != "")
            {
                ProcessCommand(_cInfo, _zone[7]);
            }
        }

        public static void ProcessCommand(ClientInfo _cInfo, string _command)
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
                                Timers.Zone_SingleUseTimer(_time, _cInfo.CrossplatformId.CombinedString, _commands);
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
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_playerId);
                if (cInfo != null)
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
                                Timers.Zone_SingleUseTimer(_time, cInfo.CrossplatformId.CombinedString, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Zone command error. Unable to commit delay with improper integer: {0}", _commands));
                            }
                        }
                        else
                        {
                            Command(cInfo, _commandTrimmed);
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
                if (_command.Contains("{EntityId}"))
                {
                    _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                }
                if (_command.Contains("{Id}"))
                {
                    _command = _command.Replace("{Id}", _cInfo.PlatformId.CombinedString);
                }
                if (_command.Contains("{EOS}"))
                {
                    _command = _command.Replace("{EOS}", _cInfo.CrossplatformId.CombinedString);
                }
                if (_command.Contains("{PlayerName}"))
                {
                    _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                }
                if (_command.Contains("{RandomId}"))
                {
                    List<ClientInfo> clientList = GeneralOperations.ClientList();
                    if (clientList != null)
                    {
                        ClientInfo cInfo2 = clientList.ElementAt(Random.Next(clientList.Count));
                        if (cInfo2 != null)
                        {
                            _command = _command.Replace("{RandomId}", cInfo2.PlatformId.CombinedString);
                        }
                    }
                }
                if (_command.Contains("{RandomEOS}"))
                {
                    List<ClientInfo> clientList = GeneralOperations.ClientList();
                    if (clientList != null)
                    {
                        ClientInfo cInfo2 = clientList.ElementAt(Random.Next(clientList.Count));
                        if (cInfo2 != null)
                        {
                            _command = _command.Replace("{RandomEOS}", cInfo2.CrossplatformId.CombinedString);
                        }
                    }
                }
                if (_command.Contains("global "))
                {
                    _command = _command.Replace("global ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (_command.StartsWith("whisper "))
                {
                    _command = _command.Replace("whisper ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (_command.StartsWith("tp "))
                {
                    _command = _command.Replace("tp ", "tele ");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null);
                }
                else if (_command.StartsWith("teleportplayer "))
                {
                    _command = _command.Replace("teleportplayer ", "tele ");
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null);
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.Command: {0}", e.Message));
            }
        }

        public static bool InsideZone(Bounds _bounds, string[] _zone, float _X, float _Y, float _Z)
        {
            if (_zone[3].ToLower() == "true")
            {
                string[] _corner1 = _zone[1].Split(',');
                float.TryParse(_corner1[0], out float xMin);
                float.TryParse(_corner1[2], out float zMin);
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
                if (VectorBox(_bounds, new Vector3(_X, _Y, _Z)))
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

        public static bool VectorBox(Bounds _bounds, Vector3 _position)
        {

            if (_bounds.Contains(_position))
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
                    List<Entity> entities = GameManager.Instance.World.Entities.list;
                    int entityCount = entities.Count;
                    for (int i = 0; i < entityCount; i++)
                    {
                        if (entities[i] == null)
                        {
                            continue;
                        }
                        Entity entity = entities[i];
                        if (!entity.IsMarkedForUnload() && (entity is EntityZombie || entity is EntityEnemyAnimal || entity is EntityVulture))
                        {
                            int zoneCount = ZoneList.Count;
                            for (int j = 0; j < zoneCount; j++)
                            {
                                if (ZoneList[j][10].ToLower() == "true" && InsideZone(ZoneBounds[j], ZoneList[j], entity.position.x, entity.position.y, entity.position.z))
                                {
                                    entity.MarkToUnload();
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
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(time.Key);
                    if (cInfo != null)
                    {
                        DateTime dt = time.Value;
                        TimeSpan varTime = DateTime.Now - dt;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        int.TryParse(Reminder_Delay, out int delay);
                        if (timepassed >= delay)
                        {
                            ZonePlayer.TryGetValue(cInfo.entityId, out string[] zone);
                            if (zone != null && zone[8] != "")
                            {
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + zone[8] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Reminder[cInfo.entityId] = DateTime.Now;
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

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Zones>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- Do not use decimals in the corner positions -->");
                    sw.WriteLine("    <!-- Overlapping zones: the first zone listed that is overlapping will take priority -->");
                    sw.WriteLine("    <!-- PvPvE: 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone -->");
                    sw.WriteLine("    <!-- EntryCommand and ExitCommand trigger console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("    <!-- Possible variables for commands include {PlayerName}, {EntityId}, {Id}, {EOS}, {Delay}, whisper, global -->");
                    sw.WriteLine("    <!-- <Zone Name=\"Example\" Corner1=\"1,2,3\" Corner2=\"-3,4,-5\" Circle=\"false\" EntryMessage=\"You have entered example\" ExitMessage=\"You have exited example\" EntryCommand=\"whisper This is a pve space\" ExitCommand=\"\" ReminderNotice=\"You are still in example\" PvPvE=\"0\" NoZombie=\"True\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- Do not use") &&
                            !nodeList[i].OuterXml.Contains("<!-- Overlapping zones") && !nodeList[i].OuterXml.Contains("<!-- PvPvE") &&
                            !nodeList[i].OuterXml.Contains("<!-- EntryCommand") && !nodeList[i].OuterXml.Contains("<!-- Possible") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Zone Name=\"Example\"") && !nodeList[i].OuterXml.Contains("<Zone Name=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Zone Name=\"\" Corner1=\"\" Corner2=\"\" Circle=\"\" EntryMessage=\"\" ExitMessage=\"\" EntryCommand=\"\" ExitCommand=\"\" ReminderNotice=\"\" PvPvE=\"\" NoZombie=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Zone")
                            {
                                string name = "", corner1 = "", corner2 = "", circle = "", entryMessage = "", exitMessage = "", entryCommand = "",
                                    exitCommand = "", reminder = "", pvpve = "", noZ = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Corner1"))
                                {
                                    corner1 = line.GetAttribute("Corner1");
                                }
                                if (line.HasAttribute("Corner2"))
                                {
                                    corner2 = line.GetAttribute("Corner2");
                                }
                                if (line.HasAttribute("Circle"))
                                {
                                    circle = line.GetAttribute("Circle");
                                }
                                if (line.HasAttribute("EntryMessage"))
                                {
                                    entryMessage = line.GetAttribute("EntryMessage");
                                }
                                if (line.HasAttribute("ExitMessage"))
                                {
                                    exitMessage = line.GetAttribute("ExitMessage");
                                }
                                if (line.HasAttribute("EntryCommand"))
                                {
                                    entryCommand = line.GetAttribute("EntryCommand");
                                }
                                if (line.HasAttribute("ExitCommand"))
                                {
                                    exitCommand = line.GetAttribute("ExitCommand");
                                }
                                if (line.HasAttribute("ReminderNotice"))
                                {
                                    reminder = line.GetAttribute("ReminderNotice");
                                }
                                if (line.HasAttribute("PvPvE"))
                                {
                                    pvpve = line.GetAttribute("PvPvE");
                                }
                                if (line.HasAttribute("NoZombie"))
                                {
                                    noZ = line.GetAttribute("NoZombie");
                                }
                                sw.WriteLine(string.Format("    <Zone Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Circle=\"{3}\" EntryMessage=\"{4}\" ExitMessage=\"{5}\" EntryCommand=\"{6}\" ExitCommand=\"{7}\" ReminderNotice=\"{8}\" PvPvE=\"{9}\" NoZombie=\"{10}\" />", name, corner1, corner2, circle, entryMessage, exitMessage, entryCommand, exitCommand, reminder, pvpve, noZ));
                            }
                        }
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
