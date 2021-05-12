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
        public static string Command50 = "return";
        public static Dictionary<int, DateTime> Reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string[]> ZoneInfo = new Dictionary<int, string[]>();
        public static List<int> ZonePvE = new List<int>();
        public static List<string[]> Box1 = new List<string[]>();
        public static List<bool[]> Box2 = new List<bool[]>();
        private const string file = "Zones.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static Dictionary<int, string[]> zoneSetup1 = new Dictionary<int, string[]>();
        public static Dictionary<int, bool[]> zoneSetup2 = new Dictionary<int, bool[]>();

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
            if (!IsEnabled && IsRunning)
            {
                Box1.Clear();
                Box2.Clear();
                Reminder.Clear();
                ZoneInfo.Clear();
                ZonePvE.Clear();
                FileWatcher.Dispose();
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
            List<string[]> _addProtection = new List<string[]>();
            List<string[]> _removeProtection = new List<string[]>();
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Zone")
                {
                    Box1.Clear();
                    Box2.Clear();
                    Reminder.Clear();
                    ZoneInfo.Clear();
                    ZonePvE.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Zone' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Corner1"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing Corner1 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Corner2"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing Corner2 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Circle"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing Circle attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("EntryMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing EntryMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ExitMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing ExitMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing Response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ReminderNotice"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing ReminderNotice attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("PvE"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing PvE attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("NoZombie"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing NoZombie attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            string _circle = _line.GetAttribute("Circle");
                            if (!bool.TryParse(_circle, out bool _result1))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for Circle attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _pve = _line.GetAttribute("PvE");
                            if (!bool.TryParse(_pve, out bool _result2))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for PvE attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _noZ = _line.GetAttribute("NoZombie");
                            if (!bool.TryParse(_noZ, out bool _result3))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for NoZombie attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string[] box1 = { _line.GetAttribute("Corner1"), _line.GetAttribute("Corner2"), _line.GetAttribute("Name"), _line.GetAttribute("EntryMessage"), _line.GetAttribute("ExitMessage"),
                            _line.GetAttribute("Response"), _line.GetAttribute("ReminderNotice") };
                            if (box1[5] == "")
                            {
                                box1[5] = "**";
                            }
                            bool[] box2 = { _result1, _result2, _result3 };
                            if (!Box1.Contains(box1))
                            {
                                Box1.Add(box1);
                                Box2.Add(box2);
                                if (box2[3])
                                {
                                    string[] _vectors = new string[4];
                                    if (box2[0])
                                    {
                                        string[] _corner1 = box1[0].Split(',');
                                        _vectors[0] = _corner1[0];
                                        _vectors[1] = _corner1[2];
                                        _vectors[2] = box1[1];
                                    }
                                    else
                                    {
                                        string[] _corner1 = box1[0].Split(',');
                                        string[] _corner2 = box1[1].Split(',');
                                        _vectors[0] = _corner1[0];
                                        _vectors[1] = _corner1[2];
                                        _vectors[2] = _corner2[0];
                                        _vectors[3] = _corner2[2];
                                    }
                                    if (!_addProtection.Contains(_vectors))
                                    {
                                        _addProtection.Add(_vectors);
                                    }
                                }
                                else
                                {
                                    string[] _vectors = new string[4];
                                    if (box2[0])
                                    {
                                        string[] _corner1 = box1[0].Split(',');
                                        _vectors[0] = _corner1[0];
                                        _vectors[1] = _corner1[2];
                                        _vectors[2] = box1[1];
                                    }
                                    else
                                    {
                                        string[] _corner1 = box1[0].Split(',');
                                        string[] _corner2 = box1[1].Split(',');
                                        _vectors[0] = _corner1[0];
                                        _vectors[1] = _corner1[2];
                                        _vectors[2] = _corner2[0];
                                        _vectors[3] = _corner2[2];
                                    }
                                    if (!_removeProtection.Contains(_vectors))
                                    {
                                        _removeProtection.Add(_vectors);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Zones>");
                sw.WriteLine("    <Zone>");
                if (Box1.Count > 0)
                {
                    for (int i = 0; i < Box1.Count; i++)
                    {
                        string[] _box1 = Box1[i];
                        bool[] _box2 = Box2[i];
                        sw.WriteLine(string.Format("        <Zone Corner1=\"{0}\" Corner2=\"{1}\" Circle=\"{2}\" Name=\"{3}\" EntryMessage=\"{4}\" ExitMessage=\"{5}\" Response=\"{6}\" ReminderNotice=\"{7}\" PvE=\"{8}\" NoZombie=\"{9}\" />", _box1[0], _box1[1], _box2[0], _box1[2], _box1[3], _box1[4], _box1[5], _box1[6], _box2[1], _box2[2]));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <Zone Corner1=\"-8000,0,8000\" Corner2=\"8000,200,0\" Circle=\"false\" Name=\"North\" EntryMessage=\"You are entering the Northern side\" ExitMessage=\"You have exited the Northern Side\" Response=\"\" ReminderNotice=\"You are still in the North\" PvE=\"false\" NoZombie=\"false\" /> -->");
                    sw.WriteLine("        <!-- <Zone Corner1=\"-8000,0,-1\" Corner2=\"8000,200,-8000\" Circle=\"false\" Name=\"South\" EntryMessage=\"You are entering the Southern side\" ExitMessage=\"You have exited the Southern Side\" Response=\"whisper {PlayerName} you have entered the south side ^ ser {EntityId} 40 @ 4\" ReminderNotice=\"You are still in the South\" PvE=\"false\" NoZombie=\"false\" /> -->");
                    sw.WriteLine("        <!-- <Zone Corner1=\"-100,0,-90\" Corner2=\"40\" Circle=\"true\" Name=\"Market\" EntryMessage=\"You have entered the Market\" ExitMessage=\"You have exited the Market\" Response=\"whisper {PlayerName} you have entered the market\" ReminderNotice=\"\" PvE=\"true\" NoZombie=\"true\" /> -->");
                    sw.WriteLine("        <!-- <Zone Corner1=\"0,0,0\" Corner2=\"25,105,25\" Circle=\"false\" Name=\"Lobby\" EntryMessage=\"You have entered the Lobby\" ExitMessage=\"You have exited the Lobby\" Response=\"**\" ReminderNotice=\"You have been in the lobby for a long time...\" PvE=\"true\" NoZombie=\"true\" /> -->");
                }
                sw.WriteLine("    </Zone>");
                sw.WriteLine("</Zones>");
                sw.Flush();
                sw.Close();
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
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void ZoneCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            if (Box1.Count > 0)
            {
                for (int i = 0; i < Box1.Count; i++)
                {
                    string[] _box1 = Box1[i];
                    bool[] _box2 = Box2[i];
                    if (BoxCheck(_box1, _player.position.x, _player.position.y, _player.position.z, _box2))
                    {
                        if (ZoneInfo.ContainsKey(_player.entityId))
                        {
                            ZoneInfo.TryGetValue(_player.entityId, out string[] _info);
                            if (_info[1] != _box1[4])
                            {
                                if (Zone_Message)
                                {
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _box1[3] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (_box1[5] != "**" && _box1[5] != "")
                                {
                                    Response(_cInfo, _box1[5]);
                                }
                                _info[0] = _box1[2];
                                _info[1] = _box1[4];
                                _info[2] = _box1[6];
                                ZoneInfo[_player.entityId] = _info;
                                Reminder[_player.entityId] = DateTime.Now;
                                if (_box2[1])
                                {
                                    if (!ZonePvE.Contains(_player.entityId))
                                    {
                                        ZonePvE.Add(_player.entityId);
                                    }
                                }
                                else if (ZonePvE.Contains(_player.entityId))
                                {
                                    ZonePvE.Remove(_player.entityId);
                                }
                            }
                        }
                        else
                        {
                            if (Zone_Message)
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _box1[3] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            if (_box1[5] != "**" && _box1[5] != "")
                            {
                                Response(_cInfo, _box1[5]);
                            }
                            string[] _zoneInfo = { _box1[2], _box1[4], _box1[6] };
                            ZoneInfo.Add(_player.entityId, _zoneInfo);
                            Reminder.Add(_player.entityId, DateTime.Now);
                            if (_box2[1])
                            {
                                ZonePvE.Add(_player.entityId);
                            }
                        }
                        return;
                    }
                }
                if (ZoneInfo.ContainsKey(_player.entityId))
                {
                    if (Zone_Message)
                    {
                        if (ZoneInfo.TryGetValue(_player.entityId, out string[] _msg))
                        {
                            if (_msg[1] != "")
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _msg[1] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    ZoneInfo.Remove(_player.entityId);
                    Reminder.Remove(_player.entityId);
                    if (ZonePvE.Contains(_player.entityId))
                    {
                        ZonePvE.Remove(_player.entityId);
                    }
                }
            }
        }

        public static void ReturnToPosition(ClientInfo _cInfo)
        {
            DateTime _respawnTime = PersistentContainer.Instance.Players[_cInfo.playerId].ZoneDeathTime;
            TimeSpan varTime = DateTime.Now - _respawnTime;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
                    {
                        Time(_cInfo, _timepassed, 6);
                        return;
                    }
                }
            }
            Time(_cInfo, _timepassed, 3);
        }

        public static void Time(ClientInfo _cInfo, int _timePassed, int _timeAllowed)
        {
            if (_timePassed <= _timeAllowed)
            {
                if (Victim.TryGetValue(_cInfo.entityId, out string _deathPos))
                {
                    int x, y, z;
                    string[] _cords = _deathPos.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    Victim.Remove(_cInfo.entityId);
                }
            }
            else
            {
                Victim.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue(321, out string _phrase321);
                _phrase321 = _phrase321.Replace("{PlayerName}", _cInfo.playerName);
                _phrase321 = _phrase321.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase321 = _phrase321.Replace("{Command50}", Zones.Command50);
                _phrase321 = _phrase321.Replace("{Time}", _timeAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase321 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Response(ClientInfo _cInfo, string _response)
        {
            if (_response == "**" || _response == "")
            {
                return;
            }
            else if (_response.Contains("^"))
            {
                string[] _responseSplit = _response.Split('^');
                for (int i = 0; i < _responseSplit.Length; i++)
                {
                    string _responseAdj = _responseSplit[i].Trim();
                    _responseAdj = _responseAdj.Replace("{EntityId}", _cInfo.entityId.ToString());
                    _responseAdj = _responseAdj.Replace("{SteamId}", _cInfo.playerId);
                    _responseAdj = _responseAdj.Replace("{PlayerName}", _cInfo.playerName);
                    if (_responseAdj.ToLower().StartsWith("global "))
                    {
                        _responseAdj = _responseAdj.Replace("Global ", "");
                        _responseAdj = _responseAdj.Replace("global ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _responseAdj + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                    else if (_responseAdj.ToLower().StartsWith("whisper "))
                    {
                        _responseAdj = _responseAdj.Replace("Whisper ", "");
                        _responseAdj = _responseAdj.Replace("whisper ", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _responseAdj + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        SdtdConsole.Instance.ExecuteSync(_responseAdj, null);
                    }
                }
            }
            else
            {
                _response = _response.Replace("{EntityId}", _cInfo.entityId.ToString());
                _response = _response.Replace("{SteamId}", _cInfo.playerId);
                _response = _response.Replace("{PlayerName}", _cInfo.playerName);
                if (_response.ToLower().StartsWith("global "))
                {
                    _response = _response.Replace("Global ", "");
                    _response = _response.Replace("global ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _response + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (_response.ToLower().StartsWith("whisper "))
                {
                    _response = _response.Replace("Whisper ", "");
                    _response = _response.Replace("whisper ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _response + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_response, null);
                }
            }
        }

        public static bool BoxCheck(string[] _box, float _X, float _Y, float _Z, bool[] _box2)
        {
            string[] _corner1 = _box[0].Split(',');
            float.TryParse(_corner1[0], out float xMin);
            float.TryParse(_corner1[1], out float yMin);
            float.TryParse(_corner1[2], out float zMin);
            if (_box2[0])
            {
                if (int.TryParse(_box[1], out int _radius))
                {
                    if (VectorCircle(xMin, zMin, _X, _Z, _radius))
                    {
                        return true;
                    }
                }
            }
            else
            {
                string[] _corner2 = _box[1].Split(',');
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
            if (xMin >= 0 && xMax >= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax <= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax >= 0)
            {
                if (_X < xMin || _X > xMax)
                {
                    return false;
                }
            }
            else if (xMin >= 0 && xMax <= 0)
            {
                if (_X > xMin || _X < xMax)
                {
                    return false;
                }
            }
            if (yMin >= 0 && yMax >= 0)
            {
                if (yMin < yMax)
                {
                    if (_Y < yMin || _Y > yMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Y > yMin || _Y < yMax)
                    {
                        return false;
                    }
                }
            }
            else if (yMin <= 0 && yMax <= 0)
            {
                if (yMin < yMax)
                {
                    if (_Y < yMin || _Y > yMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Y > yMin || _Y < yMax)
                    {
                        return false;
                    }
                }
            }
            else if (yMin <= 0 && yMax >= 0)
            {
                if (_Y < yMin || _Y > yMax)
                {
                    return false;
                }
            }
            else if (yMin >= 0 && yMax <= 0)
            {
                if (_Y > yMin || _Y < yMax)
                {
                    return false;
                }
            }
            if (zMin >= 0 && zMax >= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax <= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax >= 0)
            {
                if (_Z < zMin || _Z > zMax)
                {
                    return false;
                }
            }
            else if (zMin >= 0 && zMax <= 0)
            {
                if (_Z > zMin || _Z < zMax)
                {
                    return false;
                }
            }
            return true;
        }

        public static void HostileCheck()
        {
            try
            {
                if (Box1.Count > 0)
                {
                    for (int i = 0; i < Box1.Count; i++)
                    {
                        string[] _box1 = Box1[i];
                        bool[] _box2 = Box2[i];
                        if (_box2[2])
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
                                        string[] _corner1 = _box1[0].Split(',');
                                        int.TryParse(_corner1[0], out int _xMin);
                                        int.TryParse(_corner1[1], out int _yMin);
                                        int.TryParse(_corner1[2], out int _zMin);
                                        if (_box2[0])
                                        {
                                            if (int.TryParse(_box1[1], out int _radius))
                                            {
                                                if (VectorCircle(_xMin, _zMin, _X, _Z, _radius))
                                                {
                                                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from zone {1} @ {2} {3} {4}", _name, _box1[2], _X, _Y, _Z));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string[] _corner2 = _box1[1].Split(',');
                                            int.TryParse(_corner2[0], out int _xMax);
                                            int.TryParse(_corner2[1], out int _yMax);
                                            int.TryParse(_corner2[2], out int _zMax);
                                            if (VectorBox(_xMin, _yMin, _zMin, _xMax, _yMax, _zMax, _X, _Y, _Z))
                                            {
                                                string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from zone {1} @ {2} {3} {4}", _name, _box1[2], _X, _Y, _Z));
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
                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(time.Key);
                    if (_cInfo != null)
                    {
                        DateTime _dt = time.Value;
                        TimeSpan varTime = DateTime.Now - _dt;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= Reminder_Delay)
                        {
                            ZoneInfo.TryGetValue(_cInfo.entityId, out string[] _info);
                            if (_info != null && _info[2] != "")
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _info[2] + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
    }
}
