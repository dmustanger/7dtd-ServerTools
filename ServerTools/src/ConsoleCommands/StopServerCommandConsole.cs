using System;
using System.Collections.Generic;

namespace ServerTools
{
    class StopServerCommandConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Starts a countdown with an alert system for the time specified and then stops the server";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-ss <minutes>\n" +
                "  2. st-ss cancel\n" +
                "1. Starts a shutdown process with a countdown for the specified time\n" +
                "2. Cancels the shutdown\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-StopServer", "ss", "st-ss" };
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
                if (_params[0] == "cancel")
                {
                    if (!StopServer.ShuttingDown)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver is not running");
                    }
                    else
                    {
                        StopServer.ShuttingDown = false;
                        StopServer.NoEntry = false;
                        Lottery.ShuttingDown = false;
                        if (Shutdown.IsEnabled)
                        {
                            EventSchedule.Add("Shutdown", DateTime.Now.AddMinutes(Shutdown.Delay));
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled and the next shutdown has been reset");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Stopserver has been cancelled");
                        }
                    }
                }
                else
                {
                    if (StopServer.ShuttingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Server is already set to shutdown. Cancel it if you wish to set a new countdown"));
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out StopServer.Delay))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid time specified: {0}", _params[0]));
                        }
                        else
                        {
                            StopServer.StartShutdown();
                        }
                    }                   
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServerCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}