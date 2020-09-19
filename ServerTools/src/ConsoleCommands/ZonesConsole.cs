using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class ZonesConsole : ConsoleCmdAbstract
    {
        public static Dictionary<int, int> newZone = new Dictionary<int, int>();

        public override string GetDescription()
        {
            return "[ServerTools] - Enable, setup a new zone or list existing zones.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Zones off\n" +
                   "  2. Zones on\n" +
                   "  3. Zones new <name>\n" +
                   "  4. Zones list\n" +
                   "  5. Zones delete <number>\n" +
                   "  6. Zones save\n" +
                   "  7. Zones back\n" +
                   "1. Turn off zone tool\n" +
                   "2. Turn on zone tool\n" +
                   "3. Create a new zone\n" +
                   "4. Shows the zone list\n" +
                   "5. Deletes the specific zone from the list\n" +
                   "6. Use this throughout the setup process when prompted\n" +
                   "7. Use this to go back a step in the setup process\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Zones", "zones", "st-zones"};
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                var _cInfo = _senderInfo.RemoteClientInfo;
                if (_cInfo != null)
                {
                    if (_params[0].ToLower().Equals("off"))
                    {
                        if (Zones.IsEnabled)
                        {
                            Zones.IsEnabled = false;
                            LoadConfig.WriteXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zones has been set to off"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zones is already off"));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("on"))
                    {
                        if (!Zones.IsEnabled)
                        {
                            Zones.IsEnabled = true;
                            LoadConfig.WriteXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zones has been set to on"));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zones is already on"));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("new"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            newZone[_cInfo.entityId] = 1;
                        }
                        else
                        {
                            newZone.Add(_cInfo.entityId, 1);
                        }
                        if (Zones.zoneSetup1.ContainsKey(_cInfo.entityId))
                        {
                            Zones.zoneSetup1.Remove(_cInfo.entityId);
                        }
                        if (Zones.zoneSetup2.ContainsKey(_cInfo.entityId))
                        {
                            Zones.zoneSetup2.Remove(_cInfo.entityId);
                        }
                        string[] _strings = new string[7];
                        bool[] _bools = new bool[4];
                        _params.RemoveAt(0);
                        string _name = string.Join(" ", _params);
                        _strings[2] = _name;
                        Zones.zoneSetup1.Add(_cInfo.entityId, _strings);
                        Zones.zoneSetup2.Add(_cInfo.entityId, _bools);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Started a new zone setup. Zone name set to {0}.", _name));
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the first corner of the zone and type zones save."));
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] If you would like a circle, stand in the middle of the circle and type zones circle."));
                    }
                    else if (_params[0].ToLower().Equals("list"))
                    {
                        if (Zones.Box1.Count > 0)
                        {
                            for (int i = 0; i < Zones.Box1.Count; i++)
                            {
                                string[] _box = Zones.Box1[i];
                                bool[] _box2 = Zones.Box2[i];
                                if (_box != null)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Zone number {0}:", i));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _box[0]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _box[1]));
                                    SdtdConsole.Instance.Output(string.Format("Circle = {0}", _box2[0]));
                                    SdtdConsole.Instance.Output(string.Format("Name = {0}", _box[2]));
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _box[3]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _box[4]));
                                    SdtdConsole.Instance.Output(string.Format("Response = {0}", _box[5]));
                                    SdtdConsole.Instance.Output(string.Format("Reminder Notice = {0}", _box[6]));
                                    SdtdConsole.Instance.Output(string.Format("PvE = {0}", _box2[1]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _box2[2]));
                                    SdtdConsole.Instance.Output(string.Format("Protected = {0}", _box2[3]));
                                    SdtdConsole.Instance.Output(string.Format(""));
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no zones setup."));
                        }
                    }
                    else if (_params[0].ToLower().Equals("delete"))
                    {
                        if (Zones.Box1.Count > 0)
                        {
                            if (int.TryParse(_params[1], out int _number))
                            {
                                if (Zones.Box1.Count >= _number)
                                {
                                    string[] _box1 = Zones.Box1[_number];
                                    bool[] _box2 = Zones.Box2[_number];
                                    Zones.Box1.RemoveAt(_number);
                                    Zones.Box2.RemoveAt(_number);
                                    Zones.UpdateXml();
                                    string[] _vectors = new string[4];
                                    if (_box2[0])
                                    {
                                        if (_box2[3])
                                        {
                                            string[] _corner1 = _box1[0].Split(',');
                                            List<string[]> _protected = new List<string[]>();
                                            _protected.Add(new string[] { _corner1[0], _corner1[2], _box1[1] });
                                            Zones.RemoveProtectedZones(_protected);
                                        }
                                    }
                                    else
                                    {
                                        if (_box2[3])
                                        {
                                            string[] _corner1 = _box1[0].Split(',');
                                            string[] _corner2 = _box1[1].Split(',');
                                            List<string[]> _protected = new List<string[]>();
                                            _protected.Add(new string[] { _corner1[0], _corner1[2], _corner2[0], _corner2[2] });
                                            Zones.RemoveProtectedZones(_protected);
                                        }
                                    }
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed zone entry number {0} from the list", _number));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Could not delete number {0} from the list. Entry not found", _number));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid format or non numeric entry. Type zones delete <number> from the list of zones."));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no zones setup."));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("circle"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            newZone.TryGetValue(_cInfo.entityId, out int _stage);
                            if (_stage == 1)
                            {
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                _bools[0] = true;
                                Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[0] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Saved the zone as a circle. Circle center point = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zones circle <number> to set the amount of blocks from center the zone will reach."));
                            }
                            else if (_stage == 2)
                            {
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                if (_bools[0])
                                {
                                    if (int.TryParse(_params[1], out int _radius))
                                    {
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                        _strings[1] = _radius.ToString();
                                        Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                        newZone[_cInfo.entityId] = 3;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Saved the circular zone radius to {0} blocks.", _radius));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zones save \"entry message\". This is the message players receive upon entering the zone."));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid radius for circlular zone, try again."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This zone is not setup as a circle, go back by typing zones back."));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Start a new zone setup or go back to the circle setup if you need to change it by typing zones back."));
                            }
                        }
                    }
                    else if (_params[0].ToLower().Equals("save"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            newZone.TryGetValue(_cInfo.entityId, out int _stage);
                            if (_stage == 1)
                            {
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                _bools[0] = false;
                                Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[0] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 saved. Stand in the opposite corner of the zone and type zones save."));
                            }
                            if (_stage == 2)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[1] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 3;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 2 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 2 saved. Type zones save \"entry message\". This is the message players receive upon entering the zone."));
                            }
                            else if (_stage == 3)
                            {
                                _params.RemoveAt(0);
                                string _entry = string.Join(" ", _params);
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[3] = _entry;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 4;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone entry message = \"{0}\"", _entry));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entry message saved. Type zones save \"exit message\". This is the message players receive upon exiting the zone."));
                            }
                            else if (_stage == 4)
                            {
                                _params.RemoveAt(0);
                                string _exit = string.Join(" ", _params);
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[4] = _exit;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 5;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone exit message = \"{0}\"", _exit));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Exit message saved. Type zones save \"response\". This is the console command that will occur when a player enters this zone."));
                            }
                            else if (_stage == 5)
                            {
                                _params.RemoveAt(0);
                                string _response = string.Join(" ", _params);
                                if (_response == "")
                                {
                                    _response = "**";
                                }
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[5] = _response;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 6;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone response = \"{0}\"", _response));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Response saved. Type zones save \"reminder message\". This will set the message players receive if they stay in this zone long enough."));
                            }
                            else if (_stage == 6)
                            {
                                _params.RemoveAt(0);
                                string _reminder = string.Join(" ", _params);
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                _strings[6] = _reminder;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 7;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone reminder message = \"{0}\"", _reminder));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reminder message saved. Type zones save 'true or false'. This will set PvE to true or false."));
                            }
                            else if (_stage == 7)
                            {
                                if (bool.TryParse(_params[1], out bool _result))
                                {
                                    if (_result)
                                    {
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[1] = true;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 8;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] PvE saved. Type zones save 'true or false'. This will set No_Zombie to true or false."));
                                    }
                                    else
                                    {
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[1] = false;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 8;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] PvE saved. Type zones save 'true or false'. This will set No_Zombie to true or false."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Incorrect format. Type zones save 'true or false'."));
                                }
                            }
                            else if (_stage == 8)
                            {
                                if (bool.TryParse(_params[1], out bool _result))
                                {
                                    if (_result)
                                    {
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[2] = true;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No_Zombie = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No_Zombie saved. Type zones save 'true or false'. This will set Protected to true or false."));
                                    }
                                    else
                                    {
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[2] = false;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected saved. Type zones save 'true or false'. This will set No_Zombie to true or false."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Incorrect format. Type zones save 'true or false'."));
                                }
                            }
                            else if (_stage == 9)
                            {
                                if (bool.TryParse(_params[1], out bool _result))
                                {
                                    if (_result)
                                    {
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[3] = true;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 10;
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("No zombie saved"));
                                        SdtdConsole.Instance.Output("");
                                        SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _strings[2]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[3]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[4]));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[5]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[6]));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                        SdtdConsole.Instance.Output(string.Format("Protected = {0}", _bools[3]));
                                        SdtdConsole.Instance.Output(string.Format("Type zones save. This will complete the setup."));
                                    }
                                    else
                                    {
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                        _bools[3] = false;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 10;
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("No zombie saved"));
                                        SdtdConsole.Instance.Output("");
                                        SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _strings[2]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[3]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[4]));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[5]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[6]));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                        SdtdConsole.Instance.Output(string.Format("Protected = {0}", _bools[3]));
                                        SdtdConsole.Instance.Output(string.Format("Type zones save. This will complete the setup."));
                                    }
                                }
                            }
                            else if (_stage == 10)
                            {
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                string[] _box1 = { _strings[0], _strings[1], _strings[2], _strings[3], _strings[4], _strings[5], _strings[6] };
                                bool[] _box2 = { _bools[0], _bools[1], _bools[2], _bools[3] };
                                if (!Zones.Box1.Contains(_box1))
                                {
                                    Zones.Box1.Add(_box1);
                                    Zones.Box2.Add(_box2);
                                    Zones.UpdateXml();
                                    string[] _vectors = new string[4];
                                    if (_box2[0])
                                    {
                                        if (_box2[3])
                                        {
                                            string[] _corner1 = _box1[0].Split(',');
                                            List<string[]> _protected = new List<string[]>();
                                            _protected.Add(new string[] { _corner1[0], _corner1[2], _box1[1] });
                                            Zones.AddProtectedZones(_protected);
                                        }
                                    }
                                    else
                                    {
                                        if (_box2[3])
                                        {
                                            string[] _corner1 = _box1[0].Split(',');
                                            string[] _corner2 = _box1[1].Split(',');
                                            List<string[]> _protected = new List<string[]>();
                                            _protected.Add(new string[] { _corner1[0], _corner1[2], _corner2[0], _corner2[2] });
                                            Zones.AddProtectedZones(_protected);
                                        }
                                    }
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New zone setup has been completed."));
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This zone is already setup. Setup a new zone by typing zones new."));
                                }
                                newZone.Remove(_cInfo.entityId);
                                Zones.zoneSetup1.Remove(_cInfo.entityId);
                                Zones.zoneSetup1.Remove(_cInfo.entityId);
                            }
                        }
                        else if (_params[0].ToLower().Equals("back"))
                        {
                            if (newZone.ContainsKey(_cInfo.entityId))
                            {
                                newZone.TryGetValue(_cInfo.entityId, out int _stage);
                                if (_stage == 1)
                                {
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone name = {0}", _strings[2]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the first corner of the zone and type zones save."));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] If you would like a circle, stand in the middle of the circle and type zones circle."));
                                }
                                else if (_stage == 2)
                                {
                                    newZone[_cInfo.entityId] = 1;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    string[] _corner1 = _strings[0].Split(',');
                                    int.TryParse(_corner1[0], out int x);
                                    int.TryParse(_corner1[1], out int y);
                                    int.TryParse(_corner1[2], out int z);
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                    if (_bools[0])
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Saved the zone as a circle. Circle center point = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the furthest point from the center and types zone circle."));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 saved. Stand in the opposite corner of the zone and types zones save."));
                                    }
                                }
                                else if (_stage == 3)
                                {
                                    newZone[_cInfo.entityId] = 2;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                    if (_bools[0])
                                    {
                                        string _distance = _strings[1];
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Saved the circle radius to {0}.", _distance));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zones save 'entry message'. This is the message players receive upon entering the zone."));
                                    }
                                    else
                                    {
                                        string[] _corner2 = _strings[1].Split(',');
                                        int.TryParse(_corner2[0], out int x);
                                        int.TryParse(_corner2[1], out int y);
                                        int.TryParse(_corner2[2], out int z);
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 2 = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 2 saved. Type zones save 'entry message'. This is the message players receive upon entering the zone."));
                                    }
                                }
                                else if (_stage == 4)
                                {
                                    newZone[_cInfo.entityId] = 3;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone entry message = \"{0}\"", _strings[2]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone entry message saved. Type zones save 'exit message'. This is the message players receive upon exiting the zone."));

                                }
                                else if (_stage == 5)
                                {
                                    newZone[_cInfo.entityId] = 4;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone exit message = \"{0}\"", _strings[3]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone exit message saved. Type zones save 'response'. This is the console command that will occur when a player enters this zone."));

                                }
                                else if (_stage == 6)
                                {
                                    newZone[_cInfo.entityId] = 5;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone response = \"{0}\"", _strings[4]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone response saved. Type zones save 'reminder message'. This will set the message players receive if they stay in this zone long enough."));

                                }
                                else if (_stage == 7)
                                {
                                    newZone[_cInfo.entityId] = 6;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone reminder message = \"{0}\"", _strings[5]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone reminder message saved. Type zones save 'true or false'. This will set the zone as a PvE zone or not."));

                                }
                                else if (_stage == 8)
                                {
                                    newZone[_cInfo.entityId] = 7;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone PvE = {0}", _bools[1]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone PvE saved. Type zones save 'true or false'. This will set the zone as a PvE zone or not."));

                                }
                                else if (_stage == 9)
                                {
                                    newZone[_cInfo.entityId] = 8;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No zombie = {0}", _bools[2]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No zombie saved. Type zones save 'true or false'. This will complete the setup."));
                                }
                                else if (_stage == 10)
                                {
                                    newZone[_cInfo.entityId] = 9;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone setup has gone back one step."));
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out string[] _strings);
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out bool[] _bools);
                                    SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                    SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[2]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[3]));
                                    SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[4]));
                                    SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[5]));
                                    SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                    SdtdConsole.Instance.Output(string.Format("Protected = {0}", _bools[3]));
                                    SdtdConsole.Instance.Output(string.Format("Type zone save. This will complete the setup."));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have not started the setup for a new zone. Type zones new to begin setting up a new zone."));
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ZonesConsole.Execute: {0}", e.Message));
            }
        }
    }
}
