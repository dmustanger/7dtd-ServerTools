using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunity : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Add, Remove and View steamids on the PingImmunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. pingimmunity add <steamId/entityId> <playerName>\n" +
                   "  2. pingimmunity remove <steamId/entityId> <playerName>\n" +
                   "  3. pingimmunity list\n" +
                   "1. Adds a steamID  and name to the Ping Immunity list\n" +
                   "2. Removes a steamID from the Ping Immunity list\n" +
                   "3. Lists all steamIDs that have Ping Immunity";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-PingImmunity", "pingimmunity", "pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 || _params.Count != 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    if (_params[2].Length < 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}", _params[2]));
                        return;
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            if (HighPingKicker.Dict.ContainsKey(_cInfo.playerId))
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Ping Immunity list.", _params[1]));
                                return;
                            }
                            else
                            {
                                HighPingKicker.Dict.Add(_cInfo.playerId, _params[2]);
                                SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the Ping Immunity list.", _params[1], _params[2]));
                                HighPingKicker.UpdateXml();
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
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (!HighPingKicker.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("Id {0} was not found.", _params[1]));
                            return;
                        }
                        HighPingKicker.Dict.Remove(_cInfo.playerId);
                        SdtdConsole.Instance.Output(string.Format("Removed Id {0} from Ping Immunity list.", _params[1]));
                        HighPingKicker.UpdateXml();
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (HighPingKicker.Dict.Count < 1)
                    {
                        SdtdConsole.Instance.Output("There are no Ids on the Ping Immunity list.");
                        return;
                    }
                    foreach (KeyValuePair<string, string> _key in HighPingKicker.Dict)
                    {
                        SdtdConsole.Instance.Output(string.Format("{0} {1}", _key.Key, _key.Value));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PingImmunityCommand.Run: {0}.", e));
            }
        }
    }
}