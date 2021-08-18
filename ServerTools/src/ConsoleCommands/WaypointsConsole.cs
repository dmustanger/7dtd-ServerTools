using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WaypointsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable waypoints.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-wp off\n" +
                   "  2. st-wp on\n" +
                   "  3. st-wp add <name> <cost>\n" +
                   "  4. st-wp add <name> <x> <y> <z> <cost>\n" +
                   "  5. st-wp del <name>\n" +
                   "  6. st-wp list\n" +
                   "1. Turn off waypoints\n" +
                   "2. Turn on waypoints\n" +
                   "3. Add a public waypoint with the specified name. Your current position in the world will be used\n" +
                   "4. Add a public waypoint with the specified name and location\n" +
                   "5. Delete the pubic waypoint with the specified name\n" +
                   "6. Show a list of public waypoints\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Waypoints", "wp", "st-wp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 3 && _params.Count != 6)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 3 or 6, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count == 3)
                    {
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No client info found. Join the server as a client before using this command"));
                            return;
                        }
                        EntityPlayer _player = PersistentOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.playerId);
                        if (_player != null)
                        {
                            int _x = (int)_player.position.x;
                            int _y = (int)_player.position.y;
                            int _z = (int)_player.position.z;
                            string _position = _x + "," + _y + "," + _z;
                            string _name = _params[1];
                            string _cost = _params[2];
                            if (int.TryParse(_params[2], out int _commandCost))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost: {0}", _params[2]));
                                return;
                            }
                            string[] _waypoint = { _position, _cost };
                            if (!Waypoints.Dict.ContainsKey(_name))
                            {
                                Waypoints.Dict.Add(_name, _waypoint);
                                Waypoints.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named {0} at position {1}. Cost set to {2}", _name, _position, _cost));
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                                return;
                            }
                        }
                    }
                    else if (_params.Count == 6)
                    {
                        string _name = _params[1];
                        int.TryParse(_params[2], out int _x);
                        int.TryParse(_params[3], out int _y);
                        int.TryParse(_params[4], out int _z);
                        string _position = _x + "," + _y + "," + _z;
                        string _cost = _params[5];
                        if (int.TryParse(_params[5], out int _commandCost))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost: {0}", _params[5]));
                            return;
                        }
                        string[] _waypoint = { _position, _cost };
                        if (!Waypoints.Dict.ContainsKey(_name))
                        {
                            Waypoints.Dict.Add(_name, _waypoint);
                            Waypoints.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named {0} at position {1}. Cost set to {2}", _name, _position, _cost));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("del"))
                {
                    string _name = _params[1];
                    if (Waypoints.Dict.ContainsKey(_name))
                    {
                        Waypoints.Dict.Remove(_name);
                        Waypoints.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Delete waypoint named {0}", _name));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to delete waypoint named {0}. Waypoint not found", _name));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (Waypoints.Dict.Count > 0)
                    {
                        foreach (var _waypoint in Waypoints.Dict)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoint named '{0}' @ position {1} for the cost of {2} {3}", _waypoint.Key, _waypoint.Value[0], _waypoint.Value[1], Wallet.Coin_Name));
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no waypoints on the list");
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WaypointsConsole.Execute: {0}", e.Message));
            }
        }
    }
}