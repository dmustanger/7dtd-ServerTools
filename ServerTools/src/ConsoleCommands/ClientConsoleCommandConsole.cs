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
                   "  1. st-ccc <SteamId/EntityId/PlayerName> <Command>\n" +
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
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or more, found {0}", _params.Count));
                    return;
                }
                ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (cInfo != null)
                {
                    _params.RemoveAt(0);
                    string command = string.Join(" ", _params);
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("{0}", command), true));
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Ran console command '{0}' on player with steam id {1} named {2}", command, cInfo.playerId, cInfo.playerName));
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate player {0} online", _params[0]));
                }    
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClientConsoleCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}
