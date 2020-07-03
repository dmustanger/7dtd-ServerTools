using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class StopServerCommandConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Stops the game server with a warning countdown every minute.";
        }

        public override string GetHelp()
        {
            return "Usage: stopserver <minutes>\n" +
                "Usage: stopserver cancel";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-StopServer", "StopServer", "stopserver" };
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
                if (_params[0] == "cancel")
                {
                    if (!StopServer.CountingDown)
                    {
                        SdtdConsole.Instance.Output("Stopserver is not running.");
                    }
                    else
                    {
                        StopServer.CountingDown = false;
                        StopServer.NoEntry = false;
                        Lottery.ShuttingDown = false;
                        if (AutoShutdown.IsEnabled)
                        {
                            Timers._autoShutdown = 0;
                        }
                        SdtdConsole.Instance.Output("Stopserver has been cancelled.");
                    }
                }
                else
                {
                    if (StopServer.CountingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("Server is already set to stop."));
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out StopServer.Delay))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid time specified: {0}", _params[0]));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServerConsole.Run: {0}", e.Message));
            }
        }
    }
}