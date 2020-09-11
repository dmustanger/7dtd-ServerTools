using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NewPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable new player.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. NewPlayer off\n" +
                   "  2. NewPlayer on\n" +
                   "1. Turn off new player\n" +
                   "2. Turn on new player\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-NewPlayer", "np", "st-np" };
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
                if (_params[0].ToLower().Equals("off"))
                {
                    if (NewPlayer.IsEnabled)
                    {
                        NewPlayer.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New player has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New player is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!NewPlayer.IsEnabled)
                    {
                        NewPlayer.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New player has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] New player is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NewPlayerConsole.Execute: {0}", e.Message));
            }
        }
    }
}