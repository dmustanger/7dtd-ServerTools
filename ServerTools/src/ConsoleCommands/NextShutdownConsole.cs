using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdownConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Shows the next scheduled shutdown";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-scheck\n" +
                   "1. Shows the next scheduled shutdown\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-ShutdownCheck", "scheck", "st-scheck" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (Shutdown.IsEnabled)
                {
                    EventSchedule.Schedule.TryGetValue("Shutdown", out DateTime time);
                    TimeSpan varTime = time - DateTime.Now;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int remainingTime = (int)fractionalMinutes;
                    if (remainingTime < 0)
                    {
                        remainingTime = 0;
                    }
                    if (remainingTime <= 10 && Event.Open)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] A event is currently active. The server can not shutdown until it completes"));
                        return;
                    }
                    string timeLeft = string.Format("{0:00} H : {1:00} M", remainingTime / 60, remainingTime % 60);
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] The next auto shutdown is in '{0}'", timeLeft));
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Shutdown is not enabled");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NextShutdownConsole.Execute: {0}", e.Message));
            }
        }
    }
}
