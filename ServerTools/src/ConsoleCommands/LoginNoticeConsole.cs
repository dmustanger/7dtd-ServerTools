﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    class LoginNoticeConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable login notice.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-ln off\n" +
                   "  2. st-ln on\n" +
                   "1. Turn off login notice\n" +
                   "2. Turn on login notice\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-LoginNotice", "ln", "st-ln" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (LoginNotice.IsEnabled)
                    {
                        LoginNotice.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Login notice has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Login notice is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!LoginNotice.IsEnabled)
                    {
                        LoginNotice.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Login notice has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Login notice is already off"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNoticeConsole.Execute: {0}", e.Message));
            }
        }
    }
}