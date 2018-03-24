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
                   "  1. scheck\n" +
                   "1. Shows the status of the next scheduled auto shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Shutdowncheck", "shutdowncheck" ,"scheck" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ShutdownCheck.Run: {0}.", e));
            }
        }
    }
}
