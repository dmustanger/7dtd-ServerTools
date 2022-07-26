using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HomeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable set home";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-hm off\n" +
                   "  2. st-hm on\n" +
                   "  3. st-hm reset <EOS/EntityId/PlayerName>\n" +
                   "  4. st-hm add spot <EOS/EntityId/PlayerName>\n" +
                   "  5. st-hm remove spot <EOS/EntityId/PlayerName>\n" +
                   "1. Turn off set home\n" +
                   "2. Turn on set home\n" +
                   "3. Reset the delay of a player's home tool\n" +
                   "4. Add one extra home spot for a player\n" +
                   "5. Remove one extra home spot for a player\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Home", "hm", "st-hm" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Homes.IsEnabled)
                    {
                        Homes.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Homes has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Homes is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Homes.IsEnabled)
                    {
                        Homes.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Homes has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Homes is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].LastHome = DateTime.Now.AddYears(-1);
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Home tool delay reset for '{0}' named '{1}'", _params[1], cInfo.playerName));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].LastHome = DateTime.Now.AddYears(-1);
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Home tool delay reset for '{0}' named '{1}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player with EOS id '{0}' does not have a Home tool delay to reset", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
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
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HomeSpots += 1;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added a Home spot for '{0}' named '{1}'", _params[1], cInfo.playerName));
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        PersistentContainer.Instance.Players[_params[1]].HomeSpots += 1;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added a Home spot for '{0}' named '{1}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to add a Home spot", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
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
                            if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HomeSpots > 0)
                            {
                                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].HomeSpots -= 1;
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed a Home spot for '{0}'", _params[1]));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has no Home spots to remove", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                            }
                        }
                    }
                    else if (PersistentContainer.Instance.Players[_params[1]] != null)
                    {
                        if (PersistentContainer.Instance.Players[_params[1]].HomeSpots > 0)
                        {
                            PersistentContainer.Instance.Players[_params[1]].HomeSpots -= 1;
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed a Home spot for '{0}' named '{1}'", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' named '{1}' has no Home spots to remove", _params[1], PersistentContainer.Instance.Players[_params[1]].PlayerName));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player '{0}' could not be found. Unable to remove a Home spot", _params[1]));
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HomeConsole.Execute: {0}", e.Message));
            }
        }
    }
}