using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable shutdown.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. sd off\n" +
                   "  2. sd on\n" +
                   "1. Turn off the shutdown process\n" +
                   "2. Turn on the shutdown process\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Shutdown", "sd", "st-sd" };
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
                    if (Shutdown.IsEnabled)
                    {
                        Shutdown.IsEnabled = false;
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
                    if (!Shutdown.IsEnabled)
                    {
                        Shutdown.IsEnabled = true;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdownConsole.Execute: {0}", e.Message));
            }
        }
    }
}