using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunity : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Add, Remove and View steamids on the PingImmunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. pingimmunity add <steamID> <player name>\n" +
                   "  2. pingimmunity remove <steamID>\n" +
                   "  3. pingimmunity list\n" +
                   "1. Adds a steamID  and name to the Ping Immunity list\n" +
                   "2. Removes a steamID from the Ping Immunity list\n" +
                   "3. Lists all steamIDs that have Ping Immunity";
        }

        public override string[] GetCommands()
        {
            return new string[] { "pingimmunity", "pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId: Invalid SteamId {0}", _params[1]));
                        return;
                    }
                    if (HighPingKicker.Dict.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId. {0} is already in the Ping Immunity list.", _params[1]));
                        return;
                    }
                    HighPingKicker.Dict.Add(_params[1], _params[2]);
                    SdtdConsole.Instance.Output(string.Format("Added SteamId {0} with the name of {1} the Ping Immunity list.", _params[1], _params[2]));
                    HighPingKicker.UpdateXml();
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!HighPingKicker.Dict.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("SteamId {0} was not found.", _params[1]));
                        return;
                    }
                    HighPingKicker.Dict.Remove(_params[1]);
                    SdtdConsole.Instance.Output(string.Format("Removed SteamId {0} from Ping Immunity list.", _params[1]));
                    HighPingKicker.UpdateXml();
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
                        SdtdConsole.Instance.Output("There are no steamIds on the Ping Immunity list.");
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