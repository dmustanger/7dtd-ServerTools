﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunityConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Add, remove and view ids on the ping immunity list.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-pi add <EOS/EntityId/PlayerName>\n" +
                   "  2. st-pi remove <EOS/EntityId/PlayerName>\n" +
                   "  3. st-pi list\n" +
                   "1. Adds a EOS to the ping immunity list\n" +
                   "2. Removes a EOS from the ping immunity list\n" +
                   "3. Shows all EOS that have ping immunity" +
                   "4. *Note* You can use the entity id or player name if they are online otherwise use their EOS";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-PingImmunity", "pi", "st-pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (HighPingKicker.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. '{0}' is already in the ping immunity list", _params[1]));
                        }
                        else
                        {
                            HighPingKicker.Dict.Add(_cInfo.CrossplatformId.CombinedString, _cInfo.playerName);
                            HighPingKicker.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id '{0}' to the ping immunity list", _params[1]));
                        }
                    }
                    else if (_params[1].Contains("_"))
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            if (HighPingKicker.Dict.ContainsKey(ppd.UserIdentifier.CombinedString))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. '{0}' named '{1}' is already in the ping immunity list", ppd.UserIdentifier.CombinedString, ppd.PlayerName));
                            }
                            else
                            {
                                HighPingKicker.Dict.Add(ppd.UserIdentifier.CombinedString, ppd.PlayerName);
                                HighPingKicker.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id '{0}' named '{1}' to the ping immunity list", ppd.UserIdentifier.CombinedString, ppd.PlayerName));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. No data for '{0}' could not be found", _params[1]));
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        if (!HighPingKicker.Dict.ContainsKey(cInfo.CrossplatformId.CombinedString))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id '{0}'. It is not in the ping immunity list", _params[1]));
                        }
                        else
                        {
                            HighPingKicker.Dict.Remove(cInfo.CrossplatformId.CombinedString);
                            HighPingKicker.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id '{0}' from the ping immunity list", _params[1]));
                        }
                    }
                    else if (_params[1].Contains("_"))
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            if (!HighPingKicker.Dict.ContainsKey(_params[1]))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id '{0}'. It is not in the ping immunity list", _params[1]));
                            }
                            else
                            {
                                HighPingKicker.Dict.Remove(_params[1]);
                                HighPingKicker.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from the ping immunity list", _params[1]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id '{0}'. No data could not be found", _params[1]));
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    else
                    {
                        if (HighPingKicker.Dict.Count == 0)
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] There are no players on the ping immunity list");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High ping immunity list:"));
                            foreach (KeyValuePair<string, string> _c in HighPingKicker.Dict)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' named '{1}'", _c.Key, _c.Value));
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PingImmunityCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}