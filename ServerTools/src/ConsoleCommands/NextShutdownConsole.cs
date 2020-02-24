using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Shows the next scheduled auto shutdown.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ShutdownCheck\n" +
                   "1. Shows the next scheduled auto shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ShutdownCheck", "ShutdownCheck", "scheck", "sc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (AutoShutdown.IsEnabled)
            {
                if (!AutoShutdown.Bloodmoon)
                {
                    if (!Event.Open)
                    {
                        try
                        {
                            int _time;
                            if (AutoShutdown.BloodmoonOver)
                            {
                                _time = AutoShutdown.Delay - (Timers._autoShutdownBloodmoonOver / 60);
                            }
                            else
                            {
                                _time = AutoShutdown.Delay - (Timers._autoShutdown / 60);
                            }
                            if (_time < 0)
                            {
                                _time = 0;
                            }
                            string _message = string.Format("The next auto shutdown is in {0:00} H : {1:00} M", _time / 60, _time % 60);
                            SdtdConsole.Instance.Output(_message);
                            return;
                        }
                        catch (Exception e)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Error in NextShutdownConsole.Execute: {0}.", e));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("The server is set to shutdown after the event is over.");
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("The server is set to shutdown after the bloodmoon is over.");
                    return;
                }
            }
            else
            {
                SdtdConsole.Instance.Output("Auto shutdown is not enabled.");
                return;
            }
        }
    }
}
