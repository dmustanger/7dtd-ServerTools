using System;
using System.Collections.Generic;
using System.Xml;

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
            return new string[] { "st-AdminList", "AdminList", "adminlist", "al" };
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
                    if (AdminList.IsEnabled)
                    {
                        AdminList.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Admin list has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Admin list is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!AdminList.IsEnabled)
                    {
                        AdminList.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Admin list has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Admin list is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminListConsole.Execute: {0}", e));
            }
        }
    }
}