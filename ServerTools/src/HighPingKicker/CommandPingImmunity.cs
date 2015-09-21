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
                   "  1. pingimmunity add <steamID>\n" +
                   "  2. pingimmunity add <steamID> <player name>\n" +
                   "  3. pingimmunity remove <steamID>\n" +
                   "  4. pingimmunity list\n" +
                   "1. Adds a steamID to the Ping Immunity list\n" +
                   "2. Adds a steamID  and name to the Ping Immunity list\n" +
                   "3. Removes a steamID from the Ping Immunity list\n" +
                   "4. Lists all steamIDs that have Ping Immunity";
        }

        public override string[] GetCommands()
        {
            return new string[] { "pingimmunity", "pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2 && _params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or 3, found {0}.", _params.Count));
                        return;
                    }
                    if (HighPingKicker._whiteListPlayers.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId. {0} is already in the Ping Immunity list.", _params[1]));
                        return;
                    }
                    if (_params.Count == 2)
                    {
                        HighPingKicker._whiteListPlayers.Add(_params[1], null);
                        SdtdConsole.Instance.Output(string.Format("Added SteamId {0} to the Ping Immunity list.", _params[1]));
                    }
                    else
                    {
                        HighPingKicker._whiteListPlayers.Add(_params[1], _params[2]);
                        SdtdConsole.Instance.Output(string.Format("Added SteamId {0} with the name {1} the Ping Immunity list.", _params[1], _params[2]));
                    }
                    HighPingKicker.UpdateXml();
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (!HighPingKicker._whiteListPlayers.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("SteamId {0} was not found.", _params[1]));
                        return;
                    }
                    HighPingKicker._whiteListPlayers.Remove(_params[1]);
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
                    if (HighPingKicker.SteamId.Count < 1)
                    {
                        SdtdConsole.Instance.Output("There are no steamIds on the Ping Immunity list.");
                        return;
                    }
                    foreach (string _steamid in HighPingKicker.SteamId)
                    {
                        SdtdConsole.Instance.Output(_steamid);
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in PingImmunityCommand.Run: {0}.", e));
            }
        }
    }
}