using System;
using System.Collections.Generic;

namespace ServerTools
{
    class VehicleTeleportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable vehicle recall tool.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-recall off\n" +
                   "  2. st-recall on\n" +
                   "1. Turn off the vehicle recall tool\n" +
                   "2. Turn on the vehicle recall tool\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-VehicleRecall", "recall", "st-recall" };
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
                    if (VehicleRecall.IsEnabled)
                    {
                        VehicleRecall.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle recall has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle recall is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VehicleRecall.IsEnabled)
                    {
                        VehicleRecall.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle recall has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle recall is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleRecallConsole.Execute: {0}", e.Message));
            }
        }
    }
}