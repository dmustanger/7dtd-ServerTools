using System;
using System.Collections.Generic;

namespace ServerTools
{
    class LogConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Adds the specified entry to the current output log.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-Logs 'Entry for output log'\n" +
                   "1. Adds the entry to the server output log\n";

        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Logs", "log", "st-log" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or more, found '{0}'", _params.Count));
                    return;
                }
                _params.RemoveAt(0);
                string _message = string.Join(" ", _params);
                Log.Out(_message);
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Log entry: _message");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LogConsole.Execute: {0}", e.Message));
            }
        }
    }
}
