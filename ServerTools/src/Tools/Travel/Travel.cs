using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Travel
    {
        public static bool IsEnabled = false, IsRunning = false, Player_Check = false, Zombie_Check = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command_travel = "travel";

        private static SortedDictionary<string, string[]> Dict = new SortedDictionary<string, string[]>();
        private static SortedDictionary<string, string> Destination = new SortedDictionary<string, string>();
        private const string file = "TravelLocations.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Destination.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
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
                Dict.Clear();
                Destination.Clear();
                bool upgrade = true;
                for (int i = 0; i < _childNodes.Count; i++)
                {
                    if (_childNodes[i].NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    XmlElement _line = (XmlElement)_childNodes[i];
                    if (_line.HasAttributes)
                    {
                        if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                        {
                            upgrade = false;
                        }
                        else if (_line.HasAttribute("Name") && _line.HasAttribute("Corner1") && _line.HasAttribute("Corner2") && _line.HasAttribute("Destination"))
                        {
                            string _name = _line.GetAttribute("Name");
                            string[] _corner1 = _line.GetAttribute("Corner1").Split(',');
                            string[] _corner2 = _line.GetAttribute("Corner2").Split(',');
                            string _destination = _line.GetAttribute("Destination");
                            int.TryParse(_corner1[0], out int _x1);
                            int.TryParse(_corner1[1], out int _y1);
                            int.TryParse(_corner1[2], out int _z1);
                            int.TryParse(_corner2[0], out int _x2);
                            int.TryParse(_corner2[1], out int _y2);
                            int.TryParse(_corner2[2], out int _z2);
                            if (_x1 > _x2)
                            {
                                int _switch = _x2;
                                _x2 = _x1;
                                _x1 = _switch;
                            }
                            if (_y1 > _y2)
                            {
                                int _switch = _y2;
                                _y2 = _y1;
                                _y1 = _switch;
                            }
                            if (_y1 == _y2)
                            {
                                _y2++;
                            }
                            if (_z1 > _z2)
                            {
                                int _switch = _z2;
                                _z2 = _z1;
                                _z1 = _switch;
                            }
                            string _c1 = _x1 + "," + _y1 + "," + _z1;
                            string _c2 = _x2 + "," + _y2 + "," + _z2;
                            string[] box = { _c1, _c2, _destination };
                            if (!Dict.ContainsKey(_name))
                            {
                                Dict.Add(_name, box);
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    UpgradeXml(_childNodes);
                    return;
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Travel>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Location Name=\"zone1\" Corner1=\"0,100,0\" Corner2=\"10,100,10\" Destination=\"-100,-1,-100\" /> -->");
                    sw.WriteLine("<!-- <Location Name=\"zone2\" Corner1=\"-1,100,-1\" Corner2=\"-10,100,-10\" Destination=\"100,-1,100\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvpBox in Dict)
                        {
                            sw.WriteLine(string.Format("    <Location Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Destination=\"{3}\" />", kvpBox.Key, kvpBox.Value[0], kvpBox.Value[1], kvpBox.Value[2]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Location Name=\"\" Corner1=\"\" Corner2=\"\" Destination=\"\" />");
                    }
                    sw.WriteLine("</Travel>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.UpdateXml: {0}", e.Message));
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

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Delay_Between_Uses < 1)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        Tele(_cInfo);
                    }
                }
                else
                {
                    DateTime _lastTravel = PersistentContainer.Instance.Players[_cInfo.playerId].LastTravel;
                    TimeSpan varTime = DateTime.Now - _lastTravel;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                            if (DateTime.Now < _dt)
                            {
                                int _delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _timepassed, _delay);
                                return;
                            }
                        }
                    }
                    Time(_cInfo, _timepassed, Delay_Between_Uses);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Exec: {0}", e.Message));
            }
        }

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        Tele(_cInfo);
                    }
                }
                else
                {
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Travel3", out string _phrase);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_travel}", Command_travel);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Time: {0}", e.Message));
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            try
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    Tele(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Travel4", out string _phrase);
                    _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.CommandCost: {0}", e.Message));
            }
        }

        public static void Tele(ClientInfo _cInfo)
        {
            try
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        if (Dict.Count > 0)
                        {
                            int _x = (int)_player.position.x;
                            int _y = (int)_player.position.y;
                            int _z = (int)_player.position.z;
                            foreach (KeyValuePair<string, string[]> _travel in Dict)
                            {
                                string[] _c1 = _travel.Value[0].Split(',');
                                int.TryParse(_c1[0], out int _x1);
                                int.TryParse(_c1[1], out int _y1);
                                int.TryParse(_c1[2], out int _z1);
                                string[] _c2 = _travel.Value[1].Split(',');
                                int.TryParse(_c2[0], out int _x2);
                                int.TryParse(_c2[1], out int _y2);
                                int.TryParse(_c2[2], out int _z2);
                                if (_x >= _x1 && _x <= _x2 && _y >= _y1 && _y <= _y2 && _z >= _z1 && _z <= _z2)
                                {
                                    if (Player_Check)
                                    {
                                        if (Teleportation.PCheck(_cInfo, _player))
                                        {
                                            return;
                                        }
                                    }
                                    if (Zombie_Check)
                                    {
                                        if (Teleportation.ZCheck(_cInfo, _player))
                                        {
                                            return;
                                        }
                                    }
                                    string[] _destination = _travel.Value[2].Split(',');
                                    int.TryParse(_destination[0], out int _destinationX);
                                    int.TryParse(_destination[1], out int _destinationY);
                                    int.TryParse(_destination[2], out int _destinationZ);
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_destinationX, _destinationY, _destinationZ), null, false));
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                                    }
                                    PersistentContainer.Instance.Players[_cInfo.playerId].LastTravel = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                    Phrases.Dict.TryGetValue("Travel1", out string _phrase);
                                    _phrase = _phrase.Replace("{Destination}", _travel.Value[2]);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                            Phrases.Dict.TryGetValue("Travel2", out string _phrase1);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.Tele: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Travel>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Location Name=\"Zone1\" Corner1=\"0,100,0\" Corner2=\"10,100,10\" Destination=\"-100,-1,-100\" /> -->");
                    sw.WriteLine("<!-- <Location Name=\"Zone2\" Corner1=\"-1,100,-1\" Corner2=\"-10,100,-10\" Destination=\"100,-1,100\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Location")
                        {
                            string _name = "", _c1 = "", _c2 = "", _destination = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("Corner1"))
                            {
                                _c1 = _line.GetAttribute("Corner1");
                            }
                            if (_line.HasAttribute("Corner2"))
                            {
                                _c2 = _line.GetAttribute("Corner2");
                            }
                            if (_line.HasAttribute("Destination"))
                            {
                                _destination = _line.GetAttribute("Destination");
                            }
                            sw.WriteLine(string.Format("    <Location Name=\"{0}\" Corner1=\"{1}\" Corner2=\"{2}\" Destination=\"{3}\" />", _name, _c1, _c2, _destination));
                        }
                    }
                    sw.WriteLine("</Travel>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Travel.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}