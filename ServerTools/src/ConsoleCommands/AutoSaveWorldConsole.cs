using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AutoSaveWorldConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Auto Save World.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AutoSaveWorld off\n" +
                   "  2. AutoSaveWorld on\n" +
                   "1. Turn off the world auto save\n" +
                   "2. Turn on the world auto save\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AutoSaveWorld", "autosaveworld" };
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
                    AutoSaveWorld.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Auto save world has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    AutoSaveWorld.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Auto save world has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoSaveWorldConsole.Run: {0}.", e));
            }
        }
    }
}