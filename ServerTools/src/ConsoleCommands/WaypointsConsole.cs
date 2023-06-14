using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WaypointConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable waypoints.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-wp off\n" +
                   "  2. st-wp on\n" +
                   "  3. st-wp add <Name> <Cost>\n" +
                   "  4. st-wp add <Name> \"<X> <Y> <Z>\" <Cost>\n" +
                   "  5. st-wp remove <Name>\n" +
                   "  6. st-wp increase <EOS/EntityId/PlayerName>\n" +
                   "  7. st-wp decrease <EOS/EntityId/PlayerName>\n" +
                   "  8. st-wp show <EOS/EntityId/PlayerName>\n" +
                   "  9. st-wp list\n" +
                   "1. Turn off waypoint tool\n" +
                   "2. Turn on waypoint tool\n" +
                   "3. Add a public waypoint with the specified name. Your current position in the world will be used\n" +
                   "4. Add a public waypoint with the specified name and location\n" +
                   "5. Delete the pubic waypoint with the specified name\n" +
                   "6. Add one extra waypoint spot for a player\n" +
                   "7. Remove one extra waypoint spot for a player\n" +
                   "8. Shows how many extra waypoint spots a player has\n" +
                   "9. Show a list of public waypoints\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Waypoint", "wp", "st-wp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {

                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
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
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoints is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("increase"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots += 1;
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added a Waypoint spot for '{0}' named '{1}'. They now have '{2}'.", _params[1], cInfo.playerName, PersistentContainer.Instance.Players[_params[1]].WaypointSpots));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].WaypointSpots += 1;
                        PersistentContainer.DataChange = true;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added a Waypoint spot for '{0}' named '{1}'. They now have '{2}'.", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, PersistentContainer.Instance.Players[_params[1]].WaypointSpots));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to add a Waypoint spot", _params[1]));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("decrease"))
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots > 0)
                            {
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots -= 1;
                                PersistentContainer.DataChange = true;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed a Waypoint spot for '{0}' named '{1}'. They now have '{2}'.", _params[1], cInfo.playerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has no Waypoint spots to remove", _params[2], cInfo.playerName));
                            }
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].WaypointSpots > 0)
                        {
                            PersistentContainer.Instance.Players[_params[1]].WaypointSpots -= 1;
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed a Waypoint spot for '{0}' named '{1}'. They now have '{2}'.", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has no Waypoint spots to remove", _params[2], PersistentContainer.Instance.Players[_params[2]].PlayerName));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to remove a Waypoint spot", _params[2]));
                    }
                    return;
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
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
                        if (player != null)
                        {
                            int x = (int)player.position.x;
                            int y = (int)player.position.y;
                            int z = (int)player.position.z;
                            string position = x + "," + y + "," + z;
                            string name = _params[1];
                            string cost = _params[2];
                            if (!int.TryParse(_params[2], out int _commandCost))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost '{0}'", _params[2]));
                                return;
                            }
                            string[] waypoint = { position, cost };
                            if (!Waypoints.Dict.ContainsKey(name))
                            {
                                Waypoints.Dict.Add(name, waypoint);
                                Waypoints.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named '{0}' at position '{1}'. Cost set to '{2}'", name, position, cost));
                                return;
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                                return;
                            }
                        }
                    }
                    else if (_params.Count == 4)
                    {
                        string name = _params[1];
                        string[] positionSplit = new string[3];
                        if (_params[2].Contains(" "))
                        {
                            positionSplit = _params[2].Split(' ');
                        }
                        else if (_params[2].Contains(","))
                        {
                            positionSplit = _params[2].Split(',');
                        }
                        int.TryParse(positionSplit[0], out int x);
                        int.TryParse(positionSplit[1], out int y);
                        int.TryParse(positionSplit[2], out int z);
                        string position = x + "," + y + "," + z;
                        string cost = _params[3];
                        if (!int.TryParse(_params[3], out int commandCost))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost '{0}'", _params[4]));
                            return;
                        }
                        string[] waypoint = { position, cost };
                        if (!Waypoints.Dict.ContainsKey(name))
                        {
                            Waypoints.Dict.Add(name, waypoint);
                            Waypoints.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named '{0}' at position '{1}'. Cost set to '{2}'", name, position, cost));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 4, found '{0}'", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        string name = _params[1];
                        if (Waypoints.Dict.ContainsKey(name))
                        {
                            Waypoints.Dict.Remove(name);
                            Waypoints.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed public waypoint named '{0}'", name));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove public waypoint named '{0}'. Waypoint not found", name));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 3, found '{0}'", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("show"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has '{2}' Waypoint spots", _params[1], cInfo.playerName, PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has '{2}' Waypoint spots", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName, PersistentContainer.Instance.Players[_params[1]].WaypointSpots));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to show Waypoint spots", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (Waypoints.Dict.Count > 0)
                    {
                        foreach (var _waypoint in Waypoints.Dict)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Waypoint named '{0}' @ position '{1}' with a cost of '{2}' '{3}'", _waypoint.Key, _waypoint.Value[0], _waypoint.Value[1], Wallet.Currency_Name));
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
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WaypointsConsole.Execute: {0}", e.Message));
            }
        }
    }
}