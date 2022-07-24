using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WaypointConsole : ConsoleCmdAbstract
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
                   "  3. st-wp add <Name> <Cost>\n" +
                   "  4. st-wp add <Name> <X> <Y> <Z> <Cost>\n" +
                   "  5. st-hm add spot <EOS/EntityId/PlayerName>\n" +
                   "  6. st-hm remove spot <EOS/EntityId/PlayerName>\n" +
                   "  7. st-wp remove <Name>\n" +
                   "  8. st-wp list\n" +
                   "1. Turn off waypoint tool\n" +
                   "2. Turn on waypoint tool\n" +
                   "3. Add a public waypoint with the specified name. Your current position in the world will be used\n" +
                   "4. Add a public waypoint with the specified name and location\n" +
                   "5. Add one extra waypoint spot for a player\n" +
                   "6. Remove one extra waypoint spot for a player\n" +
                   "7. Delete the pubic waypoint with the specified name\n" +
                   "8. Show a list of public waypoints\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Waypoint", "wp", "st-wp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 3 && _params.Count != 6)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 3 or 6, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Waypoints has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Waypoints is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Waypoints has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Waypoints is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params[1].ToLower().Equals("spot"))
                    {
                        if (_params.Count != 3)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                            return;
                        }
                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                            {
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots += 1;
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added a Waypoint spot for '{0}'", _params[1]));
                            }
                        }
                        else if (PersistentContainer.Instance.Players[_params[1]] != null)
                        {
                            PersistentContainer.Instance.Players[_params[1]].WaypointSpots += 1;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added a Waypoint spot for '{0}'", _params[1]));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to add a Waypoint spot", _params[1]));
                        }
                    }
                    else
                    {
                        if (_params.Count == 3)
                        {
                            if (_senderInfo.RemoteClientInfo == null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No client info found. Join the server as a client before using this command"));
                                return;
                            }
                            EntityPlayer player = PersistentOperations.GetEntityPlayer(_senderInfo.RemoteClientInfo.entityId);
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
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost '{0}'", _params[2]));
                                    return;
                                }
                                string[] waypoint = { position, cost };
                                if (!Waypoints.Dict.ContainsKey(name))
                                {
                                    Waypoints.Dict.Add(name, waypoint);
                                    Waypoints.UpdateXml();
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named '{0}' at position '{1}'. Cost set to '{2}'", name, position, cost));
                                    return;
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                                    return;
                                }
                            }
                        }
                        else if (_params.Count == 6)
                        {
                            string name = _params[1];
                            int.TryParse(_params[2], out int x);
                            int.TryParse(_params[3], out int y);
                            int.TryParse(_params[4], out int z);
                            string position = x + "," + y + "," + z;
                            string cost = _params[5];
                            if (!int.TryParse(_params[5], out int commandCost))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid command cost '{0}'", _params[5]));
                                return;
                            }
                            string[] waypoint = { position, cost };
                            if (!Waypoints.Dict.ContainsKey(name))
                            {
                                Waypoints.Dict.Add(name, waypoint);
                                Waypoints.UpdateXml();
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added public waypoint named '{0}' at position '{1}'. Cost set to '{2}'", name, position, cost));
                                return;
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] A public waypoint with this name already exists. Use a unique name or delete the other waypoint first"));
                                return;
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params[1].ToLower().Equals("spot"))
                    {
                        if (_params.Count != 3)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                            return;
                        }
                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                            {
                                if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots > 0)
                                {
                                    PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].WaypointSpots -= 1;
                                    PersistentContainer.DataChange = true;
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed a Waypoint spot for '{0}'", _params[1]));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' has no Waypoint spots to remove", _params[1]));
                                }
                            }
                        }
                        else if (PersistentContainer.Instance.Players[_params[1]] != null)
                        {
                            if (PersistentContainer.Instance.Players[_params[1]].WaypointSpots > 0)
                            {
                                PersistentContainer.Instance.Players[_params[1]].WaypointSpots -= 1;
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed a Waypoint spot for '{0}'", _params[1]));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' has no Waypoint spots to remove", _params[1]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to remove a Waypoint spot", _params[1]));
                        }
                    }
                    else
                    {
                        string name = _params[1];
                        if (Waypoints.Dict.ContainsKey(name))
                        {
                            Waypoints.Dict.Remove(name);
                            Waypoints.UpdateXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed waypoint named '{0}'", name));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove waypoint named '{0}'. Waypoint not found", name));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (Waypoints.Dict.Count > 0)
                    {
                        foreach (var _waypoint in Waypoints.Dict)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Waypoint named '{0}' @ position '{1}' with a cost of '{2}' '{3}'", _waypoint.Key, _waypoint.Value[0], _waypoint.Value[1], Wallet.Currency_Name));
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no waypoints on the list");
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WaypointsConsole.Execute: {0}", e.Message));
            }
        }
    }
}