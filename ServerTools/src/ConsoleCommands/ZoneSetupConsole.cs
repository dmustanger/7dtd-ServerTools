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
            return "[ServerTools]-Enable, setup a new zone or check existing zones.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Zones off\n" +
                   "  2. Zones on\n" +
                   "  3. Zones new\n" +
                   "  4. Zones list\n" +
                   "  5. Zones delete <number>\n" +
                   "  6. Zones save\n" +
                   "  7. Zones back\n" +
                   "1. Turn off zones\n" +
                   "2. Turn on zones\n" +
                   "3. Create a new zone\n" +
                   "4. Shows the zone list\n" +
                   "5. Deletes the specific zone from the list\n" +
                   "6. Use this throughout the setup process when prompted\n" +
                   "7. Use this to go back a step in the setup process\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Zones", "Zones", "zones" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                var _cInfo = _senderInfo.RemoteClientInfo;
                if (_cInfo != null)
                {
                    if (_params.Count < 1 || _params.Count > 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}.", _params.Count));
                        return;
                    }
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
                        if (Zones.corner1.ContainsKey(_cInfo.entityId))
                        {
                            Zones.corner1.Remove(_cInfo.entityId);
                        }
                        if (Zones.corner2.ContainsKey(_cInfo.entityId))
                        {
                            Zones.corner2.Remove(_cInfo.entityId);
                        }
                        if (Zones.entry.ContainsKey(_cInfo.entityId))
                        {
                            Zones.entry.Remove(_cInfo.entityId);
                        }
                        if (Zones.exit.ContainsKey(_cInfo.entityId))
                        {
                            Zones.exit.Remove(_cInfo.entityId);
                        }
                        if (Zones.response.ContainsKey(_cInfo.entityId))
                        {
                            Zones.response.Remove(_cInfo.entityId);
                        }
                        if (Zones.pve.ContainsKey(_cInfo.entityId))
                        {
                            Zones.pve.Remove(_cInfo.entityId);
                        }
                        if (Zones.nozombie.ContainsKey(_cInfo.entityId))
                        {
                            Zones.nozombie.Remove(_cInfo.entityId);
                        }
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            newZone[_cInfo.entityId] = 1;
                            if (Zones.corner1.ContainsKey(_cInfo.entityId))
                            {
                                Zones.corner1.Remove(_cInfo.entityId);
                            }
                            if (Zones.corner2.ContainsKey(_cInfo.entityId))
                            {
                                Zones.corner2.Remove(_cInfo.entityId);
                            }
                            if (Zones.entry.ContainsKey(_cInfo.entityId))
                            {
                                Zones.entry.Remove(_cInfo.entityId);
                            }
                            if (Zones.exit.ContainsKey(_cInfo.entityId))
                            {
                                Zones.exit.Remove(_cInfo.entityId);
                            }
                            if (Zones.response.ContainsKey(_cInfo.entityId))
                            {
                                Zones.response.Remove(_cInfo.entityId);
                            }
                            if (Zones.pve.ContainsKey(_cInfo.entityId))
                            {
                                Zones.pve.Remove(_cInfo.entityId);
                            }
                            if (Zones.nozombie.ContainsKey(_cInfo.entityId))
                            {
                                Zones.nozombie.Remove(_cInfo.entityId);
                            }
                            SdtdConsole.Instance.Output(string.Format("The previous zone setup was reset."));
                        }
                        else
                        {
                            newZone.Add(_cInfo.entityId, 1);
                        }
                        SdtdConsole.Instance.Output(string.Format("Stand where the first corner of the zone will start and type zone save in console."));
                    }
                    else if (_params[0].ToLower().Equals("list"))
                    {
                        if (Players.Box.Count >= 1)
                        {
                            for (int i = 0; i < Players.Box.Count; i++)
                            {
                                string[] _box = Players.Box[i];
                                if (_box != null)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Zone number {0}:", i));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _box[0]));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _box[1]));
                                    SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _box[2]));
                                    SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _box[3]));
                                    SdtdConsole.Instance.Output(string.Format("Response = {0}", _box[4]));
                                    SdtdConsole.Instance.Output(string.Format("PvE = {0}", _box[5]));
                                    SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _box[6]));
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
                        if (Players.Box.Count >= 1)
                        {
                            int _number;
                            if (int.TryParse(_params[1], out _number))
                            {
                                Players.Box.RemoveAt(_number);
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
                    else if (_params[0].ToLower().Equals("save"))
                    {
                        if (newZone.ContainsKey(_cInfo.entityId))
                        {
                            int _stage;
                            newZone.TryGetValue(_cInfo.entityId, out _stage);
                            if (_stage == 1)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                Zones.corner1.Add(_cInfo.entityId, _sposition);
                                newZone[_cInfo.entityId] = 2;
                                SdtdConsole.Instance.Output(string.Format("Corner 1 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Corner 1 saved. Stand in the opposite corner of the zone and type zone save."));
                            }
                            else if (_stage == 2)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                Vector3 _position = _player.GetPosition();
                                int x = (int)_position.x;
                                int y = (int)_position.y;
                                int z = (int)_position.z;
                                string _sposition = x + "," + y + "," + z;
                                Zones.corner2.Add(_cInfo.entityId, _sposition);
                                newZone[_cInfo.entityId] = 3;
                                SdtdConsole.Instance.Output(string.Format("Corner 2 = {0} {1} {2}", x, y, z));
                                SdtdConsole.Instance.Output(string.Format("Corner 2 saved. Type zone save 'entry message'. This is the message players receive upon entering the zone."));
                            }
                            else if (_stage == 3)
                            {
                                string _entry = "";
                                for (int i = 0; i < _params.Count; i++)
                                {
                                    _entry = _entry + " " + _params[i];
                                }
                                _entry = _entry.Replace(" zone save ", "");
                                Zones.entry.Add(_cInfo.entityId, _entry);
                                newZone[_cInfo.entityId] = 4;
                                SdtdConsole.Instance.Output(string.Format("Zone entry message = \"{0}\"", _entry));
                                SdtdConsole.Instance.Output(string.Format("Zone entry message saved. Type zone save 'exit message'. This is the message players receive upon exiting the zone."));
                            }
                            else if (_stage == 4)
                            {
                                string _exit = "";
                                for (int i = 0; i < _params.Count; i++)
                                {
                                    _exit = _exit + " " + _params[i];
                                }
                                _exit = _exit.Replace(" zone save ", "");
                                Zones.exit.Add(_cInfo.entityId, _exit);
                                newZone[_cInfo.entityId] = 5;
                                SdtdConsole.Instance.Output(string.Format("Zone exit message = \"{0}\"", _exit));
                                SdtdConsole.Instance.Output(string.Format("Zone exit message saved. Type zone save 'response'. This is the console command that will occur when a player enters this zone."));
                            }
                            else if (_stage == 5)
                            {
                                string _response = "";
                                for (int i = 0; i < _params.Count; i++)
                                {
                                    _response = _response + " " + _params[i];
                                }
                                _response = _response.Replace(" zone save ", "");
                                Zones.response.Add(_cInfo.entityId, _response);
                                newZone[_cInfo.entityId] = 6;
                                SdtdConsole.Instance.Output(string.Format("Zone response = \"{0}\"", _response));
                                SdtdConsole.Instance.Output(string.Format("Zone response saved. Type zone save 'true or false'. This will set the zone as a PvE zone or not."));
                            }
                            else if (_stage == 6)
                            {
                                string _pve = "";
                                for (int i = 0; i < _params.Count; i++)
                                {
                                    _pve = _pve + " " + _params[i];
                                }
                                _pve = _pve.Replace(" zone save ", "");
                                bool _result;
                                if (bool.TryParse(_pve, out _result))
                                {
                                    if (_result)
                                    {
                                        Zones.pve.Add(_cInfo.entityId, _result);
                                        newZone[_cInfo.entityId] = 7;
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save 'true or false'. This will set the zone as a PvE zone or not."));
                                    }
                                    else
                                    {
                                        Zones.pve.Add(_cInfo.entityId, _result);
                                        newZone[_cInfo.entityId] = 7;
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save 'true or false'. This will set if zombies are removed from this zone."));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Incorrect format. Type zone save true or zone save false."));
                                }
                            }
                            else if (_stage == 7)
                            {
                                string _noZ = "";
                                for (int i = 0; i < _params.Count; i++)
                                {
                                    _noZ = _noZ + " " + _params[i];
                                }
                                _noZ = _noZ.Replace(" zone save ", "");
                                bool _result;
                                if (bool.TryParse(_noZ, out _result))
                                {
                                    if (_result)
                                    {
                                        Zones.nozombie.Add(_cInfo.entityId, true);
                                        newZone[_cInfo.entityId] = 8;
                                        string _c1;
                                        Zones.corner1.TryGetValue(_cInfo.entityId, out _c1);
                                        string _c2;
                                        Zones.corner2.TryGetValue(_cInfo.entityId, out _c2);
                                        string _entry;
                                        Zones.entry.TryGetValue(_cInfo.entityId, out _entry);
                                        string _exit;
                                        Zones.exit.TryGetValue(_cInfo.entityId, out _exit);
                                        string _response;
                                        Zones.response.TryGetValue(_cInfo.entityId, out _response);
                                        bool _pve;
                                        Zones.pve.TryGetValue(_cInfo.entityId, out _pve);
                                        SdtdConsole.Instance.Output(string.Format("Zone zombies = {0}", _result));
                                        SdtdConsole.Instance.Output(string.Format("Zone zombies set and setup is almost complete."));
                                        SdtdConsole.Instance.Output(string.Format("Review the following and type zone save to confirm the data is correct or type zone back to return to the previous setup step."));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _c1));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _c2));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _entry));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _exit));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _response));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _pve));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                    }
                                    else
                                    {
                                        Zones.nozombie.Add(_cInfo.entityId, false);
                                        newZone[_cInfo.entityId] = 8;
                                        string _c1;
                                        Zones.corner1.TryGetValue(_cInfo.entityId, out _c1);
                                        string _c2;
                                        Zones.corner2.TryGetValue(_cInfo.entityId, out _c2);
                                        string _entry;
                                        Zones.entry.TryGetValue(_cInfo.entityId, out _entry);
                                        string _exit;
                                        Zones.exit.TryGetValue(_cInfo.entityId, out _exit);
                                        string _response;
                                        Zones.response.TryGetValue(_cInfo.entityId, out _response);
                                        bool _pve;
                                        Zones.pve.TryGetValue(_cInfo.entityId, out _pve);
                                        SdtdConsole.Instance.Output(string.Format("Zone zombies = false"));
                                        SdtdConsole.Instance.Output(string.Format("Zone zombies set and setup is almost complete."));
                                        SdtdConsole.Instance.Output(string.Format("Review the following and type zone save to confirm the data is correct or type zone back to return to the previous setup step."));
                                        SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _c1));
                                        SdtdConsole.Instance.Output(string.Format("Corner 2 = {0}", _c2));
                                        SdtdConsole.Instance.Output(string.Format("Entry message = {0}", _entry));
                                        SdtdConsole.Instance.Output(string.Format("Exit message = {0}", _exit));
                                        SdtdConsole.Instance.Output(string.Format("Response = {0}", _response));
                                        SdtdConsole.Instance.Output(string.Format("PvE = {0}", _pve));
                                        SdtdConsole.Instance.Output(string.Format("No zombie = {0}", _result));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Incorrect format. Type zone save true or zone save false."));
                                }
                            }
                            else if (_stage == 8)
                            {
                                string _c1;
                                Zones.corner1.TryGetValue(_cInfo.entityId, out _c1);
                                string _c2;
                                Zones.corner2.TryGetValue(_cInfo.entityId, out _c2);
                                string _entry;
                                Zones.entry.TryGetValue(_cInfo.entityId, out _entry);
                                string _exit;
                                Zones.exit.TryGetValue(_cInfo.entityId, out _exit);
                                string _response;
                                Zones.response.TryGetValue(_cInfo.entityId, out _response);
                                bool _pve;
                                Zones.pve.TryGetValue(_cInfo.entityId, out _pve);
                                bool _noZ;
                                Zones.nozombie.TryGetValue(_cInfo.entityId, out _noZ);
                                string[] _box = { _c1, _c2, _entry, _exit, _response, _pve.ToString(), _noZ.ToString() };
                                if (!Players.Box.Contains(_box))
                                {
                                    Players.Box.Add(_box);
                                    Zones.UpdateXml();
                                    SdtdConsole.Instance.Output(string.Format("New zone setup has been completed."));
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("This zone is already setup. Setup a new zone by typing zone new."));
                                }
                                newZone.Remove(_cInfo.entityId);
                                Zones.corner1.Remove(_cInfo.entityId);
                                Zones.corner2.Remove(_cInfo.entityId);
                                Zones.entry.Remove(_cInfo.entityId);
                                Zones.exit.Remove(_cInfo.entityId);
                                Zones.response.Remove(_cInfo.entityId);
                                Zones.pve.Remove(_cInfo.entityId);
                                Zones.nozombie.Remove(_cInfo.entityId);
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
                                    SdtdConsole.Instance.Output(string.Format("Stand where the first corner of the zone will start and type zone save in console."));
                                }
                                else if (_stage == 2)
                                {
                                    Zones.corner1.Remove(_cInfo.entityId);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Stand where the first corner of the zone will start and type zone save in console."));
                                }
                                else if (_stage == 3)
                                {
                                    Zones.corner2.Remove(_cInfo.entityId);
                                    string _c1;
                                    Zones.corner1.TryGetValue(_cInfo.entityId, out _c1);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 = {0}", _c1));
                                    SdtdConsole.Instance.Output(string.Format("Corner 1 saved. Stand in the opposite corner of the zone and type zone save."));
                                    
                                }
                                else if (_stage == 4)
                                {
                                    Zones.entry.Remove(_cInfo.entityId);
                                    string _c2;
                                    Zones.corner1.TryGetValue(_cInfo.entityId, out _c2);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 = {0} {1} {2}", _c2));
                                    SdtdConsole.Instance.Output(string.Format("Corner 2 saved. Type zone save <message>. This is the message players receive upon entering the zone."));
                                    
                                }
                                else if (_stage == 5)
                                {
                                    Zones.exit.Remove(_cInfo.entityId);
                                    string _entry;
                                    Zones.entry.TryGetValue(_cInfo.entityId, out _entry);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Zone entry message = \"{0}\"", _entry));
                                    SdtdConsole.Instance.Output(string.Format("Zone entry message saved. Type zone save <message>'. This is the message players receive upon exiting the zone."));
                                    
                                }
                                else if (_stage == 6)
                                {
                                    Zones.response.Remove(_cInfo.entityId);
                                    string _exit;
                                    Zones.exit.TryGetValue(_cInfo.entityId, out _exit);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Zone exit message = \"{0}\"", _exit));
                                    SdtdConsole.Instance.Output(string.Format("Zone exit message saved. Type zone save <response>. This is the console command that will occur when a player enters this zone."));
                                    
                                }
                                else if (_stage == 7)
                                {
                                    Zones.pve.Remove(_cInfo.entityId);
                                    string _response;
                                    Zones.response.TryGetValue(_cInfo.entityId, out _response);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Zone response = \"{0}\"", _response));
                                    SdtdConsole.Instance.Output(string.Format("Zone response saved. Type zone save <true or false>. This will set the zone as a PvE zone or not."));
                                    
                                }
                                else if (_stage == 8)
                                {
                                    Zones.nozombie.Remove(_cInfo.entityId);
                                    bool _pve;
                                    Zones.pve.TryGetValue(_cInfo.entityId, out _pve);
                                    SdtdConsole.Instance.Output(string.Format("Zone setup has gone back one step."));
                                    SdtdConsole.Instance.Output(string.Format("Zone PvE = {0}", _pve));
                                    SdtdConsole.Instance.Output(string.Format("Zone PvE saved. Type zone save <true or false>. This will set if zombies are removed from this zone."));
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
