using System;
using System.Collections.Generic;


namespace ServerTools
{
    class ProtectedSpaceConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enabled, Disable, Add, Remove or List protected spaces";
        }
    
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ProtectedSpaces off\n" +
                   "  2. ProtectedSpaces on\n" +
                   "  3. ProtectedSpaces add\n" +
                   "  4. ProtectedSpaces add <x> <z> <x> <z>\n" +
                   "  5. ProtectedSpaces cancel\n" +
                   "  6. ProtectedSpaces remove\n" +
                   "  7. ProtectedSpaces remove <#>\n" +
                   "  8. ProtectedSpaces list\n" +
                   "1. Turn off ProtectedSpaces\n" +
                   "2. Turn on ProtectedSpaces\n" +
                   "3. Add a protected space. Stand in the south west corner, use add, stand in the north east corner and use add again\n" +
                   "4. Add a protected space. Set the south west corner and north east corner using specific coordinates\n" +
                   "5. Cancels the first saved point you set by using the add command\n" +
                   "6. Remove the entire protected space you are standing in, if one exists\n" +
                   "7. Remove a specific protected space from the list. The number is shown in the list\n" +
                   "8. Shows the protected spaces list\n" +
                   "*Make sure the corners used are opposite each other. Example NW with SE or SW with NE*\n";
        }
    
        public override string[] GetCommands()
        {
            return new string[] { "st-ProtectedSpaces", "protectedspaces", "st-ps", "ps" };
        }
    
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 5)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, 2 or 5, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ProtectedSpaces.IsEnabled)
                    {
                        ProtectedSpaces.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Protected_Spaces has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Protected spaces is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ProtectedSpaces.IsEnabled)
                    {
                        ProtectedSpaces.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Protected_Spaces has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Protected spaces is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 1 && _params.Count != 5)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 5, found {0}", _params.Count));
                        return;
                    }
                    if (_params.Count == 1)
                    {
                        if (_senderInfo.RemoteClientInfo == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("Not a client"));
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
                                SdtdConsole.Instance.Output(string.Format("The first position has been set to {0}x,{1}z", _x, _z));
                                SdtdConsole.Instance.Output("Stand in the opposite corner and use add. Use cancel to clear the saved location and start again");
                                return;
                            }
                            else
                            {
                                string[] _vector1;
                                ProtectedSpaces.Vectors.TryGetValue(_player.entityId, out _vector1);
                                ProtectedSpaces.Vectors.Remove(_player.entityId);
                                int xMin = int.Parse(_vector1[0]), zMin = int.Parse(_vector1[1]), xMax = (int)_player.position.x, zMax = (int)_player.position.z;
                                int _xMinAlt = xMin, _zMinAlt = zMin, _xMaxAlt = xMax, _zMaxAlt = zMax;
                                if (xMin > xMax)
                                {
                                    _xMinAlt = xMax;
                                    _xMaxAlt = xMin;
                                }
                                if (zMin > zMax)
                                {
                                    _zMinAlt = zMax;
                                    _zMaxAlt = zMin;
                                }
                                string[] _vector = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
                                if (!ProtectedSpaces.ProtectedList.Contains(_vector))
                                {
                                    ProtectedSpaces.Add(_vector);
                                    SdtdConsole.Instance.Output(string.Format("Added protected space from {0},{1} to {2},{3}", _xMinAlt, _zMinAlt, _xMaxAlt, _zMaxAlt));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("This protected space is already on the list"));
                                    return;
                                }
                            }
                        }
                    }
                    else if (_params.Count == 5)
                    {
                        int xMin, zMin, xMax, zMax;
                        if (!int.TryParse(_params[1], out xMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[1]));
                            return;
                        }
                        if (!int.TryParse(_params[2], out zMin))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[2]));
                            return;
                        }
                        if (!int.TryParse(_params[3], out xMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[3]));
                            return;
                        }
                        if (!int.TryParse(_params[4], out zMax))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[4]));
                            return;
                        }
                        int _xMinAlt = xMin, _zMinAlt = zMin, _xMaxAlt = xMax, _zMaxAlt = zMax;
                        if (xMin > xMax)
                        {
                            _xMinAlt = xMax;
                            _xMaxAlt = xMin;
                        }
                        if (zMin > zMax)
                        {
                            _zMinAlt = zMax;
                            _zMaxAlt = zMin;
                        }
                        string[] _vector = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
                        if (!ProtectedSpaces.ProtectedList.Contains(_vector))
                        {
                            ProtectedSpaces.Add(_vector);
                            SdtdConsole.Instance.Output(string.Format("Added protected space from {0},{1} to {2},{3}", _xMinAlt, _zMinAlt, _xMaxAlt, _zMaxAlt));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("This protected space is already on the list"));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("cancel"))
                {
                    if (_senderInfo.RemoteClientInfo == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Not a client"));
                        return;
                    }
                    if (ProtectedSpaces.IsEnabled && ProtectedSpaces.Vectors.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
                    {
                        ProtectedSpaces.Vectors.Remove(_senderInfo.RemoteClientInfo.entityId);
                        SdtdConsole.Instance.Output(string.Format("Cancelled the saved first position for a protected space. Use add to start again"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("You have no saved position for a protected space. Use add in the first corner you want to protect"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (ProtectedSpaces.ProtectedList.Count > 0)
                    {
                        if (_params.Count == 2)
                        {
                            int _listNum;
                            if (int.TryParse(_params[1], out _listNum))
                            {
                                if (ProtectedSpaces.ProtectedList.Count >= _listNum)
                                {
                                    string[] _vectors = ProtectedSpaces.ProtectedList[_listNum - 1];
                                    ProtectedSpaces.Remove(_vectors);
                                    SdtdConsole.Instance.Output(string.Format("Removed protected spaces list entry {0}: {1},{2} to {3},{4}", _listNum, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("Invalid list number: {0}", _params[1]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Invalid integer: {0}", _params[1]));
                                return;
                            }
                        }
                        else if (_senderInfo.RemoteClientInfo != null)
                        {
                            PersistentOperations.ClearChunkProtection(_senderInfo.RemoteClientInfo);
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("There are no protected spaces"));
                        return;
                    }
                    
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (ProtectedSpaces.ProtectedList.Count > 0)
                    {
                        SdtdConsole.Instance.Output(string.Format("Protected spaces list:"));
                        for (int i = 0; i < ProtectedSpaces.ProtectedList.Count; i++)
                        {
                            string[] _vectors = ProtectedSpaces.ProtectedList[i];
                            SdtdConsole.Instance.Output(string.Format("#{0}: {1},{2} to {3},{4}", i + 1, _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("There are no protected spaces"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaceConsole.Execute: {0}", e.Message));
            }
        }
    }
}
