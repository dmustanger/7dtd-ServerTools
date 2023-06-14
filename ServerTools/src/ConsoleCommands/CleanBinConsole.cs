using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CleanBinConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools]- Enable or disable clean bin.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-cb off\n" +
                   "  2. st-cb on\n" +
                   "  3. st-cb now\n" +
                   "1. Turn off clean bin\n" +
                   "2. Turn on clean bin\n" +
                   "3. Cleans out the current ServerTools.bin using the settings from ServerToolsConfig.xml\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-CleanBin", "cb", "st-cb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (CleanBin.IsEnabled)
                    {
                        CleanBin.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Clean bin has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Clean bin is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!CleanBin.IsEnabled)
                    {
                        CleanBin.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Clean bin has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Clean bin is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("now"))
                {
                    CleanBin.Exec();
                    SdtdConsole.Instance.Output("[SERVERTOOLS] ServerTools.bin has been cleaned based on the active options");
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CleanBinConsole.Execute: {0}", e.Message));
            }
        }
    }
}
