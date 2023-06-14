using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DeletePlayerDataConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Deletes all saved player data from ServerTools bin file";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-dpd\n" +
                   "1. Deletes all saved player data from ServerTools such as home positions, waypoints and wallets\n" +
                   "2. *Note* Be very careful running this command. It can not be reloaded without a backup\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-DeletePlayerData", "deleteplayerdata", "st-dpd" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                DeletePlayerData.Exec();
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] All save data from ServerTools.bin has been deleted"));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DeletePlayerDataConsole.Execute: {0}", e.Message));
            }
        }
    }
}
