using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class ProtectedZonesConsole : ConsoleCmdAbstract
    {
        public static Dictionary<int, int[]> Vectors = new Dictionary<int, int[]>();

        protected override string getDescription()
        {
            return "[ServerTools] - Enabled, disable, add, remove or list protected spaces";
        }
    
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-pz off\n" +
                   "  2. st-pz on\n" +
                   "  3. st-pz add\n" +
                   "  4. st-pz add <x> <z> <x> <z>\n" +
                   "  5. st-pz cancel\n" +
                   "  6. st-pz remove\n" +
                   "  7. st-pz remove <#>\n" +
                   "  8. st-pz remove chunks\n" +
                   "  9. st-pz list\n" +
                   "  10. st-pz active <#>\n" +
                   "1. Turn off Protected_Zones\n" +
                   "2. Turn on Protected_Zones\n" +
                   "3. Add a protected zone. Stand in the south west corner, use add, stand in the north east corner and use add again\n" +
                   "4. Add a protected zone. Set the south west corner and north east corner using specific coordinates\n" +
                   "5. Cancels the saved corner positions\n" +
                   "6. Remove protection from the chunk you are standing in\n" +
                   "7. Remove a protected zone from the list. The number is shown in the list\n" +
                   "8. Removes protection from the nine chunks surrounding you. Note that this will likely effect entries on the list but not remove them\n" +
                   "9. Shows the protected zones list\n" +
                   "10. Activates and deactivates the protection from the entry in the list\n" +
                   "*Make sure the corners used are opposite each other. Example NW with SE or SW with NE*\n";
        }
    
        protected override string[] getCommands()
        {
            return new string[] { "st-ProtectedZones", "pz", "st-pz" };
        }
    
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 5)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2 or 5, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ProtectedZones.IsEnabled)
                    {
                        ProtectedZones.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected zones has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected zones is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ProtectedZones.IsEnabled)
                    {
                        ProtectedZones.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected zones has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected zones is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 1 && _params.Count != 5)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 5, found '{0}'", _params.Count));
                        return;
                    }
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No client info found. Join the server as a client before using this command"));
                            return;
                        }
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                        if (player != null)
                        {
                            int x = (int)player.position.x, z = (int)player.position.z;
                            if (!Vectors.ContainsKey(player.entityId))
                            {
                                int[] vectors = new int[5];
                                vectors[0] = x;
                                vectors[1] = z;
                                Vectors.Add(player.entityId, vectors);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] The first position has been set to {0}x,{1}z", x, z));
                                SdtdConsole.Instance.Output("[SERVERTOOLS] Stand in the opposite corner and use add again. Use cancel to clear the saved position");
                                return;
                            }
                            else
                            {
                                Vectors.TryGetValue(player.entityId, out int[] vectors);
                                Vectors.Remove(player.entityId);
                                if (vectors[0] < x)
                                {
                                    vectors[2] = x + 2;
                                }
                                else
                                {
                                    vectors[2] = vectors[0] - 2;
                                    vectors[0] = x - 2;
                                }
                                if (vectors[1] < z)
                                {
                                    vectors[3] = z + 2;
                                }
                                else
                                {
                                    vectors[3] = vectors[1] - 2;
                                    vectors[1] = z - 2;
                                }
                                vectors[4] = 1;
                                if (!ProtectedZones.ProtectedList.Contains(vectors))
                                {
                                    ProtectedZones.ProtectedList.Add(vectors);
                                    ProtectedZones.UpdateXml();
                                    ProtectedZones.LoadXml();
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected zone from {0}x,{1}z to {2}x,{3}z", vectors[0], vectors[1], vectors[2], vectors[3]));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This protected zone is already on the list"));
                                    return;
                                }
                            }
                        }
                    }
                    else if (_params.Count == 5)
                    {
                        if (!int.TryParse(_params[1], out int xMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[1]));
                            return;
                        }
                        if (!int.TryParse(_params[2], out int zMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[2]));
                            return;
                        }
                        if (!int.TryParse(_params[3], out int xMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[3]));
                            return;
                        }
                        if (!int.TryParse(_params[4], out int zMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[4]));
                            return;
                        }
                        int[] vectors = new int[5];
                        vectors[0] = (xMin < xMax ? xMin : xMax) - 2;
                        vectors[1] = (zMin < zMax ? zMin : zMax) - 2;
                        vectors[2] = (xMax > xMin ? xMax : xMin) + 2;
                        vectors[3] = (zMax > zMin ? zMax : zMin) + 2;
                        vectors[4] = 1;
                        if (!ProtectedZones.ProtectedList.Contains(vectors))
                        {
                            ProtectedZones.ProtectedList.Add(vectors);
                            ProtectedZones.UpdateXml();
                            ProtectedZones.LoadXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected zone from {0}x,{1}z to {2}x,{3}z", vectors[0], vectors[1], vectors[2], vectors[3]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This protected zone is already on the list"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("cancel"))
                {
                    if (_senderInfo.RemoteClientInfo == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You are not in game. There is nothing to cancel"));
                        return;
                    }
                    if (Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        Vectors.Remove(_senderInfo.RemoteClientInfo.entityId);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Cancelled your saved corner positions"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have no saved position for a protected zone. Use add in the first corner you want to protect"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (ProtectedZones.ProtectedList.Count > 0)
                        {
                            if (_params[1].ToLower().Equals("chunks"))
                            {
                                if (_senderInfo.RemoteClientInfo != null)
                                {
                                    ProtectedZones.ClearSurroundingChunkProtection(_senderInfo.RemoteClientInfo);
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] Cleared the protected zones that overlap the chunk you are standing in and the surrounding chunks");
                                    return;
                                }
                            }
                            else if (int.TryParse(_params[1], out int _listNum))
                            {
                                if (ProtectedZones.ProtectedList.Count >= _listNum)
                                {
                                    int[] _vectors = ProtectedZones.ProtectedList[_listNum - 1];
                                    ProtectedZones.ProtectedList.Remove(_vectors);
                                    ProtectedZones.UpdateXml();
                                    ProtectedZones.LoadXml();
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] Removed protected zone {0}: {1}x,{2}z to {3}x,{4}z", _listNum, _vectors[0], _vectors[1], _vectors[2], _vectors[3]);
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid list number '{0}'", _params[1]);
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid argument '{0}'", _params[1]);
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] There are no protected zones");
                            return;
                        }
                    }
                    else if (_senderInfo.RemoteClientInfo != null)
                    {
                        ProtectedZones.ClearSingleChunkProtection(_senderInfo.RemoteClientInfo);
                        ProtectedZones.UpdateXml();
                        ProtectedZones.LoadXml();
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Cleared the protected zones that overlap the chunk you are standing in");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (ProtectedZones.ProtectedList.Count > 0)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Protected zones list:");
                        for (int i = 0; i < ProtectedZones.ProtectedList.Count; i++)
                        {
                            int[] _vectors = ProtectedZones.ProtectedList[i];
                            if (_vectors[4] == 1)
                            {
                                SdtdConsole.Instance.Output("#{0}: {1}x,{2}z to {3}x,{4}z is active", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]);
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("#{0}: {1}x,{2}z to {3}x,{4}z is deactivated", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]);
                            }
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no protected zones");
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("active"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count);
                        return;
                    }
                    int.TryParse(_params[1], out int listEntry);
                    if (ProtectedZones.ProtectedList.Count >= listEntry)
                    {
                        int[] _protectedSpace = ProtectedZones.ProtectedList[listEntry - 1];
                        if (_protectedSpace[4] == 1)
                        {
                            _protectedSpace[4] = 0;
                            ProtectedZones.ProtectedList[listEntry - 1] = _protectedSpace;
                            ProtectedZones.UpdateXml();
                            ProtectedZones.LoadXml();
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Deactivated protected zone #{0}", listEntry);
                        }
                        else
                        {
                            _protectedSpace[4] = 1;
                            ProtectedZones.ProtectedList[listEntry - 1] = _protectedSpace;
                            ProtectedZones.UpdateXml();
                            ProtectedZones.LoadXml();
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Activated protected zone #{0}", listEntry);
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] This number does not exist on the protected zones list. Unable to active or deactivate");
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid argument '{0}'", _params[0]);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in ProtectedZonesConsole.Execute: {0}", e.Message);
            }
        }
    }
}
