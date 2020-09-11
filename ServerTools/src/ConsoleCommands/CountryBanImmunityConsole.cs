using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class CountryBanImmunityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add, remove and view steam ids on the country ban immunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. cbi add <steamId/entityId/playerName>\n" +
                   "  2. cbi remove <steamId/entityId/playerName>\n" +
                   "  3. cbi list\n" +
                   "1. Adds a steam ID to the Country Ban Immunity list\n" +
                   "2. Removes a steam ID from the Country Ban Immunity list\n" +
                   "3. Lists all steam IDs that have Country Ban Immunity" +
                   "4. *Note* You can use the player id or name if they are online otherwise use their steam Id";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-CountryBanImmunity", "cbi", "st-cbi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_params[1]);
                    if (_cInfo != null)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CountryBanImmune = true;
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} to the Country Ban Immunity list.", _params[1]));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_params[1]);
                    if (_cInfo != null)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].CountryBanImmune = false;
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from the Country Ban Immunity list.", _params[1]));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    List<string> _persistentPlayers = PersistentContainer.Instance.Players.SteamIDs;
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Country Ban Immune List:"));
                    for (int i = 0; i < _persistentPlayers.Count; i++)
                    {
                        string _persistentPlayer = _persistentPlayers[i];
                        if (PersistentContainer.Instance.Players[_persistentPlayer].HardcoreEnabled)
                        {
                            SdtdConsole.Instance.Output(string.Format("Id: {0}", _persistentPlayer));
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBanImmunityConsole.Execute: {0}", e.Message));
            }
        }
    }
}
