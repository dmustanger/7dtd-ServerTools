using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AdminListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable chat command Admin List.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AdminList off\n" +
                   "  2. AdminList on\n" +
                   "1. Turn off your admin list\n" +
                   "2. Turn on your admin list\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AdminList", "adminlist", "al" };
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
                    AdminList.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Admin list has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    AdminList.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Admin list has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminListConsole.Run: {0}.", e));
            }
        }
    }
}