using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class AutoShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Auto Shutdown.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AutoShutdown off\n" +
                   "  2. AutoShutdown on\n" +
                   "1. Turn off the world auto shutdown\n" +
                   "2. Turn on the world auto shutdown\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AutoShutdown", "AutoShutdown", "autoshutdown" };
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
                    if (AutoShutdown.IsEnabled)
                    {
                        AutoShutdown.IsEnabled = false;
                        LoadConfig.WriteXml();
                        StopServer.CountingDown = false;
                        SdtdConsole.Instance.Output(string.Format("Auto shutdown has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auto shutdown is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!AutoShutdown.IsEnabled)
                    {
                        AutoShutdown.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Auto shutdown has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Auto shutdown is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdownConsole.Execute: {0}", e));
            }
        }
    }
}