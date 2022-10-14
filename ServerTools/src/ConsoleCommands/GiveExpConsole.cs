using System;
using System.Collections.Generic;

namespace ServerTools
{
    class GiveExpConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Gives experience to a player";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-ge <Id/EntityId/PlayerName> <Amount>\n" +
                "1. Gives the specified amount of experience to the id or player name\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-GiveExperience", "ge", "st-ge" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                    return;
                }
                ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    if (!int.TryParse(_params[1], out int experience))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[1]));
                        return;
                    }
                    NetPackageEntityAddExpClient package = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(cInfo.entityId, experience, Progression.XPTypes.Kill);
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, cInfo.entityId, -1, -1, -1);
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added '{0}' experience to player '{1}' '{2}' named {3}", experience, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate online player: {0}", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GiveExpConsole.Execute: {0}", e.Message));
            }
        }
    }
}
