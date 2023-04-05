using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RegionResetConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add, remove or show a list of reset regions on the server and when they will reset";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-rr\n" +
                "  2. st-rr <x> <z>\n" +
                "  3. st-rr add <time>\n" +
                "  4. st-rr add <x> <z> <time>\n" +
                "  5. st-rr add <region> <time>\n" +
                "  6. st-rr remove <region>\n" +
                "  7. st-rr list\n" +
                "1. Shows the region you are standing in and the claims within it\n" +
                "2. Shows the region at the location specified and the claims within it\n" +
                "3. Sets the region you are standing in for reset\n" +
                "4. Sets the region at the location specified for reset\n" +
                "5. Sets the specified region for reset\n" +
                "6. Cancels and clears the regions on the reset list\n" +
                "7. Shows the regions on the reset list\n" +
                "*Example*  st-rr add station 500 -1200 day\n" +
                "*Example*  st-rr add water -5.2 week\n" +
                "*Example*  st-rr add farm 2.1 month\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-RegionReset", "rr", "st-rr" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower() == "add")
                {
                    if (_params.Count < 2 || _params.Count > 4)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 to 4, found '{0}'", _params.Count));
                        return;
                    }
                    else if (_params.Count == 2)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                        if (_senderInfo.RemoteClientInfo == null || player == null)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                            return;
                        }
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
                        string region = "r." + regionX + "." + regionZ + ".7rg";
                        string time = _params[1].ToLower();
                        if (!RegionReset.Regions.ContainsKey(region))
                        {
                            RegionReset.Regions.Add(region, time);
                            RegionReset.UpdateXml();
                            RegionReset.LoadXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is set for reset in a '{1}' and added to the region reset list", region, time));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is already on the region reset list", region));
                            return;
                        }
                    }
                    else if (_params.Count == 3)
                    {
                        if (!_params[1].Contains("."))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format '{0}'", _params[2]));
                            return;
                        }
                        string[] regionSplit = _params[1].Split('.');
                        if (!int.TryParse(regionSplit[0], out int regionX))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format '{0}'", _params[0]));
                            return;
                        }
                        if (!int.TryParse(regionSplit[1], out int regionZ))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format '{0}'", _params[1]));
                            return;
                        }
                        string region = "r." + regionSplit[0] + "." + regionSplit[1] + ".7rg";
                        string time = _params[2];
                        if (!RegionReset.Regions.ContainsKey(region))
                        {
                            RegionReset.Regions.Add(region, time);
                            RegionReset.UpdateXml();
                            RegionReset.LoadXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is set for reset in a '{1}' and added to the region reset list", region, time));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is already on the region reset list", _params[1]));
                            return;
                        }
                    }
                    else if (_params.Count == 4)
                    {
                        if (!int.TryParse(_params[1], out int regionX))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format '{0}'", _params[1]));
                            return;
                        }
                        if (!int.TryParse(_params[2], out int regionZ))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid region format '{0}'", _params[2]));
                            return;
                        }
                        string region = "r." + _params[1] + "." + _params[2] + ".7rg";
                        string time = _params[3];
                        if (!RegionReset.Regions.ContainsKey(region))
                        {
                            RegionReset.Regions.Add(region, region);
                            RegionReset.UpdateXml();
                            RegionReset.LoadXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is set for reset in a '{1}' and added to the region reset list", region, time));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region '{0}' is already on the region reset list", _params[1]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower() == "remove")
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    else if (RegionReset.Regions.ContainsKey(_params[1]))
                    {
                        RegionReset.Regions.Remove(_params[1]);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed region '{0}' from the region reset list", _params[1]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to find region named '{0}'", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower() == "list")
                {
                    if (RegionReset.Regions.Count > 0)
                    {
                        foreach (var region in RegionReset.Regions)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Region name = '{0}'. Time = '{1}'", region.Key, region.Value));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no regions on the reset list");
                        return;
                    }
                }
                else if (_params.Count == 0)
                {
                    if (_senderInfo.RemoteClientInfo == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
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
                        string region = "r." + regionX + "." + regionZ + ".7rg";
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] You are standing in region '{0}'", region));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                }
                else if (_params.Count == 2)
                {
                    if (_senderInfo.RemoteClientInfo == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                    if (!float.TryParse(_params[0], out float x))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                    if (!float.TryParse(_params[1], out float z))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid user data. Unable to retrieve your position in game");
                        return;
                    }
                    double regionX, regionZ;
                    if (x < 0)
                    {
                        regionX = Math.Truncate(x / 512) - 1;
                    }
                    else
                    {
                        regionX = Math.Truncate(x / 512);
                    }
                    if (z < 0)
                    {
                        regionZ = Math.Truncate(z / 512) - 1;
                    }
                    else
                    {
                        regionZ = Math.Truncate(z / 512);
                    }
                    string region = "r." + regionX + "." + regionZ + ".7rg";
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] '{0} {1}' is in region '{2}'", _params[0], _params[1], region));
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid command '{0}'", _params.ToString()));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionResetConsole.Execute: {0}", e.Message));
            }
        }
    }
}
