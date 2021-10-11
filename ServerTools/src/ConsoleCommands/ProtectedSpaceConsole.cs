using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ProtectedSpaceConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enabled, disable, add, remove or list protected spaces";
        }
    
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-ps off\n" +
                   "  2. st-ps on\n" +
                   "  3. st-ps add\n" +
                   "  4. st-ps add <x> <z> <x> <z>\n" +
                   "  5. st-ps cancel\n" +
                   "  6. st-ps remove\n" +
                   "  7. st-ps remove <#>\n" +
                   "  8. st-ps list\n" +
                   "  9. st-ps active <#>\n" +
                   "1. Turn off ProtectedSpaces\n" +
                   "2. Turn on ProtectedSpaces\n" +
                   "3. Add a protected space. Stand in the south west corner, use add, stand in the north east corner and use add again\n" +
                   "4. Add a protected space. Set the south west corner and north east corner using specific coordinates\n" +
                   "5. Cancels the saved corner positions\n" +
                   "6. Remove the protection to the chunk you are standing in\n" +
                   "7. Remove a specific protected space from the list. The number is shown in the list\n" +
                   "8. Shows the protected spaces list\n" +
                   "9. Activates and deactivates the protection from the entry in the list\n" +
                   "*Make sure the corners used are opposite each other. Example NW with SE or SW with NE*\n";
        }
    
        public override string[] GetCommands()
        {
            return new string[] { "st-ProtectedSpaces", "ps", "st-ps" };
        }
    
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 5)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2 or 5, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ProtectedSpaces.IsEnabled)
                    {
                        ProtectedSpaces.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ProtectedSpaces.IsEnabled)
                    {
                        ProtectedSpaces.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 1 && _params.Count != 5)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 5, found {0}", _params.Count));
                        return;
                    }
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No client info found. Join the server as a client before using this command"));
                            return;
                        }
                        EntityPlayer player = PersistentOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.playerId);
                        if (player != null)
                        {
                            int x = (int)player.position.x, z = (int)player.position.z;
                            if (!ProtectedSpaces.Vectors.ContainsKey(player.entityId))
                            {
                                int[] vectors = new int[5];
                                vectors[0] = x;
                                vectors[1] = z;
                                ProtectedSpaces.Vectors.Add(player.entityId, vectors);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] The first position has been set to {0}x,{1}z", x, z));
                                SdtdConsole.Instance.Output("[SERVERTOOLS] Stand in the opposite corner and use add again. Use cancel to clear the saved location and start again");
                                return;
                            }
                            else
                            {
                                ProtectedSpaces.Vectors.TryGetValue(player.entityId, out int[] vectors);
                                ProtectedSpaces.Vectors.Remove(player.entityId);
                                if (vectors[0] < x)
                                {
                                    vectors[2] = x;
                                }
                                else
                                {
                                    vectors[2] = vectors[0];
                                    vectors[0] = x;
                                }
                                if (vectors[1] < z)
                                {
                                    vectors[3] = z;
                                }
                                else
                                {
                                    vectors[3] = vectors[1];
                                    vectors[1] = z;
                                }
                                vectors[4] = 1;
                                if (!ProtectedSpaces.ProtectedList.Contains(vectors))
                                {
                                    ProtectedSpaces.ProtectedList.Add(vectors);
                                    ProtectedSpaces.UpdateXml();
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected space from {0}x,{1}z to {2}x,{3}z", vectors[0], vectors[1], vectors[2], vectors[3], vectors[4]));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This protected space is already on the list"));
                                    return;
                                }
                            }
                        }
                    }
                    else if (_params.Count == 5)
                    {
                        if (!int.TryParse(_params[1], out int xMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[1]));
                            return;
                        }
                        if (!int.TryParse(_params[2], out int zMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[2]));
                            return;
                        }
                        if (!int.TryParse(_params[3], out int xMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[3]));
                            return;
                        }
                        if (!int.TryParse(_params[4], out int zMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[4]));
                            return;
                        }
                        int[] vectors = new int[5];
                        if (xMin < xMax)
                        {
                            vectors[0] = xMin;
                            vectors[2] = xMax;
                        }
                        else
                        {
                            vectors[0] = xMax;
                            vectors[2] = xMin;
                        }
                        if (zMin < zMax)
                        {
                            vectors[1] = zMin;
                            vectors[3] = zMax;
                        }
                        else
                        {
                            vectors[1] = zMax;
                            vectors[3] = zMin;
                        }
                        vectors[4] = 1;
                        if (!ProtectedSpaces.ProtectedList.Contains(vectors))
                        {
                            ProtectedSpaces.ProtectedList.Add(vectors);
                            ProtectedSpaces.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected space from {0}x,{1}z to {2}x,{3}z", vectors[0], vectors[1], vectors[2], vectors[3], vectors[4]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This protected space is already on the list"));
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
                    if (ProtectedSpaces.Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        ProtectedSpaces.Vectors.Remove(_senderInfo.RemoteClientInfo.entityId);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Cancelled your saved corner positions"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] You have no saved position for a protected space. Use add in the first corner you want to protect"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (ProtectedSpaces.ProtectedList.Count > 0)
                        {
                            if (int.TryParse(_params[1], out int _listNum))
                            {
                                if (ProtectedSpaces.ProtectedList.Count >= _listNum)
                                {
                                    int[] _vectors = ProtectedSpaces.ProtectedList[_listNum - 1];
                                    ProtectedSpaces.ProtectedList.Remove(_vectors);
                                    ProtectedSpaces.UpdateXml();
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed protected space {0}: {1}x,{2}z to {3}x,{4}z", _listNum, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid list number: {0}", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no protected spaces"));
                            return;
                        }
                    }
                    else if (_senderInfo.RemoteClientInfo != null)
                    {
                        PersistentOperations.ClearChunkProtection(_senderInfo.RemoteClientInfo);
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (ProtectedSpaces.ProtectedList.Count > 0)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces list:"));
                        for (int i = 0; i < ProtectedSpaces.ProtectedList.Count; i++)
                        {
                            int[] _vectors = ProtectedSpaces.ProtectedList[i];
                            if (_vectors[4] == 1)
                            {
                                SdtdConsole.Instance.Output(string.Format("#{0}: {1}x,{2}z to {3}x,{4}z is active", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("#{0}: {1}x,{2}z to {3}x,{4}z is deactivated", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                            }
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no protected spaces"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("active"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    int.TryParse(_params[1], out int _listEntry);
                    if (ProtectedSpaces.ProtectedList.Count >= _listEntry)
                    {
                        int[] _protectedSpace = ProtectedSpaces.ProtectedList[_listEntry - 1];
                        if (_protectedSpace[4] == 1)
                        {
                            _protectedSpace[4] = 0;
                            ProtectedSpaces.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Deactivated protected space #{0}", _listEntry));
                        }
                        else
                        {
                            _protectedSpace[4] = 1;
                            ProtectedSpaces.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Activated protected space #{0}", _listEntry));
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] This number does not exist on the protected spaces list. Unable to active or deactivate"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaceConsole.Execute: {0}", e.Message));
            }
        }
    }
}
