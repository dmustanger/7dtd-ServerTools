using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class ZoneSetupConsole : ConsoleCmdAbstract
    {
        public static Dictionary<int, int> newZone = new Dictionary<int, int>();

        public override string GetDescription()
        {
            return "[ServerTools]-Enable, setup a new zone or list existing zones.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Zone off\n" +
                   "  2. Zone on\n" +
                   "  3. Zone new\n" +
                   "  4. Zone list\n" +
                   "  5. Zone delete <number>\n" +
                   "  6. Zone save\n" +
                   "  7. Zone back\n" +
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
            return new string[] { "st-Zone", "Zone", "zone"};
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
                        Zones.IsEnabled = false;
                        SdtdConsole.Instance.Output(string.Format("Zones has been set to off"));
                        return;
                    }
                    else if (_params[0].ToLower().Equals("on"))
                    {
                        Zones.IsEnabled = true;
                        SdtdConsole.Instance.Output(string.Format("Zones has been set to on"));
                        return;
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
                        string[] _strings = { "", "", "", "", "", "" };
                        bool[] _bools = { false, false, false };
                        Zones.zoneSetup1.Add(_cInfo.entityId, _strings);
                        Zones.zoneSetup2.Add(_cInfo.entityId, _bools);
                        SdtdConsole.Instance.Output(string.Format("Stand at the first corner of the zone and type zone save."));
                        SdtdConsole.Instance.Output(string.Format("If you would like a circle, stand in the middle of the circle and type zone circle."));
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
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _box[2]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _box[3]));
                                    SdtdConsole.Instance.Output(string.Format("Response = {0}", _box[4]));
                                    SdtdConsole.Instance.Output(string.Format("Reminder Notice = {0}", _box[5]));
                                    SdtdConsole.Instance.Output(string.Format("Circle = {0}", _box2[0]));
                                    SdtdConsole.Instance.Output(string.Format("PvE = {0}", _box2[1]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _box2[2]));
                                    SdtdConsole.Instance.Output(string.Format(""));
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("There are no zones setup."));
                        }
                    }
                    else if (_params[0].ToLower().Equals("delete"))
                    {
                        if (Zones.Box1.Count > 0)
                        {
                            int _number;
                            if (int.TryParse(_params[1], out _number))
                            {
                                Zones.Box1.RemoveAt(_number);
                                Zones.Box2.RemoveAt(_number);
                                Zones.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("Removed zone entry number {0} from the list.", _number));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid format or non numeric entry. Type zone delete <number> from the list of zones."));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("There are no zones setup."));
                        }
                    }
                    else if (_params[0].ToLower().Equals("circle"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            int _stage;
                            newZone.TryGetValue(_cInfo.entityId, out _stage);
                            if (_stage == 1)
                            {
                                bool[] _bools;
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                _bools[0] = true;
                                Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[0] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("Saved the zone as a circle. Circle center point = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Type zone circle <number> to set the amount of blocks from center the zone will reach."));
                            }
                            else if (_stage == 2)
                            {
                                bool[] _bools;
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                if (_bools[0])
                                {
                                    int _result;
                                    if (int.TryParse(_params[1], out _result))
                                    {
                                        string[] _strings;
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                        _strings[1] = _result.ToString();
                                        Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                        newZone[_cInfo.entityId] = 3;
                                        SdtdConsole.Instance.Output(string.Format("Saved the circle radius to {0}.", _result));
                                        SdtdConsole.Instance.Output(string.Format("Type zone save \"entry message\". This is the message players receive upon entering the zone."));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("This zone is not setup as a circle, go back by typing zone back."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("This zone is not setup as a circle, go back by typing zone back."));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Start a new zone setup or go back to the circle setup if you need to change it by typing zone back."));
                            }
                        }
                    }
                    else if (_params[0].ToLower().Equals("save"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            int _stage;
                            newZone.TryGetValue(_cInfo.entityId, out _stage);
                            if (_stage == 1)
                            {
                                bool[] _bools;
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                _bools[0] = false;
                                Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[0] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("Corner 1 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Corner 1 saved. Stand in the opposite corner of the zone and type zone save."));
                            }
                            if (_stage == 2)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[1] = _sposition;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 3;
                                SdtdConsole.Instance.Output(string.Format("Corner 2 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Corner 2 saved. Type zone save \"entry message\". This is the message players receive upon entering the zone."));
                            }
                            else if (_stage == 3)
                            {
                                string _entry = _params[1];
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[2] = _entry;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 4;
                                SdtdConsole.Instance.Output(string.Format("Zone entry message = \"{0}\"", _entry));
                                SdtdConsole.Instance.Output(string.Format("Zone entry message saved. Type zone save \"exit message\". This is the message players receive upon exiting the zone."));
                            }
                            else if (_stage == 4)
                            {
                                string _exit = _params[1];
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[3] = _exit;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 5;
                                SdtdConsole.Instance.Output(string.Format("Zone exit message = \"{0}\"", _exit));
                                SdtdConsole.Instance.Output(string.Format("Zone exit message saved. Type zone save \"response\". This is the console command that will occur when a player enters this zone."));
                            }
                            else if (_stage == 5)
                            {
                                string _response = _params[1];
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[4] = _response;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 6;
                                SdtdConsole.Instance.Output(string.Format("Zone response = \"{0}\"", _response));
                                SdtdConsole.Instance.Output(string.Format("Zone response saved. Type zone save \"reminder message\". This will set the message players receive if they stay in this zone long enough."));
                            }
                            else if (_stage == 6)
                            {
                                string _response = _params[1];
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                _strings[5] = _response;
                                Zones.zoneSetup1[_cInfo.entityId] = _strings;
                                newZone[_cInfo.entityId] = 7;
                                SdtdConsole.Instance.Output(string.Format("Zone reminder message = \"{0}\"", _response));
                                SdtdConsole.Instance.Output(string.Format("Zone reminder message saved. Type zone save 'true or false'. This will set PvE to true or false."));
                            }
                            else if (_stage == 7)
                            {
                                bool _result;
                                if (bool.TryParse(_params[1], out _result))
                                {
                                    if (_result)
                                    {
                                        bool[] _bools;
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                        _bools[1] = true;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 8;
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save 'true or false'. This will set No_Zombie to true or false."));
                                    }
                                    else
                                    {
                                        bool[] _bools;
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                        _bools[1] = false;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 8;
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save 'true or false'. This will set No_Zombie to true or false."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Incorrect format. Type zone save 'true or false'."));
                                }
                            }
                            else if (_stage == 8)
                            {
                                bool _result;
                                if (bool.TryParse(_params[1], out _result))
                                {
                                    if (_result)
                                    {
                                        string[] _strings;
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                        bool[] _bools;
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                        _bools[2] = true;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("No zombie saved"));
                                        SdtdConsole.Instance.Output("");
                                        SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[2]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[3]));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[4]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[5]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                        SdtdConsole.Instance.Output(string.Format("Type zone save. This will complete the setup."));
                                    }
                                    else
                                    {
                                        string[] _strings;
                                        Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                        bool[] _bools;
                                        Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                        _bools[2] = false;
                                        Zones.zoneSetup2[_cInfo.entityId] = _bools;
                                        newZone[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("No zombie saved"));
                                        SdtdConsole.Instance.Output("");
                                        SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[2]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[3]));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[4]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[5]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                        SdtdConsole.Instance.Output(string.Format("Type zone save. This will complete the setup."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Incorrect format. Type zone save true or zone save false."));
                                }
                            }
                            else if (_stage == 9)
                            {
                                string[] _strings;
                                Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                bool[] _bools;
                                Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                string[] _box1 = { _strings[0], _strings[1], _strings[2], _strings[3], _strings[4], _strings[5] };
                                bool[] _box2 = { _bools[0], _bools[1], _bools[2] };
                                if (!Zones.Box1.Contains(_box1))
                                {
                                    Zones.Box1.Add(_box1);
                                    Zones.Box2.Add(_box2);
                                    Zones.UpdateXml();
                                    SdtdConsole.Instance.Output(string.Format("New zone setup has been completed."));
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("This zone is already setup. Setup a new zone by typing zone new."));
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
                                int _stage;
                                newZone.TryGetValue(_cInfo.entityId, out _stage);
                                if (_stage == 1)
                                {
                                    SdtdConsole.Instance.Output(string.Format("You can not go any further back in the setup process."));
                                    SdtdConsole.Instance.Output(string.Format("Stand at the first corner of the zone and type zone save."));
                                    SdtdConsole.Instance.Output(string.Format("If you would like a circle, stand in the middle of the circle and type zone circle."));
                                }
                                else if (_stage == 2)
                                {
                                    newZone[_cInfo.entityId] = 1;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    string[] _corner1 = _strings[0].Split(',');
                                    int x, y, z;
                                    int.TryParse(_corner1[0], out x);
                                    int.TryParse(_corner1[1], out y);
                                    int.TryParse(_corner1[2], out z);
                                    bool[] _bools;
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                    if (_bools[0])
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Saved the zone as a circle. Circle center point = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("Stand at the furthest point from the center and type zone circle."));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 saved. Stand in the opposite corner of the zone and type zone save."));
                                    }
                                }
                                else if (_stage == 3)
                                {
                                    newZone[_cInfo.entityId] = 2;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    bool[] _bools;
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                    if (_bools[0])
                                    {
                                        string _distance = _strings[1];
                                        SdtdConsole.Instance.Output(string.Format("Saved the circle radius to {0}.", _distance));
                                        SdtdConsole.Instance.Output(string.Format("Type zone save 'entry message'. This is the message players receive upon entering the zone."));
                                    }
                                    else
                                    {
                                        string[] _corner2 = _strings[1].Split(',');
                                        int x, y, z;
                                        int.TryParse(_corner2[0], out x);
                                        int.TryParse(_corner2[1], out y);
                                        int.TryParse(_corner2[2], out z);
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0} {1} {2}", x, y, z));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 saved. Type zone save 'entry message'. This is the message players receive upon entering the zone."));
                                    }
                                }
                                else if (_stage == 4)
                                {
                                    newZone[_cInfo.entityId] = 3;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    SdtdConsole.Instance.Output(string.Format("Zone entry message = \"{0}\"", _strings[2]));
                                    SdtdConsole.Instance.Output(string.Format("Zone entry message saved. Type zone save 'exit message'. This is the message players receive upon exiting the zone."));

                                }
                                else if (_stage == 5)
                                {
                                    newZone[_cInfo.entityId] = 4;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    SdtdConsole.Instance.Output(string.Format("Zone exit message = \"{0}\"", _strings[3]));
                                    SdtdConsole.Instance.Output(string.Format("Zone exit message saved. Type zone save 'response'. This is the console command that will occur when a player enters this zone."));

                                }
                                else if (_stage == 6)
                                {
                                    newZone[_cInfo.entityId] = 5;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    SdtdConsole.Instance.Output(string.Format("Zone response = \"{0}\"", _strings[4]));
                                    SdtdConsole.Instance.Output(string.Format("Zone response saved. Type zone save 'reminder message'. This will set the message players receive if they stay in this zone long enough."));

                                }
                                else if (_stage == 7)
                                {
                                    newZone[_cInfo.entityId] = 6;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    SdtdConsole.Instance.Output(string.Format("Zone reminder message = \"{0}\"", _strings[5]));
                                    SdtdConsole.Instance.Output(string.Format("Zone reminder message saved. Type zone save 'true or false'. This will set the zone as a PvE zone or not."));

                                }
                                else if (_stage == 8)
                                {
                                    newZone[_cInfo.entityId] = 7;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    bool[] _bools;
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                    SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _bools[1]));
                                    SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save 'true or false'. This will set the zone as a PvE zone or not."));

                                }
                                else if (_stage == 9)
                                {
                                    newZone[_cInfo.entityId] = 8;
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    string[] _strings;
                                    Zones.zoneSetup1.TryGetValue(_cInfo.entityId, out _strings);
                                    bool[] _bools;
                                    Zones.zoneSetup2.TryGetValue(_cInfo.entityId, out _bools);
                                    SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _strings[0]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _strings[1]));
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _strings[2]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _strings[3]));
                                    SdtdConsole.Instance.Output(string.Format("Response = {0}", _strings[4]));
                                    SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _strings[5]));
                                    SdtdConsole.Instance.Output(string.Format("Circle = {0}", _bools[0]));
                                    SdtdConsole.Instance.Output(string.Format("PvE = {0}", _bools[1]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _bools[2]));
                                    SdtdConsole.Instance.Output(string.Format("Type zone save. This will complete the setup."));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("You have not started the setup for a new zone. Type zone new to begin setting up a new zone."));
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ZoneSetupConsole.Run: {0}.", e));
            }
        }
    }
}
