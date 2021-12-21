using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MaxPingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Sets the max ping limit for the server.";
        }
        public override string GetHelp()
        {
            return "Usage: st-mp <ping>";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-MaxPing", "mp", "st-mp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Wrong number of arguments");
                    return;
                }
                if (!int.TryParse(_params[0], out HighPingKicker.Max_Ping))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Maxping is not an integer");
                    return;
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Max ping limit set to '{0}'", HighPingKicker.Max_Ping));
                Config.WriteXml();
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MaxPingConsole.Execute: {0}", e.Message));
            }
        }
    }
}