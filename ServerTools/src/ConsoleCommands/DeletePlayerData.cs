using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DeletePlayerDataConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Deletes all saved player data from ServerTools bin file";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. deleteplayerdata\n" +
                   "1. Deletes all saved player data from ServerTools such as home positions and wallets\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-DeletePlayerData", "deleteplayerdata", "st-dpd" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                DeletePlayerData.Exec();
                SdtdConsole.Instance.Output(string.Format("Player data has been deleted"));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DeletePlayerDataConsole.Execute: {0}", e.Message));
            }
        }
    }
}
