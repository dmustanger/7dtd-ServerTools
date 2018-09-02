using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Returns the next scheduled auto shutdown.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. scheck\n" +
                   "1. Shows the status of the next scheduled auto shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shutdowncheck" ,"scheck" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (AutoShutdown.IsEnabled)
            {
                if (!AutoShutdown.Bloodmoon)
                {
                    try
                    {
                        var _timeStart = AutoShutdown.timerStart[0];
                        TimeSpan varTime = DateTime.Now - _timeStart;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timeMinutes = (int)fractionalMinutes;
                        int _timeleftMinutes = Timers.Shutdown_Delay - _timeMinutes;
                        string TimeLeft;
                        TimeLeft = string.Format("{0:00} H :{1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                        SdtdConsole.Instance.Output(TimeLeft);
                        return;
                    }
                    catch (Exception e)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Error in ShutdownCheckConsole.Run: {0}.", e));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("The server is set to shutdown after the bloodmoon is over.");
                    return;
                }
            }
        }
    }
}
