using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ShopConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Shop.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Shop off\n" +
                   "  2. Shop on\n" +
                   "1. Turn off shop\n" +
                   "2. Turn on shop\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Shop", "shop" };
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
                    Shop.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Shop has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Shop.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Shop has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ShopConsole.Run: {0}.", e));
            }
        }
    }
}