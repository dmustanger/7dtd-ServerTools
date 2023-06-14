using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class ZonesConsole : ConsoleCmdAbstract
    {
        public static Dictionary<int, int> SetupStage = new Dictionary<int, int>();

        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable, add, remove or list zones.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-zns off\n" +
                   "  2. st-zns on\n" +
                   "  3. st-zns new <Name>\n" +
                   "  4. st-zns list\n" +
                   "  5. st-zns delete <Number>\n" +
                   "  6. st-zns save\n" +
                   "  7. st-zns back\n" +
                   "  8. st-zns forward\n" +
                   "1. Turn off zone tool\n" +
                   "2. Turn on zone tool\n" +
                   "3. Create a new zone\n" +
                   "4. Shows the zone list\n" +
                   "5. Deletes the specific zone from the list\n" +
                   "6. Use this throughout the setup process when prompted\n" +
                   "7. Use this to go back a step in the setup process\n" +
                   "8. Use this to go forward a step in the setup process if you have already completed it\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Zones", "zns", "st-zns" };
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
                            Config.WriteXml();
                            Config.LoadXml();
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
                            Config.WriteXml();
                            Config.LoadXml();
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
                        if (SetupStage.ContainsKey(_cInfo.entityId))
                        {
                            SetupStage[_cInfo.entityId] = 1;
                        }
                        else
                        {
                            SetupStage.Add(_cInfo.entityId, 1);
                        }
                        if (Zones.ZoneSetup.ContainsKey(_cInfo.entityId))
                        {
                            Zones.ZoneSetup.Remove(_cInfo.entityId);
                        }
                        string[] _newZone = new string[11];
                        _params.RemoveAt(0);
                        string _name = string.Join(" ", _params);
                        _newZone[0] = _name;
                        Zones.ZoneSetup.Add(_cInfo.entityId, _newZone);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Started a new zone setup. Zone name set to {0}", _newZone[0]));
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the first corner of the zone and type zns save"));
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] If you would like a circle, stand in the middle of the circle and type zns circle"));
                    }
                    else if (_params[0].ToLower().Equals("list"))
                    {
                        if (Zones.ZoneList.Count > 0)
                        {
                            for (int i = 0; i < Zones.ZoneList.Count; i++)
                            {
                                string[] _zone = Zones.ZoneList[i];
                                if (_zone != null)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Zone number {0}:", i));
                                    SdtdConsole.Instance.Output(string.Format("Name = {0}", _zone[0]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _zone[1]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _zone[2]));
                                    SdtdConsole.Instance.Output(string.Format("Circle = {0}", _zone[3]));
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _zone[4]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _zone[5]));
                                    SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _zone[6]));
                                    SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _zone[7]));
                                    SdtdConsole.Instance.Output(string.Format("Reminder Notice = {0}", _zone[8]));
                                    SdtdConsole.Instance.Output(string.Format("PvPvE = {0}", _zone[9]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _zone[10]));
                                    SdtdConsole.Instance.Output(string.Format(""));
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no zones setup"));
                        }
                    }
                    else if (_params[0].ToLower().Equals("delete"))
                    {
                        if (Zones.ZoneList.Count > 0)
                        {
                            if (int.TryParse(_params[1], out int _number))
                            {
                                if (Zones.ZoneList.Count >= _number)
                                {
                                    Zones.ZoneList.RemoveAt(_number);
                                    Zones.UpdateXml();
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
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid format or non numeric entry. Type zns delete <number> from the list of zones"));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no zones setup"));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("circle"))
                    {
                        if (SetupStage.ContainsKey(_cInfo.entityId))
                        {
                            SetupStage.TryGetValue(_cInfo.entityId, out int _stage);
                            if (_stage == 1)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                Zones.ZoneSetup.TryGetValue(_cInfo.entityId, out string[] _newZone);
                                int _x = (int)_position.x;
                                int _y = (int)_position.y;
                                int _z = (int)_position.z;
                                _newZone[1] = _x + "," + _y + "," + _z;
                                _newZone[3] = "true";
                                Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                SetupStage[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Circle center point = {0}", _newZone[1]));
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns circle 'number' to set the amount of blocks from center the zone will reach"));
                            }
                            else if (_stage == 2)
                            {
                                Zones.ZoneSetup.TryGetValue(_cInfo.entityId, out string[] _newZone);
                                if (_newZone[3].ToLower() == "true")
                                {
                                    if (int.TryParse(_params[1], out int _radius))
                                    {
                                        _newZone[2] = _radius.ToString();
                                        Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                        SetupStage[_cInfo.entityId] = 3;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Saved the circular zone radius to {0} blocks", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This is the message players receive upon entering the zone"));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid radius for circlular zone, try again"));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This zone is not setup as a circle, go back by typing zns back"));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Start a new zone setup or go back to the circle setup if you need to change it by typing zns back."));
                            }
                        }
                    }
                    else if (_params[0].ToLower().Equals("save"))
                    {
                        if (SetupStage.ContainsKey(_cInfo.entityId))
                        {
                            SetupStage.TryGetValue(_cInfo.entityId, out int _stage);
                            Zones.ZoneSetup.TryGetValue(_cInfo.entityId, out string[] _newZone);
                            switch (_stage)
                            {
                                case 1:
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    Vector3 _position = _player.GetPosition();
                                    int _x = (int)_position.x;
                                    int _y = (int)_position.y;
                                    int _z = (int)_position.z;
                                    _newZone[1] = _x + "," + _y + "," + _z;
                                    _newZone[3] = "false";
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 2;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 = {0}", _newZone[1]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand in the opposite corner of the zone and type zns save"));
                                    break;
                                case 2:
                                    _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    _position = _player.GetPosition();
                                    _x = (int)_position.x;
                                    _y = (int)_position.y;
                                    _z = (int)_position.z;
                                    if (_newZone[1].Contains(_y.ToString()))
                                    {
                                        _y++;
                                    }
                                    _newZone[2] = _x + "," + _y + "," + _z;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 3;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 2 = {0}", _newZone[2]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This is the message players receive upon entering the zone"));
                                    break;
                                case 3:
                                    _params.RemoveAt(0);
                                    string _entryMessage = string.Join(" ", _params);
                                    _newZone[4] = _entryMessage;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 4;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entry message = \"{0}\"", _newZone[4]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This is the message players receive upon exiting the zone"));
                                    break;
                                case 4:
                                    _params.RemoveAt(0);
                                    string _exitMessage = string.Join(" ", _params);
                                    _newZone[5] = _exitMessage;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 5;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Exit message = \"{0}\"", _newZone[5]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"command\". This is the console command that will occur when a player enters this zone"));
                                    break;
                                case 5:
                                    _params.RemoveAt(0);
                                    string _entryCommand = string.Join(" ", _params);
                                    if (_entryCommand == "")
                                    {
                                        _entryCommand = "***";
                                    }
                                    _newZone[6] = _entryCommand;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 6;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entry command = \"{0}\"", _newZone[6]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"command\". This is the console command that will occur when a player exits this zone"));
                                    break;
                                case 6:
                                    _params.RemoveAt(0);
                                    string _exitCommand = string.Join(" ", _params);
                                    if (_exitCommand == "")
                                    {
                                        _exitCommand = "***";
                                    }
                                    _newZone[7] = _exitCommand;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 7;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Exit command = \"{0}\"", _newZone[7]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This will set the message players receive if they stay in this zone long enough"));
                                    break;
                                case 7:
                                    _params.RemoveAt(0);
                                    string _reminder = string.Join(" ", _params);
                                    _newZone[8] = _reminder;
                                    Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                    SetupStage[_cInfo.entityId] = 8;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reminder message = \"{0}\"", _newZone[8]));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save '0 to 3'. This will set PvPvE to a specific player killing mode"));
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone"));
                                    break;
                                case 8:
                                    if (int.TryParse(_params[1], out int _playerKillingMode))
                                    {
                                        _newZone[9] = _playerKillingMode.ToString();
                                        Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                        SetupStage[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Zone PvPvE = {0}", _newZone[9]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save 'true or false'. This will set no zombie to true or false"));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Incorrect format. Type zns save '0 to 3'"));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone"));
                                    }
                                    break;
                                case 9:
                                    if (bool.TryParse(_params[1], out bool _noZombie))
                                    {
                                        if (_noZombie)
                                        {
                                            _newZone[10] = "true";
                                        }
                                        else
                                        {
                                            _newZone[10] = "false";
                                        }
                                        Zones.ZoneSetup[_cInfo.entityId] = _newZone;
                                        SetupStage[_cInfo.entityId] = 10;
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No zombie = {0}", _newZone[10]));
                                        SdtdConsole.Instance.Output("");
                                        SdtdConsole.Instance.Output(string.Format("Zone Review:"));
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _newZone[7]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _newZone[8]));
                                        SdtdConsole.Instance.Output(string.Format("PvPvE = {0}", _newZone[9]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _newZone[10]));
                                        SdtdConsole.Instance.Output(string.Format("Type zns save. This will complete the setup"));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Incorrect format. Type zns save 'true or false'"));
                                    }
                                    break;
                                case 10:
                                    if (!Zones.ZoneList.Contains(_newZone))
                                    {
                                        Zones.ZoneList.Add(_newZone);
                                        Zones.UpdateXml();
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New zone setup has been completed"));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This zone is already setup. Setup a new zone by typing zns new"));
                                    }
                                    Zones.ZoneSetup.Remove(_cInfo.entityId);
                                    SetupStage.Remove(_cInfo.entityId);
                                    break;
                            }
                        }
                        else if (_params[0].ToLower().Equals("back"))
                        {
                            if (SetupStage.ContainsKey(_cInfo.entityId))
                            {
                                SetupStage.TryGetValue(_cInfo.entityId, out int _stage);
                                Zones.ZoneSetup.TryGetValue(_cInfo.entityId, out string[] _newZone);
                                switch (_stage)
                                {
                                    case 1:
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the first corner of the zone and type zns save"));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] If you would like a circle, stand in the middle of the circle and type zns circle"));
                                        break;
                                    case 2:
                                        SetupStage[_cInfo.entityId] = 1;
                                        if (_newZone[3].ToLower() == "true")
                                        {
                                            SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                            SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stand at the furthest point from the center and type zns circle"));
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                            SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Corner 1 is saved. Stand in the opposite corner of the zone and type zns save"));
                                        }
                                        break;
                                    case 3:
                                        SetupStage[_cInfo.entityId] = 2;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This is the message players receive upon entering the zone"));
                                        break;
                                    case 4:
                                        SetupStage[_cInfo.entityId] = 3;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This is the message players receive upon exiting the zone"));
                                        break;
                                    case 5:
                                        SetupStage[_cInfo.entityId] = 4;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"command\". This is the console command that will occur when a player enters this zone"));
                                        break;
                                    case 6:
                                        SetupStage[_cInfo.entityId] = 5;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"command\". This is the console command that will occur when a player exits this zone"));
                                        break;
                                    case 7:
                                        SetupStage[_cInfo.entityId] = 6;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _newZone[7]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save \"message\". This will set the message players receive if they stay in this zone long enough"));
                                        break;
                                    case 8:
                                        SetupStage[_cInfo.entityId] = 7;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _newZone[7]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _newZone[8]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save '0 to 3'. This will set PvPvE to a specific player killing mode"));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone"));
                                        break;
                                    case 9:
                                        SetupStage[_cInfo.entityId] = 8;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _newZone[7]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _newZone[8]));
                                        SdtdConsole.Instance.Output(string.Format("PvPvE = {0}", _newZone[9]));
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Type zns save 'true or false'. This will set no zombie to true or false"));
                                        break;
                                    case 10:
                                        SetupStage[_cInfo.entityId] = 9;
                                        SdtdConsole.Instance.Output(string.Format("Name = {0}", _newZone[0]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _newZone[1]));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _newZone[2]));
                                        SdtdConsole.Instance.Output(string.Format("Circle = {0}", _newZone[3]));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _newZone[4]));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _newZone[5]));
                                        SdtdConsole.Instance.Output(string.Format("Entry Command = {0}", _newZone[6]));
                                        SdtdConsole.Instance.Output(string.Format("Exit Command = {0}", _newZone[7]));
                                        SdtdConsole.Instance.Output(string.Format("Reminder notice = {0}", _newZone[8]));
                                        SdtdConsole.Instance.Output(string.Format("PvPvE = {0}", _newZone[9]));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _newZone[10]));
                                        SdtdConsole.Instance.Output(string.Format("Type zns save. This will complete the setup"));
                                        break;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have not started the setup of a new zone. Type zns new 'name' to begin setting up a new zone"));
                            }
                        }
                        else if (_params[0].ToLower().Equals("forward"))
                        {
                            if (SetupStage.ContainsKey(_cInfo.entityId))
                            {

                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have not started the setup of a new zone. Type zns new 'name' to begin setting up a new zone"));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                        }
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
