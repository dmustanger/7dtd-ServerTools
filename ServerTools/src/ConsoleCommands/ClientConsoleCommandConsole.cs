using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ClientConsoleCommandConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Force a client to run a console command";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-ccc <Id/EOS/EntityId/PlayerName> <Command>\n" +
                   "1. Forces the specified player to run this console command\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ClientConsoleCommand", "ccc", "st-ccc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or more, found {0}", _params.Count));
                    return;
                }
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    _params.RemoveAt(0);
                    string command = string.Join(" ", _params);
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("{0}", command), true));
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Ran console command '{0}' on id '{1}' '{2}' named {3}", command, cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate '{0}' online", _params[0]));
                }    
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClientConsoleCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}
