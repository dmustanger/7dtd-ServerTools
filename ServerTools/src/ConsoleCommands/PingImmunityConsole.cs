using System;
using System.Data;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Add, Remove and View steamids on the PingImmunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. pingimmunity add <steamId/entityId/playerName>\n" +
                   "  2. pingimmunity remove <steamId/entityId/playerName>\n" +
                   "  3. pingimmunity list\n" +
                   "1. Adds a steam ID to the Ping Immunity list\n" +
                   "2. Removes a steam ID from the Ping Immunity list\n" +
                   "3. Lists all steam IDs that have Ping Immunity" +
                   "4. *Note* You can use the player id or name if they are online otherwise use their steam Id";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-PingImmunity", "pingimmunity", "pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        bool _highPingImmune = PersistentContainer.Instance.Players[_cInfo.playerId].HighPingImmune;
                        if (_highPingImmune)
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Ping Immunity list.", _params[1]));
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].HighPingImmune = true;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Added Id {0} to the Ping Immunity list.", _params[1]));
                        }
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("Player must be online or when offline use their steam Id which must be 17 numbers in length. Can not add Id: {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            {
                                if (p != null)
                                {
                                    bool _highPingImmune = PersistentContainer.Instance.Players[_params[1]].HighPingImmune;
                                    if (_highPingImmune)
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Ping Immunity list.", _params[1]));
                                    }
                                    else
                                    {
                                        PersistentContainer.Instance.Players[_params[1]].HighPingImmune = true;
                                        PersistentContainer.Instance.Save();
                                        SdtdConsole.Instance.Output(string.Format("Added Id {0} to the Ping Immunity list.", _params[1]));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Player must be online or when offline use their steam Id which must be 17 numbers in length. Can not add Id: {0}", _params[1]));
                        return;
                    }
                    else
                    {
                        PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                        {
                            if (p != null)
                            {
                                bool _highPingImmune = PersistentContainer.Instance.Players[_params[1]].HighPingImmune;
                                if (_highPingImmune)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Can not remove Id. {0} is not in the Ping Immunity list.", _params[1]));
                                }
                                else
                                {
                                    PersistentContainer.Instance.Players[_params[1]].HighPingImmune = true;
                                    PersistentContainer.Instance.Save();
                                    SdtdConsole.Instance.Output(string.Format("Removed Id {0} from the Ping Immunity list.", _params[1]));
                                }
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    else
                    {
                        if (HighPingKicker.Dict.Count == 0)
                        {
                            SdtdConsole.Instance.Output("There are no players on the Ping Immunity list.");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("High Ping Immunity List");
                            foreach (KeyValuePair<string, string> _c in HighPingKicker.Dict)
                            {
                                SdtdConsole.Instance.Output(string.Format("Player Id: {0} with name {1}", _c.Key, _c.Value));
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PingImmunityCommandConsole.Run: {0}.", e));
            }
        }
    }
}