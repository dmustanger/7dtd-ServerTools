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
            return new string[] { "st-StopServer", "stopserver" };
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
                    if (!StopServer.stopServerCountingDown)
                    {
                        SdtdConsole.Instance.Output("Stopserver is not running.");
                    }
                    else
                    {
                        StopServer.stopServerCountingDown = false;
                        StopServer.NoEntry = false;
                        if (AutoShutdown.IsEnabled)
                        {
                            AutoShutdown.ShutdownTime();
                            Timers._sD = 0;
                        }
                        Lottery.ShuttingDown = false;
                        SdtdConsole.Instance.Output("Stopserver has been cancelled.");
                    }
                }
                else
                {
                    if (StopServer.stopServerCountingDown)
                    {
                        SdtdConsole.Instance.Output(string.Format("Server is already stopping in {0} mins", Timers._newCount));
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out Timers.Stop_Server_Time))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in StopServerConsole.Run: {0}.", e));
            }
        }
    }
}