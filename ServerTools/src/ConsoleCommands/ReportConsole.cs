using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class ReportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Report.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Report off\n" +
                   "  2. Report on\n" +
                   "1. Turn off report\n" +
                   "2. Turn on report\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Report", "Report", "report" };
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
                    if (Report.IsEnabled)
                    {
                        Report.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Report has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Report is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Report.IsEnabled)
                    {
                        Report.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Report has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Report is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ReportConsole.Execute: {0}", e));
            }
        }
    }
}