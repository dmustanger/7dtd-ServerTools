using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextRestart : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Returns the next scheduled auto restart.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. restart check\n" +
                   "1. Shows the status of the next scheduled auto restart\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "restart" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("check"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    else
                    {
                        var _timeStart = AutoRestart.timerStart.RandomObject();
                        TimeSpan varTime = DateTime.Now - _timeStart;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timeMinutes = (int)fractionalMinutes;
                        int _timeleftMinutes = AutoRestart.DelayBetweenRestarts - _timeMinutes;
                        string TimeLeft;
                        TimeLeft = string.Format("{0:00} H :{1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                        SdtdConsole.Instance.Output(TimeLeft);
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }


            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NextRestart.Run: {0}.", e));
            }
        }

        
    }
}
