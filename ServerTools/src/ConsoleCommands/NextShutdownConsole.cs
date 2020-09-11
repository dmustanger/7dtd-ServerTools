using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NextShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Shows the next scheduled shutdown.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ShutdownCheck\n" +
                   "1. Shows the next scheduled shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ShutdownCheck", "sc", "st-sc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (Shutdown.IsEnabled)
                {
                    if (!Shutdown.Bloodmoon)
                    {
                        if (!Event.Open)
                        {
                            try
                            {
                                int _time;
                                if (Shutdown.BloodmoonOver)
                                {
                                    _time = Shutdown.Delay - (Timers._shutdownBloodmoonOver / 60);
                                }
                                else
                                {
                                    _time = Shutdown.Delay - (Timers._shutdown / 60);
                                }
                                if (_time < 0)
                                {
                                    _time = 0;
                                }
                                string _message = string.Format("[SERVERTOOLS] The next shutdown is in {0:00} H : {1:00} M", _time / 60, _time % 60);
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
                            SdtdConsole.Instance.Output("[SERVERTOOLS] The server is set to shutdown after the event is over.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] The server is set to shutdown after the bloodmoon is over.");
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Shutdown is not enabled.");
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
