using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ReduceDelayConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add or remove a reduced command delay on a player.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-rd add <id/entityId/playerName>\n" +
                   "  2. st-rd remove <id/entityId/playerName>\n" +
                   "1. Add reduced command delay for specified player\n" +
                   "2. Remove reduced command delay for specified player\n";

        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ReduceDelay", "rd", "st-rd" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                if (cInfo == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid player id or name '{0}'", _params[1]));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null && 
                        !PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ReducedDelay = true;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added reduced command delay for '{0}'", _params[1]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reduced command delay already exists for '{0}'", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString] != null &&
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ReducedDelay)
                    {
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].ReducedDelay = false;
                        PersistentContainer.DataChange = true;
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed reduced command delay for '{0}'", _params[1]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reduced command delay does not exist for '{0}'", _params[1]));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReduceDelayConsole.Execute: {0}", e.Message));
            }
        }
    }
}
