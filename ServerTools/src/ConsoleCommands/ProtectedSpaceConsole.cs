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
                   "1. Turn off ProtectedSpaces\n" +
                   "2. Turn on ProtectedSpaces\n" +
                   "3. Add a protected space. Stand in the south west corner, use add, stand in the north east corner and use add again\n" +
                   "4. Add a protected space. Set the south west corner and north east corner using specific coordinates\n" +
                   "5. Cancels the saved corner positions\n" +
                   "6. Remove the entire protected space you are standing in, if one exists\n" +
                   "7. Remove a specific protected space from the list. The number is shown in the list\n" +
                   "8. Shows the protected spaces list\n" +
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
                        EntityPlayer _player = PersistentOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.playerId);
                        if (_player != null)
                        {
                            if (!ProtectedSpaces.Vectors.ContainsKey(_player.entityId))
                            {
                                int _x = (int)_player.position.x, _z = (int)_player.position.z;
                                string[] _vector1 = { _x.ToString(), _z.ToString() };
                                ProtectedSpaces.Vectors.Add(_player.entityId, _vector1);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] The first position has been set to {0}x,{1}z", _x, _z));
                                SdtdConsole.Instance.Output("[SERVERTOOLS] Stand in the opposite corner and use add again. Use cancel to clear the saved location and start again");
                                return;
                            }
                            else
                            {
                                ProtectedSpaces.Vectors.TryGetValue(_player.entityId, out string[] _vector1);
                                ProtectedSpaces.Vectors.Remove(_player.entityId);
                                int xMin = int.Parse(_vector1[0]), zMin = int.Parse(_vector1[1]), xMax = (int)_player.position.x, zMax = (int)_player.position.z;
                                int _xMinFix = xMin, _zMinFix = zMin, _xMaxFix = xMax, _zMaxFix = zMax;
                                if (xMin > xMax)
                                {
                                    _xMinFix = xMax;
                                    _xMaxFix = xMin;
                                }
                                if (zMin > zMax)
                                {
                                    _zMinFix = zMax;
                                    _zMaxFix = zMin;
                                }
                                string[] _vectors = { _xMinFix.ToString(), _zMinFix.ToString(), _xMaxFix.ToString(), _zMaxFix.ToString() };
                                if (!ProtectedSpaces.Protected.Contains(_vectors))
                                {
                                    ProtectedSpaces.Protected.Add(_vectors);
                                    ProtectedSpaces.UpdateXml();
                                    List<string[]> _protected = new List<string[]>();
                                    _protected.Add(_vectors);
                                    ProtectedSpaces.AddProtection(_protected);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected space from {0},{1} to {2},{3}", _xMinFix, _zMinFix, _xMaxFix, _zMaxFix));
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
                        int _xMinFix = xMin, _zMinFix = zMin, _xMaxFix = xMax, _zMaxFix = zMax;
                        if (xMin > xMax)
                        {
                            _xMinFix = xMax;
                            _xMaxFix = xMin;
                        }
                        if (zMin > zMax)
                        {
                            _zMinFix = zMax;
                            _zMaxFix = zMin;
                        }
                        string[] _vectors = { _xMinFix.ToString(), _zMinFix.ToString(), _xMaxFix.ToString(), _zMaxFix.ToString() };
                        if (!ProtectedSpaces.Protected.Contains(_vectors))
                        {
                            ProtectedSpaces.Protected.Add(_vectors);
                            ProtectedSpaces.UpdateXml();
                            List<string[]> _protected = new List<string[]>();
                            _protected.Add(_vectors);
                            ProtectedSpaces.AddProtection(_protected);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added protected space from {0},{1} to {2},{3}", _xMinFix, _zMinFix, _xMaxFix, _zMaxFix));
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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Not a client"));
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
                        if (ProtectedSpaces.Protected.Count > 0)
                        {
                            int _listNum;
                            if (int.TryParse(_params[1], out _listNum))
                            {
                                if (ProtectedSpaces.Protected.Count >= _listNum)
                                {
                                    string[] _vectors = ProtectedSpaces.Protected[_listNum - 1];
                                    ProtectedSpaces.Protected.Remove(_vectors);
                                    ProtectedSpaces.UpdateXml();
                                    List<string[]> _protected = new List<string[]>();
                                    _protected.Add(_vectors);
                                    ProtectedSpaces.RemoveProtection(_protected);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed protected space {0}: {1},{2} to {3},{4}", _listNum, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
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
                    if (ProtectedSpaces.Protected.Count > 0)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Protected spaces list:"));
                        for (int i = 0; i < ProtectedSpaces.Protected.Count; i++)
                        {
                            string[] _vectors = ProtectedSpaces.Protected[i];
                            SdtdConsole.Instance.Output(string.Format("#{0}: {1},{2} to {3},{4}", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] There are no protected spaces"));
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
