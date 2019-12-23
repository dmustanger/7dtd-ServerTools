using System;
using System.Collections.Generic;


namespace ServerTools
{
    //class ProtectedSpaceConsole : ConsoleCmdAbstract
    //{
    //    public override string GetDescription()
    //    {
    //        return "[ServerTools]- Add, remove or list protected spaces";
    //    }
    //
    //    public override string GetHelp()
    //    {
    //        return "Usage:\n" +
    //               "  1. ProtectedSpace add\n" +
    //               "  2. ProtectedSpace add x,z x,z\n" +
    //               "  3. ProtectedSpace cancel\n" +
    //               "  4. ProtectedSpace remove\n" +
    //               "  5. ProtectedSpace remove #\n" +
    //               "  6. ProtectedSpace list\n" +
    //               "1. Add a protected space. Stand in one corner, use add, stand in the opposite corner and use add again\n" +
    //               "2. Add a protected space. Set two opposite corners of the space using the coordinates\n" +
    //               "3. \n" +
    //               "4. Remove protected space where you are standing if one exists\n" +
    //               "5. Remove a specific protected space from the list\n" +
    //               "6. List the protected spaces\n";
    //    }
    //
    //    public override string[] GetCommands()
    //    {
    //        return new string[] { "st-ProtectedSpace", "protectedspace", "st-ps", "ps" };
    //    }
    //
    //    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    //    {
    //        try
    //        {
    //            if (_params.Count < 1 || _params.Count > 3)
    //            {
    //                SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
    //                return;
    //            }
    //            if (_params[0].ToLower().Equals("add"))
    //            {
    //                if (_params.Count != 1 || _params.Count != 3)
    //                {
    //                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 3, found {0}", _params.Count));
    //                    return;
    //                }
    //                if (_params.Count == 1)
    //                {
    //                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.playerId);
    //                    if (_player != null)
    //                    {
    //                        if (!ProtectedSpace.Vectors.ContainsKey(_player.entityId))
    //                        {
    //                            ProtectedSpace.Vectors.Add(_player.entityId, new string[] { _player.position.x.ToString(), _player.position.z.ToString() });
    //                            SdtdConsole.Instance.Output(string.Format("Position one has been set to {0},{1}", (int)_player.position.x, (int)_player.position.z));
    //                            SdtdConsole.Instance.Output("Stand in the opposite corner and use add again");
    //                            return;
    //                        }
    //                        else
    //                        {
    //                            string[] _vector1;
    //                            ProtectedSpace.Vectors.TryGetValue(_player.entityId, out _vector1);
    //                            int xMin, zMin, xMax, zMax;
    //                            int.TryParse(_vector1[0], out xMin);
    //                            int.TryParse(_vector1[1], out zMin);
    //                            xMax = (int)_player.position.x;
    //                            zMax = (int)_player.position.z;
    //                            if (xMin > (int)_player.position.x)
    //                            {
    //                                xMax = xMin;
    //                                xMin = (int)_player.position.x;
    //                            }
    //                            if (zMin > (int)_player.position.z)
    //                            {
    //                                zMax = zMin;
    //                                zMin = (int)_player.position.z;
    //                            }
    //                            string[] _bothVector = { xMin.ToString(), zMin.ToString(), xMax.ToString(), zMax.ToString() };
    //                            if (!ProtectedSpace.ProtectedList.Contains(_bothVector))
    //                            {
    //                                ProtectedSpace.ProtectedList.Add(_bothVector);
    //                            }
    //                            else
    //                            {
    //                                SdtdConsole.Instance.Output(string.Format("This location is already on the list"));
    //                                return;
    //                            }
    //                            ProtectedSpace.Add(xMin, zMin, xMax, zMax);
    //                            SdtdConsole.Instance.Output(string.Format("Added protected space to the list"));
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (!_params[1].Contains(","))
    //                    {
    //                        SdtdConsole.Instance.Output(string.Format("Wrong set of arguments, expected x,z found {0}", _params[1]));
    //                        return;
    //                    }
    //                    if (!_params[2].Contains(","))
    //                    {
    //                        SdtdConsole.Instance.Output(string.Format("Wrong set of arguments, expected x,z found {0}", _params[2]));
    //                        return;
    //                    }
    //                    string[] _vector1 = _params[1].Split(',');
    //                    string[] _vector2 = _params[2].Split(',');
    //
    //                }
    //            }
    //            else if (_params[0].ToLower().Equals("remove"))
    //            {
    //                
    //                return;
    //            }
    //            else if (_params[0].ToLower().Equals("list"))
    //            {
    //
    //                return;
    //            }
    //            else
    //            {
    //                SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaceConsole.Execute: {0}.", e.Message));
    //        }
    //    }
    //}
}
