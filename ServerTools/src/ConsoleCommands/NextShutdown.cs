using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdown : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Returns the next scheduled auto shutdown.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. shutdown check\n" +
                   "1. Shows the status of the next scheduled auto shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shutdown", "shutdown" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                    return;
                }
                if (_params[1] != "check")
                {
                    SdtdConsole.Instance.Output(string.Format("Param 1 does not equal \"check\": Param 1 \"Check\" {0}.", _params[1]));
                    return;
                }
                if (_params[0].ToLower().Equals("check"))
                {
                    var _timeStart = AutoShutdown.timerStart.RandomObject();
                    TimeSpan varTime = DateTime.Now - _timeStart;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timeMinutes = (int)fractionalMinutes;
                    int _timeleftMinutes = Timers.Shutdown_Delay - _timeMinutes;
                    string TimeLeft;
                    TimeLeft = string.Format("{0:00} H :{1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                    SdtdConsole.Instance.Output(TimeLeft);
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }


            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NextShutdown.Run: {0}.", e));
            }
        }

        
    }
}
