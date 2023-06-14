using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ShutdownConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable shutdown.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-sd off\n" +
                   "  2. st-sd on\n" +
                   "1. Turn off the auto shutdown process\n" +
                   "2. Turn on the auto shutdown process\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Shutdown", "sd", "st-sd" };
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
                    if (Shutdown.IsEnabled)
                    {
                        Shutdown.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        Shutdown.ShuttingDown = false;
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shutdown has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shutdown is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Shutdown.IsEnabled)
                    {
                        Shutdown.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shutdown has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Shutdown is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ShutdownConsole.Execute: {0}", e.Message));
            }
        }
    }
}