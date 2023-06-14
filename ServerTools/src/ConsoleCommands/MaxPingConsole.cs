using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MaxPingConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Sets the max ping limit for the server.";
        }
        protected override string getHelp()
        {
            return "Usage: st-mp <ping>";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-MaxPing", "mp", "st-mp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments");
                    return;
                }
                if (!int.TryParse(_params[0], out HighPingKicker.Max_Ping))
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Maxping is not an integer");
                    return;
                }
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Max ping limit set to '{0}'", HighPingKicker.Max_Ping));
                Config.WriteXml();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MaxPingConsole.Execute: {0}", e.Message));
            }
        }
    }
}