using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RegionResetConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Resets a region on server shutdown";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-rr\n" +
                "  2. st-rr <x> <z>\n" +
                "  3. st-rr add\n" +
                "  4. st-rr add <x> <z>\n" +
                "  5. st-rr add <region>\n" +
                "  6. st-rr cancel\n" +
                "  7. st-rr list\n" +
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
                    EntityPlayer player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                    if (player != null)
                    {
                        double regionX, regionZ;
                        if (player.position.x < 0)
                        {
                            regionX = Math.Truncate(player.position.x / 512) - 1;
                        }
                        else
                        {
                            regionX = Math.Truncate(player.position.x / 512);
                        }
                        if (player.position.z < 0)
                        {
                            regionZ = Math.Truncate(player.position.z / 512) - 1;
                        }
                        else
                        {
                            regionZ = Math.Truncate(player.position.z / 512);
                        }
                        string region = regionX + "." + regionZ;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You are standing in region: {0}", region));
                        return;
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
                        EntityPlayer player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
                        if (player != null)
                        {
                            double regionX, regionZ;
                            if (player.position.x < 0)
                            {
                                regionX = Math.Truncate(player.position.x / 512) - 1;
                            }
                            else
                            {
                                regionX = Math.Truncate(player.position.x / 512);
                            }
                            if (player.position.z < 0)
                            {
                                regionZ = Math.Truncate(player.position.z / 512) - 1;
                            }
                            else
                            {
                                regionZ = Math.Truncate(player.position.z / 512);
                            }
                            string region = regionX + "." + regionZ;
                            if (!RegionReset.Regions.Contains(region))
                            {
                                RegionReset.Regions.Add(region);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} has been added to the reset list. It will be wiped out on server shutdown", region));
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} is already on the reset list", region));
                                return;
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
                            string[] region = _params[1].Split('.');
                            if (int.TryParse(region[0], out int regionX))
                            {
                                if (int.TryParse(region[1], out int regionZ))
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
                        if (float.TryParse(_params[1], out float regionX))
                        {
                            if (float.TryParse(_params[2], out float regionZ))
                            {
                                double x, z;
                                if (regionX < 0)
                                {
                                    x = Math.Truncate(regionX / 512) - 1;
                                }
                                else
                                {
                                    x = Math.Truncate(regionX / 512);
                                }
                                if (regionZ < 0)
                                {
                                    z = Math.Truncate(regionZ / 512) - 1;
                                }
                                else
                                {
                                    z = Math.Truncate(regionZ / 512);
                                }
                                string region = x + "." + z;
                                if (!RegionReset.Regions.Contains(region))
                                {
                                    RegionReset.Regions.Add(region);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} has been added to the reset list. It will be wiped out on server shutdown", region));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Region: {0} is already on the reset list", region));
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
