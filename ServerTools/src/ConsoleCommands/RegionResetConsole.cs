using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RegionResetConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Sends a message to all online admins.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-RegionReset\n" +
                "  2. st-RegionReset <x> <z>\n" +
                "  3. st-RegionReset add\n" +
                "  4. st-RegionReset add <x> <z>\n" +
                "  5. st-RegionReset add <region>\n" +
                "  6. st-RegionReset cancel\n" +
                "  7. st-RegionReset list\n" +
                "1. Shows the region you are standing in and the claims within it\n" +
                "2. Shows the region at the location specified and the claims within it\n" +
                "3. Sets the region you are standing in for reset\n" +
                "4. Sets the region at the location specified for reset\n" +
                "5. Sets the specified region for reset\n" +
                "6. Cancels and clears the regions on the reset list\n" +
                "7. Shows the regions on the reset list\n" +
                "*Example*  st-rr add 500 -1200\n" +
                "*Example*  st-rr add -5.2\n" +
                "*Example*  st-rr add 2.1\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-RegionReset", "rr", "st-rr" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 0 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 0)
                {
                    if (_senderInfo.RemoteClientInfo == null)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                    if (GameManager.Instance.World.Players.dict.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                        if (_player != null)
                        {
                            double _regionX, _regionZ;
                            if (_player.position.x < 0)
                            {
                                _regionX = Math.Truncate(_player.position.x / 512) - 1;
                            }
                            else
                            {
                                _regionX = Math.Truncate(_player.position.x / 512);
                            }
                            if (_player.position.z < 0)
                            {
                                _regionZ = Math.Truncate(_player.position.z / 512) - 1;
                            }
                            else
                            {
                                _regionZ = Math.Truncate(_player.position.z / 512);
                            }
                            string _region = _regionX + "." + _regionZ;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You are standing in region: {0}", _region));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "add")
                {
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                            return;
                        }
                        if (GameManager.Instance.World.Players.dict.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                            if (_player != null)
                            {
                                double _regionX, _regionZ;
                                if (_player.position.x < 0)
                                {
                                    _regionX = Math.Truncate(_player.position.x / 512) - 1;
                                }
                                else
                                {
                                    _regionX = Math.Truncate(_player.position.x / 512);
                                }
                                if (_player.position.z < 0)
                                {
                                    _regionZ = Math.Truncate(_player.position.z / 512) - 1;
                                }
                                else
                                {
                                    _regionZ = Math.Truncate(_player.position.z / 512);
                                }
                                string _region = _regionX + "." + _regionZ;
                                if (!RegionReset.Regions.Contains(_region))
                                {
                                    RegionReset.Regions.Add(_region);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} has been added to the reset list. It will be wiped out on server shutdown", _region));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} is already on the reset list", _region));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                            return;
                        }
                    }
                    else if (_params.Count == 2)
                    {
                        if (_params[1].Contains("."))
                        {
                            string[] _region = _params[1].Split('.');
                            if (int.TryParse(_region[0], out int _regionX))
                            {
                                if (int.TryParse(_region[1], out int _regionZ))
                                {
                                    if (!RegionReset.Regions.Contains(_params[1]))
                                    {
                                        RegionReset.Regions.Add(_params[1]);
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} has been added to the reset list. It will be wiped out on server shutdown", _params[1]));
                                        return;
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} is already on the reset list", _params[1]));
                                        return;
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format: {0}", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format: {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format: {0}", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        if (float.TryParse(_params[1], out float _regionX))
                        {
                            if (float.TryParse(_params[2], out float _regionZ))
                            {
                                double _x, _z;
                                if (_regionX < 0)
                                {
                                    _x = Math.Truncate(_regionX / 512) - 1;
                                }
                                else
                                {
                                    _x = Math.Truncate(_regionX / 512);
                                }
                                if (_regionZ < 0)
                                {
                                    _z = Math.Truncate(_regionZ / 512) - 1;
                                }
                                else
                                {
                                    _z = Math.Truncate(_regionZ / 512);
                                }
                                string _region = _x + "." + _z;
                                if (!RegionReset.Regions.Contains(_region))
                                {
                                    RegionReset.Regions.Add(_region);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} has been added to the reset list. It will be wiped out on server shutdown", _region));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} is already on the reset list", _region));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format: {0} {1}", _params[1], _params[2]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format: {0} {1}", _params[1], _params[2]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower() == "cancel")
                {
                    if (RegionReset.Regions.Count > 0)
                    {
                        RegionReset.Regions.Clear();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region reset list has been cancelled and cleared"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no regions on the reset list");
                        return;
                    }
                }
                else if (_params[0].ToLower() == "list")
                {
                    if (RegionReset.Regions.Count > 0)
                    {
                        for (int i = 0; i < RegionReset.Regions.Count; i++)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0}", RegionReset.Regions[i]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no regions on the reset list");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionResetConsole.Execute: {0}", e.Message));
            }
        }
    }
}
