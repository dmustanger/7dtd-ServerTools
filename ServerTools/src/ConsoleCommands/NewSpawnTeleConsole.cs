using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NewSpawnTeleConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable New Spawn Tele.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. NewSpawnTele off\n" +
                   "  2. NewSpawnTele on\n" +
                   "1. Turn off new spawn tele\n" +
                   "2. Turn on new spawn tele\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-NewSpawnTele", "newspawntele" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    NewSpawnTele.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("New player tele has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    NewSpawnTele.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("New player tele has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NewSpawnTeleConsole.Run: {0}.", e));
            }
        }
    }
}